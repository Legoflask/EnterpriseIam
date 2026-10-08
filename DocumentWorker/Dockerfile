FROM python:3.12-slim
RUN apt-get update && apt-get install -y \
    unixodbc unixodbc-dev \
    curl \
    gnupg \
    apt-transport-https \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY DocumentWorker/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY DocumentWorker/app/ .
EXPOSE 8000
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
