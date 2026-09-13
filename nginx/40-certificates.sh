#!/bin/sh
#
# Points nginx at a TLS certificate before it starts, and keeps it current.
#
# nginx cannot start without a certificate, but Let's Encrypt cannot issue one
# until nginx is up to answer its challenge. So nginx starts on a temporary
# self-signed certificate, and a background loop switches to the real one as
# soon as the certbot container has it — and reloads nginx again whenever the
# certificate is renewed.

set -eu

: "${DOMAIN:?DOMAIN is not set}"

LIVE="/etc/letsencrypt/live/$DOMAIN"
SELF_SIGNED=/etc/nginx/ssl/self-signed
ACTIVE=/etc/nginx/ssl/active

mkdir -p "$SELF_SIGNED"

if [ ! -f "$SELF_SIGNED/fullchain.pem" ]; then
    openssl req -x509 -nodes -newkey rsa:2048 -days 30 \
        -subj "/CN=$DOMAIN" \
        -keyout "$SELF_SIGNED/privkey.pem" \
        -out "$SELF_SIGNED/fullchain.pem" 2>/dev/null
fi

# The directory nginx should read the certificate from right now.
current_target() {
    if [ -f "$LIVE/fullchain.pem" ] && [ -f "$LIVE/privkey.pem" ]; then
        echo "$LIVE"
    else
        echo "$SELF_SIGNED"
    fi
}

# Changes when the certificate does: a switch of directory, or a renewal,
# which repoints the live symlink at a newer file.
fingerprint() {
    echo "$(readlink "$ACTIVE") $(stat -L -c %Y "$ACTIVE/fullchain.pem" 2>/dev/null)"
}

ln -sfn "$(current_target)" "$ACTIVE"

echo "$0: using the certificate in $(readlink "$ACTIVE")"

(
    last="$(fingerprint)"

    while :; do
        sleep 60

        ln -sfn "$(current_target)" "$ACTIVE"

        now="$(fingerprint)"

        if [ "$now" != "$last" ]; then
            echo "$0: certificate changed, now $(readlink "$ACTIVE"); reloading nginx"
            nginx -s reload || true
            last="$now"
        fi
    done
) &
