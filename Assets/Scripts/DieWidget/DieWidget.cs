using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DieWidget : MonoBehaviour
{
    [SerializeField] private bool startInInputScreen = false;

    [Header("Dice Models")]
    [SerializeField] private Transform dieSlot0;
    [SerializeField] private Transform dieSlot1;
    [SerializeField] private Transform dieSlot2;

    [Space(10f)]
    [SerializeField] private float rollSpeed = 100f;

    [Header("Dice UI")]
    [SerializeField] private GameObject dieScreen;
    [SerializeField] private TextMeshProUGUI dieSlotLabel0;
    [SerializeField] private TextMeshProUGUI dieSlotLabel1;
    [SerializeField] private TextMeshProUGUI dieSlotLabel2;

    [Space(10f)]
    [SerializeField] private GameObject dieButtonPanel;
    [SerializeField] private GameObject[] dieButtons;
    [SerializeField] private Color selectedColor = Color.gray;

    [Space(10f)]
    [SerializeField] private Toggle advantageCheck;

    [Header("Direct Input")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputScreen;

    public void Throw()
    {

    }

    public void SetAdvantage(bool isAdvantage)
    {

    }

    public void ChangeDie(int index)
    {

    }

    public void ToggleDirectInput()
    {

    }

    public void ShowDirectOptions(bool show)
    {

    }

    public void AcceptInput()
    {

    }
}
