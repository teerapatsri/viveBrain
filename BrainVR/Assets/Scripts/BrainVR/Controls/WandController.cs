using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WandController : MonoBehaviour
{
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
    private Valve.VR.EVRButtonId padButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_LaserPointer laser;

    [Header(" [Other object references]")]
    [Tooltip("The Other Wand")]
    public WandController otherWand;
    [Tooltip("Interactable Object")]
    public Interactable interactableController;
    [Tooltip("Clipping Plane")]
    public ClippingPlaneController planeController;
    [Tooltip("Radial Menu Spawner")]
    public RadialMenuSpawner spawner;
    public GameController gameController;


    private RadialMenu menuSpawned;
    private bool gripEnabled, trigEnabled, menuEnabled;
    private bool trigger;
    private bool grip;
    private bool showMenu;
    private bool controllingPlane;
    private float theta;
    private Vector2 prevAxis;
    private int option;

    [System.Serializable]
    public class Action
    {
        public Sprite sprite;
        public Sprite activeSprite;
        public string title;
    }

    public Action[] options;

    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laser = GetComponent<SteamVR_LaserPointer>();
        laser.active = false;
        trigger = false;
        controllingPlane = false;
        grip = false;
        showMenu = false;

        gripEnabled = true;
        trigEnabled = true;
        menuEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null)
        {
            Debug.Log("Controller not initialized");
            return;
        }
        if (controller.GetPressDown(menuButton) && menuEnabled)
        {
            /*
            changeMode();
            otherWand.changeMode();
            */
        }
        if (controller.GetPressUp(menuButton))
        {
        }
        if (controller.GetPressDown(triggerButton) && trigEnabled)
        {
            if (CompareTag("LeftWand"))
            {//PLANE mode
                trigger = true;
                interactableController.BeginZoom();
                //planeController.RotatePlane();
            }
            else if (CompareTag("RightWand"))
            {//NORMAL mode
                trigger = true;
                laser.active = !laser.active;
                //interactableController.BeginRotate(this);
            }
        }
        if (controller.GetPressUp(triggerButton))
        {
            //planeController.EndRotatePlane(this);
            trigger = false;
            interactableController.EndRotate(this);
            interactableController.EndZoom();
        }
        if (controller.GetPressDown(gripButton) && gripEnabled)
        {
            if (CompareTag("LeftWand"))//Plane mode
            {
                grip = false;
                planeController.BeginMovePlane(this);
            }
            else if (CompareTag("RightWand"))//NORMAL mode
            {
                grip = true;
                otherWand.grip = false;
                interactableController.BeginGrab(this);
                controller.TriggerHapticPulse(3000);
            }
        }
        if (controller.GetPressUp(gripButton))
        {
            planeController.EndMovePlane(this);
            interactableController.EndGrab(this);
            grip = false;
        }
        if (controller.GetPressDown(padButton)) //show menu and pressing menu
        {
            Debug.Log("Option: " + option);
            GameObject cubeObj = GameObject.Find("Cube");
            var renderStyle = cubeObj.GetComponent<CubeRenderStyleController>();

            //Spawn radial menu
            if (!showMenu && !otherWand.ShowingMenu() && menuSpawned == null)// no menu opened
            {
                SpawnMenu();
            }
            else if (!showMenu && otherWand.ShowingMenu())//other wand showing
            {
                otherWand.CloseMenu();
                SpawnMenu();
            }
            else if (showMenu)//this wand showing
            {
                if (option == -1)
                {
                    CloseMenu();
                }
                else if (option == 0)
                {
                    renderStyle.SetTwoSideClipping(!renderStyle.IsTwoSideClipping);
                    if (renderStyle.IsTwoSideClipping)
                    {
                        gameController.TwoSideText = "Two sides view";
                    }
                    else
                    {
                        gameController.TwoSideText = "One side view";
                    }
                    //TODO fix double clip mode in Axial mode
                }
                else if (option == 1)
                {
                    //change Color
                    int shaderNumber = (renderStyle.ShaderNumber + 1) % 8;
                    renderStyle.SetShaderNumber(shaderNumber);
                    gameController.ShaderNumber = shaderNumber + 1;
                }
                else if (option == 2)
                {
                    //switch plane
                    planeController.RotatePlane();
                }
            }
        }
        // && (controller.GetAxis().x != 0 || controller.GetAxis().y != 0)
        if (controller.GetTouch(padButton) && menuSpawned != null)//locate thumb on pad
        {
            Vector2 pos = new Vector2(controller.GetAxis().x, controller.GetAxis().y);
            float radius = pos.magnitude;
            if (radius >= 0.4)
            {
                menuSpawned.ExitButtonTransparent();
                float arcRad = (2 * Mathf.PI / options.Length); //radians dividing region
                float theta = (Mathf.Atan2(pos.y, pos.x) + 1.5f * Mathf.PI - arcRad / 2) % (2 * Mathf.PI);//angle of thumb + offset to check easier
                for (int i = 0; i < options.Length; i++)
                {
                    if (theta >= arcRad * i && theta <= arcRad * (i + 1))
                    {
                        if (option != i)
                        {
                            controller.TriggerHapticPulse(500);
                            option = i;
                            menuSpawned.selected = i;
                        }
                    }
                }
            }
            else
            {
                menuSpawned.ExitButtonOpaque();
                option = -1;
                menuSpawned.selected = -1;
            }

        }
        else if (controller.GetTouchDown(padButton) && menuSpawned == null)
        {
            interactableController.BeginRotate(this);
            prevAxis = controller.GetAxis();
        }
        if (controller.GetTouch(padButton) && menuSpawned == null)
        {
            Vector2 axis = controller.GetAxis();
            //theta = (Mathf.Atan2(axis.y, axis.x) + Mathf.PI) - (Mathf.Atan2(padAxis.y, padAxis.x) + Mathf.PI);
            theta = axis.x - prevAxis.x;
            if (Mathf.Abs(theta) > 1) theta = 0;
            Debug.Log("theta = " + theta);
            prevAxis = axis;
            interactableController.Rotate(theta);
        }
        if (controller.GetTouchUp(padButton) && menuSpawned != null)
        {
            option = -1;
            menuSpawned.selected = -1;
        }

    }
    public void GripEnable()
    {
        gripEnabled = true;
    }
    public void GripDisable()
    {
        gripEnabled = false;
    }
    public void TrigEnable()
    {
        trigEnabled = true;
    }
    public void TrigDisable()
    {
        trigEnabled = false;
    }
    public void MenuEnable()
    {
        menuEnabled = true;
    }
    public void MenuDisable()
    {
        menuEnabled = false;
    }
    public bool IsGripDown()
    {
        return grip;
    }
    public bool IsTriggerDown()
    {
        return trigger;
    }
    public bool ShowingMenu()
    {
        return showMenu;
    }
    public void changeMode()
    {
        controllingPlane = !controllingPlane;
    }
    public bool IsControllingPlane()
    {
        return controllingPlane;
    }
    private void SpawnMenu()
    {
        menuSpawned = spawner.SpawnMenu(this, gameController);
        showMenu = true;
    }
    public void CloseMenu()
    {
        spawner.CloseMenu(menuSpawned);
        menuSpawned = null;
        showMenu = false;
    }
}
