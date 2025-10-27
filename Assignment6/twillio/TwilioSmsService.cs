namespace Assignment6.twillio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;
using Assignment6;

public class TwilioSmsService
{
    private readonly WhatsappService
  _settings;
    public TwilioSmsService(IOptions<WhatsappService> opts)
    {
        _settings = opts.Value;
    }

    public async Task<MessageResource> SendSmsAsync(string to, string body)
    {
               var message = await MessageResource.CreateAsync(
               to: new PhoneNumber($"whatsapp:{to}"),
               from: new PhoneNumber(_settings.FromPhone),
               body: body
        );
        return message;
    }
}
