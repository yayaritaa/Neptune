﻿using Autofac;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Neptune.Services.Common.Bus
{
    public interface IValidator
    {
        bool TryValidate<T>(T message, out ValidationResult result) where T : IMessage;
    }

    public class Validator : IValidator
    {
        private readonly IComponentContext _context;
        private readonly ILogger<Validator> _log;

        public Validator(IComponentContext context, ILogger<Validator> log)
        {
            _context = context;
            _log = log;
        }

        public bool TryValidate<T>(T message, out ValidationResult result) where T : IMessage
        {
            var messageType = message.GetType().Name;

            var validator = _context.ResolveOptional<IMessageValidator<T>>();
            if (validator == null)
            {
                // No validator found, warn and set an empty validation result.
                _log.LogWarning("No validator for {bus-message}", messageType);
                result = new ValidationResult();
                return true;
            }

            _log.LogInformation("Validating {bus-message}...", messageType);

            result = validator.Validate(new ValidationContext<T>(message));
            if (!result.IsValid)
            {
                _log.LogError("Validation failed for {bus-message}", messageType);
                return false;
            }

            _log.LogInformation("Validated {bus-message}", messageType);
            return true;
        }
    }
}
