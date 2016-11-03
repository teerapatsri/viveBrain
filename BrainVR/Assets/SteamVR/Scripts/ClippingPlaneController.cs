using UnityEngine;
using System.Collections;

public class ClippingPlaneController : MonoBehaviour
{
    private new Rigidbody rigidbody;
    private bool currentlyMovingPlane;
    private bool currentlyRotatingPlane;
    private WandController attachedWand;
    private Transform interactionPoint;
    private Vector3 posDelta, axis, localPos;
    private Quaternion rotationDelta;
    private float angle;
    private float rotationFactor = 400f;
    private float velocityFactor = 20000f;
    // Use this for initialization
    void Start()
    {
        localPos = new Vector3(0, -0.2f, 0);
        rigidbody = GetComponent<Rigidbody>();
        interactionPoint = new GameObject().transform;
        velocityFactor /= rigidbody.mass; //mass makes object go slower
        rotationFactor /= rigidbody.mass;  // and harder to rotate?
    }

    //rotation example
    // Update is called once per frame
    void Update()
    {
        transform.localPosition = localPos;
        //case grab = move object around
        if (attachedWand != null && currentlyMovingPlane)
        {
            posDelta = attachedWand.transform.position - interactionPoint.position; //position changed
            Vector3 normal = transform.rotation * Vector3.up;
            posDelta = Vector3.Project(posDelta, normal);
            localPos += posDelta;
        }
        //case single trigger = rotate object
        if (attachedWand != null && currentlyRotatingPlane)
        {
            localPos = new Vector3(0, -0.2f, 0); //reset position to cube center
            rotationDelta = attachedWand.transform.rotation * Quaternion.Inverse(interactionPoint.rotation);
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
            {
                angle -= 360;
            }
            rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }
    }
    public void BeginMovePlane(WandController wand)
    {
        attachedWand = wand;
        interactionPoint.position = wand.transform.position;
        interactionPoint.rotation = wand.transform.rotation;
        interactionPoint.SetParent(transform, true);
        currentlyMovingPlane = true;
    }
    public void EndMovePlane(WandController wand)
    {
        if (wand == attachedWand)
        {
            attachedWand = null;
            currentlyMovingPlane = false;
        }
    }
    public void BeginRotatePlane(WandController wand)
    {
        attachedWand = wand;
        interactionPoint.position = wand.transform.position;
        interactionPoint.rotation = wand.transform.rotation;
        interactionPoint.SetParent(transform, true);
        currentlyRotatingPlane = true;
    }
    public void EndRotatePlane(WandController wand)
    {
        if (wand == attachedWand)
        {
            attachedWand = null;
            currentlyRotatingPlane = false;
        }
    }
}
