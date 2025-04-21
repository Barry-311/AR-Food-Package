using System;

namespace GetWeather.Weather
{
    [Serializable]
    public class CurrentWeather
    {
        public double temperature;    // 气温，单位 °C
        public double windspeed;      // 风速，单位 m/s
        public int weathercode;    // 天气状况码
        public string time;           // 时间戳
    }

    [Serializable]
    public class WeatherResponse
    {
        public CurrentWeather current_weather;
    }
}
