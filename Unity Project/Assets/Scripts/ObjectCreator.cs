using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public TMP_InputField myInputfield;

    void Start()
    {
        
    }

    void Update()
    {
        if (myInputfield.text.Length > 0 && Input.GetKeyUp(KeyCode.Return))
        {
            //clear the text
            myInputfield.text = "";
        }
    }
}
