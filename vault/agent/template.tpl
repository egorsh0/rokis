ADMIN__SECRET={{ with secret "kv/rokis/admin" }}{{ .Data.data.secret }}{{ end }}

POSTGRES_USER={{ with secret "kv/data/rokis/db" }}{{ .Data.data.user }}{{ end }}
POSTGRES_PASSWORD={{ with secret "kv/data/rokis/db" }}{{ .Data.data.password }}{{ end }}
POSTGRES_DB={{ with secret "kv/data/rokis/db" }}{{ .Data.data.dbname }}{{ end }}

JWT__SECRET={{ with secret "kv/data/rokis/jwt" }}{{ .Data.data.jwt }}{{ end }}

ConnectionStrings__rokisDb={{ with secret "kv/data/rokis/db_conn" }}{{ .Data.data.db_conn }}{{ end }}
ConnectionStrings__RadisConnection={{ with secret "kv/data/rokis/cache_conn" }}{{ .Data.data.cache_conn }}{{ end }}
ConnectionStrings__loki={{ with secret "kv/data/rokis/loki" }}{{ .Data.data.cache_conn }}{{ end }}

Smtp__Host={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.host }}{{ end }}
Smtp__UserName={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.user }}{{ end }}
Smtp__Password={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.pass }}{{ end }}
Smtp__FromEmail={{ with secret "kv/data/rokis/smtp" }}{{ .Data.data.from }}{{ end }}

DATA_SOURCE_NAME={{ with secret "kv/data/rokis/postgres_exporter" }}{{ .Data.data.conn }}{{ end }}
REDIS_ADDR={{ with secret "kv/data/rokis/redis_addr" }}{{ .Data.data.addr }}{{ end }}

GF_SECURITY_ADMIN_USER={{ with secret "kv/data/rokis/grafana" }}{{ .Data.data.user }}{{ end }}
GF_SECURITY_ADMIN_PASSWORD={{ with secret "kv/data/rokis/grafana" }}{{ .Data.data.pass }}{{ end }}