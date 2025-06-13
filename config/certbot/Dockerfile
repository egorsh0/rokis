FROM alpine:latest

RUN apk update && apk add --no-cache certbot bash curl nginx-mod-http-lua ca-certificates tzdata && \
    mkdir -p /var/www/certbot

COPY renew-cron /etc/crontabs/root
CMD ["crond", "-f", "-l", "2"]