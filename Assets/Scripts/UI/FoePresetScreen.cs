using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//quite possibly this class will be useful in the future, but will need major rework. for now, displaying a list based on names will suffice

public class FoePresetScreen : MonoBehaviour
{
    [SerializeField] private GameObject listTitlePrefab;
    [SerializeField] private GameObject listElementPrefab;
    [SerializeField] private GameObject listEntryPrefab;
    [SerializeField] private RectTransform container;

    [SerializeField] private TextMeshProUGUI presetChapterText;
    private int presetChapterChoice = 1;

    [Space(10f)]
    [SerializeField] private RectTransform sortButton;
    private bool sortListOpen = false;
    private RectTransform sortListRT;

    [Space(10f)]
    [SerializeField] private float entryPixelSize = 65f;
    [SerializeField] private Sprite arrowRight;
    [SerializeField] private Sprite arrowDown;

    private bool presetListBuilt = false;

    Dictionary<int, GameObject> titleIDListDict;

    [Space(10f)]
    [SerializeField] private FactionData[] factionList;

    private EsperFoe buildingFoe;

    private void OnEnable()
    {
        int lastOrderUsed = PlayerPrefs.GetInt("FoePresetSort", 0);

        if (!presetListBuilt)
            BuildPresetList(lastOrderUsed);
    }

    private void LateUpdate()
    {
        if (sortListOpen && Input.GetMouseButtonDown(0))
        {
            if (!TooltipManager.CheckMouseInArea(sortListRT))
            {
                SortButtonClose();
            }
        }
    }

    private void BuildPresetList(int orderType) // 0 - by factions, 1 - by class, 2 - by type, 3 - by name?
    {
        switch (orderType)
        {
            case 0:
                ArrangeByFaction();
                break;
            case 1:
                ArrangeByClass();
                break;

            case 2:
                ArrangeByType();
                break;
            case 3:
                ArrangeByName();
                break;
        }

        presetListBuilt = true;
    }

    //the arranging methods are super unorganized and very unelegant but god damn i no longer give a shit

    private void ArrangeByFaction()
    {
        //maybe

        PlayerPrefs.SetInt("FoePresetSort", 0);
    }

    private void ArrangeByClass()
    {
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        //very possible

        PlayerPrefs.SetInt("FoePresetSort", 1);
    }

    private void ArrangeByType()
    {
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        //who knows

        PlayerPrefs.SetInt("FoePresetSort", 2);
    }

    private void ArrangeByName()
    {
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        int charStart = 65;
        int charEnd = 90;

        for(int h = charStart; h <= charEnd; h++)
        {
            char comparisonChar = (char)h;

            FoeData foeData = UnitManager._instance.foes;

            GameObject listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
            listTitle.transform.SetAsLastSibling();
            listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = comparisonChar.ToString();

            GameObject listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
            RectTransform listBodyTF = listBody.GetComponent<RectTransform>();
            listBodyTF.SetAsLastSibling();

            titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

            int matchCount = 0;

            #region factionless classes

            #region regular foes
            for (int i = 0; i < foeData.classes.Count; i++)
            {
                Color entryColor = UnitManager._instance.unitHeavyColoring;
                if (i == 1)
                    entryColor = UnitManager._instance.unitVagabondColoring;
                else if (i == 2)
                    entryColor = UnitManager._instance.unitLeaderColoring;
                else if (i == 3)
                    entryColor = UnitManager._instance.unitArtilleryColoring;

                for (int c = 0; c < foeData.classes[i].jobs.Count; c++)
                {
                    if (!foeData.classes[i].jobs[c].name.StartsWith(comparisonChar))
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[i].jobs[c].name;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Foe";

                    //tie creation event to entryTF holdbutton
                    int auxClassIndex = i;
                    int auxJobIndex = c;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassJobIndex(FoeType.Foe, auxClassIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
            #endregion

            #region elite foes
            for (int i = 0; i < foeData.eliteClasses.Count; i++)
            {
                if (!foeData.eliteClasses[i].name.StartsWith(comparisonChar))
                    continue;

                Color entryColor = UnitManager._instance.unitHeavyColoring;
                if (i == 1)
                    entryColor = UnitManager._instance.unitVagabondColoring;
                else if (i == 2)
                    entryColor = UnitManager._instance.unitLeaderColoring;
                else if (i == 3)
                    entryColor = UnitManager._instance.unitArtilleryColoring;

                GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                Transform entryTF = nuEntry.transform;
                entryTF.SetAsLastSibling();

                entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.eliteClasses[i].name;
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Elite";

                //tie creation event to entryTF holdbutton
                int auxClassIndex = i;
                entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                    CreateByClassJobIndex(FoeType.Elite, auxClassIndex, 0);
                });

                nuEntry.SetActive(true);

                matchCount++;
            }
            #endregion

            #region legendary foes
            for (int i = 0; i < foeData.legendClasses.Count; i++)
            {
                if (!foeData.legendClasses[i].name.StartsWith(comparisonChar))
                    continue;

                Color entryColor = UnitManager._instance.unitHeavyColoring;
                if (i == 1)
                    entryColor = UnitManager._instance.unitVagabondColoring;
                else if (i == 2)
                    entryColor = UnitManager._instance.unitLeaderColoring;
                else if (i == 3)
                    entryColor = UnitManager._instance.unitArtilleryColoring;

                GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                Transform entryTF = nuEntry.transform;
                entryTF.SetAsLastSibling();

                entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[i].name;
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Legend";

                //tie creation event to entryTF holdbutton
                int auxClassIndex = i;
                entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                    CreateByClassJobIndex(FoeType.Legend, auxClassIndex, 0);
                });

                nuEntry.SetActive(true);

                matchCount++;
            }
            #endregion

            #endregion

            listTitle.SetActive(matchCount > 0); //only available if there were results

            Vector2 bodySD = listBodyTF.sizeDelta;
            bodySD.y = matchCount * entryPixelSize;
            listBodyTF.sizeDelta = bodySD;
        }

        PlayerPrefs.SetInt("FoePresetSort", 3);
    }

    public void ClearContentList()
    {
        for (int i = container.childCount - 1; i >= 2; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }
    }

    public void ArrowClick(RectTransform buttonRect)
    {
        int idKey = buttonRect.gameObject.GetInstanceID();

        GameObject targetList = titleIDListDict[idKey];

        if (targetList.activeInHierarchy)
        {
            buttonRect.GetChild(1).GetChild(0).GetComponent<Image>().sprite = arrowRight;
            targetList.SetActive(false);
        }
        else
        {
            buttonRect.GetChild(1).GetChild(0).GetComponent<Image>().sprite = arrowDown;
            targetList.SetActive(true);
        }
    }

    public void SortButtonOpen()
    {
        ListPanel listPanel = UnitManager._instance.listPanel;

        listPanel.screenProportionSize = UnitManager._instance.kinCulturePanelProportions;
        listPanel.listColor = 0.9f * UnitManager._instance.foeEntryScreen.transform.GetChild(0).GetComponent<Image>().color;

        Vector3 listOrigin = sortButton.position + (0.5f * sortButton.rect.size.x * sortButton.lossyScale.x * Vector3.right);

        List<string> foeClassTypeNames = new List<string>();

        foeClassTypeNames.Add("Faction");
        foeClassTypeNames.Add("Class");
        foeClassTypeNames.Add("Type");
        foeClassTypeNames.Add("Initial");

        listPanel.ShowPanel(listOrigin, foeClassTypeNames, true);
        sortListOpen = true;
        listPanel.OnEntryClick += ChangeSortMode;

        sortListRT = listPanel.GetComponent<RectTransform>();
    }

    public void SortButtonClose()
    {
        sortListOpen = false;
        UnitManager._instance.listPanel.ShowPanel(false);
        UnitManager._instance.listPanel.OnEntryClick -= ChangeSortMode;
    }

    public void ChangeSortMode(int entryClick)
    {
        BuildPresetList(entryClick);

        SortButtonClose();
    }

    private void CreateByClassJobIndex(FoeType type, int classIndex, int jobIndex)
    {
        buildingFoe = UnitManager._instance.GetFoeBase(type);

        buildingFoe.level = presetChapterChoice;

        buildingFoe.colorChoice = Color.white;

        buildingFoe.classIndex = classIndex;

        UnitManager._instance.ReceiveFoePreset(buildingFoe);
    }

    public void ChangePresetLevel(bool forward)
    {
        int auxChoice = presetChapterChoice;

        presetChapterChoice += forward ? 1 : -1;

        presetChapterChoice = Math.Clamp(presetChapterChoice, 1, 3);

        if (auxChoice != presetChapterChoice)
        {
            presetChapterText.text = "Chapter " + presetChapterChoice;

            int lastOrderUsed = PlayerPrefs.GetInt("FoePresetSort", 0);

            BuildPresetList(lastOrderUsed);

            NotificationSystem.Instance.PushNotification("Showing foes available for chapter " + presetChapterChoice);
        }
    }
}
