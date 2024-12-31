using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class FoeMakerPanel : MonoBehaviour
{
    [SerializeField] private FoePresets foePresets;
    
    private EsperFoe activeFoe;

    private RectTransform listRT;

    private bool editMode;

    [Header("General Panel")]
    [SerializeField] private RectTransform makerPanel;

    [SerializeField] private GameObject generalAspectsPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI colorLabel;
    [SerializeField] private Image colorImage;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI pageLabel;
    private int currentPage = 0;

    [SerializeField] private CanvasGroup forwardCharPageButton;
    [SerializeField] private CanvasGroup backCharPageButton;

    [SerializeField] private GameObject backToPresetListButton;
    
    private ColorListPanel colorListPanel;
    private bool colorListOpen = false;

    [Space(10f)]
    [SerializeField] private ListPanel listPanel;
    [SerializeField] public Vector2 slimListPanelProportions;
    [SerializeField] public Vector2 wideListPanelProportions;

    [Header("Presets Page")]
    [SerializeField] private RectTransform presetsPage;
    [SerializeField] private GameObject monsterPresetEntryPrefab;

    private Dictionary<Transform, int> presetIDEntryDict;

    [Header("Description and Abilities")]
    [SerializeField] private RectTransform descriptionAndAbilitiesPage;
    [SerializeField] private TMP_InputField foeDescriptionInput;

    [SerializeField] private RectTransform addAbilityButton;
    [SerializeField] private GameObject abilityEntryPrefab;
    [SerializeField] private TextMeshProUGUI abilityDescription;
    private bool abilityListOpen = false;
    
    private Dictionary<int, Transform> listEntryAbilityIDDict;
    private Dictionary<int, int> abilityListIndexIDDict;
    
    private int currentSelectedAbility = -1;
    
    [Header("Stats")]
    [SerializeField] private RectTransform statPage;
    [SerializeField] private TextMeshProUGUI strenghtStatDieLabel;
    [SerializeField] private TextMeshProUGUI intelligenceStatDieLabel;
    [SerializeField] private TextMeshProUGUI dexterityStatDieLabel;
    [SerializeField] private TextMeshProUGUI charismaStatDieLabel;
    [SerializeField] private TMP_InputField hpInput;
    [SerializeField] private TMP_InputField defInput;
    [SerializeField] private TMP_InputField atkModInput;
    [SerializeField] private TextMeshProUGUI atkModResultLabel;
    [SerializeField] private Image atkModErrorFrame;
    private bool statListOpen = false;
    private int currentStatIndex = 0;

    [Header("Piece")]
    [SerializeField] private RectTransform pieceLookPage;
    
    private void LateUpdate()
    {
        bool aListOpen = colorListOpen || abilityListOpen || statListOpen;

        if (aListOpen)
        {
            if (colorListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    colorListOpen = false;
                    ColorManager._instance.HideGeneralColorPanel();
                    ColorManager._instance.generalColorList.OnEntryClick -= ColorListClick;
                }
            }
            else if (statListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    statListOpen = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= SetDieStat;
                }
            }
            else if (abilityListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    abilityListOpen = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= AddAbilityToFoe;
                }
            }
        }
    }

    public void StartPanel(EsperFoe targetFoe, bool editMode)
    {
        activeFoe = targetFoe;
        this.editMode = editMode;
        
        if (!editMode)
        {
            generalAspectsPanel.SetActive(false);
            
            BuildPresetsList();
            SetPanelPage(-1);
        }
        else
        {
            nameInputField.SetTextWithoutNotify(activeFoe.unitName);
            colorLabel.text = ColorManager._instance.GetColorName(activeFoe.colorChoice);
            colorImage.color = activeFoe.colorChoice;
            
            SetUpFoePages();
            SetPanelPage(0);
            backToPresetListButton.SetActive(false);
        }


    }

    #region General Section

    public void SetPanelPage(int pageIndex)
    {
        if (pageIndex < 0)
        {
            backCharPageButton.alpha = 0f;
            forwardCharPageButton.alpha = 0f;

            presetsPage.gameObject.SetActive(true);
            
            descriptionAndAbilitiesPage.gameObject.SetActive(false);
            statPage.gameObject.SetActive(false);
            pieceLookPage.gameObject.SetActive(false);

            pageLabel.text = "";
            return;
        }

        currentPage = pageIndex;
        backCharPageButton.alpha = currentPage == 0 ? 0.2f : 1f;
        forwardCharPageButton.alpha = currentPage == 3 ? 0.2f : 1f;

        descriptionAndAbilitiesPage.gameObject.SetActive(currentPage == 0);
        statPage.gameObject.SetActive(currentPage == 1);
        pieceLookPage.gameObject.SetActive(currentPage == 2);

        if (currentPage == 2)
            UnitManager._instance.UpdateWorkFoe(activeFoe);
        
        pageLabel.text = (currentPage + 1) + "/3";
    }

    public void PageForward(int moveDir)
    {
        currentPage = Mathf.Clamp(currentPage + moveDir, 0, 2);
        SetPanelPage(currentPage);
    }

    public void UpdateFoeName()
    {
        activeFoe.unitName = nameInputField.text;
    }

    public void OpenColorOptions()
    {
        colorListPanel = ColorManager._instance.generalColorList;
        colorListPanel.screenProportionSize = slimListPanelProportions;
        colorListPanel.listColor = 0.9f * generalAspectsPanel.GetComponent<Image>().color;

        RectTransform colorButtonRT = colorLabel.transform.parent.GetComponent<RectTransform>();

        Vector3 listOrigin = colorButtonRT.position + (-2f * colorButtonRT.rect.size.x * colorButtonRT.lossyScale.x * Vector3.right);

        ColorManager._instance.ShowGeneralColorPanel(listOrigin);
        statListOpen = false;
        abilityListOpen = false;
        colorListOpen = true;
        colorListPanel.OnEntryClick += ColorListClick;

        listRT = colorListPanel.GetComponent<RectTransform>();
    }

    public void ColorListClick(int index)
    {
        activeFoe.colorChoice = ColorManager._instance.colors[index].color;
        ColorManager._instance.HideGeneralColorPanel();
        ColorManager._instance.generalColorList.OnEntryClick -= ColorListClick;
        colorListOpen = false;

        colorLabel.text = ColorManager._instance.colors[index].name;
        colorImage.color = ColorManager._instance.colors[index].color;
    }

    #endregion

    #region Preset List Methods
    
    private void BuildPresetsList()
    {
        //presets page
        Transform contentParent = monsterPresetEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        if (presetIDEntryDict == null)
            presetIDEntryDict = new Dictionary<Transform, int>();
        else
            presetIDEntryDict.Clear();

        for (int i = 0; i < foePresets.presets.Count; i++)
        {
            AddNewPresetUIEntry(i);
        }
    }
    
    public void BackToPresets()
    {
        presetsPage.gameObject.SetActive(true);
        SetPanelPage(-1);
        backToPresetListButton.SetActive(false);
    }

    public void GoFromPresets()
    {
        presetsPage.gameObject.SetActive(false);
        SetPanelPage(0);
        backToPresetListButton.SetActive(true);
    }

    private EsperFoe BuildFoeFromPreset(int presetID = -1)
    {
        EsperFoe newFoe = new EsperFoe();
        newFoe.SetupNewFoe();
        
        if (presetID >= 0)
        {
            FoePresets.FoePreset presetLoaded = foePresets.presets[presetID];
            newFoe.unitName = presetLoaded.presetName;
            newFoe.description = presetLoaded.foeDescription;
            newFoe.statSTR = presetLoaded.STRStat;
            newFoe.statINT = presetLoaded.INTStat;
            newFoe.statDEX = presetLoaded.DEXStat;
            newFoe.statCHA = presetLoaded.CHAStat;

            newFoe.abilityIDs = presetLoaded.abilityIds;
            if (newFoe.GiveATKModString(presetLoaded.ATKMod))
            {
                atkModResultLabel.text = newFoe.GetATKMod().ToString();
            }
            else
            {
                Debug.Log("ATKMod was not properly parse for foe " + newFoe.unitName);
                atkModResultLabel.text = "0";
            }
        }
        
        return newFoe;
    }

    public void SetUpFromPreset(int presetID = -1)
    {
        activeFoe = BuildFoeFromPreset(presetID);
        
        SetUpFoePages();
        
        GoFromPresets();
    }
    
    public void SetUpFoePages()
    {
        //general
        nameInputField.SetTextWithoutNotify(activeFoe.unitName);
        colorLabel.text = ColorManager._instance.GetColorName(activeFoe.colorChoice);
        colorImage.color = activeFoe.colorChoice;
        
        generalAspectsPanel.SetActive(true);
        
        //description and abilities
        Transform contentParent = abilityEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        if(listEntryAbilityIDDict == null)
            listEntryAbilityIDDict = new Dictionary<int, Transform>();
        else
            listEntryAbilityIDDict.Clear();
        
        for (int i = 0; i < activeFoe.abilityIDs.Length; i++)
        {
            AddNewAbilityUIEntry(activeFoe.abilityIDs[i]);
        }

        currentSelectedAbility = -1;
        
        abilityDescription.text = "<i>Press a skill to see its description</i>";
        
        foeDescriptionInput.SetTextWithoutNotify(activeFoe.description);
        
        //stats page
        strenghtStatDieLabel.text = activeFoe.statSTR.ToString();
        intelligenceStatDieLabel.text = activeFoe.statINT.ToString();
        dexterityStatDieLabel.text = activeFoe.statDEX.ToString();
        charismaStatDieLabel.text = activeFoe.statCHA.ToString();

        hpInput.SetTextWithoutNotify(activeFoe.GetTotalHP().ToString());
        
        defInput.SetTextWithoutNotify(activeFoe.defense.ToString());
        atkModInput.SetTextWithoutNotify(activeFoe.GetATKModString());

        atkModErrorFrame.gameObject.SetActive(false);
        
        //piece screen
        
        UpdatePiecePage();
    }

    private void AddNewPresetUIEntry(int presetIndex)
    {
        Transform contentParent = monsterPresetEntryPrefab.transform.parent;

        GameObject nuEntry = Instantiate<GameObject>(monsterPresetEntryPrefab, contentParent);
        Transform entryTF = nuEntry.transform;
        
        FoePresets.FoePreset preset = foePresets.presets[presetIndex];
        entryTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = preset.presetName;
        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = preset.foeDescription;

        presetIDEntryDict.Add(entryTF, presetIndex);
        
        int presetIdx = presetIndex;
        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate { SetUpFromPreset(presetIdx);});

        nuEntry.SetActive(true);
    }

    #endregion

    #region Foe Ability Methods

    public void OpenFoeAbilityList()
    {
        listPanel.screenProportionSize = slimListPanelProportions;
        listPanel.listColor = 0.9f * makerPanel.transform.GetChild(0).GetComponent<Image>().color;
        
        if(abilityListIndexIDDict == null)
            abilityListIndexIDDict = new Dictionary<int, int>();
        else
            abilityListIndexIDDict.Clear();
        
        RectTransform abilityButtonRT = addAbilityButton;
        Vector3 listOrigin = abilityButtonRT.position + (0.5f * abilityButtonRT.rect.size.x * abilityButtonRT.lossyScale.x * Vector3.right);
        List<string> abilityEntries = new List<string>();

        List<int> abilityIDs = new List<int>(activeFoe.abilityIDs);
        
        for (int a = 0; a < foePresets.abilites.Count; a++)
        {
            if (abilityIDs.Contains(a))
                continue;
            
            abilityListIndexIDDict.Add(abilityEntries.Count, a);
            abilityEntries.Add(foePresets.abilites[a].abilityName);
        }

        if (abilityEntries.Count == 0)
            return;

        listPanel.ShowPanel(listOrigin, abilityEntries, true);
        statListOpen = false;
        abilityListOpen = true;
        colorListOpen = false;
        listPanel.OnEntryClick += AddAbilityToFoe;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void AddAbilityToFoe(int abilityIndexInList)
    {
        int[] currentAbilities = activeFoe.abilityIDs;

        List<int> newAbilities = new List<int>(currentAbilities);

        int abilityID = abilityListIndexIDDict[abilityIndexInList];
        if (!newAbilities.Contains(abilityID))
        {
            newAbilities.Add(abilityID);
            activeFoe.abilityIDs = newAbilities.ToArray();

            AddNewAbilityUIEntry(abilityID);
        }

        abilityListOpen = false;
        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= AddAbilityToFoe;
    }

    private void AddNewAbilityUIEntry(int abilityID)
    {
        Transform contentParent = abilityEntryPrefab.transform.parent;

        GameObject nuEntry = Instantiate<GameObject>(abilityEntryPrefab, contentParent);
        Transform entryTF = nuEntry.transform;

        FoePresets.FoeAbility ability = foePresets.abilites[abilityID];
        entryTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = ability.abilityName;
        entryTF.GetChild(2).gameObject.SetActive(false);

        int abilityIndex = abilityID;
        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate { SelectAbility(abilityIndex); });

        listEntryAbilityIDDict.Add(abilityID, entryTF);

        entryTF.GetChild(1).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteAbility(abilityIndex); });

        nuEntry.SetActive(true);
    }

    public void SelectAbility(int abilityID)
    {
        Transform entry = listEntryAbilityIDDict[abilityID];

        if  (currentSelectedAbility >= 0)
        {
            if (currentSelectedAbility == abilityID)
            {
                entry.GetChild(2).gameObject.SetActive(false);
                currentSelectedAbility = -1;
                abilityDescription.text = "<i>Press a skill to see its description</i>";
                return;
            }
            else
            {
                listEntryAbilityIDDict[currentSelectedAbility].GetChild(2).gameObject.SetActive(false);
            }
        }

        FoePresets.FoeAbility ability = foePresets.abilites[abilityID];
        string desc = ability.abilityDescription;
        abilityDescription.text = desc;
        
        entry.GetChild(2).gameObject.SetActive(true);

        currentSelectedAbility = abilityID;
    }

    public void DeleteAbility(int abilityID)
    {
        Debug.Log(abilityID);

        if (!listEntryAbilityIDDict.ContainsKey(abilityID))
            return;

        int[] activeFoeAbilityIDs = activeFoe.abilityIDs;

        List<int> abilityList = new List<int>();

        //delete art and related level
        for (int i = 0; i < activeFoeAbilityIDs.Length; i++)
        {
            if (activeFoeAbilityIDs[i] != abilityID)
            {
                abilityList.Add(activeFoeAbilityIDs[i]);
            }
        }

        Transform entry = listEntryAbilityIDDict[abilityID];

        if (currentSelectedAbility == abilityID)
        {
            currentSelectedAbility = -1;
            abilityDescription.text = "<i>Press a skill to see its description</i>";
        }

        Destroy(entry.gameObject);
        listEntryAbilityIDDict.Remove(abilityID);

        Debug.Log("Removed ability " + foePresets.abilites[abilityID].abilityName);

        activeFoe.abilityIDs = abilityList.ToArray();
    }

    public void UpdateFoeDescription()
    {
        activeFoe.description = foeDescriptionInput.text;
    }
    
    #endregion

    #region Stat Methods

    public void OpenStatList(int statIndex)
    {
        listPanel.screenProportionSize = slimListPanelProportions;
        listPanel.listColor = 0.9f * makerPanel.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform statButtonRT = strenghtStatDieLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = statButtonRT.position + (0.5f * statButtonRT.rect.size.x * statButtonRT.lossyScale.x * Vector3.right);
        List<string> statEntries = new List<string>();

        statEntries.Add("4");
        statEntries.Add("6");
        statEntries.Add("8");
        statEntries.Add("10");
        statEntries.Add("12");
        statEntries.Add("20");

        currentStatIndex = statIndex;

        listPanel.ShowPanel(listOrigin, statEntries, true);
        statListOpen = true;
        abilityListOpen = false;
        colorListOpen = false;
        listPanel.OnEntryClick += SetDieStat;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void SetDieStat(int index)
    {
        int chosenStat = 4;
        if (index == 1)
            chosenStat = 6;
        else if (index == 2)
            chosenStat = 8;
        else if (index == 3)
            chosenStat = 10;
        else if (index == 4)
            chosenStat = 12;
        else if (index == 5)
            chosenStat = 20;

        if (currentStatIndex == 0)
        {
            activeFoe.statSTR = chosenStat;
            strenghtStatDieLabel.text = chosenStat.ToString();
        }
        else if (currentStatIndex == 1)
        {
            activeFoe.statINT = chosenStat;
            intelligenceStatDieLabel.text = chosenStat.ToString();
        }
        else if (currentStatIndex == 2)
        {
            activeFoe.statDEX = chosenStat;
            dexterityStatDieLabel.text = chosenStat.ToString();
        }
        else if (currentStatIndex == 3)
        {
            activeFoe.statCHA = chosenStat;
            charismaStatDieLabel.text = chosenStat.ToString();
        }

        UpdateAttackMod();
        
        statListOpen = false;
        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= SetDieStat;
    }

    public void SetHPFromInput()
    {
        if (int.TryParse(hpInput.text, out int newHP))
        {
            activeFoe.GiveAddedHP(newHP - activeFoe.baseHP);
        }
    }

    public void SetDefense()
    {
        if(int.TryParse(defInput.text, out int newDef))
        {
            activeFoe.GiveDefense(newDef);
        }
    }

    public void ParseAttackMod()
    {
        string newMod = atkModInput.text;
        
        //try to parse it and get a correct reading. if not, show it as red
        if (activeFoe.GiveATKModString(newMod))
        {
            atkModResultLabel.text = activeFoe.GetATKMod().ToString();
            atkModErrorFrame.gameObject.SetActive(false);
        }
        else
        {
            atkModResultLabel.text = "0";
            atkModErrorFrame.gameObject.SetActive(true);
        }
    }

    public void UpdateAttackMod()
    {
        atkModResultLabel.text = activeFoe.GetATKMod().ToString();
    }
    
    #endregion
    
    #region Piece Page

    public void GiveGraphicIDToPiece(string graphicID)
    {
        activeFoe.graphicImageID = graphicID;
    }

    public void UpdatePiecePage()
    {
        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerConfig(activeFoe, true);

        //GraphicPieceEditor.Instance.SetDisplayModel(activeFoe);
    }

    public void ExitAndSaveFoe()
    {
        UnitManager._instance.SaveFoe(activeFoe, !editMode);
    }

    #endregion
}
