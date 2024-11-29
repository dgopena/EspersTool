using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

using static ClassData;
using System;
using System.Reflection;

public class UnitManager : MonoBehaviour
{
    public static UnitManager _instance;

    [Header("Narrative")]
    public BondsData bonds;
    public CultureData cultures;

    [Header("Tactical")]
    public ClassData classes;
    public FoeData foes;
    public FoeData summons;
    public TemplateData templates;
    public FactionData[] factions;

    [Space(10f)]
    public TabbedContent unitListings;
    public Color unitHeavyColoring = Color.red;
    public Color unitVagabondColoring = Color.yellow;
    public Color unitLeaderColoring = Color.green;
    public Color unitArtilleryColoring = Color.blue;
    public Color unitLegendColoring = Color.white;
    public Color unitMobColor = Color.gray;
    public Color unitSummonColor = Color.magenta;

    [Header("UI")]
    public RectTransform canvasRT;
    public Animator unitMenuAnim;
    public Image slideButtonIcon;
    public Sprite leftArrowIcon;
    public Sprite rightArrowIcon;
    public bool unitMenuActive { get; private set; }
    public GameObject optionButton;
    public GameObject generalInputs;

    public RectTransform foeDescriptionsPanel;
    public RectTransform foeDescriptionsRT;
    public TextMeshProUGUI foeDescriptionsLabel;

    [Space(10f)]
    public ListPanel listPanel;
    public Vector2 kinCulturePanelProportions;
    public Vector2 bondClassJobPanelProportions;

    [Space(5f)]
    public Image charaSortButton;
    public Image foeSortButton;
    public Sprite sortAZSprite;
    public Sprite sortTimeSprite;

    private ColorListPanel colorListPanel;

    private enum UnitEditMode
    {
        None,
        CharacterNew,
        FoeNew,
        CharacterEdit,
        FoeEdit
    }

    private UnitEditMode currentMode;

    private List<IconCharacter> characterUnits;

    private List<IconFoe> foeUnits;

    #region Character UI
    [Space(20f)]
    [Header("------------Character UI------------")]
    public RectTransform charaListParent;
    public GameObject charaListEntryPrefab;
    public GameObject charaDeleteConfirmationPanel;
    private int unitIDToDelete;

    [Space(10f)]
    public GameObject characterEntries;
    public TextMeshProUGUI characterPageLabel;
    private int currentCharacterPage = 0;
    public CanvasGroup forwardCharPageButton;
    public CanvasGroup backCharPageButton;

    [Space(10f)]
    public TMP_InputField charaNameInput;
    public TextMeshProUGUI charaLevelLabel;
    public TextMeshProUGUI charaColorLabel;
    public Image charaColorImage;
    public GameObject charaEditAcceptButton;

    [Space(10f)]
    public TextMeshProUGUI charaKinLabel;
    public TextMeshProUGUI charaCultureLabel;
    public TextMeshProUGUI charaCultureData;
    [Space(10f)]
    public TextMeshProUGUI charaBondLabel;
    public TextMeshProUGUI charaBondActionModifier;
    public TextMeshProUGUI charaBondStartButtonLabel;
    public TextMeshProUGUI charaIdealsLabel;
    public TextMeshProUGUI[] charaStressLabels;
    [Space(10f)]
    public DotBar[] actionDots;
    [Space(10f)]
    public TextMeshProUGUI charaClassLabel;
    public TextMeshProUGUI charaClassTraitLabel;
    public TextMeshProUGUI charaClassStatLabel;
    [Space(10f)]
    public TextMeshProUGUI charaJobLabel;
    public TextMeshProUGUI charaJobTraitLabel;
    public TextMeshProUGUI finishCharaButtonLabel;
    public GameObject warningCharaNamePanel;
    [Space(10f)]
    public GameObject charaDetailPanel;
    public TextMeshProUGUI charaDetailNameLabel;
    public TextMeshProUGUI charaDetailGeneralLabel;
    public TextMeshProUGUI charaDetailNarrativeLabel;
    public TextMeshProUGUI charaDetailTacticalLabel;
    [Space(10f)]
    public TextMeshProUGUI charaHeadPiecePartLabel;
    public TextMeshProUGUI charaLWeaponPiecePartLabel;
    public TextMeshProUGUI charaRWeaponPiecePartLabel;

    private IconCharacter workCharacter;
    private int currentChosenLevel;
    private bool kinListOpen = false;
    private bool cultureListOpen = false;
    private bool bondListOpen = false;
    private bool startActionListOpen = false;
    private bool colorListOpen = false;
    private bool dotChangeFlag = false;

    private bool classListOpen = false;
    private bool jobListOpen = false;
    private RectTransform listRT;
    #endregion

    #region Foes UI
    [Space(20f)]
    [Header("------------Foes UI------------")]
    public RectTransform foeListParent;
    public GameObject foeListEntryPrefab;
    public GameObject foeDeleteConfirmationPanel;
    private int foeIdToDelete;

    [Space(10f)]
    [Header("General Panel")]
    public GameObject foeEntryScreen;
    public GameObject landingFoePages;
    public GameObject presetsFoePages;
    public Animator fromScratchAnim;
    public GameObject generalFoePages;
    public GameObject miniFoesSet;
    public TMP_InputField foeNameInput;
    public TextMeshProUGUI foeTypeLabel;
    public GameObject foeChapterInput;
    public Image foeColor;
    public TextMeshProUGUI foeColorLabel;
    private string foeEntryName;
    public GameObject foeEditAcceptButton;

    [Space(10f)]
    public PageStepper foeMakingPageStepper;
    public PageStepper mobMakingPageStepper;
    private bool[] pageChangedFlags;

    [Header("Base Class Page")]
    public TextMeshProUGUI foeChapter;
    public TextMeshProUGUI foeBaseClassLabel;
    public RectTransform foeClassStatsContent;
    public TextMeshProUGUI foeClassStatsText;
    private bool foeClassListOpen;

    [Header("Job / Faction Page")]
    public TextMeshProUGUI foeJobFactionChoiceLabel;
    public TextMeshProUGUI foeJobFactionSubChoiceLabel;
    public RectTransform foeJobFactionContent;
    public TextMeshProUGUI foeJobFactionChoiceText;
    private bool choiceWasClassJob = true;
    private bool foeJobFactionChoiceOpen;
    private bool foeJobFactionListOpen;

    private bool foeTemplateSubPageChosen = false;
    [Header("Template Sub Page")]
    public GameObject foeTemplateSet;
    public TextMeshProUGUI foeTemplateButtonLabel;
    public RectTransform foeTemplateContent;
    public TextMeshProUGUI foeTemplateText;
    private bool foeTemplateListOpen;
    private bool foeSubTemplateListOpen;
    public TextMeshProUGUI templatePageSubTemplateButtonLabel;

    [Header("Faction Sub Page")]
    public GameObject foeFactionSet;
    public TextMeshProUGUI foeJobUniqueChoiceLabel;
    public TextMeshProUGUI foeJobUniqueSubChoiceLabel;
    public RectTransform foeJobUniqueContent;
    public TextMeshProUGUI foeJobUniqueChoiceText;
    private bool choiceWasFactionJob = true;
    private bool foeJobUniqueChoiceOpen;
    private bool foeJobUniqueListOpen;
    public TextMeshProUGUI factionPageSubTemplateButtonLabel;

    [Header("Details Sub Page")]
    public TextMeshProUGUI foeOverallDetailsLabel;
    public TextMeshProUGUI foeOverallStatsLabel;
    public RectTransform foeOverallTraitsActionsContent;
    public TextMeshProUGUI foeOverallTraitsActionsText;

    [Header("Foe Piece Sub Page")]
    public TextMeshProUGUI finishFoeButtonLabel;

    public TextMeshProUGUI foeHeadPiecePartLabel;
    public TextMeshProUGUI foeLWeaponPiecePartLabel;
    public TextMeshProUGUI foeRWeaponPiecePartLabel;

    [Header("Mob/Summon Details")]
    public TextMeshProUGUI mobSummonButtonLabel;
    public TextMeshProUGUI mobSummonOverallStatsLabel;
    public RectTransform mobSummonOverallTraitsActionsContent;
    public TextMeshProUGUI mobSummonOverallTraitsActionsText;
    private bool mobSummonListOpen;

    [Header("Mob Piece Sub Page")]
    public TextMeshProUGUI finishMobButtonLabel;

    public TextMeshProUGUI mobHeadPiecePartLabel;
    public TextMeshProUGUI mobLWeaponPiecePartLabel;
    public TextMeshProUGUI mobRWeaponPiecePartLabel;

    [Space(10f)]
    public RectTransform foeTypeButton;
    private int currentFoePage;
    private int currentFoeType; //index for the foe ui entries to use

    private bool presetPageActive = false;

    private IconFoe workFoe;
    private int currentChosenChapter;
    public GameObject warningFoeNamePanel;

    [Space(10f)]
    public GameObject foeDetailPanel;
    public TextMeshProUGUI foeDetailNameLabel;
    public TextMeshProUGUI foeDetailGeneralLabel;
    public TextMeshProUGUI foeDetailTacticalLabel;

    private bool foeTypeListOpen = false;
    private bool templateListOpen = false;

    [Space(10f)]

    private bool foeAttackListOpen = false;
    private Color baseAttackButtonColor;
    #endregion

    [Space(30f)]
    public OptionsManager optionsManager;

    //piece spawning
    private bool withEntryGrabbed = false;
    private RectTransform entryGrabbed;
    private RectTransform entryGrabbedParent;
    private Vector3 entryGrabbedOriginalPosition;
    private Vector3 entryGrabDelta;
    private int entrySiblingIndex;

    private void Awake()
    {
        StartManager();
    }

    private bool managerStarted = false;

    public void StartManager()
    {
        if (managerStarted)
            return;

        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        currentMode = UnitEditMode.None;

        for (int i = 0; i < actionDots.Length; i++)
        {
            actionDots[i].SetBarID(i);
            actionDots[i].BarUpdate += DotBarUpdate;
        }

        //LoadCharactersCall();
        //LoadFoesCall();

        managerStarted = true;
    }

    private void LateUpdate()
    {
        if (currentMode == UnitEditMode.CharacterNew || currentMode == UnitEditMode.CharacterEdit)
        {
            bool aListOpen = kinListOpen || cultureListOpen || bondListOpen || startActionListOpen || classListOpen || jobListOpen || colorListOpen;

            if (aListOpen)
            {
                if (kinListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        kinListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if (cultureListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        cultureListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if (bondListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        bondListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if(startActionListOpen && Input.GetMouseButton(0))
                {
                    if(!TooltipManager.CheckMouseInArea(listRT))
                    {
                        startActionListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if (classListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        classListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if (jobListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        jobListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= CharaListClick;
                    }
                }
                else if (colorListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        colorListOpen = false;
                        ColorManager._instance.HideGeneralColorPanel();
                        ColorManager._instance.generalColorList.OnEntryClick -= CharaListClick;
                    }
                }
            }

            if(Input.GetMouseButtonDown(0) && charaDetailPanel.activeSelf)
            {
                if (!TooltipManager.CheckMouseInArea(charaListParent))
                {
                    charaDetailPanel.SetActive(false);
                    slideButtonIcon.transform.parent.gameObject.SetActive(true);
                }
            }
        }
        else if (currentMode == UnitEditMode.FoeNew || currentMode == UnitEditMode.FoeEdit)
        {
            bool aListOpen = colorListOpen || classListOpen || mobSummonListOpen || foeJobFactionChoiceOpen || foeJobFactionListOpen || foeTemplateListOpen || foeSubTemplateListOpen || foeJobUniqueChoiceOpen || foeJobUniqueListOpen;

            if (aListOpen)
            {
                if (colorListOpen && Input.GetMouseButtonDown(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        colorListOpen = false;
                        ColorManager._instance.HideGeneralColorPanel();
                        ColorManager._instance.generalColorList.OnEntryClick -= FoeListClick;
                    }
                }
                else if(classListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        classListOpen = false; 
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if(mobSummonListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        mobSummonListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if (foeJobFactionChoiceOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeJobFactionChoiceOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if (foeJobFactionListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeJobFactionListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if (foeTemplateListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeTemplateListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if(foeSubTemplateListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeSubTemplateListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if (foeJobUniqueChoiceOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeJobUniqueChoiceOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
                else if (foeJobUniqueListOpen && Input.GetMouseButton(0))
                {
                    if (!TooltipManager.CheckMouseInArea(listRT))
                    {
                        foeJobUniqueListOpen = false;
                        listPanel.ShowPanel(false);
                        listPanel.OnEntryClick -= FoeListClick;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && foeDetailPanel.activeSelf)
            {
                if (!TooltipManager.CheckMouseInArea(foeListParent))
                {
                    foeDetailPanel.SetActive(false);
                    slideButtonIcon.transform.parent.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (foeTypeListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    foeTypeListOpen = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= FoeListClick;
                }
            }
        }

        if (withEntryGrabbed)
        {
            entryGrabbed.position = Input.mousePosition - entryGrabDelta;
        }
    }

    public void ToggleUnitMenu()
    {
        SetUnitMenu(!unitMenuActive);
    }

    public void SetUnitMenu(bool active)
    {
        if (!unitMenuActive && active)
        {
            unitMenuAnim.SetTrigger("Show");
            slideButtonIcon.sprite = rightArrowIcon;

            if (ColorManager._instance.colorMenuActive)
                ColorManager._instance.ToggleColorMenu();
        }
        else if(unitMenuActive && !active)
        {
            unitMenuAnim.SetTrigger("Hide");
            slideButtonIcon.sprite = leftArrowIcon;
        }

        unitMenuActive = active;
    }

    public void ShowEntryScreen(int entryIndex)
    {
        characterEntries.SetActive(entryIndex == 0);
        if (characterEntries.activeSelf)
        {
            StartCharacterMaking();
            return;
        }

        foeEntryScreen.SetActive(true);
        StartFoeMaking(entryIndex);

        SetScrollTipRectsActive(false);

        generalInputs.SetActive(false);
        optionButton.SetActive(false);
    }

    public bool IsUnitEditing()
    {
        return currentMode != UnitEditMode.None;
    }

    public void CloseEntryScreen()
    {
        characterEntries.SetActive(false);

        foeEntryScreen.SetActive(false);

        currentMode = UnitEditMode.None;

        presetPageActive = false;

        MapManager._instance.ToggleHUD(true);

        SetScrollTipRectsActive(true);

        generalInputs.SetActive(true);
        optionButton.SetActive(true);

        MapManager._instance.menuManager.CheckMenuUIRearrange();
    }

    public void OpenColorList()
    {
        colorListPanel = ColorManager._instance.generalColorList;
        colorListPanel.screenProportionSize = kinCulturePanelProportions;

        if (currentMode == UnitEditMode.CharacterNew || currentMode == UnitEditMode.CharacterEdit)
            colorListPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;
        else if (currentMode == UnitEditMode.FoeNew || currentMode == UnitEditMode.FoeEdit)
            colorListPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform colorButtonRT = charaColorLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = colorButtonRT.position + (0.5f * colorButtonRT.rect.size.x * colorButtonRT.lossyScale.x * Vector3.right);

        ColorManager._instance.ShowGeneralColorPanel(listOrigin);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        jobListOpen = false;
        colorListOpen = true;
        if (currentMode == UnitEditMode.CharacterNew || currentMode == UnitEditMode.CharacterEdit)
            colorListPanel.OnEntryClick += CharaListClick;
        else if (currentMode == UnitEditMode.FoeNew || currentMode == UnitEditMode.FoeEdit)
            colorListPanel.OnEntryClick += FoeListClick;

        listRT = colorListPanel.GetComponent<RectTransform>();
    }

    public void SetScrollTipRectsActive(bool value)
    {
        unitListings.SetContentRectActive(value);
    }

    #region character management

    //save / load
    public void SaveCharacter()
    {
        if (workCharacter == null)
            return;

        /*
        //we must check if the name is not blank nor has underscores
        string charaName = workCharacter.unitName;
        bool namePass = OptionsManager.CheckValidFileName(charaName);
        namePass = namePass && !charaName.Contains("_");

        */
        if (!CheckIfCharacterNamed())
            return;

        //after pass
        int id = workCharacter.unitID;
        workCharacter.GiveID(id);

        if (currentMode == UnitEditMode.CharacterNew)
            workCharacter.SetFreshFlag(true);

        workCharacter = workCharacter.MakeCopy();

        int[] pieceIds = PieceCamera._instance.GetCurrentSamplePartIDs();

        workCharacter.lastModified = DateTime.Now;

        if (currentMode == UnitEditMode.CharacterNew)
        {
            /*
            id = PlayerPrefs.GetInt("NextIDToGive");
            PlayerPrefs.SetInt("NextIDToGive", id + 1);
            workCharacter.GiveID(id);
            */
            characterUnits.Add(workCharacter);
            UpIDCount(id);
        }
        else if(currentMode == UnitEditMode.CharacterEdit)
        {
            UpdateCharacter(workCharacter.unitID, workCharacter);
        }

        optionsManager.SaveCharacter(workCharacter);
        UpdateCharaList();

        CloseEntryScreen();
        currentMode = UnitEditMode.None;
    }

    //plug method for the graphic piece editor
    public void SaveCharacter(string graphicPieceID)
    {
        if (workCharacter == null)
            return;

        workCharacter.GiveGraphicPieceID(graphicPieceID);

        SaveCharacter();
    }

    public void SaveMeepleCharacter()
    {
        if (workCharacter == null)
            return;

        workCharacter.GiveGraphicPieceID(""); //overrides the previously set graphic piece ID

        SaveCharacter();
    }

    private void LoadCharactersCall()
    {
        optionsManager.LoadAllCharacters();
    }

    public void ReceiveLoadedCharacters(List<IconCharacter> charas)
    {
        if (charas == null || charas.Count == 0)
            characterUnits = new List<IconCharacter>();
        else
            characterUnits = charas;

        UpdateCharaList();

        if (characterUnits.Count == 0)
        {
            Debug.Log("No characters found.");
            return;
        }
    }

    //tab list and panel
    private void UpdateCharaList()
    {
        float entrySep = -20f;
        float entryHeight = -100f;

        for(int i = charaListParent.childCount - 1; i >= 1; i--)
        {
            Destroy(charaListParent.GetChild(i).gameObject);
        }

        bool alphaSorting = PlayerPrefs.GetInt("charaListSort", 0) == 0;

        List<IconCharacter> sortedUnits = new List<IconCharacter>();
        sortedUnits = characterUnits;
        if (alphaSorting)
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByName);
            charaSortButton.sprite = sortAZSprite;
        }
        else
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByNewerDate);
            charaSortButton.sprite = sortTimeSprite;
        }

        float posY = -20f;
        int childCount = 0;
        for(int i = 0; i < sortedUnits.Count; i++)
        {
            GameObject charaListEntry = Instantiate<GameObject>(charaListEntryPrefab, charaListParent);
            RectTransform entryListRT = charaListEntry.GetComponent<RectTransform>();
            childCount++;

            entryListRT.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedUnits[i].unitName;
            int charaID = sortedUnits[i].unitID;
            int charaEntryChildIndex = childCount;

            entryListRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = "DEPREC.";

            entryListRT.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                generalInputs.SetActive(false);
                optionButton.SetActive(false);
                StartCharacterEditing(charaID);
            });
            entryListRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                DeleteCharaCall(charaID);
            });
            Color identifierColor = unitHeavyColoring;
            if (sortedUnits[i].classIndex == 1)
                identifierColor = unitVagabondColoring;
            else if (sortedUnits[i].classIndex == 2)
                identifierColor = unitLeaderColoring;
            else if (sortedUnits[i].classIndex == 3)
                identifierColor = unitArtilleryColoring;

            entryListRT.GetChild(4).GetComponent<Image>().color = identifierColor;

            ShapeIcon entryPointer = entryListRT.GetComponent<ShapeIcon>();
            entryPointer.OnPointerEnterEvent.AddListener(delegate
            {
                ShowCharaDetails(charaID);
            });
            entryPointer.OnPointerExitEvent.AddListener(delegate
            {
                if (!TooltipManager.CheckMouseInArea(entryListRT))
                {
                    charaDetailPanel.SetActive(false);
                    slideButtonIcon.transform.parent.gameObject.SetActive(true);
                }
            });
            entryPointer.OnPointerDownEvent.AddListener(delegate
            {
                CharacterPieceEntryPress(charaID, charaEntryChildIndex);
            });
            entryPointer.OnPointerUpEvent.AddListener(delegate
            {
                CharacterPieceEntryRelease(charaID, charaEntryChildIndex);
            });

            Vector2 aPos = new Vector2(0f, posY);
            entryListRT.anchoredPosition = aPos;
            charaListEntry.SetActive(true);

            posY += entrySep + entryHeight;
        }

        Vector2 sd = charaListParent.sizeDelta;
        sd.y = -1f * posY;
        charaListParent.sizeDelta = sd;
    }

    public void ToggleCharaListSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("charaListSort", 0) == 0;

        if (alphaSorting)
            PlayerPrefs.SetInt("charaListSort", 1);
        else
            PlayerPrefs.SetInt("charaListSort", 0);

        UpdateCharaList();
    }

    public void ShowCharaDetails(int charaID)
    {
        for(int i = 0; i < characterUnits.Count; i++)
        {
            if(characterUnits[i].unitID == charaID)
            {
                IconCharacter charaInfo = characterUnits[i];
                charaDetailPanel.GetComponent<Image>().color = charaInfo.colorChoice;
                charaDetailNameLabel.text = charaInfo.unitName;
                charaDetailGeneralLabel.text = "DEPREC.";
                charaDetailNarrativeLabel.text = "DEPREC.";
                charaDetailTacticalLabel.text = "DEPREC.";

                charaDetailPanel.SetActive(true);
            }
        }

        slideButtonIcon.transform.parent.gameObject.SetActive(false);
    }

    //entries and variable functions
    private void StartCharacterMaking()
    {
        MapManager._instance.ToggleHUD(false);

        currentCharacterPage = 1;

        workCharacter = MakeNewCharacter();
        workCharacter.GiveID(RequestNextUnitID()); // (PlayerPrefs.GetInt("NextIDToGive"));
        charaNameInput.text = "";
        charaNameInput.ForceLabelUpdate();
        charaLevelLabel.text = "Lvl 1";
        currentChosenLevel = 1;

        charaEditAcceptButton.gameObject.SetActive(false);

        charaColorLabel.text = "White";
        charaColorImage.color = Color.white;
        workCharacter.colorChoice = Color.white;

        SetCharacterPage(currentCharacterPage);

        currentMode = UnitEditMode.CharacterNew;

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerMeepleConfig(0, 0, 0, 0);

        charaHeadPiecePartLabel.text = "DEPREC.";
        charaLWeaponPiecePartLabel.text = "DEPREC.";
        charaRWeaponPiecePartLabel.text = "DEPREC.";

        SetScrollTipRectsActive(false);
    }

    public void StartCharacterEditing(int charaID)
    {
        characterEntries.SetActive(true);

        MapManager._instance.ToggleHUD(false);
        
        currentCharacterPage = 1;

        workCharacter = GetCharacter(charaID); // characterUnits[charaID];
        charaNameInput.text = workCharacter.unitName;
        charaNameInput.ForceLabelUpdate();
        charaLevelLabel.text = "Lvl " + workCharacter.level;
        currentChosenLevel = workCharacter.level;

        charaEditAcceptButton.gameObject.SetActive(true);

        string colorName = ColorManager._instance.GetColorName(workCharacter.colorChoice);
        if (colorName.Length == 0)
            colorName = "White";
        charaColorLabel.text = colorName;
        charaColorImage.color = workCharacter.colorChoice;
        if (colorName.Length == 0)
        {
            charaColorImage.color = Color.white;
            workCharacter.colorChoice = Color.white;
        }

        SetCharacterPage(currentCharacterPage);

        currentMode = UnitEditMode.CharacterEdit;

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerConfig(workCharacter, true);

        charaHeadPiecePartLabel.text = "DEPREC.";
        charaLWeaponPiecePartLabel.text = "DEPREC.";
        charaRWeaponPiecePartLabel.text = "DEPREC.";

        SetScrollTipRectsActive(false);
    }

    public void DeleteCharaCall(int charaID)
    {
        unitIDToDelete = charaID;

        for (int i = 0; i < characterUnits.Count; i++)
        {
            if (characterUnits[i].unitID == charaID)
            {
                charaDeleteConfirmationPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Are you sure you want to delete the character \"" + characterUnits[i].unitName + "\" ?";
                charaDeleteConfirmationPanel.SetActive(true);
                break;
            }
        }
    }

    public void DeleteCharacter()
    {
        //also kill the file

        optionsManager.DeleteCharacter(unitIDToDelete);
        LoadCharactersCall();

        PieceManager._instance.ErasePiecesWithID(unitIDToDelete);

        charaDeleteConfirmationPanel.SetActive(false);

        MapManager._instance.menuManager.CheckMenuUIRearrange(); //rearranges the UI in the case we're using unitmanager functions and screens from the menu screen
    }

    public void UpdateCharacterName()
    {
        if (charaNameInput.text.Length < 1)
            return;

        workCharacter.unitName = charaNameInput.text;
    }

    public void ChangeCharacterLevel(bool forward)
    {
        currentChosenLevel += forward ? 1 : -1;

        currentChosenLevel = Mathf.Clamp(currentChosenLevel, 0, 12);

        charaLevelLabel.text = "Lvl " + currentChosenLevel;

        workCharacter.level = currentChosenLevel;

        UpdateTacticalPage(0);
    }

    public void OpenKinList()
    {
        listPanel.screenProportionSize = kinCulturePanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform kinButtonRT = charaKinLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = kinButtonRT.position + (0.5f * kinButtonRT.rect.size.x * kinButtonRT.lossyScale.x * Vector3.right);
        List<string> kinTypes = new List<string>();
        for(int i = 0; i < System.Enum.GetValues(typeof(Kin)).Length; i++)
        {
            kinTypes.Add(((Kin)i).ToString());
        }

        listPanel.ShowPanel(listOrigin, kinTypes, true);
        kinListOpen = true;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;
        jobListOpen = false;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenCultureList()
    {
        listPanel.screenProportionSize = kinCulturePanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform cultureButtonRT = charaCultureLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = cultureButtonRT.position + (0.5f * cultureButtonRT.rect.size.x * cultureButtonRT.lossyScale.x * Vector3.right);
        List<string> cultureTypes = new List<string>();
        for (int i = 0; i < cultures.cultures.Count; i++)
        {
            cultureTypes.Add(cultures.cultures[i].name);
        }

        listPanel.ShowPanel(listOrigin, cultureTypes, true);
        kinListOpen = false;
        cultureListOpen = true;
        bondListOpen = false;
        startActionListOpen = false;
        classListOpen = false;
        colorListOpen = false;
        jobListOpen = false;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenBondList()
    {
        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform bondButtonRT = charaBondLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = bondButtonRT.position + (0.5f * bondButtonRT.rect.size.x * bondButtonRT.lossyScale.x * Vector3.right);
        List<string> bondTypes = new List<string>();
        for (int i = 0; i < bonds.bonds.Count; i++)
        {
            bondTypes.Add(bonds.bonds[i].name);
        }

        listPanel.ShowPanel(listOrigin, bondTypes, true);
        kinListOpen = false;
        bondListOpen = true;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;
        jobListOpen = false;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenStartActionList()
    {
        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform bondButtonRT = charaBondStartButtonLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = bondButtonRT.position + (0.5f * bondButtonRT.rect.size.x * bondButtonRT.lossyScale.x * Vector3.right);
        List<string> startBondTypes = new List<string>();

        listPanel.ShowPanel(listOrigin, startBondTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = true;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;
        jobListOpen = false;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenClassList()
    {
        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform classButtonRT = charaClassLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = classButtonRT.position + (0.5f * classButtonRT.rect.size.x*classButtonRT.lossyScale.x * Vector3.right);
        List<string> classTypes = new List<string>();
        for (int i = 0; i < classes.classes.Count; i++)
        {
            classTypes.Add(classes.classes[i].name);
        }

        listPanel.ShowPanel(listOrigin, classTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = true;
        colorListOpen = false;
        jobListOpen = false;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenJobList()
    {
        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * characterEntries.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform jobButtonRT = charaJobLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = jobButtonRT.position + (0.5f * jobButtonRT.rect.size.x*jobButtonRT.lossyScale.x * Vector3.right);
        List<string> jobTypes = new List<string>();
        
        for (int i = 0; i < classes.classes[workCharacter.classIndex].jobs.Count; i++)
        {
            jobTypes.Add(classes.classes[workCharacter.classIndex].jobs[i].name);
        }

        listPanel.ShowPanel(listOrigin, jobTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;
        jobListOpen = true;
        listPanel.OnEntryClick += CharaListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void CharaListClick(int index)
    {
        if (kinListOpen)
        {
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            kinListOpen = false;
        }
        else if (cultureListOpen)
        {
            dotChangeFlag = true;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            cultureListOpen = false;
        }
        else if (bondListOpen)
        {
            dotChangeFlag = true;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            bondListOpen = false;
        }
        else if (startActionListOpen)
        {
            dotChangeFlag = true;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            startActionListOpen = false;
        }
        else if (classListOpen)
        {
            workCharacter.classIndex = index;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            classListOpen = false;
            UpdateTacticalPage(0);
        }
        else if (jobListOpen)
        {
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= CharaListClick;
            jobListOpen = false;
            UpdateTacticalPage(1);
        }
        else if (colorListOpen)
        {
            workCharacter.colorChoice = ColorManager._instance.colors[index].color;
            ColorManager._instance.HideGeneralColorPanel();
            ColorManager._instance.generalColorList.OnEntryClick -= CharaListClick;
            colorListOpen = false;

            charaColorLabel.text = ColorManager._instance.colors[index].name;
            charaColorImage.color = ColorManager._instance.colors[index].color;
        }
    }

    public void ChangeCharacterPage(bool forward)
    {
        currentCharacterPage += forward ? 1 : -1;
        currentCharacterPage = Mathf.Clamp(currentCharacterPage, 1, characterEntries.transform.childCount - 1);

        SetCharacterPage(currentCharacterPage);

        if (currentCharacterPage < 4) 
        {

        }
        else
            UpdateTacticalPage(currentCharacterPage - 4);
    }

    private void SetCharacterPage(int pageNumber)
    {
        characterPageLabel.text = currentCharacterPage + " / " + (characterEntries.transform.childCount - 1);

        for (int i = 1; i < characterEntries.transform.childCount; i++)
        {
            characterEntries.transform.GetChild(i).gameObject.SetActive(i == currentCharacterPage);
        }
        
        backCharPageButton.alpha = (currentCharacterPage > 1) ? 1f : 0.3f;
        forwardCharPageButton.alpha = (currentCharacterPage < (characterEntries.transform.childCount - 1)) ? 1f : 0.3f;
    }

    public void UpdateTacticalPage(int index)
    {
        //update tactical pages
        if (workCharacter == null)
            return;


        if (index == 0)
        {
            charaClassLabel.text = classes.classes[workCharacter.classIndex].name;
            string classTraits = "<b>Class Traits</b>";
            for (int i = 0; i < classes.classes[workCharacter.classIndex].classTraits.Length; i++)
            {
                classTraits += "\n\n<b>�" + classes.classes[workCharacter.classIndex].classTraits[i].traitName;

                string auxText = classes.classes[workCharacter.classIndex].classTraits[i].traitDescription;
                string traitDescription = "";

                for (int c = 0; c < auxText.Length; c++)
                {
                    if (((int)auxText[c] == 8226) || ((int)auxText[c] == 183))
                    {
                        string sub = auxText.Substring(0, c) + "\n ";
                        auxText = auxText.Substring(c);
                        auxText = sub + auxText;
                        c += 3;
                    }
                }

                traitDescription = auxText;
                classTraits += ":</b> <i>" + traitDescription + "</i>";
            }
            charaClassTraitLabel.text = classTraits;

            int chapterNum = 1; // Mathf.CeilToInt((float)workCharacter.level / 4f); //stats no longer go up with chapter changing
            if (chapterNum < 1)
                chapterNum = 1;
            string classStats = "<b>Stats</b> <size=80%><i>For chapter " + chapterNum + "</i><size=100%>";
            classStats += "\n\nVitality: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].vitality;
            classStats += "\nHP: " + (classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].vitality * 4);
            classStats += "\nElixirs: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].elixirs;
            classStats += "\nArmor: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].armor;
            classStats += "\nDefense: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].defense;
            int speedStat = classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].speed;
            classStats += "\nSpeed: " + speedStat + " <i>(Run " + Mathf.Ceil(0.5f * speedStat) + ", Dash " + speedStat + ")</i>";

            //classStats += "\n\nAttack Bonus: +" + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].attackBonus;
            classStats += "\nFray Damage: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].frayDamage;


            string damageStat = chapterNum + "d" + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].damageDie;
            int damageAdd = classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].damageAdditionMultiplier;
            if (damageAdd > 0)
                damageStat += "+" + damageAdd;

            classStats += "\nDamage: " + damageStat;
            classStats += "\nBasic Attack: " + classes.classes[workCharacter.classIndex].chapterStats[chapterNum - 1].basicAttack;
            
            charaClassStatLabel.text = classStats;

        }
        else if(index == 1)
        {
            finishCharaButtonLabel.text = (currentMode == UnitEditMode.CharacterNew) ? "Make character" : "Save changes";
        }
    }

    public void DotBarUpdate(int barIndex)
    {
        if (workCharacter == null)
            return;

        int dotMod = actionDots[barIndex].value - actionDots[barIndex].baseValue;
    }

    public IconCharacter MakeNewCharacter()
    {
        IconCharacter nuChara = new IconCharacter();
        currentChosenLevel = 0;
        nuChara.level = currentChosenLevel;

        return nuChara;
    }

    public static NarrativeChara GetBaseNarrativeChara()
    {
        NarrativeChara nuNarra = new NarrativeChara();
        nuNarra.SetCulture(0);
        nuNarra.SetBond(0);
        nuNarra.CleanDotModifiers();

        return nuNarra;
    }

    public IconCharacter GetCharacter(int id)
    {
        if (characterUnits == null)
            return null;

        for(int i = 0; i < characterUnits.Count; i++)
        {
            if (characterUnits[i].unitID == id)
                return characterUnits[i];
        }

        return null;
    }

    public void UpdateCharacter(int id, IconCharacter nuChara)
    {
        if (characterUnits == null)
            return;

        for (int i = 0; i < characterUnits.Count; i++)
        {
            if (characterUnits[i].unitID == id)
            {
                characterUnits[i] = nuChara;
                return;
            }
        }
    }

    public void UpdateCharacterPiece(int id, IconCharacter nuChara)
    {
        Debug.Log("Update Chara Missing");
    }

    public List<IconCharacter> GetCharacters()
    {
        return new List<IconCharacter>(characterUnits);
    }

    //piece casting / uncasting
    private void CharacterPieceEntryPress(int id, int childIndex)
    {
        PieceManager._instance.SpawnGrabbedCharacterPiece(GetCharacter(id));

        entryGrabbed = charaListParent.GetChild(childIndex).GetComponent<RectTransform>();
        entryGrabbedParent = charaListParent;
        entryGrabbedOriginalPosition = entryGrabbed.anchoredPosition;

        entryGrabDelta = Input.mousePosition - entryGrabbed.position;

        entrySiblingIndex = entryGrabbed.GetSiblingIndex();
        entryGrabbed.parent = canvasRT;

        withEntryGrabbed = true;

        charaDetailPanel.SetActive(false);
        slideButtonIcon.transform.parent.gameObject.SetActive(true);
        PieceManager._instance.SetPieceButtonOptions(false);
    }

    private void CharacterPieceEntryRelease(int id, int childIndex)
    {
        if (TooltipManager.CheckMouseInArea(charaListParent))
        {
            PieceManager._instance.DespawnCharacterPiece(GetCharacter(id));
        }

        entryGrabbed.parent = entryGrabbedParent;
        entryGrabbed.SetSiblingIndex(entrySiblingIndex);
        entryGrabbed.anchoredPosition = entryGrabbedOriginalPosition;
        withEntryGrabbed = false;
    }

    public bool CheckIfCharacterNamed()
    {
        if (workCharacter == null)
            return false;

        if (workCharacter.unitName == null || workCharacter.unitName.Length == 0)
        {
            warningCharaNamePanel.SetActive(true);
            return false;
        }

        return true;
    }
    #endregion

    #region foe management

    //save / load
    public void SaveFoe()
    {
        if (workFoe == null)
            return;

        //we make name if it lacks one
        if (!CheckIfFoeNamed())
            return;

        //after pass
        int id = workFoe.unitID;
        workFoe.GiveID(id);

        workFoe = workFoe.MakeCopy();

        int[] pieceIds = PieceCamera._instance.GetCurrentSamplePartIDs();

        workFoe.lastModified = DateTime.Now;

        if (currentMode == UnitEditMode.FoeNew)
        {
            foeUnits.Add(workFoe);
            UpIDCount(id);
        }
        else if (currentMode == UnitEditMode.FoeEdit)
        {
            UpdateFoe(workFoe.unitID, workFoe);
        }

        optionsManager.SaveFoe(id);
        UpdateFoeList();

        CloseEntryScreen();
        currentMode = UnitEditMode.None;
    }

    //plug method for the graphic piece editor
    public void SaveFoe(string graphicPieceID)
    {
        if (workFoe == null)
            return;

        workFoe.GiveGraphicPieceID(graphicPieceID);

        SaveFoe();
    }

    public void SaveMeepleFoe()
    {
        if (workFoe == null)
            return;

        workFoe.GiveGraphicPieceID(""); //overrides the previously set graphic piece ID

        SaveFoe();
    }

    private void LoadFoesCall()
    {
        optionsManager.LoadAllFoes();
    }

    public void ReceiveLoadedFoes(List<IconFoe> fooes)
    {
        if (fooes == null || fooes.Count == 0)
            foeUnits = new List<IconFoe>();
        else
            foeUnits = fooes;

        UpdateFoeList();

        if (foeUnits.Count == 0)
        {
            Debug.Log("No foes found.");
            return;
        }
    }

    //tab list and panel
    private void UpdateFoeList()
    {
        float entrySep = -20f;
        float entryHeight = -100f;

        for (int i = foeListParent.childCount - 1; i >= 1; i--)
        {
            Destroy(foeListParent.GetChild(i).gameObject);
        }

        bool alphaSorting = PlayerPrefs.GetInt("foeListSort", 0) == 0;

        List<IconFoe> sortedUnits = new List<IconFoe>();
        sortedUnits = foeUnits;
        if (alphaSorting)
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByName);
            foeSortButton.sprite = sortAZSprite;
        }
        else
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByNewerDate);
            foeSortButton.sprite = sortTimeSprite;
        }

        float posY = -20f;
        int childCount = 0;
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            GameObject foeListEntry = Instantiate<GameObject>(foeListEntryPrefab, foeListParent);
            RectTransform entryListRT = foeListEntry.GetComponent<RectTransform>();
            childCount++;

            string detailLabel = "";
            string nameAdd = "";

            if (sortedUnits[i].type == FoeType.Mob)
                nameAdd = " <i><size=80%>(Mob)</i>";
            else if (sortedUnits[i].type == FoeType.SpecialSummon)
                nameAdd = " <i><size=80%>(Summon)</i>";
            else if (sortedUnits[i].type == FoeType.Elite)
                nameAdd = " <i><size=80%>(Elite)</i>";
            else if(sortedUnits[i].type == FoeType.Legend)
                nameAdd = " <i><size=80%>(Legend)</i>";

            entryListRT.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedUnits[i].unitName + nameAdd;
            
            string[] details = GetFoeDetails(sortedUnits[i]);

            if (sortedUnits[i].type == FoeType.SpecialSummon)
                detailLabel += details[2];
            else if (sortedUnits[i].type == FoeType.Mob)
                detailLabel += details[1];
            else
            {
                detailLabel += details[2] + " " + details[1];
            }

            entryListRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = "<i>" + detailLabel + "</i>";



            int foeID = sortedUnits[i].unitID;

            int foeType = (int)sortedUnits[i].type;
            int foeEntryChildIndex = childCount;

            entryListRT.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                generalInputs.SetActive(false);
                optionButton.SetActive(false);
                StartFoeEditing(foeID);
            });
            entryListRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                DeleteFoeCall(foeID);
            });
            Color identifierColor = unitHeavyColoring;
            if (sortedUnits[i].type == FoeType.SpecialSummon)
                identifierColor = unitSummonColor;
            else if (sortedUnits[i].type == FoeType.Mob)
                identifierColor = unitMobColor;
            else if (sortedUnits[i].classIndex == 1)
                identifierColor = unitVagabondColoring;
            else if (sortedUnits[i].classIndex == 2)
                identifierColor = unitLeaderColoring;
            else if (sortedUnits[i].classIndex == 3)
                identifierColor = unitArtilleryColoring;

            entryListRT.GetChild(4).GetComponent<Image>().color = identifierColor;

            ShapeIcon entryPointer = entryListRT.GetComponent<ShapeIcon>();
            entryPointer.OnPointerEnterEvent.AddListener(delegate
            {
                ShowFoeDetails(foeID);
            });
            entryPointer.OnPointerExitEvent.AddListener(delegate
            {
                foeDetailPanel.SetActive(false);
                slideButtonIcon.transform.parent.gameObject.SetActive(true);
            });
            entryPointer.OnPointerDownEvent.AddListener(delegate
            {
                FoePieceEntryPress(foeID, foeEntryChildIndex);
            });
            entryPointer.OnPointerUpEvent.AddListener(delegate
            {
                FoePieceEntryRelease(foeID, foeEntryChildIndex);
            });

            Vector2 aPos = new Vector2(0f, posY);
            entryListRT.anchoredPosition = aPos;
            foeListEntry.SetActive(true);

            posY += entrySep + entryHeight;
        }

        Vector2 sd = foeListParent.sizeDelta;
        sd.y = -1f * posY;
        foeListParent.sizeDelta = sd;
    }

    public void ToggleFoeListSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("foeListSort", 0) == 0;

        if (alphaSorting)
            PlayerPrefs.SetInt("foeListSort", 1);
        else
            PlayerPrefs.SetInt("foeListSort", 0);

        UpdateFoeList();
    }

    public void ShowFoeDetails(int foeID)
    {
        for (int i = 0; i < foeUnits.Count; i++)
        {
            if (foeUnits[i].unitID == foeID)
            {
                IconFoe foeInfo = foeUnits[i];
                foeDetailPanel.GetComponent<Image>().color = foeInfo.colorChoice;
                foeDetailNameLabel.text = foeInfo.unitName;
                foeDetailGeneralLabel.text = "<b>Chapt " + foeInfo.level + "</b> - <i>" + MiscTools.GetSpacedForm(foeInfo.type.ToString()) + "</i>";

                string[] deets = GetFoeDetails(foeUnits[i]);

                string additional = "";
                foeDetailTacticalLabel.text = "<b>Tactical Combat</b><size=90%>\n\n� Class - <i>" + deets[0] + "</i>\n\n� Job - <i>" + deets[2] + " (" + deets[1] + ")" + "</i>";

                foeDetailPanel.SetActive(true);
            }
        }

        slideButtonIcon.transform.parent.gameObject.SetActive(false);
    }

    //entries and variable functions

    public void NewFoeCall()
    {
        ShowEntryScreen(1); //we make the landing page
    }

    public void PresetFoeCall()
    {
        landingFoePages.SetActive(false);
        presetsFoePages.SetActive(true);
    }

    public void BuildFoeCall()
    {
        //shoot animation
        fromScratchAnim.SetTrigger("Toggle");

        /*
        listPanel.screenProportionSize = kinCulturePanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        Vector3 listOrigin = foeTypeButton.position + (0.5f * foeTypeButton.rect.size.x * foeTypeButton.lossyScale.x * Vector3.right);

        List<string> foeClassTypeNames = new List<string>();

        foeClassTypeNames.Add("Foe");
        foeClassTypeNames.Add("Mob");
        foeClassTypeNames.Add("Elite");
        foeClassTypeNames.Add("Legend");
        foeClassTypeNames.Add("Special Summon");

        listPanel.ShowPanel(listOrigin, foeClassTypeNames, true);
        foeTypeListOpen = true;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
        */
    }

    public void BuildFoeByIndex(int index)
    {
        landingFoePages.SetActive(false);
        presetsFoePages.SetActive(false);

        ShowEntryScreen(index + 3);
    }

    public void StartFoeMaking(int foeType)
    {
        if(foeType < 3)
        {
            generalFoePages.SetActive(false);
            miniFoesSet.SetActive(false);

            foeNameInput.text = "";
            foeNameInput.ForceLabelUpdate();

            foeNameInput.gameObject.SetActive(false);
            //foeChapterInput.SetActive(false);

            foeTypeLabel.text = "";
            foeTypeLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

            foeEditAcceptButton.SetActive(false);

            foeColor.transform.parent.gameObject.SetActive(false);

            if (foeType == 1) //landing page
            {
                landingFoePages.SetActive(true);
                presetsFoePages.SetActive(false);
            }
            else if(foeType == 2) //presets page
            {
                landingFoePages.SetActive(false);
                presetsFoePages.SetActive(true);
            }
            return;
        }

        foeColor.transform.parent.gameObject.SetActive(true);
        foeNameInput.gameObject.SetActive(true);
        //foeChapterInput.SetActive(true);

        //anything else is building from zero

        currentMode = UnitEditMode.FoeNew;
        currentFoeType = foeType - 3;

        currentFoePage = 1;

        if ((FoeType)currentFoeType == FoeType.Mob || (FoeType)currentFoeType == FoeType.SpecialSummon)
        {
            mobSummonButtonLabel.transform.parent.GetComponent<HoldButton>().enabled = true;
            mobMakingPageStepper.StepperSetup();
            pageChangedFlags = new bool[mobMakingPageStepper.pages.Length];
            mobMakingPageStepper.ResetCurrentPage();
        }
        else
        {
            foeMakingPageStepper.StepperSetup();
            pageChangedFlags = new bool[foeMakingPageStepper.pages.Length];
            foeMakingPageStepper.ResetCurrentPage();
        }
        
        for (int i = 1; i < pageChangedFlags.Length; i++)
            pageChangedFlags[i] = true;

        workFoe = GetFoeBase((FoeType)currentFoeType);

        string typeName = MiscTools.GetSpacedForm(((FoeType)currentFoeType).ToString());
        foeTypeLabel.text = typeName;
        foeTypeLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = typeName;
        foeChapter.text = "Chapter 1";
        currentChosenChapter = 1;

        foeColorLabel.text = "White";
        foeColor.color = Color.white;
        workFoe.colorChoice = Color.white;

        choiceWasClassJob = true;
        choiceWasFactionJob = true;

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerMeepleConfig(0, 1, 0, 0);

        currentMode = UnitEditMode.FoeNew;

        miniFoesSet.SetActive(workFoe.type == FoeType.Mob || workFoe.type == FoeType.SpecialSummon);
        generalFoePages.SetActive(workFoe.type != FoeType.Mob && workFoe.type != FoeType.SpecialSummon);

        SetScrollTipRectsActive(false);
        UpdateFoePage(foeType, 0);
    }

    public void StartFoeEditing(int foeID)
    {
        foeEntryScreen.SetActive(true);

        currentFoePage = 1;

        workFoe = GetFoe(foeID);

        currentFoeType = (int)workFoe.type;

        if ((FoeType)currentFoeType == FoeType.Mob || (FoeType)currentFoeType == FoeType.SpecialSummon)
        {
            mobMakingPageStepper.StepperSetup();
            pageChangedFlags = new bool[mobMakingPageStepper.pages.Length];
            mobMakingPageStepper.ResetCurrentPage();
        }
        else
        {
            foeMakingPageStepper.StepperSetup();
            pageChangedFlags = new bool[foeMakingPageStepper.pages.Length];
            foeMakingPageStepper.ResetCurrentPage();
        }

        for (int i = 1; i < pageChangedFlags.Length; i++)
            pageChangedFlags[i] = true;

        foeNameInput.text = workFoe.unitName;
        foeNameInput.ForceLabelUpdate();

        foeEditAcceptButton.SetActive(true);

        string typeName = MiscTools.GetSpacedForm(((FoeType)currentFoeType).ToString());
        foeTypeLabel.text = typeName;
        foeTypeLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = typeName;
        foeChapter.text = "Chapter " + workFoe.level;
        currentChosenChapter = workFoe.level;

        string colorName = ColorManager._instance.GetColorName(workFoe.colorChoice);
        if (colorName.Length == 0)
            colorName = "White";
        foeColorLabel.text = colorName;
        foeColor.color = workFoe.colorChoice;
        if (colorName.Length == 0)
        {
            foeColor.color = Color.white;
            workFoe.colorChoice = Color.white;
        }

        /*
        if ((FoeType)currentFoeType == FoeType.Mob) 
        {
            choiceWasClassJob = foes.mobs[workFoe.classIndex].factionIndex < 0;
            if (!choiceWasClassJob)
                choiceWasFactionJob = true;
        }
        else if ((FoeType) currentFoeType == FoeType.SpecialSummon)
        {
            choiceWasClassJob = foes.specialSummons[workFoe.classIndex].factionIndex < 0;
            if (!choiceWasClassJob)
                choiceWasFactionJob = true;
        }
        */

        choiceWasFactionJob = true;

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerConfig(workFoe, true);

        currentMode = UnitEditMode.FoeEdit;

        miniFoesSet.SetActive(workFoe.type == FoeType.Mob || workFoe.type == FoeType.SpecialSummon);
        generalFoePages.SetActive(workFoe.type != FoeType.Mob && workFoe.type != FoeType.SpecialSummon);

        SetScrollTipRectsActive(false);
        UpdateFoePage(currentFoeType, 0);
    }

    public void DeleteFoeCall(int foeID)
    {
        unitIDToDelete = foeID;

        for(int i = 0; i < foeUnits.Count; i++)
        {
            if(foeUnits[i].unitID == foeID)
            {
                foeDeleteConfirmationPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Are you sure you want to delete the foe \"" + foeUnits[i].unitName + "\" ?";
                foeDeleteConfirmationPanel.SetActive(true);
                break;
            }
        }
    }

    public void DeleteFoe()
    {

        optionsManager.DeleteFoe(unitIDToDelete);
        LoadFoesCall();

        PieceManager._instance.ErasePiecesWithID(unitIDToDelete);

        foeDeleteConfirmationPanel.SetActive(false);
    }

    private IconFoe MakeNewFoe()
    {
        IconFoe nuFoe = new IconFoe();
        currentChosenChapter = 1;
        nuFoe.level = currentChosenChapter;
        nuFoe.classIndex = 0;

        return nuFoe;
    }

    public IconFoe GetFoeBase(FoeType type)
    {
        IconFoe baseFoe = MakeNewFoe();
        baseFoe.type = (FoeType)type;
        baseFoe.GiveID(RequestNextUnitID());

        return baseFoe;
    }

    public void UpdateFoeName()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        if (foeNameInput.text.Length < 1)
            return;

        workFoe.unitName = foeNameInput.text;
    }

    public void ChangeFoeLevel(bool forward)
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        int auxChapter = currentChosenChapter + (forward ? 1 : -1);
        auxChapter = Mathf.Clamp(currentChosenChapter, 1, 3);

        //check chapter limits

        //if so, refresh the abilities and traits shown in the subpanel

        //if not, show notif explaining situation

        currentChosenChapter += forward ? 1 : -1;

        currentChosenChapter = Mathf.Clamp(currentChosenChapter, 1, 3);

        foeChapter.text = "Cpt. " + currentChosenChapter;

        workFoe.level = currentChosenChapter;

        for (int i = 2; i < pageChangedFlags.Length; i++)
            pageChangedFlags[i] = true;
    }

    public void OpenFoeClassList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform classButtonRT = foeBaseClassLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = classButtonRT.position + (0.5f * classButtonRT.rect.size.x*classButtonRT.lossyScale.x * Vector3.right);

        List<string> classTypes = new List<string>();

        List<FoeData.FoeClass> classList = foes.classes;

        for (int i = 0; i < classList.Count; i++)
        {
            classTypes.Add(classList[i].name);
        }

        listPanel.ShowPanel(listOrigin, classTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = true;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;
        
        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenMobSummonList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform classButtonRT = mobSummonButtonLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = classButtonRT.position + (0.5f * classButtonRT.rect.size.x*classButtonRT.lossyScale.x * Vector3.right);

        List<string> classTypes = new List<string>();

        if (workFoe.type == FoeType.Mob)
        {
            for(int i = 0; i < foes.mobs.Length; i++)
            {
                classTypes.Add(foes.mobs[i].name);
            }
        }
        else if (workFoe.type == FoeType.SpecialSummon)
        {
            for (int i = 0; i < summons.specialSummons.Length; i++)
            {
                classTypes.Add(summons.specialSummons[i].name);
            }
        }

        listPanel.ShowPanel(listOrigin, classTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = true;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;

        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenFoeJobFactionChoiceList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform choiceButtonRT = foeJobFactionChoiceLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = choiceButtonRT.position + (0.5f * choiceButtonRT.rect.size.x*choiceButtonRT.lossyScale.x * Vector3.right);
        List<string> choices = new List<string>();
        choices.Add("Class Job");
        choices.Add("Faction Options");

        listPanel.ShowPanel(listOrigin, choices, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = true;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;

        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenFoeJobFactionList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform subChoiceButtonRT = foeJobFactionSubChoiceLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = subChoiceButtonRT.position + (0.5f * subChoiceButtonRT.rect.size.x*subChoiceButtonRT.lossyScale.x * Vector3.right);
        List<string> subChoices = new List<string>();

        if (choiceWasClassJob)
        {
            if (workFoe.type == FoeType.Foe)
            {
                for (int i = 0; i < foes.classes[workFoe.classIndex].jobs.Count; i++)
                {
                    subChoices.Add(foes.classes[workFoe.classIndex].jobs[i].name);
                }
            }
            else if(workFoe.type == FoeType.Elite)
            {
                subChoices.Add(foes.eliteClasses[workFoe.classIndex].name);
            }
            else if(workFoe.type == FoeType.Legend)
            {
                subChoices.Add(foes.legendClasses[workFoe.classIndex].name);
            }
        }
        else
        {
            for(int f = 0; f < factions.Length; f++)
            {
                for(int i = 0; i < factions[f].foeFactions.Length; i++)
                {
                    subChoices.Add(factions[f].foeFactions[i].factionName);
                }
            }
        }

        listPanel.ShowPanel(listOrigin, subChoices, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = true;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;

        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenFoeTemplateList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        
        RectTransform templateButtonRT = foeTemplateButtonLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = templateButtonRT.position + (0.5f * templateButtonRT.rect.size.x*templateButtonRT.lossyScale.x * Vector3.right);
        List<string> templateTypes = new List<string>();

        templateTypes.Add("None");

        for(int i = 0; i < templates.templates.Length; i++)
        {
            templateTypes.Add(templates.templates[i].templateName);
        }

        listPanel.ShowPanel(listOrigin, templateTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;

        templateListOpen = true;
        foeSubTemplateListOpen = false;

        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenFoeSubTemplateList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;


        RectTransform subTemplateButtonRT = templatePageSubTemplateButtonLabel.transform.parent.GetComponent<RectTransform>();
        if (!foeTemplateSubPageChosen)
            subTemplateButtonRT = factionPageSubTemplateButtonLabel.transform.parent.GetComponent<RectTransform>();

        Vector3 listOrigin = subTemplateButtonRT.position + (0.5f * subTemplateButtonRT.rect.size.x*subTemplateButtonRT.lossyScale.x * Vector3.right);
        List<string> subTemplateTypes = new List<string>();

        subTemplateTypes.Add("None");

        if (workFoe.type != FoeType.Mob)
            subTemplateTypes.Add("Mob");
        if (workFoe.type != FoeType.Elite && workFoe.type != FoeType.Legend)
            subTemplateTypes.Add("Elite");

        listPanel.ShowPanel(listOrigin, subTemplateTypes, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = false;

        templateListOpen = false;
        foeSubTemplateListOpen = true;

        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenJobUniqueChoiceList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform choiceButtonRT = foeJobFactionChoiceLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = choiceButtonRT.position + (0.5f * choiceButtonRT.rect.size.x*choiceButtonRT.lossyScale.x * Vector3.right);
        List<string> choices = new List<string>();
        choices.Add("Faction Job");
        choices.Add("Unique " + workFoe.type.ToString());

        listPanel.ShowPanel(listOrigin, choices, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = true;
        foeJobUniqueListOpen = false;

        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void OpenJobUniqueList()
    {
        if (workFoe == null)
            return;

        int foeType = (int)workFoe.type;

        listPanel.screenProportionSize = bondClassJobPanelProportions;
        listPanel.listColor = 0.9f * foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform subChoiceButtonRT = foeJobUniqueSubChoiceLabel.transform.parent.GetComponent<RectTransform>();
        Vector3 listOrigin = subChoiceButtonRT.position + (0.5f * subChoiceButtonRT.rect.size.x*subChoiceButtonRT.lossyScale.x * Vector3.right);
        List<string> subChoices = new List<string>();

        listPanel.ShowPanel(listOrigin, subChoices, true);
        kinListOpen = false;
        bondListOpen = false;
        startActionListOpen = false;
        cultureListOpen = false;
        classListOpen = false;
        colorListOpen = false;

        mobSummonListOpen = false;

        foeJobFactionChoiceOpen = false;
        foeJobFactionListOpen = false;

        foeJobUniqueChoiceOpen = false;
        foeJobUniqueListOpen = true;

        templateListOpen = false;
        foeSubTemplateListOpen = false;
        listPanel.OnEntryClick += FoeListClick;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void FoeListClick(int index)
    {
        if (foeTypeListOpen)
        {
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= FoeListClick;
            foeTypeListOpen = false;

            ShowEntryScreen(index + 3);
        }
        else if (classListOpen)
        {
            if (workFoe == null)
                return;

            int foeType = (int)workFoe.type;

            bool wasChanged = workFoe.classIndex != index;

            workFoe.classIndex = index;

            choiceWasClassJob = true;
            choiceWasFactionJob = true;

            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= FoeListClick;
            classListOpen = false;

            if (wasChanged)
            {
                UpdateFoePage(foeType, 0);

                for(int i = 1; i < pageChangedFlags.Length; i++)
                {
                    pageChangedFlags[i] = true;
                }
            }
        }
        else if (mobSummonListOpen)
        {
            if (workFoe == null)
                return;

            int foeType = (int)workFoe.type;

            bool wasChanged = workFoe.classIndex != index;

            workFoe.classIndex = index;

            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= FoeListClick;
            mobSummonListOpen = false;

            if (wasChanged)
            {
                UpdateFoePage(foeType, 0);
            }
        }
        else if (foeJobFactionChoiceOpen)
        {
            if (workFoe == null)
                return;

            int foeType = (int)workFoe.type;

            bool wasChanged = choiceWasClassJob != (index == 0);

            choiceWasClassJob = index == 0;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= FoeListClick;
            jobListOpen = false;

            if (wasChanged)
            {
                UpdateFoePage(foeType, 1);
                pageChangedFlags[2] = true;
            }
        }
        else if (foeJobFactionListOpen)
        {
            if (workFoe == null)
                return;

            int foeType = (int)workFoe.type;

            bool wasChanged = false;



            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= FoeListClick;
            jobListOpen = false;

            if (wasChanged)
            {
                UpdateFoePage(foeType, 1);
                for(int i = 2; i < pageChangedFlags.Length; i++)
                {
                    pageChangedFlags[i] = true;
                }
            }
        }
        else if (colorListOpen)
        {
            if (workFoe == null)
                return;
            
            workFoe.colorChoice = ColorManager._instance.colors[index].color;
            ColorManager._instance.HideGeneralColorPanel();
            ColorManager._instance.generalColorList.OnEntryClick -= FoeListClick;
            colorListOpen = false;

            int foeType = (int)workFoe.type;

            foeColorLabel.text = ColorManager._instance.colors[index].name;
            foeColor.color = ColorManager._instance.colors[index].color;
        }
    }

    public void ChangePageCall()
    {
        int foeType = (int)workFoe.type;
        if(workFoe.type == FoeType.Mob || workFoe.type == FoeType.SpecialSummon || presetPageActive)
            currentFoePage = mobMakingPageStepper.eventFlipFlag > 0 ? mobMakingPageStepper.currentPage + 1 : mobMakingPageStepper.currentPage - 1;
        else
            currentFoePage = foeMakingPageStepper.eventFlipFlag > 0 ? foeMakingPageStepper.currentPage + 1 : foeMakingPageStepper.currentPage - 1;

        if (pageChangedFlags[currentFoePage])
        {
            UpdateFoePage(foeType, currentFoePage);
            pageChangedFlags[currentFoePage] = false;
        }
    }

    private void UpdateFoePage(int foeType, int pageNumber)
    {
        if (workFoe == null)
            return;

        if (pageNumber == 0)
        {
            if (workFoe.type == FoeType.Mob || workFoe.type == FoeType.SpecialSummon)
            {
                mobSummonButtonLabel.text = workFoe.type == FoeType.Mob ? foes.mobs[workFoe.classIndex].name : summons.specialSummons[workFoe.classIndex].name;

                FoeData.FoeClass.FoeStats foeStats = workFoe.BuildStatsSet();

                string enemyStats = "<b>Stats:</b>";
                enemyStats += "\n\n�HP: " + (foeStats.HP);
                enemyStats += "\n�Defense: " + foeStats.defense;
                enemyStats += "\n�Speed: " + foeStats.speed + " <i>(Dash " + foeStats.dash + ")</i>";

                if(foeStats.frayDamage > 0)
                    enemyStats += "\n�Fray Damage: " + foeStats.frayDamage;

                if(foeStats.dieAmount != 0 && foeStats.damageDie != 0)
                    enemyStats += "\n�Damage: " + foeStats.dieAmount + "d" + foeStats.damageDie;

                mobSummonOverallStatsLabel.text = enemyStats;

                string enemyAttackTraits = "";

                CheckDiffPreUpdate(mobSummonOverallTraitsActionsContent, mobSummonOverallTraitsActionsText, enemyAttackTraits, 200, 1.2f);
            }
            else
            {
                if (workFoe.type == FoeType.Mob || workFoe.type == FoeType.SpecialSummon)
                {
                    return;
                }

                List<FoeData.FoeClass> classList = foes.classes;

                foeBaseClassLabel.text = classList[workFoe.classIndex].name;

                if (workFoe.classIndex < 0)
                {
                    foeClassStatsText.text = "";
                    foeClassStatsText.ForceMeshUpdate();

                    StartCoroutine(DelayedRectResize(foeClassStatsContent, foeClassStatsText));
                    return;
                }

                string classStats = "<b>Base Class Stats:</b>";
                classStats += "\n\n�HP: " + (classList[workFoe.classIndex].classStats.HP);
                classStats += "\n�Defense: " + classList[workFoe.classIndex].classStats.defense;
                classStats += "\n�Speed: " + classList[workFoe.classIndex].classStats.speed + " <i>(Dash " + classList[workFoe.classIndex].classStats.dash + ")</i>";

                //classStats += "\n\nAttack: +" + TranslateStatWithChapter(classList[workFoe.classIndex].classStats[chapterNum - 1].attack, chapterNum);
                classStats += "\n�Fray Damage: " + classList[workFoe.classIndex].classStats.frayDamage;

                classStats += "\n�Damage: " + classList[workFoe.classIndex].classStats.dieAmount + "d" + classList[workFoe.classIndex].classStats.damageDie;

                CheckDiffPreUpdate(foeClassStatsContent, foeClassStatsText, classStats, 200, 1.2f);

                return;
            }
        }
        else if(pageNumber == 1)
        {
            if(choiceWasClassJob)
            {
                foeJobFactionChoiceLabel.text = "Class Job";

                return;
            }
        }
        else if(pageNumber == 2)
        {

            return;
        }
        else if (pageNumber == 3)
        {
            BuildSummaryPage();
        }
    }

    private void CheckDiffPreUpdate(RectTransform contentRT, TextMeshProUGUI textElement, string newText, int sampleSize, float rectSizeFactor = 1f)
    {
        /*
        string sample1 = textElement.text;
        sample1 = sample1.Substring(0, Mathf.Clamp(sampleSize, 0, sample1.Length));
        string sample2 = newText.Substring(0, Mathf.Clamp(sampleSize, 0, newText.Length));

        Debug.Log("--" + sample1);
        Debug.Log("--" + sample2);

        if (sample1 != sample2)
        {
            textElement.text = newText;
            textElement.ForceMeshUpdate();
            StartCoroutine(DelayedRectResize(contentRT, textElement, rectSizeFactor));
        }
        */

        textElement.text = newText;
        textElement.ForceMeshUpdate();
        StartCoroutine(DelayedRectResize(contentRT, textElement, rectSizeFactor));
    }

    private IEnumerator DelayedRectResize(RectTransform contentRT, TextMeshProUGUI textElement, float rectSizeFactor = 1f)
    {
        yield return new WaitForEndOfFrame();

        Vector2 sd = contentRT.sizeDelta;
        sd.y = rectSizeFactor * textElement.renderedHeight;
        contentRT.sizeDelta = sd;
        contentRT.anchoredPosition = Vector2.zero;
    }

    public string[] GetFoeDetails(IconFoe unit)
    {
        string[] deets = new string[3];

        //get abilities and descriptions

        return deets;
    }

    public static string TranslateStatWithChapter(string stat, int chapterNum)
    {
        int plusIndex = stat.IndexOf('+');

        string result = "";
        if (plusIndex < 0)
        {
            stat = RemoveSpaces(stat);
            if (stat.Equals("chapter"))
                result = chapterNum.ToString();
            else
                result = stat;

            return result;
        }

        string firstHalf = stat.Substring(0, plusIndex);
        firstHalf = RemoveSpaces(firstHalf);
        string secondHalf = stat.Substring(plusIndex + 1);
        secondHalf = RemoveSpaces(secondHalf);

        int intResult = -1;
        if(!int.TryParse(firstHalf, out intResult))
        {
            result = firstHalf + " + ";
        }

        int chapInt = 0;
        if (secondHalf.Equals("chapter"))
            chapInt = chapterNum;

        if (intResult < 0)
        {
            result += chapInt;
        }
        else
            result += (chapInt + intResult).ToString();

        return result;
    }

    public IconFoe GetFoe(int id)
    {
        if (foeUnits == null)
            return null;

        for (int i = 0; i < foeUnits.Count; i++)
        {
            if (foeUnits[i].unitID == id)
                return foeUnits[i];
        }

        return null;
    }

    public void UpdateFoe(int id, IconFoe nuFoe)
    {
        if (characterUnits == null)
            return;

        for (int i = 0; i < foeUnits.Count; i++)
        {
            if (foeUnits[i].unitID == id)
            {
                foeUnits[i] = nuFoe;
                return;
            }
        }
    }

    public void UpdateFoePiece(int id, IconFoe nuFoe)
    {
        if (characterUnits == null)
            return;

        Debug.Log("Update Foe Missing");
    }

    public List<IconFoe> GetFoes()
    {
        return new List<IconFoe>(foeUnits);
    }

    private void BuildSummaryPage()
    {
        string titleLabel = "";

        //get details

        //now stats
        string overallStats = "Stats\n\n";

        FoeData.FoeClass.FoeStats stats = workFoe.BuildStatsSet();

        overallStats += "<b>�HP: </b>" + stats.HP + "\n";
        overallStats += "<b>�Speed: </b>" + stats.speed + "(Dash " + stats.dash + ")\n";
        overallStats += "<b>�Defense: </b>" + stats.defense + "\n";
        overallStats += "<b>�Fray Damage: </b>" + stats.HP + "\n";
        overallStats += "<b>�Damage: </b>" + stats.dieAmount + "d" + stats.damageDie + "\n";

        foeOverallStatsLabel.text = overallStats;

        //now, everything else

        string actionTraitText = "<b>Abilities:</b>\n";

        List<Ability> oAttacks = workFoe.GetAbilities();
        for (int i = 0; i < oAttacks.Count; i++)
        {
            if (oAttacks[i].phaseIndex == 0)
                actionTraitText += "\n><b>" + oAttacks[i].abilityName;
            else
                actionTraitText += "\n><size=65%>(Phase " + oAttacks[i].phaseIndex + " only) <size=100%><b>" + oAttacks[i].abilityName;

            actionTraitText += " (";
            actionTraitText += oAttacks[i].actionCost + " action";

            string[] aspects = oAttacks[i].abilityAspects;
            if (aspects.Length > 0)
            {
                actionTraitText += ", ";
                for (int c = 0; c < aspects.Length; c++)
                {
                    actionTraitText += aspects[c];
                    if (c < (aspects.Length - 1))
                        actionTraitText += ", ";
                    else
                        actionTraitText += ")";
                }
            }
            else
                actionTraitText += ")";

            actionTraitText += ":</b> <i>" + oAttacks[i].abilityEffect + "</i>";
        }

        CheckDiffPreUpdate(foeOverallTraitsActionsContent, foeOverallTraitsActionsText, actionTraitText, 40, 1.2f);
    }

    //receive foe from presets and display them on screen

    public void ReceiveFoePreset(IconFoe builtFoe)
    {
        workFoe = builtFoe;

        foeColor.transform.parent.gameObject.SetActive(true);
        foeNameInput.gameObject.SetActive(true);
        //foeChapterInput.SetActive(true);
        //foeChapterInput.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Cpt. " + foeChapter;

        currentFoeType = (int)workFoe.type;

        currentFoePage = 1;

        mobSummonButtonLabel.transform.parent.GetComponent<HoldButton>().enabled = false;

        //special flag for help managing presets directly on the mobsummon type of page
        presetPageActive = true;

        //set up the page stepper
        mobMakingPageStepper.StepperSetup();
        pageChangedFlags = new bool[mobMakingPageStepper.pages.Length];
        mobMakingPageStepper.ResetCurrentPage();

        for (int i = 1; i < pageChangedFlags.Length; i++)
            pageChangedFlags[i] = true;

        string typeName = MiscTools.GetSpacedForm(((FoeType)currentFoeType).ToString());
        foeTypeLabel.text = typeName;
        foeTypeLabel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = typeName;

        foeColorLabel.text = "White";
        foeColor.color = Color.white;
        workFoe.colorChoice = Color.white;

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerMeepleConfig(0, 1, 0, 0);

        currentMode = UnitEditMode.FoeNew;

        presetsFoePages.SetActive(false);
        miniFoesSet.SetActive(true); //for both cases, mini set will be used, since we only want a summary and access to the piece modification page.
        generalFoePages.SetActive(false);

        FoeData.FoeClass.FoeStats foeStats = workFoe.BuildStatsSet();

        string enemyStats = "<b>Stats:</b>";
        enemyStats += "\n\n�HP: " + (foeStats.HP);
        enemyStats += "\n�Defense: " + foeStats.defense;
        enemyStats += "\n�Speed: " + foeStats.speed + " <i>(Dash " + foeStats.dash + ")</i>";

        if (foeStats.frayDamage > 0)
            enemyStats += "\n�Fray Damage: " + foeStats.frayDamage;

        if (foeStats.dieAmount != 0 && foeStats.damageDie != 0)
            enemyStats += "\n�Damage: " + foeStats.dieAmount + "d" + foeStats.damageDie;

        mobSummonOverallStatsLabel.text = enemyStats;

        string enemyAttackTraits = "";

        List<ClassData.Ability> foeAttacks = workFoe.GetAbilities();
        if (foeAttacks.Count > 0)
        {
            enemyAttackTraits += "\n\n<b>Abilities:</b>\n";

            for (int i = 0; i < foeAttacks.Count; i++)
            {
                string actionTraitText = "\n><b>" + foeAttacks[i].abilityName;

                actionTraitText += " (";

                actionTraitText += foeAttacks[i].actionCost + " action";

                string[] aspects = foeAttacks[i].abilityAspects;
                if (aspects != null && aspects.Length > 0)
                {
                    actionTraitText += ", ";
                    for (int c = 0; c < aspects.Length; c++)
                    {
                        actionTraitText += aspects[c];
                        if (c < (aspects.Length - 1))
                            actionTraitText += ", ";
                        else
                            actionTraitText += ")";
                    }
                }
                else
                    actionTraitText += ")";

                actionTraitText += ":</b> <i>" + foeAttacks[i].abilityEffect + "</i>";

                enemyAttackTraits += actionTraitText;
            }
        }

        CheckDiffPreUpdate(mobSummonOverallTraitsActionsContent, mobSummonOverallTraitsActionsText, enemyAttackTraits, 200, 1.2f);
    }

    #region Foe Description Panels
    public void ShowFoeClassDescription()
    {
        ShowDescriptionPanel(foes.classes[workFoe.classIndex].name, foes.classes[workFoe.classIndex].foeClassDescription);
    }
    #endregion

    private void ShowDescriptionPanel(string title, string description)
    {
        string panelDesc = "<i>" + title + "</i>\n\n<size=80%>";
        panelDesc += MiscTools.GetLineJumpedForm(description);

        if (foeDescriptionsRT == null)
            foeDescriptionsRT = foeDescriptionsLabel.transform.parent.GetComponent<RectTransform>();

        CheckDiffPreUpdate(foeDescriptionsRT, foeDescriptionsLabel, panelDesc, 200, 1.2f);

        foeDescriptionsPanel.gameObject.SetActive(true);
    }

    public void ShowAttackList()
    {
        if (workFoe == null)
            return;

    }


    //piece casting / uncasting
    private void FoePieceEntryPress(int id, int childIndex)
    {
        PieceManager._instance.SpawnGrabbedFoePiece(GetFoe(id));

        entryGrabbed = foeListParent.GetChild(childIndex).GetComponent<RectTransform>();
        entryGrabbedParent = foeListParent;
        entryGrabbedOriginalPosition = entryGrabbed.anchoredPosition;

        entryGrabDelta = Input.mousePosition - entryGrabbed.position;

        entrySiblingIndex = entryGrabbed.GetSiblingIndex();
        entryGrabbed.parent = canvasRT;

        withEntryGrabbed = true;

        foeDetailPanel.SetActive(false);
        slideButtonIcon.transform.parent.gameObject.SetActive(true);
        PieceManager._instance.SetPieceButtonOptions(false);
    }

    private void FoePieceEntryRelease(int id, int childIndex)
    {
        if (TooltipManager.CheckMouseInArea(foeListParent))
        {
            PieceManager._instance.DespawnFoePiece(GetFoe(id));
        }

        entryGrabbed.parent = entryGrabbedParent;
        entryGrabbed.SetSiblingIndex(entrySiblingIndex);
        entryGrabbed.anchoredPosition = entryGrabbedOriginalPosition;
        withEntryGrabbed = false;
    }

    public bool CheckIfFoeNamed()
    {
        if (workFoe == null)
            return false;

        if (workFoe.unitName == null || workFoe.unitName.Length == 0)
        {
            warningFoeNamePanel.SetActive(true);
            return false;
        }

        return true;
    }
    #endregion

    public static string RemoveSpaces(string text)
    {
        return Regex.Replace(text, @"\s+", "");
    }
    
    //gets next available ID
    private int RequestNextUnitID()
    {
        /*
        string nextHexID = PlayerPrefs.GetString("UnitHexID", "000000");

        int candidate = int.Parse(nextHexID, System.Globalization.NumberStyles.HexNumber);
        */

        int candidate = optionsManager.unitFileIndex; //int auxCand =
        candidate++;

        //we check possible overwrites
        while (true)
        {
            IconCharacter possChara = GetCharacter(candidate);
            IconFoe possFoe = GetFoe(candidate);

            if (possChara == null || possFoe == null)
                break;
            else
                candidate++;
        }

        /*
        if (auxCand < candidate)
        {
            string newBase = candidate.ToString("X6");
            PlayerPrefs.SetString("UnitHexID", newBase);
        }
        */

        optionsManager.SetUnitIndex(candidate);

        optionsManager.SaveIndexFile();

        return candidate;
    }

    //ups the ID count and saves the hex value in the prefs
    private void UpIDCount(int usedID)
    {
        optionsManager.SetUnitIndex(usedID);

        optionsManager.SaveIndexFile();

        /*
        string nextHexID = PlayerPrefs.GetString("UnitHexID", "000000");
        int intValue = int.Parse(nextHexID, System.Globalization.NumberStyles.HexNumber);
        intValue++;
        nextHexID = intValue.ToString("X6");
        PlayerPrefs.SetString("UnitHexID", nextHexID);
        */
    }
}
