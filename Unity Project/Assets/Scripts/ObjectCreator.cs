using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    [Serializable]
    public class MeshData
    {
        public string name;
        public Mesh mesh;
    }
    public MeshData[] myMeshData;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
