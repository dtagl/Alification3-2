# Commands to Run on Server

You're currently on the server at `/var/www/room-booking/`. Follow these steps:

## Step 1: Get Updated Files to Server

You need to copy the updated files from your desktop to the server. Choose one method:

### Option A: Using SCP (from your desktop PowerShell)
```powershell
# From your desktop, run this in PowerShell:
scp -i C:\Users\denta\.ssh\id_rsa compose.yaml root@46.62.232.61:/var/www/room-booking/
scp -i C:\Users\denta\.ssh\id_rsa -r nginx root@46.62.232.61:/var/www/room-booking/
scp -i C:\Users\denta\.ssh\id_rsa Api/Program.cs root@46.62.232.61:/var/www/room-booking/Api/
```

### Option B: Using Git (if you have a repo)
```bash
# On server, if you have git:
cd /var/www/room-booking
git pull
```

### Option C: Manual copy via SFTP
Use FileZilla, WinSCP, or similar to copy:
- `compose.yaml`
- `nginx/` folder (with nginx.conf)
- `Api/Program.cs`

---

## Step 2: Create SSL Certificates (ON SERVER)

**Run these commands on the server:**

```bash
# Make sure you're in the project directory
cd /var/www/room-booking

# Create SSL directory
mkdir -p nginx/ssl

# Generate self-signed certificate (for testing)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/key.pem \
  -out nginx/ssl/cert.pem \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=46.62.232.61"

# Set proper permissions
chmod 644 nginx/ssl/cert.pem
chmod 600 nginx/ssl/key.pem

# Verify files exist
ls -la nginx/ssl/
```

---

## Step 3: Stop Old Containers and Start New Setup (ON SERVER)

```bash
# Stop all containers
docker compose down

# Start all services (including nginx)
docker compose up -d

# Check status
docker compose ps

# Check nginx logs if there are issues
docker compose logs nginx

# Check if nginx is running
docker ps | grep nginx
```

---

## Step 4: Verify HTTPS (from desktop or server)

```bash
# Test from server:
curl -k https://localhost:18473/swagger/index.html

# Or test from your desktop browser:
# https://46.62.232.61:18473/swagger/index.html
```

---

## Troubleshooting

If nginx fails to start:
```bash
# Check nginx logs
docker compose logs nginx

# Verify certificates exist
ls -la nginx/ssl/

# Check nginx config syntax (if nginx container is running)
docker compose exec nginx nginx -t
```

If you see "port already in use":
```bash
# Find what's using port 18473
sudo netstat -tulpn | grep 18473

# Or
sudo lsof -i :18473
```

