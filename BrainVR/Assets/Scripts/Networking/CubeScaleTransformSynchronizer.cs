using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CubeScaleTransformSynchronizer : NetworkBehaviour
{
    [SyncVar(hook = "OnCurrentScalingChange")]
    Vector3 currentScaling;

    [HideInInspector]
    public bool syncScaleFromServer = true;

    public void Start()
    {
        currentScaling = transform.localScale;
    }

    public void Update()
    {
        if (!syncScaleFromServer)
        {
            if (currentScaling != transform.localScale)
            {
                CmdSetCurrentScaling(transform.localScale);
            }
        }
    }

    void CmdSetCurrentScaling(Vector3 newScaling)
    {
        currentScaling = newScaling;
    }

    private void OnCurrentScalingChange(Vector3 newScaling)
    {
        if (syncScaleFromServer)
        {
            this.transform.localScale = newScaling;
        }
    }
}
