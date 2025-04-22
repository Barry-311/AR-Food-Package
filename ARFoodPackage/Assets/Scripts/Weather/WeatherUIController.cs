using UnityEngine;
using TMPro;
using System;

namespace GetWeather.Weather
{
    public class WeatherUIController : MonoBehaviour
    {
        [Header("引用 WeatherService")]
        public WeatherService weatherService;

        [Header("UI 文本引用 (TextMeshProUGUI)")]
        public TMP_Text weatherText;
        public TMP_Text localTimeText;

        void Start()
        {
            // 自动查找 WeatherService（也可以手动拖拽）
            if (weatherService == null)
                weatherService = FindObjectOfType<WeatherService>();

            if (weatherService != null)
            {
                weatherService.OnWeatherReceived += OnWeatherReceived;
                weatherService.RequestCurrentWeather();
            }
            else
            {
                Debug.LogError("未找到 WeatherService，请确认已在场景中添加并挂载该脚本。");
            }
        }

        private void OnWeatherReceived(WeatherResponse resp)
        {
            // 1. 转换天气码，并显示温度、风速
            string desc = WeatherCodeHelper.GetWeatherDescription(resp.current_weather.weathercode);
            weatherText.text = $"Weather: {desc}\n" +
                               $"Temp: {resp.current_weather.temperature}°C\n" +
                               $"Wind: {resp.current_weather.windspeed} m/s";

            // 2. 显示本地设备当前时间
            localTimeText.text = $"Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        void Update()
        {
            // 持续刷新本地时间（可选）
            localTimeText.text = $"Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
