using System.IO;
using System.IO.Compression;

using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Linq;
using System;
using AnotherFileBrowser.Windows;
using Random = System.Random;

public class MenuManager : MonoBehaviour
{
    private enum MenuState
    {
        Start,
        ExpeditionNameInput,
        MapNameInput,
        ExpeditionScreen
    }

    private MenuState currentState;

    public MapManager mapManager;

    public OptionsManager optionsManager;

    [Header("Menu BG")] 
    [SerializeField] private Image menuBGImage;
    [SerializeField] private Sprite[] bgImages;
    
    [Header("Expedition Elements")]
    [SerializeField] private ExpeditionScreen expdScreen;
    [SerializeField] private RectTransform expdList;
    [SerializeField] private Animator expListAnim;

    private bool expListShown = false;
    private GameObject expListEntryPrefab;

    [Space(10f)]
    [SerializeField] private string oldExpeditionName = "Old";

    [Header("Debug Help")]
    public bool circumventEntry = false;
    public bool circumventToLoad = false;
    public int expeditionCircumventIndex = 0;
    public int levelCircumventIndex = 0;

    [Header("Old Menu Functions")]
    [Space(10f)]
    public Animator buttonsAnim;
    [Space(10f)] //new map options
    public TMP_InputField cellXInput;
    public TMP_InputField cellYInput;
    public int cellRangeMin = 5;
    public int cellRangeMax = 200;
    public CanvasGroup smallMapButton;
    public GameObject smallMapOptions;
    public CanvasGroup mediumMapButton;
    public GameObject mediumMapOptions;
    public CanvasGroup largeMapButton;
    public GameObject largeMapOptions;
    public CanvasGroup customMapButton;
    public GameObject customMapOptions;
    [Space(5f)]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private CanvasGroup confirmNameButton;
    [SerializeField] private GameObject confirmNamePanel;
    [SerializeField] private GameObject expExistsWarning;
    private int sizeSelectedX;
    private int sizeSelectedZ;

    [Space(10f)] //load map options
    public RectTransform loadMapPanel;
    public GameObject fileScroll;
    public RectTransform fileContent;
    public GameObject fileEntryPrefab;
    public RectTransform oldFilePanelLoad;
    private int chosenFileIndex;
    [Space(5f)]
    public Image sortButton;
    public Sprite sortAZIcon;
    public Sprite sortTimeIcon;

    [Space(5f)]//delete map
    [SerializeField] private GameObject mapDeleteConfirmPanel;
    private int fileIndexWaitingDeletion;
    private string fileToDeletePath;

    private MenuMode currentMode = MenuMode.Options;

    [Space(30f)]
    public TextMeshProUGUI versionLabel;

    public bool IsMenuActive { get { return GetComponent<CanvasGroup>().interactable; } }

    private enum MenuMode
    {
        Options,
        NewMap,
        LoadMap,
        ExitMap
    }

    private void Start()
    {
        versionLabel.text = "v " + Application.version;

        nameInput.onValueChanged.AddListener(delegate { UpdateConfirmButton(); });

        currentState = MenuState.Start;

        SetMenuImage();
        
        ExpListSetup();

        if (circumventEntry)
        {
            if (circumventToLoad)
            {
                string[] dirList = Directory.GetDirectories(Application.persistentDataPath + "/Expeditions");

                string expName = Path.GetFileName(dirList[expeditionCircumventIndex]);
                optionsManager.SetNewExpRoot(expName);

                ChooseOldFile(levelCircumventIndex);
            }
            else
                MakeNewMap(14, 14);
        }

    }

    private void SetMenuImage()
    {
        int idx = UnityEngine.Random.Range(0, bgImages.Length);

        menuBGImage.sprite = bgImages[idx];
    }

    #region Expedition Menu

    private void ExpListSetup()
    {
        SetExpList(false);

        BuildExpeditionList();
    }

    private void SetExpList(bool show)
    {
        expListAnim.SetTrigger(show? "SetShown" : "SetHidden");
        expListShown = show;
    }
    
    public void ToggleShowExpList()
    {
        expListShown = !expListShown;
        if (expListShown)
            expListAnim.SetTrigger("Show");
        else
            expListAnim.SetTrigger("Hide");
    }

    private void BuildExpeditionList()
    {
        CheckFolderSetup();

        expListEntryPrefab = expdList.GetChild(2).gameObject;

        for(int i = expdList.childCount - 1; i > 2; i--)
        {
            Destroy(expdList.GetChild(i).gameObject);
        }

        //we check the expeditions and put them in the list
        string[] dirList = Directory.GetDirectories(Application.persistentDataPath + "/Expeditions");

        for(int i = 0; i < dirList.Length; i++)
        {
            string expName = Path.GetFileName(dirList[i]);
            
            GameObject newEntry = Instantiate(expListEntryPrefab, expdList);
            newEntry.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = expName;

            newEntry.GetComponent<HoldButton>().onRelease.AddListener(delegate { LoadExpedition(expName); });

            newEntry.SetActive(true);
        }
    }

    private void CheckFolderSetup()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Expeditions"))
            FixFolderSetup();
    }

    //rearranges the folders to the new file structure
    private void FixFolderSetup()
    {
        string root = Application.persistentDataPath;
        FileStream file;

        Directory.CreateDirectory(Application.persistentDataPath + "/Expeditions");

        //we move the old base folders after checking if there is, in fact, old data there
        if (Directory.Exists(root + "/FoeData"))
        {
            Directory.CreateDirectory(root + "/Expeditions/" + oldExpeditionName + "");

            Directory.Move(root + "/FoeData", root + "/Expeditions/" + oldExpeditionName + "/FoeData");
        }

        if (Directory.Exists(root + "/GraphicPieces"))
        {
            Directory.Move(root + "/GraphicPieces", root + "/Expeditions/" + oldExpeditionName + "/GraphicPieces");
        }

        if (Directory.Exists(root + "/MapData"))
        {
            Directory.Move(root + "/MapData", root + "/Expeditions/" + oldExpeditionName + "/MapData");
        }

        if (Directory.Exists(root + "/UnitData"))
        {
            Directory.Move(root + "/UnitData", root + "/Expeditions/" + oldExpeditionName + "/UnitData");
        }

        //we create the index file for the old Expedition folder
        IndexFile iFile = new IndexFile();
        int unitIndex = 0;
        int imageIndex = 0;

        //check foe Index
        string checkRoot = root + "/Expeditions/" + oldExpeditionName + "/FoeData";
        string[] foundFiles = Directory.GetFiles(checkRoot);
        for (int i = 0; i < foundFiles.Length; i++)
        {
            string fileName = Path.GetFileName(foundFiles[i]);
            int strIdx = fileName.LastIndexOf("_");
            string hex = fileName.Substring(strIdx + 1, 6);

            int fileID = Convert.ToInt32(hex, 16);
            if (fileID > unitIndex)
                unitIndex = fileID;
        }

        //check unit Index
        checkRoot = root + "/Expeditions/" + oldExpeditionName + "/UnitData";
        foundFiles = Directory.GetFiles(checkRoot);
        for (int i = 0; i < foundFiles.Length; i++)
        {
            string fileName = Path.GetFileName(foundFiles[i]);
            int strIdx = fileName.LastIndexOf("_");
            string hex = fileName.Substring(strIdx + 1, 6);

            int fileID = Convert.ToInt32(hex, 16);
            if (fileID > unitIndex)
                unitIndex = fileID;
        }

        //check image Index
        checkRoot = root + "/Expeditions/" + oldExpeditionName + "/GraphicPieces";
        foundFiles = Directory.GetFiles(checkRoot);
        for (int i = 0; i < foundFiles.Length; i++)
        {
            string fileName = Path.GetFileName(foundFiles[i]);
            int strIdx = fileName.LastIndexOf("_");
            string hex = fileName.Substring(strIdx + 1, 6);

            int fileID = Convert.ToInt32(hex, 16);
            if (fileID > imageIndex)
                imageIndex = fileID;
        }

        iFile.SetUnitIndex(unitIndex);
        iFile.SetImageIndex(imageIndex);

        //save the index file
        string indexFilePath = root + "/Expeditions/" + oldExpeditionName + "/index.iconindex";
        if (File.Exists(indexFilePath)) file = File.OpenWrite(indexFilePath);
        else file = File.Create(indexFilePath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, iFile);
        file.Close();
    }

    public void CreateExpeditionCall()
    {
        currentState = MenuState.ExpeditionNameInput;
        CallNameInputPanel(false);
    }

    //make the folders and index file of the newly created expedition
    private void CreateExpedition(string expeditionName)
    {
        Debug.Log("trying to create expedition named " + expeditionName);

        string root = Application.persistentDataPath + "/Expeditions";

        Directory.CreateDirectory(root + "/" + expeditionName);

        root = root + "/" + expeditionName;

        //we move the old base folders after checking if there is, in fact, old data there
        if (!Directory.Exists(root + "/FoeData"))
        {
            Directory.CreateDirectory(root + "/FoeData");
        }

        if (!Directory.Exists(root + "/GraphicPieces"))
        {
            Directory.CreateDirectory(root + "/GraphicPieces");
        }

        if (!Directory.Exists(root + "/MapData"))
        {
            Directory.CreateDirectory(root + "/MapData");
        }

        if (!Directory.Exists(root + "/UnitData"))
        {
            Directory.CreateDirectory(root + "/UnitData");
        }

        IndexFile iFile = new IndexFile();
        string indexFilePath = root + "/index.iconindex";

        FileStream file;
        if (File.Exists(indexFilePath)) file = File.OpenWrite(indexFilePath);
        else file = File.Create(indexFilePath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, iFile);
        file.Close();

        //we show the expedition screen. maybe we call load after we set the expedition to be loaded as the newly created one?
        LoadExpedition(expeditionName);
    }

    //load the data into the screen, while also leaving loaded the current index file of said expedition
    private void LoadExpedition(string expName)
    {
        optionsManager.SetNewExpRoot(expName);

        currentState = MenuState.ExpeditionScreen;
        expdScreen.LoadExpedition(expName);

        expdScreen.gameObject.SetActive(true);
    }

    public string GetExpeditionsRootFolder()
    {
        return Application.persistentDataPath + "/Expeditions";
    }

    public void ImportExpeditionCall()
    {
        var bp = new BrowserProperties();
        bp.title = "Choose expedition file to import. Expedition will have this file's name";
        bp.filter = "Icon Expeditions (*.iconexp) | *.iconexp";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            ChooseExpeditionFile(path);
        });
    }

    private void ChooseExpeditionFile(string path)
    {
        int nameStart = path.LastIndexOf("\\") + 1;
        int nameEnd = path.LastIndexOf(".");
        string expeditionName = path.Substring(nameStart, nameEnd - nameStart);

        string folderName = expeditionName;
        //check if expedition already exists
        while (true) {
            if (Directory.Exists(GetExpeditionsRootFolder() + "/" + folderName))
            {
                folderName += "_Copy";
            }
            else
                break;
        }

        Directory.CreateDirectory(GetExpeditionsRootFolder() + "/" + folderName);

        ZipFile.ExtractToDirectory(path, GetExpeditionsRootFolder() + "/" + folderName);

        BuildExpeditionList();
    }

    public void ReturnFromExpeditionScreen()
    {
        expdScreen.gameObject.SetActive(false);

        SetMenuImage();
        
        BuildExpeditionList();
        currentState = MenuState.Start;
    }

    #endregion

    public void NewMap()
    {
        if (currentMode != MenuMode.Options)
            return;

        currentMode = MenuMode.NewMap;
        buttonsAnim.SetTrigger("ShowNew");
    }

    public void LoadMap()
    {
        if (currentMode != MenuMode.Options)
            return;

        currentMode = MenuMode.LoadMap;
        buttonsAnim.SetTrigger("ShowLoad");

        MakeFileList();
    }

    public void ExitHelper()
    {
        if (currentMode != MenuMode.Options)
            return;

        currentMode = MenuMode.ExitMap;
        buttonsAnim.SetTrigger("ShowExit");
    }

    public void ReturnAnim()
    {
        if (currentMode == MenuMode.Options)
            return;

        currentMode = MenuMode.Options;
        buttonsAnim.SetTrigger("Hide");
    }

    public void ResetMenu()
    {
        //new map
        smallMapButton.alpha = 1f;
        smallMapOptions.SetActive(false);
        mediumMapButton.alpha = 1f;
        mediumMapOptions.SetActive(false);
        largeMapButton.alpha = 1f;
        largeMapOptions.SetActive(false);
        customMapButton.alpha = 1f;
        customMapOptions.SetActive(false);
    }

    public void ShowMenu(bool show)
    {
        CanvasGroup showGroup = GetComponent<CanvasGroup>();
        showGroup.interactable = show;
        showGroup.blocksRaycasts = show;
        showGroup.alpha = show ? 1f : 0f;
    }

    #region New Map

    public void NewSmallMap()
    {
        MakeNewMap(8, 8);
    }

    public void NewMediumMap()
    {
        MakeNewMap(14, 14);
    }

    public void NewLargeMap()
    {
        MakeNewMap(24, 24);
    }

    public void NewCustomMap()
    {
        ClampInputValues();

        int xSize = int.Parse(cellXInput.text);
        int zSize = int.Parse(cellYInput.text);

        MakeNewMap(xSize, zSize);
    }

    private void CallNameInputPanel(bool namePanel = true)
    {
        string startText = "Choose a name for your map...";
        confirmNameButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Create Map";

        expExistsWarning.SetActive(false);

        if (!namePanel)
        {
            startText = "Choose a name for your expedition...";
            confirmNameButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Create Expedition";
        }

        nameInput.placeholder.GetComponent<TextMeshProUGUI>().text = startText;

        nameInput.SetTextWithoutNotify("");
        confirmNamePanel.SetActive(true);
        UpdateConfirmButton();
    }

    public void CancelNameInput()
    {
        nameInput.SetTextWithoutNotify("");
        confirmNameButton.alpha = 0.2f;
        confirmNameButton.interactable = false;
        confirmNamePanel.SetActive(false);

        if (currentState == MenuState.ExpeditionNameInput)
            currentState = MenuState.Start;
    }

    private void UpdateConfirmButton()
    {
        if(nameInput.text.Length == 0)
        {
            confirmNameButton.alpha = 0.2f;
            confirmNameButton.interactable = false;
            confirmNameButton.GetComponent<HoldButton>().enabled = false;
            return;
        }

        if(!confirmNameButton.interactable)
            confirmNameButton.GetComponent<HoldButton>().enabled = true;

        expExistsWarning.SetActive(false);

        confirmNameButton.alpha = 1f;
        confirmNameButton.interactable = true;
    }

    public void NamePanelConfirm()
    {
        if (currentState == MenuState.ExpeditionNameInput)
        {
            //check if expedition exists
            string inputName = nameInput.text;

            bool clearPass = true;

            string[] dirList = Directory.GetDirectories(Application.persistentDataPath + "/Expeditions");

            for (int i = 0; i < dirList.Length; i++)
            {
                string expName = Path.GetFileName(dirList[i]);

                if (expName.Equals(inputName))
                {
                    clearPass = false;
                    break;
                }
            }

            if (!clearPass)
            {
                nameInput.SetTextWithoutNotify("");
                confirmNameButton.alpha = 0.2f;
                confirmNameButton.interactable = false;

                expExistsWarning.SetActive(true);
            }
            else
            {
                //proceed with creation
                CancelNameInput();

                CreateExpedition(inputName);
            }
        }
        else if (currentState == MenuState.MapNameInput)
        {
            optionsManager.GiveNewMapName(nameInput.text);

            CancelNameInput();

            ResetMenu();

            ReturnAnim();

            MapManager._instance.mapTarget.CleanExistingMap();

            MapManager._instance.cellCountX = sizeSelectedX;
            MapManager._instance.cellCountZ = sizeSelectedZ;
            MapManager._instance.StartNewMap();

            currentState = MenuState.Start;

            //close menu
            ShowMenu(false);
        }
    }

    public void CheckMenuUIRearrange(bool force = false)
    {
        if(currentState == MenuState.ExpeditionScreen || force)
            expdScreen.SetUnitMenuState(false);
    }

    private void MakeNewMap(int cellX, int cellZ)
    {
        sizeSelectedX = cellX;
        sizeSelectedZ = cellZ;

        currentState = MenuState.MapNameInput;
        CallNameInputPanel();
    }

    public void ClampInputValues()
    {
        int xSize = int.Parse(cellXInput.text);
        int zSize = int.Parse(cellYInput.text);

        xSize = Mathf.Clamp(xSize, cellRangeMin, cellRangeMax);
        zSize = Mathf.Clamp(zSize, cellRangeMin, cellRangeMax);

        cellXInput.text = xSize.ToString();
        cellYInput.text = zSize.ToString();
    }
#endregion

    #region load map

    public void MakeFileList()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + optionsManager.GetMapFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + optionsManager.GetMapFolder());

        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        string[] fileNames = new string[0];
        if (alphaSorting)
        {
            fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + optionsManager.GetMapFolder(), "*.iconmap");

            sortButton.sprite = sortAZIcon;
        }
        else
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/" + optionsManager.GetMapFolder());
            FileInfo[] files = info.GetFiles("*.iconmap").OrderBy(p => p.LastAccessTime).ToArray();
            fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = files[i].FullName;
            }

            sortButton.sprite = sortTimeIcon;
        }

        RectTransform prefabEntryRT = fileEntryPrefab.GetComponent<RectTransform>();
        float entrySeparation = optionsManager.newSaveButton.GetComponent<RectTransform>().anchoredPosition.y;
        float entryHeight = prefabEntryRT.sizeDelta.y;

        //clear list
        for (int i = fileContent.childCount - 1; i >= 1; i--)
        {
            Destroy(fileContent.GetChild(i).gameObject);
        }

        float yPOs = entrySeparation;

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
                entryRT.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate { OpenOldFilePanel(index); });
                entryRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteFileCall(index); });

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
                entryRT.GetChild(2).GetComponent<HoldButton>().onRelease.AddListener(delegate { OpenOldFilePanel(index); });
                entryRT.GetChild(3).GetComponent<HoldButton>().onRelease.AddListener(delegate { DeleteFileCall(index); });

                nuEntry.SetActive(true);

                yPOs += entrySeparation - entryHeight;
            }
        }

        Vector2 sd = fileContent.sizeDelta;
        sd.y = -1f * yPOs;
        fileContent.sizeDelta = sd;

        fileScroll.SetActive(true);
    }

    private void OpenOldFilePanel(int fileIndex)
    {
        chosenFileIndex = fileIndex;
        oldFilePanelLoad.gameObject.SetActive(true);
    }

    public void ChooseOldFile()
    {
        int fileIdx = chosenFileIndex;
        ChooseOldFile(fileIdx);
    }
    public void ChooseOldFile(int fileIndex)
    {
        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        string[] fileNames = new string[0];
        if (alphaSorting)
        {
            Debug.Log(optionsManager.GetMapFolder());
            fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + optionsManager.GetMapFolder(), "*.iconmap");

            sortButton.sprite = sortAZIcon;
        }
        else
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/" + optionsManager.GetMapFolder());
            FileInfo[] files = info.GetFiles("*.iconmap").OrderBy(p => p.LastAccessTime).ToArray();
            fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = files[i].FullName;
            }

            sortButton.sprite = sortTimeIcon;
        }

        string simpleName = fileNames[fileIndex].Substring(fileNames[fileIndex].LastIndexOf('\\') + 1);

        Debug.Log("loading: " + simpleName);

        simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

        optionsManager.LoadMap(simpleName);
        optionsManager.LoadMapPieces(simpleName);
        oldFilePanelLoad.gameObject.SetActive(false);

        ReturnAnim();
        ResetMenu();

        MapManager._instance.MapAfterLoad();

        currentState = MenuState.Start;

        ShowMenu(false);
    }

    public void ToggleFileSort()
    {
        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        if (alphaSorting)
            PlayerPrefs.SetInt("menuLoadSort", 1);
        else
            PlayerPrefs.SetInt("menuLoadSort", 0);

        MakeFileList();
    }

    public void DeleteFileCall(int fileIndex)
    {
        fileIndexWaitingDeletion = fileIndex;

        bool alphaSorting = PlayerPrefs.GetInt("menuLoadSort", 0) == 0;

        string[] fileNames = new string[0];
        if (alphaSorting)
        {
            fileNames = Directory.GetFiles(Application.persistentDataPath + "/" + optionsManager.GetMapFolder(), "*.iconmap");

            sortButton.sprite = sortAZIcon;
        }
        else
        {
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/" + optionsManager.GetMapFolder());
            FileInfo[] files = info.GetFiles("*.iconmap").OrderBy(p => p.LastAccessTime).ToArray();
            fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = files[i].FullName;
            }

            sortButton.sprite = sortTimeIcon;
        }

        fileToDeletePath = fileNames[fileIndex];

        string simpleName = fileNames[fileIndex].Substring(fileNames[fileIndex].LastIndexOf('\\') + 1);
        simpleName = simpleName.Substring(0, simpleName.LastIndexOf('.'));

        mapDeleteConfirmPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Are you sure you want to delete the \"" + simpleName + "\" map?";
        mapDeleteConfirmPanel.gameObject.SetActive(true);
    }

    public void ConfirmFileDeletion()
    {
        //delete file index
        string mapPath = fileToDeletePath;

        Debug.Log("to delete: " + mapPath);

        mapDeleteConfirmPanel.gameObject.SetActive(false);

        if (File.Exists(mapPath)) 
        { 
            File.Delete(mapPath);

            MakeFileList();
        }
        else
        {
            Debug.LogError("File not found");
            return;
        }

    }
    #endregion

    #region Close
    public void CloseProgram()
    {
        Debug.Log("Closing!!");
        Application.Quit();
    }
    #endregion
}
