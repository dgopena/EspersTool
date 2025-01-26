using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;

public class FoePiece : UnitPiece
{
    public EsperFoe foeData { get; private set; }

    public void GiveData(EsperFoe data)
    {
        foeData = data.MakeCopy();
        unitName = foeData.unitName;
    }

    public void BuildPiece()
    {
        BuildPiece(foeData);
    }

    public override void BuildPiece(EsperUnit source)
    {
        base.BuildPiece(source);
    }

    #region Old Mini Panel Code
    /*
    public override void UpdateMiniPanel()
    {
        ShowPieceTools(true);

        Color foeColor = 0.5f * foeData.colorChoice;
        foeColor.a = 1f;

        miniPanel.GetChild(0).GetComponent<Image>().color = foeColor;
        miniPanel.GetChild(0).GetChild(1).GetComponent<Image>().color = foeColor;

        UpdateMiniPanelHealthBars();

        miniPanel.GetChild(3).GetComponent<TextMeshProUGUI>().text = foeData.unitName;

        miniPanel.GetChild(4).GetComponent<CanvasGroup>().alpha = foeData.blessingTokens > 0 ? 1f : 0f; //----------------------------------

        UpdateMiniPanelStatus();

        int vitValue = Mathf.CeilToInt(0.25f * foeData.hp);

        string statString = "Vitality : " + vitValue + "\n\n";
        statString += "Defense : " + foeData.defense + "\n\n";
        statString += "Speed : " + foeData.speed + " (Dash " + foeData.dash + ")\n\n";
        statString += "Damage : " + foeData.damage + "\n\n";
        statString += "Fray : " + foeData.frayDamage + "\n\n";
        statString += foeData.attackType;

        miniPanel.GetChild(8).GetComponent<TextMeshProUGUI>().text = statString;
    }

    private void UpdateMiniPanelStatus()
    {
        RectTransform statusSet = miniPanel.GetChild(5).GetComponent<RectTransform>();

        for (int i = statusSet.childCount - 1; i > 0; i--)
        {
            Destroy(statusSet.GetChild(i).gameObject);
        }

        //possitives
        for (int i = 0; i < foeData.activePositiveEffects.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for (int p = 0; p < PieceManager._instance.statusInfo.displayEffects.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayEffects[p].effect != foeData.activePositiveEffects[i])
                    continue;

                iconIMG.sprite = PieceManager._instance.statusInfo.displayEffects[p].image;
                iconIMG.color = PieceManager._instance.possitiveEffectColor;
            }

            nuIcon.gameObject.SetActive(true);
        }

        //status
        for (int i = 0; i < foeData.activeStatus.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for (int p = 0; p < PieceManager._instance.statusInfo.displayStatus.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayStatus[p].status != foeData.activeStatus[i])
                    continue;

                iconIMG.sprite = PieceManager._instance.statusInfo.displayStatus[p].image;
                iconIMG.color = PieceManager._instance.statusEffectColor;
            }

            nuIcon.gameObject.SetActive(true);
        }

        //blights
        for (int i = 0; i < foeData.activeBlights.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for (int p = 0; p < PieceManager._instance.statusInfo.displayBlights.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayBlights[p].blight != foeData.activeBlights[i])
                    continue;

                iconIMG.sprite = PieceManager._instance.statusInfo.displayBlights[p].image;
                iconIMG.color = PieceManager._instance.blightColor;
            }

            nuIcon.gameObject.SetActive(true);
        }
    }

    private void UpdateMiniPanelHealthBars()
    {
        float auxFill = 0f;

        Transform barControls = miniPanel.GetChild(6);

        if (!foeData.textInHPFlag)
        {
            auxFill = (float)foeData.currentHP / (float)(foeData.hp + foeData.addedHP);

            miniPanel.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = auxFill;

            barControls.GetChild(2).GetComponent<TextMeshProUGUI>().text = foeData.currentHP.ToString();
            barControls.GetChild(3).GetComponent<TextMeshProUGUI>().text = (foeData.hp + foeData.addedHP).ToString();

        }
        else
            Debug.Log("text in HP");

        auxFill = 0f;
        if (foeData.vigor != 0)
            auxFill = (float)foeData.currentVigor / (float)(foeData.vigor);

        miniPanel.GetChild(2).GetChild(0).GetComponent<Image>().fillAmount = auxFill;

        barControls.GetChild(0).GetComponent<TextMeshProUGUI>().text = foeData.currentVigor.ToString();
        barControls.GetChild(1).GetComponent<TextMeshProUGUI>().text = foeData.vigor.ToString();
    }

    public override void ShowPieceTools(bool show)
    {
        Transform pieceTools = miniPanel.GetChild(7);

        pieceTools.GetChild(0).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker);
        pieceTools.GetChild(1).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.GameMode);
    }
    */
    #endregion

    public override void ModifyHealth(int value)
    {
        int hpValue = foeData.currentHP + value;
        if (hpValue < 0)
            hpValue = 0;
        else if (hpValue > (foeData.baseHP + foeData.addedHP))
            hpValue = (foeData.baseHP + foeData.addedHP);

        foeData.GiveCurrentHP(hpValue);

        SetPieceFaded(foeData.currentHP == 0);

        //UpdateMiniPanelHealthBars();
    }

}
