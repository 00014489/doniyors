FROM nginx:1.30-alpine

# Makes the temporary self-signed certificate nginx starts with until Let's
# Encrypt has issued the real one.
RUN apk add --no-cache openssl

COPY nginx.conf /etc/nginx/nginx.conf

# Rendered into /etc/nginx/conf.d at start by the image's envsubst step, which
# fills in ${DOMAIN}.
COPY templates/ /etc/nginx/templates/

# The base image's entrypoint runs executable scripts from this directory, in
# name order, before starting nginx.
COPY 40-certificates.sh /docker-entrypoint.d/40-certificates.sh
RUN chmod +x /docker-entrypoint.d/40-certificates.sh
