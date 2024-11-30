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

    public void SetUpCard(int number, int suit, string actionName, Sprite suitSprite, Color suitColor)
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
        overLabelCG.alpha = isFocused ? 1f : 0f;
    }

    public void ToggleSelection()
    {
        SelectCard(!isSelected);
    }

    public void SelectCard(bool select)
    {
        //do UI action
        //maybe check focus
        selectFrame.SetActive(select);

        isSelected = select;

        if (parentMat != null) 
        {
            if (select)
                parentMat.selectedCardNumbers++;
            else
                parentMat.selectedCardNumbers--;
        }

        if (parentMat != null && parentMat.OnCardSelectUpdate != null)
            parentMat.OnCardSelectUpdate.Invoke();
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
