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
        cameraObj.depthTextureMode = DepthTextureMode.Depth;
        
        // Turn off UI culling mask (set via master controller)
        // cameraObj.cullingMask &= ~(controller.uiLayer);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        controller.RenderImage(source, destination, renderOptions, cameraObj);
    }
}
