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

public class RayMarchingMasterController : MonoBehaviour
{
    [Header("Importer")]
    public MRIImporter importer;
    public string imageFolderPath = "";

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
    private Texture3D _volumeBuffer;
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
        yield return StartCoroutine(importer.Import(imageFolderPath));
        _volumeBuffer = importer.ImportedTexture;
        _rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffer);
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
        _rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffer);

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
