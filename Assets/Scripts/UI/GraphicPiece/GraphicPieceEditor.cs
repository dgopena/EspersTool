using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System.IO;

using AnotherFileBrowser.Windows;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;

public class GraphicPieceEditor : MonoBehaviour
{
    public static GraphicPieceEditor Instance;

    public EventSystem eventSystem;

    private int processType; // 0 - character, 1 - foe, 2 - token piece
    private UI_PieceLanding callPanel;

    [Header("Settings")]
    [SerializeField] private string pieceFolder = "GraphicPieces";

    [Space(10f)]
    [SerializeField] private int maxPointCount = 20;
    [SerializeField] private float additionMinDistance = 10f;
    [SerializeField] private Vector2 coverTextureSize;

    private Texture2D activeTexture;

    [Space(10f)]
    [SerializeField] private float pieceThickness = 0.1f;

    [Header("Default Piece Data")]
    public Vector3[] defaultPointBorder;
    public Texture2D defaultTexture;
    public Vector2 defaultBoundX;
    public Vector2 defaultBoundY;
    public float defaultModScale;
    public float defaultModHeight;

    [Header("Editor In Scene")]
    [SerializeField] private Transform cameraObject;
    [SerializeField] private Transform sceneGraphicEditor;

    [SerializeField] private Transform editorCoverMesh;
    private MeshFilter coverMF;
    private Mesh coverMesh; //mesh

    private bool editLock = false;

    [SerializeField] private Transform displayQuad;
    [SerializeField] private Material graphicQuadMaterial;

    [Space(5f)]
    [SerializeField] private Material graphicPieceMaterial;
    [SerializeField] private Material rimMaterial;
    [SerializeField] private float rimVerticalTextureSize = 10f;

    [Header("UI")]
    [SerializeField] private RectTransform rootTransform;
    [SerializeField] private GameObject startPage;

    private UI_PieceLanding activePieceLanding;

    [Space(5f)]
    [SerializeField] private GameObject menuBackButton;
    [SerializeField] private GameObject gameBackButton;

    [Header("Image Components")]
    [SerializeField] private Image[] imageElements;
    private Color[] baseGrays;

    [Header("Edit Screen")]
    [SerializeField] private GameObject editPage;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private Transform editCameraPoint;

    [Space(5f)]
    [SerializeField] private float editScreenPadding = 5f;

    [Space(5f)]
    [SerializeField] private GameObject choosePieceButton;
    [SerializeField] private CanvasGroup choosePieceButtonCG;

    [Space(5f)]
    [SerializeField] private GameObject editPointPrefab;
    private RectTransform editPointParent;
    [SerializeField] private GameObject editLinePrefab;
    private RectTransform editLineParent;
    [SerializeField] private RectTransform addPointIcon;
    [SerializeField] private float addShowDistance;

    [Space(5f)]
    [SerializeField] private GameObject confirmCreatePanel;
    [SerializeField] private CanvasGroup createButtonCanvasGroup;
    [SerializeField] private TextMeshProUGUI creationTextWarning;
    private bool creationEnabled = true;

    private List<Vector3> pointPositions;

    private List<RectTransform> editPoints;
    private List<RectTransform> editLines;

    private int indexGrabbed = -1;
    private Vector3 grabDelta;

    private float minMoveX;
    private float maxMoveX;
    private float minMoveY;
    private float maxMoveY;

    private bool additionEnabled = false;
    private int addInLineIndex = -1;

    [Header("Piece Load Screen")]
    [SerializeField] private GameObject pieceLoadPage;
    [SerializeField] private RectTransform pieceListContent;
    [SerializeField] private RectTransform horizontalSetUp;
    [SerializeField] private RectTransform horizontalSetDown;
    private Transform pieceDisplay;
    [SerializeField] private Transform displayCameraPoint;
    [SerializeField] private CanvasGroup confirmSelectionButton;
    private bool selectionEnabled = false;
    [SerializeField] private GameObject confirmDeletionPanel;

    [Header("Piece Display Modifying")]
    [SerializeField] private GameObject pieceEditButton;
    [SerializeField] private Slider pieceScaleSlider;
    [SerializeField] private Slider pieceHeightSlider;
    [SerializeField] private Image pieceHeightIcon;
    [SerializeField] private GameObject basePlaceholder;
    [SerializeField] private Image pieceScaleIcon;
    [SerializeField] private GameObject saveModsButton;

    [Space(10f)]
    [SerializeField] private float pieceHeightMultiplier;
    [SerializeField] private float pieceScaleMultiplier;

    private bool editingPiece = false;
    private GraphicPieceFile activeGraphicPiece;

    private struct IDPieceFileTuple
    {
        public GraphicPieceFile loadPiece;
        public string hexId;
    }

    private IDPieceFileTuple[] loadedPieces;

    private GameObject activeSelection;
    private RectTransform currentSelectionIcon;
    private int activerSelectionIndex = -1;

    private bool listBuilt = false;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        if (editLock)
            return;

        if (!eventSystem.IsPointerOverGameObject())
            return;

        if (editPoints == null)
            return;

        if (indexGrabbed < 0) //no point grabbed
        {
            int indexHover = -1;

            for (int i = 1; i < editPoints.Count - 1; i++) //dont take the borders
            {
                if (TooltipManager.CheckMouseInArea(editPoints[i]))
                {
                    indexHover = i;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (indexHover >= 0)
                {
                    indexGrabbed = indexHover;

                    grabDelta = editPoints[indexGrabbed].position - Input.mousePosition;

                    return;
                }
                else if (additionEnabled)
                {
                    AddNewPoint();

                    return;
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                //delete a point
                if (indexHover >= 0 && editPoints.Count > 2)
                {
                    DeletePoint(indexHover);

                    return;
                }
            }
            else
            {
                if (indexHover < 0 && (editPoints.Count < maxPointCount))
                {
                    Vector3 closestPoint = Vector3.zero;
                    float distance = float.MaxValue;
                    int closestLineIndex = -1;

                    Vector3 mousePos = Input.mousePosition;

                    for (int i = 0; i < pointPositions.Count - 1; i++)
                    {
                        Vector3 lineDir = pointPositions[i + 1] - pointPositions[i];
                        Vector3 proj = mousePos - pointPositions[i];

                        Vector3 projDir = Vector3.Project(proj, lineDir);
                        proj = pointPositions[i] + projDir;


                        //check bounds
                        if ((projDir.magnitude + additionMinDistance) >= lineDir.magnitude) //upper
                            continue;
                        else if ((projDir.magnitude - additionMinDistance) < 0f) //lower too close
                            continue;
                        else if ((projDir + lineDir).magnitude < (projDir - lineDir).magnitude) //lower
                            continue;

                        float dist = Vector3.Distance(mousePos, proj);
                        if (dist < distance)
                        {
                            distance = dist;
                            closestPoint = proj;
                            closestLineIndex = i;
                        }
                    }


                    if (distance < addShowDistance)
                    {
                        addPointIcon.position = closestPoint;
                        addPointIcon.gameObject.SetActive(true);
                        addInLineIndex = closestLineIndex;

                        additionEnabled = true;
                    }
                    else
                    {
                        addPointIcon.gameObject.SetActive(false);

                        addInLineIndex = -1;
                        additionEnabled = false;
                    }
                }
                else
                {
                    addPointIcon.gameObject.SetActive(false);

                    addInLineIndex = -1;
                    additionEnabled = false;
                }
            }
        }
        else //moving grabbed point
        {
            if (Input.GetMouseButtonUp(0))
            {
                indexGrabbed = -1;
            }
            else
            {
                Vector3 nuPosition = Input.mousePosition + grabDelta;

                nuPosition.x = Mathf.Clamp(nuPosition.x, minMoveX, maxMoveX);
                nuPosition.y = Mathf.Clamp(nuPosition.y, minMoveY, maxMoveY);

                editPoints[indexGrabbed].position = nuPosition;
                pointPositions[indexGrabbed] = nuPosition;
                UpdateLines();
                UpdateMesh();
            }
        }
    }

    public string GetPieceFolder()
    {
        return MapManager._instance.optionsManager.currentExpeditionRoot + "/" + pieceFolder;
    }

    #region File Opening

    public void OpenFileBrowser()
    {
        var bp = new BrowserProperties();
        bp.filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            //Load image from local path with UWR
            StartCoroutine(LoadImage(path));
        });
    }

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                activeTexture = DownloadHandlerTexture.GetContent(uwr);
                graphicQuadMaterial.mainTexture = activeTexture;

                StartEdit();
            }
        }
    }

    #endregion

    #region Edit Screen
    public void StartEdit()
    {
        startPage.SetActive(false);

        if (editPointParent == null)
        {
            editPointParent = editPointPrefab.transform.parent.GetComponent<RectTransform>();
            editLineParent = editLinePrefab.transform.parent.GetComponent<RectTransform>();
        }
        else
            ClearEditPrefabs();

        //create edit points and edit covers in default positions (two unmovables, two on the top corners
        BuildEditUI();
        BuildEditMesh();

        editLock = false;

        SetCameraPosition(false);

        pieceLoadPage.SetActive(false);
        editPage.SetActive(true);
    }

    private void BuildEditUI()
    {
        Vector2 propPointA = new Vector2(0.3f, 0.8f);
        Vector2 propPointB = new Vector2(0.7f, 0.8f);

        float displayPixelWidth = editPointParent.rect.width;
        float displayPixelHeight = editPointParent.rect.height;

        pointPositions = new List<Vector3>();
        editPoints = new List<RectTransform>();

        float scaleFactor = rootTransform.parent.localScale.x;

        minMoveX = editPointParent.position.x + (scaleFactor * editPointParent.rect.min.x) + editScreenPadding;
        maxMoveX = editPointParent.position.x + (scaleFactor * editPointParent.rect.max.x) - editScreenPadding;

        minMoveY = editPointParent.position.y + (scaleFactor * editPointParent.rect.min.y) + editScreenPadding;
        maxMoveY = editPointParent.position.y + (scaleFactor * editPointParent.rect.max.y) - editScreenPadding;

        Vector3 pointA = editPointParent.position + (scaleFactor * new Vector3(editPointParent.rect.min.x, editPointParent.rect.min.y, 0f));
        Vector3 pointB = editPointParent.position + (scaleFactor * new Vector3(editPointParent.rect.min.x + (propPointA.x * displayPixelWidth), editPointParent.rect.min.y + (propPointA.y * displayPixelHeight), 0f));
        Vector3 pointC = editPointParent.position + (scaleFactor * new Vector3(editPointParent.rect.min.x + (propPointB.x * displayPixelWidth), editPointParent.rect.min.y + (propPointB.y * displayPixelHeight), 0f));
        Vector3 pointD = editPointParent.position + (scaleFactor * new Vector3(editPointParent.rect.max.x, editPointParent.rect.min.y, 0f));

        pointPositions.Add(pointA);
        pointPositions.Add(pointB);
        pointPositions.Add(pointC);
        pointPositions.Add(pointD);

        //points
        for (int i = 0; i < pointPositions.Count; i++)
        {
            GameObject nuPoint = Instantiate<GameObject>(editPointPrefab, editPointParent);
            nuPoint.name = "[" + i + "]";
            RectTransform pointRT = nuPoint.GetComponent<RectTransform>();
            pointRT.position = pointPositions[i];
            editPoints.Add(pointRT);

            if (i > 0 && i < pointPositions.Count - 1)
                nuPoint.SetActive(true);
        }

        editLines = new List<RectTransform>();

        //lines
        for (int i = 0; i < pointPositions.Count - 1; i++)
        {
            GameObject nuLine = Instantiate<GameObject>(editLinePrefab, editLineParent);
            nuLine.name = "[" + i + "]";
            RectTransform lineRT = nuLine.GetComponent<RectTransform>();

            Vector3 pointDiff = pointPositions[i + 1] - pointPositions[i];
            lineRT.position = pointPositions[i] + (0.5f * pointDiff);
            lineRT.right = pointDiff.normalized;

            Vector2 sd = lineRT.sizeDelta;
            sd.x = (1f / scaleFactor) * pointDiff.magnitude;
            lineRT.sizeDelta = sd;

            editLines.Add(lineRT);

            nuLine.SetActive(true);
        }
    }

    private void BuildEditMesh()
    {
        coverMesh = new Mesh();

        coverMF = editorCoverMesh.GetComponent<MeshFilter>();

        coverMesh.vertices = coverMF.mesh.vertices;
        coverMesh.triangles = coverMF.mesh.triangles;

        UpdateMesh();
    }

    public void ClearEditPrefabs()
    {
        for (int i = editPointParent.childCount - 1; i > 0; i--)
        {
            Destroy(editPointParent.GetChild(i).gameObject);
        }

        for (int i = editLineParent.childCount - 1; i > 0; i--)
        {
            Destroy(editLineParent.GetChild(i).gameObject);
        }

        if(editPoints!= null)
            editPoints.Clear();

        if(editPoints!=null)
            editLines.Clear();
    }

    private void AddNewPoint()
    {
        //modify position array
        List<Vector3> auxPositions = new List<Vector3>();

        int newPointIndex = addInLineIndex + 1;

        int listCounter = 0;
        for (int i = 0; i < pointPositions.Count + 1; i++)
        {
            if (i != newPointIndex)
            {
                auxPositions.Add(pointPositions[listCounter]);

                listCounter++;
            }
            else
            {
                auxPositions.Add(addPointIcon.position);
            }
        }

        pointPositions = auxPositions;

        //create new point
        GameObject nuPoint = Instantiate<GameObject>(editPointPrefab, editPointParent);
        RectTransform pointRT = nuPoint.GetComponent<RectTransform>();
        pointRT.position = pointPositions[newPointIndex];

        nuPoint.SetActive(true);

        //modify edit point array
        List<RectTransform> auxEditPoints = new List<RectTransform>();
        listCounter = 0;
        for (int i = 0; i < editPoints.Count + 1; i++)
        {
            if (i != newPointIndex)
            {
                auxEditPoints.Add(editPoints[listCounter]);

                listCounter++;
            }
            else
            {
                auxEditPoints.Add(pointRT);
            }

            auxEditPoints[i].gameObject.name = "[" + i + "]";
        }

        editPoints = auxEditPoints;

        //create new line
        GameObject nuLine = Instantiate<GameObject>(editLinePrefab, editLineParent);
        RectTransform lineRT = nuLine.GetComponent<RectTransform>();

        nuLine.SetActive(true);

        //modify edit line array
        List<RectTransform> auxEditLines = new List<RectTransform>();
        listCounter = 0;
        for (int i = 0; i < editLines.Count + 1; i++)
        {
            if (i != newPointIndex)
            {
                auxEditLines.Add(editLines[listCounter]);

                listCounter++;
            }
            else
            {
                auxEditLines.Add(lineRT);
            }

            auxEditLines[i].gameObject.name = "[" + i + "]";
        }

        editLines = auxEditLines;

        indexGrabbed = newPointIndex;
        UpdateLines();
        UpdateMesh();

        additionEnabled = false;
        addPointIcon.gameObject.SetActive(false);
    }

    private void DeletePoint(int indexToDelete)
    {
        if (indexToDelete == 0 || indexToDelete == editPoints.Count - 1)
            return;

        //modify position array
        List<Vector3> auxPositions = new List<Vector3>();

        for (int i = 0; i < pointPositions.Count; i++)
        {
            if (i != indexToDelete)
            {
                auxPositions.Add(pointPositions[i]);
            }
        }

        pointPositions = auxPositions;

        //modify edit point array
        List<RectTransform> auxEditPoints = new List<RectTransform>();
        int listCount = 0;
        for (int i = 0; i < editPoints.Count; i++)
        {
            if (i != indexToDelete)
            {
                auxEditPoints.Add(editPoints[i]);
                auxEditPoints[listCount].name = "[" + listCount + "]";

                listCount++;
            }
            else
            {
                RectTransform deletion = editPoints[i];
                DestroyImmediate(deletion.gameObject);
            }
        }

        editPoints = auxEditPoints;

        //modify edit line array
        List<RectTransform> auxEditLines = new List<RectTransform>();
        listCount = 0;
        for (int i = 0; i < editLines.Count; i++)
        {
            if (i != indexToDelete)
            {
                auxEditLines.Add(editLines[i]);
                auxEditLines[listCount].name = "[" + listCount + "]";

                listCount++;
            }
            else
            {
                RectTransform deletion = editLines[i];
                DestroyImmediate(deletion.gameObject);
            }
        }

        editLines = auxEditLines;

        indexGrabbed = indexToDelete;
        UpdateLines();
        UpdateMesh();

        indexGrabbed = -1;
    }

    private void UpdateLines()
    {
        if (indexGrabbed - 1 >= 0)
        {
            RectTransform lineRT = editLines[indexGrabbed - 1];

            Vector3 pointDiff = pointPositions[indexGrabbed] - pointPositions[indexGrabbed - 1];
            lineRT.position = pointPositions[indexGrabbed - 1] + (0.5f * pointDiff);
            lineRT.right = pointDiff.normalized;

            Vector2 sd = lineRT.sizeDelta;
            sd.x = (1f / rootTransform.parent.localScale.x) * pointDiff.magnitude;
            lineRT.sizeDelta = sd;
        }

        if (indexGrabbed < editPoints.Count - 1)
        {
            RectTransform lineRT = editLines[indexGrabbed];

            Vector3 pointDiff = pointPositions[indexGrabbed + 1] - pointPositions[indexGrabbed];
            lineRT.position = pointPositions[indexGrabbed] + (0.5f * pointDiff);
            lineRT.right = pointDiff.normalized;

            Vector2 sd = lineRT.sizeDelta;
            sd.x = (1f / rootTransform.parent.localScale.x) * pointDiff.magnitude;
            lineRT.sizeDelta = sd;
        }

        FlagIntersections();
    }

    private void UpdateMesh()
    {
        List<Vector3> border = new List<Vector3>();
        border.Add(new Vector3(0.5f, -0.5f, 0f));
        border.Add(new Vector3(0.5f, 0.5f, 0f));
        border.Add(new Vector3(-0.5f, 0.5f, 0f));
        border.Add(new Vector3(-0.5f, -0.5f, 0f));

        float minXNoPad = minMoveX - editScreenPadding;
        float maxXNoPad = maxMoveX + editScreenPadding;
        float minYNoPad = minMoveY - editScreenPadding;
        float maxYNoPad = maxMoveY + editScreenPadding;

        //we adapt the positions
        for (int i = 1; i < pointPositions.Count - 1; i++)
        {
            float fracX = (pointPositions[i].x - minXNoPad) / (maxXNoPad - minXNoPad);
            float fracY = (pointPositions[i].y - minYNoPad) / (maxYNoPad - minYNoPad);

            border.Add(new Vector3(fracX - 0.5f, fracY - 0.5f, 0f));
        }

        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
        poly.outside = border;

        coverMesh = Poly2Mesh.CreateMesh(poly);

        //we adapt the UVs
        Vector3[] meshPoints = coverMesh.vertices;
        Vector2[] coverUVs = new Vector2[meshPoints.Length];

        for (int n = 0; n < meshPoints.Length; n++)
        {
            Vector3 pos = meshPoints[n];
            Vector2 uv = new Vector2(((pos.x + 0.5f) / coverTextureSize.x), ((pos.y + 0.5f) / coverTextureSize.y));
            coverUVs[n] = uv;
        }

        coverMesh.uv = coverUVs;

        coverMF.mesh = coverMesh;
    }

    //intersection of lines fuck up the whole mesh. we need to avoid creation if this flag is active
    private void FlagIntersections()
    {
        //angle check
        bool angleFlag = false;

        for(int i = 0; i < pointPositions.Count; i++)
        {
            int left = i - 1;
            if (left < 0)
                left = pointPositions.Count - 1;
            int right = i + 1;
            if (right >= pointPositions.Count)
                right = 0;

            Vector3 vl = pointPositions[left] - pointPositions[i];
            Vector3 vr = pointPositions[right] - pointPositions[i];

            if(Vector3.Angle(vl, vr) < 1f)
            {
                angleFlag = true;
                break;
            }
        }

        //intersection check
        bool intersectFlag = false;

        if(indexGrabbed > 0 && indexGrabbed < editPoints.Count - 1) //border points. dont consider
        {
            for(int j = 0; j < pointPositions.Count - 1; j++)
            {
                MiscTools.Segment lineA = new MiscTools.Segment();
                lineA.Start = pointPositions[j];
                lineA.End = pointPositions[j + 1];

                for (int i = 0; i < pointPositions.Count - 1; i++)
                {
                    if (i == j)
                        continue;
                    else if (i == j + 1)
                        continue;
                    else if (i == j - 1)
                        continue;

                    MiscTools.Segment lineB = new MiscTools.Segment();
                    lineB.End = pointPositions[i];
                    lineB.Start = pointPositions[i + 1];

                    if (MiscTools.AreInstersecting(lineA, lineB))
                    {
                        intersectFlag = true;
                        break;
                    }
                }

                if (intersectFlag)
                    break;
            }
        }

        string warn = "";
        if (angleFlag)
            warn += " Angle too sharp.";
        else if (intersectFlag)
            warn += " Lines intersecting.";

        creationTextWarning.text = warn;

        SetCreationEnabled(!angleFlag && !intersectFlag);
    }

    private void SetCreationEnabled(bool value)
    {
        creationEnabled = value;

        if (!value)
        {
            createButtonCanvasGroup.alpha = 0.2f;
            creationTextWarning.gameObject.SetActive(true);
        }
        else
        {
            createButtonCanvasGroup.alpha = 1f;
            creationTextWarning.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Load Piece Screen

    public void OpenPieceList()
    {
        startPage.SetActive(false);

        MakePieceList();

        if (activeSelection != null)
            Destroy(activeSelection);
        
        activeSelection = null;
        basePlaceholder.SetActive(false);
        ShowEditTools(false);
        activerSelectionIndex = -1;

        confirmSelectionButton.alpha = 0.2f;
        confirmSelectionButton.interactable = false;

        SetCameraPosition(true);

        pieceLoadPage.SetActive(true);
    }

    public void SetCameraPosition(bool displayPosition)
    {
        if (displayPosition)
        {
            cameraObject.position = displayCameraPoint.position;
            cameraObject.rotation = displayCameraPoint.rotation;
            cameraObject.GetComponent<Camera>().orthographic = false;
        }
        else
        {
            cameraObject.position = editCameraPoint.position;
            cameraObject.rotation = editCameraPoint.rotation;
            cameraObject.GetComponent<Camera>().orthographic = true;
        }
    }

    private void MakePieceList()
    {
        if (listBuilt)
        {
            pieceListContent.anchoredPosition = Vector3.zero;
            return;
        }

        loadedPieces = LoadAllPieces();

        GameObject piecePrefab = horizontalSetUp.GetChild(0).gameObject;
        float setHeight = piecePrefab.GetComponent<RectTransform>().sizeDelta.y;

        piecePrefab.SetActive(false);

        for(int i = 0; i < loadedPieces.Length; i++)
        {
            Transform iconParent = horizontalSetUp;
            if (i % 2 != 0)
                iconParent = horizontalSetDown;

            GameObject nuIcon = Instantiate<GameObject>(piecePrefab, iconParent);
            nuIcon.name = "[" + i + "]";
            RectTransform iconRT = nuIcon.GetComponent<RectTransform>();
            iconRT.sizeDelta = setHeight * Vector2.one;

            RawImage iconImage = nuIcon.GetComponent<RawImage>();
            iconImage.texture = loadedPieces[i].loadPiece.GetImageTexture();
            iconImage.enabled = true;

            //tie event
            int iconIndex = i;
            nuIcon.GetComponent<HoldButton>().onRelease.AddListener(delegate { SelectPiece(iconIndex, iconRT); });

            nuIcon.SetActive(true);
        }

        //resize
        RectTransform horUp = horizontalSetUp.GetComponent<RectTransform>();
        Vector2 sizeDelta = horUp.sizeDelta;
        sizeDelta.x = (setHeight + horizontalSetUp.GetComponent<HorizontalLayoutGroup>().spacing) * horizontalSetUp.childCount;
        horUp.sizeDelta = sizeDelta;

        RectTransform horDown = horizontalSetDown.GetComponent<RectTransform>();
        sizeDelta = horDown.sizeDelta;
        sizeDelta.x = (setHeight + horizontalSetDown.GetComponent<HorizontalLayoutGroup>().spacing) * horizontalSetUp.childCount;
        horDown.sizeDelta = sizeDelta;

        selectionEnabled = true;
    }

    private void ClearPieceList()
    {
        for(int i = horizontalSetUp.childCount - 1; i >= 1; i--)
        {
            DestroyImmediate(horizontalSetUp.GetChild(i).gameObject);
        }

        for (int i = horizontalSetDown.childCount - 1; i >= 1; i--)
        {
            DestroyImmediate(horizontalSetDown.GetChild(i).gameObject);
        }
    }

    private void SelectPiece(int index, RectTransform icon)
    {
        if (!selectionEnabled)
            return;

        if (activeSelection != null)
        {
            Destroy(activeSelection);
            activeSelection = null;
        }

        GraphicPieceFile gpf = loadedPieces[index].loadPiece;

        Vector3[] pieceBorder = gpf.GetPointBorder();
        Texture2D pieceImage = gpf.GetImageTexture();

        Vector2 boundX = new Vector2(gpf.minX, gpf.maxX);
        Vector2 boundY = new Vector2(gpf.minY, gpf.maxY);


        activeGraphicPiece = gpf;
        SetDisplayModel(pieceBorder, boundX, boundY, pieceImage, gpf.modHeightValue, gpf.modScaleValue);
        ShowEditTools(false);

        activerSelectionIndex = index;

        if(currentSelectionIcon != null)
        {
            currentSelectionIcon.GetChild(0).gameObject.SetActive(false);
            currentSelectionIcon.GetChild(1).gameObject.SetActive(false);
        }

        currentSelectionIcon = icon;
        currentSelectionIcon.GetChild(0).gameObject.SetActive(true);
        currentSelectionIcon.GetChild(1).gameObject.SetActive(true);

        confirmSelectionButton.alpha = 1f;
        confirmSelectionButton.interactable = true;
    }

    public void SetDisplayModel(EsperUnit target)
    {
        LoadPieceWithID(target.graphicImageID, true);
    }

    private void SetDisplayModel(Vector3[] pieceBorder, Vector2 boundX, Vector2 boundY, Texture2D pieceImage, float modHeight, float modScale)
    {
        GameObject pieceCreated = CreatePieceModel(pieceBorder, boundX, boundY, pieceImage, 0f, 0f); //we load it with no mods, to be applied later for easier managing

        pieceCreated.transform.position = displayQuad.transform.position + (2f * Vector3.forward);

        if(activeSelection != null)
        {
            Destroy(activeSelection);
        }

        activeSelection = pieceCreated;
        ApplyModsToPiece(modHeight, modScale); //here we applied the mod values

        basePlaceholder.SetActive(true);
    }

    public void CallPieceSelectionConfirm()
    {
        selectionEnabled = false;
    }

    public void ConfirmPieceSelection()
    {
        /*
        //give id to piece in UnitManager
        if(processType == 0)
            UnitManager._instance.SaveCharacter(loadedPieces[activerSelectionIndex].hexId);
        else if(processType == 1)
            UnitManager._instance.SaveFoe(loadedPieces[activerSelectionIndex].hexId);
        else
        {
            PieceManager._instance.GiveIDToActiveToken(loadedPieces[activerSelectionIndex].hexId);
        }
        */
        
        activePieceLanding.ConfirmPieceID(loadedPieces[activerSelectionIndex].hexId);

        //close panel
        rootTransform.gameObject.SetActive(false);
    }

    public void CancelPieceSelection()
    {
        selectionEnabled = true;
    }

    #endregion

    #region Piece Positioning
    
    private void ApplyModsToPiece(GraphicPieceFile loadedPiece)
    {
        ApplyModsToPiece(loadedPiece.modHeightValue, loadedPiece.modScaleValue);
    }

    private void ApplyModsToPiece(float modHeightValue, float modScaleValue)
    {
        float hght = modHeightValue * pieceHeightMultiplier;
        float sca = 1f + (modScaleValue * pieceScaleMultiplier);

        pieceHeightSlider.SetValueWithoutNotify(modHeightValue);
        pieceScaleSlider.SetValueWithoutNotify(modScaleValue);

        //apply them to the preview
        activeSelection.transform.position = displayQuad.transform.position + (2f * Vector3.forward) + (hght * Vector3.up);
        activeSelection.transform.localScale = sca * Vector3.one;
    }

    public void UpdateHeightModsToPiece()
    {
        float hght = pieceHeightSlider.value * pieceHeightMultiplier;
        activeSelection.transform.position = displayQuad.transform.position + (2f * Vector3.forward) + (hght * Vector3.up);
    }

    public void UpdateScaleModsToPiece()
    {
        float sca = 1 + (pieceScaleSlider.value * pieceScaleMultiplier);
        activeSelection.transform.localScale = sca * Vector3.one;
    }

    public void StartModEdit()
    {
        basePlaceholder.SetActive(true);
        ShowEditTools(true);
    }

    private void ShowEditTools(bool show)
    {
        pieceEditButton.SetActive(!show);
        editingPiece = show;

        pieceHeightIcon.gameObject.SetActive(show);
        pieceHeightSlider.gameObject.SetActive(show);
        pieceScaleIcon.gameObject.SetActive(show);
        pieceScaleSlider.gameObject.SetActive(show);

        saveModsButton.gameObject.SetActive(show);

        if (show)
        {
            choosePieceButtonCG.alpha = 0.2f;
            choosePieceButtonCG.interactable = false;
            choosePieceButton.GetComponent<HoldButton>().enabled = false;
        }
        else
        {
            choosePieceButtonCG.alpha = 1f;
            choosePieceButtonCG.interactable = true;
            choosePieceButton.GetComponent<HoldButton>().enabled = true;
        }
    }
    
    public void SaveModsToPiece()
    {
        //save mods to piecefile
        activeGraphicPiece.GiveModData(pieceHeightSlider.value, pieceScaleSlider.value);

        string pieceID = loadedPieces[activerSelectionIndex].hexId;
        loadedPieces[activerSelectionIndex].loadPiece = activeGraphicPiece;
        
        SaveFile(activeGraphicPiece, pieceID);

        ShowEditTools(false);
    }

    #endregion

    #region Piece Making

    public void CreateCall()
    {
        if (!creationEnabled)
            return;

        confirmCreatePanel.SetActive(true);

        editLock = true;
    }

    public void CancelCall()
    {
        editLock = false;
    }

    public void ConfirmPieceCreate()
    {
        Vector2 boundX = new Vector2(minMoveX, maxMoveX);
        Vector2 boundY = new Vector2(minMoveY, maxMoveY);

        GameObject piece = CreatePieceModel(pointPositions.ToArray(), boundX, boundY, activeTexture, 0f, 0f); //temp

        piece.transform.position = 2.25f * Vector3.forward;

        GraphicPieceFile gpf = BuildFile(pointPositions.ToArray(), boundX, boundY, 0f, 0f);

        string pieceID = SaveFile(gpf);

        /*
        //give id to piece
        if (processType == 0)
            UnitManager._instance.SaveCharacter(pieceID);
        else if(processType == 1)
            UnitManager._instance.SaveFoe(pieceID);
        else if(processType == 2)
        {
            PieceManager._instance.GiveIDToActiveToken(pieceID);
        }
        */

        if(activePieceLanding != null)
            activePieceLanding.ConfirmPieceID(pieceID);

        //close panel
        rootTransform.gameObject.SetActive(false);

        editLock = false;

        //editPage.SetActive(false);

        //ClearPieceList();
        //OpenPieceList();
    }

    public Mesh[] CreatePieceMeshes(Vector3[] borderSet, Vector2 boundX, Vector2 boundY)
    {
        pointPositions = new List<Vector3>();
        pointPositions.AddRange(borderSet);

        //we build the mesh piece by parts first

        //FRONT----------------------------------------------------------
        List<Vector3> borderFront = new List<Vector3>();

        borderFront.Add(new Vector3(0.5f, -0.5f, -0.5f * pieceThickness));
        borderFront.Add(new Vector3(-0.5f, -0.5f, -0.5f * pieceThickness));

        //we adapt the positions
        for (int i = 1; i < pointPositions.Count - 1; i++)
        {
            float fracX = (pointPositions[i].x - boundX.x) / (boundX.y - boundX.x);
            float fracY = (pointPositions[i].y - boundY.x) / (boundY.y - boundY.x);

            borderFront.Add(new Vector3(fracX - 0.5f, fracY - 0.5f, -0.5f * pieceThickness));
        }

        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
        poly.outside.Clear();
        poly.outside = borderFront;

        Mesh frontMesh = Poly2Mesh.CreateMesh(poly);

        Vector3[] frontMeshPoints = frontMesh.vertices;
        Vector2[] frontUVs = new Vector2[frontMeshPoints.Length];

        for (int n = 0; n < frontMeshPoints.Length; n++)
        {
            Vector3 pos = frontMeshPoints[n];
            Vector2 uv = new Vector2(((pos.x + 0.5f) / coverTextureSize.x), ((pos.y + 0.5f) / coverTextureSize.y));
            frontUVs[n] = uv;
        }

        frontMesh.uv = frontUVs;

        //BACK----------------------------------------------------------
        List<Vector3> borderBack = new List<Vector3>();

        borderBack.Add(new Vector3(-0.5f, -0.5f, 0.5f * pieceThickness));
        borderBack.Add(new Vector3(0.5f, -0.5f, 0.5f * pieceThickness));

        //we adapt the positions
        for (int i = pointPositions.Count - 2; i >= 1; i--)
        {
            float fracX = (pointPositions[i].x - boundX.x) / (boundX.y - boundX.x);
            float fracY = (pointPositions[i].y - boundY.x) / (boundY.y - boundY.x);

            borderBack.Add(new Vector3(fracX - 0.5f, fracY - 0.5f, 0.5f * pieceThickness));
        }

        Poly2Mesh.Polygon poly2 = new Poly2Mesh.Polygon();
        poly2.outside.Clear();
        poly2.outside = borderBack;

        poly2.CalcPlaneNormal(Vector3.forward);

        Mesh backMesh = Poly2Mesh.CreateMesh(poly2);

        Vector3[] backMeshPoints = backMesh.vertices;
        Vector2[] backUVs = new Vector2[backMeshPoints.Length];

        for (int n = 0; n < backMeshPoints.Length; n++)
        {
            Vector3 pos = backMeshPoints[n];
            Vector2 uv = new Vector2(((pos.x + 0.5f) / coverTextureSize.x), ((pos.y + 0.5f) / coverTextureSize.y));
            backUVs[n] = uv;
        }

        backMesh.uv = backUVs;

        //RIM-------------------------------------------------------

        List<Vector3> rimBorder = new List<Vector3>();
        List<Vector2> rimUVs = new List<Vector2>();

        float rimSize = 0f;

        //front side
        for (int i = 0; i < borderFront.Count; i++)
        {
            rimBorder.Add(borderFront[i]);

            if (i > 0)
            {
                rimSize += Vector3.Distance(borderFront[i], borderFront[i - 1]);
            }

            rimUVs.Add(new Vector2(0f, (rimSize / rimVerticalTextureSize)));
        }

        rimBorder.Add(rimBorder[0]);
        rimUVs.Add(new Vector2(0f, ((Vector3.Distance(borderFront[0], borderFront[borderFront.Count - 1]) + rimSize) / rimVerticalTextureSize)));

        int halfCount = rimBorder.Count;

        //back side
        for (int i = 0; i < halfCount; i++)
        {
            rimBorder.Add(rimBorder[i] + (pieceThickness * Vector3.forward));
            rimUVs.Add(new Vector2(1f, rimUVs[i].y));
        }

        //tris
        int[] rimTriangles = new int[3 * rimBorder.Count];

        for (int i = 0; i < halfCount; i++)
        {
            int BLIdx = i;
            int BRIdx = i + halfCount;
            int ULIdx = i + 1;
            if (ULIdx >= halfCount)
            {
                ULIdx = 0;
            }
            int URIdx = ULIdx + (halfCount);

            rimTriangles[(6 * i)] = BLIdx;
            rimTriangles[(6 * i) + 1] = BRIdx;
            rimTriangles[(6 * i) + 2] = ULIdx;
            rimTriangles[(6 * i) + 3] = BRIdx;
            rimTriangles[(6 * i) + 4] = URIdx;
            rimTriangles[(6 * i) + 5] = ULIdx;
        }

        Mesh rimMesh = new Mesh();

        rimMesh.vertices = rimBorder.ToArray();
        rimMesh.triangles = rimTriangles;
        rimMesh.uv = rimUVs.ToArray();

        rimMesh.RecalculateNormals();

        return new Mesh[3] { frontMesh, backMesh, rimMesh };
    }

    public GameObject CreatePieceModel(Vector3[] borderSet, Vector2 boundX, Vector2 boundY, Texture2D imageTexture, float modHeight, float modScale)
    {
        Mesh[] meshSet = CreatePieceMeshes(borderSet, boundX, boundY);

        GameObject graphicPiece = new GameObject();
        graphicPiece.name = "New Graphic Piece";

        float hght = modHeight * pieceHeightMultiplier;
        float sca = 1f + (modScale * pieceScaleMultiplier);

        //front
        GameObject displayMesh = Instantiate<GameObject>(displayQuad.gameObject, graphicPiece.transform);
        displayMesh.name = "Front Face";

        displayMesh.transform.position = (hght * Vector3.up);
        displayMesh.transform.localScale = sca * Vector3.one;

        MeshFilter displayMF = displayMesh.GetComponent<MeshFilter>();
        MeshRenderer displarMR = displayMesh.GetComponent<MeshRenderer>();

        displayMF.mesh = meshSet[0];

        Material pieceMaterial = new Material(graphicPieceMaterial);
        pieceMaterial.mainTexture = imageTexture;

        displarMR.material = pieceMaterial;

        //back
        displayMesh = Instantiate<GameObject>(displayQuad.gameObject, graphicPiece.transform);
        displayMesh.name = "Back Face";

        displayMesh.transform.position = (hght * Vector3.up);
        displayMesh.transform.localScale = sca * Vector3.one;

        displayMF = displayMesh.GetComponent<MeshFilter>();
        displarMR = displayMesh.GetComponent<MeshRenderer>();

        displayMF.mesh = meshSet[1];

        pieceMaterial = new Material(graphicPieceMaterial);
        pieceMaterial.mainTexture = imageTexture;

        displarMR.material = pieceMaterial;

        //rim
        displayMesh = Instantiate<GameObject>(displayQuad.gameObject, graphicPiece.transform);
        displayMesh.name = "Rim Face";

        displayMesh.transform.position = (hght * Vector3.up);
        displayMesh.transform.localScale = sca * Vector3.one;

        displayMF = displayMesh.GetComponent<MeshFilter>();
        displarMR = displayMesh.GetComponent<MeshRenderer>();

        displayMF.mesh = meshSet[2];

        pieceMaterial = new Material(rimMaterial);

        displarMR.material = pieceMaterial;

        meshSet[0].RecalculateBounds();
        Vector3 meshBounds = meshSet[0].bounds.size;
        meshBounds.z = pieceThickness;

        BoxCollider bc = graphicPiece.AddComponent<BoxCollider>();
        bc.size = meshBounds;

        return graphicPiece;
    }

    public GameObject LoadPieceWithID(string hexID, bool setDisplayModel = false)
    {
        if (hexID == "def")
        {
            //load the default settings
            Vector3[] pieceBorder = defaultPointBorder;

            if (editPointParent == null)
            {
                editPointParent = editPointPrefab.transform.parent.GetComponent<RectTransform>();
                editLineParent = editLinePrefab.transform.parent.GetComponent<RectTransform>();
            }

            Vector2 boundX = defaultBoundX;
            Vector2 boundY = defaultBoundY;

            if (setDisplayModel) { 
                SetDisplayModel(pieceBorder, boundX, boundY, defaultTexture, defaultModHeight, defaultModScale);
                SetCameraPosition(true);
            }

            return CreatePieceModel(pieceBorder, boundX, boundY, defaultTexture, defaultModHeight, defaultModScale); 
        }
        else
        {
            GraphicPieceFile gpf = LoadFile(hexID);

            if (gpf == null)
                return null;

            Vector3[] pieceBorder = gpf.GetPointBorder();

            Texture2D pieceImage = gpf.GetImageTexture();

            if (editPointParent == null)
            {
                editPointParent = editPointPrefab.transform.parent.GetComponent<RectTransform>();
                editLineParent = editLinePrefab.transform.parent.GetComponent<RectTransform>();
            }

            Vector2 boundX = new Vector2(gpf.minX, gpf.maxX);
            Vector2 boundY = new Vector2(gpf.minY, gpf.maxY);

            if (setDisplayModel)
            {
                SetDisplayModel(pieceBorder, boundX, boundY, pieceImage, defaultModHeight, defaultModScale);
                SetCameraPosition(true);
            }

            return CreatePieceModel(pieceBorder, boundX, boundY, pieceImage, gpf.modHeightValue, gpf.modScaleValue);
        }
    }

    #endregion

    #region file saving

    private GraphicPieceFile BuildFile(Vector3[] borderSet, Vector2 boundX, Vector2 boundY, float modHeight, float modScale)
    {
        byte[] pngByte = activeTexture.EncodeToPNG();

        GraphicPieceFile gpf = new GraphicPieceFile();
        gpf.GiveData(borderSet, pngByte, boundX, boundY);
        gpf.GiveModData(modHeight, modScale);

        return gpf;
    }

    private string SaveFile(GraphicPieceFile gpf)
    {
        int nextID = MapManager._instance.optionsManager.graphicFileIndex; // PlayerPrefs.GetInt("pieceIDCounter", 0);

        nextID++;
        MapManager._instance.optionsManager.SetGraphicPieceIndex(nextID); // PlayerPrefs.SetInt("pieceIDCounter", nextID);
        MapManager._instance.optionsManager.SaveIndexFile();

        string hexID = nextID.ToString("X6");

        return SaveFile(gpf, hexID);
    }

    private string SaveFile(GraphicPieceFile gpf, string hexID)
    {
        string unitPath = Application.persistentDataPath + "/" + GetPieceFolder() + "/gpiece_" + hexID + ".icongpiece";
        FileStream file;

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetPieceFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetPieceFolder());

        if (File.Exists(unitPath)) file = File.OpenWrite(unitPath);
        else file = File.Create(unitPath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, gpf);
        file.Close();

        return hexID; //returns the id of the piece
    }

    public GraphicPieceFile LoadFile(string hexID)
    {
        /*
        if(hexID == "def")
        {
            //loads default graphic dummy
        }
        */

        string unitPath = Application.persistentDataPath + "/" + GetPieceFolder() + "/gpiece_" + hexID + ".icongpiece";
        FileStream file;

        if (!File.Exists(unitPath))
            return null;

        file = File.Open(unitPath, FileMode.Open);

        BinaryFormatter bf = new BinaryFormatter();
        GraphicPieceFile gpf = (GraphicPieceFile)bf.Deserialize(file);
        file.Close();

        return gpf;
    }

    private IDPieceFileTuple[] LoadAllPieces()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + GetPieceFolder()))
        {
            return new IDPieceFileTuple[0];
        }

        string[] pieceFiles = Directory.GetFiles(Application.persistentDataPath + "/" + GetPieceFolder(), "*.icongpiece");

        IDPieceFileTuple[] pieces = new IDPieceFileTuple[pieceFiles.Length];

        for(int i = 0; i < pieceFiles.Length; i++)
        {
            string fileName = pieceFiles[i];

            FileStream file;
            file = File.Open(fileName, FileMode.Open);

            string hexID = fileName.Substring(fileName.IndexOf("_") + 1);
            hexID = hexID.Substring(0, hexID.IndexOf("."));

            BinaryFormatter bf = new BinaryFormatter();
            GraphicPieceFile gpf = (GraphicPieceFile)bf.Deserialize(file);
            file.Close();

            IDPieceFileTuple nuTuple = new IDPieceFileTuple();
            nuTuple.loadPiece = gpf;
            nuTuple.hexId = hexID;

            pieces[i] = nuTuple;
        }

        return pieces;
    }

    public void DeletePiece(string hexID)
    {
        string unitPath = Application.persistentDataPath + "/" + GetPieceFolder() + "/gpiece_" + hexID + ".icongpiece";

        if (!Directory.Exists(Application.persistentDataPath + "/" + GetPieceFolder()))
            Directory.CreateDirectory(Application.persistentDataPath + "/" + GetPieceFolder());

        if (File.Exists(unitPath))
            File.Delete(unitPath);

        if (activeSelection != null)
        {
            basePlaceholder.SetActive(false);
            ShowEditTools(false);
        }
    }

    #endregion

    #region External Calls

    public void OpenPieceProcess(UI_PieceLanding callPanel, Color panelColor, bool isCharacterProcess, UI_PieceLanding activeLandingPanel)
    {
        processType = isCharacterProcess ? 0 : 1;

        activePieceLanding = activeLandingPanel;

        gameBackButton.SetActive(true);
        menuBackButton.SetActive(false);

        this.callPanel = callPanel;

        AdjustPanelColors(panelColor);

        editPage.SetActive(false);

        pieceLoadPage.SetActive(true);
        ClearPieceList();
        OpenPieceList();

        startPage.SetActive(false);
        rootTransform.gameObject.SetActive(true);

        choosePieceButton.SetActive(true);
    }
    
    public void OpenPieceProcessFromMenu(Color panelColor)
    {
        this.callPanel = null;

        processType = 3;

        gameBackButton.SetActive(false);
        menuBackButton.SetActive(true);

        AdjustPanelColors(panelColor);

        editPage.SetActive(false);
        
        pieceLoadPage.SetActive(true);
        ClearPieceList();
        OpenPieceList();

        startPage.SetActive(false);
        rootTransform.gameObject.SetActive(true);

        choosePieceButton.SetActive(false);
    }

    public void OpenTokenProcess(Color panelColor)
    {
        callPanel = null;

        processType = 2;

        gameBackButton.SetActive(true);
        menuBackButton.SetActive(false);

        AdjustPanelColors(panelColor);

        editPage.SetActive(false);

        pieceLoadPage.SetActive(true);
        ClearPieceList();
        OpenPieceList();

        startPage.SetActive(false);
        rootTransform.gameObject.SetActive(true);

        choosePieceButton.SetActive(true);
    }

    private void AdjustPanelColors(Color baseColor)
    {
        if(baseGrays == null)
        {
            baseGrays = new Color[imageElements.Length];
            for(int i = 0; i < imageElements.Length; i++)
            {
                baseGrays[i] = imageElements[i].color;
            }
        }

        for(int i = 0; i < imageElements.Length; i++)
        {
            imageElements[i].color = baseColor * 1.6f * baseGrays[i];
        }
    }

    public void ClosePieceProcess()
    {
        if(callPanel != null)
            callPanel.ReturnToLanding();

        editPage.SetActive(false);
        pieceLoadPage.SetActive(false);
        startPage.SetActive(true);

        

        rootTransform.gameObject.SetActive(false);
    }

    public void CallPieceDeletion()
    {
        confirmDeletionPanel.SetActive(true);
    }

    public void ConfirmPieceDeletion()
    {
        Debug.Log("tried to delete piece of index " + loadedPieces[activerSelectionIndex].hexId);

        //call deletion
        DeletePiece(loadedPieces[activerSelectionIndex].hexId);

        //call list rebuild
        ClearPieceList();
        MakePieceList();

        confirmDeletionPanel.SetActive(false);
    }

    #endregion
}

[System.Serializable]
public class GraphicPieceFile
{
    public byte[] PNGimage { get; private set; }

    public float[] pointShapeX { get; private set; }
    public float[] pointShapeY { get; private set; }
    public float[] pointShapeZ { get; private set; }

    public float minX { get; private set; }
    public float maxX { get; private set; }
    public float minY { get; private set; }
    public float maxY { get; private set; }

    public float modHeightValue { get; private set; }

    public float modScaleValue { get; private set; }


    public void GiveData(Vector3[] borderPoints, byte[] encodedPNG, Vector2 boundX, Vector2 boundY)
    {
        pointShapeX = new float[borderPoints.Length];
        pointShapeY = new float[borderPoints.Length];
        pointShapeZ = new float[borderPoints.Length];

        for(int i = 0; i < borderPoints.Length; i++)
        {
            Vector3 point = borderPoints[i];
            pointShapeX[i] = point.x;
            pointShapeY[i] = point.y;
            pointShapeZ[i] = point.z;
        }

        minX = boundX.x;
        maxX = boundX.y;

        minY = boundY.x;
        maxY = boundY.y;

        PNGimage = encodedPNG;
    }

    public void GiveModData(float modHeight, float modScale)
    {
        modHeightValue = modHeight;
        modScaleValue = modScale;
    }

    public Vector3[] GetPointBorder()
    {
        Vector3[] borderPoints = new Vector3[pointShapeX.Length];

        for(int i = 0; i < pointShapeX.Length; i++)
        {
            borderPoints[i] = new Vector3(pointShapeX[i], pointShapeY[i], pointShapeZ[i]);
        }

        return borderPoints;
    }

    public Texture2D GetImageTexture()
    {
        Texture2D loadedImage = new Texture2D(1, 1);
        loadedImage.LoadImage(PNGimage);

        return loadedImage;
    }
}
