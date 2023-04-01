using Microsoft.Extensions.Logging;
using Moq;

namespace MiroslavGPT.Tests.Core
{
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
    }
}
