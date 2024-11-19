using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class CharacterUnitPanel : UnitPanel
{
    [Header("Character Aspects")]
    [SerializeField] private TextMeshProUGUI kinLabel;
    [SerializeField] private TextMeshProUGUI cultureLabel;
    [SerializeField] private TextMeshProUGUI classLabel;
    [SerializeField] private TextMeshProUGUI jobLabel;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI bondLabel;
    [SerializeField] private RectTransform narrativeContent;
    [SerializeField] private GameObject modifierPrefab;
    [SerializeField] private TextMeshProUGUI maxStressLabel;
    [SerializeField] private TextMeshProUGUI strainLabel;
    [SerializeField] private TextMeshProUGUI stressReliefLabel;
    [SerializeField] private TextMeshProUGUI stressSpecialLabel;
    [SerializeField] private TextMeshProUGUI idealsLabel;
    private List<GameObject> narrativeModifiersActive;

    private IconCharacter sourceCharacter;
    private CharacterPiece sourcePiece;

    public void GiveCharacterSource(IconCharacter src, CharacterPiece pees, bool editMode = true)
    {
        sourceCharacter = src;
        sourcePiece = pees;

        BuildPanel(editMode);
    }

    private void BuildPanel(bool editMode)
    {
        if (sourceCharacter.textInHPFlag)
        {
            if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
            {
                hpInput.gameObject.SetActive(true);
                hpInputJustText.gameObject.SetActive(false);

                hpInput.transform.GetChild(2).gameObject.SetActive(true); //alert frame
                hpInput.text = "";
                hpInputPlaceholder.GetComponent<TextMeshProUGUI>().text = sourceCharacter.textHP;
            }
            else
            {
                hpInput.gameObject.SetActive(false);
                hpInputJustText.gameObject.SetActive(true);

                hpInputJustText.text = sourceCharacter.textHP;
            }
        }
        else
        {
            if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
            {
                hpInput.gameObject.SetActive(true);
                hpInputJustText.gameObject.SetActive(false);

                hpInput.transform.GetChild(2).gameObject.SetActive(false);
                hpInput.SetTextWithoutNotify(GetCharaCurrentMaxHP().ToString());
            }
            else
            {
                hpInput.gameObject.SetActive(false);
                hpInputJustText.gameObject.SetActive(true);

                hpInputJustText.text = GetCharaCurrentMaxHP().ToString();
            }

            if (sourceCharacter.currentHP == 0)
                sourcePiece.SetPieceFaded(true);
            else
                sourcePiece.SetPieceFaded(false);
        }

        panelModeLabel.text = (MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode) ? "Piece Edit" : "Piece Info";

        if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
        {
            sizeInput.gameObject.SetActive(true);
            sizeInputJustText.gameObject.SetActive(false);

            sizeInput.SetTextWithoutNotify(sourceCharacter.size.ToString());
        }
        else
        {
            sizeInput.gameObject.SetActive(false);
            sizeInputJustText.gameObject.SetActive(true);

            sizeInputJustText.text = sourceCharacter.size.ToString();
        }

        int chapterNum = 1; // Mathf.CeilToInt((float)sourceCharacter.level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;

        nameLabel.text = sourceCharacter.unitName;
        levelLabel.text = "·Level - <b>" + sourceCharacter.level + "</b>";
        defenseLabel.text = "·Defense - <b>" + sourceCharacter.defense + "</b>";
        speedLabel.text = "·Speed - <b>" + sourceCharacter.speed + " <i>(Dash " + sourceCharacter.dash + ")</i>" + "</b>";
        //attackLabel.text = "Attack Bonus - <b>" + sourceCharacter.attack + "</b>";
        frayDamageLabel.text = "·Fray Damage - <b>" + sourceCharacter.frayDamage + "</b>";
        damageLabel.text = "·Damage - <b>" + sourceCharacter.damage + "</b>";
        basicAttackLabel.text = "·Basic Attack - <b>" + sourceCharacter.attackType + "</b>";

        kinLabel.text = "Kin - <b>" + sourceCharacter.kin + "</b>";
        cultureLabel.text = "Culture - <b>" + UnitManager._instance.cultures.cultures[sourceCharacter.narrativeAspect.cultureIndex].name + "</b>";
        classLabel.text = "Class - <b>" + UnitManager._instance.classes.classes[sourceCharacter.classIndex].name + "</b>";
        jobLabel.text = "Job - <b>" + UnitManager._instance.classes.classes[sourceCharacter.classIndex].jobs[sourceCharacter.jobIndex].name + "</b>";

        UpdateTraits();

        UpdateNarrative();

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    private IEnumerator DelayedTraitRecalculate()
    {
        yield return new WaitForEndOfFrame();

        Vector2 sd = traitContent.sizeDelta;
        sd.y = 1.2f * traitLabel.renderedHeight;
        traitContent.sizeDelta = sd;

        traitContent.anchoredPosition = Vector2.zero;
    }

    private IEnumerator DelayedNarrativeRecalculate()
    {
        yield return new WaitForEndOfFrame();

        float posY = -50f;
        posY -= 1.1f * idealsLabel.renderedHeight;

        posY -= 20f;
        Vector3 aPos = maxStressLabel.GetComponent<RectTransform>().anchoredPosition;
        aPos.y = posY;
        maxStressLabel.GetComponent<RectTransform>().anchoredPosition = aPos;
        maxStressLabel.text = "Effort - <b>" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].effort + "</b>";
        aPos = stressReliefLabel.GetComponent<RectTransform>().anchoredPosition;
        aPos.y = posY;
        stressReliefLabel.GetComponent<RectTransform>().anchoredPosition = aPos;
        

        posY -= 1.1f * stressReliefLabel.renderedHeight;
        aPos = strainLabel.GetComponent<RectTransform>().anchoredPosition;
        aPos.y = posY;
        strainLabel.GetComponent<RectTransform>().anchoredPosition = aPos;
        strainLabel.text = "Strain - <b>" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].strain + "</b>";
        aPos = stressSpecialLabel.GetComponent<RectTransform>().anchoredPosition;
        aPos.y = posY;
        stressSpecialLabel.GetComponent<RectTransform>().anchoredPosition = aPos;

        posY -= 2f * stressSpecialLabel.renderedHeight;


        sourceCharacter.narrativeAspect.UpdateBaseActionValues();

        //modifiers
        for (int i = 0; i < sourceCharacter.narrativeAspect.actionValues.Count; i++)
        {
            int totalValue = sourceCharacter.narrativeAspect.actionValues[i].bondModifier + sourceCharacter.narrativeAspect.actionValues[i].cultureModifier + sourceCharacter.narrativeAspect.actionValues[i].dotModifier;
            if (totalValue <= 0)
                continue;

            GameObject nuModifierEntry = Instantiate<GameObject>(modifierPrefab, modifierPrefab.transform.parent);
            RectTransform modRT = nuModifierEntry.GetComponent<RectTransform>();

            aPos = modRT.anchoredPosition;
            aPos.y = posY;
            modRT.anchoredPosition = aPos;

            DotBar modBar = nuModifierEntry.transform.GetChild(0).GetComponent<DotBar>();
            modBar.SetBarValue(totalValue);
            modBar.interactable = false;

            nuModifierEntry.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = sourceCharacter.narrativeAspect.actionValues[i].targetAction.ToString();

            posY -= 55f;
            nuModifierEntry.SetActive(true);

            narrativeModifiersActive.Add(nuModifierEntry);
        }

        Vector2 sd = narrativeContent.sizeDelta;
        sd.y = -1f * (posY - 50f);
        narrativeContent.sizeDelta = sd;
    }

    public void UpdateTraits()
    {
        if (!gameObject.activeSelf) //the update then frame end wait method of updating the content rect based on the text size only works when the gameobject in question is active. So this methos is tide now to the tab click event of TabbedContent
            return;

        string traitList = "<i>Traits</i>\n\n";
        for (int i = 0; i < sourceCharacter.traits.Count; i++)
        {
            traitList += "<b>" + sourceCharacter.traits[i].traitName + "</b>\n";
            traitList += sourceCharacter.traits[i].traitDescription + "\n\n";
        }

        Vector2 sd = traitContent.sizeDelta;
        sd.y = 10f;
        traitContent.sizeDelta = sd;

        traitLabel.text = traitList;

        traitContent.anchoredPosition = Vector2.zero;

        StartCoroutine(DelayedTraitRecalculate());
    }

    public void UpdateNarrative()
    {
        if (!gameObject.activeSelf)  //the update then frame end wait method of updating the content rect based on the text size only works when the gameobject in question is active. So this methos is tide now to the tab click event of TabbedContent
            return;

        bondLabel.text = "Bond - <b>" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].name + "</b>";
        string idealsText = "<b>Ideals</b> <i><size=75%>(1 xp for one, 2 xp for all at end of session)</i>\n<size=100%>";
        for (int i = 0; i < UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].ideals.Length; i++)
        {
            idealsText += "\n·" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].ideals[i];
        }

        idealsLabel.text = idealsText;
        idealsLabel.ForceMeshUpdate();
        stressReliefLabel.text = "Second Wind - <b>" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].secondWind + "</b>";
        stressReliefLabel.ForceMeshUpdate();
        stressSpecialLabel.text = "Special Ability - <b>" + UnitManager._instance.bonds.bonds[sourceCharacter.narrativeAspect.bondIndex].stressSpecial + "</b>";
        stressSpecialLabel.ForceMeshUpdate();

        //set piece
        //PieceCamera._instance.SetSamplerAtStartRotation();
        //PieceCamera._instance.SetSamplerConfig(sourceCharacter);

        if (narrativeModifiersActive == null)
            narrativeModifiersActive = new List<GameObject>();
        else if(narrativeModifiersActive.Count > 0)
        {
            for(int i = 0; i < narrativeModifiersActive.Count; i++)
            {
                Destroy(narrativeModifiersActive[i].gameObject);
            }
        }
        narrativeModifiersActive.Clear();

        StartCoroutine(DelayedNarrativeRecalculate());
    }

    //HP
    public void OnHPInput()
    {
        /*
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;
        */

        sourceCharacter.SetFreshFlag(false);

        int hpInputValue = 0;
        if(int.TryParse(hpInput.text, out hpInputValue))
        {
            if (hpInputValue <= 0)
            {
                if (sourceCharacter.textInHPFlag)
                {
                    hpInput.text = "";
                    hpInputPlaceholder.GetComponent<TextMeshProUGUI>().text = sourceCharacter.textHP;
                }
                else
                    hpInput.SetTextWithoutNotify(sourceCharacter.hp.ToString());

                return;
            }

            if (sourceCharacter.textInHPFlag)
            {
                sourceCharacter.CorrectTextHP(hpInputValue);

                hpInput.transform.GetChild(2).gameObject.SetActive(false);
                hpInput.SetTextWithoutNotify(sourceCharacter.hp.ToString());
            }
            else
            {
                int diff = hpInputValue - sourceCharacter.hp;
                sourceCharacter.GiveAddedHP(diff);
                sourceCharacter.GiveCurrentHP(Mathf.Clamp(sourceCharacter.currentHP, 0, GetCharaCurrentMaxHP()));

                hpInput.SetTextWithoutNotify((sourceCharacter.hp + diff).ToString());
            }
        }

        if (sourceCharacter.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    public void OnSizeInput()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;

        sourceCharacter.SetFreshFlag(false);

        int sizeInputValue = 0;
        if (int.TryParse(sizeInput.text, out sizeInputValue))
        {
            if (sizeInputValue < 0)
                return;

            PieceManager._instance.SizeChangeCall(sizeInputValue);

            sizeInputValue = Mathf.Clamp(sizeInputValue, 1, PieceManager._instance.pieceMaxSize);
            sourceCharacter.GiveSize(sizeInputValue);
            sizeInput.SetTextWithoutNotify(sourceCharacter.size.ToString());
        }

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    public void AddHP(bool add)
    {
        sourceCharacter.SetFreshFlag(false);

        int startVal = sourceCharacter.currentHP;
        startVal += add ? 1 : -1;

        startVal = Mathf.Clamp(startVal, 0, GetCharaCurrentMaxHP());

        sourceCharacter.GiveCurrentHP(startVal);

        if (sourceCharacter.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);
    }

    public int GetCharaCurrentMaxHP(bool considerWound = true)
    {
        if (considerWound)
        {
            return Mathf.CeilToInt((sourceCharacter.hp + sourceCharacter.addedHP) * (0.25f * (4 - sourcePiece.woundCount)));
        }
        else
            return (sourceCharacter.hp + sourceCharacter.addedHP);
    }

    public void AddVigor(bool add)
    {
        sourceCharacter.SetFreshFlag(false);

        int auxCV = sourceCharacter.currentVigor;
        int auxV = sourceCharacter.vigor;

        sourceCharacter.AddVigor(add);
    }

    public IconCharacter GetCharacterData()
    {
        return sourceCharacter;
    }

    public void UpdatePieceParts()
    {
        if (sourceCharacter == null)
            return;

        sourceCharacter.SetFreshFlag(false);
        int[] ids = PieceCamera._instance.GetCurrentSamplePartIDs();
        sourceCharacter.GivePartIDs(ids[0], ids[1], ids[2], ids[3]);

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    private bool CheckPieceBaseDiff()
    {
        if (sourceCharacter == null)
            return false;

        IconCharacter baseChara = UnitManager._instance.GetCharacter(sourceCharacter.unitID);

        bool showDebug = false;

        /*
        if ((baseChara.hp + baseChara.addedHP) != (sourceCharacter.hp + sourceCharacter.addedHP))
        {
            if(showDebug)
                Debug.Log("Diff in HP");
            return true;
        }
        else if (baseChara.elixirs != sourceCharacter.elixirs)
        {
            if (showDebug)
                Debug.Log("Diff in Elixir Count");
            return true;
        }
        else if (baseChara.size != sourceCharacter.size)
        {
            if (showDebug)
                Debug.Log("Diff in Size");
            return true;
        }
        else if (baseChara.armor != sourceCharacter.armor)
        {
            if (showDebug)
                Debug.Log("Diff in Armor");
            return true;
        }
        */
        if (baseChara.headPartID != sourceCharacter.headPartID)
        {
            if (showDebug)
                Debug.Log("Diff in Head Piece ID");
            return true;
        }
        else if (baseChara.lWeaponPartID != sourceCharacter.lWeaponPartID)
        {
            if (showDebug)
                Debug.Log("Diff in L Piece ID");
            return true;
        }
        else if (baseChara.rWeaponPartID != sourceCharacter.rWeaponPartID)
        {
            if (showDebug)
                Debug.Log("Diff in R Piece ID");
            return true;
        }

        return false;
    }

    public void UpdateApplyDiffButton()
    {
        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    public void ApplyDiffToBase()
    {
        if (sourceCharacter == null)
            return;

        UnitManager._instance.UpdateCharacterPiece(sourceCharacter.unitID, sourceCharacter);
        UnitManager._instance.optionsManager.SaveCharacter(sourceCharacter.unitID);

        UpdateApplyDiffButton();
    }
}
