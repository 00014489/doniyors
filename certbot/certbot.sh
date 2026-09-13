#!/bin/sh
#
# Obtains the Let's Encrypt certificate for $DOMAIN, then keeps renewing it.
#
# Runs in the certbot container. nginx serves /.well-known/acme-challenge/
# from the shared webroot volume, and picks the certificate up from the shared
# /etc/letsencrypt volume by itself (nginx/40-certificates.sh).

set -eu

: "${DOMAIN:?DOMAIN is not set}"
: "${LETSENCRYPT_EMAIL:?LETSENCRYPT_EMAIL is not set}"

WEBROOT=/var/www/certbot
LIVE="/etc/letsencrypt/live/$DOMAIN/fullchain.pem"

# Let's Encrypt allows 5 failed validations per hostname per hour, so after a
# failure (DNS not pointed yet, port 80 closed) wait a full hour.
RETRY_SECONDS=3600
RENEW_CHECK_SECONDS=43200

staging=""
if [ "${LETSENCRYPT_STAGING:-0}" = "1" ]; then
    staging="--staging"
fi

# Sleeps in the background so "docker compose stop" does not have to wait out
# the timeout before killing the container.
trap 'exit 0' TERM INT
pause() {
    sleep "$1" &
    wait $!
}

# Asking Let's Encrypt before nginx answers would only waste a validation.
mkdir -p "$WEBROOT/.well-known/acme-challenge"
echo ok > "$WEBROOT/.well-known/acme-challenge/probe"

until python3 -c "import urllib.request; urllib.request.urlopen('http://nginx/.well-known/acme-challenge/probe', timeout=5)" 2>/dev/null; do
    echo "Waiting for nginx to serve the ACME challenge directory..."
    pause 5
done

rm -f "$WEBROOT/.well-known/acme-challenge/probe"

# A staging certificate left over from a rehearsal is replaced as soon as
# staging is switched off. Otherwise it would keep renewing as staging, and no
# browser — nor Telegram — ever trusts it. nginx falls back to its self-signed
# certificate for the minute this takes.
RENEWAL_CONF="/etc/letsencrypt/renewal/$DOMAIN.conf"

if [ "${LETSENCRYPT_STAGING:-0}" != "1" ] \
    && [ -f "$RENEWAL_CONF" ] \
    && grep -q "acme-staging" "$RENEWAL_CONF"; then

    echo "Replacing the staging certificate for $DOMAIN with a trusted one."
    certbot delete --cert-name "$DOMAIN" --non-interactive || true
fi

while :; do
    if [ -f "$LIVE" ]; then
        # Renews only certificates within 30 days of expiry; otherwise a no-op.
        certbot renew --webroot -w "$WEBROOT" --non-interactive \
            || echo "Certificate renewal failed; retrying in 12 hours."

        pause "$RENEW_CHECK_SECONDS"

    elif certbot certonly \
            --webroot -w "$WEBROOT" \
            -d "$DOMAIN" \
            --email "$LETSENCRYPT_EMAIL" \
            --agree-tos --no-eff-email --non-interactive \
            $staging; then

        echo "Certificate issued for $DOMAIN. nginx switches to it within a minute."

    else
        echo "Could not obtain a certificate for $DOMAIN."
        echo "Check that its DNS A record points at this server and that port 80 is reachable from the internet."
        echo "Retrying in 1 hour."

        pause "$RETRY_SECONDS"
    fi
done
