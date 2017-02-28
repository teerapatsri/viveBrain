using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour {
    public GameController gameController;

    private bool isActive;

    public bool IsActive
    {
        get
        {
            return isActive;
        }

        set
        {
            isActive = value;
        }
    }

    // Use this for initialization
    void Start () {
        IsActive = false;
    }
	
	// Update is called once per frame
	void Update () {
		switch (gameController.PlaneAxis)
        {
            case ("Axial"):
                {
                    break;
                }
            case ("Coronal"):
                {
                    break;
                }
            case ("Sagittal"):
                {
                    break;
                }
        }
	}
}
