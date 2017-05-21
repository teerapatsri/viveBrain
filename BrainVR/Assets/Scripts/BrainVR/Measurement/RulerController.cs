using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RulerController : MonoBehaviour
{

    private GameObject lengthIndicatorWrapper;
    private GameObject lengthIndicator;

    private Renderer lengthIndicatorRenderer;
    private float lengthIndicatorDiameter = 0.02F;
    private int nPin = 0;
    private bool isPinSecondPoint = false;
    private Vector3 firstPoint, secondPoint;

    //New Ruler
    public Plane groundPlane;
    public Transform markerObject;
    public Material rulerShader;

    public Text indicatorLength;
    private GameObject cubeObj;
    private RayMarchingMasterController rayMarchingMasterController;

    void Start()
    {
        InitializeLengthIndicator();
        cubeObj = GameObject.FindWithTag("VREnvironment").GetComponent<VREnvironmentController>().cubeObj;
        rayMarchingMasterController = GameObject.FindWithTag("GameController").GetComponent<RayMarchingMasterController>();
    }

    public Canvas rulerCanvas;
    public Interactable cube;

    private float offset = 0.1f;
    // Update is called once per frame
    void Update()
    {
        rulerCanvas.transform.position = lengthIndicatorWrapper.transform.position + offset * Vector3.up;

        Vector3 v = Camera.main.transform.position - rulerCanvas.transform.position;
        v.x = v.z = 0.0f;
        rulerCanvas.transform.LookAt(Camera.main.transform.position - v);
        rulerCanvas.transform.Rotate(0, 180, 0);
    }

    void DrawLengthIndicator()
    {
        lengthIndicator.SetActive(true);

        lengthIndicatorWrapper.transform.position = (secondPoint - firstPoint) / 2 + firstPoint;
        lengthIndicatorWrapper.transform.LookAt(secondPoint);

        // Scale
        var v3T = lengthIndicatorWrapper.transform.localScale;
        v3T.y = (secondPoint - firstPoint).magnitude / 2;
        v3T.z = 0.01f;
        v3T.x = 0.01f;
        lengthIndicatorWrapper.transform.localScale = v3T;

        // Rotate
        lengthIndicatorWrapper.transform.rotation = Quaternion.FromToRotation(Vector3.up, secondPoint - firstPoint);
        indicatorLength.transform.localPosition = Vector3.zero;

    }

    void InitializeLengthIndicator()
    {
        lengthIndicatorWrapper = new GameObject();
        lengthIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        // lengthIndicator.transform.position = Vector3.zero;
        lengthIndicatorRenderer = lengthIndicator.GetComponent<Renderer>();
        lengthIndicatorRenderer.material.color = Color.green;
        lengthIndicator.transform.parent = lengthIndicatorWrapper.transform;
        lengthIndicator.SetActive(false);

        SetIndicatorText(0);

    }

    public void PinPoint(Vector3 pinPoint)
    {
        // Debug.Log(nPin);
        // Debug.Log(pinPoint);
        if (nPin == 0)
        {
            // First point
            isPinSecondPoint = false;
            firstPoint = pinPoint;
            secondPoint = pinPoint;
        }
        else
        {
            // Second point
            isPinSecondPoint = true;
            secondPoint = pinPoint;
        }
        // DrawLine();
        DrawLengthIndicator();
        // updateLength((secondPoint - firstPoint).magnitude);
        UpdateLength();
        nPin = (nPin + 1) % 2;
    }

    public void UpdateCurrentPoint(Vector3 currentPoint)
    {
        if (!isPinSecondPoint)
        {
            secondPoint = currentPoint;
        }
        DrawLengthIndicator();
        // updateLength((secondPoint - firstPoint).magnitude);
        UpdateLength();
    }

    public void UpdateLength()
    {
        var localFirstPoint = cubeObj.transform.InverseTransformPoint(firstPoint);
        var localSecondPoint = cubeObj.transform.InverseTransformPoint(secondPoint);
        var currentImageSet = rayMarchingMasterController.GetCurrentImageSet();
        if (currentImageSet.HasValue)
        {
            var c = currentImageSet.Value;
            localFirstPoint.Scale(new Vector3(c.ImgXSize, c.ImgYSize, c.ImgZSize));
            localFirstPoint.Scale(new Vector3((float)c.XScale, (float)c.YScale, (float)c.ZScale));
            localSecondPoint.Scale(new Vector3(c.ImgXSize, c.ImgYSize, c.ImgZSize));
            localSecondPoint.Scale(new Vector3((float)c.XScale, (float)c.YScale, (float)c.ZScale));
        }
        else
        {
            SetIndicatorText(-1);
            return;
        }
        float length = (localSecondPoint - localFirstPoint).magnitude;
        SetIndicatorText(length);
    }

    private void SetIndicatorText(float length)
    {
        if (length < 0)
        {
            indicatorLength.text = "Loading";
        }else
        {
            indicatorLength.text = string.Format("{0:N3} mm", length);
        }
    }
}
