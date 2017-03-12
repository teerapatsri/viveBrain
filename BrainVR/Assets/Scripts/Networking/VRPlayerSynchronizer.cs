using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRPlayerSynchronizer : NetworkBehaviour
{
    [SyncVar(hook = "OnLaserActiveChange")]
    private bool laserActive;
    [SyncVar(hook = "OnRightWandActiveChange")]
    private bool rightWandActive;
    [SyncVar(hook = "OnLeftWandActiveChange")]
    private bool leftWandActive;

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
            SetHandDisplay(leftWand, leftHandDisplay, ref leftWandActive);
            SetHandDisplay(rightWand, rightHandDisplay, ref rightWandActive);

            if (rightWand.activeSelf && laserPointer.active)
            {
                if (!laserActive) laserActive = true;
            }
            else
            {
                if (laserActive) laserActive = false;
            }
        }
    }

    private void SetHandDisplay(GameObject wand, GameObject display, ref bool displayActive)
    {
        if (wand.activeSelf)
        {
            if (!displayActive) displayActive = true;
            display.transform.position = wand.transform.position;
            display.transform.rotation = wand.transform.rotation;
        }
        else
        {
            if (displayActive) displayActive = false;
        }
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
            rightWand.SetActive(isActive);
        }
    }

    private void OnLeftWandActiveChange(bool isActive)
    {
        if (syncFromServer)
        {
            leftWand.SetActive(isActive);
        }
    }
}
