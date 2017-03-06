using UnityEngine;
using UnityEngine.VR;
using System.Linq;

public class VREnvironmentController : MonoBehaviour
{
    public GameObject vrContainerObj;
    public GameObject cubeObj;

    public void Start()
    {
        // TODO: uncomment these
        string[] supportedDevices = VRSettings.supportedDevices.Where(deviceName => deviceName != "None").ToArray();
        Debug.Log("Using VR Device: " + string.Join(", ", supportedDevices));
        VRSettings.LoadDeviceByName(supportedDevices);
    }

    public void EnableVR()
    {
        var name = VRSettings.loadedDeviceName;
        if (name != "" && name != "None")
        {
            if (Camera.current != null) Camera.current.enabled = false;

            VRSettings.enabled = true;
            vrContainerObj.SetActive(true);
            cubeObj.GetComponent<Interactable>().enabled = true;
            SetUpVRCamera();
        }
    }

    private void SetUpVRCamera()
    {
        var camera = GameObject.Find("Camera (eye)").GetComponent<Camera>();
        camera.enabled = true;
    }

    public void DisableVR()
    {
        vrContainerObj.SetActive(false);
        cubeObj.GetComponent<Interactable>().enabled = false;

        VRSettings.enabled = false;
    }
}
