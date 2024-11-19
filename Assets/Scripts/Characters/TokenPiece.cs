using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class TokenPiece : UnitPiece
{
    public RectTransform tokenTextPanel { get; private set; }

    public string tokenMessage { get; private set; }

    public void BuildToken(int tokenSize, Color colorChoice, RectTransform textPanelInstance, string graphicID)
    {
        modelObj = transform.GetChild(0);

        if (graphicID == null)
        {
            graphicID = "";
        }

        pieceImageID = graphicID;

        if(graphicID.Length > 0)
        {
            bool graphicFound = RequestPieceGraphicModel(graphicID);
            if (!graphicFound)
            {
                NotificationSystem.Instance.PushNotification("Graphic for this token was not found. Replacing with 3D Piece.");

                modelObj.GetChild(0).gameObject.SetActive(true);
                if (modelObj.childCount > 1)
                    modelObj.GetChild(1).gameObject.SetActive(false);
            }
        }
        else
        {
            modelObj.GetChild(0).gameObject.SetActive(true);
            if(modelObj.childCount > 1)
                modelObj.GetChild(1).gameObject.SetActive(false);
        }

        baseObj = transform.GetChild(1);
        pieceSize = tokenSize;

        SetPieceColor(colorChoice);

        BuildFrames();

        modelRotation = 0f;
        modelObj.rotation = Quaternion.Euler(0f, modelRotation, 0f);

        tokenTextPanel = textPanelInstance;
    }

    public void RebuildToken()
    {
        BuildToken(pieceSize, GetPieceColor(), tokenTextPanel, pieceImageID);
    }

    public void SetGraphicID(string graphicID)
    {
        pieceImageID = graphicID;
    }

    public void SetTokenText(string message)
    {
        tokenMessage = message;

        if (message.Length > 0)
        {
            tokenTextPanel.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
            tokenTextPanel.gameObject.SetActive(true);

            UpdateTextPanelPosition();
        }
        else
            tokenTextPanel.gameObject.SetActive(false);
    }

    public void TryShowMessage()
    {
        SetTokenText(tokenMessage);
    }

    public void UpdateTextPanelPosition()
    {
        Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(transform.position);
        screenPos += (PieceManager._instance.tokenSubPanelDeltaPos.x * Screen.width * Vector3.right) + (PieceManager._instance.tokenSubPanelDeltaPos.y * Screen.height * Vector3.up);

        tokenTextPanel.position = screenPos;
    }
}
