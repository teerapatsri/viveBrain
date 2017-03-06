using UnityEngine;
using System.Collections;

public class RadialMenuSpawner : MonoBehaviour
{//Spawn the menu
    private Vector3 offset = new Vector3(0f, -0.05f, -0.03f);
    public static RadialMenuSpawner ins;
    public RadialMenu menuPrefab;

    void Awake()
    {
        ins = this;
    }
    public RadialMenu SpawnMenu(WandController wand, GameController gc)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        newMenu.transform.SetParent(transform, false);
        newMenu.transform.localPosition = transform.localPosition + offset;
        newMenu.gameController = gc;
        newMenu.SpawnButtons(wand);
        return newMenu;
    }
    public void CloseMenu(RadialMenu menuSpawned)
    {
        menuSpawned.Close();
    }
}
