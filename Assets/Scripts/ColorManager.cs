using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public static ColorManager _instance;

    public ColorSet[] colors;

    [System.Serializable]
    public struct ColorSet
    {
        public string name;
        public Sprite icon;
        public Color color;
    }

    public GameObject markerTile;
    private Transform[,] coloredTiles;
    public float markerColorAlpha = 0.6f;
    public float markerAdditionalHeight = 0.25f;
    private int lastMarkerTileX;
    private int lastMarkerTileZ;
    private bool markClickReleased = true;
    private bool markerIsErasing = false;
    public GameObject clearAllMarkerButton;

    private bool markerToTerrain = true;

    [Header("Color Menu")]
    public int iconsPerColumn = 2;
    [Range(0.01f, 0.2f)]
    public float iconSpacingH = 0.01f;
    [Range(0.01f, 0.2f)]
    public float iconSpacingV = 0.03f;
    [Range(0.05f, 0.8f)]
    public float iconSize = 0.2f;
    public GameObject colorIconPrefab;
    public RectTransform colorIconContent;

    public bool colorMenuActive { get; private set; }
    private bool menuBuilt = false;

    public Animator colorPanelAnim;
    public Image mainColorIcon;
    public Color mainColor { get; private set; }
    public Image secondaryColorIcon;
    public Color secondaryColor { get; private set; }

    [Header("Color UI")]
    public Image paintObjectButton;
    public Image markerTileButton;
    public Image eraseMarkerButton;

    public Color colorButtonSelected = Color.black;
    public Color iconButtonSelected = Color.white;
    public Color colorButtonUnselected = Color.gray;
    public Color iconButtonUnselected = Color.black;

    public ColorTool currentColorTool { get; private set; }

    [Header("Color Panel")]
    public ColorListPanel generalColorList;

    public enum ColorTool
    {
        paintObject,
        tileMarker,
        eraserMarker
    }

    private void Awake()
    {
        if (ColorManager._instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        iconSize = iconSize * Screen.height;
        iconSpacingH *= Screen.height;
        iconSpacingV *= Screen.height;

        mainColor = mainColorIcon.color;
        secondaryColor = secondaryColorIcon.color;

        clearAllMarkerButton.SetActive(false);

        if (!menuBuilt)
            UpdateColorMenu();
    }

    private void LateUpdate()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.Coloring)
            return;

        if(currentColorTool == ColorTool.tileMarker || currentColorTool == ColorTool.eraserMarker)
        {
            bool overUI = MapManager._instance.eventSystem.IsPointerOverGameObject();

            if (overUI)
                return;

            Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.floorOnlyLayer))
            {
                if (!MapManager._instance.pointerActive)
                    MapManager._instance.ActivatePointer(true);

                MapManager._instance.UpdateCellPointer(hitInfo.point);
            }
        }
    }

    public void BuildMarkerArrayCall()
    {
        //we first check if previous tiles exist
        if(coloredTiles != null)
        {
            ClearAllMarkerTiles();
        }

        coloredTiles = new Transform[MapManager._instance.cellCountX, MapManager._instance.cellCountZ];
    }

    public void LoadMarkerTiles(int[] tileCoodX, int[] tileCoodZ, Color[] colors)
    {
        for(int i = 0; i < colors.Length; i++)
        {
            int coodX = tileCoodX[i];
            int coodZ = tileCoodZ[i];

            Vector2 pointCood = new Vector2(coodX, coodZ);

            //new marker tile
            GameObject nuMarkerTile = Instantiate<GameObject>(markerTile, transform);
            Transform nuMarkerTileObj = nuMarkerTile.transform;

            if (markerToTerrain)
            {
                Vector3[] borderHeightSet = MapManager._instance.mapTarget.GetCellBorderHeights(pointCood);
                Vector3[] tileVertexes = new Vector3[(int)borderHeightSet.Length];
                for (int j = 0; j < borderHeightSet.Length; j++)
                {
                    tileVertexes[j] = borderHeightSet[j] + (markerAdditionalHeight * Vector3.up);
                }

                MeshFilter mf = nuMarkerTileObj.GetComponent<MeshFilter>();
                Mesh tileMesh = mf.mesh;
                tileMesh.SetVertices(tileVertexes);
                int[] tileTris = new int[6]
                {
                            0,2,1,2,3,1
                };
                tileMesh.SetTriangles(tileTris, 0);
                tileMesh.RecalculateNormals();
            }
            else
            {
                nuMarkerTileObj.position = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(pointCood) + (markerAdditionalHeight * Vector3.up);
                nuMarkerTileObj.rotation = Quaternion.Euler(90, 0f, 0f);
            }

            MeshRenderer mr = nuMarkerTileObj.GetComponent<MeshRenderer>();

            Color markCol = colors[i];
            markCol.a = markerColorAlpha;
            mr.material.color = markCol;

            coloredTiles[coodX, coodZ] = nuMarkerTileObj;

            nuMarkerTile.SetActive(true);
        }
    }

    public void TryColor()
    {
        bool overUI = MapManager._instance.eventSystem.IsPointerOverGameObject();

        if (overUI)
            return;

        if (currentColorTool == ColorTool.paintObject)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f))
                {
                    if (hitInfo.transform.gameObject.layer == 8)
                    {
                        Transform shapeSelected = hitInfo.transform;
                        MeshRenderer mr = shapeSelected.GetComponent<MeshRenderer>();
                        if (Input.GetMouseButtonDown(0))
                        {
                            mr.material.color = mainColor;
                            shapeSelected.parent.GetComponent<ShapeInfo>().SetColor(mainColor);
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            mr.material.color = secondaryColor;
                            shapeSelected.parent.GetComponent<ShapeInfo>().SetColor(secondaryColor);
                        }
                    }
                    else if (hitInfo.transform.tag == "Character") // piece coloring
                    {
                        UnitPiece pieceElement = hitInfo.transform.parent.GetComponent<CharacterPiece>();
                        if (pieceElement == null)
                            pieceElement = hitInfo.transform.parent.GetComponent<FoePiece>();
                        if (pieceElement == null)
                            pieceElement = hitInfo.transform.parent.GetComponent<TokenPiece>();

                        if (Input.GetMouseButtonDown(0))
                        {
                            pieceElement.SetPieceColor(mainColor);
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            pieceElement.SetPieceColor(secondaryColor);
                        }
                    }
                }
            }
        }
        else if (currentColorTool == ColorTool.tileMarker || currentColorTool == ColorTool.eraserMarker)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.terrainLayer))
                {
                    Vector2 pointCood = MapManager._instance.mapTarget.TranslateToGridCoordinates(hitInfo.point);
                    if (float.IsPositiveInfinity(pointCood.x))
                        return;

                    int coodX = Mathf.FloorToInt(pointCood.x);
                    int coodZ = Mathf.FloorToInt(pointCood.y);

                    if (coodX == lastMarkerTileX && coodZ == lastMarkerTileZ && !markClickReleased)
                        return;

                    markClickReleased = false;

                    lastMarkerTileX = coodX;
                    lastMarkerTileZ = coodZ;

                    Transform found = coloredTiles[coodX, coodZ];
                    if (found == null)
                    {
                        if (currentColorTool == ColorTool.eraserMarker)
                            return;

                        //new marker tile
                        GameObject nuMarkerTile = Instantiate<GameObject>(markerTile, transform);
                        Transform nuMarkerTileObj = nuMarkerTile.transform;

                        if (markerToTerrain)
                        {
                            Vector3[] borderHeightSet = MapManager._instance.mapTarget.GetCellBorderHeights(pointCood);
                            Vector3[] tileVertexes = new Vector3[(int)borderHeightSet.Length];
                            for (int i = 0; i < borderHeightSet.Length; i++)
                            {
                                tileVertexes[i] = borderHeightSet[i] + (markerAdditionalHeight * Vector3.up);
                            }

                            MeshFilter mf = nuMarkerTileObj.GetComponent<MeshFilter>();
                            Mesh tileMesh = mf.mesh;
                            tileMesh.SetVertices(tileVertexes);
                            int[] tileTris = new int[6]
                            {
                            0,2,1,2,3,1
                            };
                            tileMesh.SetTriangles(tileTris, 0);
                            tileMesh.RecalculateNormals();
                        }
                        else
                        {
                            nuMarkerTileObj.position = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point) + (markerAdditionalHeight * Vector3.up);
                            nuMarkerTileObj.rotation = Quaternion.Euler(90, 0f, 0f);
                        }

                        MeshRenderer mr = nuMarkerTileObj.GetComponent<MeshRenderer>();

                        Color markCol = secondaryColor;
                        if (Input.GetMouseButton(0))
                            markCol = mainColor;
                        markCol.a = markerColorAlpha;
                        mr.material.color = markCol;

                        coloredTiles[coodX, coodZ] = nuMarkerTileObj;

                        nuMarkerTile.SetActive(true);
                    }
                    else
                    {
                        if (currentColorTool == ColorTool.eraserMarker)
                        {
                            //delete old
                            Destroy(found.gameObject);
                            coloredTiles[coodX, coodZ] = null;
                        }
                        else
                        {
                            //change color
                            MeshRenderer mr = found.GetComponent<MeshRenderer>();

                            Color markCol = secondaryColor;
                            if (Input.GetMouseButton(0))
                                markCol = mainColor;
                            markCol.a = markerColorAlpha;
                            mr.material.color = markCol;
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                markClickReleased = true;
        }
    }

    public Transform[,] GetMarkerTiles()
    {
        return coloredTiles;
    }

    public void ClearAllMarkerTiles()
    {
        for(int j = 0; j < coloredTiles.GetLength(1); j++)
        {
            for(int i = 0; i < coloredTiles.GetLength(0); i++)
            {
                Transform found = coloredTiles[i, j];
                if (found == null)
                    continue;
                Destroy(found.gameObject);
                coloredTiles[i,j] = null;
            }
        }
    }

    private void UpdateColorMenu()
    {
        //clear menu
        for (int i = colorIconContent.childCount - 1; i >= 1; i--)
        {
            Destroy(colorIconContent.GetChild(i).gameObject);
        }

        float accX = 0f;
        float accY = 0f;
        int currentIcons = 0;
        int totalIcons = colors.Length;

        RectTransform prefabRT = colorIconPrefab.GetComponent<RectTransform>();

        int totalCols = totalIcons / iconsPerColumn;
        int rest = totalIcons % iconsPerColumn;

        if (rest > 0)
            totalCols += 1;

        for(int c = 0; c < totalCols; c++)
        {
            for (int r = 0; r < iconsPerColumn; r++)
            {

                GameObject nuIcon = Instantiate<GameObject>(colorIconPrefab);
                RectTransform niRT = nuIcon.GetComponent<RectTransform>();
                niRT.parent = colorIconContent;

                niRT.anchorMin = prefabRT.anchorMin;
                niRT.anchorMax = prefabRT.anchorMax;
                niRT.offsetMin = prefabRT.offsetMin;
                niRT.offsetMax = prefabRT.offsetMax;

                Vector2 iconSD = niRT.sizeDelta;
                iconSD.x = iconSD.y = iconSize;
                niRT.sizeDelta = iconSD;

                float posX = (c * (iconSize + iconSpacingH)) + iconSpacingH;
                if (posX > accX)
                    accX = posX;
                float posY = (r * (iconSize + iconSpacingV)) + iconSpacingH;
                if (posY > accY)
                    accY = posY;

                niRT.anchoredPosition = new Vector2(posX, -posY);

                int colorIndex = currentIcons;
                //icon events
                niRT.GetComponent<ShapeIcon>().OnPointerDownEvent.AddListener(delegate
                {
                    ChooseColor(colorIndex);
                });

                niRT.GetChild(0).GetComponent<Image>().sprite = colors[currentIcons].icon;

                nuIcon.SetActive(true);
                currentIcons++;

                if (currentIcons >= totalIcons)
                    break;
            }

            if (currentIcons >= totalIcons)
                break;
        }

        accX += iconSize + iconSpacingH;
        accY += iconSize + iconSpacingV;

        Vector3 contentSD = colorIconContent.sizeDelta;
        contentSD = new Vector2(accX, accY);
        colorIconContent.sizeDelta = contentSD;

        menuBuilt = true;
    }

    public void ToggleColorMenu()
    {
        SetColorMenuActive(!colorMenuActive);
    }

    private void SetColorMenuActive(bool active)
    {
        if (active)
            colorPanelAnim.SetTrigger("Show");
        else
            colorPanelAnim.SetTrigger("Hide");

        if (active && ShapesManager._instance.shapeMenuActive)
            ShapesManager._instance.ToggleShapeMenu();

        if (active && MapManager._instance.modeScrollPanel.activeSelf)
            MapManager._instance.ToggleToolScroll();

        MapManager._instance.EnableControls(!active);
        colorMenuActive = active;
    }

    public void ChooseColor(int colorIndex)
    {
        if (Input.GetMouseButtonDown(0))
        {
            ChangeColor(colors[colorIndex].color, true);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ChangeColor(colors[colorIndex].color, false);
        }
    }

    public void ChangeColor(Color color, bool primaryColor)
    {
        if (primaryColor)
        {
            mainColor = color;
            mainColorIcon.color = color;
        }
        else
        {
            secondaryColor = color;
            secondaryColorIcon.color = color;
        }
    }

    public void ChoosePaintTool(int toolIndex)
    {
        currentColorTool = (ColorTool)toolIndex;

        if(currentColorTool == ColorTool.tileMarker || currentColorTool == ColorTool.eraserMarker)
        {
            if (!MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(true);

            markerIsErasing = currentColorTool == ColorTool.eraserMarker;

        }
        else
        {
            if (MapManager._instance.pointerActive)
                MapManager._instance.ActivatePointer(false);
        }

        clearAllMarkerButton.SetActive(markerIsErasing);

        for(int i = 0; i < 3; i++)
        {
            Image button = paintObjectButton;
            if (i == 1)
                button = markerTileButton;
            else if(i == 2)
                button = eraseMarkerButton;

            button.GetComponent<Image>().color = (i == toolIndex) ? colorButtonSelected : colorButtonUnselected;
            button.transform.GetChild(0).GetComponent<Image>().color = (i == toolIndex) ? iconButtonSelected : iconButtonUnselected;
            button.transform.GetChild(1).GetComponent<Image>().color = (i == toolIndex) ? iconButtonSelected : iconButtonUnselected;
        }
    }

    public void ForceUpdateColorMarkers(bool terrainUpdate)
    {
        if (markerToTerrain != terrainUpdate)
            return;

        UpdateMarkerTerrainAppearance(markerToTerrain, true);
    }

    public void UpdateMarkerTerrainAppearance(bool followMesh, bool forced = false)
    {
        bool wholeMarkerUpdate = markerToTerrain != followMesh;
        markerToTerrain = followMesh;

        if (forced)
            wholeMarkerUpdate = true;

        //update
        if (wholeMarkerUpdate)
        {
            if (coloredTiles == null)
                return;

            for(int j = 0; j < coloredTiles.GetLength(1); j++)
            {
                for(int i = 0; i < coloredTiles.GetLength(0); i++)
                {
                    Transform nuMarkerTileObj = coloredTiles[i, j];

                    if (nuMarkerTileObj == null)
                        continue;

                    Vector2 coordinates = new Vector2(i, j);

                    if (markerToTerrain)
                    {
                        nuMarkerTileObj.position = Vector3.zero;
                        nuMarkerTileObj.rotation = Quaternion.identity;

                        Vector3[] borderHeightSet = MapManager._instance.mapTarget.GetCellBorderHeights(coordinates);
                        Vector3[] tileVertexes = new Vector3[(int)borderHeightSet.Length];
                        for (int b = 0; b < borderHeightSet.Length; b++)
                        {
                            tileVertexes[b] = borderHeightSet[b] + (4f * markerAdditionalHeight * Vector3.up);
                        }

                        MeshFilter mf = nuMarkerTileObj.GetComponent<MeshFilter>();
                        Mesh tileMesh = mf.mesh;
                        tileMesh.SetVertices(tileVertexes);
                        int[] tileTris = new int[6]
                        {
                            0,2,1,2,3,1
                        };
                        tileMesh.SetTriangles(tileTris, 0);
                        tileMesh.RecalculateNormals();
                    }
                    else
                    {
                        Color oldColor = nuMarkerTileObj.GetComponent<MeshRenderer>().material.color;

                        coloredTiles[i, j] = null;
                        Destroy(nuMarkerTileObj.gameObject);

                        GameObject nuMarkerTile = Instantiate<GameObject>(markerTile, transform);
                        nuMarkerTileObj = nuMarkerTile.transform;

                        nuMarkerTileObj.position = MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(coordinates) + (markerAdditionalHeight * Vector3.up);
                        nuMarkerTileObj.rotation = Quaternion.Euler(90, 0f, 0f);

                        MeshRenderer mr = nuMarkerTileObj.GetComponent<MeshRenderer>();
                        mr.material.color = oldColor;

                        coloredTiles[i, j] = nuMarkerTileObj;

                        nuMarkerTile.SetActive(true);
                    }
                }
            }
        }
    }

    public void SetColorMarkerVisibility(bool visible)
    {
        if (coloredTiles == null)
            return;

        for (int j = 0; j < coloredTiles.GetLength(1); j++)
        {
            for (int i = 0; i < coloredTiles.GetLength(0); i++)
            {
                if (coloredTiles[i, j] == null)
                    continue;

                coloredTiles[i, j].gameObject.SetActive(visible);
            }
        }
    }

    public void ShowGeneralColorPanel(Vector3 listPosition)
    {
        generalColorList.BuildPanel(listPosition, colors, false);
        generalColorList.ShowPanel(true);
    }

    public void HideGeneralColorPanel()
    {
        generalColorList.ShowPanel(false);
    }

    public string GetColorName(Color entry)
    {
        for(int i = 0; i < colors.Length; i++)
        {
            if (colors[i].color.Equals(entry))
                return colors[i].name;
        }
        
        return "";
    }

    public Color GetNameColor(string colorName)
    {
        for(int i = 0; i < colors.Length; i++)
        {
            if (colors[i].name.Equals(colorName))
                return colors[i].color;
        }

        return new Color();
    }
}
