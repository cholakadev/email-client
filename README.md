# 📬 Email Client Web API (ASP.NET Core)

This project is a custom-built ASP.NET Core Web API for sending and receiving emails using **raw SMTP and IMAP protocols**, fully implemented from scratch — without using any pre-built libraries like `MailKit` or `SmtpClient`.

---

## Features

- Send emails over **SMTP with STARTTLS**
- Read emails using **IMAP over SSL**
- Fully secure using `SslStream`
- Configurable via `appsettings.json`
- Works with **Mailtrap**, **Gmail** and other SMTP servers for safe development and testing

---

## 🚀 Getting Started

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Internet connection for work with external SMTP server

---

## ⚙️ Configuration

### `appsettings.json`

Paste this into your root `appsettings.json` file and **replace with your own credentials** as needed:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ImapSettings": {
    "Host": "{replace-placeholder}",     // Replace with your IMAP host
    "Port": "993",                       // Port 993 for IMAP over SSL/TLS
    "Username": "{replace-placeholder}",
    "Password": "{replace-placeholder}"
  },

  "SmtpSettings": {
    "Host": "{replace-placeholder}", // Replace with your SMTP host
    "Port": "587",                   // Port 587 (default)
    "Username": "{replace-placeholder}",
    "Password": "{replace-placeholder}"
  }
}
```

---

## Reference

- [RFC 5321 – Simple Mail Transfer Protocol (SMTP)](https://datatracker.ietf.org/doc/html/rfc5321)
- [SMTP: The Protocol Behind Email Delivery – What Is It and How Does It Work](https://mailtrap.io/blog/smtp/)

- [RFC 3501 - INTERNET MESSAGE ACCESS PROTOCOL](https://datatracker.ietf.org/doc/html/rfc3501)
- [A Deep Dive Into IMAP: What Is It and How Does it Work?](https://mailtrap.io/blog/imap/)
