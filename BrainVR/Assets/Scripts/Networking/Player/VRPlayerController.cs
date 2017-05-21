using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRPlayerController : NetworkBehaviour
{
    public GameObject leftHandDisplay;
    public GameObject rightHandDisplay;
    public GameObject laserBeamDisplay;
    public GameObject vrDisplayContainer;
    public GameObject playerDisplay;
    public GameObject ruler;

    private VREnvironmentController vrEnvController;
    private Camera vrCamera;
    private CubeScaleTransformSynchronizer cubeScaleTransformSynchronizer;

    [SyncVar(hook = "OnLaserActiveChange")]
    private bool laserActive;
    [SyncVar(hook = "OnRightWandActiveChange")]
    private bool rightWandActive;
    [SyncVar(hook = "OnLeftWandActiveChange")]
    private bool leftWandActive;

    private GameObject leftWand;
    private GameObject rightWand;
    private WandController rightWandController;
    private SteamVR_LaserPointer laserPointer;
    private GameObject clipPlane;
    private GameObject cubeTarget;
    private RulerController rulerController;


    private void Awake()
    {
        vrEnvController = GameObject.FindGameObjectWithTag("VREnvironment").GetComponent<VREnvironmentController>();
        vrCamera = vrEnvController.eyeCamera;
        leftWand = vrEnvController.leftWand;
        rightWand = vrEnvController.rightWand;
        rightWandController = rightWand.GetComponent<WandController>();
        laserPointer = rightWand.GetComponent<SteamVR_LaserPointer>();
        cubeScaleTransformSynchronizer = GameObject.Find("Cube").GetComponent<CubeScaleTransformSynchronizer>();
        //for ruler
        clipPlane = GameObject.Find("Clipping Plane");
        cubeTarget = GameObject.Find("Cube");
        rulerController = ruler.GetComponent<RulerController>();
    }

    private bool isHost = false;

    public void SetAsHost()
    {
        isHost = true;
    }

    private void OnEnable()
    {
        if (isLocalPlayer)
        {
            vrEnvController.EnableVR();
            if (isHost)
            {
                cubeScaleTransformSynchronizer.syncScaleFromServer = false;
            }
            else
            {
                var lCtrl = leftWand.GetComponent<WandController>();
                lCtrl.GripDisable();
                lCtrl.TrigDisable();
                lCtrl.MenuDisable();
                lCtrl.PadDisable();
                var rCtrl = rightWand.GetComponent<WandController>();
                rCtrl.GripDisable();
                rCtrl.MenuDisable();
                rCtrl.PadDisable();
            }
        }

        vrDisplayContainer.SetActive(true);
        OnLaserActiveChange(laserActive);
        OnLeftWandActiveChange(leftWandActive);
        OnRightWandActiveChange(rightWandActive);
    }

    private void OnDisable()
    {
        OnLaserActiveChange(false);
        OnLeftWandActiveChange(false);
        OnRightWandActiveChange(false);
        vrDisplayContainer.SetActive(false);

        if (isLocalPlayer)
        {
            if (isHost)
            {
                cubeScaleTransformSynchronizer.syncScaleFromServer = true;
            }
            else
            {
                var lCtrl = leftWand.GetComponent<WandController>();
                lCtrl.GripEnable();
                lCtrl.TrigEnable();
                lCtrl.MenuEnable();
                lCtrl.PadEnable();
                var rCtrl = rightWand.GetComponent<WandController>();
                rCtrl.GripEnable();
                rCtrl.MenuEnable();
                rCtrl.PadEnable();

            }
            vrEnvController.DisableVR();
        }
    }
    private bool firstPointReceived = false;
    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            UpdateVRDisplay();
            UpdatePlayerDisplay();
            if(rightWand.activeSelf)
            {
                Plane cutPlane = new Plane(clipPlane.transform.TransformPoint(Vector3.zero), clipPlane.transform.TransformPoint(Vector3.right), clipPlane.transform.TransformPoint(Vector3.forward));
                Ray ray = new Ray(rightWand.transform.position, rightWand.transform.forward);
                float rayDistance;
                if (rightWandController.IsMeasuring())
                {
                    if (cutPlane.Raycast(ray, out rayDistance))
                    {
                        Vector3 localPoint = cubeTarget.transform.InverseTransformPoint(ray.GetPoint(rayDistance));
                        Vector3 drawnPoint = ray.GetPoint(rayDistance);
                        if (isOutOfBound(localPoint))
                        {
                            Debug.DrawLine(Vector3.zero, ray.GetPoint(rayDistance), Color.red, 0.5f);
                            rulerController.UpdateCurrentPoint(ray.GetPoint(rayDistance));
                        }
                        else
                        {
                            Debug.DrawLine(Vector3.zero, ray.GetPoint(rayDistance), Color.green, 0.5f);

                            if (!firstPointReceived)
                            {
                                rulerController.PinPoint(ray.GetPoint(rayDistance));
                                firstPointReceived = true;
                            }
                            else
                            {
                                rulerController.UpdateCurrentPoint(ray.GetPoint(rayDistance));
                            }
                        }
                    }
                } else
                {
                    if (cutPlane.Raycast(ray, out rayDistance))
                    {
                        Vector3 localPoint = cubeTarget.transform.InverseTransformPoint(ray.GetPoint(rayDistance));
                        Vector3 drawnPoint = ray.GetPoint(rayDistance);
                        if (firstPointReceived)
                        {
                            rulerController.PinPoint(ray.GetPoint(rayDistance));
                            firstPointReceived = false;
                        }
                        else
                        {
                            // don't draw
                        }
                    }
                }
            }
        }
    }

    private void UpdateVRDisplay()
    {
        // Left wand
        if (leftWand.activeSelf)
        {
            if (!leftWandActive) CmdSetLeftWandDisplayActive(true);
            leftHandDisplay.transform.position = leftWand.transform.position;
            leftHandDisplay.transform.rotation = leftWand.transform.rotation;
        }
        else
        {
            if (leftWandActive) CmdSetLeftWandDisplayActive(false);
        }

        // Right wand
        if (rightWand.activeSelf)
        {
            if (!rightWandActive) CmdSetRightWandDisplayActive(true);
            rightHandDisplay.transform.position = rightWand.transform.position;
            rightHandDisplay.transform.rotation = rightWand.transform.rotation;
        }
        else
        {
            if (rightWandActive) CmdSetRightWandDisplayActive(false);
        }

        // Laser pointer
        if (rightWand.activeSelf && laserPointer.active)
        {
            if (!laserActive) CmdSetLaserDisplayActive(true);
        }
        else
        {
            if (laserActive) CmdSetLaserDisplayActive(false);
        }
    }

    private void UpdatePlayerDisplay()
    {
        Vector3 playerPosition = vrCamera.transform.position;
        playerDisplay.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

        Quaternion playerRotation = vrCamera.transform.rotation;
        playerDisplay.transform.rotation = new Quaternion(playerRotation.x, playerRotation.y, playerRotation.z, playerRotation.w);
    }

    [Command]
    private void CmdSetLeftWandDisplayActive(bool newValue)
    {
        leftWandActive = newValue;
    }

    [Command]
    private void CmdSetRightWandDisplayActive(bool newValue)
    {
        rightWandActive = newValue;
    }

    [Command]
    private void CmdSetLaserDisplayActive(bool newValue)
    {
        laserActive = newValue;
    }

    private void OnLaserActiveChange(bool isActive)
    {
        if (!isLocalPlayer) laserBeamDisplay.SetActive(isActive);
    }

    private void OnRightWandActiveChange(bool isActive)
    {
        if (!isLocalPlayer) rightHandDisplay.SetActive(isActive);
    }

    private void OnLeftWandActiveChange(bool isActive)
    {
        if (!isLocalPlayer) leftHandDisplay.SetActive(isActive);
    }

    private bool isOutOfBound(Vector3 v)
    {
        return v.x < -0.5f || v.x > 0.5f || v.z < -0.5f || v.z > 0.5f || v.y < -0.5f || v.y > 0.5f;
    }
}
