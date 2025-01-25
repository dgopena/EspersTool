using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using TMPro;
using UnityEngine;

public class RollOperation : MonoBehaviour
{
    [SerializeField] private PieceDisplay pieceDisplay;
    
    [Header("UI")] 
    [SerializeField] private GameObject cardBasePanel;
    [SerializeField] private GameObject baseInputOptionPanel;
    [SerializeField] private GameObject baseResultOptionPanel;
    [SerializeField] private TMP_InputField baseNumberInput;
    [SerializeField] private TextMeshProUGUI baseNumberLabel;

    [Space(10f)] [SerializeField] private GameObject firstPlusSymbol;

    [Space(10f)] [SerializeField] private GameObject dicePanel;
    [SerializeField] private GameObject diceButtonPanel;
    [SerializeField] private GameObject diceResultsPanel;
    [SerializeField] private TextMeshProUGUI diceLabel;
    [SerializeField] private DieWidget diceWidget;
    
    [Space(10f)] [SerializeField] private GameObject secondPlusSymbol;
    
    [Space(10f)] 
    [SerializeField] private GameObject buffResultPanel;
    [SerializeField] private TextMeshProUGUI buffResultLabel;
    private EsperCharacter charaSource;
    private EsperFoe foeSource;
    private bool isEsperChara = true;
    
    [Space(10f)] [SerializeField] private GameObject equalSymbol;
    
    [Space(10f)] [SerializeField] private TextMeshProUGUI totalResultLabel;

    [Header("Results List")] 
    [SerializeField] private GameObject resultEntryPrefab;
    private Transform resultEntriesParent;
    
}
