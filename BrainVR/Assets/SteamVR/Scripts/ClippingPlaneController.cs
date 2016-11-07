using UnityEngine;
using System.Collections;

public class ClippingPlaneController : MonoBehaviour
{
    private new Rigidbody rigidbody;
    private bool currentlyMovingPlane;
    private bool currentlyRotatingPlane;
    private WandController wand1, wand2;
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
        if (wand1 != null && currentlyMovingPlane)
        {
            posDelta = wand1.transform.position - interactionPoint.position; //position changed
            Vector3 normal = transform.rotation * Vector3.up;//get sliding axis
            posDelta = Vector3.Project(posDelta, normal);//move only along axis
            localPos += posDelta;
        }
        //case single trigger = rotate object
        if (wand1 != null && currentlyRotatingPlane)
        {
            rotationDelta = wand1.transform.rotation * Quaternion.Inverse(interactionPoint.rotation);
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
        wand1 = wand;
        interactionPoint.position = wand.transform.position;
        interactionPoint.rotation = wand.transform.rotation;
        currentlyMovingPlane = true;
        currentlyRotatingPlane = false;
    }
    public void EndMovePlane(WandController wand)
    {
        if (wand == wand1)
        {
            wand1 = null;
            currentlyMovingPlane = false;
        }
    }
    public void BeginRotatePlane(WandController wand)
    {
        wand1 = wand;
        interactionPoint.position = wand.transform.position;
        interactionPoint.rotation = wand.transform.rotation;
        interactionPoint.SetParent(transform, true);
        currentlyRotatingPlane = true;
        currentlyMovingPlane = false;
    }
    public void EndRotatePlane(WandController wand)
    {
        if (wand == wand1)
        {
            wand1 = null;
            currentlyRotatingPlane = false;
        }
    }
    public void ResetPosition()
    {
        localPos = new Vector3(0, -0.2f, 0); //reset position to cube center
        currentlyRotatingPlane = false;
    }
}
