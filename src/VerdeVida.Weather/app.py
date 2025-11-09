import os
import logging
import flask
import sys
import re
import requests
from datetime import datetime
from opentelemetry import trace
from opentelemetry._logs import set_logger_provider
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.instrumentation.flask import FlaskInstrumentor

app = flask.Flask(__name__)

# Configure OpenTelemetry with service name
# Use environment variable if set, otherwise use default
service_name = os.environ.get("OTEL_SERVICE_NAME", "Python Weather API")
resource = Resource.create(attributes={
    "service.name": service_name,
    "service.version": "1.0.0"
})

# Configure OpenTelemetry Tracing
tracer_provider = TracerProvider(resource=resource)
trace.set_tracer_provider(tracer_provider)

# Get OTLP endpoint from environment variable (set by Aspire)
otlp_endpoint = os.environ.get('OTEL_EXPORTER_OTLP_ENDPOINT')
if otlp_endpoint:
    otlpExporter = OTLPSpanExporter(endpoint=otlp_endpoint, insecure=True)
else:
    otlpExporter = OTLPSpanExporter()

processor = BatchSpanProcessor(otlpExporter)
tracer_provider.add_span_processor(processor)

# Configure OpenTelemetry Logging
logger_provider = LoggerProvider(resource=resource)
set_logger_provider(logger_provider)

if otlp_endpoint:
    log_exporter = OTLPLogExporter(endpoint=otlp_endpoint, insecure=True)
else:
    log_exporter = OTLPLogExporter()

logger_provider.add_log_record_processor(BatchLogRecordProcessor(log_exporter))

# Set up structured logging with OpenTelemetry
handler = LoggingHandler(level=logging.INFO, logger_provider=logger_provider)

# Configure root logger
root_logger = logging.getLogger()
root_logger.setLevel(logging.INFO)
root_logger.addHandler(handler)

# Get logger for this module
logger = logging.getLogger(__name__)

if otlp_endpoint:
    logger.info("Configuring OTLP exporter", extra={
        "otlp_endpoint": otlp_endpoint,
        "service": "Python Weather API"
    })
else:
    logger.warning("OTEL_EXPORTER_OTLP_ENDPOINT not set, using default localhost:4317", extra={
        "service": "Python Weather API"
    })

FlaskInstrumentor().instrument_app(app)

@app.route('/', methods=['GET'])
def hello_world():
    logger.info("Request received", extra={
        "route": "/",
        "method": "GET",
        "service": "Python Weather API"
    })
    return 'Hello, World!'

@app.route('/python', methods=['GET'])
def python():
    logger.info("Request received", extra={
        "route": "/python",
        "method": "GET",
        "service": "Python Weather API"
    })
    return f"Python version: {sys.version}"

@app.route('/forecast/<zip_code>', methods=['GET'])
def forecast(zip_code):
    """
    Get 10-day weather forecast for a given zip code.

    Args:
        zip_code: 5-digit US zip code

    Returns:
        JSON response with 10-day forecast in Fahrenheit
    """
    tracer = trace.get_tracer(__name__)
    with tracer.start_as_current_span("forecast") as span:
        span.set_attribute("zip_code", zip_code)
        logger.info("Forecast request received", extra={
            "zip_code": zip_code,
            "route": "/forecast/<zip_code>",
            "method": "GET",
            "service": "Python Weather API"
        })

        # Validate zip code format (5 digits)
        if not re.match(r'^\d{5}$', zip_code):
            logger.warning("Invalid zip code format", extra={
                "zip_code": zip_code,
                "route": "/forecast/<zip_code>",
                "method": "GET",
                "service": "Python Weather API",
                "error_type": "validation_error"
            })
            return flask.jsonify({
                'error': 'Invalid zip code format. Please provide a 5-digit US zip code.'
            }), 400

        # Get API key from environment
        api_key = os.environ.get('OPENWEATHERMAP_API_KEY')
        if not api_key:
            logger.error("Weather API key not configured", extra={
                "zip_code": zip_code,
                "route": "/forecast/<zip_code>",
                "method": "GET",
                "service": "Python Weather API",
                "error_type": "configuration_error"
            })
            return flask.jsonify({
                'error': 'Weather API key is not configured. Please set WEATHER_API_KEY environment variable.'
            }), 500

        # OpenWeatherMap API endpoint for daily forecast
        # Using the 2.5 API with zip code and country code (US)
        base_url = "https://api.openweathermap.org/data/2.5/forecast"
        params = {
            'zip': f'{zip_code},us',
            'appid': api_key,
            'units': 'imperial'  # This gives us Fahrenheit directly
        }

        logger.info("Calling OpenWeatherMap API", extra={
            "zip_code": zip_code,
            "route": "/forecast/<zip_code>",
            "method": "GET",
            "service": "Python Weather API",
            "external_api": "OpenWeatherMap"
        })

        try:
            response = requests.get(base_url, params=params, timeout=10)
            response.raise_for_status()

            data = response.json()

            # OpenWeatherMap returns 3-hourly forecasts, we need to aggregate to daily
            # and limit to 10 days
            daily_forecasts = _aggregate_to_daily_forecast(data, limit=10)
            location = data.get('city', {}).get('name', 'Unknown')
            country = data.get('city', {}).get('country', 'US')

            logger.info("Successfully retrieved forecast", extra={
                "zip_code": zip_code,
                "location": location,
                "country": country,
                "forecast_days": len(daily_forecasts),
                "route": "/forecast/<zip_code>",
                "method": "GET",
                "service": "Python Weather API",
                "external_api": "OpenWeatherMap"
            })

            return flask.jsonify({
                'zip_code': zip_code,
                'location': location,
                'country': country,
                'forecast': daily_forecasts
            }), 200

        except requests.exceptions.Timeout:
            logger.error("Timeout calling OpenWeatherMap API", extra={
                "zip_code": zip_code,
                "route": "/forecast/<zip_code>",
                "method": "GET",
                "service": "Python Weather API",
                "external_api": "OpenWeatherMap",
                "error_type": "timeout",
                "timeout_seconds": 10
            })
            return flask.jsonify({
                'error': 'Weather service request timed out. Please try again later.'
            }), 503

        except requests.exceptions.HTTPError as e:
            status_code = e.response.status_code if e.response else 0
            if status_code == 401:
                logger.error("Invalid API key for OpenWeatherMap", extra={
                    "zip_code": zip_code,
                    "route": "/forecast/<zip_code>",
                    "method": "GET",
                    "service": "Python Weather API",
                    "external_api": "OpenWeatherMap",
                    "error_type": "authentication_error",
                    "http_status_code": status_code
                })
                return flask.jsonify({
                    'error': 'Invalid weather API key configuration.'
                }), 500
            elif status_code == 404:
                logger.warning("Zip code not found in OpenWeatherMap", extra={
                    "zip_code": zip_code,
                    "route": "/forecast/<zip_code>",
                    "method": "GET",
                    "service": "Python Weather API",
                    "external_api": "OpenWeatherMap",
                    "error_type": "not_found",
                    "http_status_code": status_code
                })
                return flask.jsonify({
                    'error': f'Weather data not found for zip code: {zip_code}'
                }), 404
            else:
                logger.error("HTTP error calling OpenWeatherMap API", extra={
                    "zip_code": zip_code,
                    "route": "/forecast/<zip_code>",
                    "method": "GET",
                    "service": "Python Weather API",
                    "external_api": "OpenWeatherMap",
                    "error_type": "http_error",
                    "http_status_code": status_code
                })
                return flask.jsonify({
                    'error': 'Weather service is currently unavailable. Please try again later.'
                }), 503

        except requests.exceptions.RequestException as e:
            logger.error("Error calling OpenWeatherMap API", extra={
                "zip_code": zip_code,
                "route": "/forecast/<zip_code>",
                "method": "GET",
                "service": "Python Weather API",
                "external_api": "OpenWeatherMap",
                "error_type": "request_exception",
                "error_message": str(e)
            })
            return flask.jsonify({
                'error': 'Unable to connect to weather service. Please try again later.'
            }), 503

def _aggregate_to_daily_forecast(data, limit=10):
    """
    Aggregate 3-hourly forecasts into daily forecasts.

    Args:
        data: OpenWeatherMap API response data
        limit: Maximum number of days to return

    Returns:
        List of daily forecast dictionaries
    """
    if 'list' not in data:
        return []

    daily_data = {}

    for item in data['list']:
        # Parse date and get just the date part (without time)
        dt = datetime.fromtimestamp(item['dt'])
        date_key = dt.strftime('%Y-%m-%d')

        if date_key not in daily_data:
            daily_data[date_key] = {
                'date': date_key,
                'temperatures': [],
                'descriptions': [],
                'humidity': [],
                'wind_speed': [],
                'conditions': []
            }

        # Collect data for aggregation
        main = item.get('main', {})
        weather = item.get('weather', [{}])[0]
        wind = item.get('wind', {})

        daily_data[date_key]['temperatures'].append(main.get('temp'))
        daily_data[date_key]['descriptions'].append(weather.get('description', ''))
        daily_data[date_key]['humidity'].append(main.get('humidity'))
        daily_data[date_key]['wind_speed'].append(wind.get('speed', 0))
        daily_data[date_key]['conditions'].append(weather.get('main', ''))

    # Aggregate to daily values
    daily_forecasts = []
    for date_key in sorted(daily_data.keys())[:limit]:
        day_data = daily_data[date_key]

        # Calculate min/max temperature
        temps = [t for t in day_data['temperatures'] if t is not None]
        if not temps:
            continue

        # Get most common description and condition
        most_common_desc = max(set(day_data['descriptions']), key=day_data['descriptions'].count) if day_data['descriptions'] else 'Unknown'
        most_common_condition = max(set(day_data['conditions']), key=day_data['conditions'].count) if day_data['conditions'] else 'Unknown'

        # Average humidity and wind speed
        avg_humidity = sum(day_data['humidity']) / len(day_data['humidity']) if day_data['humidity'] else 0
        avg_wind_speed = sum(day_data['wind_speed']) / len(day_data['wind_speed']) if day_data['wind_speed'] else 0

        daily_forecasts.append({
            'date': date_key,
            'temperature_high': round(max(temps), 1),
            'temperature_low': round(min(temps), 1),
            'temperature_avg': round(sum(temps) / len(temps), 1),
            'description': most_common_desc,
            'condition': most_common_condition,
            'humidity': round(avg_humidity, 1),
            'wind_speed': round(avg_wind_speed, 1)
        })

    return daily_forecasts


if __name__ == '__main__':
    print("Environment Variables:")
    for key, value in os.environ.items():
        print(f"{key}={value}")
    port = int(os.environ.get('PORT', 8111))
    debug = bool(os.environ.get('DEBUG', False))
    host = os.environ.get('HOST', '127.0.0.1')
    app.run(port=port, debug=debug, host=host)

