using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMark : MonoBehaviour
{
    public enum MarkType
    {
        PieceMark,
        SpaceMark
    }

    /*
    public enum InBoundType
    {
        InBounds,
        SourceOutOfBound,
        TargetOutOfBound
    }
    */

    public UnitPiece sourcePiece { get; private set; }
    public UnitPiece targetPiece { get; private set; }
    public Vector3 targetSpace { get; private set; }
    public Color baseColor { get; private set; }
    private Gradient gradBase;

    public Vector2 fromCoordinate { get; private set; }
    public Vector2 toCoordinate { get; private set; }
    public Vector3 centralMarkPosition { get; private set; }

    public string markName { get; private set; }

    public MarkType type { get; private set; }

    //public InBoundType boundType { get; private set; }

    public LineRenderer lineRend;
    public Transform targetObj;
    public Transform arrowObj;
    public Transform[] framePointers;
    public TMPro.TextMeshPro textLabel;

    public float curveFactor = 0.25f;
    public float heightFactor = 2f;
    public int heightIndex { get; private set; }
    public int lineSegments = 8;

    public bool markEnabled { get { return lineRend.enabled; } }
    public bool isDimmed { get; private set; }

    private void LateUpdate()
    {
        if(!markEnabled || isDimmed || MarkManager._instance.markEditing)
        {
            textLabel.gameObject.SetActive(false);
            return;
        }

        if(markName != null && markName.Length > 0)
        {
            textLabel.transform.forward = (textLabel.transform.position - MapManager._instance.activeCamera.transform.position);
            textLabel.gameObject.SetActive(true);
        }
        else
            textLabel.gameObject.SetActive(false);
    }

    public void SetMarkEnabled(bool value)
    {
        lineRend.enabled = value;
        targetObj.gameObject.SetActive(value);
    }

    public void MakeMark(UnitPiece src, UnitPiece trg, Color markColor)
    {
        type = MarkType.PieceMark;

        sourcePiece = src;
        targetPiece = trg;
        baseColor = markColor;
    }

    public void MakeMark(UnitPiece src, Vector3 spacePos, Color markColor)
    {
        type = MarkType.SpaceMark;
        sourcePiece = src;
        targetSpace = spacePos;
        toCoordinate = MapManager._instance.mapTarget.TranslateToGridCoordinates(targetSpace);
        baseColor = markColor;
    }

    public void SimpleRedraw(Color newColor)
    {
        Vector3 from = sourcePiece.transform.position;
        Vector3 to = (type == MarkType.PieceMark) ? targetPiece.transform.position : MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(toCoordinate);

        DrawArrow(from, to, newColor, heightIndex);
    }

    public void DimBy(float alpha)
    {
        Color dimmed = baseColor;
        dimmed.a = baseColor.a * alpha;

        Gradient lGrad = lineRend.colorGradient;
        GradientColorKey[] gck = lGrad.colorKeys;
        GradientAlphaKey[] gak = lGrad.alphaKeys;
        for(int i = 0; i < gak.Length; i++)
        {
            GradientAlphaKey igk = gak[i];
            igk.alpha *= alpha;
            gak[i] = igk;
        }

        lGrad.SetKeys(gck, gak);

        lineRend.colorGradient = lGrad;
        arrowObj.GetComponentInChildren<SpriteRenderer>().color = dimmed;

        framePointers[0].gameObject.SetActive(false);
        framePointers[1].gameObject.SetActive(false);
        framePointers[2].gameObject.SetActive(false);
        framePointers[3].gameObject.SetActive(false);

        isDimmed = true;
    }

    public void RevertDim()
    {
        lineRend.colorGradient = gradBase;
        arrowObj.GetComponentInChildren<SpriteRenderer>().color = baseColor;

        framePointers[0].gameObject.SetActive(true);
        framePointers[1].gameObject.SetActive(true);
        framePointers[2].gameObject.SetActive(true);
        framePointers[3].gameObject.SetActive(true);

        isDimmed = false;
    }

    public void SimpleRedraw(int newHeightIndex)
    {
        Vector3 from = sourcePiece.transform.position;
        Vector3 to = (type == MarkType.PieceMark) ? targetPiece.transform.position : MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(toCoordinate);

        DrawArrow(from, to, baseColor, newHeightIndex);
    }

    public void SimpleRedraw()
    {
        Vector3 from = sourcePiece.transform.position;
        Vector3 to = (type == MarkType.PieceMark) ? targetPiece.transform.position : MapManager._instance.mapTarget.TranslatePositionFromGridCoordinates(toCoordinate);

        DrawArrow(from, to, baseColor, heightIndex);
    }

    public void SetMarkName(string markName)
    {
        if (markName == null || markName == "")
            return;

        this.markName = markName;

        if(this.markName != "")
            textLabel.text = markName;
    }

    /*
    public void SetMarkBound(PlayMark.InBoundType boundType)
    {
        this.boundType = boundType;
    }
    */

    public void SetHeightIndex(int hIdx)
    {
        heightIndex = hIdx;
    }

    public void DrawArrow(Vector3 from, Vector3 to, Color arrowColor, int heightIndex = 0)
    {
        if (lineSegments < 3) {
            Debug.Log("not enough segments to make an arrow marker");
            return;
        }

        float cellScale = MapManager._instance.mapTarget.cellScale;

        float startHeight = 0.45f * cellScale;
        float displaceDist = 0.45f * cellScale;
        float heightCap = (cellScale * heightFactor) - (1f - (heightIndex * 0.2f));

        Vector3 startPoint = from + (displaceDist * (to - from).normalized) + (startHeight * Vector3.up);
        Vector3 targetPos = to;
        Vector3 endPoint = to + (displaceDist * (from - to).normalized) + (startHeight * Vector3.up);

        float pointDistance = Vector3.Distance(startPoint, endPoint);

        Vector3 midPoint = (0.5f * pointDistance) * (endPoint - startPoint).normalized;
        midPoint.y = (startHeight + heightCap);

        float startSpeedY = Mathf.Sqrt(2f * 9.8f * heightCap);
        float halfTime = startSpeedY / 9.8f;
        float timeStep = (2f * halfTime) / (float)(lineSegments + 1);
        float segmentDistance = pointDistance / (float)(lineSegments);

        this.heightIndex = heightIndex;

        List<Vector3> linePoints = new List<Vector3>();
        float lastYSpeed = startSpeedY;
        float accHeight = 0f;

        Vector3 directionalNormal = Vector3.Cross(Vector3.up, (endPoint - startPoint)).normalized;

        float lastH = 0f;
        for (int i = 0; i < lineSegments - 1; i++)
        {
            Vector3 nuPoint = startPoint + ((i * segmentDistance) * (endPoint - startPoint).normalized);

            float nuYspeed = (-9.8f * timeStep) + lastYSpeed;
            float nuHeight = (lastYSpeed * timeStep) + (0.5f * -9.8f * (timeStep * timeStep));

            lastYSpeed = nuYspeed;

            accHeight += nuHeight;
            nuPoint.y += accHeight;

            float curveFrac = curveFactor * (accHeight / heightCap) * cellScale;
            nuPoint += curveFrac * directionalNormal;

            if (lastH >= 0f && nuHeight < 0f)
            {
                centralMarkPosition = linePoints[i - 1];

                textLabel.transform.position = centralMarkPosition + (0.3f * Vector3.up);
            }

            lastH = nuHeight;

            linePoints.Add(nuPoint);
        }

        linePoints.Add(endPoint);

        lineRend.positionCount = linePoints.Count;
        lineRend.SetPositions(linePoints.ToArray());

        if (arrowColor.a == 0f)
            arrowColor = Color.white;

        Color startColor = arrowColor;
        startColor.a = 0f;
        lineRend.startColor = startColor;
        lineRend.endColor = arrowColor;
        baseColor = arrowColor;

        if (gradBase == null)
        {
            gradBase = lineRend.colorGradient;
        }

        //locate arrow point
        targetObj.transform.position = (0.15f * cellScale * Vector3.up) + targetPos;
        framePointers[0].position = targetObj.transform.position + (0.45f * MapManager._instance.mapTarget.cellScale * new Vector3(-1f, 0f, -1f));
        framePointers[1].position = targetObj.transform.position + (0.45f * MapManager._instance.mapTarget.cellScale * new Vector3(-1f, 0f, 1f));
        framePointers[2].position = targetObj.transform.position + (0.45f * MapManager._instance.mapTarget.cellScale * new Vector3(1f, 0f, 1f));
        framePointers[3].position = targetObj.transform.position + (0.45f * MapManager._instance.mapTarget.cellScale * new Vector3(1f, 0f, -1f));

        Vector3 lastLineDir = lineRend.GetPosition(lineRend.positionCount - 1) - lineRend.GetPosition(lineRend.positionCount - 2);
        Vector3 preLastLineDir = lineRend.GetPosition(lineRend.positionCount - 2) - lineRend.GetPosition(lineRend.positionCount - 3);

        Vector3 arrowRight = Vector3.Cross(lastLineDir, preLastLineDir);
        Vector3 arrowUp = -lastLineDir;
        Vector3 arrowFwd = Vector3.Cross(arrowUp, arrowRight);

        arrowObj.GetComponentInChildren<SpriteRenderer>().color = arrowColor;
        arrowObj.position = endPoint;
        arrowObj.rotation = Quaternion.LookRotation(arrowFwd, arrowUp);

        fromCoordinate = MapManager._instance.mapTarget.TranslateToGridCoordinates(from);
        toCoordinate = MapManager._instance.mapTarget.TranslateToGridCoordinates(to);
    }
}
