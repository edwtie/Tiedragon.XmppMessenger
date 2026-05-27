# Teletyptel 2.0 Project Page

Teletyptel 2.0 is an open XMPP messenger project for accessible real-time
communication. The Alpha 1 release focuses on live text: a conversation can
show text while it is being typed, then keep the final message in the chat
history.

The project is built around open protocols rather than a closed messaging
network. The current codebase contains a web client, PHP relay for local demos,
C# XMPP protocol core, STARTTLS local server, real-server smoke tool and English
and Dutch localization files.

## Download

- Source: https://github.com/edwtie/Tiedragon.XmppMessenger
- Release: https://github.com/edwtie/Tiedragon.XmppMessenger/releases

## Try Alpha 1

1. Install .NET 10 SDK and PHP 8.1 or newer.
2. Build and test the repository.
3. Start `php php/rtt-websocket-server.php`.
4. Open `php/public/chat.html` in two browser windows.
5. Connect both windows to `ws://127.0.0.1:8787`.

## Current Features

- Local browser chat UI.
- XEP-0301-style real-time text in the demo relay.
- RFC 7395 WebSocket framing experiment controls.
- Light/dark mode.
- English and Dutch UI language files.
- Account profile panel with local storage and optional MySQL API.
- Legacy smiley rendering.
- C# XMPP core for TLS/SASL/bind/roster/presence/chat and multiple XEP models.
- STARTTLS local server and real-server smoke tools.

## Standards Direction

- RFC 6120 XMPP Core
- RFC 6121 Instant Messaging and Presence
- RFC 7395 XMPP over WebSocket
- RFC 7590 TLS for XMPP
- RFC 7622 XMPP Address Format
- XEP-0030 Service Discovery
- XEP-0077 In-Band Registration
- XEP-0085 Chat State Notifications
- XEP-0184 Message Delivery Receipts
- XEP-0198 Stream Management
- XEP-0280 Message Carbons
- XEP-0301 In-Band Real Time Text
- XEP-0313 Message Archive Management

## Status

Alpha 1 is for evaluation. It is not yet a production service. The next public
milestones are a hosted demo instance, direct browser login to an XMPP
WebSocket endpoint, Android/iOS packaging experiments and stronger signed
language-package delivery.
