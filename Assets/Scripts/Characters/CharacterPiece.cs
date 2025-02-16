using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using TMPro;
using System;

public class CharacterPiece : UnitPiece
{
    public EsperCharacter characterData { get; private set; }

    public bool usingCardMode = true;

    private PlayerDeck pieceDeck;
    private FateHandWidget cardHandsPanel;
    
    public void GiveData(EsperCharacter data)
    {
        characterData = data.MakeCopy();

        unitName = characterData.unitName;
    }

    public void BuildPiece()
    {
        BuildPiece(characterData);
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

        Color charaColor = 0.5f * characterData.colorChoice;
        charaColor.a = 1f;

        miniPanel.GetChild(0).GetComponent<Image>().color = charaColor;
        miniPanel.GetChild(0).GetChild(1).GetComponent<Image>().color = charaColor;

        UpdateMiniPanelHealthBars();

        miniPanel.GetChild(3).GetComponent<TextMeshProUGUI>().text = characterData.unitName;
        
        miniPanel.GetChild(4).GetComponent<CanvasGroup>().alpha = characterData.blessingTokens > 0 ? 1f : 0f; //-----------------------------

        UpdateMiniPanelStatus();

        string statString = "Vitality : " + characterData.vitality + "\n\n";
        statString += "Defense : " + characterData.defense + "\n\n";
        statString += "Speed : " + characterData.speed + " (Dash " + characterData.dash + ")\n\n";
        statString += "Damage : " + characterData.damage + "\n\n";
        statString += "Fray : " + characterData.frayDamage + "\n\n";
        statString += characterData.attackType;

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
        for (int i = 0; i < characterData.activePositiveEffects.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for(int p = 0; p < PieceManager._instance.statusInfo.displayEffects.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayEffects[p].effect != characterData.activePositiveEffects[i])
                    continue;

                iconIMG.sprite = PieceManager._instance.statusInfo.displayEffects[p].image;
                iconIMG.color = PieceManager._instance.possitiveEffectColor;
            }

            nuIcon.gameObject.SetActive(true);
        }

        //status
        for (int i = 0; i < characterData.activeStatus.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for (int p = 0; p < PieceManager._instance.statusInfo.displayStatus.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayStatus[p].status != characterData.activeStatus[i])
                    continue;

                iconIMG.sprite = PieceManager._instance.statusInfo.displayStatus[p].image;
                iconIMG.color = PieceManager._instance.statusEffectColor;
            }

            nuIcon.gameObject.SetActive(true);
        }

        //blights
        for (int i = 0; i < characterData.activeBlights.Count; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(statusSet.GetChild(0).gameObject, statusSet);
            nuIcon.transform.SetAsLastSibling();

            Image iconIMG = nuIcon.GetComponent<Image>();

            for (int p = 0; p < PieceManager._instance.statusInfo.displayBlights.Length; p++)
            {
                if (PieceManager._instance.statusInfo.displayBlights[p].blight != characterData.activeBlights[i])
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

        if (!characterData.textInHPFlag)
        {
            auxFill = (float)characterData.currentHP / (float)((characterData.hp + characterData.addedHP) * (0.25f * (4 - woundCount)));

            miniPanel.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = auxFill;

            barControls.GetChild(2).GetComponent<TextMeshProUGUI>().text = characterData.currentHP.ToString();
            barControls.GetChild(3).GetComponent<TextMeshProUGUI>().text = ((characterData.hp + characterData.addedHP) * (0.25f * (4 - woundCount))).ToString();
        }

        auxFill = 0f;
        if (characterData.vigor != 0)
            auxFill = (float)characterData.currentVigor / (float)(characterData.vigor);

        miniPanel.GetChild(2).GetChild(0).GetComponent<Image>().fillAmount = auxFill;

        barControls.GetChild(0).GetComponent<TextMeshProUGUI>().text = characterData.currentVigor.ToString();
        barControls.GetChild(1).GetComponent<TextMeshProUGUI>().text = characterData.vigor.ToString();
    }

    public override void ShowPieceTools(bool show)
    {
        Transform pieceTools = miniPanel.GetChild(7);

        if (!show)
        {
            pieceTools.GetChild(0).gameObject.SetActive(false);
            pieceTools.GetChild(1).gameObject.SetActive(false);

            return;
        }


        pieceTools.GetChild(0).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker);
        pieceTools.GetChild(1).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.GameMode);

        if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode)
        {
            pieceTools.GetChild(1).GetChild(3).gameObject.SetActive(isSeer);
        }
    }
    */
    #endregion

    public override void ModifyHealth(int value)
    {
        characterData.SetFreshFlag(false);

        int hpValue = characterData.currentHP + value;
        if (hpValue < 0)
            hpValue = 0;

        characterData.GiveCurrentHP(hpValue);

        SetPieceFaded(characterData.currentHP == 0);
    }

    public Tuple<string, int>[] GetActiveBuffs(int statIndex, int cardNumber, bool attack)
    {
        return characterData.GetBuffs(statIndex, cardNumber, attack);
    }
    
    #region Cards
    
    public void SetUsingDeck(bool value)
    {
        usingCardMode = value;
    }

    public void SetFateDeck(Canvas baseCanvas, bool newDeck = true)
    {
        //we create its playerdeck UI instance
        GameObject newDeckDisplay = GameObject.Instantiate(PieceManager._instance.cardDeckPrefab, PieceManager._instance.pieceDeckParent);
        pieceDeck = newDeckDisplay.transform.GetChild(0).GetComponent<PlayerDeck>();
        pieceDeck.baseCanvas = baseCanvas;
        newDeckDisplay.SetActive(false);
        
        GameObject newHandDisplay = GameObject.Instantiate(PieceManager._instance.cardHandWidgetPrefab, PieceManager._instance.pieceDeckParent);
        cardHandsPanel = newHandDisplay.GetComponent<FateHandWidget>();
        cardHandsPanel.playerDeck = pieceDeck;
        cardHandsPanel.handSource = pieceDeck.GetHandMat();
        newHandDisplay.SetActive(false);
        
        if(newDeck)
            pieceDeck.SetDeckUp();
    }
    
    public void SetFateDeck(Canvas baseCanvas, int[] handNumbers, int[] handSuits, int[] fateNumbers, int[] fateSuits,
        int[] discardNumbers, int[] discardSuits, int[] aetherNumbers, int[] aetherSuits)
    {
        int totalCards = handNumbers.Length + fateNumbers.Length + discardNumbers.Length + aetherNumbers.Length;
        
        //Debug.Log("Received total cards: " + totalCards);
        
        if (totalCards == 0)
        {
            //the unit has no deck assigned
            SetFateDeck(baseCanvas);
            cardHandsPanel.SetUpDisplay();
            return;
        }
        
        SetFateDeck(baseCanvas,false);
        
        pieceDeck.BuildFromIntArrays(handNumbers, handSuits, fateNumbers, fateSuits, discardNumbers, discardSuits, aetherNumbers, aetherSuits);
        cardHandsPanel.SetUpDisplay();
    }

    public PlayerDeck.IntDeck GetCurrentDeck()
    {
        return pieceDeck.BuildIntDeckFromMats();
    }

    public void EnableCards(bool enable)
    {
        cardHandsPanel.gameObject.SetActive(enable);
        
        
    }

    public FateHandWidget GetCardHandPanel()
    {
        return cardHandsPanel;
    }
    
    #endregion
}
