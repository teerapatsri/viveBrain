﻿using UnityEngine;
using System.Collections;

public class TutorialScript : MonoBehaviour
{
    public Color highlightColor;
    private Color defaultColor;
    [Header(" [UI]")]
    [Tooltip("Instruction Text")]
    public SteamVR_RenderModel leftWandRenderer;
    public SteamVR_RenderModel rightWandRenderer;
    public UnityEngine.UI.Text instructionText;
    public UnityEngine.UI.Text grabLeft;
    public UnityEngine.UI.Text grabRight;
    public UnityEngine.UI.Text triggerLeft;
    public UnityEngine.UI.Text triggerRight;
    public UnityEngine.UI.Text menuLeft;
    public UnityEngine.UI.Text menuRight;
    public UnityEngine.UI.Text currentMode;
    public UnityEngine.UI.Text anatomicalPlane;
    public Transform backBar;
    public Transform progressBar;
    [Header(" [Input]")]
    public WandController leftWand;
    public WandController rightWand;
    public Interactable cube;
    public ClippingPlaneController plane;

    private string phase;
    private float progress;
    private float barLength = 30, barWidth = 5;
    private float angle;
    private float delay, delayTime = 1.5f;
    private bool holdingLeftTrigger = false, holdingRightTrigger = false;
    private Vector3 axis;
    private Vector3 prePos, posDelta;
    private Quaternion preRot, rotDelta;
    // Use this for initialization
    void Start()
    {
        phase = "Grab";
        prePos = cube.transform.position;
        preRot = cube.transform.rotation;
        progress = 0;
        delay = delayTime;
        grabLeft.text = "";
        grabRight.text = "";
        triggerLeft.text = "";
        triggerRight.text = "";
        menuLeft.text = "";
        menuRight.text = "";
        currentMode.text = "";
        anatomicalPlane.text = "";
        backBar.localScale = new Vector3(barLength, barWidth, 1);
        defaultColor = Color.gray;
        Transform lgrip = leftWandRenderer.FindComponent("lgrip");
        if (lgrip!=null) defaultColor = lgrip.GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    private void updateBar()
    {
        progressBar.localScale = new Vector3(progress * barLength, barWidth, 1);
        progressBar.localPosition = new Vector3(-barLength / 2 + progress * barLength / 2, -20, 0);
    }
    private void destroyBar()
    {
        progressBar.localScale = Vector3.zero;
        backBar.localScale = Vector3.zero;
    }
    private void highlightGrip()
    {
        Transform lgrip = leftWandRenderer.FindComponent("lgrip");
        if (lgrip != null)
        {
            lgrip.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
        Transform rgrip = leftWandRenderer.FindComponent("rgrip");
        if (rgrip != null)
        {
            rgrip.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
        lgrip = rightWandRenderer.FindComponent("lgrip");
        if (lgrip != null)
        {
            lgrip.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
        rgrip = rightWandRenderer.FindComponent("rgrip");
        if (rgrip != null)
        {
            rgrip.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
    }
    private void unHighlightGrip()
    {
        Transform lgrip = leftWandRenderer.FindComponent("lgrip");
        if (lgrip != null)
        {
            lgrip.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        Transform rgrip = leftWandRenderer.FindComponent("rgrip");
        if (rgrip != null)
        {
            rgrip.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        lgrip = rightWandRenderer.FindComponent("lgrip");
        if (lgrip != null)
        {
            lgrip.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        rgrip = rightWandRenderer.FindComponent("rgrip");
        if (rgrip != null)
        {
            rgrip.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
    }
    private void highlightTrig()
    {
        Transform trigger = leftWandRenderer.FindComponent("trigger");
        if (trigger != null)
        {
            trigger.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
        trigger = rightWandRenderer.FindComponent("trigger");
        if (trigger != null)
        {
            trigger.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
    }
    private void unHighlightTrig()
    {
        Transform trigger = leftWandRenderer.FindComponent("trigger");
        if (trigger != null)
        {
            trigger.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        trigger = rightWandRenderer.FindComponent("trigger");
        if (trigger != null)
        {
            trigger.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
    }
    private void highlightMenu()
    {
        Transform menu = leftWandRenderer.FindComponent("button");
        if (menu != null)
        {
            menu.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
        menu = rightWandRenderer.FindComponent("button");
        if (menu != null)
        {
            menu.GetComponent<MeshRenderer>().material.color = Color.Lerp(defaultColor, highlightColor, Mathf.PingPong(Time.time, 1));
        }
    }
    private void unHighlightMenu()
    {
        Transform menu = leftWandRenderer.FindComponent("button");
        if (menu != null)
        {
            menu.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        menu = rightWandRenderer.FindComponent("button");
        if (menu != null)
        {
            menu.GetComponent<MeshRenderer>().material.color = defaultColor;
        }
    }
    void Update()
    {
        if (leftWand.IsControllingPlane() || rightWand.IsControllingPlane())
        {
            currentMode.text = "Plane Mode";
            anatomicalPlane.text = plane.CurrentAxis();
        }
        else
        {
            currentMode.text = "Free Mode";
        }
        switch (phase)
        {
            case ("Grab"):
                {
                    instructionText.text = "Hold either of the GRIP buttons to MOVE the model";
                    grabLeft.text = "Move";
                    grabRight.text = "Move";
                    triggerLeft.text = "";
                    triggerRight.text = "";
                    menuLeft.text = "";
                    menuRight.text = "";

                    leftWand.TrigDisable();
                    rightWand.TrigDisable();
                    leftWand.MenuDisable();
                    rightWand.MenuDisable();
                    //calculate Progress
                    if (cube.IsMoving())
                    {
                        posDelta = cube.transform.position - prePos;

                        progress += posDelta.magnitude * Time.deltaTime * 20f;

                        prePos = cube.transform.position;
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        instructionText.text = "Great!";
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "Rotate";
                            progress = 0;
                            delay = delayTime;
                            unHighlightGrip();
                            Transform trigger = leftWandRenderer.FindComponent("trigger");
                            if (trigger != null)
                            {
                                defaultColor = trigger.GetComponent<MeshRenderer>().material.color;
                            }
                            break;
                        }
                    }
                    //render bar
                    updateBar();
                    //HighLight
                    highlightGrip();
                    
                    break;
                }
            case ("Rotate"):
                {
                    instructionText.text = "Hold one of the TRIGGER buttons to ROTATE";
                    grabLeft.text = "Move";
                    grabRight.text = "Move";
                    triggerLeft.text = "Rotate";
                    triggerRight.text = "Rotate";

                    leftWand.TrigEnable();
                    rightWand.TrigEnable();
                    //calculate Progress
                    if (cube.IsTriggering())
                    {
                        rotDelta = cube.transform.rotation * Quaternion.Inverse(preRot);
                        rotDelta.ToAngleAxis(out angle, out axis);
                        progress += angle * 0.0005f;
                        preRot = cube.transform.rotation;
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        instructionText.text = "Awesome!";
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "Zoom";
                            progress = 0;
                            delay = delayTime;
                        }
                    }
                    //render bar
                    updateBar();
                    highlightTrig();
                    break;
                }
            case ("Zoom"):
                {
                    instructionText.text = "Hold BOTH the TRIGGER buttons to ZOOM";
                    triggerLeft.text = "Zoom";
                    triggerRight.text = "Zoom";
                    //calculate Progress
                    if (cube.IsZooming())
                    {
                        progress += 0.2f * Time.deltaTime;
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        instructionText.text = "Fantastic!";
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "Change Mode";
                            progress = 0;
                            delay = delayTime;
                            unHighlightTrig();
                        }
                    }
                    //render bar
                    updateBar();
                    break;
                }
            case ("Change Mode"):
                {
                    instructionText.text = "Press the Menu button to switch to Plane mode";
                    grabLeft.text = "";
                    grabRight.text = "";
                    triggerLeft.text = "";
                    triggerRight.text = "";
                    menuLeft.text = "Change Mode";
                    menuRight.text = "Change Mode";
                    leftWand.GripDisable();
                    rightWand.GripDisable();
                    leftWand.TrigDisable();
                    rightWand.TrigDisable();
                    leftWand.MenuEnable();
                    rightWand.MenuEnable();
                    //calculate Progress
                    if (leftWand.IsControllingPlane() || rightWand.IsControllingPlane())
                    {
                        progress += 3f * Time.deltaTime;
                        instructionText.text = "Enter Cutting Mode!";
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "RotatePlane";
                            progress = 0;
                            delay = delayTime;
                            preRot = plane.transform.rotation;
                            unHighlightMenu();
                            break;
                        }
                    }
                    //render bar
                    updateBar();
                    highlightMenu();
                    break;
                }
            case ("RotatePlane"):
                {
                    instructionText.text = "Press the TRIGGER button to ROTATE the Plane";
                    grabLeft.text = "";
                    grabRight.text = "";
                    triggerLeft.text = "Switch Plane Axis";
                    triggerRight.text = "Switch Plane Axis";
                    menuLeft.text = "";
                    menuRight.text = "";

                    leftWand.TrigEnable();
                    rightWand.TrigEnable();
                    leftWand.MenuDisable();
                    rightWand.MenuDisable();
                    //calculate Progress
                    if (leftWand.IsTriggering() && !holdingLeftTrigger)
                    {
                        holdingLeftTrigger = true;
                        progress += 0.25f;
                    }
                    else if (!leftWand.IsTriggering())
                    {
                        holdingLeftTrigger = false;
                    }
                    if (rightWand.IsTriggering() && !holdingRightTrigger)
                    {
                        holdingRightTrigger = true;
                        progress += 0.25f;
                    }
                    else if (!rightWand.IsTriggering())
                    {
                        holdingRightTrigger = false;
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        instructionText.text = "Good Job!";
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "SlidePlane";
                            progress = 0;
                            delay = delayTime;
                            prePos = plane.transform.position;
                            unHighlightTrig();
                            break;
                        }
                    }
                    //render bar
                    updateBar();
                    highlightTrig();
                    break;
                }
            case ("SlidePlane"):
                {
                    instructionText.text = "Hold the GRIP button to SLIDE the cutting plane";
                    triggerLeft.text = "Switch Plane Axis";
                    triggerRight.text = "Switch Plane Axis";
                    grabLeft.text = "Slide";
                    grabRight.text = "Slide";

                    leftWand.GripEnable();
                    rightWand.GripEnable();
                    //calculate Progress
                    if (plane.IsMovingPlane())
                    {
                        posDelta = plane.transform.position - prePos;
                        progress += posDelta.magnitude * Time.deltaTime * 25f;
                        prePos = plane.transform.position;
                    }
                    //go to next phase
                    if (progress >= 1)
                    {
                        progress = 1;
                        instructionText.text = "Almost Done!";
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                        {
                            phase = "Free";
                            progress = 0;
                            delay = delayTime;
                            unHighlightGrip();
                            break;
                        }
                    }
                    //render bar
                    updateBar();
                    highlightGrip();
                    break;
                }
            case ("Free"):
                {
                    instructionText.text = "Try out all you have learned!";
                    triggerLeft.text = "Switch Plane Axis";
                    triggerRight.text = "Switch Plane Axis";
                    grabLeft.text = "Slide";
                    grabRight.text = "Slide";
                    menuLeft.text = "Change Mode";
                    menuRight.text = "Change Mode";
                    leftWand.MenuEnable();
                    rightWand.MenuEnable();
                    //calculate Progress
                    if (!leftWand.IsControllingPlane())
                    {
                        phase = "Free2";
                    }
                    //render bar
                    destroyBar();
                    break;
                }
            case ("Free2"):
                {
                    instructionText.text = "Try out all you have learned!";
                    triggerLeft.text = "Rotate";
                    triggerRight.text = "Rotate";
                    grabLeft.text = "Move";
                    grabRight.text = "Move";
                    menuLeft.text = "Change Mode";
                    menuRight.text = "Change Mode";
                    if (leftWand.IsControllingPlane())
                    {
                        phase = "Free";
                    }
                    //render bar
                    destroyBar();
                    break;
                }
        }

    }
}
