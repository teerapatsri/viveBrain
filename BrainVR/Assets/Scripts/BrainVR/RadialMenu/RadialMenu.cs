using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public RadialButton buttonPrefab;
    //public RadialButton selected;
    public Image icon;
    public GameController gameController;
    public Sprite exitTrans;
    public Sprite exitOpaq;
    public int selected;
    void Start()
    {
        icon.sprite = exitTrans;
    }
    public void SpawnButtons(WandController wand)
    {
        for (int i = 0; i < wand.options.Length; i++)
        {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            newButton.transform.SetParent(transform, false);
            float theta = (2 * Mathf.PI / wand.options.Length) * i;
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);
            newButton.transform.localPosition = new Vector3(xPos, yPos, 0f) * 0.1f;
            newButton.gameController = gameController;
            newButton.sprite = wand.options[i].sprite;
            newButton.activeSprite = wand.options[i].activeSprite;
            newButton.title = wand.options[i].title;
            newButton.id = wand.options.Length - i - 1;
            newButton.myMenu = this;
        }
    }
    public void ExitButtonTransparent()
    {
        if (exitTrans != null)
            icon.sprite = exitTrans;
    }
    public void ExitButtonOpaque()
    {
        if (exitOpaq != null)
            icon.sprite = exitOpaq;
    }
    public void Close()
    {
        Destroy(gameObject);
    }
}
