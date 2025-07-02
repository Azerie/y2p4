using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class keyAppearScript : MonoBehaviour
{
    public GameObject lastKey;
    public GameObject keyAppear;
    public string triggeringTag = "Player";
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag(triggeringTag))
        {

            lastKey.transform.position = new Vector3(keyAppear.transform.position.x, keyAppear.transform.position.y, keyAppear.transform.position.z);
        }
    }
}

