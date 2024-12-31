using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PieceLanding : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform landingPage;
    //[SerializeField] private RectTransform piecePage;

    [Space(5f)]
    [SerializeField] private Color panelColor;
    [SerializeField] private bool isCharacterPanel = false;

    public void OpenGraphicPiecePanel()
    {
        if (isCharacterPanel && !UnitManager._instance.CheckIfCharacterNamed())
            return;
        else if (!isCharacterPanel && !UnitManager._instance.CheckIfFoeNamed())
            return;

        GraphicPieceEditor.Instance.OpenPieceProcess(this, panelColor, isCharacterPanel, this);
        //landingPage.gameObject.SetActive(false);
    }

    public void OpenMeeplePiecePanel()
    {
        if (isCharacterPanel && !UnitManager._instance.CheckIfCharacterNamed())
            return;
        else if (!isCharacterPanel && !UnitManager._instance.CheckIfFoeNamed())
            return;

        //piecePage.gameObject.SetActive(true);
        //landingPage.gameObject.SetActive(false);
    }

    public void ReturnToLanding()
    {
        landingPage.gameObject.SetActive(true);
        //piecePage.gameObject.SetActive(false);

        if (isCharacterPanel)
        {
            transform.parent.GetComponent<CharacterMakerPanel>().UpdatePiecePage();
        }
        else
        {
            transform.parent.GetComponent<FoeMakerPanel>().UpdatePiecePage();
        }
    }

    public void ConfirmPieceID(string graphicID)
    {
        if (isCharacterPanel)
        {
            transform.parent.GetComponent<CharacterMakerPanel>().GiveGraphicIDToPiece(graphicID);
            transform.parent.GetComponent<CharacterMakerPanel>().UpdatePiecePage();
        }
        else
        {
            transform.parent.GetComponent<FoeMakerPanel>().GiveGraphicIDToPiece(graphicID);
            transform.parent.GetComponent<FoeMakerPanel>().UpdatePiecePage();
        }
    }
}
