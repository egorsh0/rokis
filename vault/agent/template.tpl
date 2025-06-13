JWT={{ with secret "kv/data/rokis/jwt" }}{{ .Data.data.jwt }}{{ end }}
DB_CONN={{ with secret "kv/data/rokis/db_conn" }}{{ .Data.data.db_conn }}{{ end }}
CACHE_CONN={{ with secret "kv/data/rokis/cache_conn" }}{{ .Data.data.cache_conn }}{{ end }}

SMTP_HOST={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.host }}{{ end }}
SMTP_USER={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.user }}{{ end }}
SMTP_PASS={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.pass }}{{ end }}
SMTP_FROM={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.from }}{{ end }}

LOKI={{ with secret "kv/data/rokis/loki" }}{{ .Data.data.url }}{{ end }}
POSTGRES_EXPLORER={{ with secret "kv/data/rokis/postgres_exporter" }}{{ .Data.data.conn }}{{ end }}
REDIS_ADDR={{ with secret "kv/data/rokis/redis_addr" }}{{ .Data.data.addr }}{{ end }}

GF_USER={{ with secret "kv/data/rokis/grafana" }}{{ .Data.data.user }}{{ end }}
GF_PASS={{ with secret "kv/data/rokis/grafana" }}{{ .Data.data.pass }}{{ end }}
