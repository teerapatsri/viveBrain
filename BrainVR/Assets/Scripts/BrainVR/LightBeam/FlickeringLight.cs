using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    public float FlickerSpeed;

    [Header(" [Controlling Wand]")]
    [Tooltip("Controlling Wand")]
    public WandController wand;

    private Light lightObject;
    private float intensity;
    private bool inc, on;
    // Use this for initialization
    void Start()
    {
        inc = true;
        intensity = 0.9f;
        lightObject = GetComponent<Light>();
        lightObject.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (wand != null)
        {
            on = wand.IsGripDown();
            lightObject.enabled = on;
        }
        if (intensity > 1.3f || intensity < 0.9f)
        {
            inc = !inc;
        }
        if (inc)
        {
            intensity += FlickerSpeed * Time.fixedDeltaTime;
        }
        else
        {
            intensity -= FlickerSpeed * Time.fixedDeltaTime;
        }
            lightObject.intensity = intensity;
    }
}
