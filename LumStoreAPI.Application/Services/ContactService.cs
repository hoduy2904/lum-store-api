using LumStoreAPI.Application.DTOs.ContactDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Contact;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.Services;

internal class ContactService(
    IEmailService emailService,
    IEventLogService eventLogService,
    ISettingKeyValueRepository settingKeyValueRepository,
    IContactMessageRepository contactMessageRepository
) : IContactService
{
    private const string FALLBACK_CONTACT_EMAIL = "hello@lumnails.com";
    private const string FALLBACK_SMTP_FROM = "noreply@lumnails.com";

    public async Task<APIResponseBase> SendMessageAsync(SendMessageRequest request)
    {
        var contactEmailSetting = await settingKeyValueRepository.GetSettingKeyAsync("CONTACT_EMAIL");
        var smtpFromSetting = await settingKeyValueRepository.GetSettingKeyAsync("SMTP_FROM");

        string adminEmail = contactEmailSetting?.SettingValue ?? FALLBACK_CONTACT_EMAIL;
        string fromAddress = smtpFromSetting?.SettingValue ?? FALLBACK_SMTP_FROM;

        await contactMessageRepository.InsertAsync(new ContactMessage
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
        });

        // Notification email to store admin
        await emailService.SendEmailAsync(new EmailMessage
        {
            EmailFrom = fromAddress,
            EmailTo = [adminEmail],
            EmailSubject = $"[Contact Form] {request.Subject}",
            EmailBody = $"""
                <h2>New Contact Form Submission</h2>
                <p><strong>Name:</strong> {request.Name}</p>
                <p><strong>Email:</strong> {request.Email}</p>
                <p><strong>Subject:</strong> {request.Subject}</p>
                <p><strong>Message:</strong></p>
                <p>{request.Message}</p>
                """
        });

        // Confirmation email to sender
        await emailService.SendEmailAsync(new EmailMessage
        {
            EmailFrom = fromAddress,
            EmailTo = [request.Email],
            EmailSubject = "We received your message",
            EmailBody = $"""
                <p>Hi {request.Name},</p>
                <p>Thank you for reaching out! We have received your message and will get back to you within 1–2 business days.</p>
                <p>Here is a copy of your message:</p>
                <blockquote>
                    <p><strong>Subject:</strong> {request.Subject}</p>
                    <p>{request.Message}</p>
                </blockquote>
                <p>Best regards,<br/>The LUM Nails Team</p>
                """
        });

        await eventLogService.LogInformation(
            source: "ContactController",
            code: "CONTACT_FORM_SUBMITTED",
            name: "Contact Form Submitted",
            description: $"Contact form submitted by {request.Name} <{request.Email}> — Subject: {request.Subject}"
        );

        return APIResponseBase.Success(["Your message has been sent. We will get back to you shortly."]);
    }
}
