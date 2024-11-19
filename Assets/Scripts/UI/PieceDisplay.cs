using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PieceDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image buttonBackPanel;
    [SerializeField] private Image statBackPanel;
    [SerializeField] private Image hpBackPanel;

    [SerializeField] private Image statPanelCover;
    [SerializeField] private Image hpVigorCover;
    [SerializeField] private Image backCover;

    [SerializeField] private GameObject gameModeTools;
    public GameObject unitPanelButton;

    [Space(5f)]
    [SerializeField] private PieceReticle reticle;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI nameLabel;

    [Space(10f)]
    [SerializeField] private Animator barChangeAnim;
    [SerializeField] private NotchBar hpBar;
    [SerializeField] private TextMeshProUGUI hpLabel;
    [SerializeField] private NotchBar vigorBar;
    [SerializeField] private TextMeshProUGUI vigorLabel;
    [SerializeField] private GameObject minusHPButton;
    [SerializeField] private GameObject plusHPButton;
    [SerializeField] private GameObject minusVigorButton;
    [SerializeField] private GameObject plusVigorButton;
    [Space(5f)]
    [SerializeField] private float miniPanelHoldInitialCooldown = 1f;
    [SerializeField] private float miniPanelHoldConstantCooldown = 0.2f;
    private bool miniPanelHolding = false;
    private float currentHoldCooldown = 0f;

    [Space(10f)]
    [SerializeField] private ElixirList woundCounter;

    [Space(10f)]
    [SerializeField] private GameObject phaseCounter;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI vitStat;
    [SerializeField] private TextMeshProUGUI speedDashStat;
    [SerializeField] private TextMeshProUGUI defenseStat;
    [SerializeField] private TextMeshProUGUI frayStat;
    [SerializeField] private TextMeshProUGUI damageDieStat;
    [SerializeField] private TextMeshProUGUI basicAttackStat; //turn this off if foe

    [Space(10f)]
    [SerializeField] private RectTransform panelTools;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI secondButtonLabel;
    [SerializeField] private Color possitiveEffectColor;
    [SerializeField] private Color statusEffectColor;
    [SerializeField] private StatusList statusList;
    [SerializeField] private StatusList effectList;

    [Space(10f)]
    [SerializeField] private RectTransform blessingButton;
    [SerializeField] private Color blessingColor;

    [Space(10f)]
    [SerializeField] private Animator traitEffectBlockAnim;
    [SerializeField] private RectTransform traitEffectBlockContent;
    [SerializeField] private RectTransform traitEffectEntryPrefab;
    [SerializeField] private RectTransform traitEffectDescPanel;
    [SerializeField] private TextMeshProUGUI traitEffectText;
    private float traitEffectListSpacing;

    private List<EntryDescPair> traitEntries;
    private int currentTraitSelectedIndex = -1;
    private bool traitEffectListIsShown = false;

    [Space(5f)]
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

    [SerializeField] private GameObject abilityButton;

    [Header("Data")]
    [SerializeField] private StatusData statusInfo;
    [SerializeField] private TraitEntry[] traitsBase;
    [SerializeField] private AbilityEntry[] abilityBase;

    private struct EntryDescPair
    {
        public GameObject uiEntry;
        public string description;
    }

    private IconUnit activeUnit;
    private UnitPiece activePiece;

    public UnitPiece pieceDisplayed => activePiece;

    private CharacterPiece activeCharaPiece;
    private FoePiece activeFoePiece;

    private bool refreshTraitFlag = true;
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

        abilityButton.SetActive(false);

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

        abilityButton.SetActive(true);

        BuildDisplay();
    }

    public void SetPanelToolsMode(int md) //0 - game normal, 1 - game seer, 2 - edit
    {
        if(md == 2)
            unitPanelButton.gameObject.SetActive(false);

        panelTools.GetChild(0).gameObject.SetActive(md == 2);
        panelTools.GetChild(1).gameObject.SetActive(md < 2);

        panelTools.GetChild(1).GetChild(3).gameObject.SetActive(md == 1);
    }

    public void CloseDisplayPanel()
    {
        CloseStatusEffectLists();
        unitPanelButton.gameObject.SetActive(true);
    }

    private void BuildDisplay()
    {
        unitPanelButton.gameObject.SetActive(false);
        UnitManager._instance.SetUnitMenu(false);

        buttonBackPanel.color = activeUnit.colorChoice;
        statBackPanel.color = activeUnit.colorChoice;
        hpBackPanel.color = activeUnit.colorChoice;

        nameLabel.text = activeUnit.unitName;

        int currentMaxHP = (activeUnit.hp + activeUnit.addedHP);
        if (activeCharaPiece != null)
        {
            currentMaxHP = Mathf.FloorToInt((0.25f * (4 - activeCharaPiece.woundCount)) * (activeUnit.hp + activeUnit.addedHP));
        }

        hpBar.SetBar(currentMaxHP);
        vigorBar.SetBar(activeUnit.vigor);
        UpdateHealthBars(true);

        if (activeUnit.currentVigor != 0)
            barChangeAnim.SetTrigger("SetVigorShown");
        else
            barChangeAnim.SetTrigger("SetVigorHidden");

        //stat fill
        vitStat.text = Mathf.CeilToInt((float)activeUnit.hp / 4f).ToString(); // activeUnit.vitality.ToString();
        speedDashStat.text = activeUnit.speed + "/" + activeUnit.dash;
        defenseStat.text = activeUnit.defense.ToString();
        frayStat.text = activeUnit.frayDamage.ToString();
        damageDieStat.text = activeUnit.damage;

        //status and effects
        statusList.ClearIcons();
        effectList.ClearIcons();

        statusList.ignoreUpdateFlag = true;
        effectList.ignoreUpdateFlag = true;

        for (int i = 0; i < activeUnit.activeStatus.Count; i++)
        {
            statusList.AddStatus(activeUnit.activeStatus[i]);
        }
        for (int i = 0; i < activeUnit.activePositiveEffects.Count; i++)
        {
            effectList.AddPositiveEffect(activeUnit.activePositiveEffects[i]);
        }

        statusList.ignoreUpdateFlag = false;
        effectList.ignoreUpdateFlag = false;

        SetBlessButtonState();

        //differing setup
        if (activeCharaPiece != null)
        {
            basicAttackStat.text = activeUnit.attackType;
            basicAttackStat.transform.parent.parent.gameObject.SetActive(true);

            woundCounter.gameObject.SetActive(true);
            woundCounter.SetIconSlotCount(4);
            woundCounter.SetFillCount(activeCharaPiece.woundCount);

            phaseCounter.SetActive(false);
        }
        else if (activeFoePiece != null)
        {
            basicAttackStat.transform.parent.parent.gameObject.SetActive(false);

            woundCounter.gameObject.SetActive(false);

            if (activeFoePiece.foeData.factionIndex >= 0)
            {
                SubFaction sbf = activeFoePiece.foeData.GetFactionJob();

                if (sbf != null)
                {
                    if (sbf.phaseSet.Length > 1)
                    {
                        int phaseNum = activeFoePiece.foeData.currentPhase;

                        string phName = sbf.phaseSet[phaseNum].setName;
                        string phCond = sbf.phaseSet[phaseNum].setCondition;
                        phName += "\r\n<size=60%><i>" + phCond + "</i>";

                        phaseText.text = phName;

                        phaseCounter.SetActive(true);
                    }
                    else
                        phaseCounter.SetActive(false);
                }
                else
                    phaseCounter.SetActive(false);
            }
            else
            {
                //check non faction mobs, summons and legends
                IconFoe foeData = activeFoePiece.foeData;

                if (foeData.type == FoeType.Mob || foeData.type == FoeType.SpecialSummon)
                    phaseCounter.SetActive(false);
                else
                {
                    List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
                    if (foeData.type == FoeType.Elite)
                        classList = UnitManager._instance.foes.eliteClasses;
                    else if (foeData.type == FoeType.Legend)
                        classList = UnitManager._instance.foes.legendClasses;

                    FoeData.FoeClass classData = classList[foeData.classIndex];

                    if (classData.phaseAspects.Length > 1)
                    {
                        int phaseNum = activeFoePiece.foeData.currentPhase;

                        string phName = classData.phaseAspects[phaseNum].setName;
                        string phCond = classData.phaseAspects[phaseNum].setCondition;
                        phName += "\r\n<size=60%><i>" + phCond + "</i>";

                        phaseText.text = phName;

                        phaseCounter.SetActive(true);
                    }
                    else
                        phaseCounter.SetActive(false);
                }
            }
        }

        traitEffectListSpacing = traitEffectBlockContent.GetComponent<VerticalLayoutGroup>().spacing;
        BuildTraitEffectList();
        ShowTraitEffectList(false, false);

        abilityListSpacing = abilityBlockContent.GetComponent<VerticalLayoutGroup>().spacing;
        BuildAbilityList();
    }

    public void ModifyHealth(int value)
    {
        /*
        if (activeCharaPiece != null)
        {
            activeCharaPiece.ModifyHealth(value);
        }
        else if (activeFoePiece != null)
        {
            activeFoePiece.ModifyHealth(value);
        }

        UpdateHealthBars();
        */

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

    public void ModifyVigor(int value)
    {
        /*
        int auxVigor = activeUnit.currentVigor;

        bool atMax = activeUnit.currentVigor == activeUnit.vigor;

        if (activeCharaPiece != null)
        {
            activeCharaPiece.ModifyVigor(value);
        }
        else if (activeFoePiece != null)
        {
            activeFoePiece.ModifyVigor(value);
        }

        if ((atMax) && value > 0)
            vigorBar.SetBar(activeUnit.vigor);

        if (activeUnit.currentVigor != 0 && auxVigor == 0) //show animation
            barChangeAnim.SetTrigger("ShowVigor");
        else if (activeUnit.currentVigor == 0 && auxVigor != 0) //hide animation
            barChangeAnim.SetTrigger("HideVigor");

        UpdateHealthBars();
        */

        miniPanelHolding = false;
        currentHoldCooldown = 0;
    }

    public void ModifyVigorHold(int value)
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

        int auxVigor = activeUnit.currentVigor;

        bool atMax = activeUnit.currentVigor == activeUnit.vigor;

        if (activeCharaPiece != null)
        {
            activeCharaPiece.ModifyVigor(value);
        }
        else if (activeFoePiece != null)
        {
            activeFoePiece.ModifyVigor(value);
        }

        if ((atMax) && value > 0)
            vigorBar.SetBar(activeUnit.vigor);

        if (activeUnit.currentVigor != 0 && auxVigor == 0) //show animation
            barChangeAnim.SetTrigger("ShowVigor");
        else if (activeUnit.currentVigor == 0 && auxVigor != 0) //hide animation
            barChangeAnim.SetTrigger("HideVigor");

        UpdateHealthBars();
    }

    private void UpdateHealthBars(bool force = false)
    {
        if (!activeUnit.textInHPFlag)
        {
            /*
            int currentMaxHP = (activeUnit.hp + activeUnit.addedHP);
            if (activeCharaPiece != null)
                currentMaxHP = Mathf.FloorToInt((0.25f * (4 - activeCharaPiece.woundCount))) * (activeUnit.hp + activeUnit.addedHP);
            */

            int currentMaxHP = GetUnitCurrentMaxHP();

            if (force)
                hpBar.ForceValue(activeUnit.currentHP);
            else
                hpBar.ApplyValue(activeUnit.currentHP);

            hpLabel.text = activeUnit.currentHP + "/" + currentMaxHP;
        }
        else
        {
            hpLabel.text = "! " + activeUnit.textHP + " !";
        }

        if (force)
            vigorBar.ForceValue(activeUnit.currentVigor);
        else
            vigorBar.ApplyValue(activeUnit.currentVigor);

        vigorLabel.text = activeUnit.currentVigor + "/" + activeUnit.vigor;
    }

    public int GetUnitCurrentMaxHP(bool considerWound = true)
    {
        if (activeCharaPiece != null)
        {

            if (considerWound)
            {
                return Mathf.CeilToInt((activeUnit.hp + activeUnit.addedHP) * (0.25f * (4 - activeCharaPiece.woundCount)));
            }
            else
                return (activeUnit.hp + activeUnit.addedHP);
        }
        else if(activeFoePiece != null)
        {
            return (activeUnit.hp + activeUnit.addedHP);
        }

        return activeUnit.hp;
    }

    public void ModifyBlessing(bool add)
    {
        activeUnit.SetFreshFlag(false);
        activeUnit.SetBlessing(add);

        SetBlessButtonState();
    }

    public void ChangePhase(bool forward)
    {
        if (activeFoePiece == null)
            return;

        int phaseNum = activeFoePiece.foeData.currentPhase;

        if (activeFoePiece.foeData.factionIndex < 0) 
        {
            List<FoeData.FoeClass> classList = UnitManager._instance.foes.classes;
            if (activeFoePiece.foeData.type == FoeType.Elite)
                classList = UnitManager._instance.foes.eliteClasses;
            else if (activeFoePiece.foeData.type == FoeType.Legend)
                classList = UnitManager._instance.foes.legendClasses;

            FoeData.FoeClass classData = classList[activeFoePiece.foeData.classIndex];

            AspectSet[] phaseSet = classData.phaseAspects;

            if (phaseSet.Length <= 1)
                return;

            phaseNum += forward ? 1 : -1;
            if (phaseNum < 0)
                phaseNum = phaseSet.Length - 1;
            else if (phaseNum >= phaseSet.Length)
                phaseNum = 0;

            string phName = phaseSet[phaseNum].setName;
            string phCond = phaseSet[phaseNum].setCondition;
            phName += "\r\n<size=80%><i>" + phCond + "</i>";

            phaseText.text = phName;
        }
        else
        {
            SubFaction sbf = activeFoePiece.foeData.GetFactionJob();

            if (sbf.phaseSet.Length <= 1)
                return;

            phaseNum += forward ? 1 : -1;
            if (phaseNum < 0)
                phaseNum = sbf.phaseSet.Length - 1;
            else if (phaseNum >= sbf.phaseSet.Length)
                phaseNum = 0;

            string phName = sbf.phaseSet[phaseNum].setName;
            string phCond = sbf.phaseSet[phaseNum].setCondition;
            phName += "\r\n<size=80%><i>" + phCond + "</i>";

            phaseText.text = phName;
        }

        activeFoePiece.foeData.ChangePhase(phaseNum);

        BuildTraitEffectList();

        BuildAbilityList();
    }

    private void SetBlessButtonState()
    {
        blessingButton.GetChild(0).GetComponent<Image>().color = activeUnit.blessingTokens > 0 ? blessingColor : Color.white;
        blessingButton.GetChild(2).GetComponent<Image>().color = activeUnit.blessingTokens > 0 ? blessingColor : Color.white;
        TextMeshProUGUI blessingText = blessingButton.GetChild(1).GetComponent<TextMeshProUGUI>();
        string blessText = "Bless";
        if (activeUnit.blessingTokens > 1)
            blessText = "Blessed (" + activeUnit.blessingTokens + ")";
        else if (activeUnit.blessingTokens > 0)
            blessText = "Blessed";

        blessingText.text = blessText;
        blessingText.color = activeUnit.blessingTokens > 0 ? blessingColor : Color.white;
    }

    public void UpdateWoundCount()
    {
        if (activeCharaPiece == null)
            return;

        activeUnit.SetFreshFlag(false);

        int startVal = woundCounter.GetIconStateCount(true);

        activeCharaPiece.SetWoundCount(startVal);

        int currentHP = Mathf.Clamp(activeCharaPiece.characterData.currentHP, 0, GetUnitCurrentMaxHP());
        activeCharaPiece.characterData.GiveCurrentHP(currentHP);

        hpLabel.text = activeCharaPiece.characterData.currentHP + "/" + GetUnitCurrentMaxHP();

        hpBar.SetBar(GetUnitCurrentMaxHP());
        hpBar.ForceValue(activeCharaPiece.characterData.currentHP);
    }

    public void UpdateStatusEffects()
    {
        activeUnit.SetFreshFlag(false);
        activeUnit.GiveStatusList(statusList.GetStatusList());
        activeUnit.GiveEffectList(effectList.GetEffectList());

        refreshTraitFlag = true;
    }

    public void CloseStatusEffectLists()
    {
        statusList.CallListClose();
        effectList.CallListClose();
    }

    private void DisplayPanelCovers(bool show)
    {
        if (!show && (traitEffectListIsShown || abilityListIsShown))
            return;

        backCover.gameObject.SetActive(show);
        hpVigorCover.gameObject.SetActive(show);
        statPanelCover.gameObject.SetActive(show);

        minusHPButton.gameObject.SetActive(!show);
        plusHPButton.gameObject.SetActive(!show);
        minusVigorButton.gameObject.SetActive(!show);
        plusVigorButton.gameObject.SetActive(!show);

        gameModeTools.gameObject.SetActive(!show);

        /*
        unitPanelButton.gameObject.SetActive(!show);

        if (show)
        {
            UnitManager._instance.SetUnitMenu(false);
        }
        */
    }

    public void TraitEffectButtonClick()
    {
        ShowTraitEffectList(!traitEffectListIsShown);
    }

    private void ShowTraitEffectList(bool show, bool animted = true)
    {
        if (!animted)
        {
            if (show)
            {
                if (refreshTraitFlag)
                    BuildTraitEffectList();

                traitEffectBlockAnim.SetTrigger("JumpOpen");
            }
            else
                traitEffectBlockAnim.SetTrigger("JumpClose");
        }
        else
        {
            if (show)
            {
                if (refreshTraitFlag)
                    BuildTraitEffectList();

                traitEffectBlockAnim.SetTrigger("Open");
            }
            else
                traitEffectBlockAnim.SetTrigger("Close");
        }

        if (!show && currentTraitSelectedIndex >= 0)
        {
            traitEffectDescPanel.gameObject.SetActive(false);
            currentTraitSelectedIndex = -1;
        }

        traitEffectListIsShown = show;

        DisplayPanelCovers(show);
    }

    private void BuildTraitEffectList()
    {
        //clean the list
        for(int i = traitEffectBlockContent.childCount - 1; i >= 1; i--)
        {
            Destroy(traitEffectBlockContent.GetChild(i).gameObject);
        }

        if(traitEntries != null)
            traitEntries.Clear();
        else
            traitEntries = new List<EntryDescPair>();

        traitEffectEntryPrefab.gameObject.SetActive(false);

        //effects
        List<IconUnit.PositiveEffects> effs = activeUnit.activePositiveEffects;

        for (int i = 0; i < effs.Count; i++)
        {
            IconUnit.PositiveEffects ef = effs[i];
            string desc = effectList.GetEffectDescription(ef);

            GameObject entry = CreateTraitEffectEntry(ef.ToString(), desc);
            entry.GetComponent<Image>().color = possitiveEffectColor;
        }

        //status
        List<IconUnit.Status> status = activeUnit.activeStatus;

        for(int i = 0; i < status.Count; i++)
        {
            IconUnit.Status st = status[i];
            string desc = statusList.GetStatusDescription(st);

            GameObject entry = CreateTraitEffectEntry(st.ToString(), desc);
            entry.GetComponent<Image>().color = statusEffectColor;
        }

        //traits
        List<ClassData.Trait> trts = activeUnit.GetTraits();

        for (int i = 0; i < trts.Count; i++)
        {
            ClassData.Trait tr = trts[i];

            CreateTraitEffectEntry(tr.traitName, tr.traitDescription);
        }

        currentTraitSelectedIndex = -1;
        refreshTraitFlag = false;
    }

    private GameObject CreateTraitEffectEntry(string title, string description)
    {
        description = description.Replace("\\n", "<br>");

        GameObject nuEntry = Instantiate<GameObject>(traitEffectEntryPrefab.gameObject, traitEffectBlockContent);
        nuEntry.transform.GetChild(0).gameObject.SetActive(false); //frame
        nuEntry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = title;

        int descPairIndex = traitEntries.Count;
        nuEntry.GetComponent<HoldButton>().onRelease.AddListener(delegate
        {
            EntryDescPair pair = traitEntries[descPairIndex];
            if (currentTraitSelectedIndex >= 0)
            {
                if (currentTraitSelectedIndex == descPairIndex)
                {
                    //same choice. deactivate
                    pair.uiEntry.transform.GetChild(0).gameObject.SetActive(false);
                    traitEffectDescPanel.gameObject.SetActive(false);
                    currentTraitSelectedIndex = -1;
                }
                else
                {
                    //change target
                    traitEntries[currentTraitSelectedIndex].uiEntry.transform.GetChild(0).gameObject.SetActive(false);
                    pair.uiEntry.transform.GetChild(0).gameObject.SetActive(true);
                    traitEffectText.text = pair.description;
                    currentTraitSelectedIndex = descPairIndex;
                }
            }
            else
            {
                //open the panel and show description
                pair.uiEntry.transform.GetChild(0).gameObject.SetActive(true);
                traitEffectDescPanel.gameObject.SetActive(true);
                traitEffectText.text = pair.description;
                currentTraitSelectedIndex = descPairIndex;
            }
        });

        EntryDescPair nuPair = new EntryDescPair() { uiEntry = nuEntry, description = description };
        traitEntries.Add(nuPair);

        nuEntry.SetActive(true);

        return nuEntry;
    }

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

    public ClassData.Trait FindTrait(int docID, int ID)
    {
        TraitEntry.Trait[] coll = traitsBase[docID].Traits;

        for (int i = 0; i < coll.Length; i++)
        {
            if (coll[i].traitIDInList == ID) 
            { 
                ClassData.Trait trait = new ClassData.Trait();
                trait.traitID = coll[i].traitIDInList;
                trait.docID = docID;
                trait.traitName = coll[i].traitName;
                trait.traitDescription = coll[i].traitEffect;

                return trait;
            }
        }

        return new ClassData.Trait();
    }

    public ClassData.Ability FindAbility(int docID, int ID)
    {
        AbilityEntry.FoeAbility[] coll = abilityBase[docID].abilities;

        for (int i = 0; i < coll.Length; i++)
        {
            if (coll[i].abilityIDInList == ID)
            {
                ClassData.Ability ability = new ClassData.Ability();
                ability.abilityID = coll[i].abilityIDInList;
                ability.docID = docID;
                ability.abilityName = coll[i].abilityName;
                ability.abilityEffect = coll[i].effect;
                ability.abilityAspects = coll[i].additionals;
                ability.actionCost = coll[i].actionCost;
                ability.abilityComboDepth = 0;
                ability.subCombos = coll[i].subCombos;

                return ability;
            }
        }

        return new ClassData.Ability();
    }
}
