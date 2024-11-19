using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapesManager : MonoBehaviour
{
    public static ShapesManager _instance;

    public List<ShapeData> shapes;

    [System.Serializable]
    public struct ShapeData
    {
        public GameObject shapePrefab;
        public Sprite shapeIcon;
        public ShapeCategory[] categories;
    }

    [System.Serializable]
    public enum ShapeCategory
    {
        All
    }

    [Space(20f)]
    public Material outlinerMat;
    private Material storedMat;
    public ShapeWrapper wrapper;

    [Space(20f)]
    public LayerMask floorLayer;

    [Header("Shape Menu")]
    public int iconsPerColumn = 6;
    [Range(0.01f, 0.2f)]
    public float iconSpacingH = 0.01f;
    [Range(0.01f, 0.2f)]
    public float iconSpacingV = 0.03f;
    [Range(0.05f, 0.8f)]
    public float iconSize= 0.2f;
    public GameObject rowIconPrefab;
    public RectTransform shapeIconContent;
    private ScrollRect menuScrollRect;
    public Animator shapeMenuAnim;
    public GameObject dragIconPrefab;
    public RectTransform instanceArea;
    public RectTransform instanceAreaFull;
    public RectTransform menuArea;

    [Space(20f)]
    public GameObject decorMarkerPrefab;
    [Range(0.05f, 0.8f)]
    public float decorMarkerSize = 0.1f;

    public Sprite denyIcon;
    public Image slideButtonIcon;
    public Sprite leftArrowIcon;
    public Sprite rightArrowIcon;
    public bool shapeMenuActive { get; private set; }

    //instancing stuff
    private ShapeData currentShapeData;
    private int currentShapeIndex;
    private RectTransform iconRT;
    private Image iconImage;
    private bool dragIconActive = false;
    private GameObject preInstancedShape;
    private bool preInstanceValid = false;
    private bool justSwitched = false;

    private bool onMenuArea;
    private bool onInstanceArea;

    private Transform objectsCreated;

    [System.Serializable]
    public enum ShapeToolMode
    {
        Global,
        Local
    }

    [Header("Wrapper Menu")]
    public ShapeToolMode shapeToolMode = ShapeToolMode.Global;
    public Image globalButton;
    public Image localButton;
    public Image positionButton;
    public GameObject positionInputMenu;
    public Image rotationButton;
    public GameObject rotationInputMenu;
    public Image scaleButton;
    public GameObject scaleInputMenu;
    public Color selectedColor;
    public Color unselectedColor;

    public Material redHelpMat;
    public Material redHelpMatFade;
    public Material greenHelpMat;
    public Material greenHelpMatFade;
    public Material blueHelpMat;
    public Material blueHelpMatFade;

    [Space(20f)]
    public RectTransform shapeToolsMenu;

    [Space(20f)]
    public TMP_InputField positionInputX;
    public TMP_InputField positionInputY; 
    public TMP_InputField positionInputZ;

    public TMP_InputField rotationInputX;
    public TMP_InputField rotationInputY;
    public TMP_InputField rotationInputZ;

    public TMP_InputField scaleInputX;
    public TMP_InputField scaleInputY;
    public TMP_InputField scaleInputZ;

    //selecting stuff
    [Space(20f)]
    public LayerMask grabbableLayers;
    public LayerMask wrapperHelperLayer;
    private Transform shapeSelected;

    public bool shapeSnapToGrid { get; private set; }

    [Range(0.05f, 0.45f)]
    public float snapDistance = 0.15f;

    //collision matrix stuff
    [Header("Collision Matrix Settings")]
    [SerializeField] private float vertexScaleDownHorizontal = 0.8f;
    [SerializeField] private float vertexScaleDownVertical = 0.35f;
    private int[,,] shapeCollisionMatrix;
    private bool shapeCollisionBaked = false;
    [SerializeField] private Transform collisionGridParent;
    private bool collGridDisplayed = false;

    private bool collisionChangedFlag = false;

    private void Awake()
    {
        if (ShapesManager._instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        menuScrollRect = shapeIconContent.parent.parent.GetComponent<ScrollRect>();

        iconSize = iconSize * Screen.height;
        iconSpacingH *= Screen.height;
        iconSpacingV *= Screen.height;
    }

    private void LateUpdate()
    {
        if (MapManager._instance.toolMode == MapManager.ToolMode.ObjectEditor)
        {
            if (shapeMenuActive)
            {
                onInstanceArea = CheckMouseInArea(instanceArea);
                onMenuArea = CheckMouseInArea(menuArea);
            }
            else
            {
                onInstanceArea = CheckMouseInArea(instanceAreaFull);
                onMenuArea = false;
            }

            if (dragIconActive)
            {
                if (onMenuArea)
                {
                    iconImage.sprite = denyIcon;
                    iconImage.color = Color.red;

                    if (preInstancedShape != null)
                        preInstancedShape.SetActive(false);

                    iconRT.position = Input.mousePosition;
                }
                else if (onInstanceArea)
                {
                    iconImage.sprite = currentShapeData.shapeIcon;
                    Color iconColor = Color.white;
                    iconColor.a = 0.5f;
                    iconImage.color = iconColor;

                    if (preInstancedShape == null)
                    {
                        preInstancedShape = Instantiate<GameObject>(currentShapeData.shapePrefab);
                        Color toCast = ColorManager._instance.mainColor;
                        preInstancedShape.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = toCast;
                        preInstancedShape.GetComponent<ShapeInfo>().SetShapeID(currentShapeIndex);
                        preInstancedShape.GetComponent<ShapeInfo>().SetColor(toCast);
                    }

                    Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 100f, floorLayer))
                    {
                        Vector3 pos = hitInfo.point;
                        if (shapeSnapToGrid)
                        {
                            pos = MapManager._instance.mapTarget.GetSnappedPosition(pos);
                        }
                        preInstancedShape.transform.position = pos;

                        if (!preInstancedShape.activeInHierarchy)
                        {
                            preInstancedShape.SetActive(true);
                        }

                        preInstanceValid = true;
                    }
                    else
                    {
                        preInstancedShape.SetActive(false);
                        preInstanceValid = false;
                    }

                    iconRT.position = Input.mousePosition - new Vector3(0.5f * iconSize, 0.5f * iconSize, 0f);
                }


                if (Input.GetMouseButtonUp(0))
                    ReleaseIcon();
            }
            else
            {
                if (!MapManager._instance.controlEnabled)
                    return;

                if (onInstanceArea && !MapManager._instance.overUI)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray grabRay = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hitInfo;

                        bool wrapperHelpGrabbed = false;

                        //first check wrapper helpers. they have priority over piece grabbing
                        if (Physics.Raycast(grabRay, out hitInfo, 100f, wrapperHelperLayer))
                        {
                            wrapper.HelperTransformSet(hitInfo.transform.GetInstanceID(), hitInfo.point);
                            wrapperHelpGrabbed = true;
                        }

                        if (!wrapperHelpGrabbed)
                        {
                            if (Physics.Raycast(grabRay, out hitInfo, 100f, grabbableLayers))
                            {
                                if (hitInfo.transform.gameObject.layer == 8)
                                {
                                    if (shapeSelected == null || hitInfo.transform.GetInstanceID() != shapeSelected.GetInstanceID())
                                    {
                                        ReleaseCurrentShape();

                                        shapeSelected = hitInfo.transform;
                                        MeshRenderer mr = shapeSelected.GetComponent<MeshRenderer>();
                                        storedMat = mr.material;
                                        Texture txtr = mr.material.mainTexture;
                                        Material outliner = new Material(outlinerMat);
                                        outliner.SetTexture("_MainTex", txtr);
                                        outliner.SetColor("_Color", storedMat.color);
                                        outliner.color = mr.material.color;
                                        mr.material = outliner;

                                        wrapper.GiveTransform(shapeSelected.parent);
                                        justSwitched = true;
                                        globalButton.transform.parent.gameObject.SetActive(true);

                                        ShapeInfo bounds = shapeSelected.parent.GetComponent<ShapeInfo>();
                                        shapeToolsMenu.GetChild(3).GetComponent<Toggle>().isOn = bounds.isDecor;
                                        currentShapeData = shapes[bounds.shapeID];
                                        currentShapeIndex = bounds.shapeID;

                                        shapeToolsMenu.gameObject.SetActive(true);
                                        wrapper.UpdateUIInputValues();
                                        CallChangeFlag();
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                ReleaseCurrentShape();
                            }
                        }
                    }
                    else if (Input.GetMouseButton(0) && !justSwitched) //hold
                    {
                        Ray grabRay = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
                        wrapper.HelperTransform(grabRay);
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        if (justSwitched)
                            justSwitched = false;

                        if (shapeToolMode == ShapeToolMode.Local)
                            wrapper.UpdateWrapperRotation();

                        wrapper.SetHelperGraphicDeSelected();
                    }
                }
            }
        }
    }

    public void UpdateShapeMenu(ShapeCategory categoryToDisplay = ShapeCategory.All)
    {
        //clear menu
        for (int i = shapeIconContent.childCount - 1; i >= 1; i--)
        {
            Destroy(shapeIconContent.GetChild(i).gameObject);
        }

        //float accX = 0f;
        //float accY = 0f;
        int currentIcons = 0;
        int totalIcons = 0;

        for (int i = 0; i < shapes.Count; i++)
        {

            if (!IsShapeInCategory(shapes[i], categoryToDisplay))
                continue;

            totalIcons++;
        }

        int totalCols = totalIcons / iconsPerColumn;
        int rest = totalIcons % iconsPerColumn;

        int rowJumpAt = (totalCols + (rest > 0 ? 1 : 0));

        GameObject iconPrefab = rowIconPrefab.transform.GetChild(0).gameObject;

        RectTransform iconPrebabRT = iconPrefab.GetComponent<RectTransform>();

        RectTransform[] rowObjects = new RectTransform[iconsPerColumn];

        float iconHeight = 0.9f * (shapeIconContent.rect.height / (float)iconsPerColumn);

        //modify the size of the content container
        float iconWidth = iconHeight + rowIconPrefab.GetComponent<HorizontalLayoutGroup>().spacing;

        Vector2 contentSize = shapeIconContent.sizeDelta;
        contentSize.x = (rowJumpAt) * iconWidth;
        shapeIconContent.sizeDelta = contentSize;

        Vector2 rowDelta = rowIconPrefab.GetComponent<RectTransform>().sizeDelta;
        rowDelta.y = iconHeight;
        rowIconPrefab.GetComponent<RectTransform>().sizeDelta = rowDelta;

        for (int i = 0; i < iconsPerColumn; i++)
        {
            GameObject rowPrefab = Instantiate<GameObject>(rowIconPrefab, shapeIconContent);
            rowPrefab.name = "Row_" + i;
            RectTransform rowRT = rowPrefab.GetComponent<RectTransform>();

            rowObjects[i] = rowRT;

            rowPrefab.SetActive(true);
        }

        int rowToAssign = 0;
        for (int i = 0; i < shapes.Count; i++)
        {
            if (!IsShapeInCategory(shapes[i], categoryToDisplay))
                continue;

            GameObject nuIcon = Instantiate<GameObject>(iconPrefab, rowObjects[rowToAssign]);
            RectTransform niRT = nuIcon.GetComponent<RectTransform>();

            float iconSize = iconHeight;
            Vector2 iconSizeDelta = iconSize * Vector2.one;
            niRT.sizeDelta = iconSizeDelta;

            if ((i + 1) % rowJumpAt == 0)
                rowToAssign++;

            /*
            Vector2 iconSD = niRT.sizeDelta;
            iconSD.x = iconSD.y = iconSize;
            niRT.sizeDelta = iconSD;

            bool pass = !(rest != 0) || (currentIcons / iconsPerColumn) < (rest);

            float posX = ((currentIcons % (pass ? totalCols : (totalCols + 1))) * (iconSize + iconSpacingH)) + iconSpacingH;
            if (posX > accX)
                accX = posX;
            float posY = ((currentIcons / (pass ? totalCols : (totalCols + 1))) * (iconSize + iconSpacingV)) + iconSpacingH;
            if (posY > accY)
                accY = posY;

            niRT.anchoredPosition = new Vector2(posX, -posY);
            */

            int shapeIndex = i;
            //icon events
            niRT.GetComponent<ShapeIcon>().OnPointerDownEvent.AddListener(delegate
            {
                GrabIcon(shapeIndex);
            });

            niRT.GetChild(0).GetComponent<Image>().sprite = shapes[i].shapeIcon;
            niRT.GetChild(1).GetComponent<TextMeshProUGUI>().text = shapes[i].shapePrefab.name;

            currentIcons++;
            nuIcon.SetActive(true);
        }
    }

    private void GrabIcon(int dataIndex)
    {
        menuScrollRect.enabled = false;

        currentShapeData = shapes[dataIndex];
        currentShapeIndex = dataIndex;
        dragIconActive = true;
        iconRT = dragIconPrefab.GetComponent<RectTransform>();
        iconImage = dragIconPrefab.GetComponent<Image>();

        Vector2 iconSD = iconRT.sizeDelta;
        iconSD.x = iconSD.y = iconSize;
        iconRT.sizeDelta = iconSD;

        iconImage.sprite = currentShapeData.shapeIcon;

        dragIconPrefab.SetActive(true);
    }

    private void ReleaseIcon()
    {
        //check if cast or remove pre instance
        if (onInstanceArea)
        {
            if (!preInstanceValid)
            {
                if (preInstancedShape != null)
                    Destroy(preInstancedShape);
            }

            if(objectsCreated == null)
            {
                GameObject objectContainer = new GameObject();
                objectContainer.name = "Shape Container";
                objectsCreated = objectContainer.transform;
                objectsCreated.position = Vector3.zero;
            }

            if (preInstancedShape != null)
            {
                preInstancedShape.name = "[" + objectsCreated.childCount + "]_" + currentShapeData.shapePrefab.name;
                preInstancedShape.transform.parent = objectsCreated;
            }

            CallChangeFlag();
        }
        else if (onMenuArea)
        {
            if (preInstancedShape != null)
                Destroy(preInstancedShape);
        }

        preInstancedShape = null;
        preInstanceValid = false;

        menuScrollRect.enabled = true;

        dragIconPrefab.SetActive(false);
        dragIconActive = false;
    }

    public void ReturnObjectToContainer(Transform child)
    {
        if(objectsCreated == null)
        {
            GameObject objectContainer = new GameObject();
            objectContainer.name = "Shape Container";
            objectsCreated = objectContainer.transform;
            objectsCreated.position = Vector3.zero;
        }

        if(storedMat != null)
            child.GetChild(0).GetComponent<MeshRenderer>().material = storedMat;

        child.parent = objectsCreated;
    }

    public void ThrowShapeToFloor()
    {
        wrapper.ShapeToFloor();
    }

    public void ReleaseCurrentShape()
    {
        wrapper.TurnOff();
        if (shapeSelected != null)
        {
            shapeSelected.GetComponent<MeshRenderer>().material = storedMat;
            shapeSelected = null;
        }
        globalButton.transform.parent.gameObject.SetActive(false);
        shapeToolsMenu.gameObject.SetActive(false);

        CallChangeFlag();
    }

    public void CopyShape()
    {
        if (shapeSelected == null)
            return;

        CallChangeFlag();

        ShapeInfo baseInfo = wrapper.modelAnchor.GetChild(0).GetComponent<ShapeInfo>();

        GameObject copy = Instantiate<GameObject>(wrapper.modelAnchor.GetChild(0).gameObject);
        copy.transform.parent = objectsCreated.transform;
        copy.transform.position = wrapper.modelAnchor.GetChild(0).position;
        copy.transform.rotation = wrapper.modelAnchor.GetChild(0).rotation;
        copy.transform.localScale = wrapper.modelAnchor.GetChild(0).localScale;

        copy.name = "[" + objectsCreated.childCount + "]_" + currentShapeData.shapePrefab.name;

        copy.transform.GetChild(0).GetComponent<MeshRenderer>().material = new Material(storedMat);

        ShapeInfo bounds = copy.GetComponent<ShapeInfo>();
        bounds.SetShapeID(currentShapeIndex);
        bounds.SetColor(baseInfo.shapeColor);

        //right
        float dist = bounds.GetRightMostValue() - bounds.GetLeftMostValue();

        if (bounds.TryPosition(dist * Vector3.right))
            return;
        if (bounds.TryPosition(dist * Vector3.left))
            return;
        //forward
        dist = bounds.GetForwardMostValue() - bounds.GetBackMostValue();

        if (bounds.TryPosition(dist * Vector3.forward))
            return;
        if (bounds.TryPosition(dist * Vector3.back))
            return;
    }

    public void DeleteShape()
    {
        if (shapeSelected == null)
            return;

        GameObject toDelete = wrapper.modelAnchor.GetChild(0).gameObject;

        if(wrapper.modelAnchor.GetChild(0).GetComponent<ShapeInfo>().isDecor)
            wrapper.modelAnchor.GetChild(0).GetComponent<ShapeInfo>().DestroyMarker();

        wrapper.TurnOff();

        shapeToolsMenu.gameObject.SetActive(false);

        Destroy(toDelete);

        CallChangeFlag();
    }

    public void SetShapeDecor(Toggle toggleDecor)
    {
        if (shapeSelected == null)
            return;

        ShapeInfo bounds = shapeSelected.parent.GetComponent<ShapeInfo>();
        bounds.SetDecor(toggleDecor.isOn);
    }

    public void UpdateFromInput(int inputIndex)
    {
        wrapper.ApplyInputChange(inputIndex);
    }

    public void SetGridSnapping(SmoothToggle toggleSnap)
    {
        shapeSnapToGrid = toggleSnap.toggleValue;
    }

    public void ChangeWrapperReferenceMode(int toolMode)
    {
        shapeToolMode = (ShapesManager.ShapeToolMode)toolMode;
        bool pass = wrapper.ChangeReferenceMode(shapeToolMode);

        if (toolMode != 0 || pass)
        {
            globalButton.color = (toolMode == 0) ? selectedColor : unselectedColor;
            localButton.color = (toolMode == 1) ? selectedColor : unselectedColor;
        }

        wrapper.UpdateUIInputValues();
    }

    public void ChangeWrapperTransformMode(int transformMode)
    {
        if((ShapeWrapper.WrapperMode)transformMode != wrapper.mode)
        {
            positionInputMenu.SetActive(false);
            rotationInputMenu.SetActive(false);
            scaleInputMenu.SetActive(false);
        }

        wrapper.ChangeToolMode((ShapeWrapper.WrapperMode)transformMode);

        positionButton.color = (transformMode == 0) ? selectedColor : unselectedColor;
        positionButton.transform.GetChild(1).gameObject.SetActive(transformMode == 0);
        rotationButton.color = (transformMode == 1) ? selectedColor : unselectedColor;
        rotationButton.transform.GetChild(1).gameObject.SetActive(transformMode == 1);
        scaleButton.color = (transformMode == 2) ? selectedColor : unselectedColor;
        scaleButton.transform.GetChild(1).gameObject.SetActive(transformMode == 2);
    }

    public void ToggleTransformInputMenu(int index)
    {
        GameObject inputMenu = positionInputMenu;
        if (index == 1)
            inputMenu = rotationInputMenu;
        else if (index == 2)
            inputMenu = scaleInputMenu;

        SetTransformInputMenu(index, !inputMenu.activeSelf);
    }

    private void SetTransformInputMenu(int index, bool active)
    {
        GameObject inputMenu = positionInputMenu;
        if (index == 1)
            inputMenu = rotationInputMenu;
        else if (index == 2)
            inputMenu = scaleInputMenu;

        if (active)
        {
            if (!inputMenu.gameObject.activeSelf && ColorManager._instance.colorMenuActive)
                ColorManager._instance.ToggleColorMenu();

            positionInputMenu.SetActive(index == 0);
            rotationInputMenu.SetActive(index == 1);
            scaleInputMenu.SetActive(index == 2);

            wrapper.UpdateUIInputValues();
        }
        else
            inputMenu.SetActive(false);
    }

    public void ToggleShapeMenu()
    {
        SetShapeMenu(!shapeMenuActive);
    }

    public void SetShapeMenu(bool active)
    {
        if (active)
        {
            shapeMenuAnim.SetTrigger("Show");
            slideButtonIcon.sprite = rightArrowIcon;

            if (ColorManager._instance.colorMenuActive)
                ColorManager._instance.ToggleColorMenu();
        }
        else
        {
            shapeMenuAnim.SetTrigger("Hide");
            slideButtonIcon.sprite = leftArrowIcon;
        }

        shapeMenuActive = active;
    }

    //make shapes based
    public void LoadShapes(MapFile.SaveMapShape[] list)
    {
        if (objectsCreated == null)
        {
            GameObject objectContainer = new GameObject();
            objectContainer.name = "Shape Container";
            objectsCreated = objectContainer.transform;
            objectsCreated.position = Vector3.zero;
        }

        CleanShapeContainer();

        for(int i = 0; i < list.Length; i++)
        {
            ShapeData data = shapes[list[i].shapeID];
            GameObject loadedShape = Instantiate<GameObject>(data.shapePrefab);
            loadedShape.name = "[" + objectsCreated.childCount + "]_" + data.shapePrefab.name;

            Color shapeColor = new Color(list[i].colorRed, list[i].colorGreen, list[i].colorBlue);
            loadedShape.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = shapeColor;
            loadedShape.GetComponent<ShapeInfo>().SetColor(shapeColor);

            loadedShape.GetComponent<ShapeInfo>().SetShapeID(list[i].shapeID);
            loadedShape.GetComponent<ShapeInfo>().SetDecor(list[i].isDecor);

            loadedShape.transform.parent = objectsCreated;
            loadedShape.transform.position = new Vector3(list[i].posX, list[i].posY, list[i].posZ);
            loadedShape.transform.rotation = Quaternion.Euler(list[i].rotX, list[i].rotY, list[i].rotZ);
            loadedShape.transform.localScale = new Vector3(list[i].scaX, list[i].scaY, list[i].scaZ);
        }

        StartCoroutine(BakeDelay());
    }

    private IEnumerator BakeDelay()
    {
        yield return new WaitForSeconds(0.1f);
        BakeShapeCollisionMatrix();
    }

    public void CleanShapeContainer()
    {
        if (objectsCreated == null)
            return;

        //clean container
        for (int i = objectsCreated.childCount - 1; i >= 0; i--)
        {
            Destroy(objectsCreated.GetChild(i).gameObject);
        }
    }

    public void CloseShapeMode()
    {
        if (collisionChangedFlag)
            BakeShapeCollisionMatrix();

        wrapper.TurnOff();
        globalButton.transform.parent.gameObject.SetActive(false);
        HideCollisionGrid();

        if (shapeSelected != null)
        {
            shapeSelected.GetComponent<MeshRenderer>().material = storedMat;
            shapeSelected = null;
        }
    }


    public static bool IsShapeInCategory(ShapeData shape, ShapeCategory cat)
    {
        for(int c = 0; c < shape.categories.Length; c++)
        {
            if (shape.categories[c] == cat)
                return true;
        }

        return false;
    }

    public static bool CheckMouseInArea(RectTransform area)
    {
        bool ret = false;

        Vector2 mouseAreaLocalPosition = area.InverseTransformPoint(Input.mousePosition);
        if (area.rect.Contains(mouseAreaLocalPosition))
        {
            ret = true;
        }

        return ret;
    }

    protected Vector3 GetInstancedPosition()
    {
        Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000f, floorLayer))
        {
            return hitInfo.point;
        }
        return Vector3.negativeInfinity;
    }

    public Transform GetObjectContainer()
    {
        if (objectsCreated == null)
            return null;

        return objectsCreated;
    }

    #region Collision Matrix

    List<Vector3> auxPoints;
    List<Vector3> auxPointsPair;

    private void BakeShapeCollisionMatrix()
    {
        shapeCollisionBaked = false;

        int[] tileDimensions = MapManager._instance.mapTarget.GetMapDimensions();
        int gridHeight = MapManager._instance.mapTarget.maxCellHeight + 1; //takes in account pits

        BattleMap.BoundSet mapBounds = MapManager._instance.mapTarget.bounds;

        float cellScale = MapManager._instance.mapTarget.cellScale;
        float tileHeight = MapManager._instance.mapTarget.tileWorldHeight;

        Vector3 checkOrigin = new Vector3(mapBounds.minX, mapBounds.floorY - tileHeight, mapBounds.minZ) + (0.5f * new Vector3(cellScale, tileHeight, cellScale));

        shapeCollisionMatrix = new int[tileDimensions[0], gridHeight, tileDimensions[1]];

        Vector3[] pointSamples = new Vector3[13]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 1f),
            new Vector3(0f, 0f, -1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(-1f, -0.5f, -1f),
            new Vector3(-1f, -0.5f, 1f),
            new Vector3(1f, -0.5f, -1f),
            new Vector3(1f, -0.5f, 1f),
            new Vector3(-1f, 0.5f, -1f),
            new Vector3(-1f, 0.5f, 1f),
            new Vector3(1f, 0.5f, -1f),
            new Vector3(1f, 0.5f, 1f)
        };

        auxPoints = new List<Vector3>();
        auxPointsPair = new List<Vector3>();

        for(int k = 0; k < shapeCollisionMatrix.GetLength(2); k++)
        {
            for(int j = 0; j < shapeCollisionMatrix.GetLength(1); j++)
            {
                for(int i = 0; i < shapeCollisionMatrix.GetLength(0); i++)
                {

                    Vector3 checkCenter = checkOrigin + new Vector3(cellScale * i, cellScale * tileHeight * j, cellScale * k);

                    bool collFound = false;
                    for (int p = 0; p < pointSamples.Length; p++)
                    {
                        Vector3 pointSample = pointSamples[p];

                        Vector3 checkPoint = checkCenter;
                        checkPoint.x += ((0.5f * vertexScaleDownHorizontal * cellScale) * pointSample.x);
                        checkPoint.y += ((0.5f * vertexScaleDownVertical * cellScale) * pointSample.y);
                        checkPoint.z += ((0.5f * vertexScaleDownHorizontal * cellScale) * pointSample.z);

                        if (IsPointInsideAShape(checkPoint))
                        {
                            shapeCollisionMatrix[i, j, k] = 1;
                            collFound = true;

                            auxPoints.Add(checkPoint);
                            break;
                        }
                    }

                    if(!collFound)
                        shapeCollisionMatrix[i, j, k] = 0;
                }
            }
        }

        shapeCollisionBaked = true;
        collisionChangedFlag = false;
    }

    private bool IsPointInsideAShape(Vector3 point, bool ignoreDecor = true)
    {
        if(objectsCreated == null)
            return false;

        for(int s = 0; s < objectsCreated.childCount; s++)
        {
            ShapeInfo si = objectsCreated.GetChild(s).GetComponent<ShapeInfo>();
            if (si.isDecor && ignoreDecor)
                continue;

            MeshCollider mcCheck = objectsCreated.GetChild(s).GetChild(0).GetComponent<MeshCollider>();
            if (mcCheck != null)
            {
                if (IsInsideMeshCollider(mcCheck, point))
                    return true;
            }

            BoxCollider bcCheck = objectsCreated.GetChild(s).GetChild(0).GetComponent<BoxCollider>();
            if(bcCheck!= null)
            {
                if (IsInsiderBoxCollider(bcCheck, point))
                    return true;
            }

            SphereCollider scCheck = objectsCreated.GetChild(s).GetChild(0).GetComponent<SphereCollider>();
            if(scCheck != null)
            {
                float sca = objectsCreated.GetChild(s).GetChild(0).localScale.x;
                if (Vector3.Distance(objectsCreated.GetChild(s).GetChild(0).position, point) <= (scCheck.radius * sca))
                    return true;
            }
        }

        if(shapeSelected != null)
        {
            ShapeInfo si = shapeSelected.parent.GetComponent<ShapeInfo>();
            if (!si.isDecor) 
            {
                MeshCollider mcCheck = shapeSelected.GetComponent<MeshCollider>();
                if (mcCheck != null)
                {
                    if (IsInsideMeshCollider(mcCheck, point))
                        return true;
                }

                BoxCollider bcCheck = shapeSelected.GetComponent<BoxCollider>();
                if (bcCheck != null)
                {
                    if (IsInsiderBoxCollider(bcCheck, point))
                        return true;
                }

                SphereCollider scCheck = shapeSelected.GetComponent<SphereCollider>();
                if (scCheck != null)
                {
                    float sca = shapeSelected.localScale.x;
                    if (Vector3.Distance(shapeSelected.position, point) <= (scCheck.radius * sca))
                        return true;
                }
            }
        }

        return false;
    }

    private bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
    {
        var temp = Physics.queriesHitBackfaces;
        Ray ray = new Ray(point, Vector3.back);

        bool hitFrontFace = false;
        RaycastHit hit = default;

        Physics.queriesHitBackfaces = true;
        bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
        if (hitFrontOrBackFace)
        {
            Physics.queriesHitBackfaces = false;
            hitFrontFace = col.Raycast(ray, out hit, 100f);
        }
        Physics.queriesHitBackfaces = temp;

        if (!hitFrontOrBackFace)
        {
            return false;
        }
        else if (!hitFrontFace)
        {
            return true;
        }
        else
        {
            // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
            if (hit.distance > hit2.distance)
            {
                return true;
            }
            else
                return false;
        }

    }

    private bool IsInsiderBoxCollider(BoxCollider box, Vector3 point)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);

        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }

    public void DisplayCollisionMatrix()
    {
        if (collGridDisplayed)
            return;

        if (!shapeCollisionBaked || collisionChangedFlag)
            BakeShapeCollisionMatrix();

        GameObject collTilePrefab = collisionGridParent.GetChild(0).gameObject;

        BattleMap.BoundSet mapBounds = MapManager._instance.mapTarget.bounds;

        float cellScale = MapManager._instance.mapTarget.cellScale;
        float tileHeight = MapManager._instance.mapTarget.tileWorldHeight;

        Vector3 checkOrigin = new Vector3(mapBounds.minX, mapBounds.floorY - tileHeight, mapBounds.minZ) + (0.5f * new Vector3(cellScale, tileHeight, cellScale));


        for (int k = 0; k < shapeCollisionMatrix.GetLength(2); k++)
        {
            for (int j = 0; j < shapeCollisionMatrix.GetLength(1); j++)
            {
                for (int i = 0; i < shapeCollisionMatrix.GetLength(0); i++)
                {
                    Vector3 checkCenter = checkOrigin + new Vector3(cellScale * i, cellScale * tileHeight * j, cellScale * k);

                    if (shapeCollisionMatrix[i, j, k] == 1)
                    {
                        GameObject nuCollTile = Instantiate<GameObject>(collTilePrefab, collisionGridParent);
                        nuCollTile.name = "[" + i + "," + j + "," + k + "]";
                        Transform collTileTF = nuCollTile.transform;
                        collTileTF.position = checkCenter;
                        collTileTF.localScale = 1.05f * new Vector3(cellScale, tileHeight, cellScale);

                        nuCollTile.SetActive(true);
                    }
                }
            }
        }

        collGridDisplayed = true;
    }

    public void CallChangeFlag()
    {
        collisionChangedFlag = true;

        HideCollisionGrid();
    }

    private void HideCollisionGrid()
    {
        for (int i = collisionGridParent.childCount - 1; i > 0; i--)
        {
            Destroy(collisionGridParent.GetChild(i).gameObject);
        }

        collGridDisplayed = false;
    }

    public float CheckWithCollision(Vector3 snappedPosition, int size)
    {
        BattleMap.BoundSet mapBounds = MapManager._instance.mapTarget.bounds;

        if (!shapeCollisionBaked || collisionChangedFlag)
            BakeShapeCollisionMatrix();

        float cellScale = MapManager._instance.mapTarget.cellScale;
        float tileHeight = MapManager._instance.mapTarget.tileWorldHeight;
        Vector3 checkOrigin = new Vector3(mapBounds.minX, mapBounds.floorY - tileHeight, mapBounds.minZ) + (0.5f * new Vector3(cellScale, tileHeight, cellScale));

        Vector3 coodVect = snappedPosition - checkOrigin;

        int i = Mathf.FloorToInt(coodVect.x / cellScale);
        int j = Mathf.FloorToInt(coodVect.y / (cellScale * tileHeight)) + 1; //lowest height we'll be getting would be -1
        int k = Mathf.FloorToInt(coodVect.z / cellScale);

        int iSteps = size;
        int kSteps = iSteps;
        int jSteps = size * Mathf.FloorToInt(1f / tileHeight);

        int heighestPoint = j;

        for(int kz = 0; kz < kSteps; kz++)
        {
            for(int ix = 0; ix < iSteps; ix++)
            {
                //we check the clear berth up to see if the piece fits
                int auxj = j;
                bool remakeLoop = true;
                while (remakeLoop)
                {
                    remakeLoop = false;
                    for(int jy = 0; jy < jSteps; jy++)
                    {
                        if((auxj + jy) >= shapeCollisionMatrix.GetLength(1))
                        {
                            //surpassed upper limit
                            if (auxj > heighestPoint)
                                heighestPoint = auxj;
                            break;
                        }

                        bool outOfBounds = ((i + ix) > shapeCollisionMatrix.GetLength(0) || (i + ix) < 0) ||
                            ((auxj + jy) > shapeCollisionMatrix.GetLength(1) || (auxj + jy) < 0) ||
                            ((k + kz) > shapeCollisionMatrix.GetLength(2) || (k + kz) < 0);

                        if (outOfBounds)
                            continue;

                        if (shapeCollisionMatrix[i + ix, auxj + jy, k + kz] == 0)
                        {
                            continue;
                        }
                        else //collision, move index to this one + 1. try again from here
                        {
                            auxj = auxj + jy + 1;
                            if (auxj > heighestPoint)
                                heighestPoint = auxj;
                            remakeLoop = true;
                            break;
                        }
                    }
                }
            }
        }

        if(heighestPoint >= shapeCollisionMatrix.GetLength(1)) //out of upper bounds
            return float.PositiveInfinity;
        else
        {
            return (cellScale * tileHeight * (heighestPoint - 1));
        }
    }

    /*
    private void OnDrawGizmos()
    {
        if (!shapeCollisionBaked)
            return;

        Gizmos.color = Color.red;
        for(int i = 0; i < auxPoints.Count; i++)
        {
            Gizmos.DrawSphere(auxPoints[i], 0.05f);
        }

        Gizmos.color = Color.yellow;

        BattleMap.BoundSet mapBounds = MapManager._instance.mapTarget.bounds;

        float cellScale = MapManager._instance.mapTarget.cellScale;
        float tileHeight = MapManager._instance.mapTarget.tileWorldHeight;

        Vector3 checkOrigin = new Vector3(mapBounds.minX, mapBounds.floorY - tileHeight, mapBounds.minZ) + (0.5f * new Vector3(cellScale, tileHeight, cellScale));


        for (int k = 0; k < shapeCollisionMatrix.GetLength(2); k++)
        {
            for (int j = 0; j < shapeCollisionMatrix.GetLength(1); j++)
            {
                for (int i = 0; i < shapeCollisionMatrix.GetLength(0); i++)
                {
                    Vector3 checkCenter = checkOrigin + new Vector3(cellScale * i, cellScale * tileHeight * j, cellScale * k);

                    if (shapeCollisionMatrix[i, j, k] == 1)
                        Gizmos.DrawSphere(checkCenter, 0.2f);
                }
            }
        }
    }
    */

    #endregion
}
