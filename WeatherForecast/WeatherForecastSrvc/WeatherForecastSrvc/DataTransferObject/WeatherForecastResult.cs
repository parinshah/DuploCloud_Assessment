namespace WeatherForecastSrvc.DataTransferObject
{
    public class WeatherForecastResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Temperature { get; set; }
        public double Windspeed { get; set; }
        public string Time { get; set; } = string.Empty;
        public double WindDirection { get; set; }
        public int isDay { get; set; }
    }
}
