using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using System;
using System.Linq;
using UnityEngine.VR;

public class PlayerController : NetworkBehaviour
{
    const float playerDisplayYOffset = 0.0f;

    public GameObject playerDisplay;
    public GameObject playerColorObj;
    public GameObject firstPersonObj;
    public Camera playerCamera;

    private GameObject startCameraObj;
    private GameObject observedPlayer;
    private VREnvironmentController vrEnvController;
    private Camera vrCamera;
    private CubeScaleTransformSynchronizer cubeScaleTransformSynchronizer;

    enum PlayerMode { FirstPerson, VR, Observer, Unknown };

    [SyncVar(hook = "OnPlayerModeChange")]
    PlayerMode currentPlayerMode = PlayerMode.Unknown;

    [SyncVar]
    bool isHost = false;

    bool isBeingObserved = false;

    public override void OnStartClient()
    {
        // Set up player appearance
        UpdatePlayerDisplayAppearance();
    }

    public override void OnStartLocalPlayer()
    {
        isHost = CheckIsHost();
        if (isHost) Debug.Log("Current player is a host.");

        startCameraObj = GameObject.Find("Start Camera");

        // Set up current camera
        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioListener>().enabled = true;
        SetMainCameraAudioListenerEnableSafe(false);
        startCameraObj.SetActive(false);

        // Try to open in VR mode
        if (CanEnableVRMode()) CmdSetPlayerMode(PlayerMode.VR);
        else CmdSetPlayerMode(PlayerMode.FirstPerson); 

        // CmdSetPlayerMode(PlayerMode.FirstPerson);
    }

    private static bool CanEnableVRMode()
    {
        string name = VRSettings.loadedDeviceName;
        return name != "" && name != "None";
    }

    private bool CheckIsHost()
    {
        return GameObject.FindGameObjectsWithTag("Player").Length == 1;
    }

    [Command]
    private void CmdSetPlayerMode(PlayerMode newPlayerMode)
    {
        if (newPlayerMode == currentPlayerMode) return;
        currentPlayerMode = newPlayerMode;
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.V) && CanEnableVRMode()) CmdSetPlayerMode(PlayerMode.VR);
            else if (Input.GetKeyDown(KeyCode.F)) CmdSetPlayerMode(PlayerMode.FirstPerson);
            else if (Input.GetKeyDown(KeyCode.O) && CanBeObserver()) CmdSetPlayerMode(PlayerMode.Observer);
        }
    }

    private bool CanBeObserver()
    {
        return !isHost;
    }

    void LateUpdate()
    {
        if (isLocalPlayer)
        {
            if (currentPlayerMode == PlayerMode.VR || currentPlayerMode == PlayerMode.FirstPerson) UpdatePlayerDisplayTransform();
            else if (currentPlayerMode == PlayerMode.Observer) UpdateObserveCamera();
        }
    }

    private void UpdatePlayerDisplayTransform()
    {
        Vector3 playerPosition = currentPlayerMode == PlayerMode.VR ? vrCamera.transform.position : firstPersonObj.transform.position;
        playerDisplay.transform.position = new Vector3(playerPosition.x, playerPosition.y + playerDisplayYOffset, playerPosition.z);

        Quaternion playerRotation = currentPlayerMode == PlayerMode.VR ? vrCamera.transform.rotation : playerCamera.transform.rotation;
        playerDisplay.transform.rotation = new Quaternion(playerRotation.x, playerRotation.y, playerRotation.z, playerRotation.w);
    }

    private void UpdateObserveCamera()
    {
        if (observedPlayer != null)
        {
            GameObject targetDisplay = observedPlayer.GetComponent<PlayerController>().playerDisplay;
            Transform t = targetDisplay.transform;
            playerCamera.transform.position = new Vector3(t.position.x, t.position.y, t.position.z);
            playerCamera.transform.rotation = new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
        }
    }

    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            switch (currentPlayerMode)
            {
                case PlayerMode.FirstPerson: DisableLocalFirstPersonMode(); break;
                case PlayerMode.VR: DisableLocalVRMode(); break;
                case PlayerMode.Observer: DisableLocalObserverMode(); break;
            }

            startCameraObj.SetActive(true);
            SetMainCameraAudioListenerEnableSafe(true);
        }
    }

    private void SetMainCameraAudioListenerEnableSafe(bool enable)
    {
        var mcal = startCameraObj.GetComponent<AudioListener>();
        if (mcal != null)
        {
            mcal.enabled = enable;
        }
    }

    private void OnPlayerModeChange(PlayerMode newPlayerMode)
    {
        if (currentPlayerMode == newPlayerMode) return;

        if (isLocalPlayer)
        {
            switch (currentPlayerMode)
            {
                case PlayerMode.FirstPerson: DisableLocalFirstPersonMode(); break;
                case PlayerMode.VR: DisableLocalVRMode(); break;
                case PlayerMode.Observer: DisableLocalObserverMode(); break;
            }
        }

        currentPlayerMode = newPlayerMode;

        if (isLocalPlayer)
        {
            switch (newPlayerMode)
            {
                case PlayerMode.FirstPerson: EnableLocalFirstPersonMode(); break;
                case PlayerMode.VR: EnableLocalVRMode(); break;
                case PlayerMode.Observer: EnableLocalObserverMode(); break;
            }
        }

        UpdatePlayerDisplayAppearance(newPlayerMode);
    }

    private void EnableLocalVRMode()
    {
        playerCamera.enabled = false;
        playerCamera.GetComponent<AudioListener>().enabled = false;

        vrEnvController = GameObject.FindGameObjectWithTag("VREnvironment").GetComponent<VREnvironmentController>();
        vrEnvController.EnableVR();

        vrCamera = vrEnvController.eyeCamera;
        cubeScaleTransformSynchronizer = GameObject.Find("Cube").GetComponent<CubeScaleTransformSynchronizer>();

        cubeScaleTransformSynchronizer.syncScaleFromServer = false;
    }

    private void DisableLocalVRMode()
    {
        cubeScaleTransformSynchronizer.syncScaleFromServer = true;

        if (vrEnvController != null)
        {
            vrEnvController.DisableVR();
            vrEnvController = null;
        }

        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioListener>().enabled = true;
    }

    private void EnableLocalFirstPersonMode()
    {
        GetComponentInChildren<FirstPersonController>().enabled = true;
    }

    private void DisableLocalFirstPersonMode()
    {
        GetComponentInChildren<FirstPersonController>().enabled = false;
    }

    private void EnableLocalObserverMode()
    {
        observedPlayer = GameObject.FindGameObjectsWithTag("Player")
            .Where(gameObj => IsPlayerAHost(gameObj))
            .First();

        var playerController = observedPlayer.GetComponent<PlayerController>();
        playerController.isBeingObserved = true;
        playerController.UpdatePlayerDisplayAppearance();
    }

    private bool IsPlayerAHost(GameObject player)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller == null) return false;
        return controller.isHost;
    }

    private void DisableLocalObserverMode()
    {
        if (observedPlayer != null)
        {
            var playerController = observedPlayer.GetComponent<PlayerController>();
            playerController.isBeingObserved = false;
            playerController.UpdatePlayerDisplayAppearance();
        }
    }

    private void UpdatePlayerDisplayAppearance()
    {
        UpdatePlayerDisplayAppearance(currentPlayerMode);
    }

    private void UpdatePlayerDisplayAppearance(PlayerMode targetPlayerMode)
    {
        if (targetPlayerMode == PlayerMode.Unknown) return;

        // Set color to player display
        Color playerColor = targetPlayerMode == PlayerMode.VR ? Color.green : Color.blue;

        // If the player's camera is using, don't show the player mesh. Just display the shadow.
        var shadowCastingMode = (isLocalPlayer || isBeingObserved) ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;

        // If the player is observing other, completely hide the player display.
        var isVisible = targetPlayerMode != PlayerMode.Observer;

        foreach (MeshRenderer renderer in playerDisplay.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = isVisible;
            if (isVisible) renderer.shadowCastingMode = shadowCastingMode;
        }

        playerColorObj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", playerColor);
    }
}
