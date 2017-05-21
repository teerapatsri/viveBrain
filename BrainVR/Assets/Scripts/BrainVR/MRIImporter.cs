using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MRIImporter : MonoBehaviour
{
    public bool useBilinear = false;

    private List<SliceImageSet> _result;
    public List<SliceImageSet> ImportedSliceImageSets { get { return _result; } }

    [Header("Drag all the textures in here")]
    [SerializeField]
    private List<Texture2D[]> slicesSets = new List<Texture2D[]>();
    private List<double> xScaleList = new List<double>();
    private List<double> yScaleList = new List<double>();
    private List<double> zScaleList = new List<double>();
    private List<int> xSizeList = new List<int>();
    private List<int> ySizeList = new List<int>();
    private List<int> zSizeList = new List<int>();
    private Texture2D[] currentLoadingSlices;

    public IEnumerator Import(string imageFolderPath)
    {
        _result = null;

        Debug.Log("Importing MRI: " + imageFolderPath);

        yield return StartCoroutine(LoadMRIImageSetFromFolder(imageFolderPath));
        Debug.Log("Slice set count: " + slicesSets.Count);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(GenerateVolumeTexture());

        Debug.Log("Loading MRI Images Complete");
        yield return null;
    }

    private IEnumerator LoadMRIImageSetFromFolder(string imageSetFolderPath)
    {
        // Change this to change pictures folder
        var basePath = Path.Combine(Application.dataPath, "MRI Images");
        var path = Path.GetFullPath(Path.Combine(basePath, imageSetFolderPath));

        // Find Subdirectories
        string[] subdirectoryEntries = Directory.GetDirectories(path);
        foreach (string subdirectory in subdirectoryEntries)
        {
            Debug.Log(subdirectory);
            var pngFilePaths = Directory.GetFiles(subdirectory, "*.png");
            var jpgFilePaths = Directory.GetFiles(subdirectory, "*.jpg");

            Array.Sort(pngFilePaths);
            Array.Sort(jpgFilePaths);

            int sliceCount = pngFilePaths.Length + jpgFilePaths.Length;
            currentLoadingSlices = new Texture2D[sliceCount];

            var currentIndex = 0;
            foreach (var pngFilePath in pngFilePaths)
            {
                yield return StartCoroutine(LoadTextureFromURL(pngFilePath, true, currentIndex));
                currentIndex++;
                SetProgressBar("Loading texture data... ", currentIndex, sliceCount);
            }
            foreach (var jpgFilePath in jpgFilePaths)
            {
                yield return StartCoroutine(LoadTextureFromURL(jpgFilePath, false, currentIndex));
                currentIndex++;
                SetProgressBar("Loading texture data... ", currentIndex, sliceCount);
            }

            xSizeList.Add(currentLoadingSlices[0].width);
            ySizeList.Add(currentLoadingSlices[0].height);
            zSizeList.Add(currentLoadingSlices.Length);
            slicesSets.Add(currentLoadingSlices);

            var configFileName = Path.Combine(subdirectory, "config.txt");
            double x = 1, y = 1, z = 1;
            if (File.Exists(configFileName))
            {
                var lines = File.ReadAllLines(configFileName);
                x = double.Parse(lines[0]);
                y = double.Parse(lines[1]);
                z = double.Parse(lines[2]);
            }

            xScaleList.Add(x);
            yScaleList.Add(y);
            zScaleList.Add(z);
        }

        yield return null;
    }

    private IEnumerator LoadTextureFromURL(string filePath, bool isPNG, int index)
    {
        // LoadImageIntoTexture compresses JPGs by DXT1 and PNGs by DXT5
        // var texture = new Texture2D(8, 8, isPNG ? TextureFormat.DXT5 : TextureFormat.DXT1, false);

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D loadedTexture = new Texture2D(2, 2);
        loadedTexture.LoadImage(fileData);

        var newWidth = Mathf.NextPowerOfTwo(loadedTexture.width);
        var newHeight = Mathf.NextPowerOfTwo(loadedTexture.height);

        var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false)
        {
            name = Path.GetFileName(filePath)
        };

        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                Color pixelColor;
                if (useBilinear)
                {
                    pixelColor = loadedTexture.GetPixelBilinear((float)x / newWidth, (float)y / newHeight);
                }
                else
                {
                    pixelColor = loadedTexture.GetPixel(
                        x * (loadedTexture.width - 1) / (newWidth - 1),
                        y * (loadedTexture.height - 1) / (newHeight - 1)
                    );
                }
                resizedTexture.SetPixel(x, y, pixelColor);
            }
        }

        currentLoadingSlices[index] = resizedTexture;

        yield return null;
    }

    private IEnumerator GenerateVolumeTexture()
    {
        var textureList = new List<SliceImageSet>();
        for (var i = 0; i < slicesSets.Count; i++)
        {
            var slices = slicesSets[i];
            // It should be the power of two. For w and h, they are already power of two values.
            int w = slices[0].width;
            int h = slices[0].height;
            int d = Mathf.NextPowerOfTwo(slices.Length);

            long volumeColorsLength = (long)w * h * d;

            Debug.Log("Using 3D texture of size " + w + " x " + h + " x " + d + " = " + volumeColorsLength.ToString("N0") + " pixels");

            var volumeColors = new Color[volumeColorsLength];

            for (int z = 0; z < d; z++)
            {
                var sliceIndex = z * (slices.Length - 1) / (d - 1);
                // var sliceIndex = z % slices.Length;

                // var srcColors32 = slices[sliceIndex].GetPixels32();
                // Array.Copy(srcColors32, 0, volumeColors, (long)z * w * h, srcColors32.LongLength);

                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        long idx = x + (y * w) + ((long)z * (w * h));
                        volumeColors[idx].a = slices[sliceIndex].GetPixel(x, y).r;
                    }
                }

                SetProgressBar("Building volumetric data... ", z + 1, d);
                yield return new WaitForSeconds(0.1f);
            }

            var tex = new Texture3D(w, h, d, TextureFormat.Alpha8, false);
            tex.SetPixels(volumeColors);
            tex.Apply();

            textureList.Add(new SliceImageSet(tex, xScaleList[i], yScaleList[i], zScaleList[i], xSizeList[i], ySizeList[i], zSizeList[i]));
        }

        _result = textureList;
    }

    private void SetProgressBar(string message, int from, int total)
    {
        Debug.Log(message + " " + from + "/" + total + " (" + (from * 100 / total) + "%)");
    }
}
