using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

//class to control the movement, placing and modifying of unit pieces on a map
public class UnitPiece : MonoBehaviour
{
    public Vector2 mapPosition { get; private set; }
    public Vector2 mapCoordinates { get; private set; }

    private Vector3 modelPositionDelta;

    /*
    public RectTransform miniPanel { get; private set; }
    private Vector3 piecePanelDeltaPos;
    private bool piecePanelDeltaPosSet = false;
    */

    public int pieceSize { get; protected set; }

    protected Transform modelObj;
    protected Transform baseObj;
    protected Transform selectorObj;

    public string unitName { get; protected set; }

    public bool pieceIsGraphic { get; protected set; }
    public string pieceImageID { get; protected set; }

    public int headPartId { get; private set; }
    public int bodyPartId { get; private set; }
    public int weaponLPartId { get; private set; }
    public int weaponRPartId { get; private set; }

    public float modelRotation { get; protected set; }

    public Vector3 lastAcceptedPosition { get; private set; }
    public bool onMap { get; private set; }
    public bool triggerColliding { get; private set; }
    private Transform collidingWith;
    private float collisionDistance;

    public bool pieceIsFaded { get; private set; }
    private float pieceFadeValue = 0.2f;

    protected MeshRenderer[] frameRends;

    public virtual void BuildPiece(IconUnit source)
    {
        modelObj = transform.GetChild(0);
        baseObj = transform.GetChild(1);
        selectorObj = transform.GetChild(2);

        BuildFrames();

        pieceSize = source.size;

        if (source.graphicImageID != null)
        {
            pieceIsGraphic = source.graphicImageID.Length > 0;
            pieceImageID = source.graphicImageID;
        }
        else
        {
            pieceIsGraphic = false;
            pieceImageID = "";
        }

        if (!pieceIsGraphic)
            UpdatePieceModel(source.headPartID, source.bodyPartID, source.lWeaponPartID, source.rWeaponPartID);
        else
        {
            bool graphicFound = RequestPieceGraphicModel(source.graphicImageID);
            if (!graphicFound)
            {
                NotificationSystem.Instance.PushNotification("Graphic for this piece was not found. Replacing with 3D Piece.");
                UpdatePieceModel(source.headPartID, source.bodyPartID, source.lWeaponPartID, source.rWeaponPartID);
            }
        }

        SetPieceColor(source.colorChoice);

        modelRotation = 0f;
        modelObj.rotation = Quaternion.Euler(0f, modelRotation, 0f);
    }

    public void SetMapPosition(Vector3 pos)
    {
        mapPosition = new Vector2(pos.x, pos.z);
        mapCoordinates = MapManager._instance.mapTarget.TranslateToGridCoordinates(pos);
        triggerColliding = false;
        onMap = true;
        lastAcceptedPosition = transform.position;
    }

    public void UpdateHeightInPos()
    {
        if (!onMap)
            return;

        Vector3 auxPos = new Vector3(mapPosition.x, 0f, mapPosition.y);
        float h = MapManager._instance.mapTarget.GetTileHeightAt(auxPos);
        auxPos.y = h;
        transform.position = auxPos;

        lastAcceptedPosition = transform.position;
    }

    public void SetOuterMapPosition(Vector3 pos)
    {
        transform.position = pos;
        if (triggerColliding)
        {
            float dist = Vector3.Distance(transform.position, collidingWith.position);
            if (dist <= collisionDistance)
            {
                transform.position = lastAcceptedPosition;
                return;
            }

        }

        lastAcceptedPosition = transform.position;
        onMap = false;
    }

    /*
    public void GiveMiniPanel(RectTransform miniPanel)
    {
        this.miniPanel = miniPanel;
        
        RelocateMiniPanel();
        UpdateMiniPanel();
    }
    */

    public void ChangeSize(int value)
    {
        if (value < 1)
            value = 1;

        pieceSize = value;

        Vector3 modPos = modelObj.transform.localPosition;
        modPos.y = (0.5f * value) + 0.1f;
        Vector3 modSca = modelObj.transform.localScale;
        modSca = 0.8f * value * Vector3.one;

        modelObj.transform.localPosition = modPos;
        modelObj.transform.localScale = modSca;

        selectorObj.transform.localPosition = modPos;
        selectorObj.transform.localScale = modSca;

        Vector3 framePos = baseObj.GetChild(0).localPosition;
        framePos = new Vector3(0f, 0.05f, -0.5f * value);
        baseObj.GetChild(1).localPosition = framePos;

        framePos = baseObj.GetChild(1).localPosition;
        framePos = new Vector3(-0.5f * value, 0.05f, 0f);
        baseObj.GetChild(2).localPosition = framePos;

        framePos = baseObj.GetChild(2).localPosition;
        framePos = new Vector3(0f, 0.05f, 0.5f * value);
        baseObj.GetChild(3).localPosition = framePos;

        framePos = baseObj.GetChild(3).localPosition;
        framePos = new Vector3(0.5f * value, 0.05f, 0f);
        baseObj.GetChild(4).localPosition = framePos;
        
        Vector3 frameSca = new Vector3(value, 0.15f, 0.1f);
        baseObj.GetChild(1).localScale = frameSca;
        baseObj.GetChild(3).localScale = frameSca;
        frameSca = new Vector3(0.1f, 0.15f, value);
        baseObj.GetChild(2).localScale = frameSca;
        baseObj.GetChild(4).localScale = frameSca;

        Vector3 baseSca = new Vector3(value, value, 1f);
        baseObj.GetChild(0).localScale = baseSca;

        //change position
        modelPositionDelta = new Vector3(0.5f * (value - 1), modelObj.localPosition.y, 0.5f * (value - 1));
        modelObj.localPosition = modelPositionDelta;
        modelPositionDelta.y = 0f;
        baseObj.localPosition = modelPositionDelta;
        selectorObj.transform.localPosition = modelPositionDelta;
    }

    public void SetFocus(bool focused)
    {
        for(int i = 0; i < frameRends.Length; i++)
        {
            frameRends[i].material.color = focused ? Color.red : Color.yellow;
        }
    }

    protected void BuildFrames()
    {
        frameRends = new MeshRenderer[4];
        frameRends[0] = baseObj.GetChild(1).GetComponent<MeshRenderer>();
        frameRends[1] = baseObj.GetChild(2).GetComponent<MeshRenderer>();
        frameRends[2] = baseObj.GetChild(3).GetComponent<MeshRenderer>();
        frameRends[3] = baseObj.GetChild(4).GetComponent<MeshRenderer>();
    }

    public void SetSelected(bool state)
    {
        selectorObj.gameObject.SetActive(state);
    }

    public void SetMovingFrame(bool moving)
    {
        Debug.Log("set move for " + transform.name + " as " + moving);

        for(int i = 1; i < baseObj.childCount; i++)
        {
            baseObj.GetChild(i).GetChild(0).gameObject.SetActive(moving);
        }
    }

    /*
    public void RelocateMiniPanel()
    {
        return;

        if (miniPanel == null)
            return;

        Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(transform.GetChild(0).position);

        if(!piecePanelDeltaPosSet)
        {
            piecePanelDeltaPos = PieceManager._instance.miniPanelAdjustment;
            piecePanelDeltaPos.x *= Screen.width;
            piecePanelDeltaPos.y *= Screen.height;

            piecePanelDeltaPosSet = true;
        }

        miniPanel.position = screenPos + piecePanelDeltaPos;
    }

    public void DeleteMiniPanel()
    {
        if (miniPanel == null)
            return;

        Destroy(miniPanel.gameObject);
    }

    public virtual void UpdateMiniPanel()
    {

    }
    */

    public virtual void ModifyHealth(int value)
    {

    }

    public virtual void ModifyVigor(int value)
    {

    }

    /*
    public void SetMiniPanelActive(bool active)
    {
        if (miniPanel == null)
            return;

        if (active)
            UpdateMiniPanel();
        else
            ShowPieceTools(false);

        miniPanel.gameObject.SetActive(active);
    }

    public virtual void ShowPieceTools(bool show)
    {

    }
    
    */

    public void SetPieceColor(UnityEngine.Color color)
    {
        if (!pieceIsGraphic)
        {
            //Debug.Log("color: " + color.ToString());

            for (int i = 0; i < modelObj.GetChild(0).childCount; i++)
            {
                MeshRenderer rend = modelObj.GetChild(0).GetChild(i).GetComponent<MeshRenderer>();
                UnityEngine.Color samp = color;
                if (pieceIsFaded)
                    samp.a = pieceFadeValue;
                rend.material.color = samp;
            }
        }
        else
        {
            //change the base
            MeshRenderer rend = baseObj.GetChild(0).GetComponent<MeshRenderer>();
            rend.material.color = color;
        }
    }

    public UnityEngine.Color GetPieceColor()
    {
        if (!pieceIsGraphic)
        {
            MeshRenderer rend = modelObj.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
            return rend.material.color;
        }
        else
        {
            MeshRenderer rend = baseObj.GetChild(0).GetComponent<MeshRenderer>();
            return rend.material.color;
        }
    }

    public void SetPieceFaded(bool value)
    {
        if (value == pieceIsFaded)
            return;

        pieceIsFaded = value;
        SetPieceAlpha(pieceIsFaded ? 0.2f : 1f);
    }

    private void SetPieceAlpha(float value)
    {
        if (!pieceIsGraphic)
        {
            for (int i = 0; i < modelObj.GetChild(0).childCount; i++)
            {
                MeshRenderer rend = modelObj.GetChild(0).GetChild(i).GetComponent<MeshRenderer>();
                Material matInst = new Material(PieceManager._instance.pieceMat);
                if (pieceIsFaded)
                    matInst = new Material(PieceManager._instance.pieceMatFade); // rend.material;

                UnityEngine.Color givenColor = rend.material.color;
                givenColor.a = value;
                matInst.color = givenColor;
                rend.material = matInst;
            }
        }
        else
        {
            for (int i = 0; i < modelObj.GetChild(1).childCount; i++)
            {
                MeshRenderer rend = modelObj.GetChild(1).GetChild(i).GetComponent<MeshRenderer>();
                Material matInst = new Material(PieceManager._instance.graphicPieceMat);
                if (pieceIsFaded)
                    matInst = new Material(PieceManager._instance.graphicPieceMatFade); // rend.material;

                matInst.mainTexture = rend.material.mainTexture;

                UnityEngine.Color givenColor = rend.material.color;
                givenColor.a = value;
                matInst.color = givenColor;
                rend.material = matInst;
            }
        }
    }

    public void UpdatePieceModel(int headIndex, int bodyIndex, int leftHandIndex, int rightHandIndex)
    {
        Mesh meshTarget = PieceCamera._instance.GetHeadPart(headIndex).partObj.GetComponent<MeshFilter>().mesh;

        modelObj.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh = meshTarget;
        headPartId = headIndex;

        meshTarget = PieceCamera._instance.GetBodyPart(bodyIndex).partObj.GetComponent<MeshFilter>().mesh;
        modelObj.GetChild(0).GetChild(1).GetComponent<MeshFilter>().mesh = meshTarget;
        bodyPartId = bodyIndex;

        modelObj.GetChild(0).GetChild(2).gameObject.SetActive(leftHandIndex != 0);
        if (leftHandIndex != 0)
        {
            meshTarget = PieceCamera._instance.GetWeaponPart(leftHandIndex).partObj.GetComponent<MeshFilter>().mesh;
            modelObj.GetChild(0).GetChild(2).GetComponent<MeshFilter>().mesh = meshTarget;
        }
        weaponLPartId = leftHandIndex;

        modelObj.GetChild(0).GetChild(3).gameObject.SetActive(rightHandIndex != 0);
        if (rightHandIndex != 0)
        {
            meshTarget = PieceCamera._instance.GetWeaponPart(rightHandIndex).partObj.GetComponent<MeshFilter>().mesh;
            modelObj.GetChild(0).GetChild(3).GetComponent<MeshFilter>().mesh = meshTarget;
        }
        weaponRPartId = rightHandIndex;
    }

    protected bool RequestPieceGraphicModel(string hexID)
    {
        //modelObj child 1 is the one that should be the graphic piece in question
        GameObject loadedPiece = GraphicPieceEditor.Instance.LoadPieceWithID(hexID);

        if(loadedPiece == null)
        {
            return false;
        }

        if (modelObj.childCount > 1)
        {
            //already has a token image
            Destroy(modelObj.GetChild(1).gameObject);
        }

        modelObj.GetChild(0).gameObject.SetActive(false); //turn off the meeple

        loadedPiece.layer = 0;
        for (int i = 0; i < loadedPiece.transform.childCount; i++)
        {
            loadedPiece.transform.GetChild(i).gameObject.layer = 0;
        }

        loadedPiece.transform.SetParent(modelObj);
        loadedPiece.transform.localPosition = ((0.5f * (PieceManager._instance.graphicPieceScaleUp - 1f)) * Vector3.up);
        loadedPiece.transform.localScale = PieceManager._instance.graphicPieceScaleUp * Vector3.one;

        return true;
    }

    public Transform GetGraphicModel()
    {
        if (!pieceIsGraphic)
            return null;
        else
            return modelObj.GetChild(1);
    }

    public Vector3 GetModelPosition()
    {
        return modelObj.position;
    }

    public void RotatePiece(float rotationDelta)
    {
        modelRotation += rotationDelta;
        modelObj.rotation = Quaternion.Euler(0f, modelRotation, 0f);
    }

    public void SetPieceRotation(float value)
    {
        modelRotation = value;
        modelObj.rotation = Quaternion.Euler(0f, modelRotation, 0f);
    }

    public void SetColliding(bool value, Transform colliding)
    {
        triggerColliding = value;

        if (value)
        {
            collidingWith = colliding;
            collisionDistance = Vector3.Distance(transform.position, collidingWith.position);
        }
        else
            collidingWith = null;
    }
}
