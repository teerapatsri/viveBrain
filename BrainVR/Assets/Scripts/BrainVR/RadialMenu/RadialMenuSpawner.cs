using UnityEngine;
using System.Collections;

public class RadialMenuSpawner : MonoBehaviour {

    public static RadialMenuSpawner Instance { get; private set; }
    public RadialMenu menuPrefab;

    private Vector3 offset = Vector3.up;

    void Awake()
    {
        // First we check if there are any other instances conflicting
        if (Instance != null && Instance != this)
        {
            // If that is the case, we destroy other instances
            Destroy(gameObject);
        }

        // Here we save our singleton instance
        Instance = this;

        // Furthermore we make sure that we don't destroy between scenes (this is optional)
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnMenu(WandController wand)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        newMenu.transform.SetParent(transform, false);
        newMenu.transform.position = wand.transform.position + offset;
    }
}
