using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class MapModeControl : MonoBehaviour
{
    private int currentMapMode = 1; //0 - tiled, 1 - mesh

    public int CurrentMode => currentMapMode;

    [Header("UI")]
    [SerializeField] private GameObject tileModeMenu;
    [SerializeField] private GameObject meshModeMenu;
    [SerializeField] private SmoothToggle mapModeToggle;
    [Space(5f)]
    [SerializeField] private SmoothToggle checkeredToggle;
    [SerializeField] private SmoothToggle heightedToggle;

    [Header("Tile Menu")]
    [SerializeField] private Animator tileMenuAnim;
    [SerializeField] private GameObject tileTerrainTypePanel;

    [Header("Mesh Menu")]
    [SerializeField] private Animator meshMenuAnim;
    [SerializeField] private GameObject meshTerrainTypePanel;
    [SerializeField] private GameObject meshAutoTilePanel;
    [SerializeField] private GameObject meshMorphPanel;

    [Header("Color Panel")]
    [SerializeField] private ColorManager colorManager;
    [SerializeField] private int iconsPerColumn = 2;
    [SerializeField] private Animator mapColorAnim;
    [SerializeField] private Image chosenColorIcon;
    [SerializeField] private RectTransform colorPanelContent;
    [SerializeField] private GameObject rowIconPrefab;
    private bool colorPanelBuilt = false;

    private bool colorMenuActive = false;

    [Header("Outline Panel")]
    [SerializeField] private Animator outlinePanelAnim;
    private bool outlinePanelActive = false;
    [SerializeField] private RectTransform outlineButtonSet;

    private void Awake()
    {
        BuildColorPanel(colorManager.colors);
    }

    public void TerrainModeSwitch()
    {
        if (mapModeToggle.toggleValue)
        {
            currentMapMode = 1;
            tileModeMenu.SetActive(false);
            meshModeMenu.SetActive(true);
        }
        else
        {
            currentMapMode = 0;
            tileModeMenu.SetActive(true);
            meshModeMenu.SetActive(false);
        }

        MapManager._instance.mapTarget.UpdateMapModeLook();
    }

    public void SetTerrainMode(bool tileMode, bool callUpdate = true)
    {
        if (!tileMode)
        {
            currentMapMode = 1;
            tileModeMenu.SetActive(false);
            meshModeMenu.SetActive(true);
        }
        else
        {
            currentMapMode = 0;
            tileModeMenu.SetActive(true);
            meshModeMenu.SetActive(false);
        }

        mapModeToggle.ForceValue(!tileMode);

        if (callUpdate)
        {
            MapManager._instance.mapTarget.UpdateMapModeLook();
            GameModeManager._instance.SetTileNumerationUpdateFlag();
        }
    }

    #region Tool Control

    public void ActivateTerrainTypeMenu(bool active)
    {
        if (currentMapMode == 0)
        {
            if (active)
            {
                tileTerrainTypePanel.SetActive(true);

                tileMenuAnim.SetTrigger("SubMenuChange");
            }
            else
            {
                tileMenuAnim.SetTrigger("MenuChange");
            }
        }
        else if (currentMapMode == 1)
        {
            if (active)
            {
                meshMorphPanel.SetActive(false);
                meshTerrainTypePanel.SetActive(true);
                meshAutoTilePanel.SetActive(false);

                meshMenuAnim.SetTrigger("SubMenuChange");
            }
            else
            {
                meshMenuAnim.SetTrigger("MenuChange");
            }
        }
    }

    public void ActivateAutoTileMenu(bool active)
    {
        if (currentMapMode != 1)
            return;

        if (active)
        {
            meshMorphPanel.SetActive(false);
            meshTerrainTypePanel.SetActive(false);
            meshAutoTilePanel.SetActive(true);

            meshMenuAnim.SetTrigger("SubMenuChange");
        }
        else
        {
            meshMenuAnim.SetTrigger("MenuChange");
        }
    }

    public void ActivateTerrainMorphMenu(bool active)
    {
        if (currentMapMode != 1)
            return;

        if (active)
        {
            meshMorphPanel.SetActive(true);
            meshTerrainTypePanel.SetActive(false);
            meshAutoTilePanel.SetActive(false);

            meshMenuAnim.SetTrigger("SubMenuChange");
        }
        else
        {
            meshMenuAnim.SetTrigger("MenuChange");
        }
    }

    public void SetTerrainChange(int index)
    {
        if(currentMapMode == 0)
        {
            for(int i = 0; i < tileTerrainTypePanel.transform.GetChild(1).childCount; i++)
            {
                tileTerrainTypePanel.transform.GetChild(1).GetChild(i).GetChild(4).gameObject.SetActive(i == index);
                tileTerrainTypePanel.transform.GetChild(1).GetChild(i).GetComponent<CanvasGroup>().alpha = (i == index) ? 1f : 0.4f;
            }
        }
        else
        {
            for (int i = 0; i < meshTerrainTypePanel.transform.GetChild(1).childCount; i++)
            {
                meshTerrainTypePanel.transform.GetChild(1).GetChild(i).GetChild(4).gameObject.SetActive(i == index);
                meshTerrainTypePanel.transform.GetChild(1).GetChild(i).GetComponent<CanvasGroup>().alpha = (i == index) ? 1f : 0.4f;
            }
        }
    }

    public void CloseSubMenu()
    {
        if (currentMapMode == 0)
        {
            tileMenuAnim.SetTrigger("MenuChange");
        }
        else if (currentMapMode == 1)
        {
            meshMenuAnim.SetTrigger("MenuChange");
        }

        MapManager._instance.mapTarget.ChangeTool(-1);
    }

    #endregion

    #region Color Menu

    private void BuildColorPanel(ColorManager.ColorSet[] entries, int selectedIndex = 0)
    {
        //clear menu
        for (int i = colorPanelContent.childCount - 1; i >= 1; i--)
        {
            Destroy(colorPanelContent.GetChild(i).gameObject);
        }

        //float accX = 0f;
        //float accY = 0f;
        int currentIcons = 0;
        int totalIcons = entries.Length;

        int totalCols = totalIcons / iconsPerColumn;
        int rest = totalIcons % iconsPerColumn;

        int rowJumpAt = (totalCols + (rest > 0 ? 1 : 0));

        GameObject iconPrefab = rowIconPrefab.transform.GetChild(0).gameObject;

        RectTransform iconPrebabRT = iconPrefab.GetComponent<RectTransform>();

        RectTransform[] rowObjects = new RectTransform[iconsPerColumn];

        float iconHeight = 0.9f * (colorPanelContent.rect.height / (float)iconsPerColumn);

        //modify the size of the content container
        float iconWidth = iconHeight + rowIconPrefab.GetComponent<HorizontalLayoutGroup>().spacing;

        Vector2 contentSize = colorPanelContent.sizeDelta;
        contentSize.x = (rowJumpAt) * iconWidth;
        colorPanelContent.sizeDelta = contentSize;

        Vector2 rowDelta = rowIconPrefab.GetComponent<RectTransform>().sizeDelta;
        rowDelta.y = iconHeight;
        rowIconPrefab.GetComponent<RectTransform>().sizeDelta = rowDelta;

        for (int i = 0; i < iconsPerColumn; i++)
        {
            GameObject rowPrefab = Instantiate<GameObject>(rowIconPrefab, colorPanelContent);
            rowPrefab.name = "Row_" + i;
            RectTransform rowRT = rowPrefab.GetComponent<RectTransform>();

            rowObjects[i] = rowRT;

            rowPrefab.SetActive(true);
        }

        int rowToAssign = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            GameObject nuIcon = Instantiate<GameObject>(iconPrefab, rowObjects[rowToAssign]);
            RectTransform iconRT = nuIcon.GetComponent<RectTransform>();

            float iconSize = iconHeight;
            Vector2 iconSizeDelta = iconSize * Vector2.one;
            iconRT.sizeDelta = iconSizeDelta;

            if ((i + 1) % rowJumpAt == 0)
                rowToAssign++;

            iconRT.GetChild(0).GetComponent<Image>().color = entries[i].color;
            int entryIndex = i;
            iconRT.GetComponent<HoldButton>().onRelease.AddListener(delegate
            {
                SetTerrainColor(entryIndex);
            });

            currentIcons++;
            nuIcon.SetActive(true);
        }

        colorPanelBuilt = true;
    }

    public void SetTerrainColor(int entryIndex)
    {
        Color chosenColor = colorManager.colors[entryIndex].color;

        chosenColorIcon.color = chosenColor;

        //change mesh and tiles call
        MapManager._instance.mapTarget.SetMapColor(chosenColor, MapManager._instance.checkeredColor, MapManager._instance.heightColor);
    }

    public void ToggleColorMenu()
    {
        SetColorMenuActive(!colorMenuActive);
    }

    private void SetColorMenuActive(bool active)
    {
        if (active)
            mapColorAnim.SetTrigger("Show");
        else
            mapColorAnim.SetTrigger("Hide");

        colorMenuActive = active;
    }

    public void CheckeredToggle()
    {
        MapManager._instance.ChangeColorStyle(checkeredToggle.toggleValue, heightedToggle.toggleValue);
        MapManager._instance.mapTarget.SetCheckered(checkeredToggle.toggleValue);
        
        MapManager._instance.mapTarget.UpdateMapModeLook();
    }

    public void HeightedToggle()
    {
        MapManager._instance.ChangeColorStyle(checkeredToggle.toggleValue, heightedToggle.toggleValue);
        MapManager._instance.mapTarget.SetHeightColor(heightedToggle.toggleValue);
        
        MapManager._instance.mapTarget.UpdateMapModeLook();
    }

    public void UpdateColorPanelUI()
    {
        chosenColorIcon.color = MapManager._instance.mapTarget.mapColor;
    }

    public void SetColorConditions(bool checkered, bool heighted)
    {
        checkeredToggle.ForceValue(checkered);
        heightedToggle.ForceValue(heighted);
    }

    #endregion

    #region Outline Menu

    public void ToggleOutlineMenu()
    {
        SetOutlineMenuActive(!outlinePanelActive);
    }

    public void SetOutlineMenuActive(bool active)
    {
        if (active)
        {
            outlinePanelAnim.SetTrigger("Show");
            SetOutlineButtonSelected(PlayerPrefs.GetInt("TileStyle", 1));
        }
        else
            outlinePanelAnim.SetTrigger("Hide");

        outlinePanelActive = active;
    }

    public void SetOutlineMode(int choice)
    {
        //set outline style
        MapManager._instance.ChangeTileStyle(choice);

        SetOutlineButtonSelected(choice);
    }

    private void SetOutlineButtonSelected(int choice)
    {
        int childIndex = choice + 1;
        if (childIndex >= outlineButtonSet.childCount)
            childIndex = 0;

        for(int i = 0; i < outlineButtonSet.childCount; i++)
        {
            Transform childObj = outlineButtonSet.GetChild(i);
            if(i == childIndex)
            {
                childObj.GetComponent<Image>().color = Color.white;

                Image iconPic = childObj.GetChild(0).GetComponent<Image>();
                float alpha = iconPic.color.a;
                Color iconColor = Color.black;
                iconColor.a = alpha;
                iconPic.color = iconColor;

                childObj.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.black;
            }
            else
            {
                childObj.GetComponent<Image>().color = Color.black;

                Image iconPic = childObj.GetChild(0).GetComponent<Image>();
                float alpha = iconPic.color.a;
                Color iconColor = Color.white;
                iconColor.a = alpha;
                iconPic.color = iconColor;

                childObj.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            }
        }
    }

    #endregion
}
