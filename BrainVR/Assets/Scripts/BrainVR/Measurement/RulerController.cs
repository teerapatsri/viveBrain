using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulerController : MonoBehaviour {
	
	private GameObject lengthIndicator;
	private LineRenderer lengthIndicatorRenderer;
	private float lengthIndicatorDiameter = 0.02F;
	private int nPin = 0;
	private bool isPinSecondPoint = false;
	private Vector3 firstPoint, secondPoint;
	
	//New Ruler
	public Plane groundPlane;
    public Transform markerObject;
	public Material rulerShader;

	void Start () {
		InitializeLengthIndicator();
	}
	
	void Update () {
		
	}

	void InitializeLengthIndicator() {
		lengthIndicator = new GameObject();
		lengthIndicator.transform.position = Vector3.zero;
		lengthIndicator.AddComponent<LineRenderer>();

		lengthIndicatorRenderer = lengthIndicator.GetComponent<LineRenderer>();

		lengthIndicatorRenderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		// lengthIndicatorRenderer.material = rulerShader;

		lengthIndicatorRenderer.startColor = Color.green;
		lengthIndicatorRenderer.endColor = Color.green;

		lengthIndicatorRenderer.startWidth = lengthIndicatorDiameter;
		lengthIndicatorRenderer.endWidth = lengthIndicatorDiameter;

		lengthIndicatorRenderer.SetPosition(0, Vector3.zero);
		lengthIndicatorRenderer.SetPosition(1, Vector3.zero);
	}

	void DrawLine()
	{
		lengthIndicatorRenderer.SetPosition(0, firstPoint);
		lengthIndicatorRenderer.SetPosition(1, secondPoint);	
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
		DrawLine();
		nPin = (nPin + 1) % 2;
	}

	public void UpdateCurrentPoint(Vector3 currentPoint)
	{
		if(!isPinSecondPoint) 
		{
			secondPoint = currentPoint;
		}
		DrawLine();
	}

}
