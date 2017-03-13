﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ObserverPlayerController : NetworkBehaviour
{
    public GameObject playerCameraObj;

    private GameObject observedPlayer;

    private void OnEnable()
    {
        // Find player to observe
        observedPlayer = FindObservablePlayer();
        // Set that player as being observed
        observedPlayer.GetComponent<PlayerController>().IsBeingObserved = true;

        playerCameraObj.SetActive(true);
    }

    private void OnDisable()
    {
        playerCameraObj.SetActive(false);

        // Set that player as not being observed
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
            playerCameraObj.transform.position = new Vector3(t.position.x, t.position.y, t.position.z);
            playerCameraObj.transform.rotation = new Quaternion(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w);
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
