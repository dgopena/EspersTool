using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Networking.UnityWebRequest;

public class DieWidget : MonoBehaviour
{
    [SerializeField] private bool startInInputScreen = false;
    private bool inputMode = false;

    [Header("Dice Models")]
    [SerializeField] private Transform dieWidgetCamera;
    private Transform dieSlot0;
    private Transform dieSlot1;
    private Transform dieSlot2;

    [Serializable]
    public struct PosScaleLabel
    {
        public Vector2 rePos;
        public float fontSize;
    }

    [SerializeField] private PosScaleLabel[] fontSizesBasic;
    [SerializeField] private PosScaleLabel[] fontSizesAdvantage;

    private Vector3 twist0;
    private Vector3 twist1;
    private Vector3 twist2;

    private int result0;
    private int result1;
    private int result2;

    public int finalResult { get; private set; }

    private bool midRoll = false;

    private bool widgetSet = false;

    [Space(10f)]
    [SerializeField] private float rollSpeed = 100f;
    [SerializeField] private float rollingTime = 2f;
    [SerializeField] private float rollingDelta = 0.5f;
    [SerializeField] private float rollingResultWait = 3f;

    [Header("Dice UI")]
    [SerializeField] private GameObject dieScreen;
    [SerializeField] private TextMeshProUGUI dieSlotLabel0;
    private RectTransform dieSlotRect0;
    [SerializeField] private TextMeshProUGUI dieSlotLabel1;
    private RectTransform dieSlotRect1;
    [SerializeField] private TextMeshProUGUI dieSlotLabel2;
    private RectTransform dieSlotRect2;
    [SerializeField] private CanvasGroup dieButtonCG;
    [SerializeField] private CanvasGroup sideButtonsCG;
    [SerializeField] private TextMeshProUGUI throwAcceptButtonLabel;
    
    [Space(10f)]
    [SerializeField] private GameObject dieButtonPanel;
    [SerializeField] private GameObject[] dieButtons;
    [SerializeField] private Color nonSelectedColor = Color.gray;
    private int currentDieSelection = 0;

    [Space(10f)]
    [SerializeField] private Toggle advantageCheck;
    private bool isWithAdvantage = false;

    [Space(10f)] [SerializeField] private bool tryToSquareUI = true;
    [SerializeField] private float screenFactor = 0.25f;

    [Header("Direct Input")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputScreen;

    public UnityEvent<int> OnResultGet;
    
    private void Awake()
    {
        //SetWidget(inputMode, 0, false);
    }

    private void SetUp()
    {
        dieSlot0 = dieWidgetCamera.GetChild(0);
        dieSlot1 = dieWidgetCamera.GetChild(1);
        dieSlot2 = dieWidgetCamera.GetChild(2);

        for (int i = 0; i < dieButtons.Length; i++)
        {
            int dieSelection = i;
            dieButtons[i].GetComponent<HoldButton>().onRelease.AddListener(delegate { ChangeDie(dieSelection); });
        }

        inputMode = startInInputScreen;

        widgetSet = true;
    }

    private void LateUpdate()
    {
        if (!midRoll)
            return;

        if (!isWithAdvantage)
        {
            dieSlot0.Rotate(rollSpeed * Time.deltaTime * twist0);
        }
        else
        {
            dieSlot1.Rotate(rollSpeed * Time.deltaTime * twist1);
            dieSlot2.Rotate(rollSpeed * Time.deltaTime * twist2);
        }
    }

    public void SetWidget(bool startInInput, int dieSelection, bool advantage)
    {
        if (!widgetSet)
            SetUp();

        if(tryToSquareUI)
            TrySquareUI();
        
        midRoll = false;

        dieButtonCG.alpha = 1f;
        dieButtonCG.interactable = true;
        dieButtonCG.blocksRaycasts = true;

        sideButtonsCG.alpha = 1f;
        sideButtonsCG.interactable = true;
        sideButtonsCG.blocksRaycasts = true;
        
        throwAcceptButtonLabel.transform.parent.gameObject.SetActive(false);

        inputScreen.SetActive(startInInput);
        dieScreen.SetActive(!startInInput);

        if (startInInput)
        {
            inputField.SetTextWithoutNotify("");
        }
        else
        {
            int indexDie = 0;
            if (dieSelection == 6)
                indexDie = 1;
            else if (dieSelection == 8)
                indexDie = 2;
            else if (dieSelection == 10)
                indexDie = 3;
            else if (dieSelection == 12)
                indexDie = 4;
            else if (dieSelection >= 20)
                indexDie = 5;
            
            ChangeDie(indexDie);

            dieSlotLabel0.text = dieSlotLabel1.text = dieSlotLabel2.text = "";

            SetAdvantage(advantage);
        }
    }

    private void TrySquareUI()
    {
        float sizeLength = screenFactor * Screen.width;
        
        dieScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeLength, sizeLength);
        inputScreen.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeLength,sizeLength);
    }
    
    public void ToggleAdvantage()
    {
        if (midRoll)
            return;

        SetAdvantage(!isWithAdvantage);
    }

    public void SetAdvantage(bool isAdvantage)
    {
        if (midRoll)
            return;

        isWithAdvantage = isAdvantage;

        dieSlot0.gameObject.SetActive(!isWithAdvantage);
        dieSlot1.gameObject.SetActive(isWithAdvantage);
        dieSlot2.gameObject.SetActive(isWithAdvantage);

        advantageCheck.SetIsOnWithoutNotify(isWithAdvantage);
    }

    public void ChangeDie(int index)
    {
        if (midRoll)
            return;

        currentDieSelection = index;

        for (int i = 0; i < dieSlot0.childCount; i++)
        {
            dieSlot0.GetChild(i).gameObject.SetActive(i == currentDieSelection);
            dieSlot1.GetChild(i).gameObject.SetActive(i == currentDieSelection);
            dieSlot2.GetChild(i).gameObject.SetActive(i == currentDieSelection);
        }

        for (int i = 0; i < dieButtons.Length; i++)
        {
            dieButtons[i].GetComponent<Image>().color = (i == currentDieSelection) ? Color.white : nonSelectedColor;
        }
    }

    public void ToggleDirectInput()
    {
        if (midRoll)
            return;

        ShowDirectInputOptions(!inputMode);
    }

    public void ShowDirectInputOptions(bool show)
    {
        inputMode = show;
        SetWidget(inputMode, currentDieSelection, isWithAdvantage);
    }

    public void ClearInput()
    {
        if (midRoll)
            return;

        inputField.SetTextWithoutNotify("");
    }

    #region Throw Dice Mechanics
    
    public void Throw()
    {
        if (midRoll)
            return;

        twist0 = new Vector3(UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f));
        twist0.Normalize();
        twist1 = new Vector3(UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f));
        twist1.Normalize();
        twist2 = new Vector3(UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f));
        twist2.Normalize();    

        midRoll = true;

        dieButtonCG.alpha = 0.2f;
        dieButtonCG.interactable = false;
        dieButtonCG.blocksRaycasts = false;

        sideButtonsCG.alpha = 0.2f;
        sideButtonsCG.interactable = true;
        sideButtonsCG.blocksRaycasts = true;
        
        throwAcceptButtonLabel.transform.parent.gameObject.SetActive(false);

        dieSlotLabel0.text = " ";
        dieSlotLabel1.text = " ";
        dieSlotLabel2.text = " ";
        
        StartCoroutine(RollSequence());
    }

    private IEnumerator RollSequence()
    {
        yield return new WaitForSeconds(rollingTime);

        if (!isWithAdvantage)
        {
            //display results
            result0 = GetRandomResult();

            PosScaleLabel aux = fontSizesBasic[currentDieSelection];

            dieSlotLabel0.text = result0.ToString();
            dieSlotLabel0.fontSize = aux.fontSize;
            if (dieSlotRect0 == null)
                dieSlotRect0 = dieSlotLabel0.GetComponent<RectTransform>();
            dieSlotRect0.anchoredPosition = new Vector2(aux.rePos.x, aux.rePos.y);

            dieSlot0.rotation = Quaternion.identity;
            twist0 = Vector3.zero;

            finalResult = result0;
        }
        else if (isWithAdvantage)
        {
            result1 = GetRandomResult();

            PosScaleLabel aux = fontSizesAdvantage[currentDieSelection];

            dieSlotLabel1.text = result1.ToString();
            dieSlotLabel1.fontSize = aux.fontSize;
            if (dieSlotRect1 == null)
                dieSlotRect1 = dieSlotLabel1.GetComponent<RectTransform>();
            dieSlotRect1.anchoredPosition = new Vector2(aux.rePos.x, aux.rePos.y);

            dieSlot1.rotation = Quaternion.identity;
            twist1 = Vector3.zero;

            yield return new WaitForSeconds(rollingDelta);

            result2 = GetRandomResult();

            dieSlotLabel2.text = result2.ToString();
            dieSlotLabel2.fontSize = aux.fontSize;
            if (dieSlotRect2 == null)
                dieSlotRect2 = dieSlotLabel2.GetComponent<RectTransform>();
            dieSlotRect2.anchoredPosition = new Vector2(-aux.rePos.x, aux.rePos.y);

            dieSlot2.rotation = Quaternion.identity;
            twist2 = Vector3.zero;

            finalResult = Math.Max(result1, result2);
        }

        yield return new WaitForSeconds(rollingResultWait);

        dieButtonCG.alpha = 1f;
        dieButtonCG.interactable = true;
        dieButtonCG.blocksRaycasts = true;

        sideButtonsCG.alpha = 1f;
        sideButtonsCG.interactable = true;
        sideButtonsCG.blocksRaycasts = true;

        throwAcceptButtonLabel.text = "Accept (" + finalResult + ")";
        throwAcceptButtonLabel.transform.parent.gameObject.SetActive(true);

        midRoll = false;
        //AcceptResult();
    }

    private int GetRandomResult()
    {
        int result = 0;
        if (currentDieSelection == 0)
            result = UnityEngine.Random.Range(1, 5);
        else if (currentDieSelection == 1)
            result = UnityEngine.Random.Range(1, 7);
        else if (currentDieSelection == 2)
            result = UnityEngine.Random.Range(1, 9);
        else if (currentDieSelection == 3)
            result = UnityEngine.Random.Range(1, 11);
        else if (currentDieSelection == 4)
            result = UnityEngine.Random.Range(1, 13);
        else if (currentDieSelection == 5)
            result = UnityEngine.Random.Range(1, 21);

        return result;
    }

    public void AcceptResult()
    {
        Debug.Log("Sending result back: " + finalResult);
        
        if(OnResultGet != null)
            OnResultGet.Invoke(finalResult);
    }

    #endregion

    //direct input
    public void AcceptInput()
    {
        int result = 0;
        if(int.TryParse(inputField.text, out result))
        {
            if (result > 0)
            {
                finalResult = result;
                AcceptResult();
                return;
            }
        }

        inputField.text = "";
    }
}
