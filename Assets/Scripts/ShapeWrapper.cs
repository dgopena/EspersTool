using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeWrapper : MonoBehaviour
{
    private HeldAxis currentHeldAxis;

    public Transform positionControls;
    private int[] posID;
    private Vector3 displaceVector;
    private Vector3 displaceNormal;
    private Vector3 displaceNormalBackup;
    public float minMouseMoveDelta = 0.01f;

    public Transform rotationControls;
    private int[] rotID;
    private Vector3 rotatingUp;
    private Vector3 planeUp;

    public Transform scaleControls;
    private int[] scaID;
    private Vector3 scalingVector;
    private Vector3 scalingNormal;
    private Vector3 scalingNormalBackup;

    public float controlsScaleFactor = 0.75f;
    private int lastHelperID = -1; //set to -1 upon changing local and global
    private Vector3 lastWorldPos;
    public float helperDeselectedAlpha = 0.1f;

    [Space(20f)]
    public Transform modelAnchor;
    private ShapeInfo currentShapeBounds;

    public WrapperMode mode { get; private set; }

    private Vector3 unsnappedPosition;

    [System.Serializable]
    public enum WrapperMode
    {
        Position,
        Rotation,
        Scale
    }

    public enum HeldAxis
    {
        X,
        Y,
        Z,
        None
    }
    
    public bool wrapperActive { get; private set; }

    private void Awake()
    {
        posID = new int[3];
        rotID = new int[3];
        scaID = new int[3];
        for(int i = 0; i < positionControls.childCount; i++)
        {
            posID[i] = positionControls.GetChild(i).GetInstanceID();
            rotID[i] = rotationControls.GetChild(i).GetChild(0).GetInstanceID();
            scaID[i] = scaleControls.GetChild(i).GetInstanceID();
        }

        Vector2 sd = ShapesManager._instance.shapeToolsMenu.sizeDelta;
        sd.x = 0.8f * 0.175f * Screen.width;
        sd.y = 0.8f * 0.1f * Screen.height;
        ShapesManager._instance.shapeToolsMenu.sizeDelta = sd;

        unsnappedPosition = transform.position;
    }

    private void LateUpdate()
    {
        float appliedScale = controlsScaleFactor * Vector3.Distance(MapManager._instance.activeCamera.transform.position, transform.position);

        positionControls.localScale = rotationControls.localScale = scaleControls.localScale = appliedScale * Vector3.one;

        if(modelAnchor.childCount > 0)
        {
            //if (ShapesManager._instance.shapeToolsMenu.gameObject.activeSelf) { }

            Vector3 screenPos = MapManager._instance.activeCamera.cameraComp.WorldToScreenPoint(modelAnchor.GetChild(0).position);
            ShapesManager._instance.shapeToolsMenu.position = screenPos - (0.15f * Screen.height * Vector3.up);

            ShapesManager._instance.shapeToolsMenu.gameObject.SetActive(!MapManager._instance.activeCamera.rotatingFlag);
        }
    }

    public void GiveTransform(Transform toWrap)
    {
        if (modelAnchor.childCount > 0)
        {
            ShapesManager._instance.ReturnObjectToContainer(modelAnchor.GetChild(0)); //release anchored
        }

        bool local = ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Local;

        transform.position = toWrap.position;
        if (local)
            transform.rotation = toWrap.rotation;
        else
            transform.rotation = Quaternion.identity;

        toWrap.parent = modelAnchor;
        positionControls.gameObject.SetActive(mode == WrapperMode.Position);
        rotationControls.gameObject.SetActive(mode == WrapperMode.Rotation);
        scaleControls.gameObject.SetActive(mode == WrapperMode.Scale);
        currentShapeBounds = toWrap.GetComponent<ShapeInfo>();

        wrapperActive = true;
    }

    public void TurnOff()
    {
        positionControls.gameObject.SetActive(false);
        rotationControls.gameObject.SetActive(false);
        scaleControls.gameObject.SetActive(false);

        if (modelAnchor.childCount > 0)
        {
            currentShapeBounds = null;
            ShapesManager._instance.ReturnObjectToContainer(modelAnchor.GetChild(0)); //release anchored
        }

        ShapesManager._instance.shapeToolsMenu.gameObject.SetActive(false);
        currentHeldAxis = HeldAxis.None;
        wrapperActive = false;
    }

    public void ShapeToFloor()
    {
        if (!wrapperActive)
            return;

        Vector3 test = transform.position;
        test.y = MapManager._instance.mapTarget.bounds.ceilY;
        float floorY = MapManager._instance.mapTarget.GetTerrainYAt(test);

        float lowestY = float.MaxValue;
        for(int bn = 0; bn < currentShapeBounds.boundPoints.childCount; bn++)
        {
            float val = currentShapeBounds.boundPoints.GetChild(bn).position.y;
            if (val < lowestY)
                lowestY = val;
        }

        transform.position += (floorY - lowestY) * Vector3.up;

        ShapesManager._instance.CallChangeFlag();
    }

    public void ChangeToolMode(WrapperMode nuMode)
    {
        mode = nuMode;

        if (!wrapperActive)
            return;

        positionControls.gameObject.SetActive(mode == WrapperMode.Position);
        rotationControls.gameObject.SetActive(mode == WrapperMode.Rotation);
        scaleControls.gameObject.SetActive(mode == WrapperMode.Scale);

        if (nuMode == WrapperMode.Scale && ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) //scaling can only be done in local space. add a warning
            ShapesManager._instance.ChangeWrapperReferenceMode(1);
    }

    public bool ChangeReferenceMode(ShapesManager.ShapeToolMode nuMode)
    {
        if (nuMode == ShapesManager.ShapeToolMode.Global && mode == WrapperMode.Scale) //scaling can only be done in local space
            return false;

        modelAnchor.parent = null;
        if (nuMode == ShapesManager.ShapeToolMode.Local && modelAnchor.childCount > 0)
            transform.rotation = modelAnchor.GetChild(0).rotation;
        else
            transform.rotation = Quaternion.identity;

        modelAnchor.parent = transform;
        modelAnchor.SetAsFirstSibling();
        return true;
    }

    public void UpdateWrapperRotation()
    {
        modelAnchor.parent = null;
        if (modelAnchor.childCount > 0)
        {
            transform.rotation = modelAnchor.GetChild(0).rotation;
        }
        modelAnchor.parent = transform;
        modelAnchor.SetAsFirstSibling();
    }

    public void HelperTransformSet(int id, Vector3 hitPosition)
    {
        lastHelperID = id;
        GetTransformVectors(id);
        lastWorldPos = hitPosition;
        unsnappedPosition = transform.position;
    }

    public void HelperTransform(Ray screenRay)
    {
        if (!wrapperActive)
            return;

        if (mode == WrapperMode.Position)
        {
            float denominator = Vector3.Dot(screenRay.direction, displaceNormal);
            Vector3 norm = displaceNormal;

            if (denominator < 0.001f)
            {
                denominator = Vector3.Dot(screenRay.direction, displaceNormalBackup);
                norm = displaceNormalBackup;
            }
            
            float t = Vector3.Dot(transform.position- screenRay.origin, norm) / denominator;
            Vector3 p = screenRay.origin + (screenRay.direction * t);
            
            Vector3 proj = (p - lastWorldPos);
            if (proj.magnitude <= minMouseMoveDelta)
                return;

            proj = Vector3.Project(proj, displaceVector);

            if (currentShapeBounds.TryPosition(proj, false))
            {
                transform.position += proj;
                unsnappedPosition += proj;

                if (ShapesManager._instance.shapeSnapToGrid)
                {
                    Vector3 snapped = MapManager._instance.mapTarget.GetSnappedPosition(transform.position);
                    transform.position = snapped;

                    if (Vector3.Distance(unsnappedPosition, transform.position) > ShapesManager._instance.snapDistance)
                        transform.position = unsnappedPosition;
                }
            }

            lastWorldPos = p;
        }
        else if(mode == WrapperMode.Rotation)
        {
            float denominator = Vector3.Dot(screenRay.direction, planeUp);
            Vector3 norm = planeUp;

            bool demChange = false;
            if (denominator < 0.001f)
            {
                denominator = Vector3.Dot(screenRay.direction, -planeUp);
                norm = -planeUp;
                demChange = true;
            }

            float t = Vector3.Dot(transform.position - screenRay.origin, norm) / denominator;
            Vector3 p = screenRay.origin + (screenRay.direction * t);

            Vector3 angleNew = p - modelAnchor.GetChild(0).position;
            Vector3 angleOld = lastWorldPos - modelAnchor.GetChild(0).position;

            float angle = Vector3.SignedAngle(angleOld, angleNew, norm);

            if (Mathf.Abs(angle) <= 5f * minMouseMoveDelta)
                return;

            if (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Local)
            {
                norm = rotatingUp;
                if (demChange)
                    angle *= -1;
            }

            /*
            Vector3 worldAxis = modelAnchor.GetChild(0).TransformDirection(norm);
            modelAnchor.GetChild(0).Rotate(worldAxis, angle);
            */

            currentShapeBounds.TryRotation(norm, angle, ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global);

            /*
            if(ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global)
                modelAnchor.GetChild(0).Rotate(norm, angle, Space.World);
            else
                modelAnchor.GetChild(0).Rotate(norm, angle, Space.Self);
                */

            //modelAnchor.GetChild(0).rotation *= Quaternion.FromToRotation(angleOld, angleNew); //this works for local tho

            lastWorldPos = p;
        }
        else if(mode == WrapperMode.Scale)
        {
            float denominator = Vector3.Dot(screenRay.direction, scalingNormal);
            Vector3 norm = scalingNormal;

            if (denominator < 0.001f)
            {
                denominator = Vector3.Dot(screenRay.direction, scalingNormalBackup);
                norm = scalingNormalBackup;
            }

            float t = Vector3.Dot(transform.position - screenRay.origin, norm) / denominator;
            Vector3 p = screenRay.origin + (screenRay.direction * t);

            Vector3 proj = (p - lastWorldPos);
            if (proj.magnitude <= minMouseMoveDelta)
                return;

            proj = Vector3.Project(proj, scalingVector);

            proj = modelAnchor.GetChild(0).InverseTransformDirection(proj);

            Vector3 auxScale = modelAnchor.GetChild(0).localScale;
            auxScale += proj;

            currentShapeBounds.TryScaling(auxScale);
            //modelAnchor.GetChild(0).localScale = auxScale;

            lastWorldPos = p;
        }

        UpdateUIInputValues();

        ShapesManager._instance.CallChangeFlag();
    }

    private void GetTransformVectors(int id)
    {
        for(int i = 0; i < 3; i++)
        {
            if (mode == WrapperMode.Position)
            {
                if (id == posID[i])
                {
                    if (i == 0)
                    {
                        displaceVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        displaceNormal = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        displaceNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        currentHeldAxis = HeldAxis.X;
                    }
                    else if (i == 1)
                    {
                        displaceVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        displaceNormal = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        displaceNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        currentHeldAxis = HeldAxis.Y;
                    }
                    else if (i == 2)
                    {
                        displaceVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        displaceNormal = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        displaceNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        currentHeldAxis = HeldAxis.Z;
                    }
                    SetHelperGraphicSelected(i, WrapperMode.Position);
                }
            }
            else if (mode == WrapperMode.Rotation)
            {
                if (id == rotID[i])
                {
                    if (i == 0)
                    {
                        //rotatingUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        rotatingUp = Vector3.right;
                        planeUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        currentHeldAxis = HeldAxis.X;
                    }
                    else if (i == 1)
                    {
                        //rotatingUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        rotatingUp = Vector3.up;
                        planeUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        currentHeldAxis = HeldAxis.Y;
                    }
                    else if (i == 2)
                    {
                        //rotatingUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        rotatingUp = Vector3.forward;
                        planeUp = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        currentHeldAxis = HeldAxis.Z;
                    }
                    SetHelperGraphicSelected(i, WrapperMode.Rotation);
                }
            }
            else if (mode == WrapperMode.Scale)
            {
                if (id == scaID[i])
                {
                    if (i == 0)
                    {
                        scalingVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        scalingNormal= (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        scalingNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        currentHeldAxis = HeldAxis.X;
                    }
                    else if (i == 1)
                    {
                        scalingVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        scalingNormal = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        scalingNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        currentHeldAxis = HeldAxis.Y;
                    }
                    else if (i == 2)
                    {
                        scalingVector = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.forward : transform.forward;
                        scalingNormal = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.up : transform.up;
                        scalingNormalBackup = (ShapesManager._instance.shapeToolMode == ShapesManager.ShapeToolMode.Global) ? Vector3.right : transform.right;
                        currentHeldAxis = HeldAxis.Z;
                    }
                    SetHelperGraphicSelected(i, WrapperMode.Scale);
                }
            }
        }
    }

    public void SetHelperGraphicDeSelected()
    {
        SetHelperGraphicSelected(-1, mode);
    }

    public void ApplyInputChange(int inputIndex)
    {
        if(inputIndex == 0)
        {
            float x = InputToValue(ShapesManager._instance.positionInputX.text);
            float y = InputToValue(ShapesManager._instance.positionInputY.text);
            float z = InputToValue(ShapesManager._instance.positionInputZ.text);
            transform.position = new Vector3(x, y, z);
        }
        else if (inputIndex == 1)
        {
            float x = InputToValue(ShapesManager._instance.rotationInputX.text);
            float y = InputToValue(ShapesManager._instance.rotationInputY.text);
            float z = InputToValue(ShapesManager._instance.rotationInputZ.text);
            modelAnchor.GetChild(0).rotation = Quaternion.Euler(x, y, z);
        }
        else if (inputIndex == 2)
        {
            float x = InputToValue(ShapesManager._instance.scaleInputX.text, 1f);
            float y = InputToValue(ShapesManager._instance.scaleInputY.text, 1f);
            float z = InputToValue(ShapesManager._instance.scaleInputZ.text, 1f);
            modelAnchor.GetChild(0).localScale = new Vector3(x, y, z);
        }

        ShapesManager._instance.CallChangeFlag();
    }

    private float InputToValue(string insert, float def = 0f)
    {
        float value = def;
        if (float.TryParse(insert, out value))
        {
            return value;
        }

        return value;
    }

    public void UpdateUIInputValues()
    {
        if (ShapesManager._instance.positionInputMenu.activeSelf)
        {
            ShapesManager._instance.positionInputX.text = transform.position.x.ToString();
            ShapesManager._instance.positionInputY.text = transform.position.y.ToString();
            ShapesManager._instance.positionInputZ.text = transform.position.z.ToString();
        }
        else if (ShapesManager._instance.rotationInputMenu.activeSelf)
        {
            Vector3 eulerRot = modelAnchor.GetChild(0).rotation.eulerAngles;
            ShapesManager._instance.rotationInputX.text = eulerRot.x.ToString();
            ShapesManager._instance.rotationInputY.text = eulerRot.y.ToString();
            ShapesManager._instance.rotationInputZ.text = eulerRot.z.ToString();
        }
        else if (ShapesManager._instance.scaleInputMenu.activeSelf)
        {
            ShapesManager._instance.scaleInputX.text = modelAnchor.GetChild(0).localScale.x.ToString();
            ShapesManager._instance.scaleInputY.text = modelAnchor.GetChild(0).localScale.y.ToString();
            ShapesManager._instance.scaleInputZ.text = modelAnchor.GetChild(0).localScale.z.ToString();
        }
    }

    private void SetHelperGraphicSelected(int id, WrapperMode mode)
    {
        Transform prnt = positionControls;
        if (mode == WrapperMode.Rotation)
            prnt = rotationControls;
        else if (mode == WrapperMode.Scale)
            prnt = scaleControls;

        prnt.GetChild(0).GetComponent<MeshRenderer>().material = (id == 0 || id < 0) ? ShapesManager._instance.redHelpMat : ShapesManager._instance.redHelpMatFade;
        prnt.GetChild(1).GetComponent<MeshRenderer>().material = (id == 1 || id < 0) ? ShapesManager._instance.greenHelpMat : ShapesManager._instance.greenHelpMatFade;
        prnt.GetChild(2).GetComponent<MeshRenderer>().material = (id == 2 || id < 0) ? ShapesManager._instance.blueHelpMat : ShapesManager._instance.blueHelpMatFade;

        if(mode != WrapperMode.Rotation)
        {
            prnt.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = (id == 0 || id < 0) ? ShapesManager._instance.redHelpMat : ShapesManager._instance.redHelpMatFade;
            prnt.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material = (id == 1 || id < 0) ? ShapesManager._instance.greenHelpMat : ShapesManager._instance.greenHelpMatFade;
            prnt.GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material = (id == 2 || id < 0) ? ShapesManager._instance.blueHelpMat : ShapesManager._instance.blueHelpMatFade;
        }

        /*
        for(int i = 0; i < 3; i++)
        {
            Material mat = prnt.GetChild(i).GetComponent<MeshRenderer>().material;
            Color c = mat.color;
            c.a = (i == id || id < 0) ? 1f : helperDeselectedAlpha;
            mat.color = c;
            if(mode != WrapperMode.Rotation)
            {
                mat = prnt.GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material;
                c = mat.color;
                c.a = (i == id || id < 0) ? 1f : helperDeselectedAlpha;
                mat.color = c;
            }
        }
        */
    }
}
