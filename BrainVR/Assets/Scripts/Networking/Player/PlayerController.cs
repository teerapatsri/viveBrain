using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using UnityEngine.VR;

public class PlayerController : NetworkBehaviour
{
    public VRPlayerController vrPlayerController;
    public FirstPersonPlayerController firstPersonPlayerController;
    public ObserverPlayerController observerPlayerController;

    public GameObject playerDisplay;
    public GameObject playerColorObj;
    public Camera playerCamera;

    private GameObject startCameraObj;

    public enum PlayerMode { FirstPerson, VR, Observer, Unknown };

    [SyncVar(hook = "OnPlayerModeChange")]
    private PlayerMode currentPlayerMode = PlayerMode.Unknown;
    [SyncVar]
    private bool _isHost = false;
    private bool _isBeingObserved = false;

    public bool IsHost
    {
        get
        {
            return _isHost;
        }
    }

    /// <summary>
    /// Whether this player is being observed, i.e. player on current machine is observing this player.
    /// </summary>
    public bool IsBeingObserved
    {
        get
        {
            return _isBeingObserved;
        }
        set
        {
            _isBeingObserved = value;
            UpdatePlayerDisplayAppearance(currentPlayerMode);
        }
    }

    private void Awake()
    {
        startCameraObj = GameObject.FindWithTag("StartCamera");
    }

    private void Start()
    {
        _isHost = CheckIsHost();
        if (_isHost) Debug.Log("Current player is a host.");

        // Set up player appearance
        UpdatePlayerDisplayAppearance(currentPlayerMode);
    }

    public override void OnStartLocalPlayer()
    {
        // Try to open in VR mode
        if (CanEnableVRMode()) CmdSetPlayerMode(PlayerMode.VR);
        else CmdSetPlayerMode(PlayerMode.FirstPerson);
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
        return !_isHost;
    }

    private void OnDestroy()
    {
        OnPlayerModeChange(PlayerMode.Unknown);
    }

    private void OnPlayerModeChange(PlayerMode newPlayerMode)
    {
        if (currentPlayerMode == newPlayerMode) return;

        switch (currentPlayerMode)
        {
            case PlayerMode.FirstPerson:
                firstPersonPlayerController.enabled = false;
                break;
            case PlayerMode.VR:
                vrPlayerController.enabled = false;
                break;
            case PlayerMode.Observer:
                observerPlayerController.enabled = false;
                break;
            case PlayerMode.Unknown:
                // startCameraObj.GetComponent<AudioListener>().enabled = false;
                startCameraObj.SetActive(false);
                break;
        }

        currentPlayerMode = newPlayerMode;

        switch (newPlayerMode)
        {
            case PlayerMode.FirstPerson:
                firstPersonPlayerController.enabled = true;
                break;
            case PlayerMode.VR:
                vrPlayerController.enabled = true;
                break;
            case PlayerMode.Observer:
                observerPlayerController.enabled = true;
                break;
            case PlayerMode.Unknown:
                // startCameraObj.GetComponent<AudioListener>().enabled = true;
                if (startCameraObj)
                {
                    startCameraObj.SetActive(true);
                }
                break;
        }

        UpdatePlayerDisplayAppearance(newPlayerMode);
    }

    private void UpdatePlayerDisplayAppearance(PlayerMode targetPlayerMode)
    {
        if (targetPlayerMode == PlayerMode.Unknown) return;

        // Set color to player display
        Color playerColor = targetPlayerMode == PlayerMode.VR ? Color.green : Color.blue;

        // If the player's camera is using, don't show the player mesh. Just display the shadow.
        var shadowCastingMode = (isLocalPlayer || IsBeingObserved) ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;

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
