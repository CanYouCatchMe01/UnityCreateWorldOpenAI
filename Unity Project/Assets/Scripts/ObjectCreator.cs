using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using OpenAI_API;
using OpenAI_API.Completions;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;

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
    
    //OpenAI
    OpenAI_API.OpenAIAPI myOpenAIAPI;
    Task<CompletionResult> myGenerateTask = null;
    string aiText = "Create a json block from prompt.\nExample:\ntext:Create a blue cube at position zero zero zero\njson:{\"id\": 0, \"position\": {\"x\": 0, \"y\": 0, \"z\": 0}, \"scale\": {\"x\": 1.0, \"y\": 1.0, \"z\": 1.0}, \"shape\": \"cube\", \"color\": {\"r\": 0.0, \"g\": 0.0, \"b\": 1.0}}\ntext:remove or delete the blue cube\njson:{\"id\": 0, \"remove\": true}\nReal start with id 0:\ntext:";
    string startSequence = "\njson:";
    string restartSequence = "\ntext:\n";

    //Speech
    public bool myRecordMic = true;
    SpeechRecognizer mySpeechRecognizer;
    string speechText = "";

    void Start()
    {
        //Load keys
        string secretsPath = Application.streamingAssetsPath + "/secrets/secrets.json";
        JObject secrets = JObject.Parse(File.ReadAllText(secretsPath));
        string openAIKey = secrets["OPENAI_API_KEY"].ToString();
        string speechKey = secrets["SPEECH_KEY"].ToString();
        string speechRegion = secrets["SPEECH_REGION"].ToString();

        //Open AI
        myOpenAIAPI = new OpenAI_API.OpenAIAPI(openAIKey);
        
        //Azure speech to text AI
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        mySpeechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        mySpeechRecognizer.Recognizing += (s, e) =>
        {
            speechText = e.Result.Text; //Change continuously
            Debug.Log(speechText);
        };

        mySpeechRecognizer.Recognized += (s, e) =>
        {
            myInputfield.text += speechText; //Update the input when done speeching
            speechText = "";
            Debug.Log(myInputfield.text);
        };

        StartOrStopSpeech();
    }

    void Update()
    {
        //User presses Enter
        if (myInputfield.text.Length > 0 && Input.GetKeyUp(KeyCode.Return)) 
        {
            aiText += myInputfield.text + startSequence;
            myGenerateTask = GenerateAIResponce(myOpenAIAPI, aiText); //Run async

            myInputfield.text = ""; //Clear input
        }

        if (myGenerateTask != null && myGenerateTask.IsCompleted)
        {
            string responce = myGenerateTask.Result.ToString();
            HandleAIResponce(responce);
            aiText += responce + restartSequence;
            myGenerateTask = null;
        }
    }

    //Mic stuff
    void StartOrStopSpeech()
    {
        if (myRecordMic)
        {
            mySpeechRecognizer.StartContinuousRecognitionAsync().Wait();
        }
        else
        {
            mySpeechRecognizer.StopContinuousRecognitionAsync().Wait();
        }
    }

    async Task<CompletionResult> GenerateAIResponce(OpenAI_API.OpenAIAPI anApi, string aPrompt)
    {
        var request = new CompletionRequest(
                prompt: aPrompt,
                model: OpenAI_API.Models.Model.CushmanCode,
                temperature: 0.1,
                max_tokens: 256,
                top_p: 1.0,
                frequencyPenalty: 0.0,
                presencePenalty: 0.0,
                stopSequences: new string[] { "text:", "json:", "\n" }
                );
        var result = await anApi.Completions.CreateCompletionAsync(request);
        return result;
    }

    void HandleAIResponce(string aResponce)
    {

    }


}
