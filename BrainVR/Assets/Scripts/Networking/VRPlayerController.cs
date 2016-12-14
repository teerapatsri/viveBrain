using UnityEngine;
using System.Collections;

public class VRPlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject vrCameraRig;

    private bool isFPSCamera = true;

    void Update()
    {
        /* if (Input.GetKeyDown(KeyCode.V))
        {
            mainCamera.GetComponent<AudioListener>().enabled = false;
            vrCameraRig.GetComponent<SteamVR_ControllerManager>().enabled = true;

            var cameras = vrCameraRig.GetComponentsInChildren<Camera>();
            foreach (var camera in cameras) {
                camera.enabled = true;
            }
        } */
    }
}
