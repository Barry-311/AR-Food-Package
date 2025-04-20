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
            string summary = $"当前：{cw.temperature:F1}°C，风速{cw.windspeed:F1}m/s";
            string prompt = $"Tell me something interesting about {targetName} " +
                            $"given the weather ({summary}) in 30 words.";
            Debug.Log($"[Vuforia+Weather] prompt: {prompt}");

            // —— 只设置 userMessage，不发送 ——  
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
    /// 按钮调用：发送上一步准备的 Prompt
    /// </summary>
    public void TriggerTargetPrompt()
    {
        if (!promptSent || geminiScript == null) return;
        geminiScript.SendChat();
        //geminiScript.uiText.text = "[Sent] " + geminiScript.userMessage;
    }
}
