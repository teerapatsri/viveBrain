using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public struct RayMarchingOptions
{
    // [SerializeField]
    [Header("Render in a lower resolution to increase performance.")]
    public int downscale;

    public RayMarchingOptions(int downscale = 2)
    {
        this.downscale = downscale;
    }
}

public class RayMarchingMasterController : MonoBehaviour
{
    public GameObject cubeTarget;
    public GameObject clipPlane;

    [SerializeField]
    private LayerMask volumeLayer;

    [SerializeField]
    private Shader compositeShader;
    [SerializeField]
    private Shader renderFrontDepthShader;
    [SerializeField]
    private Shader renderBackDepthShader;
    [SerializeField]
    private Shader rayMarchShader;

    [SerializeField]
    [Header("Remove all the darker colors")]
    private bool increaseVisiblity = false;


    [Header("Drag all the textures in here")]
    [SerializeField]
    private Texture2D[] slices;
    [SerializeField]
    [Range(0, 2)]
    private float opacity = 1;
    [Header("Volume texture size. These must be a power of 2")]
    [SerializeField]
    private int volumeWidth = 256;
    [SerializeField]
    private int volumeHeight = 256;
    [SerializeField]
    private int volumeDepth = 256;
    [Header("Clipping planes percentage")]
    [SerializeField]
    private Vector4 clipDimensions = new Vector4(100, 100, 100, 0);

    private Material _rayMarchMaterial;
    private Material _compositeMaterial;
    private Camera _ppCamera;
    private Texture3D _volumeBuffer;

    private void Awake()
    {
        _rayMarchMaterial = new Material(rayMarchShader);
        _compositeMaterial = new Material(compositeShader);
    }

    private void Start()
    {
        if (enableExternalImages)
        {
            LoadMRIImagesFromFolder();
        }
        GenerateVolumeTexture();
    }

    private void OnDestroy()
    {
        if (_volumeBuffer != null)
        {
            Destroy(_volumeBuffer);
        }
    }

    void OnDrawGizmos()
    {
        if (cubeTarget != null)
        {
            const float s = 0.5f;
            var pnt1 = cubeTarget.transform.TransformPoint(new Vector3(s, s, s));
            var pnt2 = cubeTarget.transform.TransformPoint(new Vector3(s, s, -s));
            var pnt3 = cubeTarget.transform.TransformPoint(new Vector3(s, -s, -s));
            var pnt4 = cubeTarget.transform.TransformPoint(new Vector3(s, -s, s));
            var pnt5 = cubeTarget.transform.TransformPoint(new Vector3(-s, s, s));
            var pnt6 = cubeTarget.transform.TransformPoint(new Vector3(-s, s, -s));
            var pnt7 = cubeTarget.transform.TransformPoint(new Vector3(-s, -s, -s));
            var pnt8 = cubeTarget.transform.TransformPoint(new Vector3(-s, -s, s));

            Gizmos.color = new Color(1, 0.5f, 0, 0.5F);

            Gizmos.DrawLine(pnt1, pnt2);
            Gizmos.DrawLine(pnt2, pnt3);
            Gizmos.DrawLine(pnt3, pnt4);
            Gizmos.DrawLine(pnt4, pnt1);

            Gizmos.DrawLine(pnt5, pnt6);
            Gizmos.DrawLine(pnt6, pnt7);
            Gizmos.DrawLine(pnt7, pnt8);
            Gizmos.DrawLine(pnt8, pnt5);

            Gizmos.DrawLine(pnt1, pnt5);
            Gizmos.DrawLine(pnt2, pnt6);
            Gizmos.DrawLine(pnt3, pnt7);
            Gizmos.DrawLine(pnt4, pnt8);
        }
    }

    public void RenderImage(RenderTexture source, RenderTexture destination, RayMarchingOptions options, Camera camera)
    {
        var renderStyle = cubeTarget.GetComponent<CubeRenderStyleController>();

        _rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffer);

        var width = source.width / options.downscale;
        var height = source.height / options.downscale;

        if (_ppCamera == null)
        {
            var go = new GameObject("PPCamera");
            _ppCamera = go.AddComponent<Camera>();
            _ppCamera.enabled = false;
        }

        _ppCamera.CopyFrom(camera);
        _ppCamera.clearFlags = CameraClearFlags.SolidColor;
        _ppCamera.backgroundColor = Color.white;
        _ppCamera.cullingMask = volumeLayer;

        var frontDepth = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);
        var backDepth = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);

        var volumeTarget = RenderTexture.GetTemporary(width, height, 0);

        // need to set this vector because unity bakes object that are non uniformily scaled
        //TODO:FIX
        //Shader.SetGlobalVector("_VolumeScale", cubeTarget.transform.localScale);

        // Render depths
        _ppCamera.targetTexture = frontDepth;
        _ppCamera.RenderWithShader(renderFrontDepthShader, "RenderType");
        _ppCamera.targetTexture = backDepth;
        _ppCamera.RenderWithShader(renderBackDepthShader, "RenderType");

        // Render volume
        _rayMarchMaterial.SetTexture("_FrontTex", frontDepth);
        _rayMarchMaterial.SetTexture("_BackTex", backDepth);

        if (cubeTarget != null && clipPlane != null && clipPlane.gameObject.activeSelf)
        {
            _rayMarchMaterial.SetVector("_ClipPlane", GetPlaneVector());
        }
        else
        {
            _rayMarchMaterial.SetVector("_ClipPlane", Vector4.zero);
        }
        _rayMarchMaterial.SetInt("_ClippingOption", renderStyle.IsTwoSideClipping ? 1 : 0); // Clipping Pane Option
        _rayMarchMaterial.SetInt("_ShaderNumber", renderStyle.ShaderNumber); // Shader Mode
        _rayMarchMaterial.SetFloat("_Opacity", opacity); // Blending strength 
        _rayMarchMaterial.SetVector("_ClipDims", clipDimensions / 100f); // Clip box


        Graphics.Blit(null, volumeTarget, _rayMarchMaterial);

        //Composite
        _compositeMaterial.SetTexture("_BlendTex", volumeTarget);
        Graphics.Blit(source, destination, _compositeMaterial);

        RenderTexture.ReleaseTemporary(volumeTarget);
        RenderTexture.ReleaseTemporary(frontDepth);
        RenderTexture.ReleaseTemporary(backDepth);
    }

    private Vector4 GetPlaneVector()
    {
        var transform = cubeTarget.transform.worldToLocalMatrix * clipPlane.transform.localToWorldMatrix;
        var p0 = transform * Vector3.zero;
        var p1 = transform * Vector3.forward;
        var p2 = transform * Vector3.right;

        var p = new Plane(p0, p1, p2);
        return new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
    }

    private void GenerateVolumeTexture()
    {
        // use a bunch of memory!
        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;

        // skip some slices if we can't fit it all in
        var countOffset = (slices.Length - 1) / (float)d;

        var volumeColors = new Color[w * h * d];

        var sliceCount = 0;
        var sliceCountFloat = 0f;
        for (int z = 0; z < d; z++)
        {
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var idx = x + (y * w) + (z * (w * h));
                    volumeColors[idx] = slices[sliceCount].GetPixelBilinear(x / (float)w, y / (float)h);
                    if (increaseVisiblity)
                    {
                        volumeColors[idx].a *= volumeColors[idx].r;
                    }
                }
            }
        }

        _volumeBuffer.SetPixels(volumeColors);
        _volumeBuffer.Apply();

        _rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffer);
    }

    public bool enableExternalImages = false;
    public string imageFolderPath = "";

    private void LoadMRIImagesFromFolder()
    {
        // Change this to change pictures folder
        var basePath = Path.Combine(Application.dataPath, "MRI Images");
        var path = Path.GetFullPath(Path.Combine(basePath, imageFolderPath));
        Debug.Log("MRI Image Base Path: " + path);

        var pngFilePaths = Directory.GetFiles(path, "*.png")
            .Select(file => LoadTextureFromURL(file, true));

        var jpgFilePaths = Directory.GetFiles(path, "*.jpg")
            .Select(file => LoadTextureFromURL(file, false));

        slices = pngFilePaths.Concat(jpgFilePaths).OrderBy(texture => texture.name).ToArray();

        Debug.Log("Slice count: " + slices.Length);
    }

    private Texture2D LoadTextureFromURL(string filePath, bool isPNG)
    {
        // LoadImageIntoTexture compresses JPGs by DXT1 and PNGs by DXT5
        var texture = new Texture2D(1024, 1024, isPNG ? TextureFormat.DXT5 : TextureFormat.DXT1, false);

        WWW www = new WWW(@"file://" + filePath);
        www.LoadImageIntoTexture(texture);

        texture.name = Path.GetFileName(filePath);

        return texture;
    }
}
