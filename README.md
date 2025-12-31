# Turn Signal Violation Tracker

A modern .NET 8 Blazor Server application for tracking and reporting turn signal violations. Features AI-powered voice logging, GPS location capture, and comprehensive statistics visualization.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4)
![MudBlazor](https://img.shields.io/badge/MudBlazor-8.x-7B1FA2)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Dapper-CC2927)

## Features

### 🎤 Voice-Powered Violation Logging
- Record voice descriptions of traffic violations
- AI extracts structured data (car make, model, color, year, violation type, location)
- Supports both OpenAI and Google Gemini AI providers
- Configurable via appsettings

### 📍 GPS Location Capture
- Automatic browser geolocation
- Reverse geocoding to get city, state, and country
- Manual location entry option

### 📊 Comprehensive Dashboard
- Total violations overview
- Violations by car brand (with logos)
- Violations by car color
- Violations by type (pie/donut charts)
- Time of day analysis
- Day of week patterns
- 30-day trend visualization
- Location hotspots

### 🚗 Car Brand Logos
- Pre-populated database with 35+ major car brands
- Automatic logo display in statistics
- Easy to add new brands

### 🔐 Simple Authentication
- Username/password login
- Pre-configured admin account
- Session-based auth state

## Tech Stack

- **Frontend**: Blazor Server (.NET 8)
- **UI Framework**: MudBlazor (Material Design)
- **Backend**: ASP.NET Core
- **Database**: SQL Server with Dapper (no EF Core)
- **AI Providers**: OpenAI (GPT-4, Whisper) / Google Gemini
- **Geocoding**: OpenStreetMap Nominatim

## Project Structure

```
TurnSignalViolationTracker/
├── src/
│   ├── TurnSignalViolationTracker.Core/        # Domain models, interfaces, DTOs
│   ├── TurnSignalViolationTracker.Infrastructure/  # Data access, AI services
│   └── TurnSignalViolationTracker.Web/         # Blazor Server application
├── database/
│   ├── 001_CreateDatabase.sql                  # Database schema
│   └── 002_SeedData.sql                        # Car brands and sample data
└── README.md
```

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full)
- OpenAI API key and/or Google Gemini API key

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TurnSignalViolationTracker
```

### 2. Set Up the Database

Run the SQL scripts in order against your SQL Server:

```bash
# Using sqlcmd
sqlcmd -S localhost -i database/001_CreateDatabase.sql
sqlcmd -S localhost -i database/002_SeedData.sql
```

Or open the scripts in SQL Server Management Studio and execute them.

### 3. Configure the Application

Edit `src/TurnSignalViolationTracker.Web/appsettings.json`:

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=TurnSignalViolationTracker;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AI": {
    "Provider": "OpenAI",  // or "Gemini"
    "OpenAI": {
      "ApiKey": "your-openai-api-key-here",
      "Model": "gpt-4o",
      "WhisperModel": "whisper-1"
    },
    "Gemini": {
      "ApiKey": "your-gemini-api-key-here",
      "Model": "gemini-2.0-flash-exp"
    }
  }
}
```

### 4. Run the Application

```bash
cd src/TurnSignalViolationTracker.Web
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

### 5. Login

Default credentials:
- **Username**: `admin`
- **Password**: `admin123`

## Usage

### Logging a Violation

1. Navigate to **Log Violation**
2. Click **Start Recording** and describe the violation:
   - "I just saw a black Toyota Camry, 2022, change lanes without signaling on Main Street"
3. Click **Stop Recording** - AI will extract the data
4. Optionally click **Get Current GPS Location**
5. Review and edit the extracted data
6. Click **Log Violation**

### Viewing Statistics

The **Dashboard** shows an overview of all violations with:
- Summary cards (total, today, week, month)
- Bar charts for top offending brands
- Pie charts for violation types
- Color distribution
- Time patterns

The **Statistics** page provides more detailed analysis.

### Managing Violations

The **All Violations** page shows a searchable, sortable table of all logged violations with the ability to view details or delete entries.

## Violation Types

1. Left Turn Without Signal
2. Right Turn Without Signal
3. Lane Change Left Without Signal
4. Lane Change Right Without Signal
5. U-Turn Without Signal

## API Configuration

### OpenAI
- Uses Whisper for audio transcription
- Uses GPT-4 for data extraction
- Best for voice recording features

### Google Gemini
- Uses Gemini for text extraction
- Audio processing requires multimodal model
- Good alternative for text-based input

Switch providers by changing `AI.Provider` in appsettings.json.

## Development

### Building

```bash
dotnet build
```

### Running in Development

```bash
cd src/TurnSignalViolationTracker.Web
dotnet watch run
```

### Adding New Car Brands

Insert into the CarBrands table:

```sql
INSERT INTO CarBrands (Name, LogoUrl, IsActive) 
VALUES ('BrandName', 'https://url-to-logo.png', 1);
```

## Security Notes

- Change the default admin password in production
- Store API keys securely (use user secrets or environment variables)
- The pre-computed password hash in seed data is for demo purposes only
- Enable HTTPS in production

## Browser Requirements

- Modern browser with:
  - MediaRecorder API (for voice recording)
  - Geolocation API (for GPS)
  - WebRTC support

## License

MIT

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.
