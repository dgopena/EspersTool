using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class MarkManager : MonoBehaviour
{
    public static MarkManager _instance;

    public GameObject markPrefab;
    public GameObject confirmMarkPrompt;

    public float sizeScreenOptionsFraction = 0.05f;
    public RectTransform markOptions;
    public GameObject selectionCircle;
    public GameObject selectionOptions;
    public TMP_InputField markNameInput;

    private int markOptionMode = 0; // 0 - not showing, 1 - showing circle, 2 - showing options
    public bool markEditing { get {
            bool editCameraLock = (markOptionMode == 2) &&
                markNameInput.isFocused;
            return editCameraLock; } }
    private int markEditIndex = -1;

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        markOptions.sizeDelta = sizeScreenOptionsFraction * Screen.height * Vector2.one;

        selectionCircle.SetActive(false);
        selectionOptions.SetActive(false);
    }

    private UnitPiece markCaller;
    private PlayMark tempMark;
    private Color tempColor;
    private Vector3 lastPosCheck;
    public bool markPlacing { get; private set; }
    private bool makeNewMark = false;

    private List<PlayMark> inPlayMarks;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Toggle allActiveToggle;
    [SerializeField] private UnityEngine.UI.Toggle onlyActiveToggle;
    [SerializeField] private UnityEngine.UI.Toggle noneActiveToggle;
    private int markShowMode = 1;  //0 - not show, 1 - show only active, 2 - show all
    private bool markShowState = false;

    // Start is called before the first frame update
    void Start()
    {
        /*
        GameObject testInstance = GameObject.Instantiate<GameObject>(markPrefab);

        PlayMark markLogic = testInstance.GetComponent<PlayMark>();
        markLogic.DrawArrow(Vector3.zero, new Vector3(3f, 0f, 4f), Color.red);
        */
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.GameMode)
            return;

        if (markPlacing)
        {
            Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.terrainLayer))
            {
                Vector3 tentPos = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point);

                if (makeNewMark)
                {
                    GameObject markInstance = GameObject.Instantiate<GameObject>(markPrefab);
                    markInstance.transform.parent = transform;
                    tempMark = markInstance.GetComponent<PlayMark>();
                    lastPosCheck = Vector3.zero;
                    makeNewMark = false;
                }

                if (Vector3.Distance(tentPos, lastPosCheck) > 0.4f * MapManager._instance.mapTarget.cellScale)
                {
                    tempMark.DrawArrow(markCaller.transform.position, tentPos, tempColor);
                    lastPosCheck = tentPos;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                //cancel mark placing
                CancelMark();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TryConfirmMark();
                markPlacing = false;
            }
        }
        else
        {
            if(Input.GetMouseButtonDown(0) && markOptionMode == 1)
            {
                ShowMarkOptions(true);
            }

            if(markOptionMode == 2 && (inPlayMarks != null || inPlayMarks.Count > 0))
            {
                if(markEditIndex >= 0)
                {
                    Vector3 markPos = inPlayMarks[markEditIndex].centralMarkPosition;
                    markOptions.position = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(markPos);
                }
            }

            if (inPlayMarks == null || inPlayMarks.Count == 0 || markOptionMode == 2)
                return;

            float activationDistance = 0.5f * sizeScreenOptionsFraction * Screen.height;
            Vector3 toolPos = Vector3.zero;
            bool markMatch = false;
            for (int i = 0; i < inPlayMarks.Count; i++)
            {
                if (!inPlayMarks[i].markEnabled)
                    continue;

                if (PieceManager._instance.PieceDisplayPanel.gameObject.activeInHierarchy && !PieceManager._instance.PieceDisplayPanel.pieceDisplayed.Equals(inPlayMarks[i].sourcePiece))
                    continue; //can't edit marks of other pieces if display panel of a piece is active

                Vector3 markPos = inPlayMarks[i].centralMarkPosition;
                markPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(markPos);

                if(Vector3.Distance(Input.mousePosition, markPos) < activationDistance)
                {
                    toolPos = markPos;
                    markMatch = true;
                    markEditIndex = i;
                    break;
                }
            }

            if (markMatch)
            {
                markOptions.position = toolPos;
                selectionCircle.gameObject.SetActive(true);
                markOptionMode = 1;
            }
            else
            {
                selectionCircle.gameObject.SetActive(false);
                markEditIndex = -1;
                markOptionMode = 0;
            }
        }
    }

    private void ShowMarkOptions(bool enable)
    {
        if (enable)
        {
            selectionCircle.gameObject.SetActive(false);
            selectionOptions.gameObject.SetActive(true);
            markOptionMode = 2;


            if(markEditIndex >= 0)
            {
                Color toApply = inPlayMarks[markEditIndex].baseColor;
                markNameInput.GetComponent<UnityEngine.UI.Image>().color = toApply;

                if (toApply.grayscale > 0.5f)
                {
                    Color textColor = Color.black;
                    markNameInput.textComponent.color = textColor;
                    textColor.a = 0.4f;
                    markNameInput.placeholder.color = textColor;
                }
                else
                {
                    Color textColor = Color.white;
                    markNameInput.textComponent.color = textColor;
                    textColor.a = 0.4f;
                    markNameInput.placeholder.color = textColor;
                }

                markNameInput.text = "";
                if (inPlayMarks[markEditIndex].markName != "")
                    markNameInput.text = inPlayMarks[markEditIndex].markName;
            }
        }
        else
        {
            selectionCircle.gameObject.SetActive(false);
            selectionOptions.gameObject.SetActive(false);
            markOptionMode = 0;
        }
    }

    public void CloseMarkEdit()
    {
        if (markOptionMode < 2)
            return;

        if (markEditIndex >= 0)
        {
            inPlayMarks[markEditIndex].SetMarkName(markNameInput.text);
        }

        ShowMarkOptions(false);
    }

    public void DeleteMark()
    {
        if (markOptionMode < 2)
            return;

        if (markEditIndex >= 0)
        {
            GameObject toDestroy = inPlayMarks[markEditIndex].gameObject;
            inPlayMarks.RemoveAt(markEditIndex);
            Destroy(toDestroy);
        }

        markEditIndex = -1;
        ShowMarkOptions(false);
    }

    public void CleanAllMarks()
    {
        if (inPlayMarks == null)
            return;

        ShowMarkOptions(false);

        for (int i = inPlayMarks.Count - 1; i >= 0; i--)
        {
            Destroy(inPlayMarks[i].gameObject);
        }

        inPlayMarks.Clear();
    }

    public List<PlayMark> GetMarkList()
    {
        if (inPlayMarks == null)
            inPlayMarks = new List<PlayMark>();

        return inPlayMarks;
    }

    //loads the marks of each piece saved on the piecefile
    public void GiveLoadedMarks(PieceFile loadedFile)
    {
        if (inPlayMarks != null)
        {
            for (int i = inPlayMarks.Count - 1; i >= 0; i--)
            {
                Destroy(inPlayMarks[i].gameObject);
            }
        }

        inPlayMarks = new List<PlayMark>();

        PieceFile.SaveMark[] loadedMarks = loadedFile.marks;
        if (loadedMarks == null)
            return;

        for(int i = 0; i < loadedMarks.Length; i++)
        {
            Vector3 fromPosition = new Vector3(loadedMarks[i].sourceX, loadedMarks[i].sourceY, loadedMarks[i].sourceZ);
            Vector3 toPosition = new Vector3(loadedMarks[i].targetX, loadedMarks[i].targetY, loadedMarks[i].targetZ);

            GameObject markInstance = GameObject.Instantiate<GameObject>(markPrefab);
            markInstance.transform.parent = transform;

            PlayMark loMark = markInstance.GetComponent<PlayMark>();
            loMark.SetMarkName(loadedMarks[i].markName);

            UnitPiece srcPiece = PieceManager._instance.GetPieceClosestTo(fromPosition);
            if (srcPiece == null)
                continue;

            Color markColor = srcPiece.GetPieceColor();
            loMark.SetHeightIndex(loadedMarks[i].heightIndex);

            if (loadedMarks[i].isPieceMark)
            {
                UnitPiece trgPiece = PieceManager._instance.GetPieceClosestTo(toPosition);
                if (trgPiece == null)
                    continue;

                loMark.MakeMark(srcPiece, trgPiece, markColor);
                loMark.SimpleRedraw();
            }
            else
            {
                loMark.MakeMark(srcPiece, toPosition, markColor);
                loMark.SimpleRedraw();
            }

            inPlayMarks.Add(loMark);
        }
    }

    public void ShowMarks(bool show)
    {
        if (!show)
        {
            ShowMarks(0);
        }
        else
        {
            ShowMarks(markShowMode); //should apply this according to the settings in options
        }
    }

    public void ShowMarks(int showType) //0 - none, 1 - only active piece, 2 - all
    {
        if (inPlayMarks == null)
            return;

        //not hide temp

        for (int i = 0; i < inPlayMarks.Count; i++)
        {
            if (showType == 2)
                inPlayMarks[i].SetMarkEnabled(true);
            else if (showType == 1)
            {
                UnitPiece activePiece = PieceManager._instance.GetActivePiece();
                if (activePiece != null && (inPlayMarks[i].sourcePiece == activePiece || inPlayMarks[i].targetPiece == activePiece))
                    inPlayMarks[i].SetMarkEnabled(true);
                else
                    inPlayMarks[i].SetMarkEnabled(false);
            }
            else
                inPlayMarks[i].SetMarkEnabled(false);
        }
    }

    public void DimMarks(int showType, float alpha) //0 - all, 1 - only active piece
    {
        if (inPlayMarks == null)
            return;

        for (int i = 0; i < inPlayMarks.Count; i++)
        {
            if (showType == 0)
                inPlayMarks[i].DimBy(alpha);
            else if (showType == 1)
            {
                UnitPiece activePiece = PieceManager._instance.GetActivePiece();
                if (activePiece != null && inPlayMarks[i].sourcePiece == activePiece)
                {
                    inPlayMarks[i].DimBy(alpha);
                    return;
                }
            }
        }
    }

    public void ReverseDim(int showType)
    {
        if (inPlayMarks == null)
            return;

        for (int i = 0; i < inPlayMarks.Count; i++)
        {
            if (showType == 0)
                inPlayMarks[i].RevertDim();
            else if (showType == 1)
            {
                UnitPiece activePiece = PieceManager._instance.GetActivePiece();
                if (activePiece != null && inPlayMarks[i].sourcePiece == activePiece)
                {
                    inPlayMarks[i].RevertDim();
                    return;
                }
            }
        }
    }

    public void UpdateMarks()
    {
        //update all marks
    }

    public void UpdateMarks(UnitPiece target)
    {
        if (inPlayMarks == null)
            return;

        for(int i = 0; i < inPlayMarks.Count; i++)
        {
            if(inPlayMarks[i].sourcePiece == target || inPlayMarks[i].targetPiece == target)
            {


                inPlayMarks[i].SimpleRedraw();
            }
        }
    }

    public void StartMarkMaking(UnitPiece caller, Color markColor)
    {
        markCaller = caller;
        tempColor = markColor;
        markPlacing = true;
        makeNewMark = true;

        PieceManager._instance.SetDisplayPanelActive(false);
        PieceManager._instance.SetPieceButtonOptions(false);
        DimMarks(0, 0.4f); //adjust show type according to options
    }

    public void TryConfirmMark()
    {
        bool pieceCrash = PieceManager._instance.IsPieceInCoordinates(tempMark.toCoordinate);

        if (pieceCrash)
        {
            confirmMarkPrompt.SetActive(true);
        }
        else
        {
            if (inPlayMarks == null)
                inPlayMarks = new List<PlayMark>();
            tempMark.MakeMark(markCaller, lastPosCheck, tempColor);

            if(!IsDuplicate(tempMark))
                inPlayMarks.Add(tempMark);

            markPlacing = false;
            ReverseDim(0); //adjust show type according to options
        }
    }

    public void CancelMark()
    {
        Destroy(tempMark.gameObject);
        markPlacing = false;
        ReverseDim(0); //adjust show type according to options
    }

    public void ConfirmMarkAs(int index)
    {
        if(index == 0)
        {
            UnitPiece matchPiece = PieceManager._instance.GetPieceInCoordinates(tempMark.toCoordinate);
            if (matchPiece != null)
            {
                if (inPlayMarks == null)
                    inPlayMarks = new List<PlayMark>();
                tempMark.MakeMark(markCaller, matchPiece, tempColor);
                if (!IsDuplicate(tempMark))
                    inPlayMarks.Add(tempMark);
            }
            else
            {
                CancelMark();
                return;
            }
        }
        else
        {
            if (inPlayMarks == null)
                inPlayMarks = new List<PlayMark>();
            tempMark.MakeMark(markCaller, lastPosCheck, tempColor);
            if (!IsDuplicate(tempMark))
                inPlayMarks.Add(tempMark);
        }

        markPlacing = false;
        ReverseDim(0); //adjust show type according to options
    }

    public bool IsDuplicate(PlayMark nuMark)
    {
        if (inPlayMarks == null)
        {
            inPlayMarks = new List<PlayMark>() { nuMark };
            return false;
        }
        else
        {
            //check duplicates
            for (int i = 0; i < inPlayMarks.Count; i++)
            {
                if (inPlayMarks[i].type == nuMark.type)
                {
                    if (nuMark.type == PlayMark.MarkType.PieceMark && (nuMark.targetPiece == inPlayMarks[i].targetPiece && nuMark.sourcePiece == inPlayMarks[i].sourcePiece))
                    {
                        //duplicate. pedal back.
                        Destroy(nuMark.gameObject);
                        return true;
                    }
                    else if (nuMark.type == PlayMark.MarkType.SpaceMark && (nuMark.toCoordinate == inPlayMarks[i].toCoordinate && nuMark.sourcePiece == inPlayMarks[i].sourcePiece))
                    {
                        //duplicate. pedal back.
                        Destroy(nuMark.gameObject);
                        return true;
                    }
                }
                else
                {
                    //check same directionals
                    if (nuMark.toCoordinate == inPlayMarks[i].toCoordinate)
                    {
                        nuMark.SimpleRedraw(inPlayMarks[i].heightIndex == 0 ? 1 : 0);
                    }
                }
            }

            return false;
        }
    }

    public void PieceDeletion(UnitPiece pieceGone)
    {
        if (inPlayMarks == null)
            return;

        for(int i = inPlayMarks.Count - 1; i >= 0; i--)
        {
            if(inPlayMarks[i].sourcePiece == pieceGone || (inPlayMarks[i].type == PlayMark.MarkType.PieceMark && inPlayMarks[i].targetPiece == pieceGone))
            {
                Destroy(inPlayMarks[i].gameObject);
                inPlayMarks.RemoveAt(i);
            }
        }
    }

    public void SetMarkDisplayMode(int mode)
    {
        markShowMode = mode;

        allActiveToggle.SetIsOnWithoutNotify(mode == 2);
        onlyActiveToggle.SetIsOnWithoutNotify(mode == 1);
        noneActiveToggle.SetIsOnWithoutNotify(mode == 0);

        ShowMarks(MapManager._instance.toolMode == MapManager.ToolMode.GameMode);

        MapManager._instance.optionsManager.SaveMarkShowSetting(mode);
    }
}
