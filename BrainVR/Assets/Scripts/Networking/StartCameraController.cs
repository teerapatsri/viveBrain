using UnityEngine;
using System.Collections;

public class StartCameraController : MonoBehaviour
{
    public Camera startCamera;

    void Start()
    {
        startCamera.enabled = true;
    }
}
