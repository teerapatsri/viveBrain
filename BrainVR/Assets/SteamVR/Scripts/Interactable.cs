using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {
    [Header(" [Other object references]")]
    [Tooltip("Left Wand")]
    public WandController leftWand;
    [Tooltip("Right Wand")]
    public WandController rightWand;
    [Header(" [Rigid Body]")]
    public new Rigidbody rigidbody;
    public Rigidbody rbPlane;

    private bool currentlyMoving, currentlyRotating, currentlyZooming;

    private WandController attachedWand;
	private Transform interactionPoint;
	private Vector3 posDelta, axis;
	private Quaternion wandPreRotation, preRotation, rotationDelta;

    private float startDistance;
    private float distance;
	private float angle;
    private float change;
	private float rotationFactor = 40000f;
    private float velocityFactor = 20000f;
    private Vector3 defaultScale, preScale, localPos;
    // Use this for initialization
    void Start () {
		rigidbody = GetComponent<Rigidbody> ();
        defaultScale = transform.localScale;
        localPos = rbPlane.transform.localPosition;
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
        if (rbPlane.angularVelocity.sqrMagnitude > 0.01)
        {
            rbPlane.transform.localPosition = localPos;
        }
        if (attachedWand != null && currentlyMoving) {
            posDelta = attachedWand.transform.position - interactionPoint.position; //position changed
            rigidbody.velocity = posDelta * velocityFactor * Time.deltaTime;
            rbPlane.velocity = posDelta * velocityFactor * Time.deltaTime;
        }
        //case single trigger = rotate object
        if (attachedWand != null && currentlyRotating)
        {
            rotationDelta = Quaternion.LookRotation(attachedWand.transform.position - transform.position) * Quaternion.Inverse(preRotation); //WAND POSITION DELTA
            rotationDelta *= Quaternion.Inverse(attachedWand.transform.rotation * Quaternion.Inverse(wandPreRotation));//PLUS WAND ROTATION DELTA
            rotationDelta.x = 0;
            rotationDelta.z = 0;
            rotationDelta.ToAngleAxis(out angle, out axis);
            rigidbody.angularVelocity = (Time.deltaTime * angle * axis) * rotationFactor;
            rbPlane.angularVelocity = (Time.deltaTime * angle * axis) * rotationFactor;
            if (rigidbody.angularVelocity.sqrMagnitude >= 0)
            {
                int index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestLeft);
                if (rightWand.IsTriggering())
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
        if (leftWand != null && rightWand != null && currentlyZooming)
        {
           distance = Vector3.Distance(leftWand.transform.position, rightWand.transform.position); //distance between wands
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

		currentlyMoving = true;
        currentlyRotating = false;
        currentlyZooming = false;
        int index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestLeft);
        if (rightWand.IsGripping())
        {
            index = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.FarthestRight);
        }
        SteamVR_Controller.Input(index).TriggerHapticPulse(3000);
    }
	public void EndGrab(WandController wand){
		if (wand == attachedWand) {
			attachedWand = null;
			currentlyMoving = false;
		}
	}
    public void BeginTrigger(WandController wand)
    {
        localPos = rbPlane.transform.localPosition;
        attachedWand = wand;
        preRotation = Quaternion.LookRotation(attachedWand.transform.position - transform.position);
        currentlyRotating = true;
        currentlyMoving = false;
        currentlyZooming = false;
    }
    public void EndTrigger(WandController wand)
    {
        if (wand == attachedWand)
        {
            attachedWand = null;
            currentlyRotating = false;
        }
    }
    public void BeginZoom()
    {
        startDistance = Vector3.Distance(leftWand.transform.position, rightWand.transform.position);
        currentlyZooming = true;
        currentlyMoving = false;
        currentlyRotating = false; //to ensure getting out of rotating mode
    }
    public void EndZoom()
    {
        preScale = transform.localScale; //redefine defaultScale
        currentlyZooming = false;
        currentlyRotating = true;
    }
    public bool IsMoving(){
		return currentlyMoving;
	}
    public bool IsTriggering(){
        return currentlyRotating;
    }
    public bool IsZooming()
    {
        return currentlyZooming;
    }
}
