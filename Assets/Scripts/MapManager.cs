using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MapManager : MonoBehaviour
{
    public static MapManager _instance;

    public MapModeControl mapModeController;

    public BattleMap mapTarget;

    public EventSystem eventSystem;

    public OptionsManager optionsManager;

    public MenuManager menuManager;

    public enum ToolMode
    {
        TerrainEditor,
        UnitMaker,
        ObjectEditor,
        Coloring,
        GameMode
    }

    [Header("Maker Settings")]
    public ToolMode toolMode = ToolMode.TerrainEditor;

    [Header("Camera Settings")]
    public List<MapCamera> cameras;
    public MapCamera activeCamera { get; private set; }
    private Vector3 cameraStartPos;
    private Quaternion cameraStartRot;
    public bool useMoveAccelerationOnInput;
    public bool useMoveAccelerationOnAuto;

    public bool invertX = false;
    public bool invertY = false;
    public bool useRotationAccelerationOnInput;
    public bool useRotationAccelerationOnAuto;
    private Vector3 lastMousePosition;

    public float zoomSensitivity = 50f;
    public bool invertZoom = false;
    public bool useZoomAccelerationOnInput;
    public bool useZoomAccelerationOnAuto;

    public bool controlEnabled { get; private set; }
    public float boundScale = 0.5f;

    public float doubleClickDelta = 0.2f;
    private bool waitingForDoubleClick = false;
    private float firstClickTimestamp;

    [Header("UI")]
    public GameObject terrainScreen;

    [Space(10f)]
    public SmoothToggle gameEditToggle;
    public TabbedLabels editModeLabels;

    public Image modeButton;
    public GameObject modeScrollPanel;
    public RectTransform modeContent;
    public float modeScrollEntrySpacing = 10f;

    public List<ToolModeTip> modeTips;

    [Space(10f)]
    public RectTransform gridToolPanel;

    [System.Serializable]
    public struct ToolModeTip
    {
        public ToolMode mode;
        public string tooltip;
    }

    private bool scrollBuilt = false;
    public bool overUI { get; private set; }

    public Color unselectedButtonColor = Color.black;
    public Color selectedButtonColor = Color.gray;

    [Header("Cell Pointer Settings")]
    public Transform cellPointObject;
    private float currentCoodX;
    private float currentCoodY;
    public bool pointerActive {get; private set; }
    private bool pointerSetup = false;
    public float pointerHeight = 0.1f;

    [Header("Terrain Tool Settings")]
    public bool onHoldTerrainEditing = false;

    public int cellCountX = 20;
    public int cellCountZ = 20;
    public float floorHeight = 0f;
    public float ceilingHeight = 6f;
    [Range(1,4)]
    public int terrainMeshDetail = 4;

    public Image[] autoMapTileButtons;
    public Image[] mapTileButtons;
    public Image[] chooseTileButtons;
    public Image[] modifyTerrainButtons;
    public RectTransform radiusModArea;
    public float minModRadiusValue = 0.25f;
    public float maxModRadiusValue = 5f;
    
    public float curveRange = 3;

    public enum TileStyles
    {
        Solid,
        Outline,
        DashedOutline,
        JustCorners
    }

    [System.Serializable]
    public struct TileStylePair
    {
        public TileStyles style;
        public Material styleMat;
    }

    [Space(10f)]
    public TileStylePair[] tileStyles;
    [HideInInspector] public Material tileMaterial;
    private int activeTileStyle = 1;
    public int TileStyle => activeTileStyle;

    public Material tileMaterialFaded;
    public Material mapTestMaterial;
    public Material mapTestMaterialFaded;
    public Material shapeTestMaterial;
    public Material shapeTestMaterialFaded;

    public bool showTerrainTiles;
    public bool showTileTypes;

    public bool checkeredColor { get; private set; }
    public bool heightColor { get; private set; }

    [Space(10f)]
    public Transform terrainEdgePlane;
    public LayerMask terrainEdgeLayer;
    public float terrainEdgeSizeFactor = 1.2f;

    [Header("Shape Tool Settings")]
    public GameObject shapeScreen;

    [Header("Coloring Tool Settings")]
    public GameObject coloringScreen;

    [Header("Unit Maker Settings")]
    public GameObject unitScreen;

    [Header("Game Screen Settings")]
    public GameObject gameScreen;

    [Header("Tile Numbering Settings")]
    public Material fontTileMaterial;
    public Material fontMeshMaterial;

    private void Awake()
    {
        if (MapManager._instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        activeCamera = cameras[0];
        cameraStartPos = activeCamera.transform.position;
        cameraStartRot = activeCamera.transform.rotation;

        optionsManager.LoadSettings();
    }

    public void StartNewMap()
    {
        //new map stuff
        menuManager.CheckMenuUIRearrange();

        showTerrainTiles = PlayerPrefs.GetInt("ShowHeightTiles", 0) == 1;
        showTileTypes= PlayerPrefs.GetInt("ShowTerrainTypeTiles", 0) == 1;

        int sizeX = cellCountX;
        int sizeZ = cellCountZ;

        mapTarget.BuildMap(sizeX, sizeZ, true);
        UpdateCameraBounds();

        modeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = MiscTools.GetSpacedForm(((ToolMode)0).ToString());

        EnableControls(true);

        SetGameEditMode(true);

        mapModeController.SetTerrainMode(true, false);

        MarkManager._instance.CleanAllMarks();
        ShapesManager._instance.CleanShapeContainer();
        PieceManager._instance.CleanPieces();

        //PlayerPrefs.SetInt("FirstBoot", 0);

        //check first time
        bool firstTime = PlayerPrefs.GetInt("FirstBoot", 0) == 0;
        if (firstTime)
        {
            NotificationSystem.Instance.PushNotification("First Time? Be sure to check the help section in the top right corner (?)");
            PlayerPrefs.SetInt("FirstBoot", 1);
        }

        optionsManager.QuickSave();
    }

    public void MapAfterLoad()
    {
        menuManager.CheckMenuUIRearrange();

        mapModeController.SetTerrainMode(mapModeController.CurrentMode == 0, false);
        mapModeController.UpdateColorPanelUI();
        mapModeController.SetColorConditions(checkeredColor, heightColor);

        int savedTileStyle = PlayerPrefs.GetInt("TileStyle", 0);
        showTerrainTiles = PlayerPrefs.GetInt("ShowHeightTiles", 0) == 1;

        if (mapModeController.CurrentMode == 1)
            ChangeTileStyle(savedTileStyle, false);
        else
            ChangeTileStyle(0, false);

        showTileTypes = PlayerPrefs.GetInt("ShowTerrainTypeTiles", 0) == 1;

        int sizeX = cellCountX;
        int sizeZ = cellCountZ;

        UpdateCameraBounds();

        modeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = MiscTools.GetSpacedForm(((ToolMode)0).ToString());

        EnableControls(true);

        //mapTarget.ShowTiles(MapManager._instance.showTerrainTiles);
        mapTarget.ShowTerrainTypes(MapManager._instance.showTileTypes);
        mapTarget.SetObjectsMat(false);

        SetGameEditMode(true);

        //PlayerPrefs.SetInt("FirstBoot", 0);

        //check first time
        bool firstTime = PlayerPrefs.GetInt("FirstBoot", 0) == 0;
        if (firstTime)
        {
            NotificationSystem.Instance.PushNotification("First Time? Be sure to check the help section in the top right corner (?)");
            PlayerPrefs.SetInt("FirstBoot", 1);
        }
    }

    public void UpdateCameraBounds()
    {
        int sizeX = cellCountX;
        int sizeZ = cellCountZ;
        activeCamera.SetBounds(-boundScale * sizeX, boundScale * sizeX, -boundScale * sizeZ, boundScale * sizeZ);

        //relocate camera to start
        activeCamera.transform.position = cameraStartPos;
        activeCamera.transform.rotation = cameraStartRot;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        overUI = eventSystem.IsPointerOverGameObject();

        if (!controlEnabled)
            return;

        //controls camera
        if (activeCamera != null) {

            if (activeCamera.autoNavigating || activeCamera.cameraLocked || MarkManager._instance.markEditing)
                return;

            Vector3 moveDir = Vector3.zero;
            if (Input.GetKey(KeyCode.A))
                moveDir -= activeCamera.transform.right;
            if (Input.GetKey(KeyCode.D))
                moveDir += activeCamera.transform.right;
            if (Input.GetKey(KeyCode.W))
                moveDir += activeCamera.transform.forward;
            if (Input.GetKey(KeyCode.S))
                moveDir -= activeCamera.transform.forward;

            moveDir.y = 0f;
            activeCamera.Move(moveDir.normalized, !useMoveAccelerationOnInput);

            Vector3 rotDir = Vector3.zero;
            if (Input.GetMouseButton(2)) //mid click rotates
            {
                Vector2 diff = Input.mousePosition - lastMousePosition;

                rotDir.x += (invertX ? 1f : -1f) * diff.y;
                rotDir.y += (invertY ? -1f : 1f) * diff.x;

                activeCamera.Rotate(rotDir.normalized, !useRotationAccelerationOnInput);
                activeCamera.SetRotatingFlag(true);
            }
            else
            {
                activeCamera.Rotate(Vector2.zero, !useRotationAccelerationOnInput);
                activeCamera.SetRotatingFlag(false);
            }

            lastMousePosition = Input.mousePosition;

            if (!overUI)
            {
                /*
                //double click look
                if (Input.GetMouseButtonDown(0))
                {
                    if (!waitingForDoubleClick)
                    {
                        waitingForDoubleClick = true;
                        firstClickTimestamp = Time.time;
                    }
                    else if (waitingForDoubleClick)
                    {
                        //double click action
                        CameraClickMove(Input.mousePosition);

                        waitingForDoubleClick = false;
                    }
                }

                if (waitingForDoubleClick && (Time.time - firstClickTimestamp) > doubleClickDelta)
                    waitingForDoubleClick = false;
                */

                
                if (!PieceManager._instance.hoverOnPiece)
                {
                    if (Input.mouseScrollDelta.y != 0f)
                    {
                        float zoomQ = (invertZoom ? 1f : -1f) * Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                        activeCamera.Zoom(zoomQ, !useZoomAccelerationOnInput);
                        activeCamera.SetZoomingFlag(true);
                    }
                    else
                        activeCamera.SetZoomingFlag(false);
                }
                
            }
        }

        //terrain tool
        if(toolMode == ToolMode.TerrainEditor)
        {
            if (mapTarget.currentTool == BattleMap.TerrainTools.TileModifying)
            {
                if (overUI)
                    return;

                if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && onHoldTerrainEditing))
                    mapTarget.ModifyTiles(true, onHoldTerrainEditing);
                else if (Input.GetMouseButtonDown(1) || (Input.GetMouseButton(1) && onHoldTerrainEditing))
                    mapTarget.ModifyTiles(false, onHoldTerrainEditing);
                else
                    mapTarget.ResetAuxTileRegistry();

                GameModeManager._instance.UpdateTileNumbering();
            }
            else if(mapTarget.currentTool == BattleMap.TerrainTools.TerrainMorphing)
            {
                if (ShapesManager.CheckMouseInArea(radiusModArea))
                {
                    float denominator = Vector3.Dot(activeCamera.transform.forward, Vector3.down);
                    Vector3 norm = Vector3.down;

                    if (denominator < 0.001f)
                    {
                        return;
                    }

                    float t = Vector3.Dot(Vector3.zero - activeCamera.transform.position, norm) / denominator;
                    Vector3 p = activeCamera.transform.position + (activeCamera.transform.forward * t);

                    mapTarget.terrainModHelper.transform.position = p;

                    mapTarget.terrainModHelper.SetActive(true);
                }
                else if (overUI)
                    mapTarget.terrainModHelper.SetActive(false);
                else
                    mapTarget.terrainModHelper.SetActive(true);

                if (overUI || ShapesManager.CheckMouseInArea(radiusModArea))
                    return;

                mapTarget.ModifyFloorMesh();
            }
            else if(mapTarget.currentTool == BattleMap.TerrainTools.TileType)
            {
                if (overUI)
                    return;

                if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && onHoldTerrainEditing))
                    mapTarget.ModifyType();
                else
                    mapTarget.ResetAuxTileRegistry();
            }
        }
        else if(toolMode == ToolMode.Coloring)
        {
            ColorManager._instance.TryColor();
        }
    }

    public void CameraClickMove(Vector3 clickPos)
    {
        if (activeCamera == null)
            return;

        Ray clickRay = activeCamera.cameraComp.ScreenPointToRay(clickPos);
        RaycastHit hitInfo;
        if(Physics.Raycast(clickRay, out hitInfo, 100f, mapTarget.terrainLayer))
        {
            activeCamera.AutoNavigateTo(hitInfo.point, 2f);
        }
    }

    public void ToggleToolScroll()
    {
        ToggleToolScroll(!modeScrollPanel.activeSelf, false);
    }

    public void ToggleToolScroll(bool active, bool recalculateList)
    {
        if (active)
        {
            modeButton.color = selectedButtonColor;

            if (recalculateList || !scrollBuilt)
            {
                for (int i = modeContent.childCount - 1; i >= 1; i--)
                {
                    Destroy(modeContent.GetChild(i).gameObject);
                }

                GameObject modeScrollEntry = modeContent.GetChild(0).gameObject;
                RectTransform mseRT = modeScrollEntry.GetComponent<RectTransform>();

                int modeCount = Enum.GetValues(typeof(ToolMode)).Length;

                float startY = -1f * modeScrollEntrySpacing;
                for (int i = 0; i < modeCount; i++)
                {
                    GameObject nuEntry = Instantiate<GameObject>(modeScrollEntry, modeContent);
                    RectTransform entryRT = nuEntry.GetComponent<RectTransform>();

                    /*
                    entryRT.offsetMin = mseRT.offsetMin;
                    entryRT.offsetMax = mseRT.offsetMax;
                    entryRT.anchorMin = mseRT.anchorMin;
                    entryRT.anchorMax = mseRT.anchorMax;
                    */

                    float posY = startY;
                    Vector2 auxRTPos = mseRT.anchoredPosition;
                    auxRTPos.y = posY;
                    entryRT.anchoredPosition = auxRTPos;
                    startY = posY - modeScrollEntrySpacing - entryRT.sizeDelta.y;

                    nuEntry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = MiscTools.GetSpacedForm(((ToolMode)i).ToString());

                    int index = i;
                    nuEntry.GetComponent<HoldButton>().onRelease.AddListener(
                        delegate {
                            ChangeToolMode(((ToolMode)index));
                            modeButton.color = unselectedButtonColor;
                            modeScrollPanel.SetActive(false);
                        });

                    for(int t = 0; t < modeTips.Count; t++)
                    {
                        if(modeTips[t].mode == (ToolMode)index)
                        {
                            TooltipManager._instance.AddTip(entryRT, modeTips[t].tooltip);
                            break;
                        }
                    }

                    nuEntry.SetActive(true);
                }

                Vector2 sd = modeContent.sizeDelta;
                sd.y = -startY;
                modeContent.sizeDelta = sd;

                scrollBuilt = true;
            }

            if (ColorManager._instance.colorMenuActive)
                ColorManager._instance.ToggleColorMenu();
        }
        else
            modeButton.color = unselectedButtonColor;

        modeScrollPanel.SetActive(active);
    }

    public void UpdateGameEditMode()
    {
        if (!gameEditToggle.toggleValue)
        {
            if (toolMode != ToolMode.GameMode)
                return;

            editModeLabels.gameObject.SetActive(true);

            if (editModeLabels.chosenIndex == 0)
                ChangeToolMode(ToolMode.TerrainEditor);
            else if (editModeLabels.chosenIndex == 1)
                ChangeToolMode(ToolMode.UnitMaker);
            else if (editModeLabels.chosenIndex == 2)
                ChangeToolMode(ToolMode.ObjectEditor);
            else if (editModeLabels.chosenIndex == 3)
                ChangeToolMode(ToolMode.Coloring);
        }
        else
        {
            if (toolMode == ToolMode.GameMode)
                return;

            editModeLabels.gameObject.SetActive(false);

            mapModeController.SetOutlineMenuActive(false);

            ChangeToolMode(ToolMode.GameMode);
        }
    }

    private void SetGameEditMode(bool gameMode)
    {
        gameEditToggle.ForceValue(gameMode);

        UpdateGameEditMode();
    }

    public void ToggleHUD(bool show)
    {
        if (show)
        {
            editModeLabels.gameObject.SetActive(!gameEditToggle.toggleValue);
            gameEditToggle.gameObject.SetActive(true);
            return;
        }

        editModeLabels.gameObject.SetActive(false);
        gameEditToggle.gameObject.SetActive(false);
    }

    public void TabLabelClick()
    {
        if (toolMode == ToolMode.GameMode)
            return;

        if (editModeLabels.chosenIndex == 0)
            ChangeToolMode(ToolMode.TerrainEditor);
        else if (editModeLabels.chosenIndex == 1)
            ChangeToolMode(ToolMode.UnitMaker);
        else if (editModeLabels.chosenIndex == 2)
            ChangeToolMode(ToolMode.ObjectEditor);
        else if (editModeLabels.chosenIndex == 3)
            ChangeToolMode(ToolMode.Coloring);
    }

    public void ChangeToolMode(ToolMode nuMode)
    {
        if(toolMode == ToolMode.GameMode && nuMode != ToolMode.GameMode)
        {
            //close things of markmanager and distance ruler
            //MapManager._instance.mapTarget.SetCellNumberingVisibility(GameModeManager._instance.updateTileFlag);
            GameModeManager._instance.tileNumberToggle.ForceValue(false); // .SetIsOnWithoutNotify(false);
        }

        Debug.Log("changed to " + nuMode);
        toolMode = nuMode;
        modeButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = MiscTools.GetSpacedForm(nuMode.ToString());

        if (shapeScreen.activeSelf && nuMode != ToolMode.ObjectEditor)
            ShapesManager._instance.CloseShapeMode();

        if (terrainScreen.activeSelf && nuMode != ToolMode.TerrainEditor)
        {   
            mapTarget.ShowTerrainTypes(MapManager._instance.showTileTypes);

            mapTarget.SetMapLook(BattleMap.MapLookState.Default);

            mapTarget.PostMorphCleanup();
            ColorManager._instance.SetColorMarkerVisibility(true);
        }
        else if(coloringScreen.activeSelf && nuMode != ToolMode.Coloring)
        {
            if (MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(false);
        }

        terrainScreen.SetActive(nuMode == ToolMode.TerrainEditor);
        shapeScreen.SetActive(nuMode == ToolMode.ObjectEditor);
        coloringScreen.SetActive(nuMode == ToolMode.Coloring);
        unitScreen.SetActive(nuMode == ToolMode.UnitMaker);
        gameScreen.SetActive(nuMode == ToolMode.GameMode);

        gridToolPanel.gameObject.SetActive(nuMode != ToolMode.TerrainEditor);

        if (!terrainScreen.activeSelf)
        {
            ChangeTileStyle(PlayerPrefs.GetInt("TileStyle", 1), false);
        }

        if (terrainScreen.activeSelf)
        {
            ChangeTileStyle(0, false);

            mapTarget.SetMapLook(BattleMap.MapLookState.TerrainEdit);

            mapTarget.ShowTerrainTypes(true);
            mapTarget.ChangeTool((int)BattleMap.TerrainTools.TileModifying);

            ColorManager._instance.SetColorMarkerVisibility(false);
        }
        else if (shapeScreen.activeSelf)
        {
            ShapesManager._instance.UpdateShapeMenu();
            //mapTarget.SetSavedGridVisibility();
        }
        else if (unitScreen.activeSelf)
        {
            PieceManager._instance.PieceDisplayPanel.unitPanelButton.SetActive(true);
        }
        else if (coloringScreen.activeSelf)
        {
            ColorManager._instance.ChoosePaintTool(0);
        }


        PieceManager._instance.SetPieceButtonOptions(false);
        PieceManager._instance.SetDisplayPanelActive(nuMode == ToolMode.GameMode || nuMode == ToolMode.UnitMaker); // != ToolMode.TerrainTool);
        MarkManager._instance.ShowMarks(nuMode == ToolMode.GameMode);
        ColorManager._instance.colorPanelAnim.gameObject.SetActive(shapeScreen.activeSelf || coloringScreen.activeSelf);
    }

    public void ChangeTerrainTool(int toolIndex)
    {
        mapTarget.ChangeTool(toolIndex);
    }

    public void EnableControls(bool enabled)
    {
        controlEnabled = enabled;
    }

    public void SetInvertX(bool value)
    {
        invertX = value;
    }

    public void SetInvertY(bool value)
    {
        invertY = value;
    }

    public void ChangeTileStyle(int type, bool saveOption = true)
    {
        if(type < tileStyles.Length)
            tileMaterial = tileStyles[type].styleMat;

        activeTileStyle = type;

        if (saveOption)
        {
            //save pref
            PlayerPrefs.SetInt("TileStyle", type);
        }

        mapTarget.UpdateMapModeLook();
    }

    public void ChangeColorStyle(bool checkered, bool heighted)
    {
        checkeredColor = checkered;
        heightColor = heighted;
    }

    public void ActivatePointer(bool value)
    {
        pointerActive = value;
        cellPointObject.gameObject.SetActive(value);
    }

    public void UpdateCellPointer(Vector3 pointerWorldPos)
    {
        if (!pointerActive)
            return;

        Vector2 coodPosition = mapTarget.TranslateToGridCoordinates(pointerWorldPos);
        bool updatePointer = (currentCoodX != coodPosition.x) || (currentCoodY != coodPosition.y);

        if (!pointerSetup || updatePointer)
        {
            currentCoodX = coodPosition.x;
            currentCoodY = coodPosition.y;

            if (!pointerSetup)
                pointerSetup = true;

            Vector3[] borderHeightSet = mapTarget.GetCellBorderHeights(coodPosition);
            if (borderHeightSet.Length == 0)
                return;

            cellPointObject.GetChild(0).position = (0.5f * (borderHeightSet[0] + borderHeightSet[2])) + (pointerHeight * Vector3.up);
            Vector3 pointForward = (borderHeightSet[2] - borderHeightSet[0]).normalized;
            Vector3 pointRight = (borderHeightSet[1] - borderHeightSet[0]).normalized;
            Vector3 pointUp = Vector3.Cross(pointForward, pointRight);
            cellPointObject.GetChild(0).rotation = Quaternion.LookRotation(pointForward, pointUp);
            cellPointObject.GetChild(0).localScale = new Vector3(0.1f, 0.1f, 0.9f * Vector3.Distance(borderHeightSet[2], borderHeightSet[0]));

            cellPointObject.GetChild(1).position = (0.5f * (borderHeightSet[0] + borderHeightSet[1])) + (pointerHeight * Vector3.up);
            pointForward = (borderHeightSet[1] - borderHeightSet[0]).normalized;
            pointRight = (borderHeightSet[1] - borderHeightSet[3]).normalized;
            pointUp = Vector3.Cross(pointForward, pointRight);
            cellPointObject.GetChild(1).rotation = Quaternion.LookRotation(pointForward, pointUp);
            cellPointObject.GetChild(1).localScale = new Vector3(0.1f, 0.1f, 0.9f * Vector3.Distance(borderHeightSet[1], borderHeightSet[0]));

            cellPointObject.GetChild(2).position = (0.5f * (borderHeightSet[1] + borderHeightSet[3])) + (pointerHeight * Vector3.up);
            pointForward = (borderHeightSet[1] - borderHeightSet[3]).normalized;
            pointRight = (borderHeightSet[2] - borderHeightSet[3]).normalized;
            pointUp = Vector3.Cross(pointForward, pointRight);
            cellPointObject.GetChild(2).rotation = Quaternion.LookRotation(pointForward, pointUp);
            cellPointObject.GetChild(2).localScale = new Vector3(0.1f, 0.1f, 0.9f * Vector3.Distance(borderHeightSet[1], borderHeightSet[3]));

            cellPointObject.GetChild(3).position = (0.5f * (borderHeightSet[2] + borderHeightSet[3])) + (pointerHeight * Vector3.up);
            pointForward = (borderHeightSet[3] - borderHeightSet[2]).normalized;
            pointRight = (borderHeightSet[0] - borderHeightSet[2]).normalized;
            pointUp = Vector3.Cross(pointForward, pointRight);
            cellPointObject.GetChild(3).rotation = Quaternion.LookRotation(pointForward, pointUp);
            cellPointObject.GetChild(3).localScale = new Vector3(0.1f, 0.1f, 0.9f * Vector3.Distance(borderHeightSet[3], borderHeightSet[2]));
        }
    }
}
