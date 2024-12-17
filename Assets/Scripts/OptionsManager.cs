using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System;
using System.Linq;
using JetBrains.Annotations;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private string mapFolder = "MapData";
    [SerializeField] private string charaFolder = "UnitData";
    [SerializeField] private string foeFolder = "FoeData";

    public int unitFileIndex { get; private set; }

    public int graphicFileIndex { get; private set; }

    public string currentExpeditionRoot { get; private set; }

    private float entrySeparation;
    private float entryHeight;

    [Header("UI")]
    public GameObject optionsScreen;

    public Image optionsButton;
    public GameObject fileScroll;
    public GameObject newSaveButton;
    public RectTransform fileContent;
    public GameObject fileEntryPrefab;
    public RectTransform newFilePanel;
    public RectTransform oldFilePanelSave;
    public RectTransform oldFilePanelLoad;
    public TMPro.TMP_InputField newFileInput;

    [Space(5f)]
    public Image sortButton;
    public Sprite sortAZIcon;
    public Sprite sortTimeIcon;

    private string currentlyLoadedMap = "";

    [System.Serializable]
    public struct ResolutionOption
    {
        public Toggle toggleOption;
        public TextMeshProUGUI toggleLabel;
        public int width;
        public int height;
    }

    [Space(10f)]
    public ResolutionOption[] resOptions;

    [Space(10f)]
    public Toggle allMiniPanel;
    public Toggle selectedMiniPanel;
    public Toggle noMiniPanel;
    public Toggle showTiles;
    public Toggle showSpecialTerrain;
    public Toggle showGrid;
    public Toggle cameraInvertX;
    public Toggle cameraInvertY;
    public Toggle allMarks;
    public Toggle selectedMarks;
    public Toggle noMarks;

    [Space(10f)]
    public Toggle pieceAutoSize;
    public Toggle markerAdaptToTerrain;
    public Toggle markerAdaptToTile;

    [Space(10f)]
    public TMPro.TextMeshProUGUI tileStyleLabel;
    public Vector2 tileStylePanelProportions;
    private bool tileStyleListOpen;
    private RectTransform tileStyleListRT;
    public ListPanel tileStylePanel;

    char[] invalidFileChars = new char[10] { '<', '>', ':', '\"', '/', '\\', '|', '?', '*', ' ' };

    private int chosenFileIndex;
    private bool toSave = false;

    [Header("Autosave stuff")]
    [SerializeField] private RectTransform autoSaveButtonRT;
    [SerializeField] private TextMeshProUGUI autosaveButtonLabel;
    private bool autosavePanelOpen = false;
    [SerializeField] private int[] autosaveMinuteOptions;
    [SerializeField] private int defaultAutosaveOption = 1;
    private RectTransform listRT;

    private int currentAutosaveTimeOption;
    private float lastSaveTimeStamp;

    private void OnEnable()
    {
        /*
        if (MapManager._instance == null)
            return;

        if (MapManager._instance.mapTarget == null)
            showGrid.SetIsOnWithoutNotify(false);
        else
            showGrid.SetIsOnWithoutNotify(MapManager._instance.mapTarget.gridActive);
        */

        currentAutosaveTimeOption = PlayerPrefs.GetInt("autoSaveInterval", defaultAutosaveOption);
        if (currentAutosaveTimeOption == 0)
        {
            autosaveButtonLabel.text = "Never";
        }
        else if(currentAutosaveTimeOption == 1)
        {
            autosaveButtonLabel.text = "Save every minute";
        }
        else
        {
            autosaveButtonLabel.text = "Save every " + autosaveMinuteOptions[currentAutosaveTimeOption - 1] + " minutes";
        }

        lastSaveTimeStamp = Time.unscaledTime;
    }

    public void ToggleOptions()
    {
        SetOptionsActive(!optionsScreen.activeSelf);
    }

    public void SetOptionsActive(bool enabled)
    {
        optionsButton.color = enabled ? ShapesManager._instance.selectedColor : ShapesManager._instance.unselectedColor;
        optionsScreen.SetActive(enabled);

        MapManager._instance.EnableControls(!enabled);

        if (enabled)
        {
            ShapesManager._instance.ReleaseCurrentShape();
        }
        else
        {
            newFilePanel.gameObject.SetActive(false);
            fileScroll.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (MapManager._instance.menuManager.IsMenuActive)
            return;

        if (autosavePanelOpen && Input.GetMouseButtonDown(0))
        {
            if (!TooltipManager.CheckMouseInArea(listRT))
            {
                autosavePanelOpen = false;
                UnitManager._instance.listPanel.ShowPanel(false);
                UnitManager._instance.listPanel.OnEntryClick -= AutosaveListClick;
            }
        }

        if(currentAutosaveTimeOption > 0 && currentlyLoadedMap != "")
        {
            if ((Time.unscaledTime - lastSaveTimeStamp) > ((float)autosaveMinuteOptions[currentAutosaveTimeOption - 1] * 60f))
            {
                QuickSave();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !HelpManager._instance.IsHelpPanelActive)
            ToggleOptions();

        if(Input.GetKeyDown(KeyCode.F1))
            HelpManager._instance.SetHelpPanelActive();

        if (Input.GetKeyDown(KeyCode.F5))
            QuickSave();

        if (tileStyleListOpen && Input.GetMouseButtonDown(0))
        {
            if (!TooltipManager.CheckMouseInArea(tileStyleListRT))
            {
                tileStyleListOpen = false;
                tileStylePanel.ShowPanel(false);
                tileStylePanel.OnEntryClick -= UpdateTileStyle;
            }
        }
    }

    public void GoToMenu()
    {
        currentlyLoadedMap = "";

        SetOptionsActive(false);
        //MapManager._instance.menuManager.ReturnAnim();
        MapManager._instance.menuManager.ResetMenu();
        MapManager._instance.menuManager.ShowMenu(true);
        MapManager._instance.menuManager.MakeFileList();
    }

    public void OpenAutosaveTimeList()
    {
        UnitManager._instance.listPanel.screenProportionSize = 0.6f * UnitManager._instance.bondClassJobPanelProportions;
        UnitManager._instance.listPanel.listColor = 0.9f * Color.black;

        Vector3 listOrigin = autoSaveButtonRT.position + (-0.5f * autoSaveButtonRT.rect.size.x * autoSaveButtonRT.lossyScale.x * Vector3.right) + (1.5f * autoSaveButtonRT.rect.size.y * autoSaveButtonRT.lossyScale.y * Vector3.up);
        List<string> optionLabels = new List<string>();
        optionLabels.Add("Never");
        optionLabels.Add("Save every minute");
        for(int i = 1; i < autosaveMinuteOptions.Length; i++)
        {
            optionLabels.Add("Save every " + autosaveMinuteOptions[i] + " minutes");
        }

        UnitManager._instance.listPanel.ShowPanel(listOrigin, optionLabels, true);
        autosavePanelOpen = true;
        UnitManager._instance.listPanel.OnEntryClick += AutosaveListClick;

        listRT = UnitManager._instance.listPanel.GetComponent<RectTransform>();
    }

    public void AutosaveListClick(int optionIndex)
    {
        if(optionIndex == 0)
        {
            autosaveButtonLabel.text = "Never";
        }
        else if (currentAutosaveTimeOption == 1)
        {
            autosaveButtonLabel.text = "Save every minute";
        }
        else
        {
            autosaveButtonLabel.text = "Save every " + autosaveMinuteOptions[optionIndex - 1] + " minutes";
        }

        PlayerPrefs.SetInt("autoSaveInterval", optionIndex);

        currentAutosaveTimeOption = optionIndex;

        autosavePanelOpen = false;
        UnitManager._instance.listPanel.ShowPanel(false);
        UnitManager._instance.listPanel.OnEntryClick -= AutosaveListClick;

        lastSaveTimeStamp = Time.unscaledTime;
    }

    public void SetNewExpRoot(string expeditionName)
    {
        currentExpeditionRoot = "/Expeditions/" + expeditionName;
    }

    public string GetMapFolder()
    {
        return currentExpeditionRoot + "/" + mapFolder;
    }

    public string GetCharaFolder()
    {
        return currentExpeditionRoot + "/" + charaFolder;
    }

    public string GetFoeFolder()
    {
        return currentExpeditionRoot + "/" + foeFolder;
    }

    public void SetUnitIndex(int value)
    {
        unitFileIndex = value;
    }

    public void SetGraphicPieceIndex(int value)
    {
        graphicFileIndex = value;
    }

    public void SaveIndexFile()
    {
        FileStream file;

        //save the index file
        string indexFilePath = Application.persistentDataPath + currentExpeditionRoot + "/index.iconindex";
        if (File.Exists(indexFilePath)) file = File.OpenWrite(indexFilePath);
        else file = File.Create(indexFilePath);

        IndexFile data = new IndexFile();
        data.SetUnitIndex(unitFileIndex);
        data.SetImageIndex(graphicFileIndex);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public IndexFile LoadIndexFile()
    {
        FileStream file;

        //save the index file
        string indexFilePath = Application.persistentDataPath + currentExpeditionRoot + "/index.iconindex";
        if (File.Exists(indexFilePath)) file = File.OpenRead(indexFilePath);
        else file = File.Create(indexFilePath);

        BinaryFormatter bf = new BinaryFormatter();
        IndexFile data = (IndexFile)bf.Deserialize(file);
        file.Close();

        unitFileIndex = data.unitIndex;
        graphicFileIndex = data.imageIndex;

        return data;
    }

    #region Save Map Mechanic
    public void SaveMap(string fileName)
    {
        string mapPath = Application.persistentDataPath + "/" + GetMapFolder() + "/" + fileName + ".iconmap";
        FileStream file;

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetMapFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetMapFolder());

        if (File.Exists(mapPath)) file = File.OpenWrite(mapPath);
        else file = File.Create(mapPath);


        MapFile mf = new MapFile();
        //build mapfile
        mf = GetTerrainData(mf);
        mf = GetShapesData(mf);
        mf = GetTileData(mf);
        mf = GetTypeData(mf);
        mf = GetColoredMarkedTiles(mf);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, mf);
        file.Close();

        Debug.Log("map saved");

        SetOptionsActive(false);

        currentlyLoadedMap = fileName;
    }

    public void SaveCall()
    {
        MakeFileList(true);
    }

    public void SaveNewFile()
    {
        string fileName = newFileInput.text;

        fileName = MakeNameValid(fileName);
        if(fileName.Length == 0) 
            return;

        SaveMap(fileName);
        SaveMapPieces(fileName);

        currentlyLoadedMap = fileName;
    }

    public string MakeNameValid(string fileName)
    {
        //take out invalid characters
        for (int i = 0; i < invalidFileChars.Length; i++)
        {
            fileName.Replace(invalidFileChars[i], '_');
        }

        if (fileName.Length == 0)
            return "";

        return fileName;
    }

    public void QuickSave()
    {
        if (currentlyLoadedMap == "")
        {
            Debug.Log("no file loaded");
            return;
        }

        lastSaveTimeStamp = Time.unscaledTime;

        Debug.Log("saving " + currentlyLoadedMap);

        SaveMap(currentlyLoadedMap);
        SaveMapPieces(currentlyLoadedMap);
    }

    public void GiveNewMapName(string name)
    {
        currentlyLoadedMap = MakeNameValid(name);
    }

    #endregion

    #region Load Map Mechanic
    public void LoadMap(string fileName)
    {
        string mapPath = Application.persistentDataPath + "/" + GetMapFolder() + "/" + fileName + ".iconmap";
        FileStream file;

        File.SetLastAccessTime(mapPath, DateTime.Now);

        if (File.Exists(mapPath)) file = File.OpenRead(mapPath);
        else
        {
            Debug.LogError("File not found");
            return;
        }


        BinaryFormatter bf = new BinaryFormatter();
        MapFile data = (MapFile)bf.Deserialize(file);
        file.Close();

        //use data to build map
        MapManager._instance.mapModeController.SetTerrainMode(data.mapIsTileMode, false);

        MapManager._instance.mapTarget.LoadFloorMesh(data.mapSizeX, data.mapSizeZ, data.mapCellScale, data.mapTerrainDetail, data.yMap);
        ShapesManager._instance.CleanShapeContainer();

        if (data.shapeList != null)
            ShapesManager._instance.LoadShapes(data.shapeList);

        if (data.tileMap != null)
            MapManager._instance.mapTarget.LoadTileMap(data.tileMap);
        if (data.typeMap != null)
            MapManager._instance.mapTarget.LoadTypeMap(data.typeMap);

        MapManager._instance.mapTarget.SetCellNumberingVisibility(false);
        GameModeManager._instance.tileNumberToggle.ForceValue(false); // .SetIsOnWithoutNotify(false);

        if(data.colorMarkR != null)
        {
            Color[] tileMarkerColors = new Color[data.colorMarkR.Length];
            for (int i = 0; i < tileMarkerColors.Length; i++)
            {
                Color loadColor = new Color(data.colorMarkR[i], data.colorMarkG[i], data.colorMarkB[i]);
                tileMarkerColors[i] = loadColor;
            }

            ColorManager._instance.LoadMarkerTiles(data.coloredMarkedX, data.coloredMarkedZ, tileMarkerColors);
        }

        Color mapColor = new Color(data.MapColorR, data.MapColorG, data.MapColorB);
        MapManager._instance.mapTarget.SetMapColor(mapColor, MapManager._instance.checkeredColor, MapManager._instance.heightColor, false);
        MapManager._instance.mapTarget.SetMapLook(BattleMap.MapLookState.Default);
        currentlyLoadedMap = fileName;
    }

    public void LoadCall()
    {
        MakeFileList(false);
    }
    #endregion

    #region Map Data Managing
    private MapFile GetTerrainData(MapFile data)
    {
        MeshFilter floorMeshFilter = MapManager._instance.mapTarget.GetFloorMeshFilter();
        Mesh floorMesh = floorMeshFilter.mesh;

        float[] yMap = new float[floorMesh.vertexCount];

        for (int i = 0; i < floorMesh.vertexCount; i++)
        {
            yMap[i] = floorMesh.vertices[i].y;
        }

        data.GiveMapDetails(MapManager._instance.mapModeController.CurrentMode == 0, MapManager._instance.mapTarget.mapColor);

        data.GiveMapHeights(yMap);
        int[] dims = MapManager._instance.mapTarget.GetMapDimensions();
        data.GiveMapSize(dims[0], dims[1]);

        data.GiveMapCellScale(MapManager._instance.mapTarget.cellScale);
        data.GiveTerrainDetail(MapManager._instance.terrainMeshDetail);

        return data;
    }

    private MapFile GetShapesData(MapFile data)
    {
        Transform objContainer = ShapesManager._instance.GetObjectContainer();
        if (objContainer == null)
            return data;

        List<ShapeInfo> infoList = new List<ShapeInfo>();
        for (int i = 0; i < objContainer.childCount; i++)
        {
            ShapeInfo si = objContainer.GetChild(i).GetComponent<ShapeInfo>();
            infoList.Add(si);
        }

        data.GiveShapeList(infoList.ToArray());

        return data;
    }

    private MapFile GetTileData(MapFile data)
    {
        int[,] hMap = MapManager._instance.mapTarget.GetHeightMap();
        int[] arr = new int[hMap.GetLength(0) * hMap.GetLength(1)];

        int counter = 0;
        for (int j = 0; j < hMap.GetLength(1); j++)
        {
            for (int i = 0; i < hMap.GetLength(0); i++)
            {
                arr[counter] = hMap[i, j];
                counter++;
            }
        }

        data.GiveTileHeights(arr);
        return data;
    }

    private MapFile GetTypeData(MapFile data)
    {
        int[,] tMap = MapManager._instance.mapTarget.GetTypeMap();
        int[] arr = new int[tMap.GetLength(0) * tMap.GetLength(1)];

        int counter = 0;
        for (int j = 0; j < tMap.GetLength(1); j++)
        {
            for (int i = 0; i < tMap.GetLength(0); i++)
            {
                arr[counter] = tMap[i, j];
                counter++;
            }
        }

        data.GiveTileTypes(arr);
        return data;
    }

    private MapFile GetColoredMarkedTiles(MapFile data)
    {
        Transform[,] coloredTiles = ColorManager._instance.GetMarkerTiles();
        if (coloredTiles == null)
            return data;

        List<int> tileCoodX = new List<int>();
        List<int> tileCoodZ = new List<int>();
        List<float> tileColorR = new List<float>();
        List<float> tileColorG = new List<float>();
        List<float> tileColorB = new List<float>();

        for (int j = 0; j < coloredTiles.GetLength(1); j++)
        {
            for (int i = 0; i < coloredTiles.GetLength(0); i++)
            {
                Transform found = coloredTiles[i, j];
                if (found == null)
                    continue;

                tileCoodX.Add(i);
                tileCoodZ.Add(j);

                Color tileColor = found.GetComponent<MeshRenderer>().material.color;
                tileColorR.Add(tileColor.r);
                tileColorG.Add(tileColor.g);
                tileColorB.Add(tileColor.b);
            }
        }

        data.GiveColoredMarkTiles(tileCoodX.ToArray(), tileCoodZ.ToArray(), tileColorR.ToArray(), tileColorG.ToArray(), tileColorB.ToArray());
        return data;
    }
    #endregion


    #region Save Characters Mechanic
    public void SaveCharacter(int ID)
    {
        CharaFile toSave = GetCharaData(ID);

        if(toSave == null)
        {
            Debug.LogError("Character to save was not found. Saving unsuccesful.");
            return;
        }

        SaveCharacter(toSave);
    }

    public void SaveCharacter(EsperCharacter chara)
    {
        CharaFile toSave = GetCharaData(chara);

        if (toSave == null)
        {
            Debug.LogError("Character to save was not found. Saving unsuccesful.");
            return;
        }

        SaveCharacter(toSave);
    }

    public void SaveCharacter(CharaFile toSave)
    {
        Debug.Log("-----------> " + toSave.graphicPieceID);

        string hexID = toSave.charaID.ToString("X6");

        string unitPath = Application.persistentDataPath + "/" + GetCharaFolder() + "/chara_" + hexID + ".iconunits";
        FileStream file;

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetCharaFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetCharaFolder());

        if (File.Exists(unitPath)) file = File.OpenWrite(unitPath);
        else file = File.Create(unitPath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, toSave);
        file.Close();

        //Debug.Log("character saved");
    }
    #endregion

    #region Load Characters Mechanic

    public void LoadAllCharacters()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + GetCharaFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetCharaFolder());

        string[] fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + GetCharaFolder(), "*.iconunits");

        List<CharaFile> charaFiles = new List<CharaFile>();
        for(int i = 0; i < fileNames.Length; i++)
        {
            string simpleName = fileNames[i].Substring(fileNames[i].LastIndexOf('\\') + 1);
            simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

            CharaFile loadedChara = LoadCharacterFile(simpleName);
            if(loadedChara != null)
            {
                charaFiles.Add(loadedChara);
            }
        }

        List<EsperCharacter> loadedCharacters = new List<EsperCharacter>();
        for(int i = 0; i < charaFiles.Count; i++)
        {
            EsperCharacter nuChara = new EsperCharacter();
            nuChara.unitName = charaFiles[i].name;
            nuChara.level = charaFiles[i].level;
            nuChara.SetBaseHP(charaFiles[i].hp);
            nuChara.GiveID(charaFiles[i].charaID);
            nuChara.GiveGraphicPieceID(charaFiles[i].graphicPieceID);

            //nuChara.magicArts = charaFiles[i].classIndex;

            nuChara.lastModified = DateTime.FromBinary(charaFiles[i].lastModified);

            loadedCharacters.Add(nuChara);
        }

        UnitManager._instance.ReceiveLoadedCharacters(loadedCharacters);
        //Debug.Log("characters loaded");
    }

    public CharaFile LoadCharacterFile(string fileName)
    {
        string unitPath = Application.persistentDataPath + "/" + GetCharaFolder() + "/" + fileName + ".iconunits";
        FileStream file;

        if (File.Exists(unitPath)) file = File.OpenRead(unitPath);
        else
        {
            Debug.LogError("File not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        CharaFile data = (CharaFile)bf.Deserialize(file);
        file.Close();

        return data;
    }
    #endregion

    #region Delete Characters Mechanic
    public void DeleteCharacter(int id)
    {
        // delete character
        string hexID = id.ToString("X6");

        string unitPath = Application.persistentDataPath + "/" + GetCharaFolder() + "/chara_" + hexID + ".iconunits";

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetCharaFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetCharaFolder());

        if (File.Exists(unitPath))
            File.Delete(unitPath);

        /*

        //rearrange ids
        List<IconCharacter> allCharas = UnitManager._instance.GetCharacters();
        int counter = 0;
        int deleteIndex = -1;
        for(int i = 0; i < allCharas.Count; i++)
        {
            if (allCharas[i].unitID == id)
            {
                deleteIndex = i;
                continue;
            }

            IconCharacter aux = allCharas[i];
            aux.GiveID(counter);
            allCharas[i] = aux;
            counter++;
        }

        allCharas.RemoveAt(deleteIndex);

        for(int i = 0; i < allCharas.Count; i++)
        {
            SaveCharacter(allCharas[i]);
        }

        //delete last file (surplus)
        unitPath = Application.persistentDataPath + "/" + charaFolder + "/chara_" + allCharas.Count.ToString() + ".iconunits";

        if (File.Exists(unitPath))
            File.Delete(unitPath);
            */
    }
    #endregion

    #region Character Data Managing
    private CharaFile GetCharaData(int ID)
    {
        EsperCharacter source = UnitManager._instance.GetCharacter(ID);
        if (source == null)
            return null;

        return GetCharaData(source);
    }

    private CharaFile GetCharaData(EsperCharacter source)
    {
        CharaFile nuFile = new CharaFile();
        nuFile.GiveUnitID(source.unitID);
        nuFile.GiveGeneralAspects(source.unitName, source.level, source.baseHP, source.colorChoice);
        nuFile.GivePiecePartIDs(source.graphicImageID);
        //nuFile.GiveTacticalAspects(source.magicArts, source.skillsIDs);
        nuFile.SetLastModification(source.lastModified);

        return nuFile;
    }
    #endregion


    #region Save Foes Mechanic
    public void SaveFoe(int ID)
    {
        FoeFile toSave = GetFoeData(ID);

        if (toSave == null)
        {
            Debug.LogError("Foe to save was not found. Saving unsuccesful.");
            return;
        }

        SaveFoe(toSave);
    }

    public void SaveFoe(IconFoe foe)
    {
        FoeFile toSave = GetFoeData(foe);

        if (toSave == null)
        {
            Debug.LogError("Foe to save was not found. Saving unsuccesful.");
            return;
        }

        SaveFoe(toSave);
    }

    public void SaveFoe(FoeFile toSave)
    {
        string hexID = toSave.foeID.ToString("X6");

        string unitPath = Application.persistentDataPath + "/" + GetFoeFolder() + "/foe_" + hexID + ".iconunits";
        FileStream file;

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetFoeFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetFoeFolder());

        if (File.Exists(unitPath)) file = File.OpenWrite(unitPath);
        else file = File.Create(unitPath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, toSave);
        file.Close();

        //Debug.Log("character saved");
    }
    #endregion

    #region Load Foes Mechanic
    public void LoadAllFoes()
    {
        
        if (!Directory.Exists(Application.persistentDataPath + "/" + GetFoeFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetFoeFolder());

        string[] fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + GetFoeFolder(), "*.iconunits");

        List<FoeFile> foeFiles = new List<FoeFile>();
        for (int i = 0; i < fileNames.Length; i++)
        {
            string simpleName = fileNames[i].Substring(fileNames[i].LastIndexOf('\\') + 1);
            simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

            FoeFile loadedChara = LoadFoeFile(simpleName);
            if (loadedChara != null)
            {
                foeFiles.Add(loadedChara);
            }
        }

        List<IconFoe> loadedFoes = new List<IconFoe>();
        for (int i = 0; i < foeFiles.Count; i++)
        {
            IconFoe nuFoe = new IconFoe();
            nuFoe.unitName = foeFiles[i].name;
            nuFoe.type = (FoeType)foeFiles[i].foeTypeIndex;
            nuFoe.level = foeFiles[i].chapter;
            nuFoe.SetBaseHP(foeFiles[i].hp);
            nuFoe.colorChoice = new Color(foeFiles[i].colorRed, foeFiles[i].colorGreen, foeFiles[i].colorBlue);
            nuFoe.GiveID(foeFiles[i].foeID);
            nuFoe.GiveGraphicPieceID(foeFiles[i].graphicPieceID);

            nuFoe.classIndex = foeFiles[i].classIndex;

            nuFoe.lastModified = DateTime.FromBinary(foeFiles[i].lastModified);

            loadedFoes.Add(nuFoe);
        }

        UnitManager._instance.ReceiveLoadedFoes(loadedFoes);
        //Debug.Log("characters loaded");
    }

    public FoeFile LoadFoeFile(string fileName)
    {
        string unitPath = Application.persistentDataPath + "/" + GetFoeFolder() + "/" + fileName + ".iconunits";
        FileStream file;

        if (File.Exists(unitPath)) file = File.OpenRead(unitPath);
        else
        {
            Debug.LogError("File not found");
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FoeFile data = (FoeFile)bf.Deserialize(file);
        file.Close();

        return data;
    }
    #endregion

    #region Delete Foes Mechanic
    public void DeleteFoe(int id)
    {
        // delete character
        string hexID = id.ToString("X6");

        string unitPath = Application.persistentDataPath + "/" + GetFoeFolder() + "/foe_" + hexID + ".iconunits";

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetFoeFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetFoeFolder());

        if (File.Exists(unitPath))
            File.Delete(unitPath);

        /*
        //rearrange ids
        List<IconFoe> allFoes = UnitManager._instance.GetFoes();
        int counter = 0;
        int deleteIndex = -1;
        for (int i = 0; i < allFoes.Count; i++)
        {
            if (allFoes[i].unitID == id)
            {
                deleteIndex = i;
                continue;
            }

            IconFoe aux = allFoes[i];
            aux.GiveID(counter);
            allFoes[i] = aux;
            counter++;
        }

        allFoes.RemoveAt(deleteIndex);

        for (int i = 0; i < allFoes.Count; i++)
        {
            SaveFoe(allFoes[i]);
        }

        //delete last file (surplus)
        unitPath = Application.persistentDataPath + "/" + foeFolder + "/foe_" + allFoes.Count.ToString() + ".iconunits";

        if (File.Exists(unitPath))
            File.Delete(unitPath);
            */
    }
    #endregion

    #region Foe Data Managing
    private FoeFile GetFoeData(int ID)
    {
        IconFoe source = UnitManager._instance.GetFoe(ID);
        if (source == null)
            return null;

        return GetFoeData(source);
    }

    private FoeFile GetFoeData(IconFoe source)
    {
        FoeFile nuFile = new FoeFile();
        nuFile.GiveUnitID(source.unitID);
        nuFile.GiveGeneralAspects(source.unitName, source.level, source.baseHP, (int)source.type, source.colorChoice);
        nuFile.GivePiecePartIDs(source.graphicImageID);
        nuFile.GiveTacticalAspects(source.classIndex);
        nuFile.SetLastModification(source.lastModified);

        return nuFile;
    }
    #endregion


    #region Save Pieces Mechanic
    public void SaveMapPieces(string fileName)
    {
        string piecePath = Application.persistentDataPath + "/" + GetMapFolder() + "/" + fileName + ".iconpieces";
        FileStream file;

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetMapFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetMapFolder());

        if (File.Exists(piecePath)) file = File.OpenWrite(piecePath);
        else file = File.Create(piecePath);

        PieceFile pf = new PieceFile();
        pf = GetPieceData(pf);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, pf);
        file.Close();
    }
    #endregion

    #region Load Pieces Mechanic
    public void LoadMapPieces(string fileName)
    {
        string piecePath = Application.persistentDataPath + "/" + GetMapFolder() + "/" + fileName + ".iconpieces";
        FileStream file;

        if (File.Exists(piecePath)) file = File.OpenRead(piecePath);
        else
        {
            Debug.LogError("File not found");
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        PieceFile data = (PieceFile)bf.Deserialize(file);
        file.Close();

        //use data to castpiece
        PieceManager._instance.GiveLoadedPieces(data);
    }
    #endregion

    #region Piece Data Managing
    private PieceFile GetPieceData(PieceFile pf)
    {
        List<UnitPiece> castedPieces = PieceManager._instance.GetPieceList();
        pf.GivePieces(castedPieces);
        return pf;
    }
    #endregion

    private void MakeFileList(bool save)
    {
        newSaveButton.SetActive(save);

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetMapFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetMapFolder());

        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        string[] fileNames = new string[0];
        if (alphaSorting)
        {
            fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + GetMapFolder(), "*.iconmap");

            sortButton.sprite = sortAZIcon;
        }
        else
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/" + GetMapFolder());
            FileInfo[] files = info.GetFiles("*.iconmap").OrderBy(p => p.LastAccessTime).ToArray();
            fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = files[i].FullName;
            }

            sortButton.sprite = sortTimeIcon;
        }

        RectTransform prefabEntryRT = fileEntryPrefab.GetComponent<RectTransform>();
        entrySeparation = newSaveButton.GetComponent<RectTransform>().anchoredPosition.y;
        entryHeight = prefabEntryRT.sizeDelta.y;

        //clear list
        for(int i = fileContent.childCount - 1; i >= 2; i--)
        {
            Destroy(fileContent.GetChild(i).gameObject);
        }

        float yPOs = entrySeparation;
        if (save)
            yPOs += entrySeparation + (-1f * entryHeight);

        if (!alphaSorting)
        {
            for (int i = fileNames.Length - 1; i >= 0; i--)
            {
                GameObject nuEntry = Instantiate<GameObject>(fileEntryPrefab, fileContent);
                RectTransform entryRT = nuEntry.GetComponent<RectTransform>();
                Vector3 rPos = prefabEntryRT.localPosition;
                rPos.y = yPOs;
                entryRT.localPosition = rPos;

                string simpleName = fileNames[i].Substring(fileNames[i].LastIndexOf('\\') + 1);
                simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

                entryRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = simpleName;
                int index = i;
                entryRT.GetComponent<HoldButton>().onRelease.AddListener(delegate { OpenOldFilePanel(index, save); });

                nuEntry.SetActive(true);

                yPOs += entrySeparation - entryHeight;
            }
        }
        else
        {
            for (int i = 0; i < fileNames.Length; i++)
            {
                GameObject nuEntry = Instantiate<GameObject>(fileEntryPrefab, fileContent);
                RectTransform entryRT = nuEntry.GetComponent<RectTransform>();
                Vector3 rPos = prefabEntryRT.localPosition;
                rPos.y = yPOs;
                entryRT.localPosition = rPos;

                string simpleName = fileNames[i].Substring(fileNames[i].LastIndexOf('\\') + 1);
                simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

                entryRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = simpleName;
                int index = i;
                entryRT.GetComponent<HoldButton>().onRelease.AddListener(delegate { OpenOldFilePanel(index, save); });

                nuEntry.SetActive(true);

                yPOs += entrySeparation - entryHeight;
            }
        }

        Vector2 sd = fileContent.sizeDelta;
        sd.y = -1f * yPOs;
        fileContent.sizeDelta = sd;

        fileScroll.SetActive(true);
    }

    public void ToggleFileSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        if (alphaSorting)
            PlayerPrefs.SetInt("menuLoadSort", 1);
        else
            PlayerPrefs.SetInt("menuLoadSort", 0);

        MakeFileList(newSaveButton.activeInHierarchy);
    }

    private void OpenOldFilePanel(int fileIndex, bool save)
    {
        chosenFileIndex = fileIndex;
        toSave = save;

        if (toSave)
            oldFilePanelSave.gameObject.SetActive(true);
        else
            oldFilePanelLoad.gameObject.SetActive(true);
    }

    public void ChooseOldFile()
    {
        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        string[] fileNames = new string[0];
        if (alphaSorting)
        {
            fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + GetMapFolder(), "*.iconmap");
        }
        else
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/" + GetMapFolder());
            FileInfo[] files = info.GetFiles("*.iconmap").OrderBy(p => p.LastAccessTime).ToArray();
            fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = files[i].FullName;
            }
        }

        string simpleName = fileNames[chosenFileIndex].Substring(fileNames[chosenFileIndex].LastIndexOf('\\') + 1);
        simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

        if (toSave)
        {
            SaveMap(simpleName);
            SaveMapPieces(simpleName);
            oldFilePanelSave.gameObject.SetActive(false);
        }
        else
        {
            LoadMap(simpleName);
            LoadMapPieces(simpleName);
            MapManager._instance.MapAfterLoad();
            oldFilePanelLoad.gameObject.SetActive(false);
        }

        ToggleOptions();
    }

    public void NewFile()
    {
        currentlyLoadedMap = "";

        newFilePanel.gameObject.SetActive(true);
    }

    public static bool CheckValidFileName(string fileName)
    {
        char[] invalidFileChars = new char[10] { '<', '>', ':', '\"', '/', '\\', '|', '?', '*', ' ' };

        for (int c = 0; c < invalidFileChars.Length; c++)
        {
            if (fileName.Contains(invalidFileChars[c].ToString()))
                return false;
        }

        return true;
    }

    public void UpdateShowTerrainTiles()
    {
        MapManager._instance.showTerrainTiles = showTiles.isOn;
        if(MapManager._instance.mapTarget != null && MapManager._instance.toolMode != MapManager.ToolMode.TerrainEditor)
            MapManager._instance.mapTarget.ShowTiles(MapManager._instance.showTerrainTiles);

        PlayerPrefs.SetInt("ShowHeightTiles", showTiles.isOn ? 1 : 0);
    }

    public void UpdateShowSpecialTerrain()
    {
        MapManager._instance.showTileTypes = showSpecialTerrain.isOn;
        if (MapManager._instance.mapTarget != null && MapManager._instance.toolMode != MapManager.ToolMode.TerrainEditor)
            MapManager._instance.mapTarget.ShowTerrainTypes(MapManager._instance.showTileTypes);

        PlayerPrefs.SetInt("ShowTerrainTypeTiles", showSpecialTerrain.isOn ? 1 : 0);
    }

    public void UpdateScreenSize(int choice)
    {
        for(int i = 0; i < resOptions.Length; i++)
        {
            resOptions[i].toggleOption.SetIsOnWithoutNotify(i == choice);
        }

        ResolutionOption resOp = resOptions[choice];

        PlayerPrefs.SetInt("CurrentScreenResolution", choice);

        Screen.SetResolution(resOp.width, resOp.height, false);
    }

    public void UpdateInvertX()
    {
        MapManager._instance.SetInvertX(cameraInvertX.isOn);

        PlayerPrefs.SetInt("CameraInvertX", cameraInvertX.isOn ? 1 : 0);
    }

    public void UpdateInvertY()
    {
        MapManager._instance.SetInvertY(cameraInvertY.isOn);

        PlayerPrefs.SetInt("CameraInvertY", cameraInvertY.isOn ? 1 : 0);
    }

    public void UpdateShowGrid()
    {
        GameModeManager._instance.ChangeGridVisibility(showGrid.isOn);
    }

    public void UpdateTileStyleCall()
    {
        tileStylePanel.screenProportionSize = tileStylePanelProportions;

        RectTransform tileStyleButtonRT = tileStyleLabel.transform.parent.GetComponent<RectTransform>();
        tileStylePanel.listColor = 0.9f * tileStyleButtonRT.GetComponent<Image>().color;

        Vector3 listOrigin = tileStyleButtonRT.position + (0.5f * tileStyleButtonRT.rect.size.x * tileStyleButtonRT.lossyScale.x * Vector3.right);
        List<string> tileStyleTypes = new List<string>();
        for (int i = 0; i < MapManager._instance.tileStyles.Length; i++)
        {
            tileStyleTypes.Add(MiscTools.GetSpacedForm(MapManager._instance.tileStyles[i].style.ToString()));
        }

        tileStylePanel.ShowPanel(listOrigin, tileStyleTypes, true);
        tileStyleListOpen = true;
        tileStylePanel.OnEntryClick += UpdateTileStyle;

        tileStyleListRT = tileStylePanel.GetComponent<RectTransform>();
    }

    private void UpdateTileStyle(int choice)
    {
        if (tileStyleListOpen)
        {
            tileStyleLabel.text = MiscTools.GetSpacedForm(((MapManager.TileStyles)choice).ToString());
            tileStylePanel.ShowPanel(false);
            tileStylePanel.OnEntryClick -= UpdateTileStyle;
            tileStyleListOpen = false;

            MapManager._instance.ChangeTileStyle(choice, false);

            //save pref
            PlayerPrefs.SetInt("TileStyle", choice);
        }
    }

    public void UpdatePieceAutoSizeOption()
    {
        PieceManager._instance.SetPieceAutoSize(pieceAutoSize.isOn);

        PlayerPrefs.SetInt("PieceAutoSize", pieceAutoSize.isOn ? 1 : 0);
    }

    public void UpdateMarkerAppearance(int choice)
    {
        markerAdaptToTerrain.SetIsOnWithoutNotify(choice == 0);
        markerAdaptToTile.SetIsOnWithoutNotify(choice != 0);

        ColorManager._instance.UpdateMarkerTerrainAppearance(choice == 0);

        PlayerPrefs.SetInt("MarkerTileAppearance", choice);
    }

    public void SaveMiniPanelSetting(int index)
    {
        PlayerPrefs.SetInt("ShowMiniPanel", index);
    }

    public void SaveMarkShowSetting(int index)
    {
        PlayerPrefs.SetInt("ShowMarks", index);
    }

    public void LoadSettings()
    {
        int savedTileStyle = PlayerPrefs.GetInt("TileStyle", 0);
        tileStyleLabel.text = MiscTools.GetSpacedForm(((MapManager.TileStyles)savedTileStyle).ToString());
        //MapManager._instance.ChangeTileStyle(savedTileStyle, false);

        MapManager._instance.ChangeColorStyle(PlayerPrefs.GetInt("CheckeredColor", 0) == 1, PlayerPrefs.GetInt("HeightColor", 0) == 1);
        

        showTiles.isOn = PlayerPrefs.GetInt("ShowHeightTiles", 0) == 1;
        showSpecialTerrain.isOn = PlayerPrefs.GetInt("ShowTerrainTypeTiles", 0) == 1;
        showGrid.isOn = PlayerPrefs.GetInt("GridVisible", 0) == 1;

        for (int i = 0; i < resOptions.Length; i++)
        {
            resOptions[i].toggleLabel.text = resOptions[i].width + " x " + resOptions[i].height;
        }

        UpdateScreenSize(PlayerPrefs.GetInt("CurrentScreenResolution", 2));

        cameraInvertX.isOn = PlayerPrefs.GetInt("CameraInvertX", 0) == 1;
        cameraInvertY.isOn = PlayerPrefs.GetInt("CameraInvertY", 0) == 1;

        pieceAutoSize.isOn = PlayerPrefs.GetInt("PieceAutoSize", 1) == 1;

        PieceManager._instance.SetPanelDisplayMode(1);

        bool markerToTerrain = PlayerPrefs.GetInt("MarkerTileAppearance", 0) == 0;

        markerAdaptToTerrain.isOn = markerToTerrain;
        markerAdaptToTile.isOn = !markerToTerrain;

        ColorManager._instance.UpdateMarkerTerrainAppearance(markerToTerrain);
    }
}

[System.Serializable]
public class IndexFile
{
    public int unitIndex { get; private set; }
    public int imageIndex { get; private set; }

    public void SetUnitIndex(int value)
    {
        unitIndex = value;
    }

    public void SetImageIndex(int value)
    {
        imageIndex = value;
    }
}

[System.Serializable]
public class MapFile
{
    public int mapSizeX { get; private set; }
    public int mapSizeZ { get; private set; }

    public float mapCellScale { get; private set; }

    public int mapTerrainDetail { get; private set; }

    public float[] yMap { get; private set; }

    public bool mapIsTileMode { get; private set; }

    private float mapColorR = -1;
    public float MapColorR => mapColorR;

    private float mapColorG = -1;
    public float MapColorG => mapColorG;

    private float mapColorB = -1;
    public float MapColorB => mapColorB;

    public int[] tileMap { get; private set; }

    public int[] typeMap { get; private set; }

    public int[] coloredMarkedX { get; private set; }
    public int[] coloredMarkedZ { get; private set; }
    public float[] colorMarkR { get; private set; }
    public float[] colorMarkG { get; private set; }
    public float[] colorMarkB { get; private set; }

    public void GiveMapSize(int x, int z)
    {
        mapSizeX = x;
        mapSizeZ = z;
    }

    public void GiveMapCellScale(float scale)
    {
        mapCellScale = scale;
    }

    public void GiveTerrainDetail(int detail)
    {
        mapTerrainDetail = detail;
    }

    public void GiveMapDetails(bool tileMode, Color mapColor)
    {
        mapIsTileMode = tileMode;
        mapColorR = mapColor.r;
        mapColorG = mapColor.g;
        mapColorB = mapColor.b;
    }

    public void GiveMapHeights(float[] meshHeights)
    {
        yMap = meshHeights;
    }

    public void GiveTileHeights(int[] tileHeight)
    {
        tileMap = tileHeight;
    }

    public void GiveTileTypes(int[] typing)
    {
        typeMap = typing;
    }

    public void GiveColoredMarkTiles(int[] colX, int[] colZ, float[] colrR, float[] colrG, float[] colrB)
    {
        coloredMarkedX = colX;
        coloredMarkedZ = colZ;
        colorMarkR = colrR;
        colorMarkG = colrG;
        colorMarkB = colrB;
    }

    public void ShowHeights()
    {
        string str = "[";
        for(int i = 0; i < yMap.Length; i++)
        {
            str += yMap[i];
            if (i < yMap.Length - 1)
                str += ",";
        }
        str += "]";

        Debug.Log(str);
    }

    public SaveMapShape[] shapeList;

    [System.Serializable]
    public struct SaveMapShape
    {
        public float posX;
        public float posY;
        public float posZ;
        
        public float rotX;
        public float rotY;
        public float rotZ;
        
        public float scaX;
        public float scaY;
        public float scaZ;

        public float colorRed;
        public float colorGreen;
        public float colorBlue;

        public int shapeID;
        public bool isDecor;
    }

    public void GiveShapeList(ShapeInfo[] shapes)
    {
        shapeList = new SaveMapShape[shapes.Length];

        for(int i = 0; i < shapes.Length; i++)
        {
            SaveMapShape sms = new SaveMapShape();
            Vector3 position = shapes[i].transform.position;
            sms.posX = position.x;
            sms.posY = position.y;
            sms.posZ = position.z;
            Vector3 rotation = shapes[i].transform.rotation.eulerAngles;
            sms.rotX = rotation.x;
            sms.rotY = rotation.y;
            sms.rotZ = rotation.z;
            Vector3 scale = shapes[i].transform.localScale;
            sms.scaX = scale.x;
            sms.scaY = scale.y;
            sms.scaZ = scale.z;

            sms.shapeID = shapes[i].shapeID;
            Color shapeColor = shapes[i].shapeColor;
            sms.colorRed = shapeColor.r;
            sms.colorGreen = shapeColor.g;
            sms.colorBlue = shapeColor.b;
            sms.isDecor = shapes[i].isDecor;
            shapeList[i] = sms;
        }
    }
}

[System.Serializable]
public class PieceFile
{
    public SaveMapPiece[] pieces;

    public SaveMark[] marks;

    [System.Serializable]
    public struct SaveMapPiece
    {
        public float posX;
        public float posY;
        public float posZ;
        public int pieceSize;
        public bool freshFlag;
        public bool onMap;

        public float colorR;
        public float colorG;
        public float colorB;

        public int pieceID;
        public bool textHPFlag;
        public string textHP;
        public bool textArmorFlag;
        public string textArmor;

        public int pieceArmor;

        public int pieceHeadPartID;
        public int pieceBodyPartID;
        public int pieceLWeaponPartID;
        public int pieceRWeaponPartID;
        public float pieceRotation;

        public string pieceGraphicID;

        public int pieceHP;
        public int pieceCurrentHP;
        public int pieceAddedHP;

        public int pieceWoundCount;

        public int pieceCurrentVigor;
        public int pieceVigor;

        public int blessingCount;

        //lists will be stored in string format
        public string pieceActiveBlight;
        public string pieceActiveStatus;
        public string pieceActivePositiveEffect;
        public string pieceMarks;

        public int pieceElixirCount;
        public bool[] pieceElixirState;

        public string tokenMessage;
    }

    [System.Serializable]
    public struct SaveMark
    {
        public bool isPieceMark;
        public float sourceX;
        public float sourceY;
        public float sourceZ;
        public float targetX;
        public float targetY;
        public float targetZ;
        public string markName;

        public int heightIndex;
    }

    //load piece list and fill file. need the map name too.
    public void GivePieces(List<UnitPiece> pieces)
    {
        SaveMapPiece[] savePieces = new SaveMapPiece[pieces.Count];

        for(int i = 0; i < pieces.Count; i++)
        {
            SaveMapPiece pees = new SaveMapPiece();

            pees.onMap = pieces[i].onMap;
            if (pieces[i].onMap)
            {
                pees.posX = pieces[i].mapPosition.x;
                pees.posZ = pieces[i].mapPosition.y;
            }
            else
            {
                pees.posX = pieces[i].lastAcceptedPosition.x;
                pees.posY = pieces[i].lastAcceptedPosition.y;
                pees.posZ = pieces[i].lastAcceptedPosition.z;
            }

            Color pieceColor = pieces[i].GetPieceColor();
            pees.colorR = pieceColor.r;
            pees.colorG = pieceColor.g;
            pees.colorB = pieceColor.b;

            pees.pieceSize = pieces[i].pieceSize;

            pees.pieceHeadPartID = pieces[i].headPartId;
            pees.pieceBodyPartID = pieces[i].bodyPartId;
            pees.pieceLWeaponPartID = pieces[i].weaponLPartId;
            pees.pieceRWeaponPartID = pieces[i].weaponRPartId;
            pees.pieceRotation = pieces[i].modelRotation;

            pees.pieceGraphicID = pieces[i].pieceImageID;

            if(pieces[i] is CharacterPiece)
            {
                EsperCharacter iconChara = (pieces[i] as CharacterPiece).characterData;
                pees.freshFlag = iconChara.freshFlag;
                pees.pieceID = iconChara.unitID;
                pees.pieceHP = iconChara.baseHP;
                pees.pieceCurrentHP = iconChara.currentHP;
                pees.pieceAddedHP = iconChara.addedHP;

                pees.pieceActiveBlight = PassToString(iconChara.activeBlights);
                pees.pieceActiveStatus = PassToString(iconChara.activeStatus);
                pees.pieceActivePositiveEffect = PassToString(iconChara.activePositiveEffects);
            }
            else if(pieces[i] is FoePiece)
            {
                IconFoe iconFoe = (pieces[i] as FoePiece).foeData;
                pees.freshFlag = iconFoe.freshFlag;
                pees.pieceID = iconFoe.unitID;
                pees.pieceHP = iconFoe.baseHP;
                pees.pieceCurrentHP = iconFoe.currentHP;
                pees.pieceAddedHP = iconFoe.addedHP;

                pees.pieceActiveBlight = PassToString(iconFoe.activeBlights);
                pees.pieceActiveStatus = PassToString(iconFoe.activeStatus);
                pees.pieceActivePositiveEffect = PassToString(iconFoe.activePositiveEffects);
            }
            else if (pieces[i] is TokenPiece)
            {
                TokenPiece piece = pieces[i] as TokenPiece;
                pees.pieceID = -1;
                pees.tokenMessage = piece.tokenMessage;
            }

            savePieces[i] = pees;
        }

        this.pieces = savePieces;
    }

    public string PassToString(List<IconUnit.Blight> blights)
    {
        string res = "";
        for(int i = 0; i < blights.Count; i++)
        {
            res += ((int)blights[i]).ToString();
            if (i < (blights.Count - 1))
                res += ".";
        }
        return res;
    }

    //returns the blight list of the mappiece requested, if there is one
    public List<IconUnit.Blight> PassToBlightList(int index)
    {
        if (index < 0 || index > (pieces.Length - 1))
            return new List<IconUnit.Blight>();

        return PassToBlightList(pieces[index].pieceActiveBlight);
    }

    public List<IconUnit.Blight> PassToBlightList(string text)
    {
        List<IconUnit.Blight> blightList = new List<IconUnit.Blight>();
        string aux = text;
        while (true)
        {
            int pointIndex = aux.IndexOf(".");
            if(pointIndex < 0)
            {
                int parsed = -1;
                if(int.TryParse(aux, out parsed))
                    blightList.Add((IconUnit.Blight)parsed);
                break;
            }
            else
            {
                int parsed = int.Parse(aux.Substring(0, pointIndex));

                aux = aux.Substring(pointIndex + 1);

                blightList.Add((IconUnit.Blight)parsed);
            }
        }

        return blightList;
    }

    public string PassToString(List<IconUnit.Status> status)
    {
        string res = "";
        for (int i = 0; i < status.Count; i++)
        {
            res += ((int)status[i]).ToString();
            if (i < (status.Count - 1))
                res += ".";
        }
        return res;
    }

    //returns the status list of the mappiece requested, if there is one
    public List<IconUnit.Status> PassToStatusList(int index)
    {
        if (index < 0 || index > (pieces.Length - 1))
            return new List<IconUnit.Status>();

        return PassToStatusList(pieces[index].pieceActiveStatus);
    }

    public List<IconUnit.Status> PassToStatusList(string text)
    {
        List<IconUnit.Status> statusList = new List<IconUnit.Status>();
        string aux = text;
        while (true)
        {
            int pointIndex = aux.IndexOf(".");
            if (pointIndex < 0)
            {
                int parsed = -1;
                if (int.TryParse(aux, out parsed))
                    statusList.Add((IconUnit.Status)parsed);

                break;
            }
            else
            {
                int parsed = int.Parse(aux.Substring(0, pointIndex));

                aux = aux.Substring(pointIndex + 1);

                statusList.Add((IconUnit.Status)parsed);
            }
        }

        return statusList;
    }

    public string PassToString(List<IconUnit.PositiveEffects> effects)
    {
        string res = "";
        for (int i = 0; i < effects.Count; i++)
        {
            res += ((int)effects[i]).ToString();
            if (i < (effects.Count - 1))
                res += ".";
        }
        return res;
    }

    //returns the status list of the mappiece requested, if there is one
    public List<IconUnit.PositiveEffects> PassToEffectList(int index)
    {
        if (index < 0 || index > (pieces.Length - 1))
            return new List<IconUnit.PositiveEffects>();

        return PassToEffectList(pieces[index].pieceActivePositiveEffect);
    }

    public List<IconUnit.PositiveEffects> PassToEffectList(string text)
    {
        List<IconUnit.PositiveEffects> effectList = new List<IconUnit.PositiveEffects>();
        string aux = text;
        while (true)
        {
            int pointIndex = aux.IndexOf(".");
            if (pointIndex < 0)
            {
                int parsed = -1;
                if (int.TryParse(aux, out parsed))
                    effectList.Add((IconUnit.PositiveEffects)parsed);

                break;
            }
            else
            {
                int parsed = int.Parse(aux.Substring(0, pointIndex));

                aux = aux.Substring(pointIndex + 1);

                effectList.Add((IconUnit.PositiveEffects)parsed);
            }
        }

        return effectList;
    }

    public void GiveMarks(List<PlayMark> marks)
    {
        SaveMark[] saveMarks = new SaveMark[marks.Count];

        for (int i = 0; i < marks.Count; i++)
        {
            SaveMark nuMark = new SaveMark();
            nuMark.markName = marks[i].markName;
            nuMark.isPieceMark = (marks[i].type == PlayMark.MarkType.PieceMark);
            nuMark.sourceX = marks[i].sourcePiece.transform.position.x;
            nuMark.sourceZ = marks[i].sourcePiece.transform.position.z;

            if (marks[i].type == PlayMark.MarkType.SpaceMark)
            {
                nuMark.targetX = marks[i].targetSpace.x;
                nuMark.targetY = marks[i].targetSpace.y;
                nuMark.targetZ = marks[i].targetSpace.z;
            }
            else
            {
                nuMark.targetX = marks[i].targetPiece.transform.position.x;
                nuMark.targetY = marks[i].targetPiece.transform.position.y;
                nuMark.targetZ = marks[i].targetPiece.transform.position.z;
            }

            //nuMark.boundType = (int)marks[i].boundType;
            nuMark.heightIndex = marks[i].heightIndex;

            saveMarks[i] = nuMark;
        }

        this.marks = saveMarks;
    }
}

[System.Serializable]
public class CharaFile
{
    public int charaID { get; private set; }

    public string name { get; private set; }
    public int level { get; private set; }

    public int hp { get; private set; }
    public int elixirCount { get; private set; }
    public int size { get; private set; }
    public int armor { get; private set; }

    public float colorRed { get; private set; }
    public float colorGreen { get; private set; }
    public float colorBlue { get; private set; }

    public string graphicPieceID { get; private set; }

    public int headPartID { get; private set; }
    public int bodyPartID { get; private set; }
    public int lWeaponPartID { get; private set; }
    public int rWeaponPartID { get; private set; }

    public int relatedImageID { get; private set; }

    public int kinIndex { get; private set; }
    public int classIndex { get; private set; }
    public int firstJobIndex { get; private set; }
    public int secondJobIndex { get; private set; }
    public int thirdJobIndex { get; private set; }

    public int cultureIndex { get; private set; }
    public int bondIndex { get; private set; }
    public int startActionIndex { get; private set; }

    public int[] dotModifiers { get; private set; }

    public long lastModified { get; private set; }

    public CharaFile()
    {
        firstJobIndex = -1;
        secondJobIndex = -1;
        thirdJobIndex = -1;
    }

    public void GiveUnitID(int ID)
    {
        charaID = ID;
    }

    public void GiveGeneralAspects(string name, int level, int hp, Color choiceColor)
    {
        this.name = name;
        this.level = level;
        this.hp = hp;
        colorRed = choiceColor.r;
        colorBlue = choiceColor.b;
        colorGreen = choiceColor.g;
    }

    public void GivePiecePartIDs(string graphicPieceID)
    {
        this.graphicPieceID = graphicPieceID;
    }

    public void GiveTacticalAspects(int classIndex, int[] skillsID)
    {
        this.classIndex = classIndex;
    }

    public void SetLastModification(DateTime lastMod)
    {
        lastModified = lastMod.ToBinary();
    }
}

[System.Serializable]
public class FoeFile
{
    public int foeID { get; private set; }

    public string name { get; private set; }
    public int chapter { get; private set; }

    public int hp { get; private set; }
    public int size { get; private set; }
    public int armor { get; private set; }

    public float colorRed { get; private set; }
    public float colorGreen { get; private set; }
    public float colorBlue { get; private set; }

    public string graphicPieceID { get; private set; }

    public int headPartID { get; private set; }
    public int bodyPartID { get; private set; }
    public int lWeaponPartID { get; private set; }
    public int rWeaponPartID { get; private set; }

    public int foeTypeIndex { get; private set; }

    public int classIndex { get; private set; }
    public int jobIndex { get; private set; }

    public int factionIndex { get; private set; }

    public int subFactionIndex { get; private set; }

    public int templateIndex { get; private set; }

    public int hasSubTemplate { get; private set; }

    public bool isDefaultFactionEntry { get; private set; }

    public int imageID { get; private set; }

    public long lastModified { get; private set; }

    public void GiveUnitID(int ID)
    {
        foeID = ID;
    }

    public void GiveGeneralAspects(string name, int chapter, int hp, int foeType, Color choiceColor)
    {
        this.name = name;
        this.chapter = chapter;
        this.hp = hp;

        foeTypeIndex = foeType;

        colorRed = choiceColor.r;
        colorBlue = choiceColor.b;
        colorGreen = choiceColor.g;
    }

    public void GivePiecePartIDs(string graphicPieceID)
    {
        this.graphicPieceID = graphicPieceID;
    }

    public void GiveTacticalAspects(int classIndex)
    {
        this.classIndex = classIndex;
    }

    public void SetLastModification(DateTime lastMod)
    {
        lastModified = lastMod.ToBinary();
    }
}
