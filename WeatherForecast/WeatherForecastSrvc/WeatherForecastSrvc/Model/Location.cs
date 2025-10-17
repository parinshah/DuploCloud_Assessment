using System.ComponentModel.DataAnnotations;

namespace WeatherForecastSrvc.Model
{
    public class Location
    {
        /// <summary>
        /// Id for the Location, so it auto-generated when we add the location in DB.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Latitude value for the location. Constraints on the range value and also it is required field.
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude value for the location. Constraints on the range value and also it is required field.
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }


        /// <summary>
        /// When new location is added, this field is going to be updated with current UTC datetime.
        /// This will be useful when the same request comes for adding the location we can send the response with creationtime.
        /// </summary>
        public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
    }
}
