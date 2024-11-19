using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class UnitPanel : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] protected TMP_InputField hpInput;
    [SerializeField] protected TextMeshProUGUI hpInputPlaceholder;
    [SerializeField] protected TMP_InputField sizeInput;
    [SerializeField] protected TextMeshProUGUI hpInputJustText;
    [SerializeField] protected TextMeshProUGUI sizeInputJustText;

    [Header("General")]
    [SerializeField] protected TextMeshProUGUI nameLabel;
    [SerializeField] protected TextMeshProUGUI levelLabel;
    [SerializeField] protected TextMeshProUGUI panelModeLabel;
    [SerializeField] protected GameObject applyToBaseButton;

    [Space(10f)]
    [SerializeField] protected TextMeshProUGUI defenseLabel;
    [SerializeField] protected TextMeshProUGUI speedLabel;
    [SerializeField] protected TextMeshProUGUI damageLabel;
    [SerializeField] protected TextMeshProUGUI frayDamageLabel;
    [SerializeField] protected TextMeshProUGUI attackLabel;
    [SerializeField] protected TextMeshProUGUI basicAttackLabel;

    [Space(10f)]
    [SerializeField] protected RectTransform traitContent;
    [SerializeField] protected TextMeshProUGUI traitLabel;
}
