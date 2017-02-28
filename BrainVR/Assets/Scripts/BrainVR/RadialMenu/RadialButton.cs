using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialButton : MonoBehaviour {
    public Image icon;
    
    public Sprite sprite, activeSprite;
    public RadialMenu myMenu;
    public string title;
    public int id;
    public GameController gameController;
    public Text buttonText;
    private Vector3 scale;

    void Start()
    {
        buttonText = GetComponentInChildren<Text>();
        if (icon != null)
        {
            scale = icon.transform.localScale;
        }
        if (buttonText != null)
        {
            buttonText.text = title;
        }
    }

    void Update()
    {
        if (buttonText != null && gameController != null)
        {
            switch (title)
            {
                case ("SwitchPlane"):
                    {
                        buttonText.text = "Plane axis: " + gameController.PlaneAxis;
                        break;
                    }
                case ("TwoSide"):
                    {
                        buttonText.text = gameController.TwoSideText;
                        break;
                    }
                case ("Shader"):
                    {
                        buttonText.text = "Shader : " + gameController.ShaderNumber;
                        break;
                    }
            }
        }
        if (myMenu!=null&&myMenu.selected == id)
        {
            icon.sprite = activeSprite;
            icon.transform.localScale = scale * 1.3f;
            //do something when selected e.g. BIGGER/ CHANGE COLOR
        }
        else if (sprite!=null)
        {
            icon.sprite = sprite;
            icon.transform.localScale = scale;
        }
    }
}
