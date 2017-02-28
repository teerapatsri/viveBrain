using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeLooker : MonoBehaviour {
    // Update is called once per frame
    public GameObject cube;
    void Start()
    {
        cube = GameObject.Find("Cube");
    }
	void Update () {
        if(cube != null)
            transform.rotation = Quaternion.LookRotation(cube.transform.position - transform.position);
    }
}
