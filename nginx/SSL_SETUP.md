# SSL Certificate Setup Guide

This guide explains how to set up SSL certificates for HTTPS on port 18473.

## Option 1: Using Let's Encrypt (Recommended for Production)

### Prerequisites
- Domain name pointing to your server (46.62.232.61)
- Port 80 and 18473 open in firewall

### Steps

1. **Install Certbot on your server:**
   ```bash
   sudo apt-get update
   sudo apt-get install certbot
   ```

2. **Obtain SSL certificate:**
   ```bash
   sudo certbot certonly --standalone -d yourdomain.com
   ```
   Replace `yourdomain.com` with your actual domain name.

3. **Copy certificates to nginx/ssl directory:**
   ```bash
   sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem ./nginx/ssl/cert.pem
   sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem ./nginx/ssl/key.pem
   sudo chmod 644 ./nginx/ssl/cert.pem
   sudo chmod 600 ./nginx/ssl/key.pem
   ```

4. **Set up auto-renewal:**
   ```bash
   sudo certbot renew --dry-run
   ```
   Add a cron job to renew certificates:
   ```bash
   sudo crontab -e
   # Add this line:
   0 0 * * * certbot renew --quiet && docker-compose restart nginx
   ```

## Option 2: Using Self-Signed Certificate (For Testing Only)

**Warning:** Self-signed certificates will show security warnings in browsers. Use only for testing.

1. **Create SSL directory:**
   ```bash
   mkdir -p nginx/ssl
   ```

2. **Generate self-signed certificate:**
   ```bash
   openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
     -keyout nginx/ssl/key.pem \
     -out nginx/ssl/cert.pem \
     -subj "/C=US/ST=State/L=City/O=Organization/CN=46.62.232.61"
   ```

3. **Set proper permissions:**
   ```bash
   chmod 644 nginx/ssl/cert.pem
   chmod 600 nginx/ssl/key.pem
   ```

## Option 3: Using Existing Certificates

If you already have SSL certificates:

1. **Create SSL directory:**
   ```bash
   mkdir -p nginx/ssl
   ```

2. **Copy your certificates:**
   ```bash
   cp /path/to/your/certificate.crt nginx/ssl/cert.pem
   cp /path/to/your/private.key nginx/ssl/key.pem
   ```

3. **Set proper permissions:**
   ```bash
   chmod 644 nginx/ssl/cert.pem
   chmod 600 nginx/ssl/key.pem
   ```

## After Setting Up Certificates

1. **Restart the nginx service:**
   ```bash
   docker-compose restart nginx
   ```

2. **Verify HTTPS is working:**
   ```bash
   curl -k https://46.62.232.61:18473/swagger/index.html
   ```
   Or visit `https://46.62.232.61:18473/swagger/index.html` in your browser.

## Troubleshooting

- **Certificate not found error:** Make sure certificates are in `nginx/ssl/` directory with correct names (`cert.pem` and `key.pem`)
- **Permission denied:** Check file permissions (cert.pem: 644, key.pem: 600)
- **Port already in use:** Make sure no other service is using port 18473
- **Connection refused:** Check firewall settings and ensure port 18473 is open

