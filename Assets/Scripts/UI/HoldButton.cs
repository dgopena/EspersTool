using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerClickHandler,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler,IPointerUpHandler {

    public bool isDown { get; private set; }

    public Button.ButtonClickedEvent onDown;
    public Button.ButtonClickedEvent onHold;
    public Button.ButtonClickedEvent onRelease;

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
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDown = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDown && onRelease != null)
            onRelease.Invoke();
        isDown = false;
    }
}
