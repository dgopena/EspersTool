using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Collections;
using TMPro;

public class BattleMap : MonoBehaviour
{
    public float cellScale = 1f;

    private int cellCountX;
    private int cellCountZ;

    public int maxCellHeight = 4; //3 you can reach
    public float tileWorldHeight = 0.5f;

    private int[,] heightMap;

    private Vector3 mapOrigin;

    public TerrainTools currentTool { get; private set; }

    [Header("Map Utilities")]
    public bool makeGridLines = true;
    public GameObject gridLinePrefab;
    public bool gridLine3D;
    public bool gridMade = false;
    public bool gridActive { get; private set; }

    [Space(10f)]
    public GameObject tileNumberPrefab;
    public Transform tileNumberParent;
    private List<GameObject> tileNumerationObjects;

    public Vector3 tileNumberPosMod = new Vector3(-0.9f, 0.1f, 0.9f);
    public float numberFontSize = 4;

    private float tileMeshZFightFactor = 0.045f;
    private bool tileNumberingActive = false;
    private bool lastTilingWasMesh = false;

    [Space(10f)]
    public GameObject terrainTilePrefab;
    public GameObject negativeTerrainTilePrefab;

    public BoundSet bounds { get; private set; }

    public bool showGizmos = true;

    /*
    public GameObject vertexHelpPrefab;
    private Transform vertexHelpParent;
    public bool vertexHelpActive { get; private set; }
    */

    public Slider morphRadiusSlider;
    public float ringSizeFactor = 2f;

    public Slider morphCurveSlider;

    private MeshComplexParallel meshParallelModifier;

    [Header("Terrain Variables")]
    public LayerMask terrainLayer;
    public LayerMask floorOnlyLayer;
    public LayerMask outerEdgeLayer;
    public LayerMask terrainSetLayers;
    private int lastTileX = int.MinValue;
    private int lastTileZ = int.MinValue;

    public float morphingStrength = 1.4f;
    public float maxMorphHeight { get; private set; }
    public float minMorphHeight { get; private set; }

    private bool beingMorphedFlag = false;
    private bool wasMorphed = false;

    private Mesh floorMesh;

    public Color mapColor { get; private set; }
    private bool checkeredColor;
    private bool heightColor;

    public float blackenFactor = 0.5f;
    public float whitenFactor = 2f;
    public float startWhite = 0.4f;

    private Transform floorObj;
    public Material floorMaterial; //we need to change this variable and update the renderer in the future case we want to implement floor material changing
    public Material floorGridMaterial;
    private MeshFilter floorMF;
    private MeshRenderer floorMR;
    private MeshCollider floorMC;
    public GameObject terrainModHelper;
    private Transform arrowIndicator;
    private Transform ringIndicator;
    private Transform glowIndicator;
    public Color morphPlusColor = Color.red;
    public Color morphMinusColor = Color.blue;
    public Color morphIdleColor = Color.gray;

    private int mapTileFadedState = 0;
    private int mapMeshFadedState = 0;
    private bool objTestState = false;

    private float[,] floorMeshHeightMap;

    private bool glowFixedFlag = false;

    public float currentRadiusValue { get; private set; }
    public float currentCurveValue { get; private set; }
    private bool root = false;
    
    private List<TransformMatStoring> matStore;

    public struct TransformMatStoring
    {
        public Transform element;
        public Material storeMat;
    }

    public enum TerrainTools
    {
        TileModifying,
        TerrainMorphing,
        AutoTile,
        TileType
    }

    private int[,] typeMap;

    [SerializeField]
    private TeerrainTypeDisplay[] tileTypes;
    public GameObject typeTilePrefab;
    public float typeTileAddHeight = 0.065f;

    private int currentTypeChosen = 0;

    public enum TerrainTypes
    {
        Ground,
        Difficult,
        Dangerous,
        Impassable,
        Destructible
    }

    [System.Serializable]
    private struct TeerrainTypeDisplay
    {
        public TerrainTypes type;
        public Sprite icon;
        public Color color;
    }

    public struct BoundSet
    {
        public float minX;
        public float maxX;
        public float minZ;
        public float maxZ;
        public float floorY;
        public float ceilY;
    }

    public void BuildMap(int sizeX, int sizeZ, bool freshMap = false)
    {
        bool autoBuildFloorMesh = true;
        bool clearFloorMesh = true; 
        bool startFreshTileMap = true;

        if (!freshMap)
        {
            autoBuildFloorMesh = false;
            clearFloorMesh = false;
            startFreshTileMap = false;
        }

        heightMap = new int[sizeX, sizeZ];
        typeMap = new int[sizeX, sizeZ];

        transform.localScale = cellScale * new Vector3(0.1f * sizeX, 1f, 0.1f * sizeZ);

        float xCood = -0.5f * sizeX * cellScale;
        float zCood = -0.5f * sizeZ * cellScale;

        mapOrigin = new Vector3(xCood, MapManager._instance.floorHeight, zCood);

        BoundSet aux = new BoundSet();
        aux.minX = mapOrigin.x;
        aux.minZ = mapOrigin.z;
        aux.maxX = mapOrigin.x + (cellScale * sizeX);
        aux.maxZ = mapOrigin.z + (cellScale * sizeZ);
        aux.floorY = MapManager._instance.floorHeight;
        aux.ceilY = MapManager._instance.ceilingHeight;

        bounds = aux;

        arrowIndicator = terrainModHelper.transform.GetChild(0);
        ringIndicator = terrainModHelper.transform.GetChild(1);
        glowIndicator = terrainModHelper.transform.GetChild(2).GetChild(0);

        maxMorphHeight = cellScale * ((0.5f * (3)) + 0.05f);
        minMorphHeight = cellScale * (-0.6f);
        morphRadiusSlider.normalizedValue = 0.35f;
        root = false;
        morphCurveSlider.normalizedValue = 0.5f;
        UpdateRadiusSlider();
        UpdateCurveSlider();

        RebuildGrid();

        /*
        if (makeGridLines)
            MakeGrid();
            */

        cellCountX = sizeX;
        cellCountZ = sizeZ;

        FixHelpGlowNormals();

        if (clearFloorMesh)
            ClearFloorMesh();

        if (autoBuildFloorMesh)
            BuildFloorMesh();

        if (startFreshTileMap)
        {
            int[] zeroTiles = new int[cellCountX * cellCountZ];
            LoadTileMap(zeroTiles);
        }

        SetSavedGridVisibility();

        Transform edgeObj = MapManager._instance.terrainEdgePlane;
        float edgeScaX = MapManager._instance.terrainEdgeSizeFactor * (bounds.maxX - bounds.minX);
        float edgeScaZ = MapManager._instance.terrainEdgeSizeFactor * (bounds.maxZ - bounds.minZ);

        edgeObj.localScale = new Vector3(edgeScaX, 0.1f, edgeScaZ);

        MapManager._instance.ChangeTileStyle(PlayerPrefs.GetInt("TileStyle", 1), false);

        if (freshMap)
        {
            bool colorCheckered = PlayerPrefs.GetInt("CheckeredColor", 0) == 1;
            bool colorHeight = PlayerPrefs.GetInt("HeightColor", 0) == 1;

            Color startColor = Color.grey;
            SetMapColor(startColor, colorCheckered, colorHeight);

            UpdateMapModeLook();

            MapManager._instance.mapModeController.UpdateColorPanelUI();
        }

        MapManager._instance.UpdateCameraBounds();
        ColorManager._instance.BuildMarkerArrayCall();
    }
    
    public void ClearFloorMesh()
    {
        float[] yMap = new float[((MapManager._instance.terrainMeshDetail * heightMap.GetLength(1)) + 1) * ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1)];
        floorMeshHeightMap = ArrayToYMap(yMap);
        BuildFloorMesh(floorMeshHeightMap);
    }

    private void BuildFloorMesh()
    {
        if (floorMesh != null)
            return;

        float[] yMap = new float[((MapManager._instance.terrainMeshDetail * heightMap.GetLength(1)) + 1) * ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1)];
        floorMeshHeightMap = ArrayToYMap(yMap);
        BuildFloorMesh(floorMeshHeightMap);
    }

    private void BuildFloorMesh(float[,] yMap)
    {
        if (floorMesh == null)
        {
            floorMesh = new Mesh();

            GameObject nuFloor = new GameObject();
            nuFloor.transform.position = Vector3.zero;
            nuFloor.transform.parent = transform;
            nuFloor.name = "Floor Mesh";

            floorObj = nuFloor.transform;

            floorMF = floorObj.gameObject.AddComponent<MeshFilter>();
            floorMR = floorObj.gameObject.AddComponent<MeshRenderer>();
            meshParallelModifier = floorObj.gameObject.AddComponent<MeshComplexParallel>();
        }

        floorMesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int j = 0; j < ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(1)) + 1); j++)
        {
            for(int i = 0; i < ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1); i++)
            {
                Vector3 point = mapOrigin + (i * (cellScale / (float)MapManager._instance.terrainMeshDetail) * Vector3.right) + (j * (cellScale / (float)MapManager._instance.terrainMeshDetail) * Vector3.forward) + (yMap[i,j] * Vector3.up);
                vertices.Add(point);

                Vector2 uv = new Vector2(i, j);
                uvs.Add(uv);
            }
        }

        List<int> tris = new List<int>();
        for (int j = 0; j < (MapManager._instance.terrainMeshDetail * heightMap.GetLength(1)); j++)
        {
            for (int i = 0; i < (MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)); i++)
            {
                int v0 = i + (((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1) * j);
                int v1 = (v0 + 1);
                int v2 = v0 + ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1);
                int v3 = v1 + ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1);

                tris.Add(v0); tris.Add(v3); tris.Add(v1);
                tris.Add(v0); tris.Add(v2); tris.Add(v3);
            }
        }

        floorMesh.SetVertices(vertices);
        floorMesh.SetTriangles(tris, 0);
        floorMesh.SetUVs(0, uvs);

        floorMesh.name = "MapFloor";

        floorMesh.RecalculateNormals();
        floorMesh.RecalculateBounds();

        floorMF.mesh = floorMesh;
        floorMR.material = floorMaterial;

        meshParallelModifier.CleanNativeArrays();
        meshParallelModifier.Set(bounds);


        ResetFloorMeshScale();

        MakeBorderHeightSet();
    }

    public void ResetFloorMeshScale()
    {
        floorObj.localScale = new Vector3((1f / floorObj.parent.localScale.x), (1f / floorObj.parent.localScale.y), (1f / floorObj.parent.localScale.z));
    }

    #region Map Colors

    public enum MapLookState
    {
        Default,
        TerrainEdit,
        TerrainMorph,
    }

    private MapLookState currentLook;

    public void SetMapLook(MapLookState mode)
    {
        currentLook = mode;

        UpdateMapModeLook();
    }

    public void UpdateMapModeLook()
    {
        if(currentLook == MapLookState.Default)
        {
            if(MapManager._instance.mapModeController.CurrentMode == 0) //tile
            {
                SetMapAlphaState(2, 0);
                SetObjectsMat(false);
            }
            else //mesh
            {
                SetMapAlphaState(0, 2);
                SetObjectsMat(false);
            }
        }
        else if(currentLook == MapLookState.TerrainEdit)
        {
            if (MapManager._instance.mapModeController.CurrentMode == 0) //tile
            {
                SetMapAlphaState(2, 0);
                SetObjectsMat(true);
            }
            else //mesh
            {
                SetMapAlphaState(1, 2);
                SetObjectsMat(true);
            }
        }
        else if(currentLook == MapLookState.TerrainMorph)
        {
            SetMapAlphaState(0, 2);
            SetObjectsMat(true);
        }

        ApplyMapColor();

        if (tileNumberingActive)
        {
            //SetCellNumberingVisibility(true, true);
        }

        ColorManager._instance.UpdateMarkerTerrainAppearance(MapManager._instance.mapModeController.CurrentMode != 0, true);
    }

    public void SetMapColor(Color mapColor, bool checkered, bool heighted, bool callUpdate = true)
    {
        this.mapColor = mapColor;
        checkeredColor = checkered;
        heightColor = heighted;
        
        PlayerPrefs.SetInt("CheckeredColor", checkered ? 1 : 0);
        PlayerPrefs.SetInt("HeightColor", heighted ? 1 : 0);

        if(callUpdate)
            UpdateMapModeLook();
    }

    public void SetCheckered(bool checkered)
    {
        PlayerPrefs.SetInt("CheckeredColor", checkered ? 1 : 0);
        SetMapColor(mapColor, checkered, heightColor);
    }

    public void SetHeightColor(bool heighted)
    {
        PlayerPrefs.SetInt("HeightColor", heighted ? 1 : 0);
        SetMapColor(mapColor, checkeredColor, heighted);
    }

    public void SetMapAlphaState(int tileFadeState, int meshFadeState) //0 - turnedOff, 1 - faded, 2 - solid
    {
        mapTileFadedState = tileFadeState;
        mapMeshFadedState = meshFadeState;

        /*
        Material tileMat = faded ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
        Material meshMat = faded ? MapManager._instance.mapTestMaterialFaded : MapManager._instance.mapTestMaterial;

        float baseHeight = (MapManager._instance.floorHeight - MapManager._instance.mapTarget.tileWorldHeight);
        float heightTotal = (MapManager._instance.mapTarget.maxCellHeight + 1) * MapManager._instance.mapTarget.tileWorldHeight;

        if (keepColor)
        {
            float baseAlpha = tileMat.color.a;
            Color baseColor = mapColor;
            baseColor.a = baseAlpha;
            tileMat.color = baseColor;

            baseAlpha = meshMat.color.a;
            baseColor.a = baseAlpha;
            meshMat.color = baseColor;
        }

        if (target == 1 || target == 0)
        {
            for (int i = 0; i < transform.GetChild(0).childCount; i++)
            {
                Material modMat = tileMat;
                if((checkeredColor || heightColor) && MapManager._instance.mapModeController.CurrentMode == 0)
                {
                    string tileName = transform.GetChild(0).GetChild(i).name;
                    int x = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
                    tileName = tileName.Substring(tileName.IndexOf('|') + 1);
                    int z = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));

                    bool darkened = ((x % 2 == 0) && (z % 2 == 0)) || ((x % 2 != 0) && (z % 2 != 0));


                    Color matColor = modMat.color;

                    if (checkeredColor)
                    {
                        if (darkened)
                        {
                           matColor = blackenFactor * matColor;
                        }
                    }

                    if (heightColor)
                    {
                        float frac = (transform.GetChild(0).GetChild(i).position.y - baseHeight) / heightTotal;
                        float mult = Mathf.Lerp(startWhite, whitenFactor, frac);
                        matColor = mult * matColor;
                    }

                    modMat.color = matColor;
                }

                MeshRenderer tileMR = transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>();
                
                if(tileMR != null)
                    tileMR.material = tileMat;
                else //pit tile
                {
                    for(int c = 0; c < transform.GetChild(0).GetChild(i).childCount; c++)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(c).GetComponent<MeshRenderer>().material = modMat;
                    }
                }
            }

            for(int i = 0; i < transform.GetChild(2).childCount; i++)
            {
                Material modMat = tileMat;
                if ((checkeredColor || heightColor) && MapManager._instance.mapModeController.CurrentMode == 0)
                {
                    string tileName = transform.GetChild(0).GetChild(i).name;
                    int x = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
                    tileName = tileName.Substring(tileName.IndexOf('|') + 1);
                    int z = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));

                    bool darkened = ((x % 2 == 0) && (z % 2 == 0)) || ((x % 2 != 0) && (z % 2 != 0));


                    Color matColor = modMat.color;

                    if (checkeredColor)
                    {
                        if (darkened)
                        {
                            matColor = blackenFactor * matColor;
                        }
                    }

                    if (heightColor)
                    {
                        float frac = (transform.GetChild(0).GetChild(i).position.y - baseHeight) / heightTotal;
                        float mult = Mathf.Lerp(startWhite, whitenFactor, frac);
                        matColor = mult * matColor;
                    }

                    modMat.color = matColor;
                }

                MeshRenderer typeMR = transform.GetChild(2).GetChild(i).GetComponent<MeshRenderer>();

                Color aux = mapColor; // typeMR.material.color;
                typeMR.material = modMat;
                aux.a = faded ? 0.4f : 1f;
                typeMR.material.color = aux;

                SpriteRenderer typeSR = transform.GetChild(2).GetChild(i).GetChild(0).GetComponent<SpriteRenderer>();
                aux = typeSR.color;
                aux.a = faded ? 0.4f : 1f;
                typeSR.color = aux;
            }
        }
        else if(target == 2 || target == 0)
        {
            floorMR = floorObj.GetComponent<MeshRenderer>();
            floorMR.material = meshMat;
            floorMR.material.color = mapColor;

            Transform objCont = ShapesManager._instance.GetObjectContainer();
            if (objCont == null)
                return;
            
            for (int i = 0; i < objCont.childCount; i++)
            {
                Transform obj = objCont.GetChild(i);
                MeshRenderer mr = obj.GetChild(0).GetComponent<MeshRenderer>();

                mr.material = faded ? MapManager._instance.shapeTestMaterialFaded : MapManager._instance.shapeTestMaterial;
            }
        }
        */
    }

    public void ApplyMapColor()
    {
        //if map mode is tile, all tiles are applied the color and styles
        //if mesh mode, all the tiles are white, and the mesh is colored

        ApplyColorToMesh();

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            ApplyColorToTile(transform.GetChild(0).GetChild(i));
        }

        #region Old Color Assign
        /*
        float baseHeight = (MapManager._instance.floorHeight - MapManager._instance.mapTarget.tileWorldHeight);
        float heightTotal = (MapManager._instance.mapTarget.maxCellHeight + 1) * MapManager._instance.mapTarget.tileWorldHeight;

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Transform child = transform.GetChild(0).GetChild(i);
            MeshRenderer childMR = child.GetComponent<MeshRenderer>();

            string tileName = child.name;
            int x = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
            tileName = tileName.Substring(tileName.IndexOf('|') + 1);
            int z = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));

            Color baseColor = mapColor;
            if (MapManager._instance.mapModeController.CurrentMode == 1)
            {
                baseColor = Color.white;
            }

            bool darkened = ((x % 2 == 0) && (z % 2 == 0)) || ((x % 2 != 0) && (z % 2 != 0));

            if (childMR == null) //pit
            {
                for (int j = 0; j < transform.GetChild(0).GetChild(i).childCount; j++)
                {
                    childMR = transform.GetChild(0).GetChild(i).GetChild(j).GetComponent<MeshRenderer>();
                    baseColor = mapColor;
                    if (MapManager._instance.mapModeController.CurrentMode == 1)
                    {
                        baseColor = Color.white;
                    }

                    if (MapManager._instance.mapModeController.CurrentMode == 0)
                    {
                        if (checkeredColor)
                        {
                            if (darkened)
                            {
                                baseColor = blackenFactor * baseColor;
                            }
                        }

                        if (heightColor)
                        {
                            float frac = (child.position.y - baseHeight) / heightTotal;
                            float mult = Mathf.Lerp(startWhite, whitenFactor, frac);
                            baseColor = mult * baseColor;
                        }
                    }

                    childMR.material.color = baseColor;
                }
            }
            else
            {
                if (MapManager._instance.mapModeController.CurrentMode == 0)
                {
                    if (checkeredColor)
                    {
                        if (darkened) //need to make this in base of coordinates, not index. need to check name, then
                        {
                            baseColor = blackenFactor * baseColor;
                        }
                    }

                    if (heightColor)
                    {
                        float frac = (child.position.y - baseHeight) / heightTotal;
                        float mult = Mathf.Lerp(startWhite, whitenFactor, frac);

                        baseColor = mult * baseColor;
                    }
                }

                childMR.material.color = baseColor;
            }
        }
        */
        #endregion
    }

    public void ApplyColorToMesh()
    {
        if (floorMR == null)
            return;

        floorMR.enabled = MapManager._instance.mapModeController.CurrentMode == 1;
        if (MapManager._instance.mapModeController.CurrentMode == 0)
        {
            return;
        }

        gridActive = PlayerPrefs.GetInt("GridVisible", 0) == 1;

        floorObj.gameObject.SetActive(mapMeshFadedState > 0);
        if (mapMeshFadedState > 0)
        {
            Material meshMat = gridActive ? floorGridMaterial : floorMaterial;
            if (currentLook == MapLookState.TerrainMorph)
                meshMat = mapMeshFadedState == 1 ? MapManager._instance.mapTestMaterialFaded : MapManager._instance.mapTestMaterial;

            Color matColor = mapColor;
            matColor.a = meshMat.color.a;

            floorMR.material = meshMat;
            floorMR.material.color = matColor;
        }
    }

    public void ApplyColorToTile(Transform tileObj)
    {
        float baseHeight = (MapManager._instance.floorHeight - MapManager._instance.mapTarget.tileWorldHeight);
        float heightTotal = (MapManager._instance.mapTarget.maxCellHeight + 1) * MapManager._instance.mapTarget.tileWorldHeight;

        MeshRenderer tileMR = tileObj.GetComponent<MeshRenderer>();

        Material tileMaterial = MapManager._instance.tileMaterial;

        if (currentLook == MapLookState.Default)
        {
            if (MapManager._instance.mapModeController.CurrentMode == 1)
            {
                tileObj.gameObject.SetActive(MapManager._instance.TileStyle != 4); //4 is invisible tiles

                if (MapManager._instance.TileStyle < 4)
                    tileMaterial = MapManager._instance.tileStyles[MapManager._instance.TileStyle].styleMat;
            }
            else
            {
                tileMaterial = MapManager._instance.tileStyles[0].styleMat;
                tileObj.gameObject.SetActive(true);
            }

        }
        else if (currentLook == MapLookState.TerrainMorph)
        {
            tileObj.gameObject.SetActive(false);
        }
        else
        {
            tileObj.gameObject.SetActive(true);

            tileMaterial = MapManager._instance.tileStyles[0].styleMat;
        }

        Material tileMat = (mapTileFadedState == 1) ? MapManager._instance.tileMaterialFaded : tileMaterial;

        float matAlpha = tileMat.color.a;

        string tileName = tileObj.name;
        int x = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
        tileName = tileName.Substring(tileName.IndexOf('|') + 1);
        int z = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));

        bool darkened = ((x % 2 == 0) && (z % 2 == 0)) || ((x % 2 != 0) && (z % 2 != 0));

        Color tileColor = mapColor;
        if (MapManager._instance.mapModeController.CurrentMode == 1)
        {
            tileColor = Color.white;
        }
        else
        {
            if (checkeredColor)
            {
                if (darkened)
                {
                    tileColor = blackenFactor * tileColor;
                }
            }

            if (heightColor)
            {
                float frac = (tileObj.position.y - baseHeight) / heightTotal;
                float mult = Mathf.Lerp(startWhite, whitenFactor, frac);
                tileColor = mult * tileColor;
            }
        }

        if (tileMR == null) //pit
        {
            for (int j = 0; j < tileObj.childCount; j++)
            {
                tileMR = tileObj.GetChild(j).GetComponent<MeshRenderer>();

                tileMR.material = tileMat;
                tileColor.a = matAlpha;
                tileMR.material.color = tileColor;
            }
        }
        else
        {
            tileMR.material = tileMat;
            tileColor.a = matAlpha;
            tileMR.material.color = tileColor;
        }
    }

    public void SetObjectsMat(bool test)
    {
        if (test && !objTestState)
        {
            floorMR = floorObj.GetComponent<MeshRenderer>();

            floorMR.material = MapManager._instance.mapTestMaterial;

            Transform objCont = ShapesManager._instance.GetObjectContainer();
            if (objCont == null)
                return;

            matStore = new List<TransformMatStoring>();
            for (int i = 0; i < objCont.childCount; i++)
            {
                Transform obj = objCont.GetChild(i);
                MeshRenderer mr = obj.GetChild(0).GetComponent<MeshRenderer>();

                TransformMatStoring tms = new TransformMatStoring();
                tms.element = obj;
                tms.storeMat = mr.material;

                matStore.Add(tms);

                mr.material = MapManager._instance.shapeTestMaterialFaded;
            }

            objTestState = true;
        }
        else if(!test && objTestState)
        {
            floorMR = floorObj.GetComponent<MeshRenderer>();

            SetSavedGridVisibility();

            Transform objCont = ShapesManager._instance.GetObjectContainer();
            if (objCont == null)
                return;

            if (matStore == null)
                matStore = new List<TransformMatStoring>();

            for (int i = 0; i < matStore.Count; i++)
            {
                try
                {
                    TransformMatStoring tms = matStore[i];
                    tms.element.GetChild(0).GetComponent<MeshRenderer>().material = tms.storeMat;
                }
                catch (System.Exception e)
                {

                }
            }

            objTestState = false;
        }
    }

    #endregion

    //make floor mesh based on float array
    public void LoadFloorMesh(int sizeX, int sizeZ, float cellScale, int meshDetail, float[] yMap)
    {
        this.cellScale = cellScale;

        MapManager._instance.cellCountX = sizeX;
        MapManager._instance.cellCountZ = sizeZ;

        BuildMap(sizeX, sizeZ, false);
        RebuildGrid();
        MapManager._instance.terrainMeshDetail = meshDetail;
        floorMeshHeightMap = ArrayToYMap(yMap);
        BuildFloorMesh(floorMeshHeightMap);
        SetSavedGridVisibility();
    }

    //make tile matrix from the load file
    public void LoadTileMap(int[] tileArr)
    {
        heightMap = ArrayToMapMatrix(tileArr);

        for (int i = transform.GetChild(0).childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(0).GetChild(i).gameObject);
        }

        for (int j = 0; j < heightMap.GetLength(1); j++)
        {
            for (int i = 0; i < heightMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                float posY = bounds.ceilY;
                float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                if (heightMap[i, j] < 0)
                {
                    float tryHeight = transform.position.y + (cellScale * ((-0.25f)));
                    
                    GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                    nuTile.name = coodX + "|" + coodZ + "|" + ("pit");
                    posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                    posY = tryHeight + tileMeshZFightFactor;
                    posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                    nuTile.transform.position = new Vector3(posX, posY, posZ);
                    nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                    nuTile.transform.parent = transform.GetChild(0);

                    /*
                    if (mapAlphaState == 1 | mapAlphaState == 0)
                    {
                        for (int c = 0; c < nuTile.transform.childCount; c++)
                        {
                            nuTile.transform.GetChild(c).GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                        }
                    }
                    */
                }
                else
                {
                    if (heightMap[i, j] == 0)
                    {
                        float tryHeight = transform.position.y + (cellScale * ((0.25f)));

                        GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|0";
                        posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        posY = tryHeight + tileMeshZFightFactor;
                        posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        for (int t = 0; t < 4; t++)
                        {
                            nuTile.transform.GetChild(t).gameObject.SetActive(false);
                        }

                        nuTile.transform.parent = transform.GetChild(0);
                    }
                    else
                    {
                        for (int t = -1; t < maxCellHeight; t++)
                        {
                            if (t >= heightMap[i, j])
                                break;

                            float tryHeight = transform.position.y + (cellScale * ((0.5f * t + 0.249f)));

                            GameObject nuTile = Instantiate<GameObject>(terrainTilePrefab);
                            nuTile.name = coodX + "|" + coodZ + "|" + (t + 1);
                            posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                            posY = tryHeight + tileMeshZFightFactor;
                            posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                            nuTile.transform.position = new Vector3(posX, posY, posZ);
                            nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                            nuTile.transform.parent = transform.GetChild(0);

                            /*
                            if (mapAlphaState == 1 || mapAlphaState == 0)
                            {
                                nuTile.GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                            }
                            */

                            if (t == (maxCellHeight - 1))
                            {
                                nuTile.transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        PieceManager._instance.AdaptToTileHeight();

        UpdateAllPitTiles();

        GameModeManager._instance.SetTileNumerationUpdateFlag();
        ColorManager._instance.ForceUpdateColorMarkers(false);

        /*
        if(MapManager._instance.toolMode == MapManager.ToolMode.TerrainTool)
        {
            SetObjectsMat(true);
            MapManager._instance.ChangeTerrainTool((int)TerrainTools.TileModifying);
        }
        else
        {
            ShowTiles(false);
        }
        */
    }

    //make type matrix from the load file
    public void LoadTypeMap(int[] typeArr)
    {
        typeMap = ArrayToMapMatrix(typeArr);

        for (int i = transform.GetChild(2).childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(2).GetChild(i).gameObject);
        }

        for (int j = 0; j < typeMap.GetLength(1); j++)
        {
            for (int i = 0; i < typeMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                int typeToCast = typeMap[i, j];
                if (typeToCast == 0)
                    continue;

                GameObject nuTile = Instantiate<GameObject>(typeTilePrefab);
                nuTile.name = coodX + "|" + coodZ;
                float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                float posY = transform.position.y + (cellScale * ((0.5f * heightMap[coodX, coodZ]) + typeTileAddHeight));
                float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                nuTile.transform.position = new Vector3(posX, posY, posZ);
                nuTile.transform.localScale = new Vector3(cellScale, cellScale, 0.5f * cellScale);

                MeshRenderer mr = nuTile.GetComponent<MeshRenderer>();
                mr.material.color = tileTypes[typeToCast].color;
                nuTile.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tileTypes[typeToCast].icon;
                nuTile.transform.GetChild(0).GetComponent<SpriteRenderer>().color = tileTypes[typeToCast].color;
                typeMap[coodX, coodZ] = typeToCast;

                nuTile.transform.parent = transform.GetChild(2);
            }
        }

        if (MapManager._instance.toolMode == MapManager.ToolMode.TerrainEditor)
        {
            SetObjectsMat(true);
            MapManager._instance.ChangeTerrainTool((int)TerrainTools.TileModifying);

            ShowTerrainTypes(true);
        }
        else
        {
            ShowTerrainTypes(MapManager._instance.showTileTypes);
        }
    }

    #region old mesh helper code
    /*
    private void SetMeshHelp()
    {
        if (vertexHelpParent != null)
        {
            Vector3[] vertices = floorMesh.vertices;
            for (int v = 0; v < floorMesh.vertexCount; v++)
            {
                Transform ch = vertexHelpParent.GetChild(v);
                ch.position = vertices[v];
            }

            vertexHelpParent.gameObject.SetActive(true);
        }
        else
        {
            GameObject nuHelpParent = new GameObject();
            nuHelpParent.name = "Terrain Modifying Helper";
            vertexHelpParent = nuHelpParent.transform;

            Vector3[] vertices = floorMesh.vertices;
            for (int v = 0; v < floorMesh.vertexCount; v++)
            {
                GameObject nuHelp = Instantiate<GameObject>(vertexHelpPrefab);
                nuHelp.transform.position = vertices[v];

                nuHelp.transform.parent = vertexHelpParent;
            }
        }
    }
    */
    #endregion

    public void ModifyFloorMesh()
    {
        /*
        if (!vertexHelpActive)
            return;
            */

        Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100f, floorOnlyLayer))
        {
            terrainModHelper.SetActive(true);
            terrainModHelper.transform.position = hitInfo.point;

            if (Input.GetMouseButtonDown(0))
            {
                SetTerrainMorphToolState(1);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                SetTerrainMorphToolState(2);
            }
            else if (Input.GetMouseButton(0))
            {
                MorphInPoint(hitInfo.point, 1f);
                beingMorphedFlag = true;
                wasMorphed = true;
            }
            else if (Input.GetMouseButton(1))
            {
                MorphInPoint(hitInfo.point, -1f);
                beingMorphedFlag = true;
                wasMorphed = true;
            }
            else if(!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                if (beingMorphedFlag)
                {
                    SetTerrainMorphToolState(0);
                    floorMesh = floorMF.mesh;
                    beingMorphedFlag = false;
                }
            }
        }
        else
        {
            terrainModHelper.SetActive(false);
        }
    }

    public int[,] GetHeightMap()
    {
        return heightMap;
    }

    public int[,] GetTypeMap()
    {
        return typeMap;
    }

    private void MorphInPoint(Vector3 point, float direction)
    {
        if (meshParallelModifier != null)
        {
            float exp = currentCurveValue + 1;
            if (currentCurveValue < 0)
            {
                exp = currentCurveValue - 1;
                exp = Mathf.Abs(exp);
            }

            exp = (currentCurveValue < 0) ? (1f / exp) : exp;
            
            meshParallelModifier.ReceiveJob(point, currentRadiusValue, exp, morphingStrength, direction);
        }
    }

    private void MakeGrid()
    {
        return;

        float lengthMult = gridLine3D ? 1f : 0.39f;
        float lineWidth = gridLine3D ? 0.05f : 0.025f;

        float posStart = -0.5f * cellScale * heightMap.GetLength(1);
        for(int i = 1; i < heightMap.GetLength(1); i++)
        {
            GameObject nuLine = GameObject.Instantiate(gridLinePrefab);
            nuLine.name = "x[" + i.ToString() + "]";
            nuLine.transform.localPosition = (0.05f * Vector3.up) + ((posStart + (cellScale * i)) * Vector3.forward);
            nuLine.transform.localScale = cellScale * new Vector3(heightMap.GetLength(0) * lengthMult, lineWidth, 1f);
            nuLine.transform.parent = transform.GetChild(1);
        }

        posStart = -0.5f * cellScale * heightMap.GetLength(0);
        for (int i = 1; i < heightMap.GetLength(0); i++)
        {
            GameObject nuLine = GameObject.Instantiate(gridLinePrefab);
            nuLine.name = "z[" + i.ToString() + "]";
            nuLine.transform.localPosition = (0.05f * Vector3.up) + ((posStart + (cellScale * i)) * Vector3.right);
            nuLine.transform.localScale = cellScale * new Vector3(lineWidth, heightMap.GetLength(1) * lengthMult, 1f);
            nuLine.transform.parent = transform.GetChild(1);
        }

        SetSavedGridVisibility();

        gridMade = true;
    }

    public void RebuildGrid()
    {
        for(int i = transform.GetChild(1).childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(1).GetChild(i).gameObject);
        }

        gridMade = false;

        MakeGrid();
    }

    public void ModifyTiles(bool add, bool keyHoldEnabled = false)
    {
        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, 100f, terrainLayer))
        {
            if (hitInfo.transform.tag == "Tile")
            {
                string tileName = hitInfo.transform.name;

                int coodX = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
                tileName = tileName.Substring(tileName.IndexOf('|') + 1);
                int coodZ = int.Parse(tileName.Substring(0, tileName.IndexOf('|')));
                tileName = tileName.Substring(tileName.IndexOf('|') + 1);
                int coodY = int.Parse(tileName);

                if (coodX == lastTileX && coodZ == lastTileZ)
                    return;

                lastTileX = coodX;
                lastTileZ = coodZ;

                int curHeight = heightMap[coodX, coodZ];
                if (add) {
                    if (curHeight == maxCellHeight)
                        return;

                    heightMap[coodX, coodZ] = curHeight + 1;

                    GameObject nuTile = Instantiate<GameObject>(terrainTilePrefab);
                    nuTile.name = coodX + "|" + coodZ + "|" + heightMap[coodX, coodZ];
                    float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                    float posY = transform.position.y + (cellScale * ((0.5f * (heightMap[coodX, coodZ] - 1)) + 0.25f)) + tileMeshZFightFactor;
                    float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                    nuTile.transform.position = new Vector3(posX, posY, posZ);
                    nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                    nuTile.transform.parent = transform.GetChild(0);

                    /*
                    if (mapAlphaState == 1 || mapAlphaState == 0)
                    {
                        nuTile.GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                    }
                    */

                    if (curHeight + 1 == maxCellHeight)
                    {
                        nuTile.transform.GetChild(0).gameObject.SetActive(true);
                        UpdateTypeIndicators(coodX, coodZ, false);
                    }
                    else
                        UpdateTypeIndicators(coodX, coodZ);

                    ApplyColorToTile(nuTile.transform);
                }
                else
                {
                    int topTileHeight = heightMap[coodX, coodZ];
                    Transform topTile = transform.GetChild(0).Find(coodX + "|" + coodZ + "|" + topTileHeight);
                    Destroy(topTile.gameObject);
                    heightMap[coodX, coodZ] = topTileHeight - 1;

                    if(topTileHeight == 0)
                    {
                        float tryHeight = transform.position.y + (cellScale * ((-0.25f)));

                        GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|" + ("pit");
                        float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        float posY = transform.position.y + (cellScale * (-0.25f)) + tileMeshZFightFactor;
                        float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        nuTile.transform.parent = transform.GetChild(0);

                        UpdatePitTiles(coodX, coodZ);

                        ApplyColorToTile(nuTile.transform);
                    }

                    UpdateTypeIndicators(coodX, coodZ);
                }
            }
            else //raw terrain
            {
                float diff = hitInfo.point.x - mapOrigin.x;
                int coodX = Mathf.FloorToInt(diff / cellScale);
                diff = hitInfo.point.z - mapOrigin.z;
                int coodZ = Mathf.FloorToInt(diff / cellScale);

                if (coodX == lastTileX && coodZ == lastTileZ)
                    return;

                lastTileX = coodX;
                lastTileZ = coodZ;


                if (heightMap[coodX, coodZ] == 0)
                {
                    if (add)
                    {
                        heightMap[coodX, coodZ] = 1;

                        GameObject nuTile = Instantiate<GameObject>(terrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|" + heightMap[coodX, coodZ];
                        float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        float posY = transform.position.y + (cellScale * ((0.5f * (heightMap[coodX, coodZ] - 1)) + 0.25f)) + tileMeshZFightFactor;
                        float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        nuTile.transform.parent = transform.GetChild(0);

                        /*
                        if(mapAlphaState == 1 || mapAlphaState == 0)
                        {
                            nuTile.GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                        }
                        */

                        UpdateTypeIndicators(coodX, coodZ);

                        ApplyColorToTile(nuTile.transform);
                    }
                    else
                    {
                        Transform surfTile = transform.GetChild(0).Find(coodX + "|" + coodZ + "|" + 0);
                        if(surfTile != null)
                            Destroy(surfTile.gameObject);

                        heightMap[coodX, coodZ] = -1;

                        float tryHeight = transform.position.y + (cellScale * ((-0.25f)));

                        GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|" + ("pit");
                        float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        float posY = transform.position.y + (cellScale * (-0.25f)) + tileMeshZFightFactor;
                        float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        nuTile.transform.parent = transform.GetChild(0);

                        /*
                        if (mapAlphaState == 1 || mapAlphaState == 0)
                        {
                            for (int c = 0; c < nuTile.transform.childCount; c++)
                            {
                                nuTile.transform.GetChild(c).GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                            }
                        }
                        */

                        UpdateTypeIndicators(coodX, coodZ);

                        UpdatePitTiles(coodX, coodZ);

                        ApplyColorToTile(nuTile.transform);
                    }
                }
                else if(heightMap[coodX, coodZ] < 0)
                {
                    if (add)
                    {
                        Transform topTile = transform.GetChild(0).Find(coodX + "|" + coodZ + "|" + "pit");

                        Destroy(topTile.gameObject);
                        heightMap[coodX, coodZ] = 0;

                        GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|" + heightMap[coodX, coodZ];
                        float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        float posY = transform.position.y + (cellScale * 0.25f) + tileMeshZFightFactor;
                        float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        for (int t = 0; t < 4; t++)
                        {
                            nuTile.transform.GetChild(t).gameObject.SetActive(false);
                        }

                        nuTile.transform.parent = transform.GetChild(0);

                        UpdateTypeIndicators(coodX, coodZ);
                        UpdatePitTiles(coodX, coodZ);

                        ApplyColorToTile(nuTile.transform);
                    }
                }
            }

            PieceManager._instance.AdaptToTileHeight();
            GameModeManager._instance.SetTileNumerationUpdateFlag();
            ColorManager._instance.ForceUpdateColorMarkers(false);
        }

        
    }

    public void ModifyType()
    {
        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100f, terrainLayer))
        {
            float diff = hitInfo.point.x - mapOrigin.x;
            int coodX = Mathf.FloorToInt(diff / cellScale);
            diff = hitInfo.point.z - mapOrigin.z;
            int coodZ = Mathf.FloorToInt(diff / cellScale);

            if (coodX == lastTileX && coodZ == lastTileZ)
                return;

            lastTileX = coodX;
            lastTileZ = coodZ;

            Transform currentTile = transform.GetChild(2).Find(coodX + "|" + coodZ);

            if(currentTile != null)
            {
                int currentType = typeMap[coodX, coodZ];
                if (currentType == currentTypeChosen)
                {
                    Destroy(currentTile.gameObject);
                    typeMap[coodX, coodZ] = 0;
                    return;
                }

                if (currentType != 0)
                {
                    MeshRenderer mr = currentTile.GetComponent<MeshRenderer>();
                    mr.material.color = tileTypes[currentTypeChosen].color;
                    currentTile.GetChild(0).GetComponent<SpriteRenderer>().sprite = tileTypes[currentTypeChosen].icon;
                    currentTile.transform.GetChild(0).GetComponent<SpriteRenderer>().color = tileTypes[currentTypeChosen].color;
                    typeMap[coodX, coodZ] = currentTypeChosen;
                }
                else
                {
                    Destroy(currentTile.gameObject);
                    typeMap[coodX, coodZ] = currentTypeChosen;
                }
            }
            else
            {
                if (currentTypeChosen == 0)
                    return;

                GameObject nuTile = Instantiate<GameObject>(typeTilePrefab);
                nuTile.name = coodX + "|" + coodZ;
                float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                float posY = transform.position.y + (cellScale * ((0.5f * heightMap[coodX, coodZ]) + typeTileAddHeight));
                float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                nuTile.transform.position = new Vector3(posX, posY, posZ);
                nuTile.transform.localScale = new Vector3(cellScale, cellScale, 0.5f * cellScale);

                MeshRenderer mr = nuTile.GetComponent<MeshRenderer>();
                mr.material.color = tileTypes[currentTypeChosen].color;
                nuTile.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tileTypes[currentTypeChosen].icon;
                nuTile.transform.GetChild(0).GetComponent<SpriteRenderer>().color = tileTypes[currentTypeChosen].color;
                typeMap[coodX, coodZ] = currentTypeChosen;

                nuTile.transform.parent = transform.GetChild(2);
            }
        }
    }

    private void UpdateTypeIndicators(int coodX, int coodZ, bool setActive = true)
    {
        Transform currentTile = transform.GetChild(2).Find(coodX + "|" + coodZ);

        if (currentTile != null)
        {
            Vector3 pos = currentTile.position;
            pos.y = transform.position.y + (cellScale * ((0.5f * heightMap[coodX, coodZ]) + 0.25f));
            currentTile.position = pos;
            currentTile.gameObject.SetActive(setActive);
        }
    }

    private void UpdateAllTypeIndicators()
    {
        for (int j = 0; j < heightMap.GetLength(1); j++)
        {
            for (int i = 0; i < heightMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                UpdateTypeIndicators(coodX, coodZ, MapManager._instance.showTileTypes);
            }
        }
    }

    private void ClearTypeIndicators()
    {
        if (heightMap == null)
            return;

        for (int j = 0; j < heightMap.GetLength(1); j++)
        {
            for (int i = 0; i < heightMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                Transform currentTile = transform.GetChild(2).Find(coodX + "|" + coodZ);

                if (currentTile != null)
                {
                    typeMap[coodX, coodZ] = 0;
                    Destroy(currentTile.gameObject);
                }
            }
        }
    }

    private void UpdatePitTiles(int coodX, int coodZ, bool stopRecursion = false)
    {
        Transform topTile = transform.GetChild(0).Find(coodX + "|" + coodZ + "|" + ("pit"));

        bool quadActive = !(coodX > 0 && heightMap[coodX - 1, coodZ] < 0);
        topTile.GetChild(0).gameObject.SetActive(quadActive);
        if (!quadActive && !stopRecursion)
            UpdatePitTiles(coodX - 1, coodZ, true);
        quadActive = !(coodZ < (heightMap.GetLength(1) - 1) && heightMap[coodX, coodZ + 1] < 0);
        topTile.GetChild(1).gameObject.SetActive(quadActive);
        if (!quadActive && !stopRecursion)
            UpdatePitTiles(coodX, coodZ + 1, true);
        quadActive = !(coodX < (heightMap.GetLength(0) - 1) && heightMap[coodX + 1, coodZ] < 0);
        topTile.GetChild(2).gameObject.SetActive(quadActive);
        if (!quadActive && !stopRecursion)
            UpdatePitTiles(coodX + 1, coodZ, true);
        quadActive = !(coodZ > 0 && heightMap[coodX, coodZ - 1] < 0);
        topTile.GetChild(3).gameObject.SetActive(quadActive);
        if (!quadActive && !stopRecursion)
            UpdatePitTiles(coodX, coodZ - 1, true);
    }

    private void UpdateAllPitTiles()
    {
        for (int j = 0; j < heightMap.GetLength(1); j++)
        {
            for (int i = 0; i < heightMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                if (heightMap[i, j] < 0)
                {
                    Transform topTile = transform.GetChild(0).Find(coodX + "|" + coodZ + "|" + ("pit"));

                    topTile.GetChild(0).gameObject.SetActive(!(i > 0 && heightMap[i - 1, j] < 0));
                    topTile.GetChild(1).gameObject.SetActive(!(j < (heightMap.GetLength(1) - 1) && heightMap[i, j + 1] < 0));
                    topTile.GetChild(2).gameObject.SetActive(!(i < (heightMap.GetLength(0) - 1) && heightMap[i + 1, j] < 0));
                    topTile.GetChild(3).gameObject.SetActive(!(j > 0 && heightMap[i, j - 1] < 0));
                }
            }
        }
    }

    private List<Vector3> auxPoints;

    public void AutoBuildTileMap()
    {
        ClearTileMap();

        auxPoints = new List<Vector3>();

        for (int j = 0; j < heightMap.GetLength(1); j++)
        {
            for(int i = 0; i < heightMap.GetLength(0); i++)
            {
                int coodX = i;
                int coodZ = j;

                float posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                //posX += 0.5f * (cellScale / (float)MapManager._instance.terrainMeshDetail);
                float posY = bounds.ceilY + tileMeshZFightFactor;
                float posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);
                //posZ += 0.5f * (cellScale / (float)MapManager._instance.terrainMeshDetail);

                float crashY = GetTerrainYAt(new Vector3(posX, posY, posZ));

                Vector3[] rayOrigins = new Vector3[5] { new Vector3(posX, posY, posZ),
                new Vector3(posX, posY, posZ) + (0.48f * cellScale * Vector3.right),
                new Vector3(posX, posY, posZ) - (0.48f * cellScale * Vector3.right),
                new Vector3(posX, posY, posZ) + (0.48f * cellScale * Vector3.forward),
                new Vector3(posX, posY, posZ) - (0.48f * cellScale * Vector3.forward)};

                for (int rayIdx = 0; rayIdx < rayOrigins.Length; rayIdx++)
                {
                    //we raycast obstacles
                    Ray ray = new Ray(rayOrigins[rayIdx], Vector3.down);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 100f, ShapesManager._instance.grabbableLayers))
                    {
                        if (hitInfo.transform.gameObject.layer == 8)
                        {
                            /* deprecated after the new object collision system

                            //we need to maybe check if its marked as a decoration, to give maximum height
                            ShapeInfo sb = hitInfo.transform.parent.GetComponent<ShapeInfo>();
                            if (!sb.isDecor)
                            {
                                if (crashY < hitInfo.point.y)
                                    crashY = hitInfo.point.y;
                            }
                            */
                        }
                    }
                }

                auxPoints.Add(new Vector3(posX, crashY, posZ));

                if (crashY < 0.8f * minMorphHeight)
                {
                    float tryHeight = transform.position.y + (cellScale * ((-0.25f)));

                    GameObject nuTile = Instantiate<GameObject>(negativeTerrainTilePrefab);
                    nuTile.name = coodX + "|" + coodZ + "|" + ("pit");
                    posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                    posY = tryHeight + tileMeshZFightFactor;
                    posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                    nuTile.transform.position = new Vector3(posX, posY, posZ);
                    nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                    nuTile.transform.parent = transform.GetChild(0);

                    heightMap[coodX, coodZ] = -1;

                    /*
                    if (mapAlphaState == 1 || mapAlphaState == 0)
                    {
                        for (int c = 0; c < nuTile.transform.childCount; c++)
                        {
                            nuTile.transform.GetChild(c).GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                        }
                    }
                    */
                }
                else
                {
                    for (int t = 0; t < maxCellHeight; t++)
                    {
                        float tryHeight = transform.position.y + (cellScale * ((0.5f * heightMap[coodX, coodZ]) + 0.25f));

                        if (tryHeight > crashY)
                            break;

                        GameObject nuTile = Instantiate<GameObject>(terrainTilePrefab);
                        nuTile.name = coodX + "|" + coodZ + "|" + (heightMap[coodX, coodZ] + 1);
                        posX = mapOrigin.x + ((coodX + 0.5f) * cellScale);
                        posY = tryHeight + tileMeshZFightFactor;
                        posZ = mapOrigin.z + ((coodZ + 0.5f) * cellScale);

                        nuTile.transform.position = new Vector3(posX, posY, posZ);
                        nuTile.transform.localScale = new Vector3(cellScale, 0.5f * cellScale, cellScale);

                        nuTile.transform.parent = transform.GetChild(0);

                        heightMap[coodX, coodZ] = heightMap[coodX, coodZ] + 1;

                        /*
                        if (mapAlphaState == 1 || mapAlphaState == 0)
                        {
                            nuTile.GetComponent<MeshRenderer>().material = mapFadedState ? MapManager._instance.tileMaterialFaded : MapManager._instance.tileMaterial;
                        }
                        */

                        if (heightMap[coodX, coodZ] == maxCellHeight)
                        {
                            nuTile.transform.GetChild(0).gameObject.SetActive(true);
                            UpdateTypeIndicators(coodX, coodZ, false);
                        }
                    }
                }

                UpdateTypeIndicators(coodX, coodZ);
            }
        }

        PieceManager._instance.AdaptToTileHeight();
        GameModeManager._instance.SetTileNumerationUpdateFlag();
        ColorManager._instance.ForceUpdateColorMarkers(false);

        StartCoroutine(AfterFramePitUpdate());

        ChangeTool(0);
    }

    public void ClearTileMap(bool updateTypes = true)
    {
        heightMap = new int[heightMap.GetLength(0), heightMap.GetLength(1)];

        for (int i = transform.GetChild(0).childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(0).GetChild(i).gameObject);
        }

        if(updateTypes)
            UpdateAllTypeIndicators();
    }

    public void CleanExistingMap()
    {
        if (heightMap == null)
            return;

        ClearTypeIndicators();
        ClearTileMap(false);
    }

    private IEnumerator AfterFramePitUpdate()
    {
        yield return new WaitForEndOfFrame();

        UpdateAllPitTiles();
    }

    public void ResetAuxTileRegistry()
    {
        lastTileX = lastTileZ = int.MaxValue;
    }

    public void UpdateTileMaterial()
    {
        for(int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            Transform targetTile = transform.GetChild(0).GetChild(i);

            ApplyColorToTile(targetTile);
        }
    }

    public bool CoordinateInGrid(Vector2 cood)
    {
        if (cood.x >= heightMap.GetLength(0) || cood.y >= heightMap.GetLength(1))
            return false;
        else if (cood.x < 0 || cood.y < 0)
            return false;

        return true;
    }

    public Vector3 GetSnappedPosition(Vector3 pos)
    {
        float auxHeight = pos.y;

        float diffX = pos.x - mapOrigin.x;
        float diffZ = pos.z - mapOrigin.z;

        if (diffX < 0f || diffZ < 0f)
            return pos;

        int i = Mathf.FloorToInt((diffX / cellScale));
        int j = Mathf.FloorToInt((diffZ / cellScale));

        if (i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return pos;

        Vector3 compPoint = new Vector3(((i + 0.5f) * cellScale) + mapOrigin.x, auxHeight, ((j + 0.5f) * cellScale) + mapOrigin.z);
        compPoint.y = mapOrigin.y + (0.5f * heightMap[i, j]);

        if (Mathf.Abs(compPoint.x - pos.x) < ShapesManager._instance.snapDistance &&
            Mathf.Abs(compPoint.z - pos.z) < ShapesManager._instance.snapDistance)
        {
            return compPoint;
        }
        else
            return pos;
    }

    public Vector3 TranslateToGridPosition(Vector3 pos)
    {
        float auxHeight = pos.y;

        float diffX = pos.x - mapOrigin.x;
        float diffZ = pos.z - mapOrigin.z;

        if (diffX < 0f || diffZ < 0f)
            return pos;

        int i = Mathf.FloorToInt((diffX / cellScale));
        int j = Mathf.FloorToInt((diffZ / cellScale));

        if (i < 0 || j < 0 || i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return pos;

        Vector3 compPoint = new Vector3(((i + 0.5f) * cellScale) + mapOrigin.x, auxHeight, ((j + 0.5f) * cellScale) + mapOrigin.z);

        compPoint.y = mapOrigin.y + (0.5f * heightMap[i, j]);

        return compPoint;
    }

    public Vector2 TranslateToGridCoordinates(Vector3 pos)
    {
        float auxHeight = pos.y;

        float diffX = pos.x - mapOrigin.x;
        float diffZ = pos.z - mapOrigin.z;

        int i = Mathf.FloorToInt((diffX / cellScale));
        int j = Mathf.FloorToInt((diffZ / cellScale));

        if (i < 0 || j < 0 || i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return Vector2.positiveInfinity;

        return new Vector2(i, j);
    }

    public Vector3 TranslatePositionFromGridCoordinates(Vector2 coods)
    {
        float x = coods.x * cellScale;
        float z = coods.y * cellScale;
        x += mapOrigin.x + 0.5f;
        z += mapOrigin.z + 0.5f;

        float y = 0.5f * heightMap[Mathf.FloorToInt(coods.x), Mathf.FloorToInt(coods.y)];

        return new Vector3(x, y, z);
    }

    public void ChangeTool(int toolIndex)
    {
        if (currentTool == TerrainTools.TerrainMorphing && (TerrainTools)toolIndex != TerrainTools.TerrainMorphing)
        {
            terrainModHelper.SetActive(false);
            morphRadiusSlider.gameObject.SetActive(false);
            morphCurveSlider.gameObject.SetActive(false);
            morphRadiusSlider.transform.parent.gameObject.SetActive(false);

            floorMesh = floorMF.mesh;

            SetMapLook(MapLookState.TerrainEdit);

            PostMorphCleanup();
        }
        else if (currentTool != TerrainTools.TerrainMorphing && (TerrainTools)toolIndex == TerrainTools.TerrainMorphing)
        {
            terrainModHelper.SetActive(true);
            morphRadiusSlider.gameObject.SetActive(true);
            morphCurveSlider.gameObject.SetActive(true);
            morphRadiusSlider.transform.parent.gameObject.SetActive(true);

            SetMapLook(MapLookState.TerrainMorph);

            MapManager._instance.mapModeController.ActivateTerrainMorphMenu(true);
        }

        if (currentTool == TerrainTools.TileType && (TerrainTools)toolIndex != TerrainTools.TileType)
        {
            //MapManager._instance.mapModeController.ActivateTerrainTypeMenu(false); // .tileTypesPanel.gameObject.SetActive(false);
        }
        else if (currentTool != TerrainTools.TileType && (TerrainTools)toolIndex == TerrainTools.TileType)
        {
            MapManager._instance.mapModeController.ActivateTerrainTypeMenu(true); // .tileTypesPanel.gameObject.SetActive(true);
        }

        if (currentTool == TerrainTools.AutoTile && (TerrainTools)toolIndex != TerrainTools.AutoTile)
        {
            //MapManager._instance.mapModeController.ActivateAutoTileMenu(false); // .autoTilePanel.gameObject.SetActive(false);
        }
        else if (currentTool != TerrainTools.AutoTile && (TerrainTools)toolIndex == TerrainTools.AutoTile)
        {
            MapManager._instance.mapModeController.ActivateAutoTileMenu(true); // .autoTilePanel.gameObject.SetActive(true);
        }

        currentTool = (TerrainTools)toolIndex;

        for (int i = 0; i < 4; i++)
        {
            Image button = MapManager._instance.mapTileButtons[MapManager._instance.mapModeController.CurrentMode];
            if (i == 1)
                button = MapManager._instance.modifyTerrainButtons[0];
            else if (i == 2)
                button = MapManager._instance.autoMapTileButtons[0];
            else if (i == 3)
                button = MapManager._instance.chooseTileButtons[MapManager._instance.mapModeController.CurrentMode];

            button.GetComponent<Image>().color = (i == toolIndex) ? ColorManager._instance.colorButtonSelected : ColorManager._instance.colorButtonUnselected;
            button.transform.GetChild(0).GetComponent<Image>().color = (i == toolIndex) ? ColorManager._instance.iconButtonSelected : ColorManager._instance.iconButtonUnselected;
            button.transform.GetChild(1).GetComponent<Image>().color = (i == toolIndex) ? ColorManager._instance.iconButtonSelected : ColorManager._instance.iconButtonUnselected;
        }
    }

    public void PostMorphCleanup()
    {
        if (wasMorphed)
        {

            UpdateFloorMeshBorderHeights();
            MakeBorderHeightSet();
            ColorManager._instance.ForceUpdateColorMarkers(true);
            wasMorphed = false;
        }
    }

    public void ChangeTerrainType(int index)
    {
        currentTypeChosen = index;

        MapManager._instance.mapModeController.SetTerrainChange(index);
    }

    public void SetTerrainMorphToolState(int state)
    {
        // 0 - none, 1 - up, 2 - down
        Color colly = morphIdleColor;
        if (state == 1)
            colly = morphPlusColor;
        else if (state == 2)
            colly = morphMinusColor;

        if (state != 0)
        {
            arrowIndicator.gameObject.SetActive(true);
            arrowIndicator.GetChild(0).GetComponent<MeshRenderer>().material.color = colly;
            arrowIndicator.GetChild(1).GetComponent<MeshRenderer>().material.color = colly;
            arrowIndicator.rotation = Quaternion.Euler(state == 1 ? 0f : 180f, 0f, 0f);
        }
        else
            arrowIndicator.gameObject.SetActive(false);

        ringIndicator.GetComponent<MeshRenderer>().material.color = colly;
        glowIndicator.GetComponent<MeshRenderer>().material.color = colly;
    }

    public void UpdateRadiusSlider()
    {
        float normValue = morphRadiusSlider.normalizedValue;
        currentRadiusValue = Mathf.Lerp(MapManager._instance.minModRadiusValue, MapManager._instance.maxModRadiusValue, normValue);

        Vector3 sc = ringIndicator.localScale;
        sc.y = sc.z = currentRadiusValue * ringSizeFactor;
        ringIndicator.localScale = sc;
        sc.x = sc.z;
        sc.y = 1f;
        glowIndicator.parent.localScale = sc;
    }

    public void UpdateCurveSlider()
    {
        float normValue = morphCurveSlider.normalizedValue;
        currentCurveValue = Mathf.Lerp(-MapManager._instance.curveRange, MapManager._instance.curveRange, normValue);
    }


    public void ShowTiles(bool show)
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(show);
        }
    }

    public void ShowTerrainTypes(bool show)
    {
        for (int i = 0; i < transform.GetChild(2).childCount; i++)
        {
            transform.GetChild(2).gameObject.SetActive(show);
        }
    }

    public void SetGridVisibility(bool value)
    {
        /*
        if (value && !gridMade)
            return;
        */

        //transform.GetChild(1).gameObject.SetActive(value);

        /*
        if (floorObj != null)
        {
            floorMR = floorObj.GetComponent<MeshRenderer>();
            floorMR.material = value ? floorGridMaterial : floorMaterial;
        }
        */

        gridActive = value;
        PlayerPrefs.SetInt("GridVisible", value ? 1 : 0);

        ApplyColorToMesh();
    }

    public void SetSavedGridVisibility()
    {
        gridActive = PlayerPrefs.GetInt("GridVisible", 0) == 1;

        //transform.GetChild(1).gameObject.SetActive(gridActive);

        /*
        if (floorObj != null)
        {
            floorMR = floorObj.GetComponent<MeshRenderer>();
            floorMR.material = gridActive ? floorGridMaterial : floorMaterial;
        }
        */

        ApplyColorToMesh();
    }

    public void MakeTileNumeration()
    {
        bool meshMode = MapManager._instance.mapModeController.CurrentMode != 0;
        lastTilingWasMesh = meshMode;

        if (tileNumerationObjects != null)
        {
            for(int i = 0; i < tileNumerationObjects.Count; i++)
            {
                Destroy(tileNumerationObjects[i]);
            }
        }

        tileNumerationObjects = new List<GameObject>();

        for(int j = 0; j < cellCountZ; j++)
        {
            for (int i = 0; i < cellCountX; i++)
            {
                GameObject nuTileNumber = Instantiate<GameObject>(tileNumberPrefab, tileNumberParent);
                nuTileNumber.name = i + "," + j;

                Vector3 numberPosition = TranslatePositionFromGridCoordinates(new Vector2(i, j)) + (0.1f * Vector3.up);

                numberPosition.x += tileNumberPosMod.x * cellScale;
                numberPosition.z += tileNumberPosMod.z * cellScale;
                numberPosition.y += tileNumberPosMod.y;

                nuTileNumber.transform.position = numberPosition;

                TextMeshPro tmpro = nuTileNumber.transform.GetComponent<TextMeshPro>();
                tmpro.text = MiscTools.GetCellCoordinateForm(i, j);
                tmpro.fontSize = numberFontSize;
                tmpro.fontSharedMaterial = meshMode ? MapManager._instance.fontMeshMaterial : MapManager._instance.fontTileMaterial;

                tmpro.SetMaterialDirty();

                tileNumerationObjects.Add(nuTileNumber);

                /*
                if(i == cellCountX - 1 && j == cellCountZ - 1)
                    tmpro.fontSharedMaterial = meshMode ? MapManager._instance.fontMeshMaterial : MapManager._instance.fontTileMaterial;
                */
            }
        }
    }

    public void SetCellNumberingVisibility(bool value, bool forceNumerationUpdate = false)
    {
        bool currentIsMesh = MapManager._instance.mapModeController.CurrentMode != 0;

        if (tileNumerationObjects == null || forceNumerationUpdate || lastTilingWasMesh != currentIsMesh)
            MakeTileNumeration();

        for(int i = 0; i < tileNumerationObjects.Count; i++)
        {
            tileNumerationObjects[i].SetActive(value);
        }

        tileNumberingActive = value;
    }

    public float GetTerrainYAt(Vector3 position)
    {
        Vector3[] vertices = floorMesh.vertices;
        int[] tris = floorMesh.triangles;

        position.y = 0;

        List<Vector3> vMatchs = new List<Vector3>();
        float fracDistance = cellScale / (float)MapManager._instance.terrainMeshDetail;

        int counter = 0;
        while (true)
        {
            if (counter >= tris.Length)
                break;

            bool match = true;
            for(int i = 0; i < 3; i++)
            {
                Vector3 v = vertices[tris[counter + i]];
                if(Mathf.Abs(v.x - position.x) > fracDistance)
                {
                    match = false;
                    break;
                }
                else if(Mathf.Abs(v.z - position.z) > fracDistance)
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                vMatchs.Add(vertices[tris[counter]]);
                vMatchs.Add(vertices[tris[counter + 1]]);
                vMatchs.Add(vertices[tris[counter + 2]]);
                break;
            }
            else
                counter += 3;
        }

        if(vMatchs.Count < 3)
        {
            return 0f;
        }

        Vector3 origin = vMatchs[0];
        Vector3 direction = Vector3.down;
        Vector3 norm = Vector3.Cross(vMatchs[2] - vMatchs[0], vMatchs[1] - vMatchs[0]);

        float denominator = Vector3.Dot(direction, norm);

        if (denominator < 0.001f)
        {
            return 0f;
        }

        float t = Vector3.Dot(origin - position, norm) / denominator;
        Vector3 p = position + (Vector3.down * t);

        return p.y;
    }

    public float GetTileHeightAt(Vector3 position)
    {
        float auxHeight = position.y;

        float diffX = position.x - mapOrigin.x;
        float diffZ = position.z - mapOrigin.z;

        if (diffX < 0f || diffZ < 0f)
            return position.y;

        int i = Mathf.FloorToInt((diffX / cellScale));
        int j = Mathf.FloorToInt((diffZ / cellScale));

        if (i < 0 || j < 0 || i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return 0f;

        return 0.5f * heightMap[i, j];
    }

    public int GetTileValueAt(Vector2 position)
    {
        int i = Mathf.FloorToInt(position.x);
        int j = Mathf.FloorToInt(position.y);

        if (i < 0 || j < 0 || i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return -1;

        return heightMap[i, j];
    }

    public int GetTileTypeAt(Vector2 position)
    {
        int i = Mathf.FloorToInt(position.x);
        int j = Mathf.FloorToInt(position.y);

        if (i < 0 || j < 0 || i >= heightMap.GetLength(0) || j >= heightMap.GetLength(1))
            return -1;

        return typeMap[i, j];
    }

    private void FixHelpGlowNormals()
    {
        if (glowFixedFlag)
            return;

        MeshFilter helpMF = terrainModHelper.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
        Mesh glowMesh = helpMF.mesh;

        Vector3[] vertices = glowMesh.vertices;
        Vector2[] uvs = glowMesh.uv;

        int i = 0;
        while(i < vertices.Length)
        {
            for (int v = 0; v < 4; v++)
            {
                Vector2 uv = uvs[i + v];
                if (v == 0)
                    uv = new Vector2(0f, 0f);
                else if (v == 1)
                    uv = new Vector2(1f, 0f);
                else if (v == 2)
                    uv = new Vector2(1f, 1f);
                else if (v == 3)
                    uv = new Vector2(0f, 1f);

                uvs[i + v] = uv;
            }

            i += 4;
        }

        glowMesh.SetUVs(0, uvs);
        helpMF.mesh = glowMesh;
        glowFixedFlag = true;
    }

    private float[,] ArrayToYMap(float[] yMap)
    {
        int counter = 0;
        float[,] yMeshMap = new float[((MapManager._instance.terrainMeshDetail * heightMap.GetLength(0)) + 1), ((MapManager._instance.terrainMeshDetail * heightMap.GetLength(1)) + 1)];

        for (int j = 0; j < yMeshMap.GetLength(1); j++)
        {
            for (int i = 0; i < yMeshMap.GetLength(0); i++)
            {
                yMeshMap[i, j] = yMap[counter];
                counter++;
            }
        }

        return yMeshMap;
    }

    private int[,] ArrayToMapMatrix(int[] arr)
    {
        int counter = 0;
        int[,] tileMap = new int[heightMap.GetLength(0), heightMap.GetLength(1)];

        for(int j = 0; j < tileMap.GetLength(1); j++)
        {
            for(int i = 0; i < tileMap.GetLength(1); i++)
            {
                tileMap[i, j] = arr[counter];
                counter++;
            }
        }

        return tileMap;
    }

    public MeshFilter GetFloorMeshFilter()
    {
        return floorMF;
    }

    public Vector3[] GetCellBorderHeights(Vector2 cood)
    {
        if (!CoordinateInGrid(cood))
            return new Vector3[0];

        //there has to be a better way. maybe store the y's.
        //return BorderHeightsThroughIntersect(cood);

        return BorderHeighsThroughArray(cood);
    }

    private Vector3[] BorderHeightsThroughIntersect(Vector2 cood)
    {
        //very slow

        Vector3 worldCenter = TranslatePositionFromGridCoordinates(cood);
        worldCenter.y = 0.5f + maxMorphHeight;

        Vector3[] borderHeights = new Vector3[4];
        Ray intersector = new Ray(worldCenter + (0.45f * cellScale * Vector3.left) + (0.45f * cellScale * Vector3.forward), Vector3.down);
        Vector3 outInters;
        if (GetFloorIntersection(intersector, out outInters))
            borderHeights[0] = outInters;

        intersector = new Ray(worldCenter + (0.45f * cellScale * Vector3.right) + (0.45f * cellScale * Vector3.forward), Vector3.down);
        if (GetFloorIntersection(intersector, out outInters))
            borderHeights[1] = outInters;

        intersector = new Ray(worldCenter + (0.45f * cellScale * Vector3.right) + (0.45f * cellScale * Vector3.back), Vector3.down);
        if (GetFloorIntersection(intersector, out outInters))
            borderHeights[2] = outInters;

        intersector = new Ray(worldCenter + (0.45f * cellScale * Vector3.left) + (0.45f * cellScale * Vector3.back), Vector3.down);
        if (GetFloorIntersection(intersector, out outInters))
            borderHeights[3] = outInters;

        return borderHeights;
    }

    public bool GetFloorIntersection(Ray ray, out Vector3 IntersectionPoint)
    {
        IntersectionPoint = Vector3.negativeInfinity;

        if (floorMesh == null)
            return false;

        if(MiscTools.RayMeshClosestIntersection(floorMesh, ray, out IntersectionPoint))
        {
            return true;
        }

        return false;
    }

    private struct CoodHeightSet
    {
        public Vector3[] heightPoints;
    }

    private CoodHeightSet[,] savedBorderHeightSets;

    //very very slow, but only triggers upon building the mesh or changing it
    private void MakeBorderHeightSet()
    {
        savedBorderHeightSets = new CoodHeightSet[cellCountX,cellCountZ];

        int indexCounter = 0;
        for(int z = 0; z < cellCountZ; z++)
        {
            for(int x = 0; x < cellCountX; x++)
            {
                //Vector2 coodVec = new Vector2(x, z);
                //Vector3[] heightSet = BorderHeightsThroughIntersect(coodVec);

                int fmhmX = x * MapManager._instance.terrainMeshDetail; //floor mesh height map X
                int fmhmZ = z * MapManager._instance.terrainMeshDetail;

                Vector3 centerCood = TranslatePositionFromGridCoordinates(new Vector2(x,z));

                Vector3[] heightSet = new Vector3[4];
                heightSet[0] = new Vector3(centerCood.x - (0.5f * cellScale), floorMeshHeightMap[fmhmX, fmhmZ], centerCood.z - (0.5f * cellScale));
                heightSet[1] = new Vector3(centerCood.x + (0.5f * cellScale), floorMeshHeightMap[fmhmX + MapManager._instance.terrainMeshDetail, fmhmZ], centerCood.z - (0.5f * cellScale));
                heightSet[2] = new Vector3(centerCood.x - (0.5f * cellScale), floorMeshHeightMap[fmhmX, fmhmZ + MapManager._instance.terrainMeshDetail], centerCood.z + (0.5f * cellScale));
                heightSet[3] = new Vector3(centerCood.x + (0.5f * cellScale), floorMeshHeightMap[fmhmX + MapManager._instance.terrainMeshDetail, fmhmZ + MapManager._instance.terrainMeshDetail], centerCood.z + (0.5f * cellScale));

                CoodHeightSet aux = new CoodHeightSet();
                aux.heightPoints = heightSet;

                savedBorderHeightSets[x,z] = aux;

                indexCounter++;
            }
        }
    }

    private Vector3[] BorderHeighsThroughArray(Vector2 cood)
    {
        for(int i = 0; i < savedBorderHeightSets.Length; i++)
        {
            int codX = Mathf.FloorToInt(cood.x);
            int codZ = Mathf.FloorToInt(cood.y);

            return savedBorderHeightSets[codX, codZ].heightPoints;
        }

        return new Vector3[0];
    }

    public void UpdateFloorMeshBorderHeights()
    {
        float[] yMap = new float[floorMesh.vertexCount];

        for (int i = 0; i < floorMesh.vertexCount; i++)
        {
            yMap[i] = floorMesh.vertices[i].y;
        }

        floorMeshHeightMap = ArrayToYMap(yMap);
    }

    public int[] GetMapDimensions()
    {
        return new int[2] { cellCountX, cellCountZ };
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mapOrigin, 1f);

        if (auxPoints != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < auxPoints.Count; i++)
            {
                Gizmos.DrawWireSphere(auxPoints[i], 0.15f);
            }
        }

        /*
        if (heightMap != null)
        {
            Gizmos.color = Color.cyan;
            Vector3[] p = new Vector3[8];
            for(int i = 0; i < p.Length; i++)
            {
                p[i] = mapOrigin;
            }
            p[1].x = bounds.maxX;
            p[2].x = bounds.maxX;
            p[2].z = bounds.maxZ;
            p[3].z = bounds.maxZ;
            p[4] = p[0]; p[4].y = bounds.ceilY;
            p[5] = p[1]; p[5].y = bounds.ceilY;
            p[6] = p[2]; p[6].y = bounds.ceilY;
            p[7] = p[3]; p[7].y = bounds.ceilY;

            Gizmos.DrawLine(p[0], p[1]);
            Gizmos.DrawLine(p[1], p[2]);
            Gizmos.DrawLine(p[2], p[3]);
            Gizmos.DrawLine(p[3], p[0]);
            Gizmos.DrawLine(p[4], p[5]);
            Gizmos.DrawLine(p[5], p[6]);
            Gizmos.DrawLine(p[6], p[7]);
            Gizmos.DrawLine(p[7], p[4]);
            Gizmos.DrawLine(p[0], p[4]);
            Gizmos.DrawLine(p[1], p[5]);
            Gizmos.DrawLine(p[2], p[6]);
            Gizmos.DrawLine(p[3], p[7]);
        }
        */
    }
}
