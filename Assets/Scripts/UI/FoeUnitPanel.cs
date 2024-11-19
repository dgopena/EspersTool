using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using static ClassData;

public class FoeUnitPanel : UnitPanel
{
    [Header("Foe Aspects")]
    [SerializeField] private TextMeshProUGUI classLabel;
    [SerializeField] private TextMeshProUGUI jobLabel;

    [Space(10f)]
    [SerializeField] private TMP_InputField phaseInput;
    [SerializeField] private TextMeshProUGUI phaseInputJustText;

    [Space(10f)]
    [SerializeField] private RectTransform attackContent;
    [SerializeField] private TextMeshProUGUI attackText;

    private IconFoe sourceFoe;
    private FoePiece sourcePiece;

    public void GiveFoeSource(IconFoe src, FoePiece pees, bool editMode = true)
    {
        sourceFoe = src;
        sourcePiece = pees;

        BuildPanel(editMode);
    }

    private void BuildPanel(bool editMode)
    {
        if (sourceFoe.textInHPFlag)
        {
            if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
            {
                hpInput.gameObject.SetActive(true);
                hpInputJustText.gameObject.SetActive(false);

                hpInput.transform.GetChild(2).gameObject.SetActive(true); //alert frame
                hpInput.text = "";
                hpInputPlaceholder.GetComponent<TextMeshProUGUI>().text = sourceFoe.textHP;
            }
            else
            {
                hpInput.gameObject.SetActive(false);
                hpInputJustText.gameObject.SetActive(true);

                hpInputJustText.text = sourceFoe.textHP;
            }
        }
        else
        {
            if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
            {
                hpInput.gameObject.SetActive(true);
                hpInputJustText.gameObject.SetActive(false);

                hpInput.transform.GetChild(2).gameObject.SetActive(false);
                hpInput.SetTextWithoutNotify((sourceFoe.hp + sourceFoe.addedHP).ToString());
            }
            else
            {
                hpInput.gameObject.SetActive(false);
                hpInputJustText.gameObject.SetActive(true);

                hpInputJustText.text = (sourceFoe.hp + sourceFoe.addedHP).ToString();
            }

            if (sourceFoe.currentHP == 0)
                sourcePiece.SetPieceFaded(true);
            else
                sourcePiece.SetPieceFaded(false);
        }

        panelModeLabel.text = editMode ? "Piece Edit" : "Piece Info"; //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker) ? "Piece Edit" : "Piece Info";

        if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
        {
            sizeInput.gameObject.SetActive(true);
            sizeInputJustText.gameObject.SetActive(false);

            sizeInput.SetTextWithoutNotify(sourceFoe.size.ToString());
        }
        else
        {
            sizeInput.gameObject.SetActive(false);
            sizeInputJustText.gameObject.SetActive(true);

            sizeInputJustText.text = sourceFoe.size.ToString();
        }

        if (sourceFoe.type == FoeType.Legend)
        {
            if (editMode) //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode)
            {
                phaseInput.gameObject.SetActive(true);
                phaseInputJustText.gameObject.SetActive(false);

                phaseInput.SetTextWithoutNotify((sourceFoe.currentPhase + 1).ToString());
            }
            else
            {
                phaseInput.gameObject.SetActive(false);
                phaseInputJustText.gameObject.SetActive(true);

                phaseInputJustText.text = (sourceFoe.currentPhase + 1).ToString();
            }
        }
        else
        {
            phaseInput.gameObject.SetActive(false);
            phaseInputJustText.gameObject.SetActive(false);
        }

        int chapterNum = 1; // Mathf.CeilToInt((float)sourceFoe.level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;

        nameLabel.text = sourceFoe.unitName;
        levelLabel.text = "·Chapter - <b>" + sourceFoe.level + "</b>";
        defenseLabel.text = "·Defense - <b>" + sourceFoe.defense + "</b>";
        speedLabel.text = "·Speed - <b>" + sourceFoe.speed + " <i>(Dash " + sourceFoe.dash + ")</i>" + "</b>";
        //attackLabel.text = "Attack Bonus - <b>" + sourceFoe.attack + "</b>";
        frayDamageLabel.text = "·Fray Damage - <b>" + sourceFoe.frayDamage + "</b>";
        damageLabel.text = "·Damage - <b>" + sourceFoe.damage + "</b>";

        string className = "";

        if (sourceFoe.type == FoeType.Mob)
            className = UnitManager._instance.foes.mobs[sourceFoe.classIndex].name;
        else if (sourceFoe.type == FoeType.SpecialSummon)
            className = UnitManager._instance.summons.specialSummons[sourceFoe.classIndex].name;
        else if (sourceFoe.type == FoeType.Elite)
            className = UnitManager._instance.foes.eliteClasses[sourceFoe.classIndex].name;
        else if (sourceFoe.type == FoeType.Foe)
            className = UnitManager._instance.foes.classes[sourceFoe.classIndex].name;
        else
            className = "Legend"; //  UnitManager._instance.foes.legendClasses[sourceFoe.classIndex].name;

        List<string> dataLiens = sourceFoe.GetFoeData();

        if (dataLiens.Count > 1) 
        {
            classLabel.text = dataLiens[0];
            jobLabel.text = dataLiens[1];
            jobLabel.gameObject.SetActive(true);
        }
        else
        {
            classLabel.text = dataLiens[0];
            jobLabel.gameObject.SetActive(false);
        }

        if (sourceFoe.type != FoeType.SpecialSummon)
            classLabel.text = "Class - <b>" + className + "</b>";
        else
        {
            int startBracket = className.IndexOf("[");
            int endBracket = className.IndexOf("]");

            string baseClass = className.Substring(0, startBracket);
            
            string summonedBy = className.Substring(startBracket + 1, (endBracket - startBracket) - 1);

            classLabel.text = baseClass;
            jobLabel.text = "Summon of " + summonedBy;
            jobLabel.gameObject.SetActive(true);
        }

        traitLabel.text = "";
        traitLabel.ForceMeshUpdate();

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerConfig(sourceFoe);

        UpdateTraitAndAttack();

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    private void UpdateTraitAndAttack()
    {
        //update traits and attacks
        UpdateTraits();

        UpdateAttacks();
    }

    public void UpdateTraits()
    {
        sourceFoe.ChangePhase(sourceFoe.currentPhase);

        string traitList = "<size=150%><i>Traits</i><size=100%>\n\n";

        for (int i = 0; i < sourceFoe.traits.Count; i++)
        {
            traitList += "<b>" + sourceFoe.traits[i].traitName + "</b>\n";

            string desc = sourceFoe.traits[i].traitDescription.Replace("\\n", "<br>");
            traitList += desc + "\n\n";
        }

        Vector2 sd = traitContent.sizeDelta;
        sd.y = 10f;
        traitContent.sizeDelta = sd;

        traitLabel.text = traitList;
        traitLabel.ForceMeshUpdate();

        sd = traitContent.sizeDelta;
        sd.y = 1.1f * traitLabel.renderedHeight;
        traitContent.sizeDelta = sd;
        traitContent.anchoredPosition = Vector2.zero;

        gameObject.SetActive(true);
        StartCoroutine(DelayedTraitDimensionRecalculate());
    }

    public void UpdateAttacks()
    {
        //clean attack list
        for (int i = attackContent.childCount - 1; i > 1; i--)
        {
            Destroy(attackContent.GetChild(i).gameObject);
        }

        Vector2 sd = attackContent.sizeDelta;
        sd.y = 10f;
        traitContent.sizeDelta = sd;

        List<ClassData.Ability> foeAttacks = sourceFoe.GetAbilities();

        string attackComp = "<size=150%><b>Abilities</b><size=100%>\n";

        for (int i = 0; i < foeAttacks.Count; i++)
        {
            attackComp += "\n\n><b>" + foeAttacks[i].abilityName;

            attackComp += " (";

            attackComp += foeAttacks[i].actionCost + " action";

            string[] aspects = foeAttacks[i].abilityAspects;
            if (aspects != null && aspects.Length > 0)
            {
                attackComp += ", ";
                for (int c = 0; c < aspects.Length; c++)
                {
                    attackComp += aspects[c];
                    if (c < (aspects.Length - 1))
                        attackComp += ", ";
                    else
                        attackComp += ")";
                }
            }
            else
                attackComp += ")";

            string eff = foeAttacks[i].abilityEffect.Replace("\\n", "<br>");
            attackComp += ":</b> <i>" + eff + "</i>";
        }

        attackText.text = attackComp;
        attackText.ForceMeshUpdate();
        StartCoroutine(DelayedAttackDimensionRecalculate());
    }

    private IEnumerator DelayedTraitDimensionRecalculate()
    {
        yield return new WaitForEndOfFrame();

        Vector2 sd = traitContent.sizeDelta;
        sd.y = 1.2f * traitLabel.renderedHeight;
        traitContent.sizeDelta = sd;
        traitContent.anchoredPosition = Vector2.zero;
    }

    private IEnumerator DelayedAttackDimensionRecalculate()
    {
        yield return new WaitForEndOfFrame();

        Vector2 sd = attackContent.sizeDelta;
        sd.y = 1.2f * attackText.renderedHeight;
        attackContent.sizeDelta = sd;
        attackContent.anchoredPosition = Vector2.zero;
    }

    //HP
    public void OnHPInput()
    {
        /*
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;
        */

        sourceFoe.SetFreshFlag(false);

        int hpInputValue = 0;
        if (int.TryParse(hpInput.text, out hpInputValue))
        {
            if (hpInputValue <= 0)
            {
                if (sourceFoe.textInHPFlag)
                {
                    hpInput.text = "";
                    hpInputPlaceholder.GetComponent<TextMeshProUGUI>().text = sourceFoe.textHP;
                }
                else
                    hpInput.SetTextWithoutNotify(sourceFoe.hp.ToString());

                return;
            }

            if (sourceFoe.textInHPFlag)
            {
                sourceFoe.CorrectTextHP(hpInputValue);

                hpInput.transform.GetChild(2).gameObject.SetActive(false);
                hpInput.SetTextWithoutNotify(sourceFoe.hp.ToString());
            }
            else
            {
                int diff = hpInputValue - sourceFoe.hp;
                sourceFoe.GiveAddedHP(diff);
                sourceFoe.GiveCurrentHP(Mathf.Clamp(sourceFoe.currentHP, 0, (sourceFoe.hp + sourceFoe.addedHP)));

                hpInput.SetTextWithoutNotify((sourceFoe.hp + diff).ToString());
            }
        }

        if (sourceFoe.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    public void OnSizeInput()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;

        sourceFoe.SetFreshFlag(false);

        int sizeInputValue = 0;
        if (int.TryParse(sizeInput.text, out sizeInputValue))
        {
            if (sizeInputValue < 0)
                return;

            PieceManager._instance.SizeChangeCall(sizeInputValue);

            sizeInputValue = Mathf.Clamp(sizeInputValue, 1, PieceManager._instance.pieceMaxSize);
            sourceFoe.GiveSize(sizeInputValue);
            sizeInput.SetTextWithoutNotify(sourceFoe.size.ToString());
        }

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    public void OnPhaseInput()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;

        sourceFoe.SetFreshFlag(false);

        int phaseInputValue = 0;
        if (int.TryParse(phaseInput.text, out phaseInputValue))
        {
            if (phaseInputValue < 0)
                return;

            int phaseCount = UnitManager._instance.foes.legendClasses[sourceFoe.classIndex].phaseAspects.Length;
            phaseInputValue = Mathf.Clamp(phaseInputValue, 1, phaseCount);
            sourceFoe.ChangePhase(phaseInputValue - 1);
            phaseInput.SetTextWithoutNotify(phaseInputValue.ToString());

            //update traits and attacks
            UpdateTraitAndAttack();
        }
    }

    public void AddHP(bool add)
    {
        sourceFoe.SetFreshFlag(false);

        int startVal = sourceFoe.currentHP;
        startVal += add ? 1 : -1;

        startVal = Mathf.Clamp(startVal, 0, sourceFoe.hp + sourceFoe.addedHP);

        sourceFoe.GiveCurrentHP(startVal);

        if (sourceFoe.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);
    }

    public void AddVigor(bool add)
    {
        sourceFoe.SetFreshFlag(false);

        int auxCV = sourceFoe.currentVigor;
        int auxV = sourceFoe.vigor;

        sourceFoe.AddVigor(add);
    }

    public IconFoe GetFoeData()
    {
        return sourceFoe;
    }

    public void UpdatePieceParts()
    {
        if (sourceFoe == null)
            return;

        int[] ids = PieceCamera._instance.GetCurrentSamplePartIDs();
        sourceFoe.GivePartIDs(ids[0], ids[1], ids[2], ids[3]);

        applyToBaseButton.SetActive(CheckPieceBaseDiff());
    }

    private bool CheckPieceBaseDiff()
    {
        if (sourceFoe == null)
            return false;

        IconFoe baseFoe = UnitManager._instance.GetFoe(sourceFoe.unitID);

        bool showDebug = false;

        /*
        if (baseFoe.hp != sourceFoe.hp)
        {
            if (showDebug)
                Debug.Log("Diff in HP");
            return true;
        }
        else if (baseFoe.size != sourceFoe.size)
        {
            if (showDebug)
                Debug.Log("Diff in Size");
            return true;
        }
        else if (baseFoe.armor != sourceFoe.armor)
        {
            if (showDebug)
                Debug.Log("Diff in Armor");
            return true;
        }
        */
        if (baseFoe.headPartID != sourceFoe.headPartID)
        {
            if (showDebug)
                Debug.Log("Diff in Head Piece ID");
            return true;
        }
        else if (baseFoe.lWeaponPartID != sourceFoe.lWeaponPartID)
        {
            if (showDebug)
                Debug.Log("Diff in L Piece ID");
            return true;
        }
        else if (baseFoe.rWeaponPartID != sourceFoe.rWeaponPartID)
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
        if (sourceFoe == null)
            return;

        UnitManager._instance.UpdateFoePiece(sourceFoe.unitID, sourceFoe);
        UnitManager._instance.optionsManager.SaveFoe(sourceFoe.unitID);

        UpdateApplyDiffButton();
    }
}
