using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class FirstPersonPlayerController : NetworkBehaviour
{
    public GameObject firstPersonObj;
    public GameObject playerDisplay;
    public GameObject playerCameraObj;
    public FirstPersonController firstPersonController;

    private void OnEnable()
    {
        if (isLocalPlayer)
        {
            playerCameraObj.SetActive(true);
            firstPersonController.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            firstPersonController.enabled = false;
            playerCameraObj.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            Vector3 playerPosition = playerCameraObj.transform.position;
            playerDisplay.transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);

            Quaternion playerRotation = playerCameraObj.transform.rotation;
            playerDisplay.transform.rotation = new Quaternion(playerRotation.x, playerRotation.y, playerRotation.z, playerRotation.w);
        }
    }
}
