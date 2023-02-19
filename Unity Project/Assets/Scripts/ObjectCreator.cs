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
    public TMP_Text myTPMText;
    
    //OpenAI
    OpenAI_API.OpenAIAPI myOpenAIAPI;
    Task<CompletionResult> myGenerateTask = null;
    string myAIText = "Create a json block from prompt.\nExample:\ntext:Create a blue cube at position zero zero zero\njson:{\"id\": 0, \"position\": {\"x\": 0, \"y\": 0, \"z\": 0}, \"scale\": {\"x\": 1.0, \"y\": 1.0, \"z\": 1.0}, \"shape\": \"cube\", \"color\": {\"r\": 0.0, \"g\": 0.0, \"b\": 1.0}}\ntext:remove or delete the blue cube\njson:{\"id\": 0, \"remove\": true}\nReal start with id 0:\ntext:";
    const string myStartSequence = "\njson:";
    const string myRestartSequence = "\ntext:\n";

    //Speech
    public bool myRecordMic = true;
    SpeechRecognizer mySpeechRecognizer;
    string mySpeechText = "";
    string myPreviousInputText = "";

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
            mySpeechText = e.Result.Text; //Change continuously
            Debug.Log(e.Result.Text);
        };

        mySpeechRecognizer.Recognized += (s, e) =>
        {
            mySpeechText = "";
        };

        StartOrStopSpeech();
    }

    void Update()
    {
        //Update the text
        myTPMText.text = myAIText;

        //Update text with speech
        if (mySpeechText == "")
        {
            myPreviousInputText = myInputfield.text;
        }
        else
        {
            myInputfield.text = myPreviousInputText + " " + mySpeechText;

            //Move to the end of the text
            myInputfield.caretPosition = myInputfield.text.Length;
            myInputfield.ActivateInputField(); //Text needs to be selected
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Submit();
        }

        //Wait for the AI responce
        if (myGenerateTask != null && myGenerateTask.IsCompleted)
        {
            string responce = myGenerateTask.Result.ToString();
            HandleAIResponce(responce);
            myAIText += responce + myRestartSequence;
            myGenerateTask = null;
        }
    }

    public void Submit()
    {
        if (myInputfield.text.Length > 0)
        {
            myAIText += myInputfield.text + myStartSequence;
            //myGenerateTask = GenerateAIResponce(myOpenAIAPI, aiText); //Run async

            myInputfield.text = ""; //Clear input
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
