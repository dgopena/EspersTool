using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FateCard : MonoBehaviour
{
    public int cardNumber { get; private set; } //J-11, Q-12, K-13
    public int cardSuit { get; private set; } //1-spade,2-club,3-diamond,4-heart

    private bool isFocused = false;
    private bool isSelected = false;
    private bool isHidden = false;

    public bool IsSelected => isSelected;

    private CardMat parentMat;

    [Header("UI")]
    public RectTransform cardRT;
    public Animator cardAnim;
    public RectTransform graphicRT;
    public GameObject selectFrame;
    public GameObject backImage;

    public float baseWidth { get { return 125f; } }
    public float baseHeight { get { return 200f; } }

    public Image suitImage;
    public TextMeshProUGUI actionLabel;
    public TextMeshProUGUI numberLabel;

    public Image suitURImage;
    public TextMeshProUGUI numberURLabel;

    public Image suitLLImage;
    public TextMeshProUGUI numberLLLabel;

    [Space(10f)]
    [SerializeField] private CanvasGroup overLabelCG;
    [SerializeField] private Image overLabelSuitImage;
    [SerializeField] private TextMeshProUGUI overLabelActionLabel;
    [SerializeField] private TextMeshProUGUI overLabelNumberLabel;

    public void SetUpCard(int number, int suit, string actionName, Sprite suitSprite, Color suitColor, bool startHidden = false)
    {
        cardNumber = number;
        cardSuit = suit;

        actionLabel.text = overLabelActionLabel.text = actionName;
        suitImage.sprite = suitURImage.sprite = suitLLImage.sprite = overLabelSuitImage.sprite = suitSprite;
        suitImage.color = suitURImage.color = suitLLImage.color = overLabelSuitImage.color = suitColor;

        overLabelCG.alpha = 0f;

        string numberText = number.ToString();
        if (number == 11)
            numberText = "J";
        else if (number == 12)
            numberText = "Q";
        else if (number == 13)
            numberText = "K";

        numberLabel.text = numberURLabel.text = numberLLLabel.text = overLabelNumberLabel.text = numberText;

        if (startHidden)
            HideCard(true);
    }

    public void ToggleFocus()
    {
        FocusCard(!isFocused);
    }

    public void FocusCard(bool focus)
    {
        if (isSelected)
            return;

        //do UI action
        if (focus)
            cardAnim.SetTrigger("FocusOn");
        else
            cardAnim.SetTrigger("FocusOff");

        isFocused = focus;
            
        /*
        if (isFocused && !isHidden)
            overLabelCG.alpha = 1f;
        else
            overLabelCG.alpha = 0f;
            */
    }

    public void ShowOverLabel(bool show)
    {
        if (show && !isHidden)
            overLabelCG.alpha = 1f;
        else
            overLabelCG.alpha = 0f;
    }

    public void ToggleSelection()
    {
        SelectCard(!isSelected);
    }

    public void SelectCard(bool select, bool UIUpdate = true)
    {
        isSelected = select;
        
        selectFrame.SetActive(select);


        if (parentMat != null) 
        {
            if (select)
                parentMat.selectedCardNumbers++;
            else
                parentMat.selectedCardNumbers--;
        }

        if (parentMat != null && parentMat.OnCardSelectUpdate != null)
            parentMat.OnCardSelectUpdate.Invoke();

        if (isSelected && !isHidden)
            overLabelCG.alpha = 1f;
        else
            overLabelCG.alpha = 0f;
    }

    public void ToggleHidden()
    {
        HideCard(!isHidden);
    }

    public void HideCard(bool value)
    {
        isHidden = value;
        backImage.SetActive(isHidden);

        if (!isHidden && isSelected)
            overLabelCG.alpha = 1f;
        else if (isHidden)
            overLabelCG.alpha = 0f;
    }

    public bool Equals(FateCard comparison)
    {
        Debug.Log(">> " + cardNumber + "|" + comparison.cardNumber);
        Debug.Log(">>>> " + cardSuit + "|" + comparison.cardSuit);
        
        return cardNumber == comparison.cardNumber && 
               cardSuit == comparison.cardSuit;
    }
    
    public void ChangeScaleByFactor(float newHeight)
    {
        float factor = newHeight / baseHeight;
        cardRT.localScale = factor * Vector3.one;
    }

    public void SetParentMat(CardMat parent)
    {
        parentMat = parent;
    }
}
