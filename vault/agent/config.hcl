auto_auth {
  method "approle" {
    config = {
      role_id_file_path = "/vault/agent/role_id"
      secret_id_file_path = "/vault/agent/secret_id"
    }
  }

  sink "file" {
    config = {
      path = "/vault/agent/sink-token"
    }
  }
}