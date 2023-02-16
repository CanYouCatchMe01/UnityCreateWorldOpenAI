using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using OpenAI_API;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json.Linq;
using System.IO;

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

    //API
    OpenAI_API.OpenAIAPI myOpenAIAPI;
    SpeechRecognizer mySpeechRecognizer;

    void Start()
    {
        string secretsPath = Application.streamingAssetsPath + "/secrets.json";
        JObject secrets = JObject.Parse(File.ReadAllText(secretsPath));
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
