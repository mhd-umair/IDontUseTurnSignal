# Turn Signal Tracker

A .NET 8 Blazor Server app for tracking turn signal violations with **AI automation**.

## AI Automation Features

### Voice & Text Logging
- **Voice**: Tap the microphone in the Admin Portal to describe a violation. AI extracts car make, model, color, year, violation type, and location.
- **Text**: Type a description and click "Process with AI" for the same automated extraction.
- **Auto-submit**: Toggle on to save violations automatically after AI processing — no manual review step.

### API Automation
Programmatic violation logging via HTTP:

**Authenticated endpoint** (requires admin login cookie):
```http
POST /api/violations/automate
Content-Type: application/json

{
  "description": "Black BMW X5 turned left on Main Street without signal",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "autoSave": true
}
```

**Webhook endpoint** (for external integrations):
```http
POST /api/violations/webhook
X-Automation-Key: your-api-key
Content-Type: application/json

{
  "description": "Silver Tesla Model 3 cut me off at 5th Ave",
  "autoSave": true
}
```

### AI Dashboard Insights
The public dashboard automatically generates trend summaries from violation data using your configured AI provider (or rule-based fallback when using Mock mode).

## Configuration

Edit `appsettings.json`:

```json
{
  "AISettings": {
    "Provider": "OpenAI",
    "OpenAI": {
      "ApiKey": "sk-...",
      "Model": "gpt-4-turbo"
    },
    "Gemini": {
      "ApiKey": "..."
    }
  },
  "Automation": {
    "AutoSubmitAfterAiProcessing": false,
    "EnableAiInsights": true,
    "InsightsRefreshMinutes": 30,
    "ApiKey": "your-webhook-secret"
  }
}
```

Set `Provider` to `"Mock"` for offline demo mode (regex-based extraction, no API key needed).

## Getting Started

```bash
cd TurnSignalTracker
dotnet run
```

Default admin login: `admin` / `admin123`

## Tech Stack

- Blazor Server (.NET 8)
- Dapper + SQL Server
- OpenAI / Google Gemini AI providers
- Web Speech API for voice input
