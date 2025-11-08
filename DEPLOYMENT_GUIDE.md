# HTTPS Deployment Guide - Where to Do What

This guide explains **where** (desktop vs server) and **how** to set up HTTPS.

## Overview

- **Desktop (Your Computer)**: Code changes are already done ✅
- **Server (46.62.232.61)**: SSL certificates and Docker deployment

---

## Step 1: Deploy Code to Server (From Desktop)

You need to get your updated code to the server.

### Option A: Using Git (Recommended)
```bash
# On your desktop, commit and push changes
git add .
git commit -m "Add HTTPS support with nginx"
git push

# Then on server, pull the changes
ssh user@46.62.232.61
cd /path/to/your/project
git pull
```

### Option B: Using SCP/SFTP (If no Git)
```bash
# From your desktop, copy files to server
scp -r . user@46.62.232.61:/path/to/your/project/
# Or use FileZilla/WinSCP to copy files
```

### Option C: Manual Upload
- Upload the entire project folder to the server
- Make sure these files are updated:
  - `compose.yaml`
  - `Api/Program.cs`
  - `nginx/nginx.conf`

---

## Step 2: Set Up SSL Certificates (ON THE SERVER)

**⚠️ IMPORTANT: This must be done ON THE SERVER, not on your desktop!**

SSH into your server:
```bash
ssh user@46.62.232.61
cd /path/to/your/project
```

### Choose Your SSL Option:

#### Option 1: Self-Signed Certificate (Quick Testing)
```bash
# On the server
mkdir -p nginx/ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/key.pem \
  -out nginx/ssl/cert.pem \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=46.62.232.61"

chmod 644 nginx/ssl/cert.pem
chmod 600 nginx/ssl/key.pem
```

#### Option 2: Let's Encrypt (Production - Requires Domain)
```bash
# On the server
sudo apt-get update
sudo apt-get install certbot

# Get certificate (replace yourdomain.com with your domain)
sudo certbot certonly --standalone -d yourdomain.com

# Copy to project directory
mkdir -p nginx/ssl
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem nginx/ssl/key.pem
sudo chmod 644 nginx/ssl/cert.pem
sudo chmod 600 nginx/ssl/key.pem
```

#### Option 3: Use Existing Certificates
```bash
# On the server
mkdir -p nginx/ssl
# Copy your existing certificate files:
cp /path/to/your/certificate.crt nginx/ssl/cert.pem
cp /path/to/your/private.key nginx/ssl/key.pem
chmod 644 nginx/ssl/cert.pem
chmod 600 nginx/ssl/key.pem
```

---

## Step 3: Deploy with Docker Compose (ON THE SERVER)

**On the server**, navigate to your project directory and run:

```bash
# Stop existing containers
docker-compose down

# Rebuild and start with new configuration
docker-compose up -d

# Check if everything is running
docker-compose ps

# Check nginx logs if there are issues
docker-compose logs nginx
```

---

## Step 4: Verify HTTPS (From Desktop or Anywhere)

Test that HTTPS is working:

```bash
# From your desktop or any computer
curl -k https://46.62.232.61:18473/swagger/index.html
```

Or open in browser:
- `https://46.62.232.61:18473/swagger/index.html`

**Note**: If using self-signed certificate, browsers will show a security warning. Click "Advanced" → "Proceed anyway" for testing.

---

## Quick Reference: Where to Do What

| Task | Location | Command/Notes |
|------|----------|---------------|
| Code changes | ✅ Desktop (Already done) | Files are ready |
| Deploy code | Desktop → Server | `git push` or `scp` |
| Create SSL certs | **Server** | `openssl` or `certbot` |
| Place certs | **Server** | `nginx/ssl/` directory |
| Run Docker | **Server** | `docker-compose up -d` |
| Test HTTPS | Desktop/Browser | Visit URL |

---

## Troubleshooting

### "Certificate not found" error
- **Location**: Check on the server
- **Fix**: Ensure `nginx/ssl/cert.pem` and `nginx/ssl/key.pem` exist on the server

### "Port 18473 already in use"
- **Location**: Check on the server
- **Fix**: Stop old containers: `docker-compose down`

### "502 Bad Gateway"
- **Location**: Check on the server
- **Fix**: Check if API container is running: `docker-compose ps`

### Can't connect to server
- **Location**: Desktop
- **Fix**: Check SSH access and firewall settings

---

## Summary Checklist

- [ ] Code deployed to server (from desktop)
- [ ] SSL certificates created/placed in `nginx/ssl/` (on server)
- [ ] Docker containers restarted (on server)
- [ ] HTTPS accessible at `https://46.62.232.61:18473` (test from desktop)

