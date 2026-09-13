# Deploying

The whole product runs from `docker-compose.yml` on one Ubuntu 24.04 server:

| Service        | What it does                                                         |
|----------------|----------------------------------------------------------------------|
| `nginx`        | The only public entry point (ports 80/443). HTTPS, and routes `/` → frontend, `/api/` → backend, `/webhook` → telegram-bot |
| `certbot`      | Gets the Let's Encrypt certificate for `DOMAIN` and renews it        |
| `frontend`     | The Angular Mini App                                                 |
| `backend`      | ASP.NET API                                                          |
| `telegram-bot` | Receives Telegram updates; sets the bot's menu button to open the Mini App |
| `migrate`      | Applies database migrations, then exits. `backend` and `telegram-bot` start only after it succeeds |
| `postgres`     | PostgreSQL 17, data in the `postgres_data` volume                    |


## 1. Before the first deploy

**Everything must be committed and pushed.** The server builds from the repository alone — any
file that exists only on a development machine is missing there.

**DNS.** Create an `A` record for your domain pointing at the server's public IP, and wait until
`dig +short your-domain` returns it. Let's Encrypt cannot issue a certificate before this.

**Server.** Ubuntu 24.04 with at least 2 GB of RAM (the .NET and Angular builds need it; on a
1 GB server add swap first). Nothing else may listen on ports 80 or 443 — remove any
host-installed nginx or Apache.

Install Docker Engine and the Compose plugin:

```bash
sudo apt-get update
sudo apt-get install -y ca-certificates curl git
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker "$USER"   # log out and back in afterwards
```

Firewall — allow SSH first, or you lock yourself out:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

Docker starts on boot by default, and every service has `restart: unless-stopped`, so the stack
comes back by itself after a reboot.


## 2. Deploy

```bash
git clone <repository-url> doniyors
cd doniyors

cp .env.example .env
nano .env            # fill in every empty value; the comments explain each one

docker compose up -d --build
```

Generate the secrets (`POSTGRES_PASSWORD`, `JWT_KEY`, `TELEGRAM_WEBHOOK_SECRET`) with
`openssl rand -hex 32`. If a required value is missing, `docker compose` stops and names it.

The first build takes several minutes. Then check:

```bash
docker compose ps                  # everything "running"; migrate "exited (0)"
docker compose logs -f certbot     # "Certificate issued for ..." within a minute or two
docker compose logs telegram-bot   # "Webhook registered" and "Menu button now opens the Mini App"
```

Open `https://your-domain` in a browser: it must show a valid certificate. Then open the bot in
Telegram — the menu button next to the message box opens the Mini App. To confirm Telegram can
deliver updates:

```bash
curl -s "https://api.telegram.org/bot$(grep '^TELEGRAM_BOT_TOKEN=' .env | cut -d= -f2)/getWebhookInfo"
```

`url` should be `https://your-domain/webhook` with no `last_error_message`.

**The first administrator.** Everyone starts as an ordinary member. Send `/start` to the bot
once, then promote yourself in the database:

```bash
docker compose exec postgres psql -U postgres -d MainDb \
  -c "UPDATE \"Users\" SET \"TypeUserId\" = 1 WHERE \"TgUserId\" = <your Telegram id>;"
```

(`1` = SuperAdmin, `2` = Admin.) Send `/start` again to get the admin keyboard; reopen the Mini
App on desktop for the admin panel.


## 3. Updating

```bash
cd doniyors
git pull
docker compose up -d --build
```

Migrations run automatically before the new API and bot start. Clean up old images now and
then with `docker image prune -f`.


## 4. Backups

All data — including adventure images — lives in PostgreSQL.

```bash
# Back up
docker compose exec -T postgres pg_dump -U postgres -Fc MainDb > backup-$(date +%F).dump

# Restore (overwrites the current data)
docker compose exec -T postgres pg_restore -U postgres -d MainDb --clean --if-exists < backup-YYYY-MM-DD.dump
```

Copy backups off the server. `docker compose down -v` **deletes the database volume** — never
use `-v` on the production server.


## 5. Troubleshooting

| Symptom | Cause and fix |
|---|---|
| Browser warns about the certificate | Let's Encrypt has not issued it yet; nginx serves a temporary self-signed one until then. `docker compose logs certbot` says why — usually DNS not pointing at the server or port 80 blocked. It retries hourly; `docker compose restart certbot` retries now. |
| Rehearsed with `LETSENCRYPT_STAGING=1` and now want a real certificate | Set it to `0` in `.env` and run `docker compose up -d`. certbot notices the staging certificate and replaces it with a trusted one. |
| A member sees the wrong language | Everything reads `Users.LanguageCode`. Check it with `docker compose exec postgres psql -U postgres -d MainDb -c "SELECT \"TgUserId\", \"LanguageCode\" FROM \"Users\" WHERE \"TgUserId\" = <id>;"`. Empty means "not chosen yet": the bot asks on `/start`, and the Mini App shows English until then. |
| `backend` / `telegram-bot` never start | `docker compose logs migrate` — the migration failed, usually a wrong database password. |
| Changed `POSTGRES_PASSWORD` and nothing can connect | The password is set only when the volume is created. Change it inside the database too: `docker compose exec postgres psql -U postgres -c "ALTER USER postgres PASSWORD '<new>';"`, then `docker compose up -d`. |
| API refuses to start: `Jwt:Key must be at least 32 characters` | Fix `JWT_KEY` in `.env`, then `docker compose up -d`. |
| Bot does not answer | `getWebhookInfo` (above) shows Telegram's last delivery error; `docker compose logs telegram-bot` shows the bot's side. |

Logs are rotated automatically (5 × 10 MB per service): `docker compose logs -f <service>`.
