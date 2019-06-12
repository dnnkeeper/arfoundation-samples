using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ARPointCloud))]
[RequireComponent(typeof(ParticleSystem))]
public class ARPointCloudParticleVizualizerExt : MonoBehaviour
{

    [ContextMenu("TestPS")]
    private void TestPS()
    {
        var ps = GetComponent<ParticleSystem>();
        //for (int i = 0; i < 60; i++)
        //{
        //    var newParticle = new ParticleSystem.Particle();
        //    newParticle.position = Random.insideUnitSphere * 1f;
        //    //newParticle.startSize = Random.Range(0.05f, 0.05f);
        //    newParticle.startColor = ps.main.startColor.color;
        //    newParticle.startSize = ps.main.startSize.constant;
        //    newParticle.remainingLifetime = 1f;
        //    newParticle.startLifetime = ps.main.startLifetime.constant;
        //    particlesList.Enqueue(newParticle);
        //}
        //particlesList.TrimExcess();
        //ps.SetParticles(particlesList.ToArray(), particlesList.Count);
        //Debug.Log(particlesList.Count);

        var s_Vertices = new List<Vector3>();
        for (int i = 0; i < 50; i++)
        {
            s_Vertices.Add(Random.onUnitSphere);
        }
            

        if (mesh == null)
            mesh = new Mesh();
        mesh.Clear();
        mesh.SetVertices(s_Vertices);

        var indices = new int[s_Vertices.Count];
        for (int i = 0; i < s_Vertices.Count; ++i)
        {
            indices[i] = i;
        }

        mesh.SetIndices(indices, MeshTopology.Points, 0);

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
            meshFilter.sharedMesh = mesh;
    }


    void OnPointCloudChanged(ARPointCloudUpdatedEventArgs eventArgs)
    {
        var color = m_ParticleSystem.main.startColor.color;
        var size = m_ParticleSystem.main.startSize.constant;
        pointCloudLength = m_PointCloud.positions.Length;
        particleCount = m_ParticleSystem.particleCount;
        //for (int i = 0; i < m_PointCloud.positions.Length; i++)
        //{
        //    var point = m_PointCloud.positions[i];
        //    pointsFromCloudList.Add(point);
        //    var newParticle = new ParticleSystem.Particle();
        //    newParticle.position = point;
        //    newParticle.startColor = color;
        //    newParticle.startSize = size;
        //    newParticle.remainingLifetime = Random.value;
        //    newParticle.startLifetime = 1f;
        //    particlesList.Enqueue(newParticle);
        //}

        //particlesList.TrimExcess();
        //m_ParticleSystem.SetParticles(particlesList.ToArray(), pointCloudLength );


        //var newPointsCount = particleCount - pointCloudLength;
        //if (newPointsCount > 0)
        //for (int i = newPointsCount; i > 0; i--)
        //{
        //    var point = m_PointCloud.positions[particleCount + i];
        //    ParticleSystem.EmitParams emitParam = new ParticleSystem.EmitParams();
        //    emitParam.position = point;
        //    m_ParticleSystem.Emit(emitParam, 1);

        //}
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null) {
            var shape = m_ParticleSystem.shape;
            shape.meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            //if (meshCached == null || meshCached.vertexCount < meshFilter.sharedMesh.vertexCount)
            //{
            //    meshCached = (Mesh)Instantiate(meshFilter.sharedMesh);
            //    shape.mesh = meshCached;
            //    Debug.Log("Set new Point Cloud Mesh: "+ meshCached.vertexCount);
            //    var vertList = new List<Vector3>();
            //    meshCached.GetVertices(vertList);
            //    for (int i = 0; i < Mathf.Min(10, vertList.Count); i++)
            //    {
            //        Debug.Log(i+" = "+vertList[i]);
            //    }
            //}
            shape.mesh = meshFilter.sharedMesh;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
            
        }

    }

    Mesh mesh;

    void Awake()
    {
        m_PointCloud = GetComponent<ARPointCloud>();
        m_ParticleSystem = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        m_PointCloud.updated += OnPointCloudChanged;
        UpdateVisibility();
    }

    void OnDisable()
    {
        m_PointCloud.updated -= OnPointCloudChanged;
        UpdateVisibility();
    }

    void Update()
    {
        //var ps = GetComponent<ParticleSystem>();
        //ps.SetParticles(particlesList.ToArray(), particlesList.Count);
        UpdateVisibility();
    }

    void UpdateVisibility()
    {
        var visible =
            enabled
            &&
            true;
            //(m_PointCloud.trackingState != TrackingState.None);

        SetVisible(visible);
    }

    void SetVisible(bool visible)
    {
        if (m_ParticleSystem == null)
            return;

        var renderer = m_ParticleSystem.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = visible;
    }

    ARPointCloud m_PointCloud;

    ParticleSystem m_ParticleSystem;

    public Queue<ParticleSystem.Particle> particlesList = new Queue<ParticleSystem.Particle>(100);

    //ParticleSystem.Particle[] particlesArray;

    int pointCloudLength, particleCount;

    static List<Vector3> pointsFromCloudList = new List<Vector3>();
}
