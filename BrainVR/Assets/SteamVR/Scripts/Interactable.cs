using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {
	public new Rigidbody rigidbody;
    public Rigidbody rbPlane;
    public WandController wand1;
    public WandController wand2;
    private bool currentlyGrabbing, currentlyTrigger, currentlyZooming;
    private WandController attachedWand;
	private Transform interactionPoint;
	private Vector3 posDeltaG, posDeltaT, axis;
	private Quaternion wandPreRotation, preRotation, rotationDelta;
    private float startDistance;
    private float distance;
	private float angle;
    private float change;
	private float rotationFactor = 40000f;
    private float velocityFactor = 20000f;
    private Vector3 defaultScale, preScale;
    // Use this for initialization
    void Start () {
		rigidbody = GetComponent<Rigidbody> ();
        defaultScale = transform.localScale;
        preScale = defaultScale;
        preRotation = transform.rotation;
        interactionPoint = new GameObject ().transform;
		velocityFactor /= rigidbody.mass; //mass makes object go slower
		rotationFactor /= rigidbody.mass;  // and harder to rotate?
        
    }

    //rotation example
    // Update is called once per frame
    void Update () {
        //case grab = move object around
		if (attachedWand != null && currentlyGrabbing) {
			posDeltaG = attachedWand.transform.position - interactionPoint.position; //position changed
            rigidbody.velocity = posDeltaG * velocityFactor * Time.deltaTime;
        }
        //case single trigger = rotate object
        if (attachedWand != null && currentlyTrigger && !currentlyZooming) //if both triggered go to case 3
        {
            rotationDelta = Quaternion.LookRotation(attachedWand.transform.position - transform.position) * Quaternion.Inverse(preRotation);
            rotationDelta *= Quaternion.Inverse(attachedWand.transform.rotation * Quaternion.Inverse(wandPreRotation));
            //rotationDelta *= rotationDelta;
            rotationDelta.x = 0;
            rotationDelta.z = 0;
            rotationDelta.ToAngleAxis(out angle, out axis);
            rigidbody.angularVelocity = (Time.deltaTime * angle * axis) * rotationFactor;
            rbPlane.angularVelocity = (Time.deltaTime * angle * axis) * rotationFactor;
            if (rigidbody.angularVelocity.sqrMagnitude >= 0)
            {
                int index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestLeft);
                if (wand2.trigger)
                {
                    index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestRight);
                }
                SteamVR_Controller.Input(index).TriggerHapticPulse((ushort)Mathf.Lerp(0, 1000, rigidbody.angularVelocity.sqrMagnitude/50));

            }
            //transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * rotationDelta, rotationFactor * Time.deltaTime);
            preRotation = Quaternion.LookRotation(attachedWand.transform.position - transform.position);
            wandPreRotation = attachedWand.transform.rotation;
        }
        //case double trigger = ZOOM (Scale) object
        if (wand1 != null && wand2 != null && currentlyZooming)
        {
           distance = Vector3.Distance(wand1.transform.position, wand2.transform.position); //distance between wands
           change = distance - startDistance;   //difference of default and current distance
           transform.localScale = new Vector3(Mathf.Clamp(preScale.x * (1+change), defaultScale.x * 0.1f, defaultScale.x * 3f),
                                                Mathf.Clamp(preScale.y * (1 + change), defaultScale.y * 0.1f, defaultScale.y * 3f),
                                                Mathf.Clamp(preScale.z * (1 + change), defaultScale.z * 0.1f, defaultScale.z * 3f));
            
           //compare with startDistance then change 
        }
    }
    
public void BeginGrab(WandController wand){
		attachedWand = wand;
		interactionPoint.position = wand.transform.position;
		interactionPoint.rotation = wand.transform.rotation;
		interactionPoint.SetParent (transform, true);

		currentlyGrabbing = true;
        currentlyTrigger = false;
        currentlyZooming = false;
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
        preRotation = Quaternion.LookRotation(attachedWand.transform.position - transform.position);
        currentlyTrigger = true;
        currentlyGrabbing = false;
        currentlyZooming = false;
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
        currentlyZooming = true;
        currentlyGrabbing = false;
        currentlyTrigger = false; //to ensure getting out of rotating mode
    }
    public void EndZoom()
    {
        preScale = transform.localScale; //redefine defaultScale
        currentlyZooming = false;
        currentlyTrigger = true;
    }
    public bool IsGrabbing(){
		return currentlyGrabbing;
	}
    public bool IsTriggering(){
        return currentlyTrigger;
    }
}
