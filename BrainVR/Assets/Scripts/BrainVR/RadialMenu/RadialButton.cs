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
    private Vector3 scale;

    void Start()
    {
        scale = icon.transform.localScale;
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
