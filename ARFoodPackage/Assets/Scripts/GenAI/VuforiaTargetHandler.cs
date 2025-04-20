using UnityEngine;
using Vuforia;
using YourNamespace.Weather;

public class VuforiaTargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    public UnityAndGeminiV3 geminiScript;
    public WeatherService weatherService;
    private bool promptSent = false;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        HandleTargetPrompt(behaviour, targetStatus);
    }

    private void HandleTargetPrompt(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (promptSent) return;
        if (behaviour == null || geminiScript == null) return;
        if (targetStatus.Status != Status.TRACKED &&
            targetStatus.Status != Status.EXTENDED_TRACKED)
            return;

        weatherService.OnWeatherReceived += resp =>
        {
            string targetName = behaviour.TargetName;
            var cw = resp.current_weather;
            string summary = $"��ǰ��{cw.temperature:F1}��C������{cw.windspeed:F1}m/s";
            string prompt = $"Tell me something interesting about {targetName} " +
                            $"given the weather ({summary}) in 30 words.";
            Debug.Log($"[Vuforia+Weather] prompt: {prompt}");

            // ���� ֻ���� userMessage�������� ����  
            geminiScript.userMessage = prompt;
            //geminiScript.uiText.text = "[Ready to send] " + prompt;

            promptSent = true;
            weatherService.OnWeatherReceived = null;
        };

        weatherService.RequestCurrentWeather();
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    /// <summary>
    /// ��ť���ã�������һ��׼���� Prompt
    /// </summary>
    public void TriggerTargetPrompt()
    {
        if (!promptSent || geminiScript == null) return;
        geminiScript.SendChat();
        //geminiScript.uiText.text = "[Sent] " + geminiScript.userMessage;
    }
}
