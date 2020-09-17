using ClassifiedAds.Application;
using ClassifiedAds.Domain.Events;
using ClassifiedAds.Domain.Infrastructure.MessageBrokers;
using ClassifiedAds.Domain.Repositories;
using ClassifiedAds.Modules.Notification.Contracts.DTOs;
using ClassifiedAds.Modules.Notification.Contracts.Services;
using ClassifiedAds.Modules.Notification.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ClassifiedAds.Modules.Notification.Services
{
    public class EmailMessageService : CrudService<EmailMessage>, IEmailMessageService
    {
        private readonly ILogger _logger;
        private readonly IRepository<EmailMessage, Guid> _repository;
        private readonly IMessageSender<EmailMessageCreatedEvent> _emailMessageCreatedEventSender;

        public EmailMessageService(ILogger<EmailMessageService> logger,
            IRepository<EmailMessage, Guid> repository,
            IMessageSender<EmailMessageCreatedEvent> emailMessageCreatedEventSender,
            IDomainEvents domainEvents)
            : base(repository, domainEvents)
        {
            _logger = logger;
            _repository = repository;
            _emailMessageCreatedEventSender = emailMessageCreatedEventSender;
        }

        public void CreateEmailMessage(EmailMessageDTO emailMessage)
        {
            AddOrUpdate(new EmailMessage
            {
                From = emailMessage.From,
                Tos = emailMessage.Tos,
                CCs = emailMessage.CCs,
                BCCs = emailMessage.BCCs,
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
            });
        }

        public int ResendEmailMessages()
        {
            var dateTime = DateTimeOffset.Now.AddMinutes(-1);

            var messages = _repository.GetAll()
                .Where(x => x.SentDateTime == null && x.RetriedCount < 3)
                .Where(x => (x.RetriedCount == 0 && x.CreatedDateTime < dateTime) || (x.RetriedCount != 0 && x.UpdatedDateTime < dateTime))
                .ToList();

            if (messages.Any())
            {
                foreach (var email in messages)
                {
                    _emailMessageCreatedEventSender.Send(new EmailMessageCreatedEvent { Id = email.Id });

                    email.RetriedCount++;

                    _repository.AddOrUpdate(email);
                    _repository.UnitOfWork.SaveChanges();
                }
            }
            else
            {
                _logger.LogInformation("No email to resend.");
            }

            return messages.Count;
        }
    }
}
