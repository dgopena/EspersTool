using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerClickHandler,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler {

    public bool isDown { get; private set; }

    [Header("UI")] [SerializeField] private Image buttonGraphic;
    private Color baseColor;
    [SerializeField] private bool brightOnHover = false;
    [SerializeField] private float brightenFactor = 0.2f;
    
    [Header("Events")]
    public Button.ButtonClickedEvent onDown;
    public Button.ButtonClickedEvent onHold;
    public Button.ButtonClickedEvent onRelease;

    private void Awake()
    {
        if(buttonGraphic != null)
            baseColor = buttonGraphic.color;
    }

    void Update()
    {
        if (onHold != null && isDown)
            onHold.Invoke();
    }

    public void OnPointerClick(PointerEventData pointerData){}

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        if (onDown != null)
            onDown.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(brightOnHover)
            BrightenButton(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
        
        if(brightOnHover)
            BrightenButton(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDown && onRelease != null)
            onRelease.Invoke();
        isDown = false;
    }

    private void BrightenButton(bool enable)
    {
        if (!enable)
        {
            buttonGraphic.color = baseColor;
        }
        else
        {
            Color bColor = (1f + brightenFactor) * baseColor;
            bColor.a = 1f;
            
            buttonGraphic.color = bColor;
        }
    }
}
