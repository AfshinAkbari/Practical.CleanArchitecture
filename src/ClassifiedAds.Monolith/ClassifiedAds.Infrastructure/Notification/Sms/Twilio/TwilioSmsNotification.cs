using ClassifiedAds.Domain.Entities;
using ClassifiedAds.Domain.Notification;
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

        public void Send(SmsMessage smsMessage)
        {
            throw new NotImplementedException();
        }
    }
}
