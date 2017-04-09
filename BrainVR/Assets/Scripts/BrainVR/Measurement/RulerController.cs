using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulerController : MonoBehaviour {

	private GameObject lengthIndicator;
	private LineRenderer lengthIndicatorRenderer;

	private float lengthIndicatorDiameter = 0.02F;
	public Vector3 firstPoint;

	void Start () {
		InitializeLengthIndicator();
	}
	
	void Update () {
		
	}

	void InitializeLengthIndicator() {
		lengthIndicator = new GameObject();
		lengthIndicator.transform.position = Vector3.zero;
		lengthIndicator.AddComponent<LineRenderer>();

		LineRenderer lengthIndicatorRenderer = lengthIndicator.GetComponent<LineRenderer>();

		lengthIndicatorRenderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
	
		lengthIndicatorRenderer.startColor = Color.green;
		lengthIndicatorRenderer.endColor = Color.green;

		lengthIndicatorRenderer.startWidth = lengthIndicatorDiameter;
		lengthIndicatorRenderer.endWidth = lengthIndicatorDiameter;

		lengthIndicatorRenderer.SetPosition(0, Vector3.zero);
		lengthIndicatorRenderer.SetPosition(1, Vector3.zero);
	}

}
