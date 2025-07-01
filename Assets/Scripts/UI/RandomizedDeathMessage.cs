using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomizedDeathMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private string[] messages;

    private void OnEnable()
    {
        text.text = messages[Random.Range(0, messages.Length)];
    }
}
