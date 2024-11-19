using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceReticle : MonoBehaviour
{
    public Camera cam;
    public Transform targetObject;

    [SerializeField] private RectTransform baseReticle;
    [SerializeField] private RectTransform arrowReticle;

    [SerializeField] private Color reticleColor;

    public Color ReticleColor => reticleColor;

    public bool inCamera { get; private set; }

    [Space(10f)]
    [SerializeField] private float baseDownPositioning = 300f;
    [SerializeField] private float arrowDownPositioning = 300f;
    [SerializeField] private Vector2 clampFactorX = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 clampFactorY = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 screenSizeX = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 screenSizeY = new Vector2(0.1f, 0.9f);

    private void Start()
    {
        baseReticle.GetChild(0).GetComponent<Image>().color = reticleColor;
        arrowReticle.GetChild(0).GetComponent<Image>().color = reticleColor;
    }

    void Update()
    {
        if (targetObject == null)
        {
            return;
        }

        Vector3 vp = cam.WorldToViewportPoint(targetObject.position);

        bool preCam = inCamera;

        inCamera = vp.x >= screenSizeX.x && vp.x <= screenSizeX.y && vp.y >= screenSizeY.x && vp.y <= screenSizeY.y;

        if (inCamera && !preCam)
        {
            baseReticle.gameObject.SetActive(true);
            arrowReticle.rotation = Quaternion.identity;
        }
        else if(!inCamera && preCam)
        {
            baseReticle.gameObject.SetActive(false);
        }

        if (inCamera)
        {
            baseReticle.position = cam.WorldToScreenPoint(targetObject.position) + (baseDownPositioning * Vector3.down);
            arrowReticle.position = cam.WorldToScreenPoint(targetObject.position) + (arrowDownPositioning * Vector3.down);
        }
        else
        {
            Vector3 cvp = new Vector3(Mathf.Clamp(vp.x, clampFactorX.x, clampFactorX.y), Mathf.Clamp(vp.y, clampFactorY.x, clampFactorY.y), 0f);
            Vector3 clampPosition = cam.ViewportToScreenPoint(cvp);

            Vector3 diff = cam.ViewportToScreenPoint(vp) - clampPosition;
            diff.z = 0f;

            arrowReticle.position = clampPosition;
            arrowReticle.up = diff.normalized;
        }
    }

    public void ChangeColor(Color color)
    {
        reticleColor = color;

        baseReticle.GetChild(0).GetComponent<Image>().color = reticleColor;
        arrowReticle.GetChild(0).GetComponent<Image>().color = reticleColor;
    }
}
