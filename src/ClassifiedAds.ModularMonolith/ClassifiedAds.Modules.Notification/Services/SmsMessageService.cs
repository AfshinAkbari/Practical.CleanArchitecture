using ClassifiedAds.Domain.Infrastructure.MessageBrokers;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Modules.Notification.Contracts.DTOs;
using ClassifiedAds.Modules.Notification.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ClassifiedAds.Modules.Notification.Services
{
    public class SmsMessageService
    {
        private readonly ILogger _logger;
        private readonly IRepository<SmsMessage, Guid> _repository;
        private readonly IMessageSender<SmsMessageCreatedEvent> _smsMessageCreatedEventSender;

        public SmsMessageService(ILogger<SmsMessageService> logger,
            IRepository<SmsMessage, Guid> repository,
            IMessageSender<SmsMessageCreatedEvent> smsMessageCreatedEventSender)
        {
            _logger = logger;
            _repository = repository;
            _smsMessageCreatedEventSender = smsMessageCreatedEventSender;
        }

        public int ResendSmsMessage()
        {
            var dateTime = DateTimeOffset.Now.AddMinutes(-1);

            var messages = _repository.GetAll()
                .Where(x => x.SentDateTime == null && x.RetriedCount < 3)
                .Where(x => (x.RetriedCount == 0 && x.CreatedDateTime < dateTime) || (x.RetriedCount != 0 && x.UpdatedDateTime < dateTime))
                .ToList();

            if (messages.Any())
            {
                foreach (var sms in messages)
                {
                    _smsMessageCreatedEventSender.Send(new SmsMessageCreatedEvent { Id = sms.Id });

                    sms.RetriedCount++;

                    _repository.AddOrUpdate(sms);
                    _repository.UnitOfWork.SaveChanges();
                }
            }
            else
            {
                _logger.LogInformation("No SMS to resend.");
            }

            return messages.Count;
        }
    }
}
