using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SimpleFlow.Tests;

/// <summary>
/// Интеграционные тесты основных HTTP-маршрутов приложения.
/// </summary>
public class HttpRouteTests : IClassFixture<SimpleFlowWebApplicationFactory>
{
    private readonly HttpClient client;

    /// <summary>
    /// Набор интеграционных тестов с изолированным тестовым сервером.
    /// </summary>
    /// <param name="factory">Фабрика тестового веб-приложения.</param>
    public HttpRouteTests(SimpleFlowWebApplicationFactory factory)
    {
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// Успешный ответ основных публичных GET-маршрутов.
    /// </summary>
    /// <param name="route">Проверяемый HTTP-маршрут.</param>
    [Theory]
    [InlineData("/")]
    [InlineData("/Customers")]
    [InlineData("/Customers/Create")]
    [InlineData("/Products")]
    [InlineData("/Products/Create")]
    [InlineData("/Projects")]
    [InlineData("/Projects/Create")]
    [InlineData("/Warehouses")]
    [InlineData("/Warehouses/Create")]
    [InlineData("/Identity/Account/Login")]
    public async Task PublicGetRoute_ReturnsSuccess(string route)
    {
        var response = await client.GetAsync(route);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Маршрут {route} вернул {(int)response.StatusCode}. Ответ: {responseBody}");
    }

    /// <summary>
    /// Ответ 404 для отсутствующей записи в основных ERP-модулях.
    /// </summary>
    /// <param name="route">Маршрут отсутствующей записи.</param>
    [Theory]
    [InlineData("/Customers/Edit/999999")]
    [InlineData("/Products/Edit/999999")]
    [InlineData("/Projects/Edit/999999")]
    [InlineData("/Warehouses/Edit/999999")]
    public async Task MissingEntityRoute_ReturnsNotFound(string route)
    {
        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
