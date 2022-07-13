﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using WebApp.Domain;

namespace WebApp.Services;

public sealed class SmtpEmailService : IEmailService
{
    private MessageSenderInfo _sender = null!;

    public SmtpEmailService(IOptions<SmtpCredentials> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Sender = new()
        {
            Name = options.Value.Name,
            Host = options.Value.Host,
            Login = options.Value.Login,
            Address = options.Value.Address,
            Password = options.Value.Password,
            Port = options.Value.Port,
        };
    }

    public MessageSenderInfo Sender
    {
        get => _sender;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (string.IsNullOrEmpty(value.Address)
            || string.IsNullOrEmpty(value.Login)
            || string.IsNullOrEmpty(value.Name)
            || string.IsNullOrEmpty(value.Password)
            || string.IsNullOrEmpty(value.Host))
            {
                throw new ArgumentNullException("Not all sender properties has value!");
            }

            _sender = value;
        }
    }

    /// <summary>
    /// Sending a message to all of recipients.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void SendMessage(MailMessage message, params MessageRecipientInfo[] reсipients)
    {
        ArgumentNullException.ThrowIfNull(reсipients);
        ArgumentNullException.ThrowIfNull(message);

        if(reсipients.Length == 0)
        {
            throw new ArgumentException($"The number of recipients must be greater than 0.");
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(Sender.Name, Sender.Address));
        foreach (var recipient in reсipients)
        {
            mimeMessage.To.Add(new MailboxAddress(recipient.Name, recipient.Address));
        }
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
        {
            Text = message.Body,
        };

        using (var client = new SmtpClient())
        {
            client.Connect(Sender.Host, Sender.Port, false);
            client.Authenticate(Sender.Login, Sender.Password);
            client.Send(mimeMessage);
            client.Disconnect(true);
        }
    }
}
