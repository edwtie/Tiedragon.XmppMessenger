# Teletyptel 2.0 Alpha 1 Release Notes

Release tag: `v0.1.0-alpha1`

Date: 2026-05-27

## Purpose

Alpha 1 is the first public evaluation release. It exists so XMPP reviewers,
accessibility testers and developers can run the current client direction
instead of only reading a concept description.

## What Can Be Evaluated

- Browser chat client with accessible real-time text behavior.
- Local PHP relay for two-window chat testing.
- RFC 7395 WebSocket framing experiment path.
- Light and dark UI mode.
- English and Dutch web localization.
- Local account profile storage and optional MySQL API.
- Legacy smiley rendering.
- C# XMPP core models and protocol helpers.
- STARTTLS local XMPP server with registration and chat smoke.
- Real-server smoke tool for TLS/hostname/XEP-0077/two-account chat checks.

## Quick Start

```powershell
dotnet build Tiedragon.XmppMessenger.slnx
dotnet run --project tests/Tiedragon.XmppMessenger.Tests
php php/rtt-websocket-server.php
```

Then open `php/public/chat.html` in two browser windows and connect to
`ws://127.0.0.1:8787`.

## Security Position

- XMPP client settings require TLS by default.
- The local XMPP server requires STARTTLS.
- Local self-signed smoke testing uses certificate SHA-256 pinning.
- The PHP relay is local development infrastructure and does not provide
  production authentication or transport security.

## Not Yet Included

- Public hosted demo instance.
- Android/iOS packages.
- Production XMPP WebSocket login from the browser UI.
- OMEMO encryption.
- Multi-user chat.
- File upload.
- Voice/video calling.

## Reviewer Notes

The prior software-directory submission was too early. Alpha 1 addresses that
feedback by adding a tagged release target, repeatable evaluation path, user
documentation, release notes, project-page copy and a clearer readiness
checklist.
