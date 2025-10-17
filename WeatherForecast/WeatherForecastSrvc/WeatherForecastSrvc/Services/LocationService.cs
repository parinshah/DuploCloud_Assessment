using Microsoft.EntityFrameworkCore;
using WeatherForecastSrvc.Data;
using WeatherForecastSrvc.Model;

namespace WeatherForecastSrvc.Services
{
    public class LocationService
    {
        private readonly AppDbContext _db;


        /// <summary>
        /// DbContext is injected by ASP.NET Core via dependency injection
        /// </summary>
        /// <param name="db"></param>
        public LocationService(AppDbContext db)
        {
            _db = db;
        }


        /// <summary>
        /// Adds a new location if it doesn’t already exist.
        /// If the same latitude and longitude are already in the database, it simply returns the existing record.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<Location> AddLocationAsync(double latitude, double longitude, CancellationToken cancelToken)
        {
            //Check if the location already exists
            var existingLocation = await _db.Locations
                .FirstOrDefaultAsync(l => l.Latitude == latitude && l.Longitude == longitude, cancelToken);

            if (existingLocation != null)
            {
                // Return the same record if already present
                return existingLocation;
            }

            //Create a new Location entity and add it to the DBContext
            var location = new Location
            {
                Latitude = latitude,
                Longitude = longitude
                // CreatedAt automatically set by model default
            };

            _db.Locations.Add(location);

            try
            {
                // Save the changes to the database (executes SQL INSERT)
                await _db.SaveChangesAsync(cancelToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                var output = await _db.Locations
                                      .FirstOrDefaultAsync(l => l.Latitude == latitude && l.Longitude == longitude, cancelToken);

                //Here the exception happened because we got concurrent request on adding location.
                //But because they both passed existingLocation check and did not find the location.
                //They proceeded towards the insert and for first one insert was successful.
                //But for second insert failed because of unique constraint, hence we got into this exception.
                //! is just telling the compiler that we know there is no null value hence it should not show warning on possible null reference.
                return output!;
            }

            // EF Core fills the new Id after saving, so location.Id is now valid
            //When we do SaveChangesAsAsync it updates the in-memory location with value in DB.
            return location;
        }
        

        /// <summary>
        /// Get location details from DB using id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<Location?> GetLocationByIdAsync(int id, CancellationToken cancelToken)
        {
            return await _db.Locations.FindAsync(new object[] { id }, cancelToken);
        }


        /// <summary>
        /// Retrieves all locations from the database, ordered by their ID.
        /// Uses AsNoTracking for better performance since we’re only reading data from DB.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<List<Location>> GetAllLocationsAsync(CancellationToken cancelToken)
        {
            var query = _db.Locations
                .AsNoTracking()  // disables change tracking (read-only)
                .OrderBy(data => data.Id);

            var list = await query.ToListAsync(cancelToken);
            return list;
        }


        /// <summary>
        /// Deletes a location by ID. Returns true if deleted successfully, false if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLocationAsync(int id, CancellationToken cancelToken)
        {
            //Find the location by primary key
            var loc = await _db.Locations.FindAsync(new object[] { id }, cancelToken);

            if (loc == null)
            {
                // Nothing to delete
                return false;
            }

            //Remove the entity from the DbContext
            _db.Locations.Remove(loc);

            //Commit the delete to the database (executes SQL DELETE)
            await _db.SaveChangesAsync(cancelToken);

            return true;
        }
    }
}
