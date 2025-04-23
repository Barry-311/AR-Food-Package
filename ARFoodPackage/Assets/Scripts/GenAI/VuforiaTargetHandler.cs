using UnityEngine;
using Vuforia;
using GetWeather.Weather;
using System.Collections;
using System.Collections.Generic;

public class VuforiaTargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    public UnityAndGeminiV3 geminiScript;
    public WeatherService weatherService;
    public GameObject Nutrients;
    public GameObject EarthRoot;
    public GameObject Earth;

    private GameObject earthInstance;

    private bool promptSent = false;

    // Introduction models for two targets
    private readonly Dictionary<string, string> brandIntroTemplates = new Dictionary<string, string>()
{
    { "bushmills", "Introduce the product: Bushmills Irish Whiskey (Triple distilled), " +
            "including the history of this brand and the flavour of this product, " +
            "Introduce only the product and brand. Do not consider weather or recipes, " +
            "in 50 words" },
    { "starbucks", "Introduce the product: Starbucks Caramel Latte (Instant Coffee), " +
            "including the brand features and how good this product is, " +
            "Introduce only the product and brand. Do not consider weather or recipes, " +
            "in 50 words"}
};

    // Menu models for two targets including weather
    private readonly Dictionary<string, string> recipeTemplates = new Dictionary<string, string>()
{
    { "bushmills", "The weather is：{weather}. " +
            "Please recommend a recipe for drinking or mixing this Bushmills whisky, " +
            "in 70 words" },
    { "starbucks", "The weather is：{weather}. " +
            "Please recommend a method and creative recipe for using Starbucks instant latte coffee, " +
            "in 70 words" }
};

    private string currentTargetName;

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
        Debug.Log($"[Vuforia] Detected target: '{behaviour.TargetName}'");
        if (promptSent) return;
        if (targetStatus.Status != Status.TRACKED && targetStatus.Status != Status.EXTENDED_TRACKED)
            return;

        currentTargetName = behaviour.TargetName.ToLower(); // bushmills or starbucks
        promptSent = true;

        if (brandIntroTemplates.TryGetValue(currentTargetName, out var introPrompt))
        {
            geminiScript.userMessage = introPrompt;
            // geminiScript.SendChat();
        }
    }

    // 新增：激活 Nutrients 后，8 秒再隐藏
    private IEnumerator ShowNutrientsForSeconds(float seconds)
    {
        if (Nutrients == null)
            yield break;

        Nutrients.SetActive(true);
        yield return new WaitForSeconds(seconds);
        Nutrients.SetActive(false);
    }


    /// <summary>
    /// 等待 seconds 后销毁 go，并把实例引用清空
    /// </summary>
    private IEnumerator RemoveAfterSeconds(GameObject go, float seconds)
     {
    yield return new WaitForSeconds(seconds);
            if (go != null) Destroy(go);
            if (earthInstance == go) earthInstance = null;
     }

    [Header("Trigger Cooldown")]
    public float triggerCooldown = 10f;  // 冷却时长（秒）
    private float lastTriggerTime = -Mathf.Infinity;

    [ContextMenu("▶ Trigger Brand Intro")]
    public void TriggerBrandIntroPrompt()
    {
        if (Time.time - lastTriggerTime < triggerCooldown) return;
        lastTriggerTime = Time.time;

        // —— 新增：在 EarthRoot 下创建 Earth（如果没创建过的话）
        if (earthInstance == null && EarthRoot != null && Earth != null)
        {
            earthInstance = Instantiate(Earth, EarthRoot.transform);
            // 8 秒后删除它
            StartCoroutine(RemoveAfterSeconds(earthInstance, 8f));
        }
        if (string.IsNullOrEmpty(currentTargetName) || geminiScript == null) return;

        // 发送聊天
        geminiScript.SendChat();

    }

    [ContextMenu("▶ Trigger Recipe Intro")]
    public void TriggerRecipePrompt()
    {
        if (Time.time - lastTriggerTime < triggerCooldown) return;
        lastTriggerTime = Time.time;

        // **新增**：启动协程，把 Nutrients 激活 8 秒后再隐藏
        StartCoroutine(ShowNutrientsForSeconds(8f));

        if (string.IsNullOrEmpty(currentTargetName)) return;

        // get current weather
        weatherService.OnWeatherReceived += resp =>
        {
            var cw = resp.current_weather;
            string desc = WeatherCodeHelper.GetWeatherDescription(cw.weathercode);

            string summary = $"{desc}, {cw.temperature:F1}°C, wind {cw.windspeed:F1} m/s";

            if (recipeTemplates.TryGetValue(currentTargetName, out var template))
            {
                // set userMessage and send chat
                geminiScript.userMessage = template.Replace("{weather}", summary);
                geminiScript.SendChat();
            }

            weatherService.OnWeatherReceived = null;
        };
        weatherService.RequestCurrentWeather();
    }


    void OnDestroy()
    {
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

}
