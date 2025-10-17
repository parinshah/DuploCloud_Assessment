using System.ComponentModel.DataAnnotations;

namespace WeatherForecastSrvc.DataTransferObject
{
    public record AddLocationRequest
    (
      [Range(-90, 90)] double Latitude,
      [Range(-180, 180)] double Longitude
    );
}
