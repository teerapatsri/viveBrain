using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {
	public new Rigidbody rigidbody;

	private bool currentlyGrabbing;
    private bool currentlyTrigger;
    private WandController attachedWand;
	private Transform interactionPoint;
	private Vector3 posDeltaG, posDeltaT, axis;
	private Quaternion rotationDelta;
	private float angle;
	private float rotationFactor = 400f;
	private float velocityFactor = 20000f;
	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		interactionPoint = new GameObject ().transform;
		velocityFactor /= rigidbody.mass;
		rotationFactor /= rigidbody.mass;
	}
	
	// Update is called once per frame
	void Update () {
		if (attachedWand != null && currentlyGrabbing) {
			posDeltaG = attachedWand.transform.position - interactionPoint.position;
			this.rigidbody.velocity = posDeltaG * velocityFactor * Time.fixedDeltaTime;

			//rotationDelta = attachedWand.transform.rotation * Quaternion.Inverse (interactionPoint.rotation);
			//rotationDelta.ToAngleAxis (out angle, out axis);

			if (angle > 180) {
				angle -= 360;
			}

            //this.rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }
        if (attachedWand != null && currentlyTrigger)
        {
            //posDeltaT = attachedWand.transform.position - interactionPoint.position;
            //this.rigidbody.velocity = posDeltaT * velocityFactor * Time.fixedDeltaTime;

            rotationDelta = attachedWand.transform.rotation * Quaternion.Inverse (interactionPoint.rotation);
            rotationDelta.ToAngleAxis (out angle, out axis);
            if (angle > 180)
            {
                angle -= 360;
            }

            this.rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }
    }

	public void BeginGrab(WandController wand){
		attachedWand = wand;
		interactionPoint.position = wand.transform.position;
		interactionPoint.rotation = wand.transform.rotation;
		interactionPoint.SetParent (transform, true);

		currentlyGrabbing = true;
	}
	public void EndGrab(WandController wand){
		if (wand == attachedWand) {
			attachedWand = null;
			currentlyGrabbing = false;
		}
	}
    public void BeginTrigger(WandController wand)
    {
        attachedWand = wand;
        interactionPoint.position = wand.transform.position;
        interactionPoint.rotation = wand.transform.rotation;
        interactionPoint.SetParent(transform, true);

        currentlyTrigger = true;
    }
    public void EndTrigger(WandController wand)
    {
        if (wand == attachedWand)
        {
            attachedWand = null;
            currentlyTrigger = false;
        }
    }

    public bool IsGrabbing(){
		return currentlyGrabbing;
	}
    public bool IsTriggering(){
        return currentlyTrigger;
    }
}
