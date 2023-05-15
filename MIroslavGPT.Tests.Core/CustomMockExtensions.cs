using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;

namespace MIroslavGPT.Tests.Core;

[ExcludeFromCodeCoverage]
public static class CustomMockExtensions
{
    public static void VerifyLogError<T>(this Mock<ILogger<T>> mockLogger, string message)
    {
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>())
        );
    }
    
    public static void VerifyLogError<T>(this Mock<ILogger<T>> mockLogger, Exception ex, string message)
    {
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
            ex,
            It.IsAny<Func<It.IsAnyType, Exception, string>>())
        );
    }
}