using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

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

}
