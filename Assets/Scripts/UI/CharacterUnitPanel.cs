using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class CharacterUnitPanel : UnitPanel
{
    [Header("Character Aspects")]
    [SerializeField] private TextMeshProUGUI classLabel;

    [Space(10f)]
    [SerializeField] private GameObject modifierPrefab;

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

        panelModeLabel.text = (MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker || editMode) ? "Piece Edit" : "Piece Info";

        int chapterNum = 1; // Mathf.CeilToInt((float)sourceCharacter.level / 4f);
        if (chapterNum < 1)
            chapterNum = 1;

        nameLabel.text = sourceCharacter.unitName;
        levelLabel.text = "·Level - <b>" + sourceCharacter.level + "</b>";
        defenseLabel.text = "·Defense - <b>" + sourceCharacter.defense + "</b>";
        speedLabel.text = "·Speed - <b>" + sourceCharacter.speed;

        classLabel.text = "Class - <b>" + UnitManager._instance.classes.classes[sourceCharacter.classIndex].name + "</b>";
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
                hpInput.SetTextWithoutNotify(sourceCharacter.hp.ToString());

                return;
            }

            int diff = hpInputValue - sourceCharacter.hp;
            sourceCharacter.GiveAddedHP(diff);
            sourceCharacter.GiveCurrentHP(Mathf.Clamp(sourceCharacter.currentHP, 0, GetCharaCurrentMaxHP()));

            hpInput.SetTextWithoutNotify((sourceCharacter.hp + diff).ToString());
        }

        if (sourceCharacter.currentHP == 0)
            sourcePiece.SetPieceFaded(true);
        else
            sourcePiece.SetPieceFaded(false);
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
            return Mathf.CeilToInt((sourceCharacter.hp + sourceCharacter.addedHP));
        }
        else
            return (sourceCharacter.hp + sourceCharacter.addedHP);
    }

    public IconCharacter GetCharacterData()
    {
        return sourceCharacter;
    }

    public void ApplyDiffToBase()
    {
        if (sourceCharacter == null)
            return;

        UnitManager._instance.UpdateCharacterPiece(sourceCharacter.unitID, sourceCharacter);
        UnitManager._instance.optionsManager.SaveCharacter(sourceCharacter.unitID);
    }
}
