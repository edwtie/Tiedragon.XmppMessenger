# Tiedragon XMPP Messenger

Tiedragon XMPP Messenger is a planned chat application based on open XMPP standards.

The project goal is a modern messenger with:

- one-to-one chat
- contact list / roster
- presence status
- message history
- delivery receipts
- real-time text
- group chat
- file and image sharing
- audio calling
- video calling

## Protocol Direction

Core protocols:

- RFC 6120 - XMPP Core
- RFC 6121 - Instant Messaging and Presence
- RFC 7622 - XMPP Address Format
- RFC 7590 - TLS for XMPP

Important XMPP extensions:

- XEP-0030 - Service Discovery
- XEP-0045 - Multi-User Chat
- XEP-0085 - Chat State Notifications
- XEP-0184 - Message Delivery Receipts
- XEP-0198 - Stream Management
- XEP-0280 - Message Carbons
- XEP-0301 - In-Band Real Time Text
- XEP-0313 - Message Archive Management
- XEP-0363 - HTTP File Upload
- XEP-0384 - OMEMO Encryption
- XEP-0166 - Jingle
- XEP-0167 - Jingle RTP Sessions
- XEP-0176 - Jingle ICE-UDP Transport

Audio and video will use XMPP/Jingle for signaling and WebRTC for media transport.

## Server Direction

Candidate server stack:

- Prosody or ejabberd for XMPP
- coturn for STUN/TURN
- HTTP upload module for files
- MAM support for history
- PubSub/PEP support for OMEMO

## Release Lines

- Alpha: basic XMPP login and text chat
- Beta: tester-ready messenger with RTT, group chat and file sharing
- Release: stable messenger

Calling and video are planned after the core messenger is stable.
