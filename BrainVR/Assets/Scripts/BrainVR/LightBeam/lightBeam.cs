using UnityEngine;
using System.Collections;

public class lightBeam : MonoBehaviour
{
    [Header(" [Controlling Wand]")]
    [Tooltip("Controlling Wand")]
    public WandController wand;
    LineRenderer lineRend;

    [Header(" [Start and End position]")]
    public Transform startPos;
    public Transform cube;

    private float textureOffset = 0f;
    private bool on;
    private Transform endPos;

    // Use this for initialization
    void Start()
    {
        on = false;
        endPos = cube;
        lineRend = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //turn lightsaber off and on//
        if (wand != null)
        {
            on = wand.IsGripping();
        }
        endPos = cube;
        //extend the line//
        if (on)
        {
            endPos.localPosition = Vector3.zero;
            //update line positions//
            lineRend.SetPosition(0, startPos.position);
            lineRend.SetPosition(1, endPos.position);

            //pan texture//
            textureOffset -= Time.deltaTime * 2f;
            if (textureOffset < -10f)
            {
                textureOffset += 10f;
            }
            lineRend.sharedMaterials[1].SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));
        }
        else
        {
            lineRend.SetPosition(0, Vector3.zero);
            lineRend.SetPosition(1, Vector3.zero);
        }
    }
}
