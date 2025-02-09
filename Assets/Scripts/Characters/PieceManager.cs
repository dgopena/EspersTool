using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

public class PieceManager : MonoBehaviour
{
    public static PieceManager _instance;

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        grabbingActive = true;
    }

    private ManagerState currentState;

    private List<UnitPiece> castedPieces;

    [Range(0, 8)]
    public int pieceMaxSize = 6;
    [Space(10f)]
    [SerializeField] private GameObject characterPiecePrefab;
    [SerializeField] private GameObject foePiecePrefab;
    [SerializeField] private GameObject tokenPiecePrefab;

    [SerializeField] private RectTransform characterUnitPanel;
    [SerializeField] private RectTransform foeUnitPanel;
    [SerializeField] private RectTransform pieceRotationButtons;
    public Vector3 pieceOptionsAdjustments = new Vector3(0f, 0.2f, 0f);

    [Space(10f)]
    [SerializeField] private float pieceRotationSpeed = 180f;

    [Space(10f)]
    public float graphicPieceScaleUp = 2f;

    [Space(10f)]
    [SerializeField] private float grabTiming = 0.6f;
    [SerializeField] private float grabHover = 0.25f;
    private float grabTimeStamp;
    private bool pieceGrabbing = false;
    public UnityEngine.UI.Image grabFillCircle;
    public float grabFillCircleSize = 0.34f;
    [SerializeField] private float pieceAllocationPermisiveness = 0.1f; //factor of the tile height it allows as a valid positioning

    [Space(10f)]
    public Material pieceMat;
    public Material pieceMatFade;
    [Space(5f)]
    public Material graphicPieceMat;
    public Material graphicPieceMatFade;

    [Space(10f)]
    [Header("Piece Panel")]
    [SerializeField] private PieceDisplay pieceDisplayPanel;

    public PieceDisplay PieceDisplayPanel => pieceDisplayPanel;

    public bool showMiniPanels = true;
    [SerializeField] private GameObject miniPanelPrefab;
    [SerializeField] private Transform miniPanelContainer;
    public Vector3 miniPanelAdjustment = new Vector3(0f, -0.2f, 0f);

    [Space(10f)]
    [SerializeField] private float miniPanelHoldInitialCooldown = 1f;
    [SerializeField] private float miniPanelHoldConstantCooldown = 0.2f;
    private bool miniPanelHolding = false;
    private float currentHoldCooldown = 0f;

    private int miniPanelShow = 1; //0 - not show, 1 - show only active, 2 - show all
    private bool panelShowState = false;

    [Space(5f)]
    public StatusData statusInfo;
    public Color possitiveEffectColor;
    public Color statusEffectColor;
    public Color blightColor;

    [Space(10f)]
    [SerializeField] private UnityEngine.UI.Toggle allActiveToggle;
    [SerializeField] private UnityEngine.UI.Toggle onlyActiveToggle;
    [SerializeField] private UnityEngine.UI.Toggle noneActiveToggle;

    private int activePieceType; // 0 - chara, 1 - foe, 2 - token
    private CharacterPiece activeCharacterPiece;
    private FoePiece activeFoePiece;
    private TokenPiece activeTokenPiece;

    [SerializeField] private GameObject infoPanelBackdrop;
    [SerializeField] private CharacterUnitPanel characterInfoPanel;
    [SerializeField] private FoeUnitPanel foeInfoPanel;
    [SerializeField] private GameObject unloadedPiecesWarning;

    private bool pieceOptionButtonActive = false;
    private bool changedPiece = false;
    private bool preClickButtonValues = false;

    private bool pieceGrabbed = false;
    public bool grabbingActive { get; private set; }

    public bool hoverOnPiece { get; private set; }
    private int hoveredID = -1;
    private UnitPiece pieceHovered;

    public UnityAction OnPieceAdded;
    public UnityAction OnPieceRemoved;

    [Header("UI")] 
    public Canvas mainCanvas;
    
    [Space(10f)]
    public GameObject cardOptionButton;

    [Space(10f)]
    public GameObject unitModeRoot;
    public GameObject pieceDeleteWarning;
    public GameObject modeButton;
    public GameObject optionsButton;

    [Space(10f)] 
    [SerializeField] public RectTransform pieceDeckParent;
    public GameObject cardDeckPrefab;
    public GameObject cardHandWidgetPrefab;
    
    public bool pieceBeingEdited { get; private set; }

    [Header("General Panel")]
    [SerializeField] private GameObject gridOptionsPanel;
    
    [Header("Token Variables")]
    public RectTransform tokenPieceMainPanel;
    public Vector2 tokenMainPanelDeltaPos = new Vector2(0.2f, 0.1f);
    public Vector2 tokenSubPanelDeltaPos = new Vector2(0f, 0.2f);
    public RectTransform tokenPieceIndividualPanelParent;
    private bool showingTokenMainPanel = false;
    public TMP_InputField tokenPanelInputText;
    
    //right click menu
    private RectTransform listRT;
    private bool rightMenuOpen;

    //piece options
    public bool pieceAutoSize { get; private set; }

    private enum ManagerState
    {
        None,
        MovingPiece,
        EditingPiece
    }

    private void LateUpdate()
    {
        if (currentHoldCooldown > 0f)
            currentHoldCooldown -= Time.unscaledDeltaTime;

        if (rightMenuOpen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!TooltipManager.CheckMouseInArea(listRT))
                {
                    rightMenuOpen = false;
                    UnitManager._instance.listPanel.ShowPanel(false);
                    if (activePieceType == 0)
                        UnitManager._instance.listPanel.OnEntryClick -= CharaRightMenuClick;
                }
            }

            //update position if such
            Vector3 trackingWorldPos = Vector3.zero;
            if (activePieceType == 0)
                trackingWorldPos = activeCharacterPiece.transform.position;
            else if (activePieceType == 1)
                trackingWorldPos = activeFoePiece.transform.position;
            else if (activePieceType == 2)
                trackingWorldPos = activeTokenPiece.transform.position;

            Vector3 uiPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(trackingWorldPos);
            listRT.position = uiPos;
        }

        if (!grabbingActive || (MapManager._instance.toolMode != MapManager.ToolMode.UnitMaker && MapManager._instance.toolMode != MapManager.ToolMode.GameMode))
        {
            if (hoverOnPiece)
            {
                hoverOnPiece = false;
                if (pieceHovered != null)
                    pieceHovered.SetFocus(false);
                hoveredID = -1;
            }
            return;
        }

        UnitPiece activePiece = activeTokenPiece as UnitPiece;
        if (activePieceType == 0)
            activePiece = (activeCharacterPiece as UnitPiece);
        else if(activePieceType == 1)
            activePiece = (activeFoePiece as UnitPiece);

        if (pieceGrabbed)
        {
            hoverOnPiece = true;

            if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode) // && GameModeManager._instance.currentToolMode != GameModeManager.ToolMode.None)
                return;

            GameModeManager._instance.ChangeCursor(GameModeManager.CursorTypes.Circle);

            Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Input.GetMouseButtonUp(0))
            {
                hoverOnPiece = false;

                activePiece.transform.position -= (grabHover * Vector3.up);
                /*
                if (activePieceType != 2)
                {
                    activePiece.RelocateMiniPanel();
                    activePiece.miniPanel.gameObject.SetActive(true);
                }
                */
                currentState = ManagerState.None;
                pieceGrabbed = false;

                activePiece.SetMovingFrame(false);

                //dont show if no message

                if (activeTokenPiece != null)
                {
                    bool tokenMessageIsNotEmpty = activeTokenPiece.tokenMessage != null && activeTokenPiece.tokenMessage.Length > 0;
                    if (tokenMessageIsNotEmpty)
                    {
                        activeTokenPiece.tokenTextPanel.gameObject.SetActive(true);
                        activeTokenPiece.UpdateTextPanelPosition();
                    }
                }
            }

            if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.terrainLayer) && pieceGrabbed)
            {
                //check validity on map target
                Vector3 tentPos = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point);

                //height pass
                float targetHeight = MapManager._instance.mapTarget.GetTileHeightAt(tentPos);

                bool pieceCollisionPass = true;

                /*
                if (activeTokenPiece == null)
                    pieceCollisionPass = !IsPieceCollidingWithPiece(tentPos, activePiece.transform.GetInstanceID());
                */

                bool cellPass = (targetHeight < (0.5f * MapManager._instance.mapTarget.maxCellHeight)) && pieceCollisionPass;

                float collisionHeight = CheckPieceShapeCollision(tentPos, targetHeight, activePiece.pieceSize);

                if (float.IsPositiveInfinity(collisionHeight))
                    cellPass = false;
                else
                    tentPos.y = collisionHeight;

                if (cellPass)
                {
                    activePiece.transform.position = tentPos + (grabHover * Vector3.up);
                    activePiece.SetMapPosition(activePiece.transform.position);
                }
            }
            else if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.outerEdgeLayer) && pieceGrabbed)
            {
                activePiece.SetOuterMapPosition(hitInfo.point + (grabHover * Vector3.up));
            }
        }
        else if (!MapManager._instance.eventSystem.IsPointerOverGameObject())
        {
            Ray ray = MapManager._instance.activeCamera.cameraComp.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            hoverOnPiece = false;
            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                if (hitInfo.transform.tag == "Character")
                {
                    hoverOnPiece = true;

                    int curID = hitInfo.transform.GetInstanceID();

                    if (hoveredID != curID)
                    {
                        if (pieceHovered != null)
                            pieceHovered.SetFocus(false);

                        pieceHovered = hitInfo.transform.parent.GetComponent<UnitPiece>();
                        pieceHovered.SetFocus(true);

                        hoveredID = curID;
                    }
                }
            }

            //control shape of the "cursor"
            if (hoverOnPiece)
                GameModeManager._instance.ChangeCursor(GameModeManager.CursorTypes.Circle);
            else
            {
                if (pieceHovered != null)
                    pieceHovered.SetFocus(false);
                hoveredID = -1;

                GameModeManager._instance.ChangeCursor(GameModeManager.CursorTypes.Regular);
            }

            //left click
            if (Input.GetMouseButtonDown(0))
            {
                /*
                if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode) // && GameModeManager._instance.currentToolMode != GameModeManager.ToolMode.None)
                    return;
                */

                preClickButtonValues = pieceOptionButtonActive;
                changedPiece = false;

                bool charaHit = false;

                if (hoverOnPiece)
                {
                    charaHit = true;

                    int currentID = -1;
                    if (activeCharacterPiece != null)
                        currentID = activeCharacterPiece.transform.GetInstanceID();
                    else if (activeFoePiece != null)
                        currentID = activeFoePiece.transform.GetInstanceID();
                    else if (activeTokenPiece != null)
                        currentID = activeTokenPiece.transform.GetInstanceID();

                    activeCharacterPiece = hitInfo.transform.parent.GetComponent<CharacterPiece>();
                    activeFoePiece = hitInfo.transform.parent.GetComponent<FoePiece>();
                    activeTokenPiece = hitInfo.transform.parent.GetComponent<TokenPiece>();

                    if (activeCharacterPiece != null)
                        activePieceType = 0;
                    else if (activeFoePiece != null)
                        activePieceType = 1;
                    else if (activeTokenPiece != null)
                        activePieceType = 2;

                    if (activeCharacterPiece != null)
                    {
                        if (currentID == activeCharacterPiece.transform.GetInstanceID())
                            pieceOptionButtonActive = !pieceOptionButtonActive;
                        else
                        {
                            pieceOptionButtonActive = false;
                            changedPiece = true;
                        }

                        if(tokenPieceMainPanel)
                            tokenPieceMainPanel.gameObject.SetActive(false);
                        
                        if (pieceOptionButtonActive && MapManager._instance.toolMode != MapManager.ToolMode.GameMode)
                        {
                            pieceRotationButtons.GetChild(2).gameObject.SetActive(true);
                            pieceRotationButtons.GetChild(3).gameObject.SetActive(true);

                            activeCharacterPiece.gameObject.SetActive(true);
                        }
                    }
                    else if (activeFoePiece != null)
                    {
                        if (currentID == activeFoePiece.transform.GetInstanceID())
                            pieceOptionButtonActive = !pieceOptionButtonActive;
                        else
                        {
                            pieceOptionButtonActive = false;
                            changedPiece = true;
                        }

                        tokenPieceMainPanel.gameObject.SetActive(false);
                        if (pieceOptionButtonActive && MapManager._instance.toolMode != MapManager.ToolMode.GameMode)
                        {
                            pieceRotationButtons.GetChild(2).gameObject.SetActive(true);
                            pieceRotationButtons.GetChild(3).gameObject.SetActive(true);
                        }
                    }
                    else if (activeTokenPiece != null)
                    {
                        if (currentID == activeTokenPiece.transform.GetInstanceID())
                            showingTokenMainPanel = !showingTokenMainPanel;
                        else
                        {
                            showingTokenMainPanel = false;
                            changedPiece = true;
                        }

                        if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode)
                        {
                            pieceOptionButtonActive = false;

                            //CloseAllMiniPanels(); //patch solution. should look more closely on deselection upon choosing a token piece

                            tokenPieceMainPanel.gameObject.SetActive(showingTokenMainPanel);

                            if (pieceDisplayPanel.gameObject.activeInHierarchy && showingTokenMainPanel)
                                SetDisplayPanelActive(false);

                            if (showingTokenMainPanel)
                            {
                                activeTokenPiece.tokenTextPanel.gameObject.SetActive(false);
                                Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(activePiece.GetModelPosition());
                                screenPos += (tokenMainPanelDeltaPos.x * Screen.width * Vector3.right) + (tokenMainPanelDeltaPos.y * Screen.height * Vector3.up);

                                tokenPieceMainPanel.position = screenPos;
                            }
                            else
                                activeTokenPiece.TryShowMessage();

                            if (changedPiece)
                            {
                                RebuildTokenMainPanel();
                            }
                        }
                        else
                        {
                            if (currentID == activeTokenPiece.transform.GetInstanceID())
                                pieceOptionButtonActive = !pieceOptionButtonActive;
                            else
                            {
                                pieceOptionButtonActive = false;
                                changedPiece = true;
                            }

                            if (pieceOptionButtonActive)
                            {
                                pieceRotationButtons.GetChild(2).gameObject.SetActive(false);
                                pieceRotationButtons.GetChild(3).gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    pieceOptionButtonActive = false;

                    DeselectPiece();
                }

                if (charaHit)
                {
                    grabTimeStamp = Time.time;
                    grabFillCircle.GetComponent<RectTransform>().position = Input.mousePosition;
                    grabFillCircle.GetComponent<RectTransform>().sizeDelta = grabFillCircleSize * Screen.height * Vector2.one;
                    grabFillCircle.fillAmount = 0f;
                    grabFillCircle.gameObject.SetActive(true);
                    pieceGrabbing = true;
                }
                else
                    pieceGrabbing = false;
            }
            else if (Input.GetMouseButton(0) && pieceGrabbing)
            {
                if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode) // && GameModeManager._instance.currentToolMode != GameModeManager.ToolMode.None)
                    return;

                if ((activeCharacterPiece != null || activeFoePiece != null || activeTokenPiece != null) && !pieceGrabbed)
                {
                    float timeDiff = (Time.time - grabTimeStamp);
                    if (timeDiff > grabTiming)
                    {
                        //grab piece and move
                        pieceGrabbed = true;
                        pieceGrabbing = false;
                        currentState = ManagerState.MovingPiece;
                        SetDisplayPanelActive(false);

                        activePiece.SetMovingFrame(true);

                        showingTokenMainPanel = false;
                        tokenPieceMainPanel.gameObject.SetActive(false);

                        if (activeTokenPiece != null)
                            activeTokenPiece.tokenTextPanel.gameObject.SetActive(false);

                        grabFillCircle.gameObject.SetActive(false);

                        if (!pieceOptionButtonActive)
                            SetPieceButtonOptions(false);
                    }
                    else
                        grabFillCircle.fillAmount = (timeDiff / grabTiming);
                }
            }

            //right click
            if (Input.GetMouseButtonDown(1) && !pieceGrabbing && hoverOnPiece)
            {
                int currentID = -1;
                if (activeCharacterPiece != null)
                    currentID = activeCharacterPiece.transform.GetInstanceID();
                else if (activeFoePiece != null)
                    currentID = activeFoePiece.transform.GetInstanceID();
                else if (activeTokenPiece != null)
                    currentID = activeTokenPiece.transform.GetInstanceID();

                activeCharacterPiece = hitInfo.transform.parent.GetComponent<CharacterPiece>();
                activeFoePiece = hitInfo.transform.parent.GetComponent<FoePiece>();
                activeTokenPiece = hitInfo.transform.parent.GetComponent<TokenPiece>();

                if (activeCharacterPiece != null)
                    activePieceType = 0;
                else if (activeFoePiece != null)
                    activePieceType = 1;
                else if (activeTokenPiece != null)
                    activePieceType = 2;

                if (activePieceType < 2)
                {
                    UnitRightClickMenu();
                }
                else if(activePieceType == 2)
                {

                }
            }

            //release left click
            if (Input.GetMouseButtonUp(0))
            {
                grabFillCircle.gameObject.SetActive(false);
                pieceGrabbing = false;

                if (preClickButtonValues != pieceOptionButtonActive || changedPiece)
                {
                    SetPieceButtonOptions(pieceOptionButtonActive);
                }
                
                if ((MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker) || (MapManager._instance.toolMode == MapManager.ToolMode.GameMode)) // && GameModeManager._instance.currentToolMode == GameModeManager.ToolMode.None))
                    SetDisplayPanelActive(true);
            }

            if (hoverOnPiece)
            {
                if (Input.mouseScrollDelta.y != 0f)
                {
                    bool right = Input.GetAxis("Mouse ScrollWheel") > 0;
                    hitInfo.transform.parent.GetComponent<UnitPiece>().RotatePiece((right ? -1f : 1f) * 10 * pieceRotationSpeed * Time.deltaTime);
                }
            }
        }
        else
            GameModeManager._instance.ChangeCursor(GameModeManager.CursorTypes.Regular);

        if (pieceOptionButtonActive && activePiece != null)
        {
            Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(activePiece.GetModelPosition());
            screenPos += pieceOptionsAdjustments.y * Screen.height * Vector3.up;
            screenPos += pieceOptionsAdjustments.x * Screen.width * Vector3.right;

            pieceRotationButtons.position = screenPos;
        }

        bool cameraChanged = MapManager._instance.activeCamera.CameraChange();

        /*
        if (cameraChanged && showMiniPanels && castedPieces != null)
        {
            foreach(UnitPiece piece in castedPieces)
            {
                piece.RelocateMiniPanel();
            }
        }
        */

        if(cameraChanged)
        {
            //update all token labels
            foreach(UnitPiece piece in castedPieces)
            {
                if(piece is TokenPiece)
                    (piece as TokenPiece).UpdateTextPanelPosition();
            }

            if (showingTokenMainPanel && activePieceType == 2)
            {
                Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(activePiece.GetModelPosition());
                screenPos += (tokenMainPanelDeltaPos.x * Screen.width * Vector3.right) + (tokenMainPanelDeltaPos.y * Screen.height * Vector3.up);

                tokenPieceMainPanel.position = screenPos;
            }
        }
    }

    public void DeselectPiece()
    {
        Debug.Log("called deselect");

        SetDisplayPanelActive(false);

        if(activeTokenPiece != null)
        {
            activeTokenPiece.SetTokenText(activeTokenPiece.tokenMessage);
            tokenPieceMainPanel.gameObject.SetActive(false);
        }

        activeCharacterPiece = null;
        activeFoePiece = null;
        activeTokenPiece = null;

        activePieceType = -1;

        pieceOptionButtonActive = false;
        changedPiece = true;
    }

    public void SpawnGrabbedCharacterPiece(EsperCharacter toSpawn)
    {
        Debug.Log("spawn chara call: " + toSpawn.unitName);

        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.terrainSetLayers))
        {
            Vector3 mapPos = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point);
            toSpawn = toSpawn.MakeCopyForNewPiece();
            toSpawn.SetFreshFlag(true);
            Transform pieceObj = SpawnCharacterPiece(toSpawn, mapPos, false);

            activeCharacterPiece = pieceObj.GetComponent<CharacterPiece>();
            activePieceType = 0;
            activeFoePiece = null;
            activeTokenPiece = null;

            activeCharacterPiece.SetMovingFrame(true);

            pieceGrabbed = true;
            currentState = ManagerState.MovingPiece;
        }
        else
        {
            Debug.Log("out of layers");

            Vector3 planeNormal = Vector3.up;

            float denom = Vector3.Dot(planeNormal, ray.direction);
            if(Mathf.Abs(denom) > 0.0001f)
            {
                float mag = Vector3.Dot((-1f * ray.origin), planeNormal);
                Vector3 mapPos = ray.origin + (mag * ray.direction);

                toSpawn.SetFreshFlag(true);
                Transform pieceObj = SpawnCharacterPiece(toSpawn, mapPos, false);

                activeCharacterPiece = pieceObj.GetComponent<CharacterPiece>();
                activePieceType = 0;
                activeFoePiece = null;
                activeTokenPiece = null;

                activeCharacterPiece.SetMovingFrame(true);

                pieceGrabbed = true;
                currentState = ManagerState.MovingPiece;
            }
        }

        if (OnPieceAdded != null)
            OnPieceAdded.Invoke();
    }

    //toSpawn must given from the unitmanager value (fresh one) or loaded from the map piece file (not fresh) so the data mantains
    public Transform SpawnCharacterPiece(EsperCharacter toSpawn, Vector3 spawnPosition, bool loadedPiece, bool onMap = true)
    {
        GameObject nuPiece = Instantiate<GameObject>(characterPiecePrefab, transform);
        Transform nuPieceObj = nuPiece.transform;
        nuPieceObj.position = spawnPosition;

        //set up character
        CharacterPiece pieceLogic = nuPiece.GetComponent<CharacterPiece>();

        pieceLogic.GiveData(toSpawn);
        pieceLogic.BuildPiece();
        if (onMap)
            pieceLogic.SetMapPosition(spawnPosition);
        else
            pieceLogic.SetOuterMapPosition(spawnPosition);
        if (!loadedPiece)
        {
            pieceLogic.SetUsingDeck(true);
            pieceLogic.SetFateDeck(mainCanvas);
        }
        
        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();
        castedPieces.Add(pieceLogic);

        GameObject nuMiniPanel = Instantiate<GameObject>(miniPanelPrefab, miniPanelContainer);
        RectTransform miniPanelRT = nuMiniPanel.GetComponent<RectTransform>();
        miniPanelRT.SetAsFirstSibling();

        //pieceLogic.GiveMiniPanel(miniPanelRT);

        return nuPieceObj;
    }

    public void DespawnCharacterPiece(EsperCharacter toDespawn)
    {
        Debug.Log("despawn chara call: " + toDespawn.unitName);

        if(pieceGrabbed && activePieceType == 0) 
        {
            if(activeCharacterPiece.characterData.unitID == toDespawn.unitID)
            {
                currentState = ManagerState.None;
                pieceGrabbed = false;
            }
        }

        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i] is CharacterPiece)
            {
                if ((activeCharacterPiece as CharacterPiece).characterData.unitID == toDespawn.unitID && activeCharacterPiece.transform.GetInstanceID() == castedPieces[i].transform.GetInstanceID())
                {
                    Destroy(castedPieces[i].gameObject);
                    castedPieces.RemoveAt(i);
                    return;
                }
            }
        }

        if (OnPieceRemoved != null)
            OnPieceRemoved.Invoke();
    }

    public void DespawnCharacterPiece(CharacterPiece toDespawn)
    {
        Debug.Log("despawn piece call: " + toDespawn.characterData.unitName);

        if (pieceGrabbed && activePieceType == 0)
        {
            if (activeCharacterPiece.characterData.unitID == toDespawn.characterData.unitID)
            {
                currentState = ManagerState.None;
                pieceGrabbed = false;
            }
        }

        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i] is CharacterPiece)
            {
                if (castedPieces[i].transform.GetInstanceID() == toDespawn.transform.GetInstanceID())
                {
                    Destroy(castedPieces[i].gameObject);
                    castedPieces.RemoveAt(i);
                    return;
                }
            }
        }

        if (OnPieceRemoved != null)
            OnPieceRemoved.Invoke();
    }

    public void SpawnGrabbedFoePiece(EsperFoe toSpawn)
    {
        Debug.Log("spawn foe call: " + toSpawn.unitName);

        Ray ray = MapManager._instance.activeCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 100f, MapManager._instance.mapTarget.terrainSetLayers))
        {
            Vector3 mapPos = MapManager._instance.mapTarget.TranslateToGridPosition(hitInfo.point);
            toSpawn.SetFreshFlag(true);
            Transform pieceObj = SpawnFoePiece(toSpawn, mapPos);

            activeFoePiece = pieceObj.GetComponent<FoePiece>();
            activePieceType = 1;
            activeCharacterPiece = null;
            activeTokenPiece = null;

            activeFoePiece.SetMovingFrame(true);

            pieceGrabbed = true;
            currentState = ManagerState.MovingPiece;
        }
        else
        {
            Debug.Log("out of layers");

            Vector3 planeNormal = Vector3.up;

            float denom = Vector3.Dot(planeNormal, ray.direction);
            if (Mathf.Abs(denom) > 0.0001f)
            {
                float mag = Vector3.Dot((-1f * ray.origin), planeNormal);
                Vector3 mapPos = ray.origin + (mag * ray.direction);

                toSpawn.SetFreshFlag(true);
                Transform pieceObj = SpawnFoePiece(toSpawn, mapPos);

                activeFoePiece = pieceObj.GetComponent<FoePiece>();
                activePieceType = 1;
                activeCharacterPiece = null;
                activeTokenPiece = null;

                activeFoePiece.SetMovingFrame(true);

                pieceGrabbed = true;
                currentState = ManagerState.MovingPiece;
            }
        }

        if (OnPieceAdded != null)
            OnPieceAdded.Invoke();
    }

    //toSpawn must given from the unitmanager value (fresh one) or loaded from the map piece file (not fresh) so the data mantains
    public Transform SpawnFoePiece(EsperFoe toSpawn, Vector3 spawnPosition, bool onMap = true, bool fromLoad = false)
    {
        GameObject nuPiece = Instantiate<GameObject>(foePiecePrefab, transform);
        Transform nuPieceObj = nuPiece.transform;
        nuPieceObj.position = spawnPosition;

        //set up foe
        FoePiece pieceLogic = nuPiece.GetComponent<FoePiece>();
        pieceLogic.GiveData(toSpawn);
        pieceLogic.BuildPiece();
        if (onMap)
            pieceLogic.SetMapPosition(spawnPosition);
        else
            pieceLogic.SetOuterMapPosition(spawnPosition);

        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();
        castedPieces.Add(pieceLogic);

        GameObject nuMiniPanel = Instantiate<GameObject>(miniPanelPrefab, miniPanelContainer);
        RectTransform miniPanelRT = nuMiniPanel.GetComponent<RectTransform>();
        miniPanelRT.SetAsFirstSibling();

        //pieceLogic.GiveMiniPanel(miniPanelRT);

        return nuPieceObj;
    }

    public void DespawnFoePiece(EsperFoe toDespawn)
    {
        Debug.Log("despawn foe call: " + toDespawn.unitName);

        if (pieceGrabbed && activePieceType == 1)
        {
            if (activeFoePiece.foeData.unitID == toDespawn.unitID)
            {
                currentState = ManagerState.None;
                pieceGrabbed = false;
            }
        }

        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i] is FoePiece)
            {
                if (activeFoePiece.foeData.unitID == toDespawn.unitID && activeFoePiece.transform.GetInstanceID() == castedPieces[i].transform.GetInstanceID())
                {
                    Destroy(castedPieces[i].gameObject);
                    castedPieces.RemoveAt(i);
                    return;
                }
            }
        }

        if (OnPieceRemoved != null)
            OnPieceRemoved.Invoke();
    }

    public void DespawnFoePiece(FoePiece toDespawn)
    {
        Debug.Log("despawn piece call: " + toDespawn.foeData.unitName);

        if (pieceGrabbed && activePieceType == 1)
        {
            if (activeFoePiece.foeData.unitID == toDespawn.foeData.unitID)
            {
                currentState = ManagerState.None;
                pieceGrabbed = false;
            }
        }

        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i] is FoePiece)
            {
                if (castedPieces[i].transform.GetInstanceID() == toDespawn.transform.GetInstanceID())
                {
                    Destroy(castedPieces[i].gameObject);
                    castedPieces.RemoveAt(i);
                    return;
                }
            }
        }

        if (OnPieceRemoved != null)
            OnPieceRemoved.Invoke();
    }

    public Transform SpawnTokenPiece(Vector3 tokenPos, Color tokenColor, string tokenMessage, string graphicID)
    {
        GameObject nuPiece = Instantiate<GameObject>(tokenPiecePrefab, transform);
        Transform nuPieceObj = nuPiece.transform;
        nuPieceObj.position = tokenPos;

        GameObject nuTokenPanel = Instantiate<GameObject>(tokenPieceIndividualPanelParent.GetChild(0).gameObject, tokenPieceIndividualPanelParent);
        RectTransform tokenPanelRT = nuTokenPanel.GetComponent<RectTransform>();

        //set up token
        TokenPiece pieceLogic = nuPiece.GetComponent<TokenPiece>();
        pieceLogic.BuildToken(1, tokenColor, tokenPanelRT, graphicID);
        pieceLogic.SetMapPosition(tokenPos);
        pieceLogic.SetTokenText(tokenMessage);

        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();
        castedPieces.Add(pieceLogic);

        return nuPieceObj;
    }

    public void DespawnActiveTokenPiece()
    {
        if (activeTokenPiece == null)
            return;

        DespawnTokenPiece(activeTokenPiece);
    }

    public void DespawnTokenPiece(TokenPiece toDespawn)
    {
        if (pieceGrabbed && activePieceType == 2)
        {
            if (activeTokenPiece.transform.GetInstanceID() == toDespawn.transform.GetInstanceID())
            {
                currentState = ManagerState.None;
                pieceGrabbed = false;
            }
        }

        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i] is TokenPiece)
            {
                if (activeTokenPiece.transform.GetInstanceID() == toDespawn.transform.GetInstanceID() && activeTokenPiece.transform.GetInstanceID() == castedPieces[i].transform.GetInstanceID())
                {
                    showingTokenMainPanel = false;
                    tokenPieceMainPanel.gameObject.SetActive(false);
                    Destroy(activeTokenPiece.tokenTextPanel.gameObject);
                    Destroy(castedPieces[i].gameObject);
                    castedPieces.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public void CloneActiveTokenPiece()
    {
        if (activeTokenPiece == null)
            return;

        CloneTokenPiece(activeTokenPiece);
    }

    public void CloneTokenPiece(TokenPiece toClone)
    {
        Debug.Log("tring to clone " + toClone.unitName);

        Vector3 positionToCloneIn = toClone.transform.position;
        bool clonePosPass = false;

        //we search for available positions
        for (int i = 0; i < 4; i++)
        {
            Vector3 tryout = Vector3.left;
            if (i == 1)
                tryout = Vector3.back;
            else if (i == 2)
                tryout = Vector3.right;
            else if (i == 3)
                tryout = Vector3.forward;

            //check validity on map target
            Vector3 tentPos = MapManager._instance.mapTarget.TranslateToGridPosition(toClone.transform.position + tryout);

            //height pass
            float targetHeight = MapManager._instance.mapTarget.GetTileHeightAt(tentPos);

            bool pieceCollisionPass = true;

            bool cellPass = (targetHeight < (0.5f * MapManager._instance.mapTarget.maxCellHeight)) && pieceCollisionPass;

            float collisionHeight = CheckPieceShapeCollision(tentPos, targetHeight, 1);

            if (float.IsPositiveInfinity(collisionHeight))
                cellPass = false;
            else
                tentPos.y = collisionHeight;

            if (cellPass)
            {
                positionToCloneIn = tentPos;
                clonePosPass = true;
                break;
            }
        }

        if (!clonePosPass)
        {
            Debug.Log("[PieceManager] Was not able to find valid position to clone the token in");
            return;
        }

        GameObject nuPiece = Instantiate<GameObject>(toClone.gameObject, transform);
        Transform nuPieceObj = nuPiece.transform;
        nuPieceObj.position = positionToCloneIn;

        GameObject nuTokenPanel = Instantiate<GameObject>(tokenPieceIndividualPanelParent.GetChild(0).gameObject, tokenPieceIndividualPanelParent);
        RectTransform tokenPanelRT = nuTokenPanel.GetComponent<RectTransform>();

        //set up token
        TokenPiece pieceLogic = nuPiece.GetComponent<TokenPiece>();
        pieceLogic.BuildToken(1, toClone.GetPieceColor(), tokenPanelRT, toClone.pieceImageID);
        pieceLogic.SetMapPosition(positionToCloneIn);
        pieceLogic.SetTokenText(toClone.tokenMessage);

        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();
        castedPieces.Add(pieceLogic);
    }

    public void ChangeActiveTokenImage()
    {
        if (activeTokenPiece == null)
            return;

        ChangeTokenImage(activeTokenPiece);
    }

    public void ChangeTokenImage(TokenPiece toChange)
    {
        GraphicPieceEditor.Instance.OpenTokenProcess(Color.gray);
    }

    public void ResetActiveTokenPiece()
    {
        if (activeTokenPiece == null)
            return;

        ResetTokenImage(activeTokenPiece);
    }

    public void ResetTokenImage(TokenPiece toChange)
    {
        toChange.SetGraphicID("");
        toChange.RebuildToken();
    }

    public void GiveIDToActiveToken(string givenID)
    {
        activeTokenPiece.SetGraphicID(givenID);
        activeTokenPiece.RebuildToken();
    }

    private void RebuildTokenMainPanel()
    {
        if (activeTokenPiece == null)
            return;

        if(activeTokenPiece.tokenMessage != null)
            tokenPanelInputText.SetTextWithoutNotify(activeTokenPiece.tokenMessage);
        else
            tokenPanelInputText.SetTextWithoutNotify("");
    }

    public void CloseTokenMainPanel()
    {
        showingTokenMainPanel = false;
        tokenPieceMainPanel.gameObject.SetActive(false);
    }

    public void OnChosenTokenTextChange()
    {
        if (activeTokenPiece == null)
            return;

        activeTokenPiece.SetTokenText(tokenPanelInputText.text);
    }

    public List<UnitPiece> GetPieceList()
    {
        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();

        return castedPieces;
    }

    public UnitPiece GetActivePiece()
    {
        if (activePieceType == 0)
            return activeCharacterPiece;
        else if (activePieceType == 1)
            return activeFoePiece;
        else
            return activeTokenPiece;
    }

    public void GiveLoadedPieces(PieceFile loadedData)
    {
        CleanPieces();

        for(int i = 0; i < loadedData.pieces.Length; i++)
        {
            EsperCharacter charaBase = UnitManager._instance.GetCharacter(loadedData.pieces[i].pieceID);
            
            PieceFile.SaveMapPiece smp = loadedData.pieces[i];
            if(charaBase != null)
            {
                //it's character
                EsperCharacter load = charaBase.MakeCopy();

                
                load.SetFreshFlag(false); //may cause problems
                load.GiveCurrentHP(smp.pieceCurrentHP);
                load.GiveAddedHP(smp.pieceAddedHP);

                load.GiveBlightList(loadedData.PassToBlightList(i));
                load.GiveStatusList(loadedData.PassToStatusList(i));
                load.GiveEffectList(loadedData.PassToEffectList(i));

                Vector3 pos = Vector3.zero;
                if (smp.onMap)
                {
                    pos = new Vector3(smp.posX, 0f, smp.posZ);
                    float posY = MapManager._instance.mapTarget.GetTileHeightAt(pos);
                    pos.y = posY;
                }
                else
                {
                    pos = new Vector3(smp.posX, smp.posY, smp.posZ);
                }

                Transform pieceObj = SpawnCharacterPiece(load, pos, true, smp.onMap);

                Color pieceColor = new Color(smp.colorR, smp.colorG, smp.colorB);

                CharacterPiece castedChara = pieceObj.GetComponent<CharacterPiece>();
                bool allowMiniPanel = (miniPanelShow == 2) || (miniPanelShow == 1 && (activePieceType == 0) && activeCharacterPiece == castedChara);
                castedChara.SetPieceColor(pieceColor);
                castedChara.SetPieceRotation(smp.pieceRotation);
                castedChara.SetPieceFaded(castedChara.characterData.currentHP == 0);
                
                //get card data
                castedChara.SetFateDeck(mainCanvas, smp.handNumbers, smp.handSuits, smp.fateNumbers, smp.fateSuits, smp.discardNumbers, smp.discardSuits, smp.aetherNumbers, smp.aetherSuits);
                castedChara.LoadHistory(smp.actionRollTypes, smp.rollValues);
            }

            EsperFoe foeBase = UnitManager._instance.GetFoe(smp.pieceID);
            if(foeBase != null)
            {
                //it's foe
                EsperFoe load = foeBase.MakeCopy();

                load.SetFreshFlag(false); //may cause problems

                load.GiveCurrentHP(smp.pieceCurrentHP);
                load.GiveAddedHP(smp.pieceAddedHP);

                load.GiveBlightList(loadedData.PassToBlightList(i));
                load.GiveStatusList(loadedData.PassToStatusList(i));
                load.GiveEffectList(loadedData.PassToEffectList(i));

                //marks pending

                Vector3 pos = Vector3.zero;
                if (smp.onMap)
                {
                    pos = new Vector3(smp.posX, 0f, smp.posZ);
                    float posY = MapManager._instance.mapTarget.GetTileHeightAt(pos);
                    pos.y = posY;
                }
                else
                {
                    pos = new Vector3(smp.posX, smp.posY, smp.posZ);
                }

                Transform pieceObj = SpawnFoePiece(load, pos, smp.onMap, true);

                Color pieceColor = new Color(smp.colorR, smp.colorG, smp.colorB);

                FoePiece castedFoe = pieceObj.GetComponent<FoePiece>();
                bool allowMiniPanel = (miniPanelShow == 2) || (miniPanelShow == 1 && (activePieceType == 1) && activeFoePiece == castedFoe);
                castedFoe.SetPieceColor(pieceColor);
                castedFoe.SetPieceRotation(smp.pieceRotation);
                //castedFoe.SetMiniPanelActive(allowMiniPanel);
                castedFoe.SetPieceFaded(castedFoe.foeData.currentHP == 0);
                
                castedFoe.LoadHistory(smp.actionRollTypes, smp.rollValues);
            }

            if(charaBase == null && foeBase == null)
            {
                if (smp.pieceID < 0)
                {
                    Vector3 pos = Vector3.zero;
                    if (smp.onMap)
                    {
                        pos = new Vector3(smp.posX, 0f, smp.posZ);
                        float posY = MapManager._instance.mapTarget.GetTileHeightAt(pos);
                        pos.y = posY;
                    }
                    else
                    {
                        pos = new Vector3(smp.posX, smp.posY, smp.posZ);
                    }

                    Color pieceColor = new Color(smp.colorR, smp.colorG, smp.colorB);

                    //token piece
                    Transform pieceObj = SpawnTokenPiece(pos, pieceColor, smp.tokenMessage, smp.pieceGraphicID);
                }
                else
                {
                    //throw sign that some pieces ID were not found, therefore not loaded
                    unloadedPiecesWarning.SetActive(true);
                }
            }
        }

        AdaptToTileHeight();
        GameModeManager._instance.SetTileNumerationUpdateFlag();
        ColorManager._instance.ForceUpdateColorMarkers(false);

        if (OnPieceAdded != null)
            OnPieceAdded.Invoke();

        GameModeManager._instance.OnPiecesLoadedCall();
    }

    public bool IsPieceCollidingWithPiece(Vector3 position, int unitID)
    {
        for(int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i].transform.GetInstanceID() == unitID)
                continue;

            //token pieces can be ignored
            if (castedPieces[i] is TokenPiece)
                continue;

            //ignore pieces out of map
            if (!castedPieces[i].onMap)
                continue;

            float minBoundx = castedPieces[i].mapPosition.x;
            float maxBoundx = castedPieces[i].mapPosition.x + (0.85f * castedPieces[i].pieceSize);
            float minBoundz = castedPieces[i].mapPosition.y;
            float maxBoundz = castedPieces[i].mapPosition.y + (0.85f * castedPieces[i].pieceSize);

            if ((position.x >= minBoundx && position.x <= maxBoundx) &&
                (position.z >= minBoundz && position.z <= maxBoundz))
                return true;
        }

        return false;
    }

    public float CheckPieceShapeCollision(Vector3 position, float targetHeight, int size)
    {
        Vector3 tentPos = position;
        tentPos.y = targetHeight;

        return ShapesManager._instance.CheckWithCollision(tentPos, size);
    }

    public void CleanPieces()
    {
        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();

        SetPieceButtonOptions(false);

        activeCharacterPiece = null;
        activeFoePiece = null;
        activeTokenPiece = null;
        activePieceType = -1;

        for(int i = miniPanelContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(miniPanelContainer.GetChild(i).gameObject);
        }

        for(int i = castedPieces.Count - 1; i >= 0; i--)
        {
            Destroy(castedPieces[i].gameObject);
        }

        castedPieces = new List<UnitPiece>();

        /*
        //clean token panels
        for(int i = tokenPieceIndividualPanelParent.childCount - 1; i >= 1; i--)
        {
            Destroy(tokenPieceIndividualPanelParent.GetChild(i).gameObject);
        }
        */

        GameModeManager._instance.OnPiecesLoadedCall();
    }

    public void ErasePiecesWithID(int pieceID)
    {
        if (castedPieces == null)
            castedPieces = new List<UnitPiece>();

        SetPieceButtonOptions(false);

        activeCharacterPiece = null;
        activeFoePiece = null;
        activeTokenPiece = null;
        activePieceType = -1;

        for(int i = castedPieces.Count - 1; i >= 0; i--)
        {
            if(castedPieces[i] is CharacterPiece)
            {
                CharacterPiece cp = (CharacterPiece)castedPieces[i];
                if(cp.characterData.unitID == pieceID)
                {
                    //cp.DeleteMiniPanel();
                    castedPieces.RemoveAt(i);
                    Destroy(cp.gameObject);
                }
            }
            else if(castedPieces[i] is FoePiece)
            {
                FoePiece fp = (FoePiece)castedPieces[i];
                if(fp.foeData.unitID == pieceID)
                {
                    //fp.DeleteMiniPanel();
                    castedPieces.RemoveAt(i);
                    Destroy(fp.gameObject);
                }
            }
        }
    }

    public void MoveCall()
    {
        Debug.Log("Move");
    }

    public void InfoCall(bool editMode)
    {
        /*
        if (!pieceOptionButtonActive)
            return;
        */

        pieceBeingEdited = editMode;

        Vector3 lookPos = Vector3.zero;

        UnitManager._instance.generalInputs.SetActive(false);
        UnitManager._instance.optionButton.SetActive(false);

        if (activePieceType == 0)
        {
            if (!activeCharacterPiece)
                return;

            characterInfoPanel.gameObject.SetActive(true);
            foeInfoPanel.gameObject.SetActive(false);

            CharacterPiece castPiece = activeCharacterPiece;
            characterInfoPanel.GiveCharacterSource(castPiece.characterData, castPiece, editMode);

            SetDisplayPanelActive(false);
            
            SetPieceButtonOptions(false);

            lookPos = activeCharacterPiece.transform.position + (2f * MapManager._instance.activeCamera.transform.right);

            if (!castPiece.pieceIsGraphic)
            {
                PieceCamera._instance.SetSamplerMeepleConfig(castPiece.headPartId, castPiece.bodyPartId, castPiece.weaponLPartId, castPiece.weaponRPartId);
            }
            else
            {
                Transform graphicPiece = castPiece.GetGraphicModel();
                if (graphicPiece != null)
                    PieceCamera._instance.SetSamplerGraphicConfig(graphicPiece);
            }
        }
        else if(activePieceType == 1)
        {
            if (!activeFoePiece)
                return;

            foeInfoPanel.gameObject.SetActive(true);
            characterInfoPanel.gameObject.SetActive(false);

            FoePiece castPiece = activeFoePiece;
            foeInfoPanel.GiveFoeSource(castPiece.foeData, castPiece, editMode);

            SetDisplayPanelActive(false);

            SetPieceButtonOptions(false);

            lookPos = activeFoePiece.transform.position + (2f * MapManager._instance.activeCamera.transform.right);

            if (!castPiece.pieceIsGraphic)
            {
                PieceCamera._instance.SetSamplerMeepleConfig(castPiece.headPartId, castPiece.bodyPartId, castPiece.weaponLPartId, castPiece.weaponRPartId);
            }
            else
            {
                Transform graphicPiece = castPiece.GetGraphicModel();
                if (graphicPiece != null)
                    PieceCamera._instance.SetSamplerGraphicConfig(graphicPiece);
            }
        }

        UnitManager._instance.SetUnitMenu(false);
        infoPanelBackdrop.gameObject.SetActive(true);
        grabbingActive = false;
        MapManager._instance.EnableControls(false);
        
        //MapManager._instance.activeCamera.AutoNavigateTo(lookPos);
    }

    public void DeleteCall()
    {
        /*
        if (!pieceOptionButtonActive)
            return;
        */

        if (activeCharacterPiece == null && activeFoePiece == null && activeTokenPiece == null)
            return;

        if (activePieceType == 0)
        {
            DespawnCharacterPiece(activeCharacterPiece);
        }
        else if(activePieceType == 1)
        {
            DespawnFoePiece(activeFoePiece);
        }
        else if(activePieceType == 2)
        {
            DespawnTokenPiece(activeTokenPiece);
        }

        SetPieceButtonOptions(false);

        SetDisplayPanelActive(false);

        return;
    }

    public void CallDeleteWarning()
    {
        if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode)
        {
            unitModeRoot.SetActive(true);

            rightMenuOpen = false;
            UnitManager._instance.listPanel.ShowPanel(false);
            if (activePieceType == 0)
                UnitManager._instance.listPanel.OnEntryClick -= CharaRightMenuClick;
        }

        pieceDeleteWarning.SetActive(true);
        modeButton.SetActive(false);
        optionsButton.SetActive(false);
    }

    public void DeleteWarningClose()
    {
        //clean remaining called panels
        if (MapManager._instance.toolMode == MapManager.ToolMode.GameMode)
            unitModeRoot.SetActive(false);
    }

    public void SizeChangeCall(int value)
    {
        value = Mathf.Clamp(value, 1, pieceMaxSize);

        if (activePieceType == 0 && activeCharacterPiece != null)
            activeCharacterPiece.ChangeSize(value);
        else if (activePieceType == 1 && activeFoePiece != null)
            activeFoePiece.ChangeSize(value);
        else if (activePieceType == 2 && activeTokenPiece != null)
            activeTokenPiece.ChangeSize(value);
    }

    public void AdaptToTileHeight()
    {
        if (castedPieces == null)
            return;

        for(int i = 0; i < castedPieces.Count; i++)
        {
            castedPieces[i].UpdateHeightInPos();
        }
    }

    public void CloseInfoPanel()
    {
        if (activePieceType == 0)
        {
            characterInfoPanel.gameObject.SetActive(false);
            activeCharacterPiece.GiveData(characterInfoPanel.GetCharacterData());

            int[] partIDs = PieceCamera._instance.GetCurrentSamplePartIDs();
            activeCharacterPiece.UpdatePieceModel(partIDs[0], partIDs[1], partIDs[2], partIDs[3]);

            //activeCharacterPiece.SetMiniPanelActive(true);

            //SetPieceButtonOptions(true);
        }
        else if(activePieceType == 1)
        {
            foeInfoPanel.gameObject.SetActive(false);
            activeFoePiece.GiveData(foeInfoPanel.GetFoeData());

            activeFoePiece.foeData.GiveCurrentHP(foeInfoPanel.GetFoeData().currentHP);

            int[] partIDs = PieceCamera._instance.GetCurrentSamplePartIDs();
            activeFoePiece.UpdatePieceModel(partIDs[0], partIDs[1], partIDs[2], partIDs[3]);
            //activeFoePiece.SetMiniPanelActive(true);
        }

        SetDisplayPanelActive(true);
        infoPanelBackdrop.gameObject.SetActive(false);

        pieceBeingEdited = false;

        grabbingActive = true;
        MapManager._instance.activeCamera.CancelAutoNav();
        UnitManager._instance.generalInputs.SetActive(true);
        UnitManager._instance.optionButton.SetActive(true);
        MapManager._instance.EnableControls(true);
    }

    public void SetPieceButtonOptions(bool active)
    {
        if (active)
            pieceRotationButtons.GetComponent<Animator>().SetTrigger("Show");
        else if (!pieceRotationButtons.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("PieceButtons_HideIdle"))
            pieceRotationButtons.GetComponent<Animator>().SetTrigger("Hide");

        pieceOptionButtonActive = active;

        /*
        if (active)
        {
            pieceRotationButtons.GetChild(0).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker);
            pieceRotationButtons.GetChild(1).gameObject.SetActive(MapManager._instance.toolMode == MapManager.ToolMode.GameMode);
        }
        */
    }

    public void SetPieceAutoSize(bool value)
    {
        pieceAutoSize = value;
    }

    public void RotateActivePiece(bool right)
    {
        if(activePieceType == 0 && activeCharacterPiece != null)
        {
            activeCharacterPiece.RotatePiece((right ? -1f : 1f) * pieceRotationSpeed * Time.deltaTime);
        }
        else if (activePieceType == 1 && activeFoePiece != null)
        {
            activeFoePiece.RotatePiece((right ? -1f : 1f) * pieceRotationSpeed * Time.deltaTime);
        }
    }

    public bool IsPieceInCoordinates(Vector2 coordinates, bool considerSize = false)
    {
        for(int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i].onMap) 
            {
                if (!considerSize && castedPieces[i].mapCoordinates == coordinates)
                    return true;
                else if ((castedPieces[i].mapCoordinates.x) <= coordinates.x &&
                    (castedPieces[i].mapCoordinates.x + (castedPieces[i].pieceSize - 1)) >= coordinates.x &&
                    (castedPieces[i].mapCoordinates.y) <= coordinates.y &&
                    (castedPieces[i].mapCoordinates.y + (castedPieces[i].pieceSize - 1)) >= coordinates.y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsEnemyPieceInCoordinates(Vector2 coordinates, bool checkFoes, bool considerSize = false)
    {
        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i].onMap)
            {
                if (!considerSize && castedPieces[i].mapCoordinates == coordinates)
                {
                    if (checkFoes && castedPieces[i] is FoePiece)
                        return true;
                    else if (!checkFoes && castedPieces[i] is CharacterPiece)
                        return true;
                }
                else if ((castedPieces[i].mapCoordinates.x) <= coordinates.x &&
                    (castedPieces[i].mapCoordinates.x + (castedPieces[i].pieceSize - 1)) >= coordinates.x &&
                    (castedPieces[i].mapCoordinates.y) <= coordinates.y &&
                    (castedPieces[i].mapCoordinates.y + (castedPieces[i].pieceSize - 1)) >= coordinates.y)
                {
                    if (checkFoes && castedPieces[i] is FoePiece)
                        return true;
                    else if (!checkFoes && castedPieces[i] is CharacterPiece)
                        return true;
                }
            }
        }

        return false;
    }

    public UnitPiece GetPieceInCoordinates(Vector2 coordinates, bool considerSize = false)
    {
        for (int i = 0; i < castedPieces.Count; i++)
        {
            if (castedPieces[i].onMap)
            {
                if (!considerSize && castedPieces[i].mapCoordinates == coordinates)
                    return castedPieces[i];
                else if ((castedPieces[i].mapCoordinates.x) <= coordinates.x &&
                    (castedPieces[i].mapCoordinates.x + (castedPieces[i].pieceSize - 1)) >= coordinates.x &&
                    (castedPieces[i].mapCoordinates.y) <= coordinates.y &&
                    (castedPieces[i].mapCoordinates.y + (castedPieces[i].pieceSize - 1)) >= coordinates.y)
                {
                    return castedPieces[i];
                }
            } 
        }

        return null;
    }

    public UnitPiece GetPieceClosestTo(Vector3 position)
    {
        if (castedPieces == null)
            return null;

        UnitPiece bestMatch = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < castedPieces.Count; i++)
        {
            float dist = Vector3.Distance(position, castedPieces[i].transform.position);
            if(dist < bestDistance)
            {
                bestMatch = castedPieces[i];
                bestDistance = dist;
            }
        }

        return bestMatch;
    }

    public void SetDisplayPanelActive(bool active)
    {
        if (castedPieces == null)
            return;
        
        if (active && (!activeCharacterPiece) && (!activeFoePiece))
            return;
        
        pieceDisplayPanel.gameObject.SetActive(active);

        if (active)
        {
            if (activeCharacterPiece != null)
            {
                pieceDisplayPanel.DisplayCharacterPiece(activeCharacterPiece);
            }
            else if (activeFoePiece != null)
                pieceDisplayPanel.DisplayFoePiece(activeFoePiece);

            int panelMode = 0;
            if (MapManager._instance.toolMode == MapManager.ToolMode.UnitMaker)
                panelMode = 2;
            else
            {
                if (activeFoePiece)
                    panelMode = 1;
            }

            pieceDisplayPanel.SetPanelToolsMode(panelMode);
            
            gridOptionsPanel.SetActive(false);
        }
        else
        {
            pieceDisplayPanel.CloseDisplayPanel();
            
            gridOptionsPanel.SetActive(true);
        }
        /*
        int pieceID = -1;
        if (activeCharacterPiece != null)
            pieceID = activeCharacterPiece.transform.GetInstanceID();
        else if(activeFoePiece != null)
            pieceID = activeFoePiece.transform.GetInstanceID();
        
        for (int i = 0; i < castedPieces.Count; i++)
        {
            if(miniPanelShow == 0)
            {
                castedPieces[i].SetMiniPanelActive(false);
                continue;
            }
            else if (miniPanelShow == 1 && castedPieces[i].transform.GetInstanceID() != pieceID)
            {
                castedPieces[i].SetMiniPanelActive(false);
                continue;
            }

            castedPieces[i].SetMiniPanelActive(active);
            castedPieces[i].RelocateMiniPanel();
        }
        */

        panelShowState = active;
    }

    /*
    public void CloseAllMiniPanels()
    {
        if (castedPieces == null)
            return;

        for (int i = 0; i < castedPieces.Count; i++)
        {
            castedPieces[i].SetMiniPanelActive(false);
        }
    }
    */

    public void SetPanelDisplayMode(int mode)
    {
        miniPanelShow = mode;

        allActiveToggle.SetIsOnWithoutNotify(mode == 2);
        onlyActiveToggle.SetIsOnWithoutNotify(mode == 1);
        noneActiveToggle.SetIsOnWithoutNotify(mode == 0);

        SetDisplayPanelActive(panelShowState);

        MapManager._instance.optionsManager.SaveMiniPanelSetting(mode);
    }

    public void ModifyHP(int value)
    {
        /*
        if (activeCharacterPiece != null)
            activeCharacterPiece.ModifyHealth(value);
        else if (activeFoePiece != null)
            activeFoePiece.ModifyHealth(value);
        */

        miniPanelHolding = false;
        currentHoldCooldown = 0;
    }

    public void ModifyHPHold(int value)
    {
        if (currentHoldCooldown > 0)
            return;

        if (!miniPanelHolding)
        {
            currentHoldCooldown = miniPanelHoldInitialCooldown;
            miniPanelHolding = true;
        }
        else
            currentHoldCooldown = miniPanelHoldConstantCooldown;

        if (activeCharacterPiece != null)
            activeCharacterPiece.ModifyHealth(value);
        else if (activeFoePiece != null)
            activeFoePiece.ModifyHealth(value);
    }

    public void ModifyVigor(int value)
    {
        /*
        if (activeCharacterPiece != null)
            activeCharacterPiece.ModifyVigor(value);
        else if (activeFoePiece != null)
            activeFoePiece.ModifyVigor(value);
        */

        miniPanelHolding = false;
        currentHoldCooldown = 0;
    }

    public void ModifyVigorHold(int value)
    {
        if (currentHoldCooldown > 0)
            return;

        if (!miniPanelHolding)
        {
            currentHoldCooldown = miniPanelHoldInitialCooldown;
            miniPanelHolding = true;
        }
        else
            currentHoldCooldown = miniPanelHoldConstantCooldown;

        if (activeCharacterPiece != null)
            activeCharacterPiece.ModifyVigor(value);
        else if (activeFoePiece != null)
            activeFoePiece.ModifyVigor(value);
    }

    // right click menu ----------------

    private void UnitRightClickMenu()
    {
        UnitManager._instance.listPanel.screenProportionSize = UnitManager._instance.bondClassJobPanelProportions;
        UnitManager._instance.listPanel.listColor = 0.9f * Color.black;

        Vector3 listOrigin = Input.mousePosition;
        List<string> menuList = new List<string>();

        menuList.Add("Show Details");
        menuList.Add("Roll Accuracy Against Piece");
        menuList.Add("Edit Piece");
        menuList.Add("Delete Piece");

        UnitManager._instance.listPanel.ShowPanel(listOrigin, menuList, false);
        UnitManager._instance.listPanel.OnEntryClick += CharaRightMenuClick;

        listRT = UnitManager._instance.listPanel.GetComponent<RectTransform>();
        rightMenuOpen = true;
    }

    private void CharaRightMenuClick(int index)
    {
        if(index == 0)
        {
            //details call
            InfoCall(false);
        }
        else if (index == 1)
        {
            Debug.Log("TO_DO: Implement accuracy checks by piece");
        }
        else if(index == 2)
        {
            //edit call
            InfoCall(true);
        }
        else
        {
            //delete call
            CallDeleteWarning();
        }
    }
}
