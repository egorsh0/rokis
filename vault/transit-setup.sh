#!/bin/bash
export VAULT_ADDR="https://localhost:8200"
export VAULT_CACERT="./certs/vault.crt"

vault operator init -key-shares=5 -key-threshold=3 -format=json > init.json
jq -r '.root_token' init.json > root.token
jq -r '.unseal_keys_b64[]' init.json > keys.txt

vault login $(cat root.token)

vault secrets enable transit
vault write -f transit/keys/autounseal-key
vault policy write transit-policy -<<EOF
path "transit/decrypt/autounseal-key" { capabilities = ["update"] }
path "transit/encrypt/autounseal-key" { capabilities = ["update"] }
EOF

vault token create -policy=transit-policy -wrap-ttl=60s -format=json > wrap.json
jq -r '.wrap_info.token' wrap.json > transit.token
