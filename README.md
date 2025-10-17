# DuploCloud_Assessment
WeatherForecastSrvc Backend API

# Weather Forecast Service

### A RESTful API built with .NET 7, EF Core, and Open-Meteo API

---

## Overview

This project is a **Weather Forecast API** that allows users to:

- Manage a list of **geographic locations** (latitude and longitude).  
- Retrieve the **current weather forecast** from the [Open-Meteo](https://open-meteo.com) API.  

It demonstrates best practices in:
- REST API design  
- Entity Framework Core with SQLite  
- Dependency Injection  
- External API consumption via `HttpClient`  
- Swagger documentation  
- Unit testing with xUnit  

---

## Tech Stack

| Layer | Technology |
|--------|-------------|
| Framework | .NET 7 |
| Web | ASP.NET Core Web API |
| Database | SQLite (EF Core 7) |
| Client Integration | Open-Meteo Public API |
| Testing | xUnit |
| Docs | Swagger / OpenAPI |

---

## Architecture

+----------------------------+
| Swagger UI |
| (HTTP Client Frontend) |
+-------------+--------------+
|
v
+-------------+--------------+
| ASP.NET Core API |
| Controllers + Routing Layer |
+-------------+--------------+
|
+----------+-----------+
| |
v v
+--------+ +-----------+
|Location| |Weather API|
|Service | |Service |
+---+----+ +-----+-----+
| |
v |
+---+------------------+ |
| EF Core DbContext | |
| (SQLite Persistence) | |
+----------------------+ |
|
v
External Open-Meteo API


---

## Project Structure

WeatherForecastSrvc/
│
├── Controllers/
│ ├── LocationController.cs
│ └── WeatherForecastController.cs
│
├── Data/
│ ├── AppDbContext.cs
│ ├── AppDbContextFactory.cs
│
├── DataTransferObject/
│ ├── AddLocationRequest.cs
│ ├── WeatherForecastResult.cs
│ └── GetWeatherRequest.cs
│
├── Model/
│ ├── Location.cs
│ └── WeatherForecast.cs
│
├── Services/
│ ├── LocationService.cs
│ └── WeatherForecastService.cs
│
├── Program.cs
└── appsettings.json


---

## API Endpoints

### **Location API**

| Method | Endpoint | Description | Returns |
|--------|-----------|--------------|----------|
| `POST` | `/api/locations` | Add new latitude/longitude | `200 OK` with location |
| `GET` | `/api/locations` | Get all stored locations | `200 OK` / `404 Not Found` |
| `DELETE` | `/api/locations/{id}` | Delete location by ID | `204 No Content` / `404` |

---

### **Weather Forecast API**

| Method | Endpoint | Query Params | Description | Returns |
|--------|-----------|---------------|-------------|----------|
| `GET` | `/api/weatherforecast` | `latitude`, `longitude` | Get forecast for coordinates | `200 OK` / `400` / `404` |
| `GET` | `/api/weatherforecast/{id}` | Location ID | Get forecast for stored location | `200 OK` / `404` |

---

## Data Model

### **Location**
| Field | Type | Description |
|-------|------|-------------|
| Id | `int` | Auto-increment primary key |
| Latitude | `double` | Range: -90 → 90 |
| Longitude | `double` | Range: -180 → 180 |
| CreationTime | `DateTimeOffset` | Auto-set to current UTC time |

**Unique Constraint:** (`Latitude`, `Longitude`)

---

### **WeatherForecast / CurrentWeather**
Mapped to Open-Meteo JSON structure using `[JsonPropertyName]`.

Example response:
```json
{
  "latitude": 37.7749,
  "longitude": -122.4194,
  "timezone": "GMT",
  "current_weather": {
    "temperature": 18.2,
    "windspeed": 12.4,
    "winddirection": 240,
    "is_day": 1,
    "time": "2025-10-17T15:45"
  }
}

Database Schema (SQLite)
Table: Locations

Column	Type	Constraints
Id	INTEGER	PK, AutoIncrement
Latitude	REAL	Required
Longitude	REAL	Required
CreationTime	TEXT	Default = UTC now
Unique Index	Latitude + Longitude	Prevent duplicates

Configuration
appsettings.json

json
Copy code
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=weather.db"
  },
  "WeatherApi": {
    "BaseUrl": "https://api.open-meteo.com/v1/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

Unit Tests
Test Project: WeatherForecastSrvc.Tests
Uses xUnit for testing WeatherForecastService.

Simulates API calls using custom HttpMessageHandler.

Test	Description
 Returns forecast on success	Verifies valid JSON deserialization
 Returns null on 500 error	Handles server failure gracefully
 Returns null on malformed JSON	Ensures safe fallback on bad data
 Returns null on network error	Simulates network failure
 Uses default base URL when config missing	Verifies configuration fallback
 Integration test (optional)	Can call real Open-Meteo API (disabled by default)

Running Locally
1.) Restore dependencies
bash
Copy code
dotnet restore
2.)Apply EF Core migrations
bash
Copy code
dotnet ef migrations add InitialCreate
dotnet ef database update
3.)Run the API
bash
Copy code
dotnet run
4.)Open Swagger UI
Go to:
 https://localhost:7172/swagger

Dependency Injection Setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<LocationService>();
builder.Services.AddHttpClient<WeatherForecastService>();

Error Handling
Scenario	Response	Status
Invalid coordinates	BadRequest	400
Location not found	NotFound	404
Weather API failure	NotFound with message	404
Duplicate location	Returns existing record	200

GitHub Clean Setup
To exclude local build files:

.gitignore

gitignore

.vs/
bin/
obj/
*.user
*.suo
TestResults/
Then clean and commit:

bash

git rm -r --cached .
git add .
git commit -m "Add clean project with .gitignore"
git push
