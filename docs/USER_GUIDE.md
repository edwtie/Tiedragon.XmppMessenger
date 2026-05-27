# User Guide

Teletyptel 2.0 Alpha 1 is a local evaluation build for accessible real-time
text chat. It is intended for testers and developers, not for production use.

## Opening The Client

Open:

```text
php/public/chat.html
```

Use two browser windows to test a conversation. Both windows can connect to the
same local relay.

## Connecting

1. Start `php php/rtt-websocket-server.php`.
2. Leave the relay URL as `ws://127.0.0.1:8787`.
3. Choose a display name.
4. Press **Connect**.

The status in the title bar changes when the relay is connected.

## Sending Messages

- Type in the message box.
- With **Live RTT** enabled, the other side sees your text while you type.
- Press Enter to send a final message.
- Use Shift+Enter to insert a line break.

## Modes

- **Relay**: local PHP relay mode for Alpha 1 chat testing.
- **RFC 7395**: WebSocket framing test mode for XMPP-over-WebSocket experiments.

Relay mode is the normal Alpha 1 demo path.

## Light And Dark Mode

Use the theme button in the top bar. The selected mode is saved in browser
storage.

## Languages

The web client currently includes English and Dutch UI language files:

```text
php/public/lang/eng.lng
php/public/lang/ned.lng
```

These are development language files. Signed LngPdk packages are the stricter
production direction.

## Smilies

The smiley toggle converts supported legacy text codes, such as `:)` and `:D`,
to local SVG/GIF smiley assets.

## Account Profile

The account panel lets you set:

- display name
- JID
- peer/contact
- phone field
- provider id
- language
- optional password memory setting

The profile is saved locally. When the MySQL API is configured, it can also be
stored through `php/public/api/account.php`.

## Privacy Notes

Alpha 1 is a local demo. Do not use real passwords in the local relay demo. The
password field is for testing account-profile behavior and later XMPP login
flows.

## Current Limitations

- No hosted public service yet.
- No production authentication on the PHP relay.
- No OMEMO encryption yet.
- No group chat or file upload yet.
- No packaged Android/iOS app yet.
