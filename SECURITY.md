# Security Policy

## Supported Versions

DudeBot.NET is committed to the security of its users. We currently provide security updates for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 6.x     | :white_check_mark: |
| < 6.0   | :x:                |

We recommend all users stay on the latest release to ensure they have the latest security patches and features.

## Reporting a Vulnerability

If you discover a security vulnerability within DudeBot.NET, please do not disclose it publicly. Reporting vulnerabilities helps us maintain a safe environment for all users.

### How to Report

Please use the **[GitHub Security Advisories](https://github.com/NexusRisen/DudeBot.NET/security/advisories)** feature to report vulnerabilities privately.

1.  Navigate to the "Security" tab of the repository.
2.  Click on "Advisories".
3.  Click "Report a vulnerability".

### What to Expect

*   **Acknowledgement**: You can expect an initial response within 48-72 hours.
*   **Investigation**: We will investigate the report and may ask for additional information or reproduction steps.
*   **Resolution**: If the vulnerability is confirmed, we will work on a fix and coordinate a disclosure timeline.

## Security Best Practices for Users

*   **Protect your Configuration**: Never share your `config.json` file publicly, as it may contain sensitive information like Discord bot tokens or Twitch credentials.
*   **Use Official Releases**: Only download DudeBot.NET from the official [GitHub Releases](https://github.com/NexusRisen/DudeBot.NET/releases) page.
*   **Keep Dependencies Updated**: If you are building from source, ensure your .NET environment and dependencies are up to date.
