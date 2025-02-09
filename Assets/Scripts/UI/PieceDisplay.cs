using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PieceDisplay : MonoBehaviour
{
    [Header("UI Elements")] [SerializeField]
    private Image buttonBackPanel;

    [SerializeField] private Image statBackPanel;
    [SerializeField] private Image hpBackPanel;

    [SerializeField] private Image statPanelCover;
    [SerializeField] private Image hpVigorCover;
    [SerializeField] private Image backCover;

    [SerializeField] private GameObject gameModeTools;
    public GameObject unitPanelButton;

    [Space(5f)] [SerializeField] private PieceReticle reticle;

    [Space(10f)] [SerializeField] private TextMeshProUGUI nameLabel;

    [Space(10f)] 
    [SerializeField] private NotchBar hpBar;
    [SerializeField] private TextMeshProUGUI hpLabel;
    [SerializeField] private GameObject minusHPButton;
    [SerializeField] private GameObject plusHPButton;
    
    [Space(5f)] 
    [SerializeField] private float miniPanelHoldInitialCooldown = 1f;
    [SerializeField] private float miniPanelHoldConstantCooldown = 0.2f;
    private bool miniPanelHolding = false;
    private float currentHoldCooldown = 0f;

    [Header("Stat Panel")] 
    [SerializeField] private TextMeshProUGUI strStatLabel;
    [SerializeField] private TextMeshProUGUI intStatLabel;
    [SerializeField] private TextMeshProUGUI dexStatLabel;
    [SerializeField] private TextMeshProUGUI chaStatLabel;
    [SerializeField] private TextMeshProUGUI defStatLabel;

    [Header("Piece Tools")]
    [SerializeField] private RectTransform panelTools;
    [SerializeField] private GameObject cardModeButton;
    [SerializeField] private GameObject inputModeButton;
    [SerializeField] private GameObject itemsButton;
    [SerializeField] private GameObject detailsButton;
    [SerializeField] private GameObject editButton;
    [SerializeField] private GameObject deleteButton;

    [Header("Action Panel")] 
    [SerializeField] private GameObject charaActionsPanel;
    [SerializeField] private GameObject charaCardHandButton;
    [SerializeField] private GameObject foeActionsPanel;
    [SerializeField] private Image[] buttonGraphics;
    [SerializeField] private float grayingValue = 0.8f;

    [Header("Roll Operations Panel")]
    [SerializeField] private RollOperation rollOperationsPanel;

    private FateHandWidget activeHandWidget;
    
    [Header("Result List")] 
    [SerializeField] private Animator resultsListAnim;
    [SerializeField] private GameObject resultEntryPrefab;
    [SerializeField] private int maxResultEntries = 8;
    [SerializeField] private TextMeshProUGUI lastResultText;

    private bool rollListOpen = false;
    
    /*

[Space(10f)]
    [SerializeField] private TextMeshProUGUI secondButtonLabel;
    [SerializeField] private Color possitiveEffectColor;
    [SerializeField] private Color statusEffectColor;
    [SerializeField] private StatusList statusList;
    [SerializeField] private StatusList effectList;
*/

    //only for foes
    [Header("Ability Panel")]
    [SerializeField] private Animator abilityBlockAnim;
    [SerializeField] private RectTransform abilityBlockContent;
    [SerializeField] private RectTransform abilityEntryPrefab;
    [SerializeField] private float comboLeftMargin;
    [SerializeField] private RectTransform abilitDescPanel;
    [SerializeField] private TextMeshProUGUI abiltyPanelText;
    private float abilityListSpacing;

    private List<EntryDescPair> abilityEntries;
    private int currentAbilitySelectedIndex = -1;
    private bool abilityListIsShown = false;

    [Header("Data")]
    [SerializeField] private AbilityData abilityInfo;
    [SerializeField] private SkillsData skillsInfo;
    [SerializeField] private ItemsData itemsInfo;

    private struct EntryDescPair
    {
        public GameObject uiEntry;
        public string description;
    }

    private EsperUnit activeUnit;
    private UnitPiece activePiece;

    public UnitPiece pieceDisplayed => activePiece;

    private CharacterPiece activeCharaPiece;
    private FoePiece activeFoePiece;

    private bool refreshAbilityFlag = true;

    private void LateUpdate()
    {
        if (currentHoldCooldown > 0f)
            currentHoldCooldown -= Time.unscaledDeltaTime;
    }

    public void DisplayCharacterPiece(CharacterPiece character)
    {
        Debug.Log("received " + character.unitName);

        activeFoePiece = null;
        activeCharaPiece = character;

        activePiece = character;
        activeUnit = character.characterData;

        reticle.ChangeColor(activeUnit.colorChoice);
        reticle.targetObject = character.transform;

        BuildDisplay();
    }

    public void DisplayFoePiece(FoePiece foe)
    {
        Debug.Log("received " + foe.unitName);

        activeFoePiece = foe;
        activeCharaPiece = null;

        activePiece = foe;
        activeUnit = foe.foeData;

        reticle.ChangeColor(activeUnit.colorChoice);
        reticle.targetObject = foe.transform;

        BuildDisplay();
    }

    public EsperUnit GetActiveUnit()
    {
        return activeUnit;
    }
    
    #region Panel Tools
    
    public void SetPanelToolsMode(int md) // 0 - character, 1 - foe, 2 - edit
    {
        editButton.SetActive(md == 2);
        deleteButton.SetActive(md == 2);
        
        cardModeButton.SetActive(activeCharaPiece.usingCardMode && md == 0);
        inputModeButton.SetActive(!activeCharaPiece.usingCardMode && md == 0);
        
        itemsButton.SetActive(md == 0);
        
        detailsButton.SetActive(true);
    }

    public void SetCardMode(bool enabled)
    {
        if (!activeCharaPiece)
            return;
        
        activeCharaPiece.usingCardMode = enabled;
        
        charaActionsPanel.SetActive(!activeCharaPiece.usingCardMode);
        charaCardHandButton.SetActive(activeCharaPiece.usingCardMode);
    }

    public void ShowItems(bool enabled)
    {
        
    }

    public void ShowDetails(bool enabled)
    {
        
    }

    public void OpenEditPanel(bool enabled)
    {
        
    }

    public void DeletePieceCall()
    {
        
    }
    
    #endregion
    
    public void CloseDisplayPanel()
    {
        //CloseStatusEffectLists();
        unitPanelButton.gameObject.SetActive(true);
    }

    private void BuildDisplay()
    {
        unitPanelButton.gameObject.SetActive(false);
        UnitManager._instance.SetUnitMenu(false);

        buttonBackPanel.color = activeUnit.colorChoice;
        statBackPanel.color = activeUnit.colorChoice;
        hpBackPanel.color = activeUnit.colorChoice;

        Color buttonColor = grayingValue * activeUnit.colorChoice;
        buttonColor.a = 1.0f;
        
        for (int i = 0; i < buttonGraphics.Length; i++)
        {
            buttonGraphics[i].color = buttonColor;
        }
        
        nameLabel.text = activeUnit.unitName;

        int currentMaxHP = (activeUnit.baseHP + activeUnit.addedHP);
        if (activeCharaPiece)
        {
            currentMaxHP = Mathf.FloorToInt((activeUnit.baseHP + activeUnit.addedHP));
        }

        hpBar.SetBar(currentMaxHP);
        UpdateHealthBars(true);

        strStatLabel.text = activeUnit.statSTR.ToString();
        intStatLabel.text = activeUnit.statINT.ToString();
        dexStatLabel.text = activeUnit.statDEX.ToString();
        chaStatLabel.text = activeUnit.statCHA.ToString();
        defStatLabel.text = activeUnit.defense.ToString();
        
        //status and effects
        // statusList.ClearIcons();
        // effectList.ClearIcons();
        //
        // statusList.ignoreUpdateFlag = true;
        // effectList.ignoreUpdateFlag = true;
        //
        // for (int i = 0; i < activeUnit.activeStatus.Count; i++)
        // {
        //     statusList.AddStatus(activeUnit.activeStatus[i]);
        // }
        // for (int i = 0; i < activeUnit.activePositiveEffects.Count; i++)
        // {
        //     effectList.AddPositiveEffect(activeUnit.activePositiveEffects[i]);
        // }
        //
        // statusList.ignoreUpdateFlag = false;
        // effectList.ignoreUpdateFlag = false;

        //differing setup
        if (activeCharaPiece)
        {
            charaActionsPanel.SetActive(!activeCharaPiece.usingCardMode);
            charaCardHandButton.SetActive(activeCharaPiece.usingCardMode);
            foeActionsPanel.SetActive(false);

            FateHandWidget handWidget = activeCharaPiece.GetCardHandPanel();
            handWidget.OnCardPlayed.RemoveAllListeners();
            handWidget.OnCardPlayed.AddListener(OnCardPlay);
            handWidget.OnCloseCall.RemoveAllListeners();
            handWidget.OnCloseCall.AddListener(CloseCardHand);

            activeHandWidget = handWidget;
        }
        else if (activeFoePiece)
        {
            charaActionsPanel.SetActive(false);
            charaCardHandButton.SetActive(false);
            foeActionsPanel.SetActive(true);
            
            //build ability list
        }
        
        UpdateRollListPanel();
    }

    #region Health Bars
    
    public void ModifyHealth(int value)
    {
        miniPanelHolding = false;
        currentHoldCooldown = 0;
    }

    public void ModifyHealthHold(int value)
    {
        if (currentHoldCooldown > 0)
            return;

        if (!miniPanelHolding)
        {
            currentHoldCooldown = miniPanelHoldInitialCooldown;
            miniPanelHolding = true;
        }
        else
            currentHoldCooldown = miniPanelHoldConstantCooldown;

        if (activeCharaPiece != null)
            activeCharaPiece.ModifyHealth(value);
        else if (activeFoePiece != null)
            activeFoePiece.ModifyHealth(value);

        UpdateHealthBars();
    }

    private void UpdateHealthBars(bool force = false)
    {
        int currentMaxHP = GetUnitCurrentMaxHP();

        if (force)
            hpBar.ForceValue(activeUnit.currentHP);
        else
            hpBar.ApplyValue(activeUnit.currentHP);

        hpLabel.text = activeUnit.currentHP + "/" + currentMaxHP;
    }

    public int GetUnitCurrentMaxHP()
    {
        if (activeCharaPiece != null)
        {
            return (activeUnit.baseHP + activeUnit.addedHP);
        }
        else if(activeFoePiece != null)
        {
            return (activeUnit.baseHP + activeUnit.addedHP);
        }

        return activeUnit.baseHP;
    }

    #endregion
    
    /*
    public void UpdateStatusEffects()
    {
        activeUnit.SetFreshFlag(false);
        activeUnit.GiveStatusList(statusList.GetStatusList());
        activeUnit.GiveEffectList(effectList.GetEffectList());
    }

    public void CloseStatusEffectLists()
    {
        statusList.CallListClose();
        effectList.CallListClose();
    }
*/
    
    private void DisplayPanelCovers(bool show)
    {
        if (!show) // && (traitEffectListIsShown || abilityListIsShown))
            return;

        backCover.gameObject.SetActive(show);
        hpVigorCover.gameObject.SetActive(show);
        statPanelCover.gameObject.SetActive(show);

        minusHPButton.gameObject.SetActive(!show);
        plusHPButton.gameObject.SetActive(!show);

        gameModeTools.gameObject.SetActive(!show);

        /*
        unitPanelButton.gameObject.SetActive(!show);

        if (show)
        {
            UnitManager._instance.SetUnitMenu(false);
        }
        */
    }

    #region Action Calls

    public void ActionCall(int actionIndex) // 0 - Dodge, 1 - Attack, 2 - Magic, 3 - Skill
    {
        if (!activeCharaPiece)
            return;
        
        rollOperationsPanel.gameObject.SetActive(true);
        HotKeyManager._instance.EnableHotKeys(false);
        rollOperationsPanel.StartWithoutCard(actionIndex);
        
        activeCharaPiece.EnableCards(false);
    }

    #endregion
    
    #region Abilities
    
    public void AbilityButtonClick()
    {
        ShowAbilityList(!abilityListIsShown);
    }

    private void ShowAbilityList(bool show, bool animted = true)
    {
        if (!animted)
        {
            if (show)
            {
                if (refreshAbilityFlag)
                    BuildAbilityList();

                abilityBlockAnim.SetTrigger("JumpOpen");
            }
            else
                abilityBlockAnim.SetTrigger("JumpClose");
        }
        else
        {
            if (show)
            {
                if (refreshAbilityFlag)
                    BuildAbilityList();

                abilityBlockAnim.SetTrigger("Open");
            }
            else
                abilityBlockAnim.SetTrigger("Close");
        }

        if (!show && currentAbilitySelectedIndex >= 0)
        {
            abilitDescPanel.gameObject.SetActive(false);
            currentAbilitySelectedIndex = -1;
        }

        abilityListIsShown = show;

        DisplayPanelCovers(show);
    }

    private void BuildAbilityList()
    {
        //clean the list
        for (int i = abilityBlockContent.childCount - 1; i >= 1; i--)
        {
            Destroy(abilityBlockContent.GetChild(i).gameObject);
        }

        if (abilityEntries != null)
            abilityEntries.Clear();
        else
            abilityEntries = new List<EntryDescPair>();

        abilityEntryPrefab.gameObject.SetActive(false);

        List<ClassData.Ability> abs = activeUnit.GetAbilities();

        for (int i = 0; i < abs.Count; i++)
        {
            ClassData.Ability ab = abs[i];

            CreateAbilityEntry(ab);
        }

        currentAbilitySelectedIndex = -1;
        refreshAbilityFlag = false;
    }

    private GameObject CreateAbilityEntry(ClassData.Ability ability)
    {
        GameObject nuEntry = Instantiate<GameObject>(abilityEntryPrefab.gameObject, abilityBlockContent);
        nuEntry.transform.GetChild(0).GetChild(1).gameObject.SetActive(false); //frame
        nuEntry.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = ability.abilityName;

        //add action cost
        nuEntry.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.actionCost + " Action";

        //add combo brackets
        if (ability.abilityComboDepth > 0)
        {
            RectTransform entryRT = nuEntry.transform.GetChild(0).GetComponent<RectTransform>();
            entryRT.offsetMin = new Vector2(comboLeftMargin * ability.abilityComboDepth, entryRT.offsetMin.y);

            RectTransform bracketRT = nuEntry.transform.GetChild(1).GetComponent<RectTransform>();
            bracketRT.offsetMin = new Vector2(comboLeftMargin * (ability.abilityComboDepth - 1), entryRT.offsetMin.y);
            bracketRT.offsetMax = new Vector2(comboLeftMargin * (ability.abilityComboDepth - 1), abilityListSpacing);
            bracketRT.gameObject.SetActive(true);
        }

        //select entry behavior
        int descPairIndex = abilityEntries.Count;
        nuEntry.GetComponent<HoldButton>().onRelease.AddListener(delegate
        {
            EntryDescPair pair = abilityEntries[descPairIndex];
            if (currentAbilitySelectedIndex >= 0)
            {
                if (currentAbilitySelectedIndex == descPairIndex)
                {
                    //same choice. deactivate
                    pair.uiEntry.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    abilitDescPanel.gameObject.SetActive(false);
                    currentAbilitySelectedIndex = -1;
                }
                else
                {
                    //change target
                    abilityEntries[currentAbilitySelectedIndex].uiEntry.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    pair.uiEntry.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                    abiltyPanelText.text = pair.description;
                    currentAbilitySelectedIndex = descPairIndex;
                }
            }
            else
            {
                //open the panel and show description
                pair.uiEntry.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                abilitDescPanel.gameObject.SetActive(true);
                abiltyPanelText.text = pair.description;
                currentAbilitySelectedIndex = descPairIndex;
            }
        });

        //we build an appropiate description
        string description = "<i><b>" + ability.abilityName + "</i></b>\n\n";

        description += "<size=85%>";

        if (ability.abilityAspects != null)
        {
            for (int i = 0; i < ability.abilityAspects.Length; i++)
            {
                description += "<i>" + ability.abilityAspects[i] + "</i>";

                if (i == ability.abilityAspects.Length - 1)
                    description += "\n\n";
                else
                    description += " | ";
            }
        }

        description += "<size=100%>";

        string abDesc = ability.abilityEffect;
        abDesc = abDesc.Replace("\\n", "<br>");

        description = description + abDesc;

        EntryDescPair nuPair = new EntryDescPair() { uiEntry = nuEntry, description = description };
        abilityEntries.Add(nuPair);

        nuEntry.SetActive(true);

        return nuEntry;
    }

    public ClassData.Ability FindAbility(int docID, int ID)
    {
        // AbilityEntry.FoeAbility[] coll = itemsInfo[docID].abilities;
        //
        // for (int i = 0; i < coll.Length; i++)
        // {
        //     if (coll[i].abilityIDInList == ID)
        //     {
        //         ClassData.Ability ability = new ClassData.Ability();
        //         ability.abilityID = coll[i].abilityIDInList;
        //         ability.docID = docID;
        //         ability.abilityName = coll[i].abilityName;
        //         ability.abilityEffect = coll[i].effect;
        //         ability.abilityAspects = coll[i].additionals;
        //         ability.actionCost = coll[i].actionCost;
        //         ability.abilityComboDepth = 0;
        //         ability.subCombos = coll[i].subCombos;
        //
        //         return ability;
        //     }
        // }

        return new ClassData.Ability();
    }
    
    #endregion
    
    #region Card Stuff / Roll Operation
    
    public void EnableCardHand(bool enabled)
    {
        if(activeCharaPiece != null)
            activeCharaPiece.EnableCards(enabled);
    }

    public void CloseCardHand()
    {
        EnableCardHand(false);
    }
    
    public void OnCardPlay(FateCard playedCard)
    {
        if (!activeCharaPiece)
            return;
        
        rollOperationsPanel.gameObject.SetActive(true);
        HotKeyManager._instance.EnableHotKeys(false);
        rollOperationsPanel.GiveCard(playedCard);
        
        activeCharaPiece.EnableCards(false);
    }

    public void CloseRollOperator()
    {
        rollOperationsPanel.gameObject.SetActive(false);
        HotKeyManager._instance.EnableHotKeys(true);

        if (activeCharaPiece)
        {
            if (activeCharaPiece.usingCardMode)
            {
                activeCharaPiece.EnableCards(true);
            }
        }
    }

    public void GetResultFromRollOperator(string actionType, int result)
    {
        Debug.Log("received " + actionType + " roll of " + result);
        
        if(activeHandWidget)
            activeHandWidget.DiscardCard(); //we discard the played card
        
        //we add the result to the result list
        GiveNewRollEntry(actionType,result);
        
        CloseRollOperator();
    }
    
    #endregion
    
    #region Roll List
    
    private void GiveNewRollEntry(string actionType, int rollResult)
    {
        Tuple<string, int> entryTuple = new Tuple<string, int>(actionType, rollResult);
        activePiece.AddToHistory(entryTuple, maxResultEntries);
        
        UpdateRollListPanel();
    }

    private void UpdateRollListPanel()
    {
        Transform resultListParent = resultEntryPrefab.transform.parent;
        for (int i = resultListParent.childCount - 1; i >= 1; i--)
        {
            Destroy(resultListParent.GetChild(i).gameObject);
        }

        Tuple<string, int>[] rollList = activePiece.GetRollStringHistory();
        if (rollList == null)
            rollList = Array.Empty<Tuple<string, int>>();
        
        for (int i = 0; i < rollList.Length; i++)
        {
            GameObject nuRollEntry = Instantiate(resultEntryPrefab, resultListParent);
            Transform rollEntryTF = nuRollEntry.transform;
            rollEntryTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = rollList[i].Item1;
            rollEntryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = rollList[i].Item2.ToString();
            
            nuRollEntry.SetActive(true);
        }

        EnableRollListPanel(rollList.Length > 0);

        if (rollList.Length > 0)
        {
            SetRollListOpen(false, true);
            lastResultText.text = rollList[0].Item2.ToString();
        }
    }

    private void EnableRollListPanel(bool enabled)
    {
        resultsListAnim.gameObject.SetActive(enabled);
    }
    
    public void ToggleRollListOpen()
    {
        SetRollListOpen(!rollListOpen);
    }

    public void SetRollListOpen(bool open)
    {
        SetRollListOpen(open, false);
    }
    
    public void SetRollListOpen(bool open, bool skipAnim)
    {
        rollListOpen = open;

        if (!skipAnim)
        {
            resultsListAnim.SetTrigger(open ? "Show" : "Hide");
        }
        else
        {
            resultsListAnim.SetTrigger(open ? "ShowSkip" : "HideSkip");
        }
    }
    
    #endregion
}
