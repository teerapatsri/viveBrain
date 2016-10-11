using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour {
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input ((int)trackedObj.index); } }
	private SteamVR_TrackedObject trackedObj;

	HashSet<Interactable> objectHoveringOver = new HashSet<Interactable>();
	private Interactable closestItem;
	private Interactable interactingItem;
	// Use this for initialization
	void Start () {
		trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if(controller==null){
			Debug.Log ("Controller not initialized");
			return;
		}

		if (controller.GetPressDown (gripButton)) {
			float minDistance = float.MaxValue;
			float distance;
			foreach (Interactable item in objectHoveringOver) {
				distance = (item.transform.position - transform.position).sqrMagnitude;
				if (distance < minDistance) {
					minDistance = distance;
					closestItem = item;
				}
			}

			interactingItem = closestItem;

//			if (interactingItem != null) {
//				if (interactingItem.IsInteracting ()) {
//					interactingItem.EndInteraction (this);
//				}
//
//				interactingItem.BeginInteraction (this);
//			}
		}
		if (controller.GetPressUp (gripButton)&& interactingItem!=null) {
			interactingItem.EndInteraction (this);
		}
	}

	private void OnTriggerEnter(Collider other){
		Interactable collidedItem = GetComponent<Collider>().GetComponent<Interactable>();
		if (collidedItem != null) {
			objectHoveringOver.Add (collidedItem);
		}
	}
	private void OnTriggerExit(Collider other){
		Interactable collidedItem = GetComponent<Collider>().GetComponent<Interactable> ();
		if (collidedItem != null) {
			objectHoveringOver.Remove (collidedItem);
		} 
	}
}
