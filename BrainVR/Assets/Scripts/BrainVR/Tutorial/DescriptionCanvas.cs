using UnityEngine;
using System.Collections;

public class DescriptionCanvas : MonoBehaviour
{
    [Header(" [Player and Object Reference]")]
    public GameObject player;
    public Interactable cube;

    private float offset = 0.8f;
    // Update is called once per frame
    void Update()
    {
        transform.position = cube.transform.position + offset * Vector3.up;
        transform.rotation = Quaternion.LookRotation(cube.transform.position - player.transform.position);
    }
}
