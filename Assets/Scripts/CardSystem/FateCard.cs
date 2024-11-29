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

    [Header("UI")]
    public RectTransform cardRT;
    public GameObject selectFrame;
    public GameObject backImage;

    public float baseWidth { get { return 125f; } }
    public float baseHeight { get { return 175f; } }

    public Image suitImage;
    public TextMeshProUGUI actionLabel;
    public TextMeshProUGUI numberLabel;

    public Image suitURImage;
    public TextMeshProUGUI numberURLabel;

    public Image suitLLImage;
    public TextMeshProUGUI numberLLLabel;

    public void SetUpCard(int number, int suit, string actionName, Sprite suitSprite, Color suitColor)
    {
        cardNumber = number;
        cardSuit = suit;

        actionLabel.text = actionName;
        suitImage.sprite = suitURImage.sprite = suitLLImage.sprite = suitSprite;
        suitImage.color = suitURImage.color = suitLLImage.color = suitColor;

        string numberText = number.ToString();
        if (number == 11)
            numberText = "J";
        else if (number == 12)
            numberText = "Q";
        else if (number == 13)
            numberText = "K";

        numberLabel.text = numberURLabel.text = numberLLLabel.text = numberText;
    }

    public void ToggleFocus()
    {
        FocusCard(!isFocused);
    }

    public void FocusCard(bool focus)
    {
        //do UI action

        isFocused = focus;
    }

    public void ToggleSelection()
    {
        SelectCard(!isSelected);
    }

    public void SelectCard(bool select)
    {
        //do UI action

        isSelected = select;
    }

    public void ChangeScaleByFactor(float newHeight)
    {
        float factor = newHeight / baseHeight;
        cardRT.localScale = factor * Vector3.one;
    }
}
