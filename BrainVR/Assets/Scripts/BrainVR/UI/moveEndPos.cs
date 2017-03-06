using UnityEngine;
using System.Collections;

public class moveEndPos : MonoBehaviour {
    public Transform endPos;
	
	// Update is called once per frame
	void Update () {
        transform.position = endPos.position;
	}
}
