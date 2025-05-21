using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance;
    public int evidenceAmount = 0;
    public TextMeshProUGUI evidenceUI;

    void Awake()
    {
        Instance = this; // Singleton pattern
    }

    public void AddEvidence()
    {
        evidenceAmount++;
        evidenceUI.text = "evidence " + evidenceAmount + "/5";
    }
}
