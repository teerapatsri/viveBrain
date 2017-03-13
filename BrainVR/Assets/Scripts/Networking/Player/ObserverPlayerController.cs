using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ObserverPlayerController : NetworkBehaviour
{
    public Camera playerCamera;

    private GameObject observedPlayer;

    private void OnEnable()
    {
        observedPlayer = FindObservablePlayer();
        var playerController = observedPlayer.GetComponent<PlayerController>();
        playerController.IsBeingObserved = true;
    }

    private void OnDisable()
    {
        if (observedPlayer != null)
        {
            var playerController = observedPlayer.GetComponent<PlayerController>();
            playerController.IsBeingObserved = false;
        }
        observedPlayer = null;
    }

    private void Update()
    {
        if (observedPlayer != null)
        {
            GameObject targetDisplay = observedPlayer.GetComponent<PlayerController>().playerDisplay;
            Transform t = targetDisplay.transform;
            playerCamera.transform.position = new Vector3(t.position.x, t.position.y, t.position.z);
            playerCamera.transform.rotation = new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
        }
    }

    private GameObject FindObservablePlayer()
    {
        return GameObject.FindGameObjectsWithTag("Player")
            .Where(gameObj => IsPlayerAHost(gameObj))
            .First();
    }

    private bool IsPlayerAHost(GameObject player)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller == null) return false;
        return controller.IsHost;
    }
}
