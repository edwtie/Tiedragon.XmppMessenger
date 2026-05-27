# XSF Software Directory Preparation

This document tracks the Teletyptel 2.0 entry for the XSF/xmpp.org software
directory. The first submission was intentionally held back after review
because the project looked like a concept instead of software that visitors
could evaluate.

Do not resubmit until every item in the readiness checklist is complete.

## Review Feedback To Address

- A usable release must exist.
- Visitors need either a public demo instance or clear demo instructions.
- The repository needs user-facing documentation.
- The README must describe what works now, not only planned platform support.
- The project should show enough development history and maintenance signals.

## Target Entry

Future entry with DOAP, after the public Teletyptel project page and DOAP file
are published:

```json
{
  "name": "Teletyptel 2.0",
  "doap": "https://www.tiedragon.com/teletyptel/doap.rdf",
  "platforms": [],
  "url": null,
  "categories": [
    "client"
  ]
}
```

Candidate public entry without DOAP:

```json
{
  "name": "Teletyptel 2.0",
  "doap": null,
  "platforms": [
    "Web",
    "Android",
    "iOS"
  ],
  "url": "https://github.com/edwtie/Tiedragon.XmppMessenger",
  "categories": [
    "client"
  ]
}
```

The DOAP entry is preferred later because xmpp.org can source the project URL
and platform metadata from the DOAP file. The initial entry uses the public
GitHub repository so the project can be reviewed before the final project page
is online.

## Public Description

Teletyptel 2.0 is a web-based XMPP client for accessible realtime
communication. Alpha 1 includes a local web chat demo, XEP-0301-style real-time
text, a C# XMPP core, STARTTLS local server smoke testing and real-server smoke
tools. Android and iOS packaging are planned after the web client stabilizes.

## XMPP Scope

Implemented or partially implemented standards for Alpha 1:

- RFC 6120: XMPP Core
- RFC 6121: Instant Messaging and Presence
- RFC 7395: XMPP over WebSocket
- RFC 7590: TLS for XMPP
- RFC 7622: XMPP Address Format
- XEP-0030: Service Discovery
- XEP-0085: Chat State Notifications
- XEP-0184: Message Delivery Receipts
- XEP-0198: Stream Management
- XEP-0280: Message Carbons
- XEP-0301: In-Band Real Time Text
- XEP-0313: Message Archive Management

Planned after Alpha 1:

- XEP-0045: Multi-User Chat
- XEP-0363: HTTP File Upload
- XEP-0384: OMEMO Encryption
- XEP-0166/0167/0176: Jingle call signaling and ICE transport

## Submit Checklist

- [x] Public Teletyptel 2.0 project page copy exists in `docs/PROJECT_PAGE.md`.
- [x] Public source repository exists.
- [x] Public README explains current Alpha 1 evaluation path.
- [x] Public license is present.
- [x] User guide exists.
- [x] Getting-started demo instructions exist.
- [x] Release notes exist.
- [x] Build, test and PHP relay validation commands are documented and pass.
- [ ] DOAP file is published at the final URL.
- [x] Initial XSF entry uses the public GitHub repository URL.
- [ ] Future XSF entry uses the final DOAP URL.
- [ ] `lint_software_list.py software.json` passes in an xmpp.org fork.
- [x] Real-server smoke test has passed with two accounts.
- [x] Alpha 1 build is runnable from public instructions.
- [x] GitHub release `v0.1.0-alpha1` exists.
- [ ] Public hosted demo instance exists.

## Resubmission Position

Resubmit only after `v0.1.0-alpha1` is published and either:

- a public hosted demo is live, or
- the XSF maintainers accept the local Alpha 1 demo path as sufficient for an
  early listing.

## XSF Process Notes

The xmpp.org software directory stores entries in `src/data/software.json`.
The XSF README says new entries are added manually to the top-level JSON array,
then validated with `lint_software_list.py`. The JSON file must be saved as
UTF-8 without a byte order mark.
