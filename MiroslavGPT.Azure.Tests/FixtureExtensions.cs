using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace MiroslavGPT.Azure.Tests;

public static class FixtureExtensions
{
    public static HttpRequest CreateMockHttpRequest<TBody>(this Fixture fixture, TBody body) {
        var json = JsonConvert.SerializeObject(body);
        var byteArray = Encoding.UTF8.GetBytes(json);

        var memoryStream = new MemoryStream(byteArray);
        memoryStream.Flush();
        memoryStream.Position = 0;

        var mockRequest = fixture.Create<Mock<HttpRequest>>();
        mockRequest.Setup(x => x.Body).Returns(memoryStream);

        return mockRequest.Object;
    }
}