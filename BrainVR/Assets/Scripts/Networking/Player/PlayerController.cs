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

    public GameObject ruler;
    private RulerController rulerController;

    private GameObject startCameraObj;

    public enum PlayerMode { FirstPerson, VR, Observer, Unknown };

    /// <summary>
    /// Mode that the player actually is.
    /// </summary>
    [SyncVar(hook = "OnPlayerModeChange")]
    private PlayerMode currentPlayerMode = PlayerMode.Unknown;
    /// <summary>
    /// Mode currently displayed to the player.
    /// </summary>
    private PlayerMode currentLocalPlayerMode = PlayerMode.Unknown;
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
        rulerController = ruler.GetComponent<RulerController>();
    }

    private void Start()
    {
        OnPlayerModeChange(currentPlayerMode); // Not local player mode

        // Set up player appearance
        UpdatePlayerDisplayAppearance(currentPlayerMode);
    }

    public override void OnStartLocalPlayer()
    {
        _isHost = CheckIsHost();
        if (_isHost) Debug.Log("Current player is a host.");

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

        // TODO: Change to WandController later
        if (Input.GetMouseButtonDown (0)) 
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector3 clickPosition = new Vector3(Input.mousePosition.x / screenWidth, Input.mousePosition.y / screenHeight, 0);
            rulerController.PinPoint(clickPosition);
        }

        rulerController.UpdateCurrentPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
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
        if (currentLocalPlayerMode == newPlayerMode) return;

        switch (currentLocalPlayerMode)
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
                if (isLocalPlayer)
                {
                    // startCameraObj.GetComponent<AudioListener>().enabled = false;
                    startCameraObj.SetActive(false);
                }
                break;
        }

        currentLocalPlayerMode = newPlayerMode;
        currentPlayerMode = newPlayerMode;

        switch (newPlayerMode)
        {
            case PlayerMode.FirstPerson:
                firstPersonPlayerController.enabled = true;
                break;
            case PlayerMode.VR:
                if (IsHost)
                {
                    vrPlayerController.SetAsHost();
                }
                vrPlayerController.enabled = true;
                break;
            case PlayerMode.Observer:
                observerPlayerController.enabled = true;
                break;
            case PlayerMode.Unknown:
                if (isLocalPlayer && startCameraObj)
                {
                    // startCameraObj.GetComponent<AudioListener>().enabled = true;
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
