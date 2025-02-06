using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using TMPro;
using Unity.Properties;
using UnityEngine;

public class RollOperation : MonoBehaviour
{
    [SerializeField] private PieceDisplay pieceDisplay;

    [Header("UI")] [SerializeField] private GameObject cardBasePanel;
    [SerializeField] private GameObject baseInputOptionPanel;
    [SerializeField] private GameObject baseResultOptionPanel;
    [SerializeField] private TMP_InputField baseNumberInput;
    [SerializeField] private TextMeshProUGUI baseNumberLabel;

    private int baseResult = 0;
    
    [Space(10f)] [SerializeField] private GameObject firstPlusSymbol;

    [Space(10f)] [SerializeField] private GameObject dicePanel;
    [SerializeField] private GameObject diceButtonPanel;
    [SerializeField] private GameObject diceResultsPanel;
    [SerializeField] private TextMeshProUGUI diceLabel;
    [SerializeField] private DieWidget diceWidget;

    [SerializeField] private GameObject[] hideWithDice;
    
    private int diceResult = 0;
    
    [Space(10f)] [SerializeField] private GameObject secondPlusSymbol;

    [Space(10f)] [SerializeField] private GameObject buffResultPanel;
    [SerializeField] private TextMeshProUGUI buffResultLabel;
    [SerializeField] private GameObject buffDetailPanel;
    [SerializeField] private GameObject buffDetailButton;
    [SerializeField] private Transform buffButtonArrowImage;
    [SerializeField] private GameObject buffDetailPrefab;
    
    private int buffResult = 0;
    
    private EsperCharacter charaSource;
    private EsperFoe foeSource;
    private bool isEsperChara = true;

    [Space(10f)] [SerializeField] private GameObject equalSymbol;

    [Space(10f)] [SerializeField] private TextMeshProUGUI totalResultLabel;
    [SerializeField] private GameObject acceptButton;
    [SerializeField] private TextMeshProUGUI rollTypeLabel;

    private int finalResult = 0;
    
    [Header("Resizing")] [SerializeField] private float screenWidthResizeFactor = 0.2f;
    [SerializeField] private float symbolsResizeFactor = 0.25f;

    [Header("Results List")] [SerializeField]
    private GameObject resultEntryPrefab;

    private Transform resultEntriesParent;

    private int currentRollType = 0;

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
        panelRT.sizeDelta =
            new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

        panelRT = secondPlusSymbol.GetComponent<RectTransform>();
        panelRT.sizeDelta =
            new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

        panelRT = equalSymbol.GetComponent<RectTransform>();
        panelRT.sizeDelta =
            new Vector2(Screen.width * screenWidthResizeFactor * symbolsResizeFactor, panelRT.sizeDelta.y);

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

        baseResult = entry;
        
        SetPanelState(0, 1);

        SetStep(1);

        SetPanelState(1, 0);
    }

    public void GiveCard(FateCard entryCard)
    {
        string actionType = "Attack";
        if (entryCard.cardSuit == 1)
            actionType = "Dodge";
        else if (entryCard.cardSuit == 3)
            actionType = "Magic";
        else if (entryCard.cardSuit == 4)
            actionType = "Skill";

        currentRollType = entryCard.cardSuit - 1;
        
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

        currentRollType = actionIndex - 1;
        
        rollTypeLabel.text = actionType;

        SetStep(0);
        SetPanelState(0, 0);
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
            SetPanelState(0, 1);
        }
        else
        {
            SetPanelState(0, 0);
        }
    }

    #endregion

    #region Dice

    public void OpenDiceWidget()
    {
        diceWidget.OnResultGet.RemoveAllListeners();
        diceWidget.OnResultGet.AddListener(ReceiveDiceResult);

        int startDie = 4;
        int statIndex = 0;
        bool firstRound = GameModeManager._instance.uiRoundCounter.GetRoundNumber() == 1;

        EsperUnit unit = pieceDisplay.GetActiveUnit();
        
        if (currentRollType == 0) //dodge - dex
        {
            startDie = unit.statDEX;
            statIndex = 2;
        }
        else if (currentRollType == 1) //attack - str
        {
            startDie = unit.statSTR;
            statIndex = 0;
        }
        else if (currentRollType == 2) //magic - int
        {
            startDie = unit.statINT;
            statIndex = 1;
        }
        else if (currentRollType == 3) //skill - cha
        {
            startDie = unit.statCHA;
            statIndex = 3;
        }

        bool adv = false;
        if (unit is EsperCharacter)
        {
            adv = (unit as EsperCharacter).HasAdvantage(statIndex, firstRound);
        }

        for (int i = 0; i < hideWithDice.Length; i++)
        {
            hideWithDice[i].SetActive(false);
        }
        
        diceWidget.SetWidget(false, startDie, adv);
        
        diceWidget.gameObject.SetActive(true);
        
    }

    public void ReceiveDiceResult(int dieResult)
    {
        diceResult = dieResult;
        
        for (int i = 0; i < hideWithDice.Length; i++)
        {
            hideWithDice[i].SetActive(true);
        }
        
        diceLabel.text = diceResult.ToString();
        diceWidget.gameObject.SetActive(false);
        
        SetPanelState(1, 1);
        
        SetStep(2);
        
        GetBuffs();
    }
    
    #endregion
    
    #region Buff Panel
    
    public void GetBuffs()
    {
        bool firstRound = GameModeManager._instance.uiRoundCounter.GetRoundNumber() == 1;
        
        EsperUnit unit = pieceDisplay.GetActiveUnit();
        if (unit is EsperCharacter)
        {
            int statIndex = 0;
            if (currentRollType == 0) //dodge - dex
            {
                statIndex = 2;
            }
            else if (currentRollType == 2) //magic - int
            {
                statIndex = 1;
            }
            else if (currentRollType == 3) //skill - cha
            {
                statIndex = 3;
            }

            Tuple<string, int>[] activeBuffs =
                (unit as EsperCharacter).GetBuffs(statIndex, baseResult, statIndex == 0, firstRound);

            buffDetailPanel.SetActive(false);
            for (int i = buffDetailPanel.transform.childCount - 1; i >= 1; i--)
            {
                Destroy(buffDetailPanel.transform.GetChild(i).gameObject);
            }
            
            if (activeBuffs.Length == 0)
            {
                buffDetailButton.SetActive(false);
                buffResultLabel.text = "0";
            }
            else
            {
                buffDetailButton.SetActive(true);
                buffButtonArrowImage.transform.rotation = Quaternion.identity;

                int addBuffs = 0;
                for (int i = 0; i < activeBuffs.Length; i++)
                {
                    GameObject nuBuffLabel = Instantiate<GameObject>(buffDetailPrefab, buffDetailPanel.transform);
                    Transform nuBuffLabelTransform = nuBuffLabel.transform;

                    nuBuffLabelTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text = activeBuffs[i].Item1;
                    
                    addBuffs+=activeBuffs[i].Item2;
                    
                    string numLabel = activeBuffs[i].Item2.ToString();
                    if (activeBuffs[i].Item2 > 0)
                        numLabel = "+" + numLabel;
                    nuBuffLabel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = numLabel;
                    
                    nuBuffLabel.gameObject.SetActive(true);
                }

                buffResult = addBuffs;
                
                string totalResult = addBuffs.ToString();
                if(addBuffs > 0)
                    totalResult = "+" + totalResult;

                buffResultLabel.text = totalResult;
            }
        }
        else if (unit is EsperFoe)
        {
            Debug.Log("TO_DO: Manage buffs for foes. The attack modifier");
        }
        
        DisplayResults();
    }

    public void ToggleBuffDetailPanel()
    {
        if (buffDetailPanel.activeInHierarchy)
        {
            buffDetailPanel.SetActive(false);
            buffButtonArrowImage.transform.rotation = Quaternion.identity;
        }
        else
        {
            buffDetailPanel.SetActive(true);
            buffButtonArrowImage.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
    }
    
    #endregion

    public void DisplayResults()
    {
        finalResult = baseResult + diceResult + buffResult;

        totalResultLabel.text = finalResult.ToString();
        
        SetStep(3);
    }

    public void AcceptResults()
    {
        string actionType = "Attack";
        if (currentRollType == 0)
            actionType = "Dodge";
        else if (currentRollType == 2)
            actionType = "Magic";
        else if (currentRollType == 3)
            actionType = "Skill";
        
        pieceDisplay.GetResultFromRollOperator(actionType, finalResult);
    }
}
