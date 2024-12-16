using AnotherFileBrowser.Windows;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System.IO;
using System.IO.Compression;

public class ExpeditionScreen : MonoBehaviour
{
    [SerializeField] private MenuManager menuManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI expeditionTitle;
    [SerializeField] private TabbedLabels optionTabs;
    [SerializeField] private GameObject confirmExpeditionDeletePanel;

    private int currentOptionIndex = 0;

    [Header("Maps")]
    [SerializeField] private GameObject mapOptionsContainer;
    [SerializeField] private GameObject exportSuccesfulScreen;

    [Header("Unit Management")]
    [SerializeField] private RectTransform unitModeGameAnchor;
    [SerializeField] private RectTransform unitModeMenuAnchor;
    [SerializeField] private RectTransform unitModeMenuRoot;
    [SerializeField] private GameObject gameUIScroll;
    [Space(5f)]
    [SerializeField] private RectTransform listPanelObject;
    [SerializeField] private RectTransform listPanelGameAnchor;
    [SerializeField] private RectTransform listPanelMenuAnchor;

    [Header("Characters")]
    [SerializeField] private GameObject characterOptionsContainer;
    [SerializeField] private RectTransform characterScrollList;
    [SerializeField] private GameObject characterEntry;
    [SerializeField] private RectTransform characterSummaryScreen;
    [SerializeField] private TextMeshProUGUI charSummaryName;
    [SerializeField] private TextMeshProUGUI charSummaryValues;
    [SerializeField] private TextMeshProUGUI charSummaryNarrative;
    [SerializeField] private TextMeshProUGUI charSummaryTactical;

    [SerializeField] private Image charaSortButton;

    [Header("Foes")]
    [SerializeField] private GameObject foeOptionsContainer;
    [SerializeField] private RectTransform foeScrollList;
    [SerializeField] private GameObject foeEntry;
    [SerializeField] private RectTransform foeSummaryScreen;
    [SerializeField] private TextMeshProUGUI foeSummaryName;
    [SerializeField] private TextMeshProUGUI foeSummaryValues;
    [SerializeField] private TextMeshProUGUI foeSummaryNarrative;
    [SerializeField] private TextMeshProUGUI foeSummaryTactical;

    [SerializeField] private Image foeSortButton;

    [Header("Graphics")]
    [SerializeField] private GameObject graphicOptionsContainer;
    [SerializeField] private Color pieceEditorColor = Color.white;

    public void LoadExpedition(string expeditionName)
    {
        Debug.Log("loading expedition " + expeditionName);

        expeditionTitle.text = expeditionName;

        //load up units. this SHOULD overwrite previous lists
        menuManager.optionsManager.LoadAllCharacters();
        menuManager.optionsManager.LoadAllFoes();

        //update indexes from indexfile
        menuManager.optionsManager.LoadIndexFile();

        //load existing maps
        menuManager.MakeFileList();


        //load units
        menuManager.optionsManager.LoadAllCharacters();
        menuManager.optionsManager.LoadAllFoes();

        BuildCharaList();
        BuildFoeList();

        optionTabs.TabClick(0);
        OptionChoice();
    }
    
    public void ExpeditionDeleteCall()
    {
        confirmExpeditionDeletePanel.SetActive(true);
    }

    public void DeleteExpedition()
    {
        //delete file
        string sourcePath = menuManager.GetExpeditionsRootFolder() + "/" + expeditionTitle.text;
        Debug.Log("deleting directory " + sourcePath);
        Directory.Delete(sourcePath, true);

        //turn off expedition screen
        menuManager.ReturnFromExpeditionScreen();

        confirmExpeditionDeletePanel.SetActive(false);
    }

    public void OptionChoice()
    {
        int chosenIndex = optionTabs.chosenIndex;
        mapOptionsContainer.SetActive(chosenIndex == 0);
        characterOptionsContainer.SetActive(chosenIndex == 1);
        foeOptionsContainer.SetActive(chosenIndex == 2);
        graphicOptionsContainer.SetActive(chosenIndex == 3);

        currentOptionIndex = chosenIndex;

        if (currentOptionIndex == 0)
            menuManager.CheckMenuUIRearrange(true); //we force rearrange
    }

    //brings the unit menus forward so they can be manipulated from the menu UI
    public void SetUnitMenuState(bool menuState)
    {
        if (menuState)
        {
            unitModeMenuRoot.parent = unitModeMenuAnchor;
            //listPanelObject.parent = listPanelMenuAnchor;
            gameUIScroll.SetActive(false);
        }
        else
        {
            unitModeMenuRoot.parent = unitModeGameAnchor;
            //listPanelObject.parent = listPanelGameAnchor;
            gameUIScroll.SetActive(true);
        }

        if (currentOptionIndex == 1)
            BuildCharaList();
        else if (currentOptionIndex == 2)
            BuildFoeList();
    }

    #region Export

    public void ExportExpeditionCall()
    {
        var bp = new BrowserProperties();
        bp.filterIndex = 0;

        new FileBrowser().OpenFolderBrowser(bp, path =>
        {
            ChooseExportPath(path);
        });
    }

    private void ChooseExportPath(string path)
    {
        if (path.Length == 0)
            return;

        string sourcePath = menuManager.GetExpeditionsRootFolder() + "/" + expeditionTitle.text;
        string zipPath = path + "/" + expeditionTitle.text + ".iconexp";

        if(File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(sourcePath, zipPath);

        exportSuccesfulScreen.SetActive(true);
    }

    #endregion

    #region Character Options

    private void BuildCharaList()
    {
        for (int i = characterScrollList.childCount - 1; i >= 1; i--)
        {
            Destroy(characterScrollList.GetChild(i).gameObject);
        }

        List<EsperCharacter> characterUnits = UnitManager._instance.GetCharacters();

        bool alphaSorting = PlayerPrefs.GetInt("charaListSort", 0) == 0;

        List<EsperCharacter> sortedUnits = new List<EsperCharacter>();
        sortedUnits = characterUnits;
        if (alphaSorting)
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByName);
            charaSortButton.sprite = UnitManager._instance.sortAZSprite;
        }
        else
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByNewerDate);
            charaSortButton.sprite = UnitManager._instance.sortTimeSprite;
        }

        int childCount = 0;
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            GameObject charaListEntry = Instantiate<GameObject>(characterEntry, characterScrollList);
            RectTransform entryListRT = charaListEntry.GetComponent<RectTransform>();
            childCount++;

            entryListRT.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedUnits[i].unitName;
            int charaID = sortedUnits[i].unitID;
            int charaEntryChildIndex = childCount;

            entryListRT.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                EditCharacterCall(charaID);
            });
            entryListRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                DeleteCharacterCall(charaID);
            });
            Color identifierColor = sortedUnits[i].colorChoice;
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
                    characterSummaryScreen.gameObject.SetActive(false);
                }
            });

            charaListEntry.SetActive(true);
        }
    }

    public void NewCharacterCall()
    {
        SetUnitMenuState(true);
        UnitManager._instance.ShowEntryScreen(0); //unitmanager.showentryscreen(0)
    }

    public void EditCharacterCall(int charaID)
    {
        SetUnitMenuState(true);
        UnitManager._instance.StartCharacterEditing(charaID);
    }

    public void DeleteCharacterCall(int charaID)
    {
        SetUnitMenuState(true);
        UnitManager._instance.DeleteCharaCall(charaID);
    }

    public void ToggleCharaListSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("charaListSort", 0) == 0;
        if (alphaSorting)
            PlayerPrefs.SetInt("charaListSort", 1);
        else
            PlayerPrefs.SetInt("charaListSort", 0);

        BuildCharaList();
    }

    public void ShowCharaDetails(int charaID)
    {
        List<EsperCharacter> characterUnits = UnitManager._instance.GetCharacters();

        for (int i = 0; i < characterUnits.Count; i++)
        {
            if (characterUnits[i].unitID == charaID)
            {
                EsperCharacter charaInfo = characterUnits[i];
                characterSummaryScreen.GetComponent<Image>().color = charaInfo.colorChoice;
                charSummaryName.text = charaInfo.unitName;

                characterSummaryScreen.gameObject.SetActive(true);

                break;
            }
        }
    }

    #endregion

    #region Foe Options

    private void BuildFoeList()
    {
        for (int i = foeScrollList.childCount - 1; i >= 1; i--)
        {
            Destroy(foeScrollList.GetChild(i).gameObject);
        }

        bool alphaSorting = PlayerPrefs.GetInt("foeListSort", 0) == 0;

        List<IconFoe> foeUnits = UnitManager._instance.GetFoes();

        List<IconFoe> sortedUnits = new List<IconFoe>();
        sortedUnits = foeUnits;
        if (alphaSorting)
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByName);
            foeSortButton.sprite = UnitManager._instance.sortAZSprite;
        }
        else
        {
            sortedUnits.Sort(MiscTools.CompareUnitsByNewerDate);
            foeSortButton.sprite = UnitManager._instance.sortTimeSprite;
        }

        int childCount = 0;
        for (int i = 0; i < sortedUnits.Count; i++)
        {
            GameObject foeListEntry = Instantiate<GameObject>(foeEntry, foeScrollList);
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
            else if (sortedUnits[i].type == FoeType.Legend)
                nameAdd = " <i><size=80%>(Legend)</i>";

            entryListRT.GetChild(0).GetComponent<TextMeshProUGUI>().text = sortedUnits[i].unitName + nameAdd;

            string[] details = UnitManager._instance.GetFoeDetails(sortedUnits[i]);

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
                EditFoeCall(foeID);
            });
            entryListRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate {
                DeleteFoeCall(foeID);
            });
            Color identifierColor = UnitManager._instance.unitHeavyColoring;
            if (sortedUnits[i].type == FoeType.SpecialSummon)
                identifierColor = UnitManager._instance.unitSummonColor;
            else if (sortedUnits[i].type == FoeType.Mob)
                identifierColor = UnitManager._instance.unitMobColor;
            else if (sortedUnits[i].classIndex == 1)
                identifierColor = UnitManager._instance.unitVagabondColoring;
            else if (sortedUnits[i].classIndex == 2)
                identifierColor = UnitManager._instance.unitLeaderColoring;
            else if (sortedUnits[i].classIndex == 3)
                identifierColor = UnitManager._instance.unitArtilleryColoring;

            entryListRT.GetChild(4).GetComponent<Image>().color = identifierColor;

            ShapeIcon entryPointer = entryListRT.GetComponent<ShapeIcon>();
            entryPointer.OnPointerEnterEvent.AddListener(delegate
            {
                ShowFoeDetails(foeID);
            });
            entryPointer.OnPointerExitEvent.AddListener(delegate
            {
                foeSummaryScreen.gameObject.SetActive(false);
            });

            foeListEntry.SetActive(true);
        }
    }

    public void NewFoeCall()
    {
        SetUnitMenuState(true);
        UnitManager._instance.ShowEntryScreen(1);
    }

    public void EditFoeCall(int foeID)
    {
        SetUnitMenuState(true);
        UnitManager._instance.StartFoeEditing(foeID);
    }

    public void DeleteFoeCall(int foeID)
    {
        SetUnitMenuState(true);
        UnitManager._instance.DeleteFoeCall(foeID);
    }

    public void ToggleFoeListSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("foeListSort", 0) == 0;

        if (alphaSorting)
            PlayerPrefs.SetInt("foeListSort", 1);
        else
            PlayerPrefs.SetInt("foeListSort", 0);

        BuildFoeList();
    }

    public void ShowFoeDetails(int foeID)
    {
        List<IconFoe> foeUnits = UnitManager._instance.GetFoes();

        for (int i = 0; i < foeUnits.Count; i++)
        {
            if (foeUnits[i].unitID == foeID)
            {
                IconFoe foeInfo = foeUnits[i];
                foeSummaryScreen.GetComponent<Image>().color = foeInfo.colorChoice;
                foeSummaryName.text = foeInfo.unitName;
                foeSummaryValues.text = "<b>Chapt " + foeInfo.level + "</b> - <i>" + MiscTools.GetSpacedForm(foeInfo.type.ToString()) + "</i>";

                string[] deets = UnitManager._instance.GetFoeDetails(foeUnits[i]);

                string additional = "";
                foeSummaryTactical.text = "<b>Tactical Combat</b><size=90%>\n\n· Class - <i>" + deets[0] + "</i>\n\n· Job - <i>" + deets[2] + " (" + deets[1] + ")" + "</i>";

                foeSummaryScreen.gameObject.SetActive(true);
            }
        }
    }

    #endregion

    #region Graphic Piece Editor

    public void PieceEditorCall()
    {
        GraphicPieceEditor.Instance.OpenPieceProcessFromMenu(pieceEditorColor);
    }

    #endregion
}
