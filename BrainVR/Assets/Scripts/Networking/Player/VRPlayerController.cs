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
    private SteamVR_LaserPointer laserPointer;

    private void Awake()
    {
        vrEnvController = GameObject.FindGameObjectWithTag("VREnvironment").GetComponent<VREnvironmentController>();
        vrCamera = vrEnvController.eyeCamera;
        leftWand = vrEnvController.leftWand;
        rightWand = vrEnvController.rightWand;
        laserPointer = rightWand.GetComponent<SteamVR_LaserPointer>();
        cubeScaleTransformSynchronizer = GameObject.Find("Cube").GetComponent<CubeScaleTransformSynchronizer>();
    }

    private void OnEnable()
    {
        if (isLocalPlayer)
        {
            vrEnvController.EnableVR();
            cubeScaleTransformSynchronizer.syncScaleFromServer = false;
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
            cubeScaleTransformSynchronizer.syncScaleFromServer = true;
            vrEnvController.DisableVR();
        }
    }

    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            UpdateVRDisplay();
            UpdatePlayerDisplay();
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
}
