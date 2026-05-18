# XMPP Protocol Notes

## Core

| Protocol | Purpose |
| --- | --- |
| RFC 6120 | XMPP streams, authentication, stanza routing and core protocol behavior. |
| RFC 6121 | Instant messaging, presence and roster behavior. |
| RFC 7622 | XMPP address/JID format. |
| RFC 7590 | TLS requirements and recommendations for XMPP. |
| RFC 7395 | XMPP over WebSocket for browser or WebView clients. |

## Chat

| XEP | Purpose |
| --- | --- |
| XEP-0030 | Service discovery. |
| XEP-0045 | Multi-user chat. |
| XEP-0085 | Chat state notifications such as typing. |
| XEP-0184 | Message delivery receipts. |
| XEP-0198 | Stream management and reconnect support. |
| XEP-0280 | Message carbons for multi-device sync. |
| XEP-0313 | Message archive management. |
| XEP-0352 | Client state indication. |
| XEP-0363 | HTTP file upload. |

## Real-Time Text

| XEP | Purpose |
| --- | --- |
| XEP-0301 | In-band real-time text. |

Real-time text is different from typing notifications. XEP-0085 says that someone is typing. XEP-0301 sends live text edits while the message is being written.

## Encryption

| XEP | Purpose |
| --- | --- |
| XEP-0384 | OMEMO end-to-end encryption. |
| XEP-0454 | OMEMO media sharing. |

## Audio And Video

| XEP | Purpose |
| --- | --- |
| XEP-0166 | Jingle session signaling. |
| XEP-0167 | Jingle RTP sessions for audio/video. |
| XEP-0176 | Jingle ICE-UDP transport. |
| XEP-0177 | Jingle raw UDP transport. |
| XEP-0343 | WebRTC data channels in Jingle. |

XMPP/Jingle handles signaling. WebRTC handles audio/video media transport.
