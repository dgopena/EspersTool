using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

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

    private IconFoe buildingFoe;

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
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        #region factionless classes
        FoeData foeData = UnitManager._instance.foes;

        GameObject listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "General";

        listTitle.SetActive(true);

        GameObject listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        RectTransform listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        int matchCount = 0;

        #region mobs
        for (int i = 0; i < foeData.mobs.Length; i++)
        {
            if (foeData.mobs[i].factionIndex != 0)
                continue;

            Color entryColor = UnitManager._instance.unitMobColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.mobs[i].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Mob";

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.Mob, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }
        #endregion

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

            for(int c = 0; c < foeData.classes[i].jobs.Count; c++)
            {
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
            Color entryColor = UnitManager._instance.unitHeavyColoring;
            if (i == 1)
                entryColor = UnitManager._instance.unitVagabondColoring;
            else if (i == 2)
                entryColor = UnitManager._instance.unitLeaderColoring;
            else if (i == 3)
                entryColor = UnitManager._instance.unitArtilleryColoring;
            else
                entryColor = UnitManager._instance.unitLegendColoring;

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

        Vector2 bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region faction classes

        for (int f = 0; f < factionList.Length; f++) {

            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if(factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
                listTitle.transform.SetAsLastSibling();
                listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                listTitle.SetActive(true);

                listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
                listBodyTF = listBody.GetComponent<RectTransform>();
                listBodyTF.SetAsLastSibling();

                titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

                matchCount = 0;

                //mobs
                for (int j = 0; j < foeData.mobs.Length; j++)
                {
                    if (foeData.mobs[j].factionIndex != (f + 1))
                        continue;

                    Color entryColor = UnitManager._instance.unitMobColor;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.mobs[j].name;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Mob";

                    //tie creation event to entryTF holdbutton
                    int auxClassIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        MobSummonClick(FoeType.Mob, auxClassIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                //defaults
                for (int j = 0; j < factionData.defaults.Length; j++)
                {
                    if (factionData.defaults[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    Color entryColor = Color.white;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].typeRestriction.ToString();

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 4, factionIndex, auxJobIndex);
                    });
                    nuEntry.SetActive(true);

                    matchCount++;
                }

                //heavies
                for (int j = 0; j < factionData.heavies.Length; j++)
                {
                    if (factionData.heavies[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    Color entryColor = UnitManager._instance.unitHeavyColoring;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].typeRestriction.ToString();

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 0, factionIndex, auxJobIndex);
                    });
                    nuEntry.SetActive(true);

                    matchCount++;
                }

                //skirmishers
                for (int j = 0; j < factionData.skirmishers.Length; j++)
                {
                    if (factionData.skirmishers[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    Color entryColor = UnitManager._instance.unitVagabondColoring;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].typeRestriction.ToString();

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 1, factionIndex, auxJobIndex);
                    });
                    nuEntry.SetActive(true);

                    matchCount++;
                }

                //leaders
                for (int j = 0; j < factionData.leaders.Length; j++)
                {
                    if (factionData.leaders[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    Color entryColor = UnitManager._instance.unitLeaderColoring;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].typeRestriction.ToString();

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 2, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                //artilleries
                for (int j = 0; j < factionData.artilleries.Length; j++)
                {
                    if (factionData.artilleries[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    Color entryColor = UnitManager._instance.unitArtilleryColoring;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].typeRestriction.ToString();

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 3, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                bodySD = listBodyTF.sizeDelta;
                bodySD.y = matchCount * entryPixelSize;
                listBodyTF.sizeDelta = bodySD;
            }
        }
        #endregion

        #region summons

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Summons";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        FoeData summonData = UnitManager._instance.summons;

        for (int i = 0; i < summonData.specialSummons.Length; i++)
        {
            Color entryColor = UnitManager._instance.unitSummonColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            string summonName = summonData.specialSummons[i].name;

            int firstBracket = summonName.IndexOf('[') + 1;

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = summonName;

            if (firstBracket > 0)
            {
                int lastBracket = summonName.LastIndexOf(']');

                string summonerName = summonName.Substring(firstBracket, lastBracket - firstBracket);
                summonName = summonName.Substring(0, firstBracket - 1);
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Summon for " + summonerName;
            }
            else
            {
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            }

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.SpecialSummon, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;
        #endregion

        PlayerPrefs.SetInt("FoePresetSort", 0);
    }

    private void ArrangeByClass()
    {
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        #region Mobs

        FoeData foeData = UnitManager._instance.foes;

        GameObject listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Mobs";

        listTitle.SetActive(true);

        GameObject listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        RectTransform listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        int matchCount = 0;

        for (int i = 0; i < foeData.mobs.Length; i++)
        {
            Color mobColor = UnitManager._instance.unitMobColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = mobColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.mobs[i].name;

            if (foeData.mobs[i].factionIndex == 0)
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            else
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionList[foeData.mobs[i].factionIndex - 1].globalFactionName;

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.Mob, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        Vector2 bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Heavies

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Heavies";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        //foes
        Color entryColor = UnitManager._instance.unitHeavyColoring;

        for (int c = 0; c < foeData.classes[0].jobs.Count; c++)
        {
            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[0].jobs[c].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Foe";

            //tie creation event to entryTF holdbutton
            int auxJobIndex = c;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Foe, 0, auxJobIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        //elite
        GameObject nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        Transform entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.eliteClasses[0].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Elite";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Elite, 0, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        //legend
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[0].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Legend";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Legend, 0, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionList[f].foeFactions[i].heavies.Length; j++)
                {
                    if (factionList[f].foeFactions[i].heavies[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].typeRestriction.ToString() + " " + factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 0, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Skirmishers

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Skirmishers";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        //foes
        entryColor = UnitManager._instance.unitVagabondColoring;

        for (int c = 0; c < foeData.classes[1].jobs.Count; c++)
        {
            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[1].jobs[c].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Foe";

            //tie creation event to entryTF holdbutton
            int auxJobIndex = c;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Foe, 1, auxJobIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        //elite
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.eliteClasses[1].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Elite";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Foe, 1, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        //legend
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[1].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Legend";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Legend, 1, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionList[f].foeFactions[i].skirmishers.Length; j++)
                {
                    if (factionList[f].foeFactions[i].skirmishers[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].typeRestriction.ToString() + " " + factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 1, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Leaders

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Leaders";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        //foes
        entryColor = UnitManager._instance.unitLeaderColoring;

        for (int c = 0; c < foeData.classes[2].jobs.Count; c++)
        {
            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[2].jobs[c].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Foe";

            //tie creation event to entryTF holdbutton
            int auxJobIndex = c;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Foe, 2, auxJobIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        //elite
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.eliteClasses[2].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Elite";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Elite, 2, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        //legend
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[2].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Legend";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Legend, 2, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionList[f].foeFactions[i].leaders.Length; j++)
                {
                    if (factionList[f].foeFactions[i].leaders[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].typeRestriction.ToString() + " " + factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 2, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Artilleries

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Artilleries";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        //foes
        entryColor = UnitManager._instance.unitArtilleryColoring;

        for (int c = 0; c < foeData.classes[3].jobs.Count; c++)
        {
            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[3].jobs[c].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Foe";

            //tie creation event to entryTF holdbutton

            nuEntry.SetActive(true);
            int auxJobIndex = c;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Foe, 3, auxJobIndex);
            });

            matchCount++;
        }

        //elite
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.eliteClasses[3].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Elite";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Elite, 3, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        //legend
        nuEnt = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
        entTF = nuEnt.transform;
        entTF.SetAsLastSibling();

        entTF.GetChild(0).GetComponent<Image>().color = entryColor;
        entTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[3].name;
        entTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Legend";

        //tie creation event to entryTF holdbutton
        entTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
            CreateByClassJobIndex(FoeType.Legend, 3, 0);
        });

        nuEnt.SetActive(true);

        matchCount++;

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionList[f].foeFactions[i].artilleries.Length; j++)
                {
                    if (factionList[f].foeFactions[i].artilleries[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].typeRestriction.ToString() + " " + factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 3, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Defaults

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Legends and Others";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        //foes
        entryColor = UnitManager._instance.unitLegendColoring;

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionList[f].foeFactions[i].defaults.Length; j++)
                {
                    if (factionList[f].foeFactions[i].defaults[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].typeRestriction.ToString() + " " + factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    FoeType typif = FoeType.Foe;
                    if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Elite)
                        typif = FoeType.Elite;
                    else if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Legend)
                        typif = FoeType.Legend;

                    int auxClassIndex = i;
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(typif, 4, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Summons

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Summons";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        FoeData summonData = UnitManager._instance.summons;

        for (int i = 0; i < summonData.specialSummons.Length; i++)
        {
            entryColor = UnitManager._instance.unitSummonColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            string summonName = summonData.specialSummons[i].name;

            int firstBracket = summonName.IndexOf('[') + 1;
            int lastBracket = summonName.LastIndexOf(']');

            string summonerName = summonName.Substring(firstBracket, lastBracket - firstBracket);
            summonName = summonName.Substring(0, firstBracket - 1);

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = summonName;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Summon for " + summonerName;

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.SpecialSummon, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        PlayerPrefs.SetInt("FoePresetSort", 1);
    }

    private void ArrangeByType()
    {
        titleIDListDict = new Dictionary<int, GameObject>();

        ClearContentList();

        #region Mobs

        FoeData foeData = UnitManager._instance.foes;

        GameObject listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Mobs";

        listTitle.SetActive(true);

        GameObject listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        RectTransform listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        int matchCount = 0;

        for (int i = 0; i < foeData.mobs.Length; i++)
        {
            Color mobColor = UnitManager._instance.unitMobColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = mobColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.mobs[i].name;

            if (foeData.mobs[i].factionIndex == 0)
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            else
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionList[foeData.mobs[i].factionIndex - 1].globalFactionName;

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.Mob, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        Vector2 bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Foes

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Foes";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        for (int i = 0; i < foeData.classes.Count; i++)
        {
            for (int c = 0; c < foeData.classes[i].jobs.Count; c++)
            {
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
                entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.classes[i].jobs[c].name;
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";

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

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionData.heavies.Length; j++)
                {
                    if (factionData.heavies[j].typeRestriction != FoeData.EnemyType.Foe)
                        continue;

                    if (factionData.heavies[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitHeavyColoring; ;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Foe, 0, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.skirmishers.Length; j++)
                {
                    if (factionData.skirmishers[j].typeRestriction != FoeData.EnemyType.Foe)
                        continue;

                    if (factionData.skirmishers[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitVagabondColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Foe, 1, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.leaders.Length; j++)
                {
                    if (factionData.leaders[j].typeRestriction != FoeData.EnemyType.Foe)
                        continue;

                    if (factionData.leaders[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitLeaderColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Foe, 2, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.artilleries.Length; j++)
                {
                    if (factionData.artilleries[j].typeRestriction != FoeData.EnemyType.Foe)
                        continue;

                    if (factionData.artilleries[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitArtilleryColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Foe, 3, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.defaults.Length; j++)
                {
                    if (factionData.defaults[j].typeRestriction != FoeData.EnemyType.Foe)
                        continue;

                    if (factionData.defaults[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = Color.grey;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Foe, 4, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Elite

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Elites";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        for (int i = 0; i < foeData.eliteClasses.Count; i++)
        {
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
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            int auxJobIndex = 0;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Elite, auxClassIndex, auxJobIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionData.heavies.Length; j++)
                {
                    if (factionData.heavies[j].typeRestriction != FoeData.EnemyType.Elite)
                        continue;

                    if (factionData.heavies[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitHeavyColoring; ;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Elite, 0, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.skirmishers.Length; j++)
                {
                    if (factionData.skirmishers[j].typeRestriction != FoeData.EnemyType.Elite)
                        continue;

                    if (factionData.skirmishers[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitVagabondColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Elite, 1, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.leaders.Length; j++)
                {
                    if (factionData.leaders[j].typeRestriction != FoeData.EnemyType.Elite)
                        continue;

                    if (factionData.leaders[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitLeaderColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Elite, 2, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.artilleries.Length; j++)
                {
                    if (factionData.artilleries[j].typeRestriction != FoeData.EnemyType.Elite)
                        continue;

                    if (factionData.artilleries[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitArtilleryColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Elite, 3, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.defaults.Length; j++)
                {
                    if (factionData.defaults[j].typeRestriction != FoeData.EnemyType.Elite)
                        continue;

                    if (factionData.defaults[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = Color.grey;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Elite, 4, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Legend

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Legends";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        // ---GENERAL

        for (int i = 0; i < foeData.legendClasses.Count; i++)
        {
            Color entryColor = UnitManager._instance.unitLegendColoring;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.legendClasses[i].name;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                CreateByClassJobIndex(FoeType.Legend, auxClassIndex, 0);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        // ---FACTIONED
        for (int f = 0; f < factionList.Length; f++)
        {
            for (int i = 0; i < factionList[f].foeFactions.Length; i++)
            {
                FoeFaction factionData = factionList[f].foeFactions[i];

                int factionIndex = f - 1;
                if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                {
                    factionIndex = 8 + i;
                }

                for (int j = 0; j < factionData.heavies.Length; j++)
                {
                    if (factionData.heavies[j].typeRestriction != FoeData.EnemyType.Legend)
                        continue;

                    if (factionData.heavies[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitHeavyColoring; ;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Legend, 0, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.skirmishers.Length; j++)
                {
                    if (factionData.skirmishers[j].typeRestriction != FoeData.EnemyType.Legend)
                        continue;

                    if (factionData.skirmishers[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitVagabondColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Legend, 1, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.leaders.Length; j++)
                {
                    if (factionData.leaders[j].typeRestriction != FoeData.EnemyType.Legend)
                        continue;

                    if (factionData.leaders[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitLeaderColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Legend, 2, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.artilleries.Length; j++)
                {
                    if (factionData.artilleries[j].typeRestriction != FoeData.EnemyType.Legend)
                        continue;

                    if (factionData.artilleries[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = UnitManager._instance.unitArtilleryColoring;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Legend, 3, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }

                for (int j = 0; j < factionData.defaults.Length; j++)
                {
                    if (factionData.defaults[j].typeRestriction != FoeData.EnemyType.Legend)
                        continue;

                    if (factionData.defaults[j].chapterLimitNum > presetChapterChoice)
                        continue;

                    GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                    Transform entryTF = nuEntry.transform;
                    entryTF.SetAsLastSibling();

                    entryTF.GetChild(0).GetComponent<Image>().color = Color.grey;
                    entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.factionName;

                    //tie creation event to entryTF holdbutton
                    int auxJobIndex = j;
                    entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                        CreateByClassFactionIndex(FoeType.Legend, 4, factionIndex, auxJobIndex);
                    });

                    nuEntry.SetActive(true);

                    matchCount++;
                }
            }
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

        #region Summons

        listTitle = GameObject.Instantiate<GameObject>(listTitlePrefab, container);
        listTitle.transform.SetAsLastSibling();
        listTitle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Summons";

        listTitle.SetActive(true);

        listBody = GameObject.Instantiate<GameObject>(listElementPrefab, container);
        listBodyTF = listBody.GetComponent<RectTransform>();
        listBodyTF.SetAsLastSibling();

        titleIDListDict.Add(listTitle.GetInstanceID(), listBody);

        matchCount = 0;

        FoeData summonData = UnitManager._instance.summons;

        for (int i = 0; i < summonData.specialSummons.Length; i++)
        {
            Color entryColor = UnitManager._instance.unitSummonColor;

            GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
            Transform entryTF = nuEntry.transform;
            entryTF.SetAsLastSibling();

            string summonName = summonData.specialSummons[i].name;

            int firstBracket = summonName.IndexOf('[') + 1;
            int lastBracket = summonName.LastIndexOf(']');

            string summonerName = summonName.Substring(firstBracket, lastBracket - firstBracket);
            summonName = summonName.Substring(0, firstBracket - 1);

            entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
            entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = summonName;
            entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Summon for " + summonerName;

            //tie creation event to entryTF holdbutton
            int auxClassIndex = i;
            entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                MobSummonClick(FoeType.SpecialSummon, auxClassIndex);
            });

            nuEntry.SetActive(true);

            matchCount++;
        }

        bodySD = listBodyTF.sizeDelta;
        bodySD.y = matchCount * entryPixelSize;
        listBodyTF.sizeDelta = bodySD;

        #endregion

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

            #region Mobs

            for (int i = 0; i < foeData.mobs.Length; i++)
            {
                if (!foeData.mobs[i].name.StartsWith(comparisonChar))
                    continue;

                Color mobColor = UnitManager._instance.unitMobColor;

                GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                Transform entryTF = nuEntry.transform;
                entryTF.SetAsLastSibling();

                entryTF.GetChild(0).GetComponent<Image>().color = mobColor;
                entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.mobs[i].name;

                if (foeData.mobs[i].factionIndex == 0)
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                else
                    entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionList[foeData.mobs[i].factionIndex - 1].globalFactionName;

                //tie creation event to entryTF holdbutton
                int auxClassIndex = i;
                entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                    MobSummonClick(FoeType.Mob, auxClassIndex);
                });

                nuEntry.SetActive(true);

                matchCount++;
            }

            #endregion

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

            #region faction classes

            for (int f = 0; f < factionList.Length; f++)
            {
                for (int i = 0; i < factionList[f].foeFactions.Length; i++)
                {
                    FoeFaction factionData = factionList[f].foeFactions[i];

                    int factionIndex = f - 1;
                    if (factionIndex < 0) //then its the folkfaction, to which we need to add the other subfaction indeces (chronicler, churner, etc)
                    {
                        factionIndex = 8 + i;
                    }

                    //defaults
                    for (int j = 0; j < factionData.defaults.Length; j++)
                    {
                        if (!factionData.defaults[j].templateName.StartsWith(comparisonChar))
                            continue;

                        if (factionData.defaults[j].chapterLimitNum > presetChapterChoice)
                            continue;

                        Color entryColor = Color.gray;

                        GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                        Transform entryTF = nuEntry.transform;
                        entryTF.SetAsLastSibling();

                        entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].templateName;
                        entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.defaults[j].typeRestriction.ToString() + " " + factionData.factionName;

                        //tie creation event to entryTF holdbutton
                        FoeType typif = FoeType.Foe;
                        if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Elite)
                            typif = FoeType.Elite;
                        else if (factionData.defaults[j].typeRestriction == FoeData.EnemyType.Legend)
                            typif = FoeType.Legend;

                        int auxClassIndex = i;
                        int auxJobIndex = j;
                        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                            CreateByClassFactionIndex(typif, 4, factionIndex, auxJobIndex);
                        });

                        nuEntry.SetActive(true);

                        matchCount++;
                    }

                    //heavies
                    for (int j = 0; j < factionData.heavies.Length; j++)
                    {
                        if (!factionData.heavies[j].templateName.StartsWith(comparisonChar))
                            continue;

                        if (factionData.heavies[j].chapterLimitNum > presetChapterChoice)
                            continue;

                        Color entryColor = UnitManager._instance.unitHeavyColoring;

                        GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                        Transform entryTF = nuEntry.transform;
                        entryTF.SetAsLastSibling();

                        entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].templateName;
                        entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.heavies[j].typeRestriction.ToString() + " " + factionData.factionName;

                        //tie creation event to entryTF holdbutton
                        FoeType typif = FoeType.Foe;
                        if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Elite)
                            typif = FoeType.Elite;
                        else if (factionData.heavies[j].typeRestriction == FoeData.EnemyType.Legend)
                            typif = FoeType.Legend;

                        int auxClassIndex = i;
                        int auxJobIndex = j;
                        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                            CreateByClassFactionIndex(typif, 0, factionIndex, auxJobIndex);
                        });

                        nuEntry.SetActive(true);

                        matchCount++;
                    }

                    //skirmishers
                    for (int j = 0; j < factionData.skirmishers.Length; j++)
                    {
                        if (!factionData.skirmishers[j].templateName.StartsWith(comparisonChar))
                            continue;

                        if (factionData.skirmishers[j].chapterLimitNum > presetChapterChoice)
                            continue;

                        Color entryColor = UnitManager._instance.unitVagabondColoring;

                        GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                        Transform entryTF = nuEntry.transform;
                        entryTF.SetAsLastSibling();

                        entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].templateName;
                        entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.skirmishers[j].typeRestriction.ToString() + " " + factionData.factionName;

                        //tie creation event to entryTF holdbutton
                        FoeType typif = FoeType.Foe;
                        if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Elite)
                            typif = FoeType.Elite;
                        else if (factionData.skirmishers[j].typeRestriction == FoeData.EnemyType.Legend)
                            typif = FoeType.Legend;

                        int auxClassIndex = i;
                        int auxJobIndex = j;
                        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                            CreateByClassFactionIndex(typif, 1, factionIndex, auxJobIndex);
                        });

                        nuEntry.SetActive(true);

                        matchCount++;
                    }

                    //leaders
                    for (int j = 0; j < factionData.leaders.Length; j++)
                    {
                        if (!factionData.leaders[j].templateName.StartsWith(comparisonChar))
                            continue;

                        if (factionData.leaders[j].chapterLimitNum > presetChapterChoice)
                            continue;

                        Color entryColor = UnitManager._instance.unitLeaderColoring;

                        GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                        Transform entryTF = nuEntry.transform;
                        entryTF.SetAsLastSibling();

                        entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].templateName;
                        entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.leaders[j].typeRestriction.ToString() + " " + factionData.factionName;

                        //tie creation event to entryTF holdbutton
                        FoeType typif = FoeType.Foe;
                        if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Elite)
                            typif = FoeType.Elite;
                        else if (factionData.leaders[j].typeRestriction == FoeData.EnemyType.Legend)
                            typif = FoeType.Legend;

                        int auxClassIndex = i;
                        int auxJobIndex = j;
                        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                            CreateByClassFactionIndex(typif, 2, factionIndex, auxJobIndex);
                        });

                        nuEntry.SetActive(true);

                        matchCount++;
                    }

                    //artilleries
                    for (int j = 0; j < factionData.artilleries.Length; j++)
                    {
                        if (!factionData.artilleries[j].templateName.StartsWith(comparisonChar))
                            continue;

                        if (factionData.artilleries[j].chapterLimitNum > presetChapterChoice)
                            continue;

                        Color entryColor = UnitManager._instance.unitArtilleryColoring;

                        GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                        Transform entryTF = nuEntry.transform;
                        entryTF.SetAsLastSibling();

                        entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                        entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].templateName;
                        entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = factionData.artilleries[j].typeRestriction.ToString() + " " + factionData.factionName;

                        //tie creation event to entryTF holdbutton
                        FoeType typif = FoeType.Foe;
                        if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Elite)
                            typif = FoeType.Elite;
                        else if (factionData.artilleries[j].typeRestriction == FoeData.EnemyType.Legend)
                            typif = FoeType.Legend;

                        int auxClassIndex = i;
                        int auxJobIndex = j;
                        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                            CreateByClassFactionIndex(typif, 3, factionIndex, auxJobIndex);
                        });

                        nuEntry.SetActive(true);

                        matchCount++;
                    }

                    //specials
                }
            }
            #endregion

            #region Summons

            for (int i = 0; i < foeData.specialSummons.Length; i++)
            {
                string summonName = foeData.specialSummons[i].name;
                if (!summonName.StartsWith(comparisonChar))
                    continue;

                Color entryColor = UnitManager._instance.unitSummonColor;

                GameObject nuEntry = GameObject.Instantiate<GameObject>(listEntryPrefab, listBodyTF);
                Transform entryTF = nuEntry.transform;
                entryTF.SetAsLastSibling();


                int firstBracket = summonName.IndexOf('[') + 1;
                int lastBracket = summonName.LastIndexOf(']');

                string summonerName = summonName.Substring(firstBracket, lastBracket - firstBracket);
                summonName = summonName.Substring(0, firstBracket - 1);

                entryTF.GetChild(0).GetComponent<Image>().color = entryColor;
                entryTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = summonName;
                entryTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Summon for " + summonerName;

                //tie creation event to entryTF holdbutton
                int auxClassIndex = i;
                entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate {
                    MobSummonClick(FoeType.SpecialSummon, auxClassIndex);
                });

                nuEntry.SetActive(true);

                matchCount++;
            }

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

    private void MobSummonClick(FoeType type, int index)
    {
        buildingFoe = UnitManager._instance.GetFoeBase(type);

        buildingFoe.level = presetChapterChoice;

        buildingFoe.colorChoice = Color.white;

        buildingFoe.classIndex = index;

        if (type == FoeType.Mob)
            buildingFoe.factionIndex = UnitManager._instance.foes.mobs[index].factionIndex - 1;

        UnitManager._instance.ReceiveFoePreset(buildingFoe);
    }

    private void CreateByClassJobIndex(FoeType type, int classIndex, int jobIndex)
    {
        buildingFoe = UnitManager._instance.GetFoeBase(type);

        buildingFoe.level = presetChapterChoice;

        buildingFoe.colorChoice = Color.white;

        buildingFoe.classIndex = classIndex;

        buildingFoe.jobIndex = jobIndex;

        buildingFoe.factionIndex = -1;

        buildingFoe.templateIndex = -1;

        buildingFoe.isDefaultFactionEntry = (classIndex == 4);

        UnitManager._instance.ReceiveFoePreset(buildingFoe);
    }

    private void CreateByClassFactionIndex(FoeType type, int classIndex, int factionIndex, int subFactionIndex)
    {
        buildingFoe = UnitManager._instance.GetFoeBase(type);

        buildingFoe.level = presetChapterChoice;

        buildingFoe.colorChoice = Color.white;

        buildingFoe.classIndex = classIndex;

        buildingFoe.jobIndex = 0;

        buildingFoe.factionIndex = factionIndex;

        buildingFoe.subFactionIndex = subFactionIndex;

        buildingFoe.templateIndex = -1;

        buildingFoe.isDefaultFactionEntry = (classIndex == 4);

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
