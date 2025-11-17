namespace fuszerkomat_api.Helpers
{
    public class DomainExceptions
    {
        public class UnauthorizedException : Exception
        {
            public UnauthorizedException(string message = "Unauthorized", string? logMessage = null, object? logData = null) : base(message)
            {
                LogMessage = logMessage;
                LogData = logData;
            }
            public string? LogMessage { get; }
            public object? LogData {  get; }
        }

        public class ForbiddenException : Exception
        {
            public ForbiddenException(string message = "Operation prohibited", string? logMessage = null, object? logData = null) : base(message) 
            {
                LogMessage = logMessage;
                LogData = logData;
            }

            public string? LogMessage { get; }
            public object? LogData { get; }
        }

        public class NotFoundException : Exception
        {
            public NotFoundException(string message = "Resource not found", string? logMessage = null, object? logData = null) : base(message) 
            {
                LogMessage = logMessage;
                LogData = logData;
            }

            public string? LogMessage { get; }
            public object? LogData { get; }
        }

        public class InternalException : Exception
        {
            public InternalException(string message = "Operation failed.", string? logMessage = null, object? logData = null) : base(message)
            {
                LogMessage = logMessage;
                LogData = logData;
            }

            public string? LogMessage { get; }
            public object? LogData { get; }
        }

        public class ConflictException : Exception
        {
            public ConflictException(string message = "Conflict", string? logMessage = null, object? logData = null) : base(message)
            {
                LogMessage = logMessage;
                LogData = logData;
            }

            public string? LogMessage { get; }
            public object? LogData { get; }
        }

    }
}