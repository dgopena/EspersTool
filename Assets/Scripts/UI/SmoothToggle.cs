using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmoothToggle : MonoBehaviour, IPointerClickHandler
{
    public bool interactable = true;

    private bool toggleSetup = false;

    [Space(10f)]
    public bool startWithDefaultValue = false;
    public bool startValue = true;

    [Space(10f)]
    public float toggleSpeed = 0.5f;

    private Vector2 startKnobPosition;
    private Vector2 endKnobPosition;

    private bool toggling = false;
    private float startChangeTimestamp;

    public bool toggleValue { get; private set; }

    private float knobValueAtTrue = 0f;
    private float knobValueAtFalse = 0f;
    private bool labelChanged = false;

    [Space(10f)]
    [SerializeField] private RectTransform circleRT;
    [SerializeField] private RectTransform knobRT;
    [SerializeField] private RectTransform togglerRT;

    [Space(10f)]
    public bool useChangeableLabel = true;
    public TextMeshProUGUI toggleLabel;
    public string onValue;
    public string offValue;

    [Space(10f)]
    public UnityEvent OnValueChange;

    private void Awake()
    {
        SetupToggler();
    }

    private void LateUpdate()
    {
        if (!toggling)
            return;

        startChangeTimestamp += Time.deltaTime;
        float t = startChangeTimestamp / toggleSpeed;
        Vector2 curPos = Vector2.Lerp(startKnobPosition, endKnobPosition, t);

        if (useChangeableLabel)
        {
            if(t < 0.5f)
            {
                float a = Mathf.Lerp(1f, 0f, 2f * t);

                Color labelCol = toggleLabel.color;
                labelCol.a = a;
                toggleLabel.color = labelCol;
            }
            else
            {
                if (!labelChanged)
                {
                    toggleLabel.text = toggleValue ? onValue : offValue;
                    labelChanged = true;
                }

                float a = Mathf.Lerp(0f, 1f, 2f * (t - 0.5f));

                Color labelCol = toggleLabel.color;
                labelCol.a = a;
                toggleLabel.color = labelCol;
            }
        }

        knobRT.anchoredPosition = curPos;

        if (t > 1f)
        {
            toggling = false;
            knobRT.anchoredPosition = endKnobPosition;
        }
    }

    public void ForceValue(bool value)
    {
        if (!toggleSetup)
            SetupToggler();

        if (!value)
            knobRT.anchoredPosition = new Vector2(knobValueAtFalse, 0f);
        else
            knobRT.anchoredPosition = new Vector2(knobValueAtTrue, 0f);

        toggleValue = value;

        if (useChangeableLabel)
            toggleLabel.text = toggleValue ? onValue : offValue;
    }

    private void SetupToggler()
    {
        knobValueAtFalse = togglerRT.rect.width - circleRT.rect.width;

        toggleLabel.enabled = useChangeableLabel;

        toggleSetup = true;

        if (startWithDefaultValue)
        {
            ForceValue(startValue);
        }
    }

    public void CallToggle()
    {
        if (!interactable)
            return;

        if (toggling)
            return;

        if (toggleValue)
        {
            startKnobPosition = knobValueAtTrue * Vector2.right;
            endKnobPosition = knobValueAtFalse * Vector2.right;
        }
        else
        {
            startKnobPosition = knobValueAtFalse * Vector2.right;
            endKnobPosition = knobValueAtTrue * Vector2.right;
        }

        startChangeTimestamp = 0f;

        toggleValue = !toggleValue;

        if (OnValueChange != null)
            OnValueChange.Invoke();

        toggling = true;
        labelChanged = false;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        CallToggle();
    }
}
