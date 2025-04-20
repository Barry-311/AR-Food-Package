using UnityEngine;
using Vuforia;
using YourNamespace.Weather;


public class VuforiaTargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    public UnityAndGeminiV3 geminiScript;

    [Header("WeatherService")]
    public WeatherService weatherService;

    private bool promptSent = false;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (promptSent) return;
        if (geminiScript == null) return;

        if (targetStatus.Status == Status.TRACKED ||
            targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            //string targetName = behaviour.TargetName;
            //string prompt = $"Tell me something about {targetName} in 30 words.";
            //Debug.Log($"[Vuforia] prompt: {prompt}");

            //geminiScript.userMessage = prompt;
            //promptSent = true;         // Lock the prompt to prevent multiple sends
            //// Automatically send the chat message
            //// geminiScript.SendChat();

            // 1. 请求天气
            weatherService.OnWeatherReceived += resp =>
            {
                // 2. 构造带天气信息的提示
                string targetName = behaviour.TargetName;
                var cw = resp.current_weather;
                string summary = $"当前：{cw.temperature:F1}°C，" +
                                 $"风速{cw.windspeed:F1}m/s";
                string prompt = $"Tell me something interesting about {targetName} " +
                                $"given the weather ({summary}) in 30 words.";
                Debug.Log($"[Vuforia+Weather] prompt: {prompt}");

                // 3. 发给 Gemini
                geminiScript.userMessage = prompt;
                //geminiScript.SendChat();

                promptSent = true;
                // 取消订阅，防止重复触发
                weatherService.OnWeatherReceived = null;
            };

            weatherService.RequestCurrentWeather();
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }
}
