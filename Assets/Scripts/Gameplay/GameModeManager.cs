using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

//control most game mode functionalities
public class GameModeManager : MonoBehaviour
{
    public static GameModeManager _instance;

    public Transform pointerTest;

    [Header("UI")]

    public Color buttonColorChosen = Color.gray;
    public Color buttonColorUnchosen = Color.white;
    public float chosenButtonScaleUp = 1.35f;

    [Header("Grid Tools")]
    public SmoothToggle tileNumberToggle;
    public bool updateTileFlag { get; private set; }
    public Image tileNumberIcon;
    public Sprite tileNumberOn;
    public Sprite tileNumberOff;

    [Space(10f)]
    public SmoothToggle viewGridToggle;
    private bool updateViewGridFlag = false;
    public Image viewGridIcon;
    public Sprite viewGridOn;
    public Sprite viewGridOff;

    public Animator toolGridPanelAnim;
    private bool toolGridPanelOpen;

    private Image currentActiveButton;

    [Space(20f)]
    public RoundCounter uiRoundCounter;

    [Space(10f)]
    public float markerSizeFactor = 0.1f;
    public float totalSizeFactor = 0.4f;

    public enum CursorTypes
    {
        Regular,
        Circle
    }

    [Header("Grid Tools")]
    [SerializeField] private CursorTypes defaultMode = CursorTypes.Regular;
    private CursorTypes currentMode;
    [SerializeField] private Texture2D regularCursor;
    [SerializeField] private Texture2D circleCursor;

    private void Awake()
    {
        if (GameModeManager._instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        SetTileNumbering(false, true);

        viewGridToggle.ForceValue(PlayerPrefs.GetInt("GridVisible", 0) == 1);

        tileNumberIcon.sprite = tileNumberToggle.toggleValue ? tileNumberOn : tileNumberOff;
        viewGridIcon.sprite = viewGridToggle.toggleValue ? viewGridOn : viewGridOff;

        ChangeCursor(defaultMode);
    }

    void LateUpdate()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.GameMode)
            return;

        /*
        //VEEEEEERY innefficient, but helpful
        Vector3 pointerPlace = Vector3.zero;
        if(MapManager._instance.mapTarget.GetFloorIntersection(MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition), out pointerPlace))
        {
            pointerTest.position = pointerPlace;
        }
        */
    }

    public void OnPiecesLoadedCall()
    {
        if (!uiRoundCounter.enabled)
            return;

        uiRoundCounter.UpdateRoundArray();
    }

    private struct RectIndexSet
    {
        public RectTransform placementRect;
        public Vector3 worldPosition;
        public float distanceToCamera;
    }

    //mueve en jerarquia los pines y puntos de colocado para dar una sensación de profundidad
    private void ArrangeByClosenessToCamera(List<RectIndexSet> elements)
    {
        RectIndexSet[] arrange = new RectIndexSet[elements.Count];
        elements.CopyTo(arrange);

        for (int i = 0; i < arrange.Length; i++)
        {
            arrange[i].distanceToCamera = Vector3.Distance(MapManager._instance.activeCamera.transform.position, elements[i].worldPosition);
        }

        RectSort(arrange, 0, arrange.Length - 1);

        for (int i = 0; i < arrange.Length; i++)
        {
            RectTransform rectTarget = arrange[i].placementRect;
            rectTarget.SetAsFirstSibling();
        }
    }

    #region Grid Tools Section
    public void ToggleGridToolPanel()
    {
        toolGridPanelOpen = !toolGridPanelOpen;

        if (toolGridPanelOpen)
            toolGridPanelAnim.SetTrigger("Open");
        else
            toolGridPanelAnim.SetTrigger("Close");
    }

    public void ChangeTileNumbering(bool updateUI)
    {
        bool value = !tileNumberToggle.toggleValue;

        SetTileNumbering(value, updateUI);
    }

    public void UpdateTileNumbering()
    {
        SetTileNumbering(tileNumberToggle.toggleValue, true);
    }

    public void SetTileNumbering(bool visible, bool updateUI = false)
    {
        if (MapManager._instance == null)
            return;

        MapManager._instance.mapTarget.SetCellNumberingVisibility(!visible, updateTileFlag);
        if (updateTileFlag)
            updateTileFlag = false;

        tileNumberIcon.sprite = !visible ? tileNumberOn : tileNumberOff;

        if (updateUI)
            tileNumberToggle.ForceValue(visible);
    }

    bool testBool = false;
    public void TestToggleGridNumber()
    {
        testBool = !testBool;

        MapManager._instance.mapTarget.SetCellNumberingVisibility(testBool, updateTileFlag);
    }

    public void SetTileNumerationUpdateFlag()
    {
        updateTileFlag = true;
    }

    public void ToggleGridVisibility(bool updateUI)
    {
        if (MapManager._instance.toolMode == MapManager.ToolMode.TerrainEditor)
            return;

        if (MapManager._instance.mapTarget == null)
            ChangeGridVisibility(false);
        else
            ChangeGridVisibility(!MapManager._instance.mapTarget.gridActive, updateUI);
    }

    public void ChangeGridVisibility(bool value, bool updateUI = false)
    {
        MapManager._instance.mapTarget?.SetGridVisibility(value);

        viewGridIcon.sprite = value ? viewGridOn : viewGridOff;

        if (updateUI)
            viewGridToggle.ForceValue(value);
    }
    #endregion

    //quick sort
    private void RectSort(RectIndexSet[] arr, int indexLeft, int indexRight)
    {
        if (indexLeft < indexRight)
        {
            int pivot = Partition(arr, indexLeft, indexRight);

            if (pivot > 1)
            {
                RectSort(arr, indexLeft, pivot - 1);
            }
            if (pivot + 1 < indexRight)
            {
                RectSort(arr, pivot + 1, indexRight);
            }
        }

    }

    private int Partition(RectIndexSet[] arr, int left, int right)
    {
        float pivotDistance = arr[left].distanceToCamera;
        while (true)
        {

            while (arr[left].distanceToCamera < pivotDistance)
            {
                left++;
            }

            while (arr[right].distanceToCamera > pivotDistance)
            {
                right--;
            }

            if (left < right)
            {
                if (arr[left].distanceToCamera == arr[right].distanceToCamera)
                    return right;

                RectIndexSet temp = arr[left];
                arr[left] = arr[right];
                arr[right] = temp;
            }
            else
            {
                return right;
            }
        }
    }

    public void ChangeCursor(CursorTypes type)
    {
        Texture2D toChange = regularCursor;
        if (type == CursorTypes.Circle)
            toChange = circleCursor;

        Cursor.SetCursor(toChange, Vector2.zero, CursorMode.Auto);

        currentMode = type;
    }
}
