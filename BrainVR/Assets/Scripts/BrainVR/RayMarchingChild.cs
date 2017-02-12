using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(Camera))]
public class RayMarchingChild : MonoBehaviour
{
    public RayMarchingOptions renderOptions;

    private Camera cameraObj;
    private RayMarchingMasterController controller;

    private void Start()
    {
        GameObject gameControllerObj = GameObject.FindGameObjectWithTag("GameController");
        controller = gameControllerObj.GetComponent<RayMarchingMasterController>();
        cameraObj = GetComponent<Camera>();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        controller.RenderImage(source, destination, renderOptions, cameraObj);
    }
}
