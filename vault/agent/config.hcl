vault {
  address = "https://vault:8200"
  tls_ca_cert = "/vault/certs/vault.crt"
}

auto_auth {
  method "token" {
    config {
      token_file_path = "/vault/agent/token"
    }
  }
  sink "file" {
    config {
      path = "/vault/agent/unwrap-token"
    }
  }
}

template {
  source = "/etc/vault/template.tpl"
  destination = "/vault/agent/secrets.env"
  perms = "0644"
}
