using System;

namespace ClassifiedAds.Infrastructure.Notification.Sms.Twilio
{
    public class TwilioSmsNotification : ISmsNotification
    {
        private readonly TwilioOptions _options;

        public TwilioSmsNotification(TwilioOptions options)
        {
            _options = options;
        }

        public void Send(SmsMessageDTO smsMessage)
        {
            throw new NotImplementedException();
        }
    }
}
