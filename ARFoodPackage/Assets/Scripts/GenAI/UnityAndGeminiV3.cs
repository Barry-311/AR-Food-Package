using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;
using System.Text;
using System.Net;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

// 1) STT 请求的配置与数据结构
[System.Serializable]
public class RecognitionConfig
{
    public string encoding = "LINEAR16";
    public int sampleRateHertz = 16000;
    public string languageCode = "en-US";
}
[System.Serializable]
public class RecognitionAudio
{
    public string content;
}
[System.Serializable]
public class SpeechRecognitionRequest
{
    public RecognitionConfig config;
    public RecognitionAudio audio;
}
[System.Serializable]
public class SpeechRecognitionAlternative
{
    public string transcript;
}
[System.Serializable]
public class SpeechRecognitionResult
{
    public SpeechRecognitionAlternative[] alternatives;
}
[System.Serializable]
public class SpeechRecognitionResponse
{
    public SpeechRecognitionResult[] results;
}
/// <summary>
/// Gemini 请求的配置与数据结构
/// </summary>

[System.Serializable]
public class TextPart
{
    public string text;
}

[System.Serializable]
public class TextContent
{
    public string role;
    public TextPart[] parts;
}

[System.Serializable]
public class TextCandidate
{
    public TextContent content;
}

[System.Serializable]
public class TextResponse
{
    public TextCandidate[] candidates;
}

[System.Serializable]
public class ChatRequest
{
    public TextContent[] contents;
}

public class UnityAndGeminiV3 : MonoBehaviour
{
    public string userMessage = "";

    [Header("JSON API Configuration")]
    public TextAsset jsonApi;

    private string apiKey = "";
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private string ttsEndpoint = "https://texttospeech.googleapis.com/v1/text:synthesize";

    [Header("ChatBot Function")]
    public TMP_Text uiText;
    private TextContent[] chatHistory;

    // —— 新增 Speech-to-Text 字段 ——  
    [Header("Speech-to-Text 配置")]
    private const string sttEndpoint = "https://speech.googleapis.com/v1/speech:recognize";
    private AudioClip recording;
    private bool isRecording = false;
    private int recordedSamples = 0;

    void Start()
    {
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;
        chatHistory = new TextContent[] { };
    }

    public void SendChat()
    {
        if (string.IsNullOrEmpty(userMessage))
        {
            Debug.LogWarning("userMessage empty");
            return;
        }
        StartCoroutine(SendChatRequestToGemini(userMessage));
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        string url = $"{apiEndpoint}?key={apiKey}";

        TextContent userContent = new TextContent
        {
            role = "user",
            parts = new TextPart[]
            {
                new TextPart { text = newMessage }
            }
        };

        List<TextContent> contentsList = new List<TextContent>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray();

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };
        string jsonData = JsonUtility.ToJson(chatRequest);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    TextContent botContent = new TextContent
                    {
                        role = "model",
                        parts = new TextPart[]
                        {
                            new TextPart { text = reply }
                        }
                    };

                    Debug.Log(reply);
                    uiText.text = reply;

                    ////Speak(reply);

                    contentsList.Add(botContent);
                    chatHistory = contentsList.ToArray();
                }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }

    //****** TTS Function ******//
    // Get audio content from JSON response
    //private string ExtractAudioContent(string json)
    //{
    //    const string key = "\"audioContent\": \"";
    //    int start = json.IndexOf(key);
    //    if (start < 0) return null;
    //    start += key.Length;
    //    int end = json.IndexOf("\"", start);
    //    if (end < 0) return null;
    //    return json.Substring(start, end - start);
    //}

    //// Play audio from file
    //private IEnumerator PlayAudioFromFile(string filePath)
    //{
    //    using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
    //    {
    //        yield return www.SendWebRequest();
    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("Audio Load Error: " + www.error);
    //        }
    //        else
    //        {
    //            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
    //            var audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    //            audio.clip = clip;
    //            audio.Play();
    //        }
    //    }
    //}

    //// Call TTS and save as MP3 and play
    //public void Speak(string textToSpeak)
    //{
    //    StartCoroutine(SpeakText(textToSpeak));
    //}

    //private IEnumerator SpeakText(string text)
    //{
    //    string url = $"{ttsEndpoint}?key={apiKey}";

    //    // Use MP3 encoding
    //    string jsonData = $@"
    //    {{
    //        ""input"": {{ ""text"": ""{text.Replace("\"", "\\\"")}"" }},
    //        ""voice"": {{ ""languageCode"": ""en-US"", ""name"": ""en-US-Wavenet-D"" }},
    //        ""audioConfig"": {{ ""audioEncoding"": ""MP3"" }}
    //    }}";

    //    byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

    //    using (var www = new UnityWebRequest(url, "POST"))
    //    {
    //        www.uploadHandler = new UploadHandlerRaw(postData);
    //        www.downloadHandler = new DownloadHandlerBuffer();
    //        www.SetRequestHeader("Content-Type", "application/json");

    //        yield return www.SendWebRequest();

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("TTS Error: " + www.error);
    //        }
    //        else
    //        {
    //            Debug.Log("TTS Success!");
    //            string responseJson = www.downloadHandler.text;
    //            string audioBase64 = ExtractAudioContent(responseJson);
    //            if (!string.IsNullOrEmpty(audioBase64))
    //            {
    //                byte[] audioData = Convert.FromBase64String(audioBase64);
    //                string path = Path.Combine(Application.persistentDataPath, "tts.mp3");
    //                File.WriteAllBytes(path, audioData);
    //                StartCoroutine(PlayAudioFromFile(path));
    //            }
    //        }
    //    }
    //}


    // —— 新增：开始录音 ——  
    // 位置：SendChat() 方法之后
    public void StartVoiceInput()
    {
        if (isRecording) return;
        recording = Microphone.Start(null, false, 60, 16000);
        isRecording = true;
        uiText.text = "Recording…";
    }

    // —— 新增：停止录音并发起转写与对话 ——  
    // 位置：紧接 StartVoiceInput() 之后
    public void StopVoiceInput()
    {
        if (!isRecording) return;
        //Microphone.End(null);
        // 先获取实际录了多少采样点
        recordedSamples = Microphone.GetPosition(null);
        Microphone.End(null);

        isRecording = false;
        uiText.text = "Processing…";
        StartCoroutine(RecognizeAndSend());
    }

    // —— 新增：录音→WAV→Base64→STT→拼接50词限制→调用 SendChat() ——  
    // 位置：紧接 StopVoiceInput() 之后
    private IEnumerator RecognizeAndSend()
    {
        // —— A. 裁剪成只含 recordedSamples 的新 AudioClip —— 
        if (recordedSamples <= 0)
        {
            uiText.text = "录音失败，请重试";
            yield break;
        }
        AudioClip trimmed = AudioClip.Create(
            "trimmed",
            recordedSamples,
            recording.channels,
            recording.frequency,
            false);
        float[] allData = new float[recordedSamples * recording.channels];
        recording.GetData(allData, 0);
        trimmed.SetData(allData, 0);

        // —— B. WAV 打包 ——  
        byte[] wavData = WavUtility.FromAudioClip(trimmed);
        if (wavData == null || wavData.Length <= 44)
        {
            Debug.LogError("[STT] 录音数据太短，无法识别");
            uiText.text = "请多说几句再试";
            yield break;
        }
        string base64 = Convert.ToBase64String(wavData);

        // —— C. 构造请求 JSON ——  
        var reqObj = new SpeechRecognitionRequest
        {
            config = new RecognitionConfig(),
            audio = new RecognitionAudio { content = base64 }
        };
        string json = JsonUtility.ToJson(reqObj);

        string responseJson = null;

        // —— D. 发送 STT 请求 ——  
        using (var www = new UnityWebRequest($"{sttEndpoint}?key={apiKey}", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[STT] {www.error}\nBody: {www.downloadHandler.text}");
                uiText.text = "STT 错误";
                yield break;
            }

            // 在 using 内读取返回文本
            responseJson = www.downloadHandler.text;
        }

        // —— E. 解析转写并发给 Gemini ——  
        var resp = JsonUtility.FromJson<SpeechRecognitionResponse>(responseJson);
        if (resp.results != null && resp.results.Length > 0 &&
            resp.results[0].alternatives.Length > 0)
        {
            string transcript = resp.results[0].alternatives[0].transcript.Trim();
            Debug.Log($"[STT] 识别结果：{transcript}");
            userMessage = transcript + " Please answer within 50 words.";
            SendChat();
        }
        else
        {
            uiText.text = "未识别到文字";
        }
    }
}
