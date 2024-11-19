using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshContainScript : MonoBehaviour
{
    public Transform shapeToTest;
    public Transform pointTest;

    private MeshCollider collTest;
    private MeshRenderer rendTest;

    public Material insideMat;
    public Material outsideMat;

    // Update is called once per frame
    void Update()
    {
        if (shapeToTest == null)
            return;

        collTest = shapeToTest.GetChild(0).GetComponent<MeshCollider>();
        rendTest = pointTest.GetComponent<MeshRenderer>();

        if (IsInsideMeshCollider(collTest, pointTest.position))
        {
            rendTest.material = insideMat;
        }
        else
            rendTest.material = outsideMat;
    }

    private bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
    {
        var temp = Physics.queriesHitBackfaces;
        Ray ray = new Ray(point, Vector3.back);

        bool hitFrontFace = false;
        RaycastHit hit = default;

        Physics.queriesHitBackfaces = true;
        bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
        if (hitFrontOrBackFace)
        {
            Physics.queriesHitBackfaces = false;
            hitFrontFace = col.Raycast(ray, out hit, 100f);
        }
        Physics.queriesHitBackfaces = temp;

        if (!hitFrontOrBackFace)
        {
            return false;
        }
        else if (!hitFrontFace)
        {
            return true;
        }
        else
        {
            // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
            if (hit.distance > hit2.distance)
            {
                return true;
            }
            else
                return false;
        }

    }
}
