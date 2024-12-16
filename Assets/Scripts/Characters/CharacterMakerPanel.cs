using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class CharacterMakerPanel : MonoBehaviour
{
    private EsperCharacter activeCharacter;

    private RectTransform listRT;

    private bool editMode;

    [Header("General Panel")]
    [SerializeField] private RectTransform makerPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI colorLabel;
    [SerializeField] private Image colorImage;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI pageLabel;
    private int currentPage = 0;

    [SerializeField] private CanvasGroup forwardCharPageButton;
    [SerializeField] private CanvasGroup backCharPageButton;

    private ColorListPanel colorListPanel;
    private bool colorListOpen = false;

    [Space(10f)]
    [SerializeField] private ListPanel listPanel;
    [SerializeField] public Vector2 slimListPanelProportions;
    [SerializeField] public Vector2 wideListPanelProportions;

    [Header("Magic Arts")]
    [SerializeField] private RectTransform magicArtsPage;
    [SerializeField] private RectTransform magicArtsButton;
    [SerializeField] private GameObject magicArtsEntryPrefab;
    private bool magicArtsListOpen = false;

    private Dictionary<int, Transform> ArtIDEntryDict;

    [SerializeField] private RectTransform magicSkillButton;
    [SerializeField] private GameObject magicSkillsEntryPrefab;
    [SerializeField] private TextMeshProUGUI magicSkillDescription;
    private bool magicSkillsListOpen = false;

    private int currentSelectedSkill = 0;

    private Dictionary<int, int> listEntrySkillIDDict;
    private Dictionary<int, Transform> SkillIDEntryDict;

    [Header("Stats")]
    [SerializeField] private RectTransform statPage;
    [SerializeField] private TextMeshProUGUI strenghtStatDieLabel;
    [SerializeField] private TextMeshProUGUI intelligenceStatDieLabel;
    [SerializeField] private TextMeshProUGUI dexterityStatDieLabel;
    [SerializeField] private TextMeshProUGUI charismaStatDieLabel;
    [SerializeField] private TMP_InputField hpInput;
    [SerializeField] private TMP_InputField defInput;
    [SerializeField] private TMP_InputField carryingInput;

    [Header("Equipment")]
    [SerializeField] private RectTransform equipmentPage;
    [SerializeField] private GameObject weaponEntryPrefab; //only allow one!
    [SerializeField] private GameObject itemEntryPrefab;
    [SerializeField] private GameObject equipmentPrefab;

    [Header("Piece")]
    [SerializeField] private RectTransform pieceLookPage;

    private void LateUpdate()
    {
        bool aListOpen = colorListOpen || magicArtsListOpen || magicSkillsListOpen;

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
            else if(magicArtsListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    magicArtsListOpen = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= AddArtToCharacter;
                }
            }
            else if(magicSkillsListOpen && Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    magicSkillsListOpen = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= AddSkillToCharacter;
                }
            }
        }
    }

    public void StartPanel(EsperCharacter targetCharacter, bool editMode)
    {
        activeCharacter = targetCharacter;

        nameInputField.text = "";
        nameInputField.ForceLabelUpdate();

        colorLabel.text = "White";
        colorImage.color = Color.white;
        activeCharacter.colorChoice = Color.white;

        this.editMode = editMode;

        SetPanelsWithInfo();
        SetPanelPage(0);
    }

    private void SetPanelsWithInfo()
    {
        //magic arts page
        //magic arts
        Transform contentParent = magicArtsEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        if (ArtIDEntryDict == null)
            ArtIDEntryDict = new Dictionary<int, Transform>();
        else
            ArtIDEntryDict.Clear();

        for (int i = 0; i < activeCharacter.magicArts.Length; i++)
        {
            AddNewArtUIEntry(activeCharacter.magicArts[i], activeCharacter.magicArtLevels[i]);
        }

        //skills
        contentParent = magicSkillsEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        if (SkillIDEntryDict == null)
            SkillIDEntryDict = new Dictionary<int, Transform>();
        else
            SkillIDEntryDict.Clear();

        for (int i = 0; i < activeCharacter.skillsIDs.Length; i++)
        {
            AddNewSkillUIEntry(activeCharacter.skillsIDs[i]);
        }

        magicSkillDescription.text = "<i>Press a skill to see its description</i>";

        currentSelectedSkill = -1;

        //stats page
        strenghtStatDieLabel.text = activeCharacter.statSTR.ToString();
        intelligenceStatDieLabel.text = activeCharacter.statINT.ToString();
        dexterityStatDieLabel.text = activeCharacter.statDEX.ToString();
        charismaStatDieLabel.text = activeCharacter.statCHA.ToString();

        hpInput.SetTextWithoutNotify(activeCharacter.hp.ToString());
        defInput.SetTextWithoutNotify(activeCharacter.defense.ToString());
        carryingInput.SetTextWithoutNotify((activeCharacter.statSTR + 2).ToString());

        //items page

        //weapon
        contentParent = weaponEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        GameObject weaponEntry = Instantiate<GameObject>(weaponEntryPrefab, contentParent);
        Transform weaponTF = weaponEntry.transform;

        ItemsData.Weapon charaWeapon = UnitManager._instance.itemData.weapons[activeCharacter.weaponID];
        weaponTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = charaWeapon.name;
        weaponTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = charaWeapon.range.ToString();

        int atkMod = charaWeapon.atkEffectModifier;
        string modLabel = atkMod + " points to attack total";
        if (atkMod > 0)
            modLabel = "+" + modLabel;
        weaponTF.GetChild(2).GetComponent<TextMeshProUGUI>().text = modLabel;
        weaponEntry.SetActive(true);

        //items
        int[] itemIDs = activeCharacter.itemInventory;

        contentParent = itemEntryPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < itemIDs.Length; i++)
        {
            GameObject itemEntry = Instantiate<GameObject>(itemEntryPrefab, contentParent);
            Transform itemTF = itemEntry.transform;

            ItemsData.Item curItem = UnitManager._instance.itemData.items[itemIDs[i]];
            itemTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = curItem.name;
            itemTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = curItem.effect;

            int itemIdx = i;
            itemTF.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteItem(itemIdx); });

            itemEntry.SetActive(true);
        }

        //equipment
        int[] equipIDs = activeCharacter.equipmentInventory;

        contentParent = equipmentPrefab.transform.parent;
        for (int i = contentParent.childCount - 1; i >= 1; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < equipIDs.Length; i++)
        {
            GameObject equipmentEntry = Instantiate<GameObject>(equipmentPrefab, contentParent);
            Transform equipmentTF = equipmentEntry.transform;

            ItemsData.Equipment curEquip = UnitManager._instance.itemData.equipment[equipIDs[i]];
            equipmentTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = curEquip.name;
            equipmentTF.GetChild(1).GetComponent<TextMeshProUGUI>().text = curEquip.effect;

            int equipmentIdx = i;
            equipmentTF.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteEquipment(equipmentIdx); });

            equipmentEntry.SetActive(true);
        }
    }

    #region General Section

    public void SetPanelPage(int pageIndex)
    {

        currentPage = pageIndex;
        backCharPageButton.alpha = currentPage == 0 ? 0.2f : 1f;
        forwardCharPageButton.alpha = currentPage == 3 ? 0.2f : 1f;

        magicArtsPage.gameObject.SetActive(currentPage == 0);
        statPage.gameObject.SetActive(currentPage == 1);
        equipmentPage.gameObject.SetActive(currentPage == 2);
        pieceLookPage.gameObject.SetActive(currentPage == 3);
    }

    public void PageForward(int moveDir)
    {
        currentPage = Mathf.Clamp(currentPage + moveDir, 0, 3);
        SetPanelPage(currentPage);
    }

    public void UpdateCharacterName()
    {
        activeCharacter.unitName = nameInputField.text;
    }

    public void OpenColorOptions()
    {
        colorListPanel = ColorManager._instance.generalColorList;
        colorListPanel.screenProportionSize = slimListPanelProportions;
        colorListPanel.listColor = 0.9f * makerPanel.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform colorButtonRT = colorLabel.transform.parent.GetComponent<RectTransform>();

        Debug.Log((0.5f * colorButtonRT.rect.size.x * Vector3.right).ToString());

        Vector3 listOrigin = colorButtonRT.position + (-0.5f * colorButtonRT.rect.size.x * colorButtonRT.lossyScale.x * Vector3.right);

        ColorManager._instance.ShowGeneralColorPanel(listOrigin);
        magicArtsListOpen = false;
        magicSkillsListOpen = false;
        colorListOpen = true;
        colorListPanel.OnEntryClick += ColorListClick;

        listRT = colorListPanel.GetComponent<RectTransform>();
    }

    public void ColorListClick(int index)
    {
        activeCharacter.colorChoice = ColorManager._instance.colors[index].color;
        ColorManager._instance.HideGeneralColorPanel();
        ColorManager._instance.generalColorList.OnEntryClick -= ColorListClick;
        colorListOpen = false;

        colorLabel.text = ColorManager._instance.colors[index].name;
        colorImage.color = ColorManager._instance.colors[index].color;
    }

    #endregion

    #region Magic Art Methods

    public void OpenMagicArtList()
    {
        listPanel.screenProportionSize = slimListPanelProportions;
        listPanel.listColor = 0.9f * makerPanel.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform magicArtRT = magicArtsButton;
        Vector3 listOrigin = magicArtRT.position + (0.5f * magicArtRT.rect.size.x * magicArtRT.lossyScale.x * Vector3.right);
        List<string> artTypes = new List<string>();
        List<SkillsData.MagicArt> arts = UnitManager._instance.skillData.magicArts;
        for (int i = 0; i < arts.Count; i++)
        {
            artTypes.Add(arts[i].artName);
        }

        listPanel.ShowPanel(listOrigin, artTypes, true);
        magicArtsListOpen = true;
        magicSkillsListOpen = false;
        colorListOpen = false;
        listPanel.OnEntryClick += AddArtToCharacter;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void AddArtToCharacter(int artID)
    {
        Debug.Log("adding art ID " + artID);

        int[] currentArts = activeCharacter.magicArts;

        List<int> newArts = new List<int>(currentArts);
        List<int> newLevels = new List<int>(activeCharacter.magicArtLevels);

        if (!ArtIDEntryDict.ContainsKey(artID))
        {
            newArts.Add(artID);
            newLevels.Add(1);
            activeCharacter.magicArts = newArts.ToArray();
            activeCharacter.magicArtLevels = newLevels.ToArray();

            AddNewArtUIEntry(artID, 1);
        }

        magicArtsListOpen = false;
        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= AddArtToCharacter;
    }

    private void AddNewArtUIEntry(int artID, int relativeLevel)
    {
        Transform contentParent = magicArtsEntryPrefab.transform.parent;

        GameObject nuEntry = Instantiate<GameObject>(magicArtsEntryPrefab, contentParent);
        Transform entryTF = nuEntry.transform;

        SkillsData.MagicArt art = UnitManager._instance.skillData.magicArts[artID];
        entryTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = art.artName;
        entryTF.GetChild(1).GetComponent<TMP_InputField>().SetTextWithoutNotify("Lvl. " + relativeLevel.ToString());

        ArtIDEntryDict.Add(artID, entryTF);

        int auxArtID = artID;
        entryTF.GetChild(1).GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate { OnEditMagicArtLevel(auxArtID); });

        int artIndex = artID;
        entryTF.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteArt(artIndex); });

        nuEntry.SetActive(true);
    }

    public void OnEditMagicArtLevel(int magicIDAssociation)
    {
        Transform entry = ArtIDEntryDict[magicIDAssociation];

        TMP_InputField levelInput = entry.GetChild(1).GetComponent<TMP_InputField>();

        int level = 0;
        if (int.TryParse(levelInput.text, out level))
        {
            level = Mathf.Clamp(level, 1, 7);
            levelInput.SetTextWithoutNotify("Lvl. " + level);

            int[] arts = activeCharacter.magicArts;
            int[] artLevels = activeCharacter.magicArtLevels;
            for (int i = 0; i < arts.Length; i++)
            {
                if (arts[i] == magicIDAssociation)
                {
                    artLevels[i] = level;
                    break;
                }
            }

            Debug.Log("Setting art " + UnitManager._instance.skillData.magicArts[magicIDAssociation].artName + " at level " + level);
            activeCharacter.magicArtLevels = artLevels;
        }
    }

    public void DeleteArt(int magicIDAssociation)
    {
        if (!ArtIDEntryDict.ContainsKey(magicIDAssociation))
            return;

        int[] magicArts = activeCharacter.magicArts;
        int[] magicLevels = activeCharacter.magicArtLevels;

        List<int> artList = new List<int>();
        List<int> levelList = new List<int>();

        //delete art and related level
        for(int i = 0; i < magicArts.Length; i++)
        {
            if (magicArts[i] != magicIDAssociation)
            {
                artList.Add(magicArts[i]);
                levelList.Add(magicLevels[i]);
            }
        }

        Destroy(ArtIDEntryDict[magicIDAssociation].gameObject);
        ArtIDEntryDict.Remove(magicIDAssociation);

        Debug.Log("Removed art " + UnitManager._instance.skillData.magicArts[magicIDAssociation].artName);

        activeCharacter.magicArts = artList.ToArray();
        activeCharacter.magicArtLevels = artList.ToArray();
    }

    #endregion

    #region Magic Skill Methods

    public void OpenMagicSkillList()
    {
        listPanel.screenProportionSize = slimListPanelProportions;
        listPanel.listColor = 0.9f * makerPanel.transform.GetChild(0).GetComponent<Image>().color;

        RectTransform magicSkillRT = magicSkillButton;
        Vector3 listOrigin = magicSkillRT.position + (0.5f * magicSkillRT.rect.size.x * magicSkillRT.lossyScale.x * Vector3.right);
        List<string> skillEntries = new List<string>();

        if (listEntrySkillIDDict == null)
            listEntrySkillIDDict = new Dictionary<int, int>();
        else
            listEntrySkillIDDict.Clear();

        int[] artIDs = activeCharacter.magicArts;
        for(int a = 0; a < artIDs.Length; a++)
        {
            SkillsData.MagicArt art = UnitManager._instance.skillData.magicArts[artIDs[a]];
            int artLevel = activeCharacter.magicArtLevels[a];

            List<SkillsData.MagicSkill> availableSkills = new List<SkillsData.MagicSkill>();
            for(int i = 0; i < art.noviceSkills.Count; i++)
            {
                availableSkills.Add(art.noviceSkills[i]);
            }
            if(artLevel >= 3)
            {
                for (int i = 0; i < art.adeptSkills.Count; i++)
                {
                    availableSkills.Add(art.adeptSkills[i]);
                }
            }
            if (artLevel >= 5)
            {
                for (int i = 0; i < art.masterSkills.Count; i++)
                {
                    availableSkills.Add(art.masterSkills[i]);
                }
            }
            if (artLevel >= 7)
            {
                for (int i = 0; i < art.grandMaster.Count; i++)
                {
                    availableSkills.Add(art.grandMaster[i]);
                }
            }

            for(int s = 0; s < availableSkills.Count; s++)
            {
                listEntrySkillIDDict.Add(skillEntries.Count, availableSkills[s].skillID);
                skillEntries.Add(availableSkills[s].skillName);
            }
        }

        if (skillEntries.Count == 0)
            return;

        listPanel.ShowPanel(listOrigin, skillEntries, true);
        magicArtsListOpen = false;
        magicSkillsListOpen = true;
        colorListOpen = false;
        listPanel.OnEntryClick += AddSkillToCharacter;

        listRT = listPanel.GetComponent<RectTransform>();
    }

    public void AddSkillToCharacter(int skillListIndex)
    {
        int skillID = listEntrySkillIDDict[skillListIndex];

        Debug.Log("adding skill ID " + skillID);

        int[] currentSkills = activeCharacter.skillsIDs;

        List<int> newSkills = new List<int>(currentSkills);

        if (!newSkills.Contains(skillID))
        {
            newSkills.Add(skillID);
            activeCharacter.skillsIDs= newSkills.ToArray();

            AddNewSkillUIEntry(skillID);
        }

        magicSkillsListOpen = false;
        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= AddSkillToCharacter;
    }

    private void AddNewSkillUIEntry(int skillID)
    {
        Transform contentParent = magicSkillsEntryPrefab.transform.parent;

        GameObject nuEntry = Instantiate<GameObject>(magicSkillsEntryPrefab, contentParent);
        Transform entryTF = nuEntry.transform;

        SkillsData.MagicSkill skill = UnitManager._instance.skillData.GetSkillWithID(skillID);
        entryTF.GetChild(0).GetComponent<TextMeshProUGUI>().text = skill.skillName;
        entryTF.GetChild(2).gameObject.SetActive(false);

        int skillIndex = skillID;
        entryTF.GetComponent<HoldButton>().onRelease.AddListener(delegate { SelectSkill(skillIndex); });

        SkillIDEntryDict.Add(skillID, entryTF);

        entryTF.GetChild(1).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteSkill(skillIndex); });

        nuEntry.SetActive(true);
    }

    public void SelectSkill(int skillID)
    {
        Transform entry = SkillIDEntryDict[skillID];

        if (currentSelectedSkill >= 0)
        {
            if(currentSelectedSkill == entry.GetSiblingIndex())
            {
                entry.GetChild(2).gameObject.SetActive(false);
                currentSelectedSkill = -1;
                magicSkillDescription.text = "<i>Press a skill to see its description</i>";
                return;
            }
            else
            {
                entry.parent.GetChild(currentSelectedSkill).GetChild(2).gameObject.SetActive(false);
            }
        }

        SkillsData.MagicSkill skill = UnitManager._instance.skillData.GetSkillWithID(skillID);
        string desc = "";
        if (skill.damage != null && skill.damage.Length > 0)
            desc += "· Damage: " + skill.damage;
        if (skill.effect != null && skill.effect.Length > 0)
            desc += "\n· Effect: " + skill.effect;

        magicSkillDescription.text = desc;
        entry.GetChild(2).gameObject.SetActive(true);

        currentSelectedSkill = entry.GetSiblingIndex();
    }

    public void DeleteSkill(int skillIDAssociation)
    {
        Debug.Log(skillIDAssociation);

        if (!SkillIDEntryDict.ContainsKey(skillIDAssociation))
            return;

        int[] magicSkills = activeCharacter.skillsIDs;

        List<int> skillList = new List<int>();

        //delete art and related level
        for (int i = 0; i < magicSkills.Length; i++)
        {
            if (magicSkills[i] != skillIDAssociation)
            {
                skillList.Add(magicSkills[i]);
            }
        }

        Transform entry = SkillIDEntryDict[skillIDAssociation];

        if(currentSelectedSkill == entry.GetSiblingIndex())
        {
            currentSelectedSkill = -1;
            magicSkillDescription.text = "<i>Press a skill to see its description</i>";
        }

        Destroy(entry.gameObject);
        SkillIDEntryDict.Remove(skillIDAssociation);

        Debug.Log("Removed skill " + UnitManager._instance.skillData.GetSkillWithID(skillIDAssociation).skillName);

        activeCharacter.skillsIDs = skillList.ToArray();
    }

    #endregion

    #region Stat Methods



    #endregion

    #region Weapon Methods



    #endregion

    #region Item Methods

    public void DeleteItem(int index)
    {

    }

    #endregion

    #region Equipment Methods

    public void DeleteEquipment(int index)
    {

    }

    #endregion
}
