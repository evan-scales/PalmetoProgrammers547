using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

public class TaskServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    WebApplicationFactory<Program> _factory;
    HttpClient _client;

    public TaskServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<string> getResponseJsonString(HttpResponseMessage response)
    {
        bool hasJsonHeader = "application/json; charset=utf-8" == response.Content.Headers.ContentType?.ToString();
        if (!hasJsonHeader)
        {
            throw new Exception("Response type is not Json.");
        }

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<bool> responseContainsJsonArray(HttpResponseMessage response)
    {
        JArray.Parse(await getResponseJsonString(response));
        return true;
    }

    private async Task<bool> responseContainsNonEmptyJsonObject(HttpResponseMessage response)
    {
        return JObject.Parse(await getResponseJsonString(response)).HasValues;
    }

    [Fact]
    public async void GetAll_ResponseOK()
    {
        var response = await _client.GetAsync("/items");

        response.EnsureSuccessStatusCode();
    }
}