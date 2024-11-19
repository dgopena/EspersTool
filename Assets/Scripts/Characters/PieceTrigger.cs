using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceTrigger : MonoBehaviour
{
    public UnitPiece pieceLogic;

    private void OnTriggerEnter(Collider other)
    {
        pieceLogic.SetColliding(true, other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        pieceLogic.SetColliding(false, other.transform);
    }
}
