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

        panelModeLabel.text = editMode ? "Piece Edit" : "Piece Info"; //(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker) ? "Piece Edit" : "Piece Info";

        int chapterNum = 1; // Mathf.CeilToInt((float)sourceFoe.level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;

        nameLabel.text = sourceFoe.unitName;
        levelLabel.text = "·Chapter - <b>" + sourceFoe.level + "</b>";
        defenseLabel.text = "·Defense - <b>" + sourceFoe.defense + "</b>";
        speedLabel.text = "·Speed - <b>" + sourceFoe.speed;

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

        classLabel.text = dataLiens[0];
        classLabel.text = "Class - <b>" + className + "</b>";

        traitLabel.text = "";
        traitLabel.ForceMeshUpdate();

        PieceCamera._instance.SetSamplerAtStartRotation();
        PieceCamera._instance.SetSamplerConfig(sourceFoe);

        UpdateAbilities();
    }

    public void UpdateAbilities()
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
                hpInput.SetTextWithoutNotify(sourceFoe.hp.ToString());

                return;
            }

            int diff = hpInputValue - sourceFoe.hp;
            sourceFoe.GiveAddedHP(diff);
            sourceFoe.GiveCurrentHP(Mathf.Clamp(sourceFoe.currentHP, 0, (sourceFoe.hp + sourceFoe.addedHP)));

            hpInput.SetTextWithoutNotify((sourceFoe.hp + diff).ToString());
        }

        if (sourceFoe.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);
    }

    public void OnSizeInput()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker)
            return;

        sourceFoe.SetFreshFlag(false);
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

    public IconFoe GetFoeData()
    {
        return sourceFoe;
    }

    public void ApplyDiffToBase()
    {
        if (sourceFoe == null)
            return;

        UnitManager._instance.UpdateFoePiece(sourceFoe.unitID, sourceFoe);
        UnitManager._instance.optionsManager.SaveFoe(sourceFoe.unitID);
    }
}
