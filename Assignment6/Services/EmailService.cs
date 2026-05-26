using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using DbService.Models;
using Microsoft.Extensions.Options;

namespace Assignment6.Services;

// ── Settings bound from appsettings.json "EmailSettings" section ──────────
public class EmailSettings
{
    public string FromAddress { get; set; }
    public string DisplayName { get; set; }
    public string Password    { get; set; }
    public string SmtpHost    { get; set; } = "smtp.gmail.com";
    public int    SmtpPort    { get; set; } = 587;
    public string AdminEmail  { get; set; }
}

// ── Interface ──────────────────────────────────────────────────────────────
public interface IEmailService
{
    Task SendBookingApprovedAsync(Booking booking);
    Task SendInvoiceEmailAsync(Booking booking, string invoiceHtml);
    Task SendNewWebsiteBookingToAdminAsync(Booking booking);
}

// ── Implementation ─────────────────────────────────────────────────────────
public class EmailService : IEmailService
{
    private readonly EmailSettings _cfg;

    public EmailService(IOptions<EmailSettings> options) => _cfg = options.Value;

    // ── Core sender (MailKit) ──────────────────────────────────────────────
    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(to)) return;

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_cfg.DisplayName, _cfg.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            // Strip spaces from Gmail app password (Google displays them spaced
            // for readability but the actual credential has no spaces)
            var password = (_cfg.Password ?? "").Replace(" ", "");

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.SmtpHost, _cfg.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_cfg.FromAddress, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            Console.WriteLine($"[Email] Sent to {to}: {subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Email] ERROR sending to {to}: {ex.GetType().Name} – {ex.Message}");
        }
    }

    // ── Public methods ─────────────────────────────────────────────────────

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

    public async Task SendInvoiceEmailAsync(Booking booking, string invoiceHtml)
    {
        var subject = $"Your Invoice – Booking #{booking.Id} | {_cfg.DisplayName}";
        var body    = InvoiceEmailWrapper(invoiceHtml, booking);

        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(booking.CustomerEmail)) recipients.Add(booking.CustomerEmail);
        if (!string.IsNullOrWhiteSpace(booking.BookedByEmail)) recipients.Add(booking.BookedByEmail);

        foreach (var r in recipients)
            await SendAsync(r, subject, body);
    }

    public async Task SendNewWebsiteBookingToAdminAsync(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(_cfg.AdminEmail)) return;
        var subject = $"New Booking Request – {booking.Home?.Name ?? "Property"} | {_cfg.DisplayName}";
        await SendAsync(_cfg.AdminEmail, subject, AdminNotificationHtml(booking));
    }

    // ── HTML Templates ─────────────────────────────────────────────────────

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
