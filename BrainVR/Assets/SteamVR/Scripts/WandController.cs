using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour
{
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
    private Valve.VR.EVRButtonId padButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    [Header(" [Other object references]")]
    [Tooltip("The Other Wand")]
    public WandController otherWand;
    [Tooltip("Interactable Object")]
    public Interactable interactableController;
    [Tooltip("Clipping Plane")]
    public ClippingPlaneController planeController;

    private bool gripEnabled, trigEnabled, menuEnabled;
    private bool trigger;
    private bool grip;
    private bool controllingPlane;
    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();

        trigger = false;
        controllingPlane = false;
        grip = false;

        gripEnabled = true;
        trigEnabled = true;
        menuEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }
        if (controller.GetPressDown(menuButton) && menuEnabled)
        {
            changeMode();
            otherWand.changeMode();
        }
        if (controller.GetPressUp(menuButton))
        {
        }
        if (controller.GetPressDown(triggerButton) && trigEnabled)
        {
            if (IsControllingPlane())
            {//PLANE mode
                trigger = true;
                planeController.RotatePlane();
                if (otherWand.trigger)
                {
                    //PlaneMODE DOUBLE TRIGGER
                }
            }
            else
            {//NORMAL mode
                trigger = true;
                interactableController.BeginTrigger(this);
                if (otherWand.trigger) //Free Mode Double TRIGGER
                {
                    interactableController.BeginZoom();
                }
            }
        }
        if (controller.GetPressUp(triggerButton))
        {
            //planeController.EndRotatePlane(this);
            trigger = false;
            interactableController.EndTrigger(this);
            if (otherWand.trigger) //Free Mode Double TRIGGER
            {
                interactableController.EndZoom();
                //planeController.BeginRotatePlane(otherWand);
            }
        }
        if (controller.GetPressDown(gripButton) && gripEnabled)
        {
            if (controllingPlane)//Plane mode
            {
                grip = false;
                planeController.BeginMovePlane(this);
            }
            else //NORMAL mode
            {
                grip = true;
                otherWand.grip = false;
                interactableController.BeginGrab(this);
            }
        }
        if (controller.GetPressUp(gripButton))
        {
            planeController.EndMovePlane(this);
            interactableController.EndGrab(this);
            grip = false;
        }
        if (controller.GetPressDown(padButton))
        {
            GameObject cubeObj = GameObject.Find("Cube");
            var renderStyle = cubeObj.GetComponent<CubeRenderStyleController>();

            var v = controller.GetAxis(padButton);
            Debug.Log(v.x + " " + v.y);
            if (v.y < 0)
            {
                renderStyle.SetShaderNumber((renderStyle.ShaderNumber + 1) % 8);
            }
            else
            {
                renderStyle.SetTwoSideClipping(!renderStyle.IsTwoSideClipping);
            }

        }
    }
    public void GripEnable()
    {
        gripEnabled = true;
    }
    public void GripDisable()
    {
        gripEnabled = false;
    }
    public void TrigEnable()
    {
        trigEnabled = true;
    }
    public void TrigDisable()
    {
        trigEnabled = false;
    }
    public void MenuEnable()
    {
        menuEnabled = true;
    }
    public void MenuDisable()
    {
        menuEnabled = false;
    }
    public bool IsGripping()
    {
        return grip;
    }
    public bool IsTriggering()
    {
        return trigger;
    }
    public void changeMode()
    {
        controllingPlane = !controllingPlane;
    }
    public bool IsControllingPlane()
    {
        return controllingPlane;
    }
}
