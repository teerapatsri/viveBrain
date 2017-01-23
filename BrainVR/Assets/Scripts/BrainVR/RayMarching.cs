using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Camera))]
public class RayMarching : MonoBehaviour
{
    // [SerializeField]
    // [Header("Clipping Option")]
    // public bool twoSideClipping = false;

    // [SerializeField]
    // [Header("Shader")]
    // public int shaderNumber = 0;

    [SerializeField]
	[Header("Render in a lower resolution to increase performance.")]
	private int downscale = 2;
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

	[SerializeField][Header("Remove all the darker colors")]
	private bool increaseVisiblity = false;


	[Header("Drag all the textures in here")]
	[SerializeField]
	private Texture2D[] slices;
	[SerializeField][Range(0, 2)]
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

	private string[] files;

	private void Awake()
	{
		_rayMarchMaterial = new Material(rayMarchShader);
		_compositeMaterial = new Material(compositeShader);
    clipPlane = GameObject.Find("Clipping Plane");
    cubeTarget = GameObject.Find("Cube");
  }

	private void Start()
	{
		if(enableExternalImages) {
			LoadMRIImagesFromFolder();
		}
		GenerateVolumeTexture();
	}

	private void OnDestroy()
	{
		if(_volumeBuffer != null)
		{
			Destroy(_volumeBuffer);
		}
	}

	private GameObject clipPlane;
	private GameObject cubeTarget;
	
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
    var renderStyle = cubeTarget.GetComponent<CubeRenderStyleController>();

		_rayMarchMaterial.SetTexture("_VolumeTex", _volumeBuffer);

		var width = source.width / downscale;
		var height = source.height / downscale;

		if(_ppCamera == null)
		{
			var go = new GameObject("PPCamera");
			_ppCamera = go.AddComponent<Camera>();
			_ppCamera.enabled = false;
		}

		_ppCamera.CopyFrom(GetComponent<Camera>());
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

		if(cubeTarget != null && clipPlane != null && clipPlane.gameObject.activeSelf)
		{
			var p = new Plane(
				cubeTarget.transform.InverseTransformDirection(clipPlane.transform.up), 
				cubeTarget.transform.InverseTransformPoint(clipPlane.transform.position));
			_rayMarchMaterial.SetVector("_ClipPlane", new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance));
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

	private void GenerateVolumeTexture()
	{
		// sort
		System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));
		
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
		for(int z = 0; z < d; z++)
		{
			sliceCountFloat += countOffset;
			sliceCount = Mathf.FloorToInt(sliceCountFloat);
			for(int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					var idx = x + (y * w) + (z * (w * h));
					volumeColors[idx] = slices[sliceCount].GetPixelBilinear(x / (float)w, y / (float)h); 
					if(increaseVisiblity)
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
		Debug.Log("Yayyy!!");
    string pathPrefix = @"file://";
		// Change this to change pictures folder
		// string imageFolderPath = @"/Users/pirsquareff/Documents/Workspace/resource-viveBrain/MRI Images";
		files = Directory.GetFiles(imageFolderPath, "*.png");

		slices = new Texture2D[files.Length];
		
		int sliceCount = 0;
		foreach(string filePath in files) {
			string filePathWithPrefix = pathPrefix + filePath;
			Debug.Log(filePath);
			WWW www = new WWW(filePathWithPrefix);
			// yield return www;
			// LoadImageIntoTexture compresses JPGs by DXT1 and PNGs by DXT5
			Texture2D texTemp = new Texture2D(1024, 1024, TextureFormat.DXT5, false);  
			www.LoadImageIntoTexture(texTemp);
			slices[sliceCount] = texTemp;
			slices[sliceCount].name = Path.GetFileNameWithoutExtension(filePath);
			Debug.Log(slices[sliceCount].name);
			sliceCount++;
		}
	}
}
