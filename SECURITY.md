# Security policy

## Reporting a vulnerability

Please report suspected vulnerabilities privately through GitHub's
[private vulnerability reporting](https://github.com/code-assurance-initiative/estate-quellbrook-orders/security/advisories/new)
for this repository. Do not open a public issue.

Include what you found, how to reproduce it, and the impact you expect. We acknowledge reports within three working
days and aim to publish a fix or a mitigation within 30 days of confirming the issue. We credit reporters in the
advisory unless they ask us not to.

## Supported versions

Only the latest release is supported.

## Scope

In scope: the order service and the build, release and deployment configuration in this repository. Out of scope:
the identity provider, the message broker and the platform's shared infrastructure (report those to the platform
team through the same channel), and denial of service through request volume (the service is reachable only through
the gateway, which applies rate limits).
