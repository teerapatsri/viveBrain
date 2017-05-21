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

    public RayMarchingOptions(int downscale = 1)
    {
        this.downscale = downscale;
    }
}

public struct SliceImageSet
{
    private Texture3D texture;
    private double xScale;
    private double yScale;
    private double zScale;
    private int imgXSize;
    private int imgYSize;
    private int imgZSize;

    public Texture3D Texture { get { return texture; } }
    public double XScale { get { return xScale; } }
    public double YScale { get { return yScale; } }
    public double ZScale { get { return zScale; } }
    public int ImgXSize { get { return imgXSize; } }
    public int ImgYSize { get { return imgYSize; } }
    public int ImgZSize { get { return imgZSize; } }

    public SliceImageSet(Texture3D texture, double xScale, double yScale, double zScale, int imgXSize, int imgYSize, int imgZSize)
    {
        this.texture = texture;
        this.xScale = xScale;
        this.yScale = yScale;
        this.zScale = zScale;
        this.imgXSize = imgXSize;
        this.imgYSize = imgYSize;
        this.imgZSize = imgZSize;
    }
}

public class RayMarchingMasterController : MonoBehaviour
{
    [Header("Importer")]
    public MRIImporter importer;
    public string imageSetFolderPath = "";

    [Header("Configurations")]
    public GameObject cubeTarget;
    public GameObject clipPlane;

    [SerializeField]
    private LayerMask volumeLayer;
    public LayerMask uiLayer;

    [SerializeField]
    private Shader compositeShader;
    [SerializeField]
    private Shader renderFrontDepthShader;
    [SerializeField]
    private Shader renderBackDepthShader;
    [SerializeField]
    private Shader rayMarchShader;

    [SerializeField]
    [Range(0, 2)]
    private float opacity = 1;
    [Header("Clipping planes percentage")]
    [SerializeField]
    private Vector4 clipDimensions = new Vector4(100, 100, 100, 0);

    private Material _rayMarchMaterial;
    private Material _compositeMaterial;
    private Camera _ppCamera;
    private Camera _uiCamera;
    private List<SliceImageSet> sliceImageSets = new List<SliceImageSet>();

    private CubeRenderStyleController renderStyle;

    private void Awake()
    {
        _rayMarchMaterial = new Material(rayMarchShader);
        _compositeMaterial = new Material(compositeShader);
        renderStyle = cubeTarget.GetComponent<CubeRenderStyleController>();
    }

    private void Start()
    {
        StartCoroutine(LoadMRIImage());
    }

    private IEnumerator LoadMRIImage()
    {
        yield return StartCoroutine(importer.Import(imageSetFolderPath));
        sliceImageSets = importer.ImportedSliceImageSets;
        UpdateCubeScale();
        //_rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffers[selectedVolumeBufferIndex]);
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        foreach (var volumeBuffer in sliceImageSets)
        {
            if (volumeBuffer.Texture != null)
            {
                Destroy(volumeBuffer.Texture);
            }
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

    public void UpdateCubeScale()
    {
        var vrEnvironment = GameObject.FindWithTag("VREnvironment").GetComponent<VREnvironmentController>();
        var cube = vrEnvironment.cubeObj;
        var currentScale = cube.transform.localScale;

        var currentImageSet = GetCurrentImageSet();
        if (currentImageSet.HasValue)
        {
            var imgSet = currentImageSet.Value;

            var oldXyzAverage = currentScale.x + currentScale.y + currentScale.z;
            // var xyzSum = imgSet.XScale + imgSet.YScale + imgSet.ZScale;
            var xyzSum = 3;

            var coeff = oldXyzAverage / xyzSum;

            /*cube.transform.localScale = new Vector3(
                (float)(imgSet.XScale * coeff),
                (float)(imgSet.YScale * coeff),
                (float)(imgSet.ZScale * coeff)
            );*/
            cube.transform.localScale = new Vector3(coeff, coeff, coeff);
        }
    }

    public void RenderImage(RenderTexture source, RenderTexture destination, RayMarchingOptions options, Camera camera)
    {
        var curImgSet = GetCurrentImageSet();
        _rayMarchMaterial.SetTexture("_VolumeTex", curImgSet.HasValue ? curImgSet.Value.Texture : null);

        var sourceWidth = source.width;
        var sourceHeight = source.height;

        var width = sourceWidth / options.downscale;
        var height = sourceHeight / options.downscale;

        // Initialize cube volume camera

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

        // Initialize UI camera

        if (_uiCamera == null)
        {
            var go = new GameObject("UICamera");
            _uiCamera = go.AddComponent<Camera>();
            _uiCamera.enabled = false;
        }

        _uiCamera.CopyFrom(camera);
        _uiCamera.clearFlags = CameraClearFlags.Nothing;
        _uiCamera.cullingMask = uiLayer;

        // Render cube depth textures

        var frontDepth = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);
        var backDepth = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);

        var volumeTarget = RenderTexture.GetTemporary(width, height, 0);

        // need to set this vector because unity bakes object that are non uniformily scaled
        //TODO:FIX
        //Shader.SetGlobalVector("_VolumeScale", cubeTarget.transform.localScale);

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

        _uiCamera.targetTexture = destination;
        _uiCamera.Render();

        RenderTexture.ReleaseTemporary(volumeTarget);
        RenderTexture.ReleaseTemporary(frontDepth);
        RenderTexture.ReleaseTemporary(backDepth);
    }

    public SliceImageSet? GetCurrentImageSet()
    {
        SliceImageSet? sliceImageSet = null;
        if (sliceImageSets.Count > 0)
        {
            sliceImageSet = sliceImageSets[renderStyle.SelectedVolumeBufferIndex % sliceImageSets.Count];
        }

        return sliceImageSet;
    }

    private Vector4 GetPlaneVector()
    {
        var p0 = CalculatePlaneVector(Vector3.zero);
        var p1 = CalculatePlaneVector(Vector3.forward);
        var p2 = CalculatePlaneVector(Vector3.right);

        var p = new Plane(p0, p1, p2);
        return new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
    }

    private Vector3 CalculatePlaneVector(Vector3 v)
    {
        return cubeTarget.transform.InverseTransformPoint(clipPlane.transform.TransformPoint(v));
    }
}
