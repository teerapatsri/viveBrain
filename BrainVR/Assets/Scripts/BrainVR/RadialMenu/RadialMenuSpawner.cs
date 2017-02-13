using UnityEngine;
using System.Collections;

public class RadialMenuSpawner : MonoBehaviour
{//Spawn the menu
    private Vector3 offset = new Vector3(0f, 0f, 0f);
    public static RadialMenuSpawner ins;
    public RadialMenu menuPrefab;

    void Awake()
    {
        ins = this;
    }
    public RadialMenu SpawnMenu(WandController wand)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        newMenu.transform.SetParent(transform, false);
        newMenu.transform.position = transform.position;
        newMenu.SpawnButtons(wand);
        return newMenu;
    }
    public void CloseMenu(RadialMenu menuSpawned)
    {
        menuSpawned.Close();
    }
}
