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

    [Header("Path Building")]
    public GameObject distanceSegmentPrefab;

    private Vector3[] pathTestPoints;
    private List<PathMark> pathMarks;
    private Vector2 lastMarkCoordinate;

    public int baseMovementWeight = 1;
    public int dangerousTerrainWeight = 1;
    public int pitTerrainWeight = 1;

    private int checkHostile = 1; //0 - No, 1 - Check enemies, 2 - Check players
    private bool checkLevitating = false;

    public float groundPathDiff = 0.2f;
    public float endsAdditionalDistance = 0.1f;

    public float spaceAddMarkerScaleFactor = 0.75f;

    private GameObject currentBuiltPathParent;

    private bool rulerMade = false;

    private Toggle distanceToolIsLevitating;

    [Space(10f)]
    [SerializeField] private TextMeshProUGUI distanceEnemyLabel;

    private Toggle distanceToolConsiderEngagementWithEnemies;
    private Toggle distanceToolConsiderEngagementWithPlayers;

    [Header("Attack Tool")]
    public GameObject attackRingPrefab;
    public GameObject attackLinePrefab;
    public Color attackLineColor = Color.white;

    private GameObject attackToolParent;

    private bool attackLineMade = false;

    private RectTransform attackerLabel;
    private RectTransform attackerAspects;
    private int attackerBoonCurseIndex = 0;
    private RectTransform defenderLabel;
    private RectTransform defenderAspects;

    public LayerMask tileLayer;

    private Toggle attackToolIsRanged;

    [Header("UI")]
    public GameObject toolButtonsParent;

    public GameObject spaceAddPrefab;
    public GameObject rulerTotalPrefab;
    public RectTransform distanceRulerAspectsParent;
    public RectTransform attackRulerAspectsParent;
    public GameObject distanceRulerUIOptions;
    public GameObject attackRulerUIOptions;

    private RectTransform totalSpaceMarker;
    public GameObject attackTypePrefab;
    public GameObject attackAspectPanelPrefab;
    public float attackTypeLabelDownFactor = 0.15f;
    private int totalSpaceCount = 0;

    public Color buttonColorChosen = Color.gray;
    public Color buttonColorUnchosen = Color.white;
    public float chosenButtonScaleUp = 1.35f;

    public Image stepRulerButton;
    public Image attackToolButton;
    public Image tokenToolButton;

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

    //tool managing inputs
    [System.Serializable]
    public enum ToolMode
    {
        None,
        DistanceTool,
        AttackTool,
        TokenTool,
        MarkerTool
    }
    public ToolMode currentToolMode { get; private set; }
    private int distanceToolPhase = 0; //0 - waiting first click, 1 - ruling
    private Vector2 firstDistanceCood;
    private Vector2 secondDistanceCood;


    private int attackToolPhase = 0;
    private UnitPiece attackingPiece;
    private UnitPiece defendingPiece;

    private struct PathMark
    {
        public int value;
        public Vector2 coordinate1;
        public Vector2 coordinate2;
        public Vector3 worldPos;
        public RectTransform associatedMarker;
    }

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

        currentToolMode = ToolMode.None;

        checkHostile = 0;

        distanceToolIsLevitating = distanceRulerUIOptions.transform.GetChild(0).GetComponent<Toggle>();
        distanceToolConsiderEngagementWithEnemies = distanceRulerUIOptions.transform.GetChild(1).GetComponent<Toggle>();
        distanceToolConsiderEngagementWithPlayers = distanceRulerUIOptions.transform.GetChild(2).GetComponent<Toggle>();

        attackToolIsRanged = attackRulerUIOptions.transform.GetChild(0).GetComponent<Toggle>();

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

        if (currentToolMode == ToolMode.DistanceTool)
        {
            DistanceToolUpdate();
        }
        else if(currentToolMode == ToolMode.AttackTool)
        {
            AttackToolUpdate();
        }
        else if(currentToolMode == ToolMode.TokenTool)
        {
            TokenToolUpdate();
        }

        /*
        //VEEEEEERY innefficient, but helpful
        Vector3 pointerPlace = Vector3.zero;
        if(MapManager._instance.mapTarget.GetFloorIntersection(MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition), out pointerPlace))
        {
            pointerTest.position = pointerPlace;
        }
        */
    }

    public void ToolButtonClick(int index)
    {
        ToolMode target = (ToolMode)index;
        ToolMode prev = currentToolMode;

        DisableTool(currentToolMode);

        if (target != prev)
            EnableTool(target);

        UnitManager._instance.generalInputs.SetActive(currentToolMode == ToolMode.None);
        UnitManager._instance.optionButton.SetActive(currentToolMode == ToolMode.None);
    }

    private void EnableTool(ToolMode target)
    {
        PieceManager._instance.SetPieceButtonOptions(false);
        PieceManager._instance.SetDisplayPanelActive(false);

        if(target == ToolMode.DistanceTool)
        {
            currentToolMode = ToolMode.DistanceTool;
            distanceToolPhase = 0;

            if(currentBuiltPathParent != null)
                currentBuiltPathParent.SetActive(true);
            
            distanceRulerAspectsParent.gameObject.SetActive(true);
            UpdatePathMarkers();

            stepRulerButton.color = buttonColorChosen;
            stepRulerButton.GetComponent<RectTransform>().localScale = chosenButtonScaleUp * Vector3.one;

            distanceEnemyLabel.text = "None";
            if (checkHostile == 1)
                distanceEnemyLabel.text = "Foes";
            else if (checkHostile == 2)
                distanceEnemyLabel.text = "Characters";

            distanceRulerUIOptions.SetActive(true);
        }
        else if(target == ToolMode.AttackTool)
        {
            currentToolMode = ToolMode.AttackTool;
            attackToolPhase = 0;

            if (attackToolParent != null)
                attackToolParent.SetActive(true);

            attackToolButton.color = buttonColorChosen;
            attackToolButton.GetComponent<RectTransform>().localScale = chosenButtonScaleUp * Vector3.one;

            if (attackLineMade)
            {
                if (!attackingPiece.onMap || !defendingPiece.onMap) 
                {
                    attackLineMade = false;
                    attackingPiece = null;
                    defendingPiece = null;
                }
                else
                    RecalculateAttackLine();
            }

            attackRulerUIOptions.SetActive(true);
        }
        else if(target == ToolMode.TokenTool)
        {
            currentToolMode = ToolMode.TokenTool;

            tokenToolButton.color = buttonColorChosen;
            tokenToolButton.GetComponent<RectTransform>().localScale = chosenButtonScaleUp * Vector3.one;
        }
    }

    private void DisableTool(ToolMode target)
    {
        if(target == ToolMode.DistanceTool)
        {
            currentToolMode = ToolMode.None;
            if (MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(false);

            if (currentBuiltPathParent != null)
                currentBuiltPathParent.SetActive(false);
            distanceRulerAspectsParent.gameObject.SetActive(false);

            stepRulerButton.color = buttonColorUnchosen;
            stepRulerButton.GetComponent<RectTransform>().localScale = Vector3.one;

            distanceRulerUIOptions.SetActive(false);
        }
        else if(target == ToolMode.AttackTool)
        {
            currentToolMode = ToolMode.None;
            if (MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(false);

            if (attackToolParent != null)
            {
                for(int i = attackToolParent.transform.childCount -1; i >= 0; i--)
                {
                    Destroy(attackToolParent.transform.GetChild(i).gameObject);
                }

                attackToolParent.SetActive(false);

                for (int i = attackRulerAspectsParent.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(attackRulerAspectsParent.GetChild(i).gameObject);
                }
                attackRulerAspectsParent.gameObject.SetActive(false);
            }

            attackToolButton.color = buttonColorUnchosen;
            attackToolButton.GetComponent<RectTransform>().localScale = Vector3.one;

            attackRulerUIOptions.SetActive(false);
        }
        else if(target == ToolMode.TokenTool)
        {
            currentToolMode = ToolMode.None;
            if (MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(false);

            tokenToolButton.color = buttonColorUnchosen;
            tokenToolButton.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    public void OnPiecesLoadedCall()
    {
        if (!uiRoundCounter.enabled)
            return;

        uiRoundCounter.UpdateRoundArray();
    }

    #region Distance Tool Section

    private void DistanceToolUpdate()
    {
        bool overUI = MapManager._instance.eventSystem.IsPointerOverGameObject();

        if (distanceToolPhase == 0 && rulerMade && (MapManager._instance.activeCamera.movingFlag || MapManager._instance.activeCamera.rotatingFlag || MapManager._instance.activeCamera.zoomingFlag))
        {
            //update path markers position
            UpdatePathMarkers();
        }

        if (overUI)
            return;

        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (distanceToolPhase == 0 && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                MapManager._instance.UpdateCellPointer(hitInfo.point);

                //check validity on map target
                firstDistanceCood = MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point);
                distanceToolPhase = 1;

                if (currentBuiltPathParent == null)
                {
                    currentBuiltPathParent = new GameObject();
                    currentBuiltPathParent.name = "PathParent";
                    currentBuiltPathParent.transform.parent = transform;
                }

                //destroy previous markers
                for (int i = distanceRulerAspectsParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(distanceRulerAspectsParent.GetChild(i).gameObject);
                }

                GameObject startMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                startMarker.name = "StartMark";
                startMarker.transform.GetChild(0).gameObject.SetActive(false);
                startMarker.transform.GetChild(1).gameObject.SetActive(true);
                startMarker.transform.position = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(firstDistanceCood) + ((groundPathDiff * 0.5f) * Vector3.up);
                startMarker.transform.localScale = (0.5f * MapManager._instance.mapTarget.cellScale) * Vector3.one;

                startMarker.transform.parent = currentBuiltPathParent.transform;
            }
        }
        else if (distanceToolPhase == 1 && Input.GetMouseButtonDown(0))
        {
            distanceToolPhase = 0;
            rulerMade = true;

            BuildPathMarkers();
        }
        else if (distanceToolPhase == 1)
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                MapManager._instance.UpdateCellPointer(hitInfo.point);

                secondDistanceCood = MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point);

                bool validPos = !float.IsPositiveInfinity(secondDistanceCood.x) && !PieceManager._instance.IsPieceInCoordinates(secondDistanceCood);

                if (validPos)
                {
                    CalculatePath();
                }
            }
        }
        else if (distanceToolPhase == 0)
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                if (!MapManager._instance.pointerActive)
                    MapManager._instance.ActivatePointer(true);

                MapManager._instance.UpdateCellPointer(hitInfo.point);
            }
        }
    }

    public void CalculatePath()
    {
        checkLevitating = distanceToolIsLevitating.isOn;

        Vector3[] prePath = CoodsToWorldPoints(MakePath(firstDistanceCood, secondDistanceCood, checkLevitating, checkHostile > 0));

        BuildPathDisplay(prePath);
    }
    
    private void RebuildPath()
    {
        for (int i = distanceRulerAspectsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(distanceRulerAspectsParent.GetChild(i).gameObject);
        }

        CalculatePath();
        BuildPathMarkers();
    }

    public Vector2[] MakePath(Vector2 from, Vector2 to, bool levitating, bool considerEngagement)
    {
        List<Vector2> path = new List<Vector2>();

        if (pathMarks == null)
            pathMarks = new List<PathMark>();
        else
            pathMarks.Clear();

        if (!MapManager._instance.mapTarget.CoordinateInGrid(from))
            return new Vector2[0];

        if (!MapManager._instance.mapTarget.CoordinateInGrid(to))
            return new Vector2[0];

        //List<GridCell> closedSet = new List<GridCell>();
        List<Vector2> openSet = new List<Vector2>();

        Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>();
        Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();

        Vector3 toWorldPos = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(to);
        Vector3 fromWorldPos = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(from);

        //start node is our staring cell
        openSet.Add(from);
        gScore.Add(from, 0f);
        fScore.Add(from, Vector3.Distance(toWorldPos, fromWorldPos)); //heuristic is simple distance
        cameFrom.Add(from, Vector2.negativeInfinity);

        while (openSet.Count > 0)
        {
            Vector2 current = GetMinWeightInSet(openSet, fScore);
            if (current == to)
            {
                lastMarkCoordinate = current;
                totalSpaceCount = 0;
                //we make the pathway
                while (true)
                {
                    path.Add(current);

                    if (float.IsNegativeInfinity(cameFrom[current].x))
                        break;

                    int weightStep = GetWeightDifference(cameFrom[current], current, levitating, considerEngagement);
                    if(weightStep > baseMovementWeight)
                    {
                        PathMark addMark = new PathMark();
                        addMark.value = weightStep - baseMovementWeight;
                        addMark.coordinate1 = cameFrom[current];
                        addMark.coordinate2 = current;
                        pathMarks.Add(addMark);
                    }

                    totalSpaceCount += weightStep;
                    current = cameFrom[current];
                }

                path.Reverse();

                return path.ToArray();
            }

            openSet.Remove(current);
            //closedSet.Add(current);

            List<Vector2> neighbors = GetNeighbors(current);

            for (int i = 0; i < neighbors.Count; i++)
            {
                float tentative_g = gScore[current] + GetWeightDifference(current, neighbors[i], levitating, considerEngagement);

                if (gScore.ContainsKey(neighbors[i]) && tentative_g >= gScore[neighbors[i]])
                    continue;

                //we record the best path
                if (!openSet.Exists(x => x == neighbors[i])) //new node
                    openSet.Add(neighbors[i]);

                if (!cameFrom.ContainsKey(neighbors[i]))
                {
                    cameFrom.Add(neighbors[i], current);
                }
                else
                    cameFrom[neighbors[i]] = current;

                if (!gScore.ContainsKey(neighbors[i]))
                    gScore.Add(neighbors[i], tentative_g);
                else
                    gScore[neighbors[i]] = tentative_g;

                Vector3 currentWorldPos = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(current);

                if (!fScore.ContainsKey(neighbors[i]))
                    fScore.Add(neighbors[i], tentative_g + Vector3.Distance(currentWorldPos, toWorldPos));
                else
                    fScore[neighbors[i]] = tentative_g + Vector3.Distance(currentWorldPos, toWorldPos);
            }
        }

        return new Vector2[0];
    }

    private void BuildPathDisplay(Vector3[] path)
    {
        if(currentBuiltPathParent == null)
        {
            currentBuiltPathParent = new GameObject();
            currentBuiltPathParent.name = "PathParent";
            currentBuiltPathParent.transform.parent = transform;
        }
        else
        {
            for(int i = currentBuiltPathParent.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(currentBuiltPathParent.transform.GetChild(i).gameObject);
            }
        }

        if (path.Length > 0)
        {
            GameObject startMarker = Instantiate<GameObject>(distanceSegmentPrefab);
            startMarker.name = "StartMark";
            startMarker.transform.GetChild(0).gameObject.SetActive(false);
            startMarker.transform.GetChild(1).gameObject.SetActive(true);
            startMarker.transform.position = path[0] + ((groundPathDiff * 0.5f) * Vector3.up);
            startMarker.transform.localScale = (0.5f * MapManager._instance.mapTarget.cellScale) * Vector3.one;

            startMarker.transform.parent = currentBuiltPathParent.transform;
        }

        Vector3 lastDir = Vector3.zero;
        //spawn corner elements when needed
        for(int i = 0; i < path.Length - 1; i++)
        {
            Vector3 p1 = path[i] + ((groundPathDiff * 0.5f) * Vector3.up);
            Vector3 p2 = path[i + 1] + ((groundPathDiff * 0.5f) * Vector3.up);

            float hDiff = p2.y - p1.y;

            Vector3 d1 = p1;
            d1.y = 0f;
            Vector3 d2 = p2;
            d2.y = 0f;
            Vector3 currentDir = (d2 - d1).normalized;

            if(currentDir != lastDir && (i > 0))
            {
                //spawn a circle
                GameObject cornerMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                cornerMarker.name = "corner_" + i;
                cornerMarker.transform.GetChild(0).gameObject.SetActive(false);
                cornerMarker.transform.GetChild(1).gameObject.SetActive(true);
                cornerMarker.transform.position = p1;
                cornerMarker.transform.localScale = (0.25f * MapManager._instance.mapTarget.cellScale) * Vector3.one;

                cornerMarker.transform.parent = currentBuiltPathParent.transform;
            }

            if (hDiff == 0)
            {
                GameObject nuPathMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                nuPathMarker.name = "path_" + i;
                nuPathMarker.transform.rotation = Quaternion.LookRotation((p2 - p1).normalized, Vector3.up);
                nuPathMarker.transform.position = (0.5f * (p2 - p1)) + p1;
                Vector3 markerScale = (0.25f * MapManager._instance.mapTarget.cellScale) * Vector3.one;
                markerScale.z = (p2 - p1).magnitude;
                nuPathMarker.transform.localScale = markerScale;

                nuPathMarker.transform.parent = currentBuiltPathParent.transform;
            }
            else
            {
                Vector3 moveCorrection = (hDiff > 0) ? (p1 - p2).normalized : (p2 - p1).normalized;
                Vector3 p3 = ((0.5f * (p2 - p1)) + p1) + (endsAdditionalDistance * moveCorrection);
                p3.y = p1.y;
                Vector3 p4 = p3 + (hDiff * Vector3.up);
                p4.y = p2.y;

                //height difference added
                //start marker
                GameObject startPathMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                startPathMarker.name = "path_" + i + "_0";
                startPathMarker.transform.rotation = Quaternion.LookRotation((p3 - p1).normalized, Vector3.up);
                startPathMarker.transform.position = (0.5f * (p3 - p1)) + p1;
                Vector3 markerScale = (0.25f * MapManager._instance.mapTarget.cellScale) * Vector3.one;
                markerScale.z = (p3 - p1).magnitude;
                startPathMarker.transform.localScale = markerScale;

                startPathMarker.transform.parent = currentBuiltPathParent.transform;

                //mid marker
                Vector3 segmentUp = (hDiff > 0) ? (p1 - p3).normalized : (p2 - p4).normalized;

                GameObject midPathMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                midPathMarker.name = "path_" + i + "_2";
                midPathMarker.transform.rotation = Quaternion.LookRotation((p4 - p3).normalized, segmentUp);
                midPathMarker.transform.position = (0.5f * (p4 - p3)) + p3;
                markerScale = (0.25f * MapManager._instance.mapTarget.cellScale) * Vector3.one;
                markerScale.z = (p4 - p3).magnitude;
                midPathMarker.transform.localScale = markerScale;

                midPathMarker.transform.parent = currentBuiltPathParent.transform;

                //end marker
                GameObject endPathMarker = Instantiate<GameObject>(distanceSegmentPrefab);
                endPathMarker.name = "path_" + i + "_2";
                endPathMarker.transform.rotation = Quaternion.LookRotation((p2 - p4).normalized, Vector3.up);
                endPathMarker.transform.position = (0.5f * (p2 - p4)) + p4;
                markerScale = (0.25f * MapManager._instance.mapTarget.cellScale) * Vector3.one;
                markerScale.z = (p2 - p4).magnitude;
                endPathMarker.transform.localScale = markerScale;

                endPathMarker.transform.parent = currentBuiltPathParent.transform;
            }

            lastDir = currentDir;
        }

        if (path.Length > 0)
        {
            GameObject endMarker = Instantiate<GameObject>(distanceSegmentPrefab);
            endMarker.name = "endMark";
            endMarker.transform.GetChild(0).gameObject.SetActive(false);
            endMarker.transform.GetChild(1).gameObject.SetActive(true);
            endMarker.transform.position = path[path.Length - 1] + ((groundPathDiff * 0.5f) * Vector3.up);
            endMarker.transform.localScale = (0.5f * MapManager._instance.mapTarget.cellScale) * Vector3.one;

            endMarker.transform.parent = currentBuiltPathParent.transform;
        }
    }

    private void BuildPathMarkers()
    {
        for(int i = distanceRulerAspectsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(distanceRulerAspectsParent.GetChild(i).gameObject);
        }

        for(int i = 0; i < pathMarks.Count; i++)
        {
            PathMark pm = pathMarks[i];

            Vector3 p1 = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(pm.coordinate1) + ((groundPathDiff * 0.5f) * Vector3.up);
            Vector3 p2 = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(pm.coordinate2) + ((groundPathDiff * 0.5f) * Vector3.up);

            float hDiff = p2.y - p1.y;

            GameObject nuSpaceAddMarker = Instantiate<GameObject>(spaceAddPrefab, distanceRulerAspectsParent);
            nuSpaceAddMarker.name = "marker_" + i;

            Vector3 worldPos = (0.5f * (p2 - p1)) + p1 + (0.05f * groundPathDiff * nuSpaceAddMarker.transform.forward);
            if (hDiff != 0)
            {
                Vector3 moveCorrection = (hDiff > 0) ? (p1 - p2).normalized : (p2 - p1).normalized;
                Vector3 p3 = ((0.5f * (p2 - p1)) + p1) + (endsAdditionalDistance * moveCorrection);
                p3.y = p1.y;
                Vector3 p4 = p3 + (hDiff * Vector3.up);
                p4.y = p2.y;

                worldPos = (0.5f * (p4 - p3)) + p3 + (0.05f * groundPathDiff * nuSpaceAddMarker.transform.forward);
            }

            RectTransform markerRT = nuSpaceAddMarker.GetComponent<RectTransform>();
            markerRT.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(worldPos);

            float baseSize = markerSizeFactor * Screen.width;
            markerRT.sizeDelta = baseSize * Vector2.one;

            nuSpaceAddMarker.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "+" + pm.value;

            pm.associatedMarker = markerRT;
            pm.worldPos = worldPos;

            pathMarks[i] = pm;
        }

        GameObject nuSpaceTotal = Instantiate<GameObject>(rulerTotalPrefab, distanceRulerAspectsParent);
        nuSpaceTotal.name = "total";
        Vector3 totalMarkPos = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(lastMarkCoordinate) + ((groundPathDiff * 0.5f) * Vector3.up);

        totalSpaceMarker = nuSpaceTotal.GetComponent<RectTransform>();
        totalSpaceMarker.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(totalMarkPos);

        float frac = totalSpaceMarker.sizeDelta.y / totalSpaceMarker.sizeDelta.x;
        totalSpaceMarker.sizeDelta = new Vector2(totalSizeFactor * Screen.width, frac * totalSizeFactor * Screen.width);
        nuSpaceTotal.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = totalSpaceCount + " spaces";
    }
    private void UpdatePathMarkers()
    {
        if (pathMarks == null)
            return;

        List<RectIndexSet> markerRects = new List<RectIndexSet>();

        for(int i = 0; i < pathMarks.Count; i++)
        {
            pathMarks[i].associatedMarker.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(pathMarks[i].worldPos);

            RectIndexSet nuSetMark = new RectIndexSet();
            nuSetMark.worldPosition = pathMarks[i].worldPos;
            nuSetMark.placementRect = pathMarks[i].associatedMarker;
            markerRects.Add(nuSetMark);
        }

        Vector3 totalMarkPos = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(lastMarkCoordinate) + ((groundPathDiff * 0.5f) * Vector3.up);
        totalSpaceMarker.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(totalMarkPos);

        RectIndexSet finalSetMark = new RectIndexSet();
        finalSetMark.worldPosition = totalMarkPos;
        finalSetMark.placementRect = totalSpaceMarker;

        markerRects.Add(finalSetMark);

        ArrangeByClosenessToCamera(markerRects);
    }

    private Vector3[] CoodsToWorldPoints(Vector2[] coods)
    {
        Vector3[] worldPoints = new Vector3[coods.Length];

        for (int i = 0; i < coods.Length; i++)
        {
            worldPoints[i] = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(coods[i]);
        }

        return worldPoints;
    }

    private int GetWeightDifference(Vector2 from, Vector2 to, bool levitating, bool considerEngagement, bool finalPiece = false)
    {
        int movementValue = baseMovementWeight;

        if (PieceManager._instance.IsEnemyPieceInCoordinates(to, checkHostile == 1, true)) // .IsPieceInCoordinates(to))
        {
            return int.MaxValue;
        }

        if (MapManager._instance.mapTarget.GetTileTypeAt(to) == (int)BattleMap.TerrainTypes.Impassable)
        {
            return int.MaxValue;
        }

        int tVal = MapManager._instance.mapTarget.GetTileValueAt(to);
        if (tVal >= MapManager._instance.mapTarget.maxCellHeight)
        {
            return int.MaxValue;
        }
        else if(tVal < 0)
        {
            tVal = pitTerrainWeight;
        }

        int gVal = tVal - MapManager._instance.mapTarget.GetTileValueAt(from);
        if(gVal < 0)
        {
            gVal = 0; //chequear fall damage
        }
        else if(levitating && gVal > 0)
        {
            gVal = 0;
        }

        movementValue += gVal;

        //chequeamos engagement
        if (considerEngagement && NeighborHasEnemy(from, checkHostile == 2))
        {
            movementValue += 1;
        }

        //chequeamos terreno
        if(MapManager._instance.mapTarget.GetTileTypeAt(from) == (int)BattleMap.TerrainTypes.Difficult)
            movementValue += 1;

        if (MapManager._instance.mapTarget.GetTileTypeAt(to) == (int)BattleMap.TerrainTypes.Dangerous)
            movementValue += dangerousTerrainWeight;

        return movementValue;
    }

    private Vector2 GetMinWeightInSet(List<Vector2> set, Dictionary<Vector2, float> fValue)
    {
        float bestValue = fValue[set[0]];
        Vector2 ret = set[0];

        for (int i = 1; i < set.Count; i++)
        {
            if (fValue[set[i]] <= bestValue)
            {
                ret = set[i];
                bestValue = fValue[set[i]];
            }
        }
        return ret;
    }

    private List<Vector2> GetNeighbors(Vector2 cell)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2 addition = new Vector2(cell.x + 1, cell.y);
        if(MapManager._instance.mapTarget.CoordinateInGrid(addition))
            neighbors.Add(addition);
        addition = new Vector2(cell.x, cell.y + 1);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
            neighbors.Add(addition);
        addition = new Vector2(cell.x - 1, cell.y);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
            neighbors.Add(addition);
        addition = new Vector2(cell.x, cell.y - 1);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
            neighbors.Add(addition);

        return neighbors;
    }

    private bool NeighborHasEnemy(Vector2 cell, bool checkForCharacter)
    {
        Vector2 addition = new Vector2(cell.x + 1, cell.y);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
        {
            UnitPiece match = PieceManager._instance.GetPieceInCoordinates(addition);
            if(match != null)
            {
                if (match is FoePiece && !checkForCharacter)
                    return true;
                else if (match is CharacterPiece && checkForCharacter)
                    return true;
            }
        }
        addition = new Vector2(cell.x, cell.y + 1);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
        {
            UnitPiece match = PieceManager._instance.GetPieceInCoordinates(addition);
            if (match != null)
            {
                if (match is FoePiece && !checkForCharacter)
                    return true;
                else if (match is CharacterPiece && checkForCharacter)
                    return true;
            }
        }
        addition = new Vector2(cell.x - 1, cell.y);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
        {
            UnitPiece match = PieceManager._instance.GetPieceInCoordinates(addition);
            if (match != null)
            {
                if (match is FoePiece && !checkForCharacter)
                    return true;
                else if (match is CharacterPiece && checkForCharacter)
                    return true;
            }
        }
        addition = new Vector2(cell.x - 1, cell.y);
        if (MapManager._instance.mapTarget.CoordinateInGrid(addition))
        {
            UnitPiece match = PieceManager._instance.GetPieceInCoordinates(addition);
            if (match != null)
            {
                if (match is FoePiece && !checkForCharacter)
                    return true;
                else if (match is CharacterPiece && checkForCharacter)
                    return true;
            }
        }

        return false;
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

    public void DistanceToggleChanged(int type)
    {
        /*
        if (type == 1 && distanceToolConsiderEngagementWithPlayers.isOn && distanceToolConsiderEngagementWithEnemies.isOn)
            distanceToolConsiderEngagementWithPlayers.SetIsOnWithoutNotify(false);
        else if (type == 2 && distanceToolConsiderEngagementWithPlayers.isOn && distanceToolConsiderEngagementWithEnemies.isOn)
            distanceToolConsiderEngagementWithEnemies.SetIsOnWithoutNotify(false);
        */

        checkHostile = type;

        distanceEnemyLabel.text = "None";
        if (checkHostile == 1)
            distanceEnemyLabel.text = "Foes";
        else if (checkHostile == 2)
            distanceEnemyLabel.text = "Characters";

        RebuildPath();
    }

    public void DistanceEnemyChange(bool right)
    {
        int auxVal = checkHostile + (right ? 1 : -1);
        if (auxVal < 0)
            auxVal = 2;
        else if (auxVal > 2)
            auxVal = 0;

        DistanceToggleChanged(auxVal);
    }

    #endregion

    #region Attack Tool Section

    Vector3 aux1 = Vector3.zero;
    Vector3 aux2 = Vector3.zero;

    private void AttackToolUpdate()
    {
        /*
            bool overUI = MapManager._instance.eventSystem.IsPointerOverGameObject();
            if (overUI)
                return;
            */

        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (attackToolPhase == 0 && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                attackRulerAspectsParent.gameObject.SetActive(true);

                MapManager._instance.UpdateCellPointer(hitInfo.point);

                firstDistanceCood = MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point);

                //check validity on map target
                if (!PieceManager._instance.IsPieceInCoordinates(firstDistanceCood, true))
                    return;

                UnitPiece gotPiece = PieceManager._instance.GetPieceInCoordinates(firstDistanceCood, true);
                if (!gotPiece.onMap)
                    return;

                attackingPiece = gotPiece;

                attackToolPhase = 1;
                attackLineMade = false;

                if (attackToolParent == null)
                {
                    attackToolParent = new GameObject();
                    attackToolParent.name = "AttackParent";
                    attackToolParent.transform.parent = transform;
                }

                for (int i = attackToolParent.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(attackToolParent.transform.GetChild(i).gameObject);
                }

                for (int i = attackRulerAspectsParent.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(attackRulerAspectsParent.GetChild(i).gameObject);
                }

                GameObject startMarker = Instantiate<GameObject>(attackRingPrefab, attackToolParent.transform);
                startMarker.name = "StartRing";
                Vector3 markerPosition = attackingPiece.GetModelPosition();
                markerPosition.y = attackingPiece.transform.position.y + 0.5f;
                startMarker.transform.position = markerPosition;
                startMarker.transform.localScale = 0.5f * attackingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * Vector3.one;

                startMarker.transform.GetChild(0).GetComponent<SpriteRenderer>().color = attackLineColor;

                startMarker.transform.parent = attackToolParent.transform;

                //display attacker mark
                DisplayAttackTypeMarker(true);
            }
        }
        else if (attackToolPhase == 1 && Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                MapManager._instance.UpdateCellPointer(hitInfo.point);

                Vector2 pressCoordinate = MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point);

                //check validity on map target
                if (!PieceManager._instance.IsPieceInCoordinates(pressCoordinate, true))
                    return;

                UnitPiece gotPiece = PieceManager._instance.GetPieceInCoordinates(pressCoordinate, true);
                if (!gotPiece.onMap)
                    return;

                if (attackingPiece == gotPiece)
                {
                    attackToolPhase = 0;
                    attackLineMade = false;
                    attackingPiece = null;
                    defendingPiece = null;

                    for (int i = attackToolParent.transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(attackToolParent.transform.GetChild(i).gameObject);
                    }

                    for (int i = attackRulerAspectsParent.transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(attackRulerAspectsParent.GetChild(i).gameObject);
                    }

                    return;
                }

                defendingPiece = gotPiece;

                attackToolPhase = 0;

                GameObject endMarker = Instantiate<GameObject>(attackRingPrefab, attackToolParent.transform);
                endMarker.name = "EndRing";
                Vector3 markerPosition = defendingPiece.GetModelPosition();
                markerPosition.y = defendingPiece.transform.position.y + 0.5f;
                endMarker.transform.position = markerPosition;
                endMarker.transform.localScale = 0.5f * defendingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * Vector3.one;

                endMarker.transform.GetChild(0).GetComponent<SpriteRenderer>().color = attackLineColor;

                endMarker.transform.parent = attackToolParent.transform;

                //display defender mark
                DisplayAttackTypeMarker(false);

                //make line correctly aligned
                BuildLine();

                //show attack info on both pieces
                DisplayAttackLineResult(attackToolIsRanged.isOn);

                attackLineMade = true;
            }
        }
        else
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                if (!MapManager._instance.pointerActive)
                    MapManager._instance.ActivatePointer(true);

                MapManager._instance.UpdateCellPointer(hitInfo.point);
            }
        }

        if (attackLineMade)
        {
            UpdateAttackTypeMarker(true);
            UpdateAttackTypeMarker(false);

            UpdateAttackLineResult();
        }
        else if (attackToolPhase == 1)
        {
            UpdateAttackTypeMarker(true);
        }
    }

    public void DisplayAttackLineResult(bool isRanged)
    {
        attackerBoonCurseIndex = 0;
        //check line of sight
        Vector3 sightPointAttacker = attackingPiece.GetModelPosition();
        sightPointAttacker.y = attackingPiece.transform.position.y;
        sightPointAttacker.y += (attackingPiece.pieceSize - 0.2f) * MapManager._instance.mapTarget.cellScale;

        Vector3 sightPointDefender = defendingPiece.GetModelPosition();
        sightPointDefender.y = defendingPiece.transform.position.y;
        sightPointDefender.y += (defendingPiece.pieceSize - 0.2f) * MapManager._instance.mapTarget.cellScale;

        aux1 = sightPointAttacker;
        aux2 = sightPointDefender;

        GameObject attackerAsp = Instantiate<GameObject>(attackAspectPanelPrefab, attackRulerAspectsParent);
        attackerAspects = attackerAsp.GetComponent<RectTransform>();

        Ray sightRay = new Ray(sightPointAttacker, (sightPointDefender - sightPointAttacker).normalized);
        RaycastHit hitInfo;

        float rangeGap = (sightPointDefender - sightPointAttacker).magnitude;
        if (Physics.Raycast(sightRay, out hitInfo, rangeGap, tileLayer))
        {
            //not in line of sight
            attackerAsp.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "No line of sight";
            attackerAsp.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            //we check closest range
            float smallestPieceDistance = float.MaxValue;
            for(int dj = 0; dj < defendingPiece.pieceSize; dj++)
            {
                Vector3 dpoint = defendingPiece.transform.position;
                dpoint.y = 0;

                for(int di = 0; di < defendingPiece.pieceSize; di++)
                {
                    //skip central pieces
                    if (di > 0 && di < (defendingPiece.pieceSize - 1) &&
                        dj > 0 && dj < (defendingPiece.pieceSize - 1))
                        continue;

                    dpoint += di * MapManager._instance.mapTarget.cellScale * Vector3.forward;
                    dpoint += dj * MapManager._instance.mapTarget.cellScale * Vector3.right;

                    for(int aj = 0; aj < attackingPiece.pieceSize; aj++)
                    {
                        Vector3 apoint = attackingPiece.transform.position;
                        apoint.y = 0;

                        for (int ai = 0; ai < attackingPiece.pieceSize; ai++)
                        {
                            //skip central pieces
                            if (ai > 0 && ai < (attackingPiece.pieceSize - 1) &&
                                aj > 0 && aj < (attackingPiece.pieceSize - 1))
                                continue;

                            apoint += ai * MapManager._instance.mapTarget.cellScale * Vector3.forward;
                            apoint += aj * MapManager._instance.mapTarget.cellScale * Vector3.right;

                            float cellDistance = Vector3.Distance(apoint, dpoint);
                            if (cellDistance <= smallestPieceDistance)
                                smallestPieceDistance = cellDistance;
                        }
                    }
                }
            }

            smallestPieceDistance -= 0.1f; //so it takes the between distance on flooring
            int rangeCeilGap = Mathf.FloorToInt(smallestPieceDistance);
            if (isRanged)
            {
                if (rangeCeilGap >= 1)
                    attackerAsp.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Target at range " + rangeCeilGap;
                else
                    attackerAsp.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Target too close to range";
            }
            else if (rangeCeilGap > 0)
                attackerAsp.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Target too far to melee";


            attackerAsp.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);

            //we check cover in defender
            if (isRanged)
            {
                sightRay = new Ray(sightPointDefender - (0.6f * Vector3.up), (sightPointAttacker - sightPointDefender).normalized);
                if (Physics.Raycast(sightRay, out hitInfo, MapManager._instance.mapTarget.cellScale, tileLayer))
                {
                    GameObject defenderAsp = Instantiate<GameObject>(attackAspectPanelPrefab, attackRulerAspectsParent);
                    defenderAspects = defenderAsp.GetComponent<RectTransform>();
                    defenderAsp.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Has Cover";

                    defenderAsp.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);

                    attackerBoonCurseIndex -= 1;
                    /*
                    GameObject nuAspect = Instantiate<GameObject>(attackerAsp.transform.GetChild(1).GetChild(0).gameObject, attackerAsp.transform.GetChild(1));
                    nuAspect.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Has Cover";
                    nuAspect.SetActive(true);
                    */
                }
            }

            //we check adjacent enemies
            bool isChara = attackingPiece is CharacterPiece;
            if(NeighborHasEnemy(attackingPiece.mapCoordinates, !isChara) && isRanged)
            {
                GameObject nuAspect = Instantiate<GameObject>(attackerAsp.transform.GetChild(1).GetChild(0).gameObject, attackerAsp.transform.GetChild(1));
                nuAspect.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Foe nearby";
                nuAspect.SetActive(true);

                attackerBoonCurseIndex -= 1;
            }

            //we check height difference. make a range addition for ranged attacks
            int heightOfAttacker = MapManager._instance.mapTarget.GetTileValueAt(attackingPiece.mapCoordinates);
            int heightOfDefender = MapManager._instance.mapTarget.GetTileValueAt(defendingPiece.mapCoordinates);
            if (isRanged && heightOfAttacker > 0)
            {
                GameObject nuAspect = Instantiate<GameObject>(attackerAsp.transform.GetChild(1).GetChild(0).gameObject, attackerAsp.transform.GetChild(1));
                nuAspect.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Range +" + heightOfAttacker;
                nuAspect.SetActive(true);
            }

            if (heightOfAttacker > heightOfDefender)
                attackerBoonCurseIndex += 1;
            else if (heightOfAttacker < heightOfDefender)
                attackerBoonCurseIndex -= 1;

            if(attackerBoonCurseIndex != 0)
            {
                GameObject nuAspect = Instantiate<GameObject>(attackerAsp.transform.GetChild(1).GetChild(0).gameObject, attackerAsp.transform.GetChild(1));
                nuAspect.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = (attackerBoonCurseIndex > 0 ? "Boon " : "Curse ") + attackerBoonCurseIndex;
                nuAspect.SetActive(true);
            }
        }
    }

    private void UpdateAttackLineResult()
    {
        if(attackerAspects != null)
        {
            attackerAspects.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(attackingPiece.GetModelPosition()) + (attackTypeLabelDownFactor * Screen.height * Vector3.up);
        }

        if(defenderAspects != null)
        {
            defenderAspects.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(defendingPiece.GetModelPosition()) + (attackTypeLabelDownFactor * Screen.height * Vector3.up);
        }
    }

    private void RecalculateAttackLine()
    {
        GameObject startMarker = Instantiate<GameObject>(attackRingPrefab, attackToolParent.transform);
        startMarker.name = "StartRing";
        Vector3 markerPosition = attackingPiece.GetModelPosition();
        markerPosition.y = attackingPiece.transform.position.y + 0.5f;
        startMarker.transform.position = markerPosition;
        startMarker.transform.localScale = 0.5f * attackingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * Vector3.one;

        startMarker.transform.GetChild(0).GetComponent<SpriteRenderer>().color = attackLineColor;

        startMarker.transform.parent = attackToolParent.transform;

        GameObject endMarker = Instantiate<GameObject>(attackRingPrefab, attackToolParent.transform);
        endMarker.name = "EndRing";
        markerPosition = defendingPiece.GetModelPosition();
        markerPosition.y = defendingPiece.transform.position.y + 0.5f;
        endMarker.transform.position = markerPosition;
        endMarker.transform.localScale = 0.5f * defendingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * Vector3.one;

        endMarker.transform.GetChild(0).GetComponent<SpriteRenderer>().color = attackLineColor;

        endMarker.transform.parent = attackToolParent.transform;

        //make line correctly aligned
        BuildLine();

        attackRulerAspectsParent.gameObject.SetActive(true);

        DisplayAttackTypeMarker(true);
        DisplayAttackTypeMarker(false);

        DisplayAttackLineResult(attackToolIsRanged.isOn);

        UpdateAttackTypeMarker(true);
        UpdateAttackTypeMarker(false);

        UpdateAttackLineResult();
    }

    private void BuildLine()
    {
        Vector3 attackerPoint = attackingPiece.GetModelPosition();
        Vector3 defenderPoint = defendingPiece.GetModelPosition();

        Vector3 fromPoint = (defenderPoint - attackerPoint).normalized;
        fromPoint.y = 0f;
        fromPoint = attackerPoint + (0.5f * attackingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * fromPoint);
        fromPoint.y = attackingPiece.transform.position.y + 0.5f;

        Vector3 toPoint = (attackerPoint - defenderPoint).normalized;
        toPoint.y = 0f;
        toPoint = defenderPoint + (0.5f * defendingPiece.pieceSize * (MapManager._instance.mapTarget.cellScale) * toPoint);
        toPoint.y = defendingPiece.transform.position.y + 0.5f;

        GameObject lineElement = Instantiate<GameObject>(attackLinePrefab, attackToolParent.transform);
        lineElement.transform.position = (0.5f * (toPoint - fromPoint)) + fromPoint;

        Vector3 pointF = (toPoint - fromPoint).normalized;
        Vector3 pointU = Vector3.Cross(pointF, Vector3.up);
        pointU = Vector3.Cross(pointU, pointF);
        lineElement.transform.rotation = Quaternion.LookRotation(pointF, pointU);

        lineElement.transform.localScale = new Vector3(0.2f, 1f, (toPoint - fromPoint).magnitude);

        lineElement.transform.GetChild(0).GetComponent<SpriteRenderer>().color = attackLineColor;
    }

    private void DisplayAttackTypeMarker(bool attacker)
    {
        GameObject uiMarker = Instantiate<GameObject>(attackTypePrefab, attackRulerAspectsParent);
        RectTransform markerRT = uiMarker.GetComponent<RectTransform>();

        markerRT.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = attacker ? "attacker" : "defender";
        if (attacker)
            attackerLabel = markerRT;
        else
            defenderLabel = markerRT;

        UpdateAttackTypeMarker(attacker);
    }

    private void UpdateAttackTypeMarker(bool attacker)
    {
        if (attacker)
        {
            attackerLabel.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(attackingPiece.GetModelPosition()) - (attackTypeLabelDownFactor * Screen.height * Vector3.up);
        }
        else
        {
            defenderLabel.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(defendingPiece.GetModelPosition()) - (attackTypeLabelDownFactor * Screen.height * Vector3.up);
        }
    }

    public void AttackToggleChanged()
    {
        for (int i = attackToolParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(attackToolParent.transform.GetChild(i).gameObject);
        }

        for (int i = attackRulerAspectsParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(attackRulerAspectsParent.GetChild(i).gameObject);
        }

        RecalculateAttackLine();
    }
    #endregion

    #region Token Tool Section
    private void TokenToolUpdate()
    {
        bool overUI = MapManager._instance.eventSystem.IsPointerOverGameObject();

        if ((MapManager._instance.activeCamera.movingFlag || MapManager._instance.activeCamera.rotatingFlag || MapManager._instance.activeCamera.zoomingFlag))
        {
            UpdateTokenPanels();
        }

        if (overUI)
            return;

        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                MapManager._instance.UpdateCellPointer(hitInfo.point);

                //check validity on map target
                if (!PieceManager._instance.IsPieceInCoordinates(MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point)))
                {
                    Vector3 mapPos = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point);

                    //create token
                    PieceManager._instance.SpawnTokenPiece(mapPos, Color.white, "", "");
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            //close the tool
            DisableTool(ToolMode.TokenTool);

            UnitManager._instance.generalInputs.SetActive(currentToolMode == ToolMode.None);
            UnitManager._instance.optionButton.SetActive(currentToolMode == ToolMode.None);
        }
        else
        {
            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                if (!MapManager._instance.pointerActive)
                    MapManager._instance.ActivatePointer(true);

                MapManager._instance.UpdateCellPointer(hitInfo.point);
            }
        }
    }

    private void UpdateTokenPanels()
    {

    }
    #endregion

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

    private void OnDrawGizmos()
    {
        if (pathTestPoints != null)
        {
            Gizmos.color = Color.magenta;

            for (int i = 0; i < pathTestPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(pathTestPoints[i], pathTestPoints[i + 1]);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(aux1, aux2);
    }
}
