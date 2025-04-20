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

            // 1. ��������
            weatherService.OnWeatherReceived += resp =>
            {
                // 2. �����������Ϣ����ʾ
                string targetName = behaviour.TargetName;
                var cw = resp.current_weather;
                string summary = $"��ǰ��{cw.temperature:F1}��C��" +
                                 $"����{cw.windspeed:F1}m/s";
                string prompt = $"Tell me something interesting about {targetName} " +
                                $"given the weather ({summary}) in 30 words.";
                Debug.Log($"[Vuforia+Weather] prompt: {prompt}");

                // 3. ���� Gemini
                geminiScript.userMessage = prompt;
                //geminiScript.SendChat();

                promptSent = true;
                // ȡ�����ģ���ֹ�ظ�����
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
