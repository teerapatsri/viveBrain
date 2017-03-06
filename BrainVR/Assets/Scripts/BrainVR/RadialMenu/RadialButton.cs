using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialButton : MonoBehaviour {
    public Image icon;
    public Sprite sprite;
    public Sprite activeSprite;
    public string title;
    public int id;
    public RadialMenu myMenu;
    private Text buttonText;
    private Vector3 scale;

    void Start()
    {
        buttonText = GetComponentInChildren<Text>();
        scale = icon.transform.localScale;
        if(buttonText!=null)
            buttonText.text = title;
    }

    public void Update()
    {
        if (myMenu.selected == id)
        {
            icon.sprite = activeSprite;
            icon.transform.localScale = scale * 1.3f;
            //do something when selected e.g. BIGGER/ CHANGE COLOR
        }
        else
        {
            icon.sprite = sprite;
            icon.transform.localScale = scale;
        }
    }
}
