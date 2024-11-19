using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class MeshComplexParallel : MonoBehaviour
{
    NativeArray<Vector3> m_Vertices;
    NativeArray<Vector3> m_Normals;

    Vector3[] m_ModifiedVertices;
    Vector3[] m_ModifiedNormals;

    MeshModJob m_MeshModJob;
    JobHandle m_JobHandle;

    Mesh m_Mesh;

    private bool set = false;

    BattleMap.BoundSet bounds;
    private float maxTerrainHeight;
    private float minTerrainHeight;

    private bool pendingFlag = false;

    public void Set(BattleMap.BoundSet bounds)
    {
        m_Mesh = gameObject.GetComponent<MeshFilter>().mesh;
        m_Mesh.MarkDynamic();

        // this persistent memory setup assumes our vertex count will not expand
        m_Vertices = new NativeArray<Vector3>(m_Mesh.vertices, Allocator.Persistent);
        m_Normals = new NativeArray<Vector3>(m_Mesh.normals, Allocator.Persistent);

        m_ModifiedVertices = new Vector3[m_Vertices.Length];
        m_ModifiedNormals = new Vector3[m_Vertices.Length];

        this.bounds = bounds;
        maxTerrainHeight = MapManager._instance.mapTarget.maxMorphHeight;
        minTerrainHeight = MapManager._instance.mapTarget.minMorphHeight;

        set = true;
    }

    struct MeshModJob : IJobParallelFor
    {
        public Vector3 refPoint;
        public Vector3 direction;

        public NativeArray<Vector3> vertices;
        public NativeArray<Vector3> normals;

        public float deltaTime;
        public float radius;
        public float fracExponent;
        public float lowerY;
        public float upperY;

        public float strength;

        public void Execute(int i)
        {
            var vertex = vertices[i];

            float auxY = vertex.y;
            vertex.y = refPoint.y;

            float distance = Vector3.Distance(vertex, refPoint);

            vertex.y = auxY;

            if (distance > radius)
                return;

            float frac = (radius - distance) / radius;

            frac = Mathf.Pow(frac, fracExponent);

            vertex = vertex + (frac * strength * deltaTime) * direction;
            vertex.y = Mathf.Clamp(vertex.y, lowerY, upperY);

            vertices[i] = vertex;

            //normals[i] = new Vector3(vertex.y, 0.5f, vertex.y).normalized;
        }
    }

    /*
    public void Update()
    {
        m_MeshModJob = new MeshModJob()
        {
            refPoint = testPoint.position,
            vertices = m_Vertices,
            normals = m_Normals,
            deltaTime = Time.deltaTime,
            sinTime = Mathf.Sin(Time.time),
            cosTime = Mathf.Cos(Time.time),
            strength = m_Strength / 5f  // map .05-1 range to smaller real strength
        };

        m_JobHandle = m_MeshModJob.Schedule(m_Vertices.Length, 64);
    }
    */

    public void LateUpdate()
    {
        if (!set)
            return;

        try
        {
            if (m_MeshModJob.vertices.Length == 0)
                return;
        }
        catch(System.Exception e)
        {
            return;
        }

        m_JobHandle.Complete();

        if (m_JobHandle.IsCompleted && pendingFlag)
        {
            // copy our results to managed arrays so we can assign them
            m_MeshModJob.vertices.CopyTo(m_ModifiedVertices);
            m_MeshModJob.normals.CopyTo(m_ModifiedNormals);

            m_Mesh.vertices = m_ModifiedVertices;
            m_Mesh.normals = m_ModifiedNormals;

            m_Mesh.RecalculateNormals();
            pendingFlag = false;
        }
    }

    public void ReceiveJob(Vector3 point, float radiusValue, float exponent, float morphStrength, float up)
    {
        if (!set)
            return;

        m_MeshModJob = new MeshModJob()
        {
            refPoint = point,
            direction = up * Vector3.up,
            vertices = m_Vertices,
            normals = m_Normals,
            deltaTime = Time.deltaTime,
            radius = radiusValue,
            fracExponent = exponent,
            lowerY = minTerrainHeight,
            upperY = maxTerrainHeight,
            strength = morphStrength,
        };

        m_JobHandle = m_MeshModJob.Schedule(m_Vertices.Length, 64);
        pendingFlag = true;
    }

    public void CleanNativeArrays()
    {
        if(m_Vertices.IsCreated)
        {
            m_Vertices.Dispose();
        }
        if (m_Normals.IsCreated)
        {
            m_Normals.Dispose();
        }
    }

    private void OnDestroy()
    {
        // make sure to Dispose() any NativeArrays when we're done
        m_Vertices.Dispose();
        m_Normals.Dispose();
    }
}