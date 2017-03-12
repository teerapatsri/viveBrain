using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRPlayerSynchronizer : NetworkBehaviour
{
    [SyncVar(hook = "OnLaserActiveChange")]
    public bool laserActive;
    [SyncVar(hook = "OnRightWandActiveChange")]
    public bool rightWandActive;
    [SyncVar(hook = "OnLeftWandActiveChange")]
    public bool leftWandActive;

    // [SyncVar]

    private VREnvironmentController vrEnvController;
    private GameObject leftWand;
    private GameObject rightWand;
    private SteamVR_LaserPointer laserPointer;

    public GameObject leftHandDisplay;
    public GameObject rightHandDisplay;
    public GameObject laserBeamDisplay;

    /// <summary>
    /// This determines whether current display is inside "local" player or not. For local player, the value is false.
    /// </summary>
    public bool syncFromServer = true;

    public void Start()
    {
        vrEnvController = GameObject.FindGameObjectWithTag("VREnvironment").GetComponent<VREnvironmentController>();
        leftWand = vrEnvController.leftWand;
        rightWand = vrEnvController.rightWand;
        laserPointer = rightWand.GetComponent<SteamVR_LaserPointer>();

        OnLaserActiveChange(laserActive);
        OnLeftWandActiveChange(leftWandActive);
        OnRightWandActiveChange(rightWandActive);
    }

    public void LateUpdate()
    {
        if (!syncFromServer)
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
                if (leftWandActive) CmdSetRightWandDisplayActive(false);
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
        if (syncFromServer)
        {
            laserBeamDisplay.SetActive(isActive);
        }
    }

    private void OnRightWandActiveChange(bool isActive)
    {
        if (syncFromServer)
        {
            rightHandDisplay.SetActive(isActive);
        }
    }

    private void OnLeftWandActiveChange(bool isActive)
    {
        if (syncFromServer)
        {
            leftHandDisplay.SetActive(isActive);
        }
    }
}
