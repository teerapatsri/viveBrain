using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour {
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId downButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index); } }
	private SteamVR_TrackedObject trackedObj;

    public WandController otherWand;
    public bool trigger;
    public bool selectObj;
    public Interactable item;
    public ClippingPlaneController plane;
	//HashSet<Interactable> objectHoveringOver = new HashSet<Interactable>();
	//private Interactable closestItem;
	//private Interactable interactingItem;
	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
        trigger = false;
        selectObj = false;

    }
	
	// Update is called once per frame
	void Update () {
		if(controller==null){
			Debug.Log ("Controller not initialized");
			return;
		}
        if (controller.GetPressDown(downButton))
        {
                selectObj = !selectObj;
                otherWand.selectObj = selectObj;
        }
        if (controller.GetPressUp(downButton))
        {
        }
        if (controller.GetPressDown(triggerButton))
        {
            if (selectObj) {
                plane.BeginRotatePlane(this);
            }
            else { 
                 trigger = true;
                 item.BeginTrigger(this);
                 if (otherWand.trigger) //both triggered
                 {
                     item.BeginZoom();
                 }
            }
        }
        if (controller.GetPressUp(triggerButton))
        {
            plane.EndRotatePlane(this);
            trigger = false;
            item.EndTrigger(this);
            if (otherWand.trigger) //both triggered
            {
                item.EndZoom();
            }
        }
        if (controller.GetPressDown (gripButton)) {
            if (selectObj)
            {
                plane.BeginMovePlane(this);
            }
            else
            {
                item.BeginGrab(this);
            }
        }
        if (controller.GetPressUp (gripButton)&& item!=null /*&&interactingItem!=null*/) {
            plane.EndMovePlane(this);
            item.EndGrab(this);
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
