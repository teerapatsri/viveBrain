using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour
{
    public GameController gameController;
    public Canvas[] canvasList;

    // Use this for initialization
    void Start()
    {
        canvasList = GetComponentsInChildren<Canvas>();

    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.ArrowActive)
        {
            foreach (Canvas canvas in canvasList)
            {
                canvas.enabled = true;
            }
            switch (gameController.PlaneAxis)
            {
                case ("Axial"):
                    {
                        transform.localRotation = Quaternion.Euler(0, 0, 90);
                        break;
                    }
                case ("Coronal"):
                    {
                        transform.localRotation = Quaternion.Euler(0, 90, 0);
                        break;
                    }
                case ("Sagittal"):
                    {
                        transform.localRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    }
            }
        }
        else
        {
            foreach (Canvas canvas in canvasList)
            {
                canvas.enabled = false;
            }
        }
    }
    public void RenderArrow()
    {

    }
}
