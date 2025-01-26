using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//manages the piece camera and the piece editing system
public class PieceCamera : MonoBehaviour
{
    public static PieceCamera _instance;

    public Transform renderCamera;

    [SerializeField] private GameObject meepleObj;
    [SerializeField] private GameObject graphicObj;

    [Space(5f)]
    [SerializeField] private int pieceSamplerLayer = 8;
    [SerializeField] private Vector3 graphicPosUp;

    [Space(10f)]
    public List<PiecePart> bodyParts;
    public List<PiecePart> headParts;
    public List<PiecePart> weaponParts; //remember that in the model, both childcount and indexes must match between both hands

    private int currentHeadId = 0;
    private int currentBodyId = 0;
    private int currentHandLId = 0;
    private int currentHandRId = 0;

    private int scrollIndexHead = 0;
    private int scrollIndexBody = 0;
    private int scrollIndexLWeapon = 0;
    private int scrollIndexRWeapon = 0;

    [Space(10f)]
    public Transform pieceSampler;
    public float pieceStartAngle = 70f;
    public float samplerRotationSpeed = 80f;

    private bool partListOpen = false;
    private int partListIndex = 0;
    private RectTransform listRT;

    [System.Serializable]
    public struct PiecePart
    {
        public string partName;
        public int pieceID; //for saving purposes
        public Transform partObj;
    }

    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        SetSamplerAtStartRotation();

        SetSamplerMeepleConfig(0, 0, 0, 0);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Q))
            RotateSampler(false);
        else if (Input.GetKey(KeyCode.E))
            RotateSampler(true);

        if (partListOpen && Input.GetMouseButtonDown(0))
        {
            if (!TooltipManager.CheckMouseInArea(listRT))
            {
                partListOpen = false;
                UnitManager._instance.listPanel.ShowPanel(false);
                UnitManager._instance.listPanel.OnEntryClick -= PiecePartListClick;
            }
        }
    }

    public void SetSamplerAtStartRotation()
    {
        pieceSampler.rotation = Quaternion.Euler(0f, pieceStartAngle, 0f);
    }

    public void SetSamplerMeepleConfig(int headIndex, int bodyIndex, int leftHandIndex, int rightHandIndex)
    {
        meepleObj.SetActive(true);
        graphicObj.SetActive(false);

        GetHeadPart(currentHeadId).partObj.gameObject.SetActive(false);
        GetBodyPart(currentBodyId).partObj.gameObject.SetActive(false);
        Transform rWeapon = GetWeaponPart(currentHandRId).partObj;
        if(rWeapon != null)
        {
            rWeapon.gameObject.SetActive(false);
        }
        Transform lWeapon = GetWeaponPart(currentHandLId).partObj;
        if(lWeapon != null)
        {
            pieceSampler.GetChild(0).GetChild(3).GetChild(lWeapon.GetSiblingIndex()).gameObject.SetActive(false);
        }

        currentHeadId = headIndex;
        currentBodyId = bodyIndex;
        currentHandLId = leftHandIndex;
        currentHandRId = rightHandIndex;
        
        GetHeadPart(currentHeadId).partObj.gameObject.SetActive(true);
        GetBodyPart(currentBodyId).partObj.gameObject.SetActive(true);
        rWeapon = GetWeaponPart(currentHandRId).partObj;
        if (rWeapon != null)
        {
            rWeapon.gameObject.SetActive(true);
        }
        lWeapon = GetWeaponPart(currentHandLId).partObj;
        if (lWeapon != null)
        {
            pieceSampler.GetChild(0).GetChild(3).GetChild(lWeapon.GetSiblingIndex()).gameObject.SetActive(true);
        }

        SetScrollingIndexes();
    }

    public void SetSamplerGraphicConfig(Transform graphicSource, bool useNewInstance = true)
    {
        meepleObj.SetActive(false);
        graphicObj.SetActive(true);

        for(int i = graphicObj.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(graphicObj.transform.GetChild(i).gameObject);
        }

        if (useNewInstance)
        {
            GameObject graphicSample = Instantiate<GameObject>(graphicSource.gameObject, graphicObj.transform);
            graphicSample.transform.localPosition = graphicPosUp;
            pieceSampler.localRotation = Quaternion.identity;
            graphicSample.transform.localRotation = Quaternion.identity;

            graphicSample.layer = pieceSamplerLayer;
            for(int i = 0; i < graphicSample.transform.childCount; i++)
            {
                graphicSample.transform.GetChild(i).gameObject.layer = pieceSamplerLayer;
            }
        }
        else
        {
            graphicSource.SetParent(graphicObj.transform);
            graphicSource.transform.localPosition = graphicPosUp;
            pieceSampler.localRotation = Quaternion.identity;
            graphicSource.transform.localRotation = Quaternion.identity;

            graphicSource.gameObject.layer = pieceSamplerLayer;
            for (int i = 0; i < graphicSource.transform.childCount; i++)
            {
                graphicSource.transform.GetChild(i).gameObject.layer = pieceSamplerLayer;
            }
        }
    }

    public void SetSamplerConfig(EsperUnit unit, bool forceMeeple = false)
    {
        if (string.IsNullOrEmpty(unit.graphicImageID))
            Debug.Log("No graphic selected for this unit");
        else
        {
            GameObject samplePiece = GraphicPieceEditor.Instance.LoadPieceWithID(unit.graphicImageID, true);
            SetSamplerGraphicConfig(samplePiece.transform, false);
        }
    }

    public void ApplyFromPieceToSample(UnitPiece piece)
    {
        if(!piece.pieceIsGraphic)
            SetSamplerMeepleConfig(piece.headPartId, piece.bodyPartId, piece.weaponLPartId, piece.weaponRPartId);
        else
        {
            Transform pieceGraphic = piece.GetGraphicModel();
            if (pieceGraphic == null)
                return;

            SetSamplerGraphicConfig(pieceGraphic);
        }
    }

    public void ApplyCurrentSampleToPiece(UnitPiece piece)
    {
        piece.UpdatePieceModel(currentHeadId, currentBodyId, currentHandLId, currentHandRId);
    }

    public int[] GetCurrentSamplePartIDs()
    {
        return new int[4] { currentHeadId, currentBodyId, currentHandLId, currentHandRId };
    }

    public PiecePart GetHeadPart(int partID)
    {
        return headParts.Find(x => x.pieceID == partID);
    }

    public PiecePart GetBodyPart(int partID)
    {
        return bodyParts.Find(x => x.pieceID == partID);
    }

    public PiecePart GetWeaponPart(int partID)
    {
        return weaponParts.Find(x => x.pieceID == partID);
    }

    public void CallPiecePartList(int partID)
    {
        UnitManager._instance.listPanel.screenProportionSize = UnitManager._instance.bondClassJobPanelProportions;
        UnitManager._instance.listPanel.listColor = 0.9f * Color.black;

        Vector3 listOrigin = Input.mousePosition;
        List<string> partTypes = new List<string>();

        List<PiecePart> partList = partID < 1 ? headParts : weaponParts;

        for (int i = 0; i < partList.Count; i++)
        {
            partTypes.Add(partList[i].partName);
        }

        UnitManager._instance.listPanel.ShowPanel(listOrigin, partTypes, true);
        partListOpen = true;
        UnitManager._instance.listPanel.OnEntryClick += PiecePartListClick;

        listRT = UnitManager._instance.listPanel.GetComponent<RectTransform>();

        partListIndex = partID;
    }

    public void PiecePartListClick(int index)
    {
        if (partListOpen)
        {
            if (partListIndex == 0)
            {
                int chosenPartId = headParts[index].pieceID;
                SetSamplerMeepleConfig(chosenPartId, currentBodyId, currentHandLId, currentHandRId);
            }
            else if (partListIndex == 1)
            {
                int chosenPartId = weaponParts[index].pieceID;
                SetSamplerMeepleConfig(currentHeadId, currentBodyId, chosenPartId, currentHandRId);
            }
            else if (partListIndex == 2)
            {
                int chosenPartId = weaponParts[index].pieceID;
                SetSamplerMeepleConfig(currentHeadId, currentBodyId, currentHandLId, chosenPartId);
            }

            UnitManager._instance.listPanel.ShowPanel(false);
            UnitManager._instance.listPanel.OnEntryClick -= PiecePartListClick;
            partListOpen = false;
        }
    }

    private void SetScrollingIndexes()
    {
        for(int i = 0; i < headParts.Count; i++)
        {
            if (headParts[i].pieceID == currentHeadId)
            {
                scrollIndexHead = i;
                break;
            }
        }

        for(int i = 0; i < bodyParts.Count; i++)
        {
            if(bodyParts[i].pieceID == currentBodyId)
            {
                scrollIndexBody = i;
                break;
            }
        }

        scrollIndexLWeapon = -1;
        scrollIndexRWeapon = -1;

        for(int i = 0; i < weaponParts.Count; i++)
        {
            if (weaponParts[i].pieceID == currentHandLId)
                scrollIndexLWeapon = i;

            if (weaponParts[i].pieceID == currentHandRId)
                scrollIndexRWeapon = i;

            if (scrollIndexLWeapon >= 0 && scrollIndexRWeapon >= 0)
                break;
        }
    }

    public void RotateSampler(bool right)
    {
        pieceSampler.Rotate(0f, (right ? -1f : 1f) * samplerRotationSpeed * Time.deltaTime, 0f);
    }
}
