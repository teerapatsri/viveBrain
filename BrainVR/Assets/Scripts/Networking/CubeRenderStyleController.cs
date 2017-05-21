using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CubeRenderStyleController : NetworkBehaviour
{
    [SyncVar]
    private int _shaderNumber = 0;

    [SyncVar]
    private bool _isTwoSideClipping = false;

    [SyncVar]
    private int _selectedVolumeBufferIndex = 0;

    public int ShaderNumber { get { return _shaderNumber; } }
    public bool IsTwoSideClipping { get { return _isTwoSideClipping; } }
    public int SelectedVolumeBufferIndex { get { return _selectedVolumeBufferIndex; } }

    public void SetShaderNumber(int value)
    {
        CmdSetShaderNumber(value);
    }

    public void SetTwoSideClipping(bool value)
    {
        CmdSetTwoSideClipping(value);
    }

    public void SetSelectedVolumeBufferIndex(int index)
    {
        CmdSetSelectedVolumeBufferIndex(index);
    }

    [Command]
    private void CmdSetShaderNumber(int newValue)
    {
        _shaderNumber = newValue;
    }

    [Command]
    private void CmdSetTwoSideClipping(bool newValue)
    {
        _isTwoSideClipping = newValue;
    }

    [Command]
    private void CmdSetSelectedVolumeBufferIndex(int newValue)
    {
        _selectedVolumeBufferIndex = newValue;
    }
}
