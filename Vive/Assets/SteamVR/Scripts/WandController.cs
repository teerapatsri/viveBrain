using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour {
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index); } }
	private SteamVR_TrackedObject trackedObj;

    public WandController otherWand;
    public bool trigger;
    public Interactable item;
	//HashSet<Interactable> objectHoveringOver = new HashSet<Interactable>();
	//private Interactable closestItem;
	//private Interactable interactingItem;
	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
        trigger = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(controller==null){
			Debug.Log ("Controller not initialized");
			return;
		}
        if (controller.GetPressDown(triggerButton))
        {
            trigger = true;
            item.BeginTrigger(this);
            if (otherWand.trigger) //both triggered
            {
                item.BeginZoom();
            }
        }
        if (controller.GetPressUp(triggerButton))
        {
            trigger = false;
            item.EndTrigger(this);
            if (otherWand.trigger) //both triggered
            {
                item.EndZoom();
            }
        }
            if (controller.GetPressDown (gripButton)) {
            item.BeginGrab(this);
            /* float minDistance = float.MaxValue;
             float distance;
             closestItem = null;
             foreach (Interactable item in objectHoveringOver) {
                 distance = (item.transform.position - transform.position).sqrMagnitude;
                 if (distance < minDistance) {
                     minDistance = distance;
                     closestItem = item;
                 }
             }

             interactingItem = closestItem;

             if (interactingItem != null) {
                 if (interactingItem.IsGrabbing ()) {
                     interactingItem.EndInteraction (this);
                 }

                 interactingItem.BeginInteraction (this);
             }*/
        }
        if (controller.GetPressUp (gripButton)&& item!=null /*&&interactingItem!=null*/) {
                item.EndGrab(this);
                //interactingItem.EndInteraction (this);
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
