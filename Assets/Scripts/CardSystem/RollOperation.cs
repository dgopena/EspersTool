using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using TMPro;
using Unity.Properties;
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
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private TextMeshProUGUI rollTypeLabel;
    
    [Header("Resizing")] 
    [SerializeField] private float screenWidthResizeFactor = 0.2f;
    [SerializeField] private float symbolsResizeFactor = 0.25f;
    
    [Header("Results List")] 
    [SerializeField] private GameObject resultEntryPrefab;
    private Transform resultEntriesParent;


    private void OnEnable()
    {
        //ResizeToScreen();
    }

    
    //useless for now
    private void ResizeToScreen()
    {
        RectTransform panelRT = cardBasePanel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor, panelRT.sizeDelta.y);

        panelRT = dicePanel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor, panelRT.sizeDelta.y);

        panelRT = buffResultPanel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor, panelRT.sizeDelta.y);

        panelRT = firstPlusSymbol.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

        panelRT = secondPlusSymbol.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

        panelRT = equalSymbol.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

        panelRT = totalResultLabel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(Screen.width * screenWidthResizeFactor, panelRT.sizeDelta.y);
    }

    public void SetStep(int stepIndex)
    {
        cardBasePanel.SetActive(stepIndex >= 0);
        
        firstPlusSymbol.SetActive(stepIndex > 0);
        dicePanel.SetActive(stepIndex > 0);
        
        secondPlusSymbol.SetActive(stepIndex > 1);
        buffResultPanel.SetActive(stepIndex > 1);
        
        equalSymbol.SetActive(stepIndex > 2);
        totalResultLabel.gameObject.SetActive(stepIndex > 2);
        acceptButton.SetActive(stepIndex > 2);
    }

    public void SetPanelState(int panelIndex, int panelState)
    {
        if (panelIndex == 0) //base panel
        {
            baseInputOptionPanel.SetActive(panelState == 0); //input mode
            baseResultOptionPanel.SetActive(panelState == 1);
        }
        else if (panelIndex == 1) //die panel
        {
            diceButtonPanel.SetActive(panelState == 0);
            diceResultsPanel.SetActive(panelState == 1);
        }
        else if (panelIndex == 2) //buff panel
        {
            buffResultPanel.SetActive(panelState >= 0);
        }
    }

    #region Base Entry Panel
    
    public void GiveBaseResult(int entry)
    {
        baseNumberInput.SetTextWithoutNotify(entry.ToString());
        baseNumberLabel.text = entry.ToString();
        
        SetPanelState(0,1);
        
        SetStep(1);
        
        SetPanelState(1,0);
    }

    public void GiveCard(FateCard entryCard)
    {
        Debug.Log(entryCard.cardSuit);
        
        string actionType = "Attack";
        if (entryCard.cardSuit == 1)
            actionType = "Dodge";
        else if (entryCard.cardSuit == 3)
            actionType = "Magic";
        else if (entryCard.cardSuit == 4)
            actionType = "Skill";

        rollTypeLabel.text = actionType;

        int valueEntry = 10;
        if (entryCard.cardNumber <= 9 && entryCard.cardNumber > 1)
            valueEntry = entryCard.cardNumber;
        else if (entryCard.cardNumber == 1)
            valueEntry = 11;
        
        GiveBaseResult(valueEntry);
    }

    public void StartWithoutCard(int actionIndex)
    {
        string actionType = "Attack";
        if (actionIndex == 1)
            actionType = "Dodge";
        else if (actionIndex == 3)
            actionType = "Magic";
        else if (actionIndex == 4)
            actionType = "Skill";
        
        rollTypeLabel.text = actionType;
        
        SetStep(0);
        SetPanelState(0,0);
    }
    
    public void GiveBaseFromInput()
    {
        int result = int.Parse(baseNumberInput.text);
        GiveBaseResult(result);
    }
    
    public void ToggleBaseInputEditMode()
    {
        if (baseInputOptionPanel.activeInHierarchy)
        {       
            baseNumberInput.SetTextWithoutNotify(baseNumberLabel.text);
            SetPanelState(0,1);
        }
        else
        {
            SetPanelState(0, 0);
        }
    }
    
    #endregion
}
