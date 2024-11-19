using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeerCard : MonoBehaviour
{
    [SerializeField] private RectTransform cardFront;
    [SerializeField] private RectTransform cardBack;

    private RectTransform cardRect;

    public bool startBackFacing = true;

    public bool isFlipped { get; private set; }

    private float flipSpeed;
    private int flipHalf;
    private bool flipping;
    private bool flippingHorizontal;
    private float flipStartTime;

    [Space(20f)]
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private Image cardIcon;
    public string cardTitle;
    public string cardDescription;
    [SerializeField] private Sprite cardSprite;


    private void Awake()
    {
        cardRect = gameObject.GetComponent<RectTransform>();

        titleLabel.text = cardTitle;
        cardIcon.sprite = cardSprite;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (flipping)
        {
            if (flipHalf == 0)
            {
                float t = (Time.time - flipStartTime) / flipSpeed;

                float tValue = 0f;
                if (isFlipped)
                    tValue = Mathf.Lerp(1f, 0f, t);
                else
                    tValue = Mathf.Lerp(-1f, 0f, t);

                Vector3 scala = cardRect.localScale;
                if (flippingHorizontal)
                    scala.x = tValue;
                else
                    scala.y = tValue;

                cardRect.localScale = scala;

                if (t > 1f)
                {
                    flipStartTime = Time.time;
                    if (isFlipped)
                        cardBack.SetAsLastSibling();
                    else
                        cardFront.SetAsLastSibling();

                    flipHalf = 1;
                }
            }
            else
            {
                float t = (Time.time - flipStartTime) / flipSpeed;

                float tValue = 0f;
                if (isFlipped)
                    tValue = Mathf.Lerp(0, -1f, t);
                else
                    tValue = Mathf.Lerp(0f, 1f, t);

                Vector3 scala = cardRect.localScale;
                if (flippingHorizontal)
                    scala.x = tValue;
                else
                    scala.y = tValue;

                cardRect.localScale = scala;

                if (t > 1f)
                {
                    flipHalf = 0;
                    flipping = false;
                    isFlipped = !isFlipped;
                }
            }
        }
    }

    public void SetCardDimensions(float heightValue, float widthValue, float cardRatioHW)
    {
        float cardWidth = widthValue;
        float cardHeight = heightValue;

        if (heightValue > (1.2f * widthValue))
        {
            cardHeight = cardRatioHW * widthValue;
        }
        else
        {
            cardWidth = heightValue / cardRatioHW;
        }

        if (cardRect == null)
            cardRect = gameObject.GetComponent<RectTransform>();

        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
    }

    public void FlipCard(float flipSpeed, bool flipHorizontal = true)
    {
        if (flipping)
            return;

        this.flipSpeed = 0.5f * (1f / flipSpeed);
        flipHalf = 0;
        flipStartTime = Time.time;
        flippingHorizontal = flipHorizontal;
        flipping = true;
    }

    public void SetCardFlipped(bool flipped, bool flipHorizontal = true)
    {
        cardRect = gameObject.GetComponent<RectTransform>();

        isFlipped = flipped;
        if (flipHorizontal)
            cardRect.localScale = new Vector3(isFlipped ? 1f : -1f, 1f, 1f);
        else
            cardRect.localScale = new Vector3(1f, isFlipped ? 1f : -1f, 1f);

        if (isFlipped)
            cardFront.SetAsLastSibling();
        else
            cardBack.SetAsLastSibling();
    }
}

