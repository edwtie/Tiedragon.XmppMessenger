CREATE DATABASE IF NOT EXISTS teletyptel
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE teletyptel;

CREATE TABLE IF NOT EXISTS account_profiles (
  account_id VARCHAR(96) NOT NULL PRIMARY KEY,
  jid VARCHAR(255) NOT NULL,
  display_name VARCHAR(120) NOT NULL,
  password_secret TEXT NULL,
  remember_password TINYINT(1) NOT NULL DEFAULT 0,
  phone_number VARCHAR(64) NOT NULL DEFAULT '',
  provider_id VARCHAR(96) NOT NULL DEFAULT 'example-provider',
  accessibility_profile_id VARCHAR(96) NOT NULL DEFAULT 'default-live-text',
  preferred_language VARCHAR(16) NOT NULL DEFAULT 'nl',
  relay_websocket VARCHAR(255) NOT NULL DEFAULT 'ws://127.0.0.1:8787',
  xmpp_websocket VARCHAR(255) NOT NULL DEFAULT 'ws://127.0.0.1:8787',
  peer VARCHAR(255) NOT NULL DEFAULT 'relay@localhost',
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  UNIQUE KEY uq_account_profiles_jid (jid)
);
