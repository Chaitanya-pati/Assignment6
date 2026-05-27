using DbService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Assignment6.Services;

public class EmailSettings
{
    public string FromAddress { get; set; }
    public string DisplayName { get; set; }
    public string Username    { get; set; }
    public string Password    { get; set; }
    public string SmtpHost    { get; set; } = "smtp.gmail.com";
    public int    SmtpPort    { get; set; } = 587;
    public string AdminEmail  { get; set; }
}

public interface IEmailService
{
    Task SendBookingApprovedAsync(Booking booking);
    Task SendInvoiceEmailAsync(Booking booking, string invoiceHtml, byte[] pdfBytes = null);
    Task SendNewWebsiteBookingToAdminAsync(Booking booking);
    Task<(bool success, string message)> SendTestEmailAsync(string toAddress);
    Task SendBookingRequestConfirmationAsync(Booking booking);
    Task SendGuestDetailsUpdatedAsync(Booking booking);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _cfg;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _cfg    = options.Value;
        _logger = logger;
    }

    private async Task SendAsync(string to, string subject, string htmlBody, byte[] pdfAttachment = null, string pdfFileName = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("[Email] Skipped: recipient address is empty. Subject: {Subject}", subject);
            return;
        }

        var smtpUser = !string.IsNullOrWhiteSpace(_cfg.Username) ? _cfg.Username : _cfg.FromAddress;

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(_cfg.Password))
        {
            _logger.LogError("[Email] Cannot send — Email Username or Password is not configured.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_cfg.DisplayName, _cfg.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyPart = new TextPart("html") { Text = htmlBody };

            if (pdfAttachment != null && pdfAttachment.Length > 0)
            {
                var multipart = new MimeKit.Multipart("mixed");
                multipart.Add(bodyPart);

                var attachment = new MimePart("application", "pdf")
                {
                    Content            = new MimeContent(new MemoryStream(pdfAttachment)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName           = pdfFileName ?? "Invoice.pdf"
                };
                multipart.Add(attachment);
                message.Body = multipart;
            }
            else
            {
                message.Body = bodyPart;
            }

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.SmtpHost, _cfg.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, _cfg.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("[Email] Sent to {To} | Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Failed to send to {To} | Subject: {Subject} | {ErrorType}: {ErrorMessage}",
                to, subject, ex.GetType().Name, ex.Message);
        }
    }

    public async Task SendBookingApprovedAsync(Booking booking)
    {
        var subject = $"Booking Confirmed – {booking.Home?.Name ?? "Your Stay"} | {_cfg.DisplayName}";
        var body    = BookingApprovedHtml(booking);

        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(booking.CustomerEmail)) recipients.Add(booking.CustomerEmail);
        if (!string.IsNullOrWhiteSpace(booking.BookedByEmail)) recipients.Add(booking.BookedByEmail);

        foreach (var r in recipients)
            await SendAsync(r, subject, body);
    }

    public async Task SendInvoiceEmailAsync(Booking booking, string invoiceHtml, byte[] pdfBytes = null)
    {
        var subject  = $"Your Invoice – Booking #{booking.Id} | {_cfg.DisplayName}";
        var body     = InvoiceEmailWrapper(invoiceHtml, booking);
        var fileName = $"Invoice_Booking_{booking.Id}.pdf";

        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(booking.CustomerEmail)) recipients.Add(booking.CustomerEmail);
        if (!string.IsNullOrWhiteSpace(booking.BookedByEmail)) recipients.Add(booking.BookedByEmail);

        foreach (var r in recipients)
            await SendAsync(r, subject, body, pdfBytes, fileName);
    }

    public async Task<(bool success, string message)> SendTestEmailAsync(string toAddress)
    {
        if (string.IsNullOrWhiteSpace(toAddress))
            toAddress = _cfg.AdminEmail;

        if (string.IsNullOrWhiteSpace(toAddress))
            return (false, "No recipient address provided and AdminEmail is not configured.");

        var smtpUser = !string.IsNullOrWhiteSpace(_cfg.Username) ? _cfg.Username : _cfg.FromAddress;

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(_cfg.Password))
            return (false, "Email Username or Password is not configured in EmailSettings.");

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_cfg.DisplayName, _cfg.FromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = $"[Test Email] {_cfg.DisplayName} – Email Configuration Working";
            message.Body = new TextPart("html")
            {
                Text = $@"<p>This is a test email sent from <strong>{_cfg.DisplayName}</strong>.</p>
                          <p>If you received this, your email configuration is working correctly.</p>
                          <p>Sent at: {DateTime.Now:dd MMM yyyy HH:mm:ss}</p>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.SmtpHost, _cfg.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, _cfg.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("[Email] Test email sent to {To}", toAddress);
            return (true, $"Test email sent successfully to {toAddress}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Test email failed: {ErrorType}: {ErrorMessage}", ex.GetType().Name, ex.Message);
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    public async Task SendNewWebsiteBookingToAdminAsync(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(_cfg.AdminEmail)) return;
        var subject = $"New Booking Request – {booking.Home?.Name ?? "Property"} | {_cfg.DisplayName}";
        await SendAsync(_cfg.AdminEmail, subject, AdminNotificationHtml(booking));
    }

    public async Task SendBookingRequestConfirmationAsync(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.CustomerEmail)) return;
        var subject = $"Booking Request Received – #{booking.Id} | {_cfg.DisplayName}";
        await SendAsync(booking.CustomerEmail, subject, BookingRequestConfirmationHtml(booking));
    }

    public async Task SendGuestDetailsUpdatedAsync(Booking booking)
    {
        if (!string.IsNullOrWhiteSpace(booking.CustomerEmail))
        {
            var subject = $"Booking Updated – #{booking.Id} | {_cfg.DisplayName}";
            await SendAsync(booking.CustomerEmail, subject, GuestDetailsUpdatedHtml(booking));
        }
        if (!string.IsNullOrWhiteSpace(_cfg.AdminEmail))
        {
            var subject = $"Guest Updated Booking #{booking.Id} | {_cfg.DisplayName}";
            await SendAsync(_cfg.AdminEmail, subject, AdminGuestUpdateNotificationHtml(booking));
        }
    }

    private string BookingApprovedHtml(Booking b)
    {
        var nights  = (b.BookingDateTo - b.BookingDateFrom).Days;
        var advance = b.AdvancePrice ?? 0;
        var balance = b.Price - advance;

        return $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:32px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0'
       style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;'>

  <tr><td style='background:linear-gradient(135deg,#1a3a6b 0%,#2563a8 100%);padding:32px 40px;text-align:center;'>
    <div style='font-size:26px;font-weight:700;color:#fff;letter-spacing:1px;'>{_cfg.DisplayName}</div>
    <div style='font-size:12px;color:#a8c4e8;margin-top:6px;letter-spacing:2px;text-transform:uppercase;'>Booking Confirmation</div>
  </td></tr>

  <tr><td style='background:#f0fdf4;padding:18px 40px;border-bottom:2px solid #bbf7d0;text-align:center;'>
    <span style='background:#16a34a;color:#fff;font-size:14px;font-weight:700;padding:9px 26px;border-radius:50px;display:inline-block;'>
      &#10003;&nbsp; Booking Confirmed
    </span>
  </td></tr>

  <tr><td style='padding:32px 40px;'>
    <p style='font-size:15px;color:#374151;margin:0 0 8px;'>Dear <strong>{b.CustomerName}</strong>,</p>
    <p style='font-size:14px;color:#6b7a8d;margin:0 0 28px;'>Your homestay booking has been <strong>confirmed</strong>. Here are your details:</p>

    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
    <tr>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;vertical-align:top;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:8px;'>Property</div>
        <div style='font-size:15px;font-weight:700;color:#1a3a6b;'>{b.Home?.Name ?? "N/A"}</div>
        <div style='font-size:11px;color:#6b7a8d;margin-top:3px;'>{b.Home?.Location ?? ""}</div>
      </td>
      <td width='2%'></td>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;vertical-align:top;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:8px;'>Stay</div>
        <div style='font-size:12px;color:#374151;'><strong>Check-in:</strong> {b.BookingDateFrom:dd MMM yyyy}</div>
        <div style='font-size:12px;color:#374151;margin-top:4px;'><strong>Check-out:</strong> {b.BookingDateTo:dd MMM yyyy}</div>
        <div style='font-size:13px;font-weight:700;color:#2563a8;margin-top:7px;'>{nights} {(nights == 1 ? "night" : "nights")}</div>
      </td>
    </tr>
    </table>

    <table width='100%' cellpadding='0' cellspacing='0'
           style='margin-bottom:20px;border:1.5px solid #dbe7ff;border-radius:8px;overflow:hidden;'>
      <tr><td style='background:#f8faff;padding:10px 16px;border-bottom:1px solid #dbe7ff;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Guest Details</div>
      </td></tr>
      <tr><td style='padding:12px 16px;'>
        <table width='100%' cellpadding='4' cellspacing='0' style='font-size:13px;'>
          <tr><td style='color:#6b7a8d;width:40%;'>Guest Name</td><td style='font-weight:600;color:#1a3a6b;'>{b.CustomerName}</td></tr>
          {Row("Email",        b.CustomerEmail)}
          {Row("Phone",        b.CustomerPhone)}
          {Row("Total Guests", b.GuestNumbers.ToString())}
          {(string.IsNullOrEmpty(b.BookedByName) ? "" : Row("Booked By",
              b.BookedByName + (string.IsNullOrEmpty(b.BookedByPhone) ? "" : " &middot; " + b.BookedByPhone)))}
        </table>
      </td></tr>
    </table>

    <table width='100%' cellpadding='0' cellspacing='0'
           style='margin-bottom:24px;border:1.5px solid #dbe7ff;border-radius:8px;overflow:hidden;'>
      <tr><td style='background:#f8faff;padding:10px 16px;border-bottom:1px solid #dbe7ff;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Payment Summary</div>
      </td></tr>
      <tr><td style='padding:12px 16px;'>
        <table width='100%' cellpadding='5' cellspacing='0' style='font-size:13px;'>
          <tr><td style='color:#6b7a8d;width:50%;'>Total Amount</td><td style='font-size:14px;font-weight:700;color:#1a3a6b;'>&#8377;{b.Price:N0}</td></tr>
          {(advance > 0 ? $"<tr><td style='color:#6b7a8d;'>Advance Paid</td><td style='font-weight:600;color:#16a34a;'>&#8377;{advance:N0}</td></tr>" : "")}
          {(balance > 0
              ? $"<tr><td style='color:#6b7a8d;'>Balance Due</td><td style='font-size:14px;font-weight:700;color:#d97706;'>&#8377;{balance:N0}</td></tr>"
              : "<tr><td colspan='2' style='font-weight:700;color:#16a34a;padding-top:6px;'>&#10003; Fully Paid</td></tr>")}
        </table>
      </td></tr>
    </table>

    <p style='font-size:13px;color:#6b7a8d;margin:0;'>We look forward to welcoming you. Feel free to reach out if you have any questions!</p>
  </td></tr>

  <tr><td style='background:#1a3a6b;padding:18px 40px;text-align:center;'>
    <div style='font-size:12px;color:#a8c4e8;'>{_cfg.DisplayName}</div>
    <div style='font-size:11px;color:#5a7aaa;margin-top:4px;'>This is an automated message. Please do not reply directly.</div>
  </td></tr>
</table>
</td></tr></table>
</body></html>";
    }

    private string BookingRequestConfirmationHtml(Booking b) => $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:32px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0'
       style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;'>

  <tr><td style='background:linear-gradient(135deg,#1a3a6b 0%,#2563a8 100%);padding:32px 40px;text-align:center;'>
    <div style='font-size:26px;font-weight:700;color:#fff;letter-spacing:1px;'>{_cfg.DisplayName}</div>
    <div style='font-size:12px;color:#a8c4e8;margin-top:6px;letter-spacing:2px;text-transform:uppercase;'>Booking Request Received</div>
  </td></tr>

  <tr><td style='background:#fffbeb;padding:18px 40px;border-bottom:2px solid #fde68a;text-align:center;'>
    <span style='color:#92400e;font-size:14px;font-weight:700;'>&#9203;&nbsp; Your request is under review. We will contact you shortly!</span>
  </td></tr>

  <tr><td style='padding:32px 40px;'>
    <p style='font-size:15px;color:#374151;margin:0 0 8px;'>Dear <strong>{b.CustomerName}</strong>,</p>
    <p style='font-size:14px;color:#6b7a8d;margin:0 0 24px;'>
      Thank you for choosing <strong>{_cfg.DisplayName}</strong>. We have received your booking request
      and our team will review and confirm it soon.
    </p>

    <div style='background:#f0f7ff;border:2px solid #3b82f6;border-radius:10px;padding:20px 24px;margin-bottom:24px;text-align:center;'>
      <div style='font-size:12px;font-weight:700;color:#1e40af;text-transform:uppercase;letter-spacing:1px;margin-bottom:14px;'>
        Your Login Details for Editing Booking
      </div>
      <table width='100%' cellpadding='0' cellspacing='0'>
        <tr>
          <td width='49%' style='background:#fff;border:1.5px solid #bfdbfe;border-radius:8px;padding:14px;text-align:center;'>
            <div style='font-size:10px;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:6px;'>Booking ID</div>
            <div style='font-size:28px;font-weight:700;color:#1a3a6b;'>#{b.Id}</div>
          </td>
          <td width='2%'></td>
          <td width='49%' style='background:#fff;border:1.5px solid #bfdbfe;border-radius:8px;padding:14px;text-align:center;'>
            <div style='font-size:10px;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:6px;'>Registered Phone</div>
            <div style='font-size:18px;font-weight:700;color:#1a3a6b;'>{b.CustomerPhone}</div>
          </td>
        </tr>
      </table>
      <p style='font-size:12px;color:#6b7a8d;margin:12px 0 0;'>
        Use these details to update your guest count or ID proof anytime before check-in.
      </p>
    </div>

    <table width='100%' cellpadding='0' cellspacing='0'
           style='margin-bottom:20px;border:1.5px solid #dbe7ff;border-radius:8px;overflow:hidden;'>
      <tr><td style='background:#f8faff;padding:10px 16px;border-bottom:1px solid #dbe7ff;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Booking Summary</div>
      </td></tr>
      <tr><td style='padding:14px 16px;'>
        <table width='100%' cellpadding='4' cellspacing='0' style='font-size:13px;'>
          <tr><td style='color:#6b7a8d;width:40%;'>Property</td><td style='font-weight:600;color:#1a3a6b;'>{b.Home?.Name ?? "N/A"}</td></tr>
          <tr><td style='color:#6b7a8d;'>Check-in</td><td style='font-weight:600;color:#1a3a6b;'>{b.BookingDateFrom:dd MMM yyyy}</td></tr>
          <tr><td style='color:#6b7a8d;'>Check-out</td><td style='font-weight:600;color:#1a3a6b;'>{b.BookingDateTo:dd MMM yyyy}</td></tr>
          <tr><td style='color:#6b7a8d;'>Total Guests</td><td style='font-weight:600;color:#1a3a6b;'>{b.GuestNumbers}</td></tr>
          <tr><td style='color:#6b7a8d;'>Estimated Total</td><td style='font-size:14px;font-weight:700;color:#2563a8;'>&#8377;{b.Price:N0}</td></tr>
        </table>
      </td></tr>
    </table>

    <p style='font-size:13px;color:#6b7a8d;margin:0;'>
      If you have any questions, please contact us directly. We look forward to welcoming you!
    </p>
  </td></tr>

  <tr><td style='background:#1a3a6b;padding:18px 40px;text-align:center;'>
    <div style='font-size:12px;color:#a8c4e8;'>{_cfg.DisplayName}</div>
    <div style='font-size:11px;color:#5a7aaa;margin-top:4px;'>This is an automated message. Please do not reply directly.</div>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

    private string GuestDetailsUpdatedHtml(Booking b) => $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:32px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0'
       style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;'>

  <tr><td style='background:linear-gradient(135deg,#1a3a6b 0%,#2563a8 100%);padding:28px 40px;text-align:center;'>
    <div style='font-size:24px;font-weight:700;color:#fff;'>{_cfg.DisplayName}</div>
    <div style='font-size:11px;color:#a8c4e8;margin-top:6px;letter-spacing:2px;text-transform:uppercase;'>Booking Details Updated</div>
  </td></tr>

  <tr><td style='background:#f0fdf4;padding:14px 40px;border-bottom:2px solid #bbf7d0;text-align:center;'>
    <span style='color:#15803d;font-size:14px;font-weight:700;'>&#10003;&nbsp; Your booking details have been updated successfully</span>
  </td></tr>

  <tr><td style='padding:28px 40px;'>
    <p style='font-size:15px;color:#374151;margin:0 0 8px;'>Dear <strong>{b.CustomerName}</strong>,</p>
    <p style='font-size:14px;color:#6b7a8d;margin:0 0 24px;'>
      Your booking <strong>#{b.Id}</strong> has been updated with the latest guest information.
    </p>
    <table width='100%' cellpadding='0' cellspacing='0'
           style='border:1.5px solid #dbe7ff;border-radius:8px;overflow:hidden;margin-bottom:20px;'>
      <tr><td style='background:#f8faff;padding:10px 16px;border-bottom:1px solid #dbe7ff;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Updated Guest Details</div>
      </td></tr>
      <tr><td style='padding:14px 16px;'>
        <table width='100%' cellpadding='4' cellspacing='0' style='font-size:13px;'>
          <tr><td style='color:#6b7a8d;width:45%;'>Total Guests</td><td style='font-weight:600;color:#1a3a6b;'>{b.GuestNumbers}</td></tr>
          {(b.TotalAdults.HasValue ? $"<tr><td style='color:#6b7a8d;'>Adults</td><td style='font-weight:600;color:#1a3a6b;'>{b.TotalAdults}</td></tr>" : "")}
          {(b.TotalKids.HasValue ? $"<tr><td style='color:#6b7a8d;'>Children</td><td style='font-weight:600;color:#1a3a6b;'>{b.TotalKids}</td></tr>" : "")}
          {(!string.IsNullOrWhiteSpace(b.Document) ? "<tr><td style='color:#6b7a8d;'>ID Proof</td><td style='font-weight:600;color:#16a34a;'>Updated &#10003;</td></tr>" : "")}
        </table>
      </td></tr>
    </table>
    <p style='font-size:13px;color:#6b7a8d;margin:0;'>If you did not make this change, please contact us immediately.</p>
  </td></tr>

  <tr><td style='background:#1a3a6b;padding:16px 40px;text-align:center;'>
    <div style='font-size:11px;color:#a8c4e8;'>{_cfg.DisplayName} &middot; Automated message.</div>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

    private string AdminGuestUpdateNotificationHtml(Booking b) => $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:32px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0'
       style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;'>

  <tr><td style='background:linear-gradient(135deg,#1a3a6b 0%,#2563a8 100%);padding:24px 40px;text-align:center;'>
    <div style='font-size:22px;font-weight:700;color:#fff;'>{_cfg.DisplayName}</div>
    <div style='font-size:11px;color:#a8c4e8;margin-top:5px;letter-spacing:2px;text-transform:uppercase;'>Guest Self-Update Notification</div>
  </td></tr>

  <tr><td style='background:#fffbeb;padding:12px 40px;border-bottom:2px solid #fde68a;text-align:center;'>
    <div style='font-size:13px;font-weight:700;color:#92400e;'>A guest has updated their booking details</div>
  </td></tr>

  <tr><td style='padding:24px 40px;'>
    <table width='100%' cellpadding='4' cellspacing='0' style='font-size:13px;border:1.5px solid #dbe7ff;border-radius:8px;overflow:hidden;'>
      <tr><td style='background:#f8faff;padding:10px 16px;border-bottom:1px solid #dbe7ff;' colspan='2'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Booking #{b.Id}</div>
      </td></tr>
      <tr><td style='padding:5px 16px;color:#6b7a8d;width:40%;'>Guest</td><td style='font-weight:600;color:#1a3a6b;padding:5px 4px;'>{b.CustomerName}</td></tr>
      <tr><td style='padding:5px 16px;color:#6b7a8d;'>Phone</td><td style='font-weight:600;color:#1a3a6b;padding:5px 4px;'>{b.CustomerPhone}</td></tr>
      <tr><td style='padding:5px 16px;color:#6b7a8d;'>Total Guests</td><td style='font-weight:700;color:#2563a8;padding:5px 4px;'>{b.GuestNumbers}</td></tr>
      {(b.TotalAdults.HasValue ? $"<tr><td style='padding:5px 16px;color:#6b7a8d;'>Adults</td><td style='font-weight:600;color:#1a3a6b;padding:5px 4px;'>{b.TotalAdults}</td></tr>" : "")}
      {(b.TotalKids.HasValue ? $"<tr><td style='padding:5px 16px;color:#6b7a8d;'>Kids</td><td style='font-weight:600;color:#1a3a6b;padding:5px 4px;'>{b.TotalKids}</td></tr>" : "")}
      {(!string.IsNullOrWhiteSpace(b.Document) ? "<tr><td style='padding:5px 16px;color:#6b7a8d;'>ID Proof</td><td style='font-weight:600;color:#16a34a;padding:5px 4px;'>Updated</td></tr>" : "")}
    </table>
  </td></tr>

  <tr><td style='background:#1a3a6b;padding:14px 40px;text-align:center;'>
    <div style='font-size:11px;color:#a8c4e8;'>{_cfg.DisplayName} &middot; Admin Notification</div>
  </td></tr>
</table>
</td></tr></table>
</body></html>";

    private string AdminNotificationHtml(Booking b)
    {
        var pmColor = (b.PaymentMethod ?? "").ToLower() == "pay later" ? "#d97706" : "#2563a8";
        return $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:32px 0;'>
<tr><td align='center'>
<table width='600' cellpadding='0' cellspacing='0'
       style='background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;'>

  <tr><td style='background:linear-gradient(135deg,#1a3a6b 0%,#2563a8 100%);padding:28px 40px;text-align:center;'>
    <div style='font-size:24px;font-weight:700;color:#fff;'>{_cfg.DisplayName}</div>
    <div style='font-size:11px;color:#a8c4e8;margin-top:6px;letter-spacing:2px;text-transform:uppercase;'>New Booking Notification</div>
  </td></tr>

  <tr><td style='background:#fffbeb;padding:14px 40px;border-bottom:2px solid #fde68a;text-align:center;'>
    <div style='font-size:14px;font-weight:700;color:#92400e;'>New booking request received from the website</div>
  </td></tr>

  <tr><td style='padding:28px 40px;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
    <tr>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;text-align:center;'>
        <div style='font-size:10px;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Booking ID</div>
        <div style='font-size:20px;font-weight:700;color:#1a3a6b;'>#{b.Id}</div>
      </td>
      <td width='2%'></td>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;text-align:center;'>
        <div style='font-size:10px;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;'>Payment Method</div>
        <div style='font-size:15px;font-weight:700;color:{pmColor};'>{b.PaymentMethod ?? "N/A"}</div>
      </td>
    </tr>
    </table>

    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
    <tr>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;vertical-align:top;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:8px;'>Guest</div>
        <table width='100%' cellpadding='3' cellspacing='0' style='font-size:12px;'>
          <tr><td style='color:#6b7a8d;width:40%;'>Name</td><td style='font-weight:600;color:#1a3a6b;'>{b.CustomerName}</td></tr>
          {Row("Email",    b.CustomerEmail)}
          {Row("Phone",    b.CustomerPhone)}
          {Row("Guests",   b.GuestNumbers.ToString())}
          {(string.IsNullOrEmpty(b.BookedByName) ? "" : Row("Booked By", b.BookedByName))}
        </table>
      </td>
      <td width='2%'></td>
      <td width='49%' style='background:#f8faff;border:1.5px solid #dbe7ff;border-radius:8px;padding:14px 16px;vertical-align:top;'>
        <div style='font-size:10px;font-weight:700;color:#6b7a8d;text-transform:uppercase;letter-spacing:.8px;margin-bottom:8px;'>Booking</div>
        <table width='100%' cellpadding='3' cellspacing='0' style='font-size:12px;'>
          <tr><td style='color:#6b7a8d;width:40%;'>Property</td><td style='font-weight:600;color:#1a3a6b;'>{b.Home?.Name ?? "N/A"}</td></tr>
          <tr><td style='color:#6b7a8d;'>Check-in</td><td style='font-weight:600;color:#1a3a6b;'>{b.BookingDateFrom:dd MMM yyyy}</td></tr>
          <tr><td style='color:#6b7a8d;'>Check-out</td><td style='font-weight:600;color:#1a3a6b;'>{b.BookingDateTo:dd MMM yyyy}</td></tr>
          <tr><td style='color:#6b7a8d;'>Total</td><td style='font-size:13px;font-weight:700;color:#2563a8;'>&#8377;{b.Price:N0}</td></tr>
          {(b.AdvancePrice > 0 ? Row("Advance", "&#8377;" + b.AdvancePrice.Value.ToString("N0")) : "")}
        </table>
      </td>
    </tr>
    </table>

    <p style='font-size:13px;color:#6b7a8d;margin:0;'>Log in to the admin panel to review and approve this request.</p>
  </td></tr>

  <tr><td style='background:#1a3a6b;padding:16px 40px;text-align:center;'>
    <div style='font-size:11px;color:#a8c4e8;'>{_cfg.DisplayName} &middot; Admin Notification</div>
  </td></tr>
</table>
</td></tr></table>
</body></html>";
    }

    private string InvoiceEmailWrapper(string invoiceHtml, Booking b) => $@"<!DOCTYPE html>
<html><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'></head>
<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f6fb;padding:24px 0;'>
<tr><td align='center'>
  <table width='700' cellpadding='0' cellspacing='0' style='max-width:700px;'>
    <tr><td style='padding:0 0 12px;text-align:center;'>
      <div style='font-size:14px;color:#6b7a8d;'>Invoice for Booking <strong>#{b.Id}</strong> from <strong>{_cfg.DisplayName}</strong>.</div>
      <div style='font-size:12px;color:#9ca3af;margin-top:4px;'>Please save or print this email for your records.</div>
    </td></tr>
    <tr><td>{invoiceHtml}</td></tr>
    <tr><td style='padding:12px 0;text-align:center;'>
      <div style='font-size:11px;color:#9ca3af;'>{_cfg.DisplayName} &middot; Automated message.</div>
    </td></tr>
  </table>
</td></tr></table>
</body></html>";

    private static string Row(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? "" :
        $"<tr><td style='color:#6b7a8d;vertical-align:top;padding:3px 0;'>{label}</td>" +
        $"<td style='font-weight:600;color:#1a3a6b;padding:3px 0;'>{value}</td></tr>";
}
