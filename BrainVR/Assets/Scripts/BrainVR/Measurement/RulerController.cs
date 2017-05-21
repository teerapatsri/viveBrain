using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulerController : MonoBehaviour {
	
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

	// Cylinder Ruler 

	void Start () {
		InitializeLengthIndicator();
	}

	void Update () {
		
	}

	void DrawLengthIndicator () {
		lengthIndicatorWrapper.transform.position = (secondPoint - firstPoint)/2 + firstPoint;
		lengthIndicatorWrapper.transform.LookAt(secondPoint);
		
		// Scale
		var v3T = lengthIndicatorWrapper.transform.localScale;
		v3T.y = (secondPoint-firstPoint).magnitude/2;
		v3T.z = 0.01f;
		v3T.x = 0.01f;
		lengthIndicatorWrapper.transform.localScale = v3T;
		
		// Rotate
		lengthIndicatorWrapper.transform.rotation = Quaternion.FromToRotation(Vector3.up, secondPoint-firstPoint);
	}

	void InitializeLengthIndicator() {
		lengthIndicatorWrapper = new GameObject();
		lengthIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		// lengthIndicator.transform.position = Vector3.zero;
		lengthIndicatorRenderer = lengthIndicator.GetComponent<Renderer>();
		lengthIndicatorRenderer.material.color = Color.green;
		lengthIndicator.transform.parent = lengthIndicatorWrapper.transform;
	}

	public void PinPoint(Vector3 pinPoint)
	{
		// Debug.Log(nPin);
		// Debug.Log(pinPoint);
		if(nPin == 0)
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
		nPin = (nPin + 1) % 2;
	}

	public void UpdateCurrentPoint(Vector3 currentPoint)
	{
		if(!isPinSecondPoint) 
		{
			secondPoint = currentPoint;
		}
		DrawLengthIndicator();
	}

}
