# HTTPS Setup Summary

Your application has been configured to use HTTPS on port 18473 through an nginx reverse proxy.

## What Changed

1. **Added nginx reverse proxy** - Handles SSL/TLS termination
2. **Updated Docker Compose** - Added nginx service, API now only exposed internally
3. **Updated Program.cs** - Added forwarded headers support for reverse proxy
4. **Created nginx configuration** - SSL-enabled reverse proxy setup

## Architecture

```
Internet (HTTPS:18473) → nginx (SSL termination) → API (HTTP:8080 internal)
```

- **Port 18473**: Public HTTPS endpoint (unchanged, as requested)
- **Port 8080**: Internal API endpoint (not exposed to host)
- **Other ports**: Unchanged (db:25432, redis:26379)

## Quick Start

**⚠️ IMPORTANT: See `DEPLOYMENT_GUIDE.md` for detailed step-by-step instructions on where to perform each action (desktop vs server).**

1. **Deploy code to server** (from desktop)
2. **Set up SSL certificates on the server** (see `nginx/SSL_SETUP.md` for details)
3. **Start services on the server:**
   ```bash
   docker-compose up -d
   ```
4. **Verify HTTPS is working** (from desktop/browser):
   - Visit: `https://46.62.232.61:18473/swagger/index.html`

## Important Notes

- **SSL certificates are required** before nginx will start successfully
- The API container is no longer directly exposed on port 18473
- All traffic now goes through nginx for SSL termination
- Other projects on the server are unaffected (only port 18473 is used)

## Troubleshooting

- **nginx won't start**: Check that SSL certificates exist in `nginx/ssl/`
- **502 Bad Gateway**: API container might not be running, check with `docker-compose ps`
- **Certificate errors**: Follow the SSL setup guide in `nginx/SSL_SETUP.md`

## Files Modified

- `compose.yaml` - Added nginx service, changed API port mapping
- `Api/Program.cs` - Added forwarded headers middleware
- `nginx/nginx.conf` - New nginx configuration file
- `nginx/SSL_SETUP.md` - SSL certificate setup instructions

