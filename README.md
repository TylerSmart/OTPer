# OTPer

A lightweight .NET 10 API that receives SMS/notification messages, extracts one-time passwords (OTP codes), and stores them in a SQLite database.

## Endpoints

| Method | Route    | Description                                           |
| ------ | -------- | ----------------------------------------------------- |
| POST   | /api/otp | Submit a message; the API extracts and stores the OTP |
| GET    | /api/otp | Retrieve recent OTP records with optional filtering   |

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

1. Open the solution in Visual Studio and press **F5** (the `http` profile), or from a terminal:

   ```bash
   cd OTPer.API
   dotnet run
   ```

2. The app starts at `http://localhost:5209`.
3. The Scalar API reference opens automatically at `http://localhost:5209/scalar/v1`.
4. The SQLite database is created automatically at `OTPer.API/data/otper.db` on first run.

---

## Deploying to TrueNAS Scale with Docker

The steps below walk through the full process: building the container image, pushing it to a registry, and running it on TrueNAS Scale.

### Step 1: Push your code to GitHub

Create a repository on GitHub and push:

```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/<your-username>/OTPer.git
git branch -M main
git push -u origin main
```

### Step 2: Build the Docker image

On any machine with Docker installed (your dev PC is fine):

```bash
# From the solution root (where the Dockerfile lives)
docker build -t otper-api .
```

This produces a Linux container image named `otper-api`.

> **First time with Docker?** Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) on Windows/Mac. It includes everything you need.

### Step 3: Push the image to a container registry

TrueNAS needs to pull the image from somewhere. The easiest free option is **Docker Hub**.

1. Create a free account at [hub.docker.com](https://hub.docker.com).
2. Log in from your terminal:

   ```bash
   docker login
   ```

3. Tag and push the image:

   ```bash
   docker tag otper-api <your-dockerhub-username>/otper-api:latest
   docker push <your-dockerhub-username>/otper-api:latest
   ```

> **Alternative:** If you prefer not to use Docker Hub, you can use GitHub Container Registry (ghcr.io). See [GitHub docs](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry).

### Step 4: Create a dataset for persistent storage on TrueNAS

The SQLite database needs to live on a TrueNAS dataset so it survives container restarts.

1. Open the TrueNAS Scale web UI.
2. Go to **Storage > Pools** and find your pool (e.g., `tank`).
3. Click the three-dot menu on an existing dataset (or the pool root) and choose **Add Dataset**.
4. Name it `otper-data` and click **Save**.

This creates a path like `/mnt/tank/otper-data` on the TrueNAS host.

### Step 5: Launch the container on TrueNAS Scale

#### Option A: TrueNAS Custom App (recommended)

1. In the TrueNAS UI, go to **Apps > Discover Apps > Custom App** (or **Apps > Launch Docker Image** on older versions).
2. Fill in the form:

   | Field            | Value                                 |
   | ---------------- | ------------------------------------- |
   | Application Name | `otper`                             |
   | Image Repository | `<your-dockerhub-username>/otper-api` |
   | Image Tag        | `latest`                            |

3. Under **Port Forwarding**, add an entry:

   | Container Port | Node Port | Protocol |
   | -------------- | --------- | -------- |
   | 7120           | 7120      | TCP      |

4. Under **Storage** (or **Host Path Volumes**), add a mount:

   | Host Path              | Mount Path  |
   | ---------------------- | ----------- |
   | `/mnt/tank/otper-data` | `/app/data` |

5. Click **Save** / **Install**.

#### Option B: SSH + docker run

If you prefer the command line, SSH into your TrueNAS box and run:

```bash
docker run -d \
  --name otper \
  --restart unless-stopped \
  -p 7120:7120 \
  -v /mnt/tank/otper-data:/app/data \
  <your-dockerhub-username>/otper-api:latest
```

### Step 6: Verify

From any machine on your network, test the API (replace `truenas-ip` with your TrueNAS IP address):

```bash
# Submit a message
curl -X POST http://<truenas-ip>:7120/api/otp \
  -H "Content-Type: application/json" \
  -d '{"message": "Your code is 483921", "sender": "TestBank"}'

# Retrieve recent codes
curl http://<truenas-ip>:7120/api/otp
```

You should get back the extracted OTP code `483921` in the response.

---

## Updating the App

When you make changes and want to redeploy:

```bash
# Rebuild and push
docker build -t <your-dockerhub-username>/otper-api:latest .
docker push <your-dockerhub-username>/otper-api:latest
```

Then on TrueNAS:
- **Custom App UI:** Go to the app page and click **Update** or redeploy.
- **CLI:** `docker pull <your-dockerhub-username>/otper-api:latest` then `docker restart otper`

Your SQLite data in `/mnt/tank/otper-data` is untouched during updates.

## Configuration

| Setting              | Default                    | Override via                                  |
| -------------------- | -------------------------- | --------------------------------------------- |
| SQLite database path | Data Source=data/otper.db  | `ConnectionStrings__DefaultConnection` env var |
| Listen port (Docker) | 7120                       | `ASPNETCORE_URLS` env var                    |
