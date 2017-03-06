using UnityEngine;
using System.Collections;

public class RadialMenu : MonoBehaviour{
    public RadialButton buttonPrefab;
    //public RadialButton selected;
    public int selected;
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
            newButton.sprite = wand.options[i].sprite;
            newButton.activeSprite = wand.options[i].activeSprite;
            newButton.title = wand.options[i].title;
            newButton.id = wand.options.Length - i - 1;
            newButton.myMenu = this;
        }
    }
    public void Close()
    {
        Destroy(gameObject);
    }
}
