#    # syntax=docker/dockerfile:1

#     FROM python:3.11-slim AS base

#     ENV PYTHONDONTWRITEBYTECODE=1 \
#         PYTHONUNBUFFERED=1 \
#         PIP_NO_CACHE_DIR=1 \
#         PIP_DISABLE_PIP_VERSION_CHECK=1

#     WORKDIR /app

#     # Install system deps (none needed now, keep layer for future)
#     RUN apt-get update && apt-get install -y --no-install-recommends \
#         ca-certificates \
#         && rm -rf /var/lib/apt/lists/*

#     # Install Python deps first for better caching
#     COPY requirements.txt ./
#     RUN pip install --no-cache-dir -r requirements.txt

#     # Copy application
#     COPY app.py ./
#     COPY gunicorn.config.py ./

#     EXPOSE 8000

#     ENV GUNICORN_BIND=0.0.0.0:8000 \
#         GUNICORN_WSGI_APP=app:app

# CMD ["gunicorn", "-c", "gunicorn.config.py", "app:app"]


FROM python:3.12.11-slim AS base

# Set the working directory
WORKDIR /app

# Set environment variables
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    HOST=0.0.0.0

# Copy the requirements file
COPY requirements.txt .

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy application files
COPY app.py .
COPY gunicorn_config.py .

# Expose port 80
EXPOSE 80

# Set the entry point to run the application 
ENTRYPOINT ["opentelemetry-instrument", \
    "--logs_exporter", "otlp", \
    "--traces_exporter", "otlp", \
    "--metrics_exporter", "otlp", \
    "gunicorn", "--config", "gunicorn_config.py", "app:app"]

CMD []

