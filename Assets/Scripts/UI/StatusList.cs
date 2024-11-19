using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;
using System;

public class StatusList : MonoBehaviour
{
    private GameObject statusIconPrefab;

    private RectTransform listRect;

    private RectTransform addButtonRect;

    private TextMeshProUGUI descriptionLabel;
    public RectTransform contentDescriptionRect;
    public bool titleOnly;

    public enum ListType
    {
        Blight,
        Status,
        Effect
    }

    [SerializeField] private ListType type;

    public StatusData.ImageBlight[] displayBlights;

    public StatusData.ImageStatus[] displayStatus;

    public StatusData.ImageEffect[] displayEffects;

    [Range(0.05f,3f)]
    public float iconSeparation = 0.1f;
    public int maximumIcons = 6;
    public Color iconColor = Color.white;

    public ListPanel listPanel;
    private RectTransform listRT;
    public Vector2 dropDownListProportions;
    public Vector3 dropDownListPosCorrection;

    bool listShown = false;

    private List<StatusIcon> icons;

    public UnityEvent OnListChange;

    public bool ignoreUpdateFlag = false;

    private bool listSet = false;

    void Start()
    {
        if (!listSet)
            SetupList();
    }

    void SetupList()
    {
        displayBlights = PieceManager._instance.statusInfo.displayBlights;
        displayEffects = PieceManager._instance.statusInfo.displayEffects;
        displayStatus = PieceManager._instance.statusInfo.displayStatus;

        listRect = transform.GetChild(0).GetComponent<RectTransform>();
        statusIconPrefab = listRect.GetChild(0).gameObject;

        addButtonRect = transform.GetChild(1).GetComponent<RectTransform>();

        float size = listRect.rect.height;

        addButtonRect.anchoredPosition = Vector3.zero;
        Vector2 sd = addButtonRect.sizeDelta;
        sd.x = size;
        addButtonRect.sizeDelta = sd;

        listSet = true;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (listShown)
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    listShown = false;
                    listPanel.ShowPanel(false);
                    listPanel.OnEntryClick -= AddIcon;
                }
            }

            if (icons != null)
            {
                for (int i = 0; i < icons.Count; i++)
                {
                    icons[i].TryClearing();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            listShown = false;
            listPanel.ShowPanel(false);
            listPanel.OnEntryClick -= AddIcon;
        }
    }

    public void AddCall()
    {
        listPanel.screenProportionSize = dropDownListProportions;
        listPanel.listColor = Color.gray;

        Vector3 listOrigin = addButtonRect.position + (0.5f * addButtonRect.rect.width * Vector3.right);
        List<string> entries = new List<string>();

        if(type == ListType.Blight)
        {
            for (int i = 0; i < displayBlights.Length; i++)
            {
                entries.Add(MiscTools.GetSpacedForm(displayBlights[i].blight.ToString()));
            }
        }
        else if (type == ListType.Status)
        {
            for (int i = 0; i < displayStatus.Length; i++)
            {
                entries.Add(MiscTools.GetSpacedForm(displayStatus[i].status.ToString()));
            }
        }
        else if (type == ListType.Effect)
        {
            for (int i = 0; i < displayEffects.Length; i++)
            {
                entries.Add(MiscTools.GetSpacedForm(displayEffects[i].effect.ToString()));
            }
        }

        listPanel.ShowPanel(listOrigin, entries, true);
        listPanel.OnEntryClick += AddIcon;
        listRT = listPanel.GetComponent<RectTransform>();

        listRT.position += dropDownListPosCorrection;

        listShown = true;
    }

    public void AddBlight(IconUnit.Blight blight)
    {
        if (type != ListType.Blight)
            return;

        int targetIndex = -1;
        for(int i = 0; i < displayBlights.Length; i++)
        {
            if(displayBlights[i].blight == blight)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
            AddIcon(targetIndex);
    }

    public List<IconUnit.Blight> GetBlightList()
    {
        if (type != ListType.Blight)
            return null;

        List<IconUnit.Blight> blightList = new List<IconUnit.Blight>();
        for (int i = 0; i < icons.Count; i++)
        {
            IconUnit.Blight match = displayBlights[icons[i].statusIndex].blight;
            blightList.Add(match);
        }

        return blightList;
    }

    public void AddStatus(IconUnit.Status status)
    {
        if (type != ListType.Status)
            return;

        int targetIndex = -1;
        for (int i = 0; i < displayStatus.Length; i++)
        {
            if (displayStatus[i].status == status)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
            AddIcon(targetIndex);
    }

    public string GetStatusDescription(IconUnit.Status status)
    {
        if (type != ListType.Status)
            return "";

        int targetIndex = -1;
        for (int i = 0; i < displayStatus.Length; i++)
        {
            if (displayStatus[i].status == status)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
            return GetStatusDescription(targetIndex);
        else
            return "";
    }

    public string GetStatusDescription(int statusIndex)
    {
        if (type != ListType.Status)
            return "";

        return displayStatus[statusIndex].statusDescription;
    }

    public List<IconUnit.Status> GetStatusList()
    {
        if (type != ListType.Status)
            return null;

        if (icons == null)
            return null;

        List<IconUnit.Status> statusList = new List<IconUnit.Status>();
        for (int i = 0; i < icons.Count; i++)
        {
            IconUnit.Status match = displayStatus[icons[i].statusIndex].status;
            statusList.Add(match);
        }

        return statusList;
    }

    public void AddPositiveEffect(IconUnit.PositiveEffects effect)
    {
        if (type != ListType.Effect)
            return;

        int targetIndex = -1;
        for (int i = 0; i < displayEffects.Length; i++)
        {
            if (displayEffects[i].effect == effect)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
            AddIcon(targetIndex);
    }

    public string GetEffectDescription(IconUnit.PositiveEffects status)
    {
        if (type != ListType.Effect)
            return "";

        int targetIndex = -1;
        for (int i = 0; i < displayEffects.Length; i++)
        {
            if (displayEffects[i].effect == status)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex >= 0)
            return GetEffectDescription(targetIndex);
        else
            return "";
    }

    public string GetEffectDescription(int effectIndex)
    {
        if (type != ListType.Effect)
            return "";

        return displayEffects[effectIndex].effectDescription;
    }

    public List<IconUnit.PositiveEffects> GetEffectList()
    {
        if (type != ListType.Effect)
            return null;

        if (icons == null)
            return null;

        List<IconUnit.PositiveEffects> effectList = new List<IconUnit.PositiveEffects>();
        for(int i = 0; i < icons.Count; i++)
        {
            IconUnit.PositiveEffects match = displayEffects[icons[i].statusIndex].effect;
            effectList.Add(match);
        }

        return effectList;
    }

    public void AddIcon(int index)
    {
        if (icons == null)
            icons = new List<StatusIcon>();

        for(int i = 0; i < icons.Count; i++)
        {
            if (icons[i].statusIndex == index)
                return;
        }

        if (icons.Count >= maximumIcons)
            return;

        if (!listSet)
            SetupList();

        GameObject nuIcon = Instantiate<GameObject>(statusIconPrefab, listRect);
        RectTransform iconRT = nuIcon.GetComponent<RectTransform>();

        float size = listRect.rect.height;
        Vector3 positioning = new Vector3(icons.Count * (size + (iconSeparation * size)), 0f, 0f);
        iconRT.anchoredPosition = positioning;

        Vector2 sd = iconRT.sizeDelta;
        sd.x = size;
        iconRT.sizeDelta = sd;

        Sprite iconImage = null;
        string auxName = "";
        string auxDescr = "";
        if (type == ListType.Blight) {
            iconImage = displayBlights[index].image;
            auxName = displayBlights[index].blight.ToString();
            auxDescr = displayBlights[index].blightDescription;
        }
        else if(type == ListType.Status)
        {
            iconImage = displayStatus[index].image;
            auxName = displayStatus[index].status.ToString();
            auxDescr = displayStatus[index].statusDescription;
        }
        else if(type == ListType.Effect)
        {
            iconImage = displayEffects[index].image;
            auxName = displayEffects[index].effect.ToString();
            auxDescr = displayEffects[index].effectDescription;
        }

        iconRT.GetChild(0).GetComponent<Image>().sprite = iconImage;
        iconRT.GetChild(0).GetComponent<Image>().color = iconColor;

        nuIcon.name = auxName;

        StatusIcon si = nuIcon.GetComponent<StatusIcon>();
        si.SetData(auxName, (int)type, index, icons.Count, auxDescr, this);

        icons.Add(si);

        nuIcon.SetActive(true);

        if (icons.Count < maximumIcons)
            addButtonRect.anchoredPosition = new Vector3(icons.Count * (size + (iconSeparation * size)), 0f, 0f);
        else
            addButtonRect.gameObject.SetActive(false);

        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= AddIcon;

        if (OnListChange != null && !ignoreUpdateFlag)
            OnListChange.Invoke();
    }

    public void RemoveIcon(int childIndex)
    {
        Debug.Log("remove call");

        int siblingIndex = icons[childIndex].transform.GetSiblingIndex();

        icons.RemoveAt(childIndex);

        Destroy(listRect.GetChild(siblingIndex).gameObject);

        float size = listRect.rect.height;
        for (int i = 0; i < icons.Count; i++)
        {
            RectTransform iconRT = icons[i].GetComponent<RectTransform>();

            Vector3 positioning = new Vector3(i * (size + (iconSeparation * size)), 0f, 0f);
            iconRT.anchoredPosition = positioning;

            icons[i].SetChildIndex(i);
        }

        addButtonRect.anchoredPosition = new Vector3(icons.Count * (size + (iconSeparation * size)), 0f, 0f);
        addButtonRect.gameObject.SetActive(true);

        if (OnListChange != null && !ignoreUpdateFlag)
            OnListChange.Invoke();
    }

    public void ClearIcons()
    {
        if (icons == null || icons.Count == 0)
            return;

        for(int i = listRect.childCount - 1; i >= 1; i--)
        {
            Destroy(listRect.GetChild(i).gameObject);
        }

        icons = new List<StatusIcon>();

        addButtonRect.anchoredPosition = new Vector3(0f, 0f, 0f);

        ChangeDescription("");
    }

    public void ChangeDescription(string descr)
    {
        if(contentDescriptionRect == null)
            return;

        if (descriptionLabel == null)
            descriptionLabel = contentDescriptionRect.GetChild(0).GetComponent<TextMeshProUGUI>();

        descriptionLabel.text = descr;

        Vector2 sd = contentDescriptionRect.sizeDelta;
        sd.y = 20f;
        contentDescriptionRect.sizeDelta = sd;

        if (descr.Length == 0)
            return;

        try
        {
            StartCoroutine(FrameUpdateRectSize());
        }
        catch(System.Exception e)
        {

        }
    }

    private IEnumerator FrameUpdateRectSize()
    {
        yield return new WaitForEndOfFrame();

        float toAdd = 50f;

        toAdd += 1.1f * descriptionLabel.renderedHeight;

        Vector2 sd = contentDescriptionRect.sizeDelta;;
        sd.y = toAdd;
        contentDescriptionRect.sizeDelta = sd;

        contentDescriptionRect.anchoredPosition = 0f * Vector2.up;
    }

    public void ClearIconsClickCounter()
    {
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].ClearClickCounter();
        }
    }

    public void CallListClose()
    {
        listShown = false;
        listPanel.ShowPanel(false);
        listPanel.OnEntryClick -= AddIcon;
    }
}
