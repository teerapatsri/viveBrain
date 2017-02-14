using UnityEngine;
using System.Collections;

public class ClippingPlaneController : MonoBehaviour
{
    public Transform cube;
    public Material clippedObjectMaterial;
    
    private new Rigidbody rigidbody;
    private bool currentlyMovingPlane;

    private WandController attachedWand;
    private Transform prePos;
    private Vector3 posDelta;

    private float rotationFactor = 400f;
    private float velocityFactor = 20000f;
    private string anatomicalPlane;
    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        prePos = new GameObject().transform;
        velocityFactor /= rigidbody.mass; //mass makes object go slower
        rotationFactor /= rigidbody.mass;  // and harder to rotate?
        anatomicalPlane = "Axial";
    }

    //rotation example
    // Update is called once per frame
    void Update()
    {
        if (cube != null)
        {
            if (attachedWand != null && currentlyMovingPlane)
            {
                posDelta = attachedWand.transform.position - prePos.position; //position changed
                Vector3 normal = Vector3.forward;//get sliding axis
                switch (anatomicalPlane)
                {
                    case ("Axial"):
                        {
                            posDelta.x = 0;
                            posDelta.z = 0;
                            break;
                        }
                    case ("Coronal"):
                        {
                            posDelta.y = 0;
                            normal = cube.rotation * normal;
                            posDelta = Vector3.Project(posDelta, normal);
                            break;
                        }
                    case ("Sagittal"):
                        {
                            posDelta.y = 0;
                            normal = cube.rotation * normal;
                            posDelta = Vector3.ProjectOnPlane(posDelta, normal);
                            break;
                        }
                }
                transform.position += posDelta;
                prePos.position = attachedWand.transform.position;
            }
        }
    }
    public void BeginMovePlane(WandController wand)
    {
        attachedWand = wand;
        prePos.position = wand.transform.position;
        prePos.rotation = wand.transform.rotation;
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
    public void RotatePlane()
    {
        if (cube != null)
        {
            transform.position = cube.position;
        }
        switch (anatomicalPlane)
        {
            case ("Axial"):
                {
                    anatomicalPlane = "Coronal";
                    transform.localRotation = Quaternion.Euler(90, 0, 0);
                    break;
                }
            case ("Coronal"):
                {
                    anatomicalPlane = "Sagittal";
                    transform.localRotation = Quaternion.Euler(90, 90, 0);
                    break;
                }
            case ("Sagittal"):
                {
                    anatomicalPlane = "Axial";
                    transform.localRotation = Quaternion.Euler(180, 0, 0);
                    break;
                }

        }
        currentlyMovingPlane = false;
    }
    public string CurrentAxis()
    {
        return anatomicalPlane;
    }
    public bool IsMovingPlane()
    {
        return currentlyMovingPlane;
    }

    Vector4 GetPlaneVector()
    {
        if (transform == null) return Vector4.zero;

        var p = new Plane(transform.up, transform.position);
        return new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
    }

    void LateUpdate()
    {
        if (clippedObjectMaterial != null)
        {
            clippedObjectMaterial.SetVector("_ClipPlane", GetPlaneVector());
        }
    }
}
