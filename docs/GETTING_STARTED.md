# Getting Started

This guide lets a reviewer evaluate Teletyptel 2.0 Alpha 1 on a local machine.
It does not require a public XMPP account.

## Requirements

- .NET 10 SDK
- PHP 8.1 or newer
- A modern browser
- Optional: MySQL or MariaDB for account profile storage

## Build And Test

```powershell
dotnet build Tiedragon.XmppMessenger.slnx
dotnet run --project tests/Tiedragon.XmppMessenger.Tests
```

Expected test ending:

```text
All RTT tests passed.
```

## Run The Web Chat Demo

Start the PHP relay:

```powershell
php php/rtt-websocket-server.php
```

Open the full Alpha 1 web client in two browser windows:

```text
php/public/chat.html
```

Connect both windows to:

```text
ws://127.0.0.1:8787
```

Type in one window. The other window should show live RTT text while typing and
then a final message bubble after Enter.

## Run The STARTTLS Fake XMPP Server

Start the local protocol harness:

```powershell
dotnet run --project tools/Tiedragon.XmppMessenger.FakeServer -- `
  --listen 127.0.0.1 `
  --port 55222 `
  --domain localhost
```

Copy the printed `Certificate SHA-256` value and run:

```powershell
dotnet run --project tools/Tiedragon.XmppMessenger.RealServerSmoke -- `
  --host 127.0.0.1 `
  --port 55222 `
  --account1 edward@localhost/desktop `
  --password1 secret `
  --account2 anna@localhost/desktop `
  --password2 secret `
  --register `
  --cert-sha256 <printed fingerprint>
```

Expected result:

```text
PASS TLS certificate accepted for configured host.
PASS Registration accepted for edward@localhost.
PASS Registration accepted for anna@localhost.
PASS Two-account chat message delivered.
```

## Optional MySQL Account Profile

Create the schema:

```sql
SOURCE php/schema.sql;
```

Copy:

```text
php/config.example.php -> php/config.php
```

Then edit `php/config.php` for your local database. The web client still works
without MySQL by using browser storage.

## What This Alpha Proves

- Live real-time text UX in a browser client.
- Repeatable local relay demo.
- XMPP core models and negotiation code in C#.
- Mandatory TLS path with local STARTTLS server smoke.
- A first public release shape that can be tested by other developers.
