# 📬 Email Client Web API (ASP.NET Core)

This project is a custom-built ASP.NET Core Web API for sending and receiving emails using **raw SMTP and IMAP protocols**, fully implemented from scratch — without using any pre-built libraries like `MailKit` or `SmtpClient`.

---

## Features

- Send emails over **SMTP with STARTTLS**
- Read emails using **IMAP over SSL**
- Fully secure using `SslStream`
- Configurable via `appsettings.json`
- Works with **Mailtrap** and other SMTP servers for safe development and testing

---

## 🚀 Getting Started

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Internet connection
- (Optional) Free Mailtrap account — [https://mailtrap.io](https://mailtrap.io)

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
    "Port": "993",                       // Usually 993 for IMAP over SSL
    "Username": "{replace-placeholder}",
    "Password": "{replace-placeholder}"
  },

  "SmtpSettings": {
    "Host": "{replace-placeholder}", // Replace with your SMTP host
    "Port": "587",
    "Username": "{replace-placeholder}",
    "Password": "{replace-placeholder}"
  }
}
```

---

## Reference

- [RFC 5321 – Simple Mail Transfer Protocol (SMTP)](https://datatracker.ietf.org/doc/html/rfc5321)
- [How does SMTP work](https://mailtrap.io/blog/smtp/#:~:text=SMTP%20or%20Simple%20Mail%20Transfer,defining%20the%20rules%20of%20communication.)
