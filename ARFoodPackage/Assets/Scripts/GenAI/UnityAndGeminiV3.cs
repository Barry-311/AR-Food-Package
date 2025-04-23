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
/// Gemini V3 API response classes
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
    public TMP_Text aiResText;
    private TextContent[] chatHistory;

    [Header("Speech-to-Text Config")]
    private const string sttEndpoint = "https://speech.googleapis.com/v1/speech:recognize";
    private AudioClip recording;
    private bool isRecording = false;
    private int recordedSamples = 0;

    void Start()
    {
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;

        TextContent systemContent = new TextContent
        {
            role = "user",
            parts = new TextPart[]
        {
            new TextPart
            {
                text =
@"You are an AI specialized in creating captivating introductions for AR-enhanced food packaging. 
Your goal is to spark curiosity and excitement about the product. 
When responding, highlight Personal benefits and storytelling that resonate with users.
Keep tone enthusiastic, concise (respect any word limits), and focused on customer engagement.
Only output plain text without any special characters or formatting."
            }
        }
        };

        // 初始化聊天历史，只包含这条系统指令
        chatHistory = new TextContent[] { systemContent };
        //chatHistory = new TextContent[] { };
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

                    Speak(reply);

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
    private string ExtractAudioContent(string json)
    {
        const string key = "\"audioContent\": \"";
        int start = json.IndexOf(key);
        if (start < 0) return null;
        start += key.Length;
        int end = json.IndexOf("\"", start);
        if (end < 0) return null;
        return json.Substring(start, end - start);
    }

    // Play audio from file
    private IEnumerator PlayAudioFromFile(string filePath)
    {
        using (var www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Audio Load Error: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                var audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                audio.clip = clip;
                audio.Play();
            }
        }
    }

    // Call TTS and save as MP3 and play
    public void Speak(string textToSpeak)
    {
        StartCoroutine(SpeakText(textToSpeak));
    }

    private IEnumerator SpeakText(string text)
    {
        string url = $"{ttsEndpoint}?key={apiKey}";

        // Use MP3 encoding
        string jsonData = $@"
        {{
            ""input"": {{ ""text"": ""{text.Replace("\"", "\\\"")}"" }},
            ""voice"": {{ ""languageCode"": ""en-US"", ""name"": ""en-US-Wavenet-D"" }},
            ""audioConfig"": {{ ""audioEncoding"": ""MP3"" }}
        }}";

        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (var www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + www.error);
            }
            else
            {
                Debug.Log("TTS Success!");
                string responseJson = www.downloadHandler.text;
                string audioBase64 = ExtractAudioContent(responseJson);
                if (!string.IsNullOrEmpty(audioBase64))
                {
                    byte[] audioData = Convert.FromBase64String(audioBase64);
                    string path = Path.Combine(Application.persistentDataPath, "tts.mp3");
                    File.WriteAllBytes(path, audioData);
                    StartCoroutine(PlayAudioFromFile(path));
                }
            }
        }
    }


    // **** Start Recording and STT **** // 
    public void StartVoiceInput()
    {
        if (isRecording) return;
        recording = Microphone.Start(null, false, 60, 16000);
        isRecording = true;
        aiResText.text = "Recording…";
    }

    // **** Stop Recording and STT **** // 
    public void StopVoiceInput()
    {
        if (!isRecording) return;
        //Microphone.End(null);
        recordedSamples = Microphone.GetPosition(null);
        Microphone.End(null);

        isRecording = false;
        aiResText.text = "Processing…";
        StartCoroutine(RecognizeAndSend());
    }

    // Record -> WAV -> Base64 -> JSON -> STT  
    private IEnumerator RecognizeAndSend()
    {
        // a new AudioClip is created to avoid the error of "AudioClip is not set to a valid value"
        if (recordedSamples <= 0)
        {
            aiResText.text = "Recording Failed, please retry";
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

        // WAV to Base64 
        byte[] wavData = WavUtility.FromAudioClip(trimmed);
        if (wavData == null || wavData.Length <= 44)
        {
            Debug.LogError("Recording too short to recognize");
            aiResText.text = "Please tell me more about your question";
            yield break;
        }
        string base64 = Convert.ToBase64String(wavData);

        // JSON request
        var reqObj = new SpeechRecognitionRequest
        {
            config = new RecognitionConfig(),
            audio = new RecognitionAudio { content = base64 }
        };
        string json = JsonUtility.ToJson(reqObj);

        string responseJson = null;

        // Send request to STT
        using (var www = new UnityWebRequest($"{sttEndpoint}?key={apiKey}", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[STT] {www.error}\nBody: {www.downloadHandler.text}");
                aiResText.text = "STT Error";
                yield break;
            }

            // Get response
            responseJson = www.downloadHandler.text;
        }

        // Send response to Gemini
        var resp = JsonUtility.FromJson<SpeechRecognitionResponse>(responseJson);
        if (resp.results != null && resp.results.Length > 0 &&
            resp.results[0].alternatives.Length > 0)
        {
            string transcript = resp.results[0].alternatives[0].transcript.Trim();
            Debug.Log($"[STT] Recognization：{transcript}");
            aiResText.text = transcript;
            userMessage = transcript + " Please answer within 50 words.";
            SendChat();
        }
        else
        {
            aiResText.text = "NOTHING";
        }
    }
}
