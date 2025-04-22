using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

namespace GetWeather.Weather
{
    public class WeatherService : MonoBehaviour
    {
        [Header("地理坐标（可在 Inspector 中修改）")]
        public double latitude = 52.52;   // 默认：柏林
        public double longitude = 13.41;

        /// <summary>
        /// 获取到天气后回调
        /// </summary>
        public Action<WeatherResponse> OnWeatherReceived;

        /// <summary>
        /// 外部调用：开始获取当前天气
        /// </summary>
        public void RequestCurrentWeather()
        {
            StartCoroutine(FetchWeather());
        }

        private IEnumerator FetchWeather()
        {
            string url = $"https://api.open-meteo.com/v1/forecast?" +
                         $"latitude={latitude}&longitude={longitude}&current_weather=true";

            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[WeatherService] 请求失败：{www.error}");
                    yield break;
                }

                // 反序列化 JSON 到 WeatherResponse
                WeatherResponse resp = JsonUtility.FromJson<WeatherResponse>(www.downloadHandler.text);
                OnWeatherReceived?.Invoke(resp);
            }
        }
    }
}
