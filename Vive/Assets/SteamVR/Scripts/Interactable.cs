using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {
	public new Rigidbody rigidbody;
    public WandController wand1;
    public WandController wand2;

    private bool currentlyGrabbing;
    private bool currentlyTrigger;
    private WandController attachedWand;
	private Transform interactionPoint;
	private Vector3 posDeltaG, posDeltaT, axis;
	private Quaternion rotationDelta;
    private float startDistance;
    private float distance;
	private float angle;
    private float change;
	private float rotationFactor = 400f;
	private float velocityFactor = 20000f;
    private Vector3 defaultScale;
    // Use this for initialization
    void Start () {
		rigidbody = GetComponent<Rigidbody> ();
        defaultScale = transform.localScale;
        interactionPoint = new GameObject ().transform;
		velocityFactor /= rigidbody.mass;
		rotationFactor /= rigidbody.mass;
	}
	
	// Update is called once per frame
	void Update () {
        //case grab
		if (attachedWand != null && currentlyGrabbing) {
			posDeltaG = attachedWand.transform.position - interactionPoint.position;
			this.rigidbody.velocity = posDeltaG * velocityFactor * Time.fixedDeltaTime;

			//rotationDelta = attachedWand.transform.rotation * Quaternion.Inverse (interactionPoint.rotation);
			//rotationDelta.ToAngleAxis (out angle, out axis);

            //this.rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }
        //case 1 trigger
        if (attachedWand != null && currentlyTrigger && ((wand1.trigger && wand2.trigger) == false)) //if both triggered go to case 3
        {
            rotationDelta = Quaternion.Euler(new Vector3(transform.position.z - attachedWand.transform.position.z, transform.position.x - attachedWand.transform.position.x, transform.position.y - attachedWand.transform.position.y));
            rotationDelta.ToAngleAxis(out angle, out axis);
            this.rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }
        //case double trigger
        if(wand1 != null && wand2 != null && wand1.trigger && wand2.trigger)
        {
           distance = Vector3.Distance(wand1.transform.position, wand2.transform.position);
           change = distance - startDistance;
           transform.localScale = new Vector3(defaultScale.x * (1+change), defaultScale.y * (1+change), defaultScale.z * (1+change));
           //compare with startDistance then change 
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
    public void BeginZoom()
    {
        startDistance = Vector3.Distance(wand1.transform.position, wand2.transform.position);
        currentlyTrigger = false;
    }
    public void EndZoom()
    {
        defaultScale = transform.localScale;
        currentlyTrigger = true;
    }
    public bool IsGrabbing(){
		return currentlyGrabbing;
	}
    public bool IsTriggering(){
        return currentlyTrigger;
    }
}
