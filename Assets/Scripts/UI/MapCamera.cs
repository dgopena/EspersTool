using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public Camera cameraComp { get; private set; }

    public float cameraHeight = 4.5f;

    [Header("Move Settings")]
    public float moveSpeed = 10f;
    public float moveAcceleration = 15f;
    private float currentSpeed;
    public bool movingFlag { get; private set; }
    private Vector3 lastMoveDir;

    private Vector3 moveDirectionAcc;

    public Vector2 xBounds { get; private set; }
    public Vector2 zBounds { get; private set; }

    [Header("Rotate Settings")]
    public float rotateSpeed = 7f;
    public float rotateAcceleration = 10f;
    private float currentRotateSpeed;
    public bool rotatingFlag { get; private set; }

    private Vector3 currentRotation;
    private Vector3 lastRotateDir;

    private Vector3 rotDirectionAcc;

    public Vector2 pitchBounds;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;
    public float zoomAcceleration = 15f;
    private float currentZoomSpeed;
    public bool zoomingFlag { get; private set; }

    private float currentZoom;
    private Vector3 zoomAcc;

    [Header("Auto Move")]
    public float autoMoveDefAngle = 40f;
    public float angleCorrectionSpeed = 50f;
    public bool autoNavigating { get; private set; }
    private bool autoNavStoppingFlag = false;
    private float autoNavMoveMultiplier = 1f;
    private Vector3 autoNavTarget;
    private Vector3 autoNavDetectionDirection;
    private float stoppingTime;
    private float stoppingTimeStamp;

    public bool cameraLocked { get; private set; }

    public Vector2 zoomBounds;

    [Header("Quick Look")]
    [SerializeField] private float lookDistance = 5f;
    [SerializeField] private float lookHeight = 4f;

    private void Awake()
    {
        cameraComp = GetComponent<Camera>();

        cameraHeight = Mathf.Clamp(cameraHeight, zoomBounds.x, zoomBounds.y);
        Vector3 auxPos = transform.position;
        auxPos.y = cameraHeight;
        transform.position = auxPos;

        currentRotation = transform.rotation.eulerAngles;
        currentZoom = cameraComp.fieldOfView;
    }

    private void LateUpdate()
    {
        if (autoNavigating)
        {
            float distanceToGoal = Vector3.Distance((transform.position + autoNavDetectionDirection), autoNavTarget);
            if (!autoNavStoppingFlag)
            {
                float stopDistance = moveDirectionAcc.sqrMagnitude / (2f * autoNavMoveMultiplier * moveAcceleration);

                if (distanceToGoal <= stopDistance)
                {
                    //make expected time
                    stoppingTime = moveDirectionAcc.magnitude / (autoNavMoveMultiplier * moveAcceleration);
                    stoppingTimeStamp = Time.time;

                    autoNavStoppingFlag = true;
                }

                Vector3 dir = autoNavTarget - (transform.position + autoNavDetectionDirection);
                Move(dir);
            }
            else
            {
                Move(Vector3.zero);

                if((Time.time - stoppingTimeStamp) > stoppingTime) //  distanceToGoal <= 0.2f)
                {
                    autoNavigating = false;
                }
            }

            if(Mathf.Abs(transform.rotation.eulerAngles.x - autoMoveDefAngle) > 0.5f)
            {
                float angleCorrectionSign = (transform.rotation.eulerAngles.x < autoMoveDefAngle) ? 1f : -1f;
                Vector3 rot = transform.rotation.eulerAngles;
                rot.x = (angleCorrectionSign * Time.unscaledDeltaTime * angleCorrectionSpeed) + rot.x;
                transform.rotation = Quaternion.Euler(rot);
            }
            else
            {
                Vector3 rot = transform.rotation.eulerAngles;
                rot.x = autoMoveDefAngle;
                transform.rotation = Quaternion.Euler(rot);
            }
        }
    }

    public void SetBounds(float minX, float maxX, float minZ, float maxZ)
    {
        xBounds = new Vector2(minX, maxX);
        zBounds = new Vector2(minZ, maxZ);
    }


    public void Move(Vector3 direction, bool ignoreAcceleration = false)
    {
        if (direction.magnitude == 0f && moveDirectionAcc.magnitude > 0.05f)
            direction = -moveDirectionAcc.normalized;
        else if (direction.magnitude == 0f && moveDirectionAcc.magnitude <= 0.05f)
            return;

        if (!ignoreAcceleration)
        {
            moveDirectionAcc += autoNavMoveMultiplier * moveAcceleration * Time.unscaledDeltaTime * direction;
            if (moveDirectionAcc.magnitude > moveSpeed)
                moveDirectionAcc = moveSpeed * moveDirectionAcc.normalized;

            movingFlag = moveDirectionAcc.magnitude > 0.08f;

            Vector3 auxPosition = transform.position + (Time.unscaledDeltaTime * moveDirectionAcc);
            auxPosition.x = Mathf.Clamp(auxPosition.x, xBounds.x, xBounds.y);
            auxPosition.y = Mathf.Clamp(auxPosition.y, zoomBounds.x, zoomBounds.y);
            auxPosition.z = Mathf.Clamp(auxPosition.z, zBounds.x, zBounds.y);
            transform.position = auxPosition;
        }
        else
        {
            movingFlag = direction.magnitude > 0.08f;

            Vector3 auxPosition = transform.position + (moveSpeed * Time.unscaledDeltaTime * direction);
            auxPosition.x = Mathf.Clamp(auxPosition.x, xBounds.x, xBounds.y);
            auxPosition.y = Mathf.Clamp(auxPosition.y, zoomBounds.x, zoomBounds.y);
            auxPosition.z = Mathf.Clamp(auxPosition.z, zBounds.x, zBounds.y);
            transform.position = auxPosition;
        }
    }

    public void SetMovingFlag(bool value)
    {
        movingFlag = value;
    }

    public void Rotate(Vector3 direction, bool ignoreAcceleration = false)
    {
        if (direction.magnitude == 0f && rotDirectionAcc.magnitude > 0.05f)
            direction = -rotDirectionAcc.normalized;
        else if (direction.magnitude == 0f && rotDirectionAcc.magnitude <= 0.05f)
            return;

        if (!ignoreAcceleration)
        {
            rotDirectionAcc += rotateAcceleration * Time.unscaledDeltaTime * direction;
            if (rotDirectionAcc.magnitude > rotateSpeed)
                rotDirectionAcc = rotateSpeed * rotDirectionAcc.normalized;

            Vector3 auxRot = currentRotation + (Time.unscaledDeltaTime * rotDirectionAcc);
            auxRot.x = Mathf.Clamp(auxRot.x, pitchBounds.x, pitchBounds.y);

            transform.rotation = Quaternion.Euler(auxRot);
            currentRotation = auxRot;
        }
        else
        {
            Vector3 auxRot = currentRotation + (rotateSpeed * Time.unscaledDeltaTime * direction);
            auxRot.x = Mathf.Clamp(auxRot.x, pitchBounds.x, pitchBounds.y);

            transform.rotation = Quaternion.Euler(auxRot);
            currentRotation = auxRot;
        }
    }

    public void SetRotatingFlag(bool value)
    {
        rotatingFlag = value;
    }

    public void Zoom(float amount, bool ignoreAcceleration = false)
    {
        Vector3 direction = -amount * transform.forward;

        float accMag = zoomAcc.magnitude;
        if (direction.magnitude == 0f && accMag > 0.05f)
            direction = -zoomAcc.normalized;
        else if (direction.magnitude == 0f && accMag <= 0.05f)
            return;

        if (!ignoreAcceleration)
        {
            zoomAcc += zoomAcceleration * Time.unscaledDeltaTime * direction;
            if (zoomAcc.magnitude > moveSpeed)
                zoomAcc = zoomSpeed * zoomAcc.normalized;

            Vector3 auxPosition = transform.position + (Time.unscaledDeltaTime * zoomAcc);
            auxPosition.x = Mathf.Clamp(auxPosition.x, xBounds.x, xBounds.y);
            auxPosition.y = Mathf.Clamp(auxPosition.y, zoomBounds.x, zoomBounds.y);
            auxPosition.z = Mathf.Clamp(auxPosition.z, zBounds.x, zBounds.y);

            if (auxPosition.y != zoomBounds.x && auxPosition.y != zoomBounds.y) //don't move on limits
                transform.position = auxPosition;
            else
                zoomAcc = Vector3.zero;
        }
        else
        {
            Vector3 auxPosition = transform.position + (zoomSpeed * Time.unscaledDeltaTime * direction);
            auxPosition.x = Mathf.Clamp(auxPosition.x, xBounds.x, xBounds.y);
            auxPosition.y = Mathf.Clamp(auxPosition.y, zoomBounds.x, zoomBounds.y);
            auxPosition.z = Mathf.Clamp(auxPosition.z, zBounds.x, zBounds.y);

            if (auxPosition.y != zoomBounds.x && auxPosition.y != zoomBounds.y) //don't move on limits
                transform.position = auxPosition;
        }
    }

    public void SetZoomingFlag(bool value)
    {
        zoomingFlag = value;
    }

    public bool CameraChange()
    {
        return zoomingFlag || movingFlag || rotatingFlag;
    }

    public void AutoNavigateTo(Vector3 lookPoint, float navigationMultiplier = 1f)
    {
        autoNavTarget = lookPoint;
        autoNavTarget.y = MapManager._instance.floorHeight; //lock to floor

        Vector3 camFloorPos = transform.position;
        camFloorPos.y = MapManager._instance.floorHeight;

        float upDist = transform.position.y - MapManager._instance.floorHeight;

        float detectDist = upDist * Mathf.Tan(((90f - autoMoveDefAngle) * Mathf.PI) / 180f);

        Vector3 auxFWd = transform.forward;
        auxFWd.y = 0f;
        autoNavDetectionDirection = (detectDist * auxFWd) + camFloorPos;
        autoNavDetectionDirection = autoNavDetectionDirection - transform.position;

        autoNavMoveMultiplier = navigationMultiplier;

        autoNavStoppingFlag = false;
        autoNavigating = true;
    }
    
    public void CancelAutoNav()
    {
        autoNavigating = false;
    }

    public void CameraSnap(int actionCode)
    {
        if (MapManager._instance.toolMode != MapManager.ToolMode.GameMode)
            return;

        if (PieceManager._instance.pieceBeingEdited)
            return;

        if(actionCode == 5)
        {
            //relocate the camera in the same angle and height, looking to the center of the map

            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance = 0f;
            plane.Raycast(new Ray(transform.position, transform.forward), out distance);

            transform.position = -distance * transform.forward;
        }
        else
        {
            float planeDistance = Mathf.Sqrt((lookDistance * lookDistance) - (lookHeight * lookHeight));

            Vector3 modf = new Vector3(1f, 0f, 1f);
            if (actionCode == 6)
                modf = new Vector3(1f, 0f, 0f);
            else if (actionCode == 3)
                modf = new Vector3(1f, 0f, -1f);
            else if (actionCode == 2)
                modf = new Vector3(0f, 0f, -1f);
            else if (actionCode == 1)
                modf = new Vector3(-1f, 0f, -1f);
            else if (actionCode == 4)
                modf = new Vector3(-1f, 0f, 0f);
            else if (actionCode == 7)
                modf = new Vector3(-1f, 0f, 1f);
            else if (actionCode == 8 )
                modf = new Vector3(0f, 0f, 1f);

            modf *= planeDistance;
            modf += lookHeight * Vector3.up;

            transform.position = modf;
            transform.LookAt(Vector3.zero);
        }
    }

    private void OnDrawGizmos()
    {
        if (autoNavigating)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + autoNavDetectionDirection, 0.3f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(autoNavTarget, 0.45f);
        }
    }
}
