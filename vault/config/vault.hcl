ui = true
disable_mlock = false

listener "tcp" {
  address = "0.0.0.0:8200"
  tls_cert_file = "/vault/certs/vault.crt"
  tls_key_file = "/vault/certs/vault.key"
}

storage "raft" {
  path    = "/vault/data"
  node_id = "vault-1"
}

seal "transit" {
  address = "https://127.0.0.1:8200"
  key_name = "autounseal-key"
}

api_addr = "https://vault.example.com:8200"
cluster_addr = "https://0.0.0.0:8201"
