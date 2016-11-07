using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour {
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
    private Valve.VR.EVRButtonId padButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index); } }
	private SteamVR_TrackedObject trackedObj;

    public WandController otherWand;
    public bool trigger;
    public bool controllingPlane;
    public Interactable interactableController;
    public ClippingPlaneController planeController;
	//HashSet<Interactable> objectHoveringOver = new HashSet<Interactable>();
	//private Interactable closestItem;
	//private Interactable interactingItem;
	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
        trigger = false;
        controllingPlane = false;

    }
	
	// Update is called once per frame
	void Update () {
		if(controller==null){
			Debug.Log ("Controller not initialized");
			return;
		}
        if (controller.GetPressDown(menuButton))
        {
                controllingPlane = !controllingPlane;
                otherWand.controllingPlane = controllingPlane;
        }
        if (controller.GetPressUp(menuButton))
        {
        }
        if (controller.GetPressDown(triggerButton))
        {
            if (controllingPlane) {//Clipping mode
                trigger = true;
                planeController.BeginRotatePlane(this);
                if (otherWand.trigger)
                {
                    planeController.ResetPosition();
                }
            }
            else { 
                 trigger = true;
                 interactableController.BeginTrigger(this);
                 if (otherWand.trigger) //both triggered
                 {
                     interactableController.BeginZoom();
                 }
            }
        }
        if (controller.GetPressUp(triggerButton))
        {
            planeController.EndRotatePlane(this);
            trigger = false;
            interactableController.EndTrigger(this);
            if (otherWand.trigger) //both triggered
            {
                interactableController.EndZoom();
                planeController.BeginRotatePlane(otherWand);
            }
        }
        if (controller.GetPressDown (gripButton)) {
            if (controllingPlane)//Clipping mode
            {
                planeController.BeginMovePlane(this);
            }
            else
            {
                interactableController.BeginGrab(this);
            }
        }
        if (controller.GetPressUp (gripButton)&& interactableController!=null /*&&interactingItem!=null*/) {
            planeController.EndMovePlane(this);
            interactableController.EndGrab(this);
	    }
	}

	private void OnTriggerEnter(Collider other)
    {
        Interactable collidedItem = other.GetComponent<Collider>().GetComponent<Interactable>();
		if (collidedItem != null) {
            //objectHoveringOver.Add (collidedItem);
        }
	}
	private void OnTriggerExit(Collider other){
        Interactable collidedItem = other.GetComponent<Collider>().GetComponent<Interactable> ();
		if (collidedItem != null) {
			//objectHoveringOver.Remove (collidedItem);
        } 
	}
}
