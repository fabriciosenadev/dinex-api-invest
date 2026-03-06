namespace DinExApi.Core;

public sealed class DomainValidationException(string message) : Exception(message);
