using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrainingCatalog.Application;

namespace TrainingCatalog.Api.Tests;

public sealed class TrainingCreationTests
{
    [Fact]
    public async Task ReturnsConflictWhenStartDateAlreadyExists()
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-15",
            4);

        var firstResponse = await client.PostAsJsonAsync("/api/trainings", request);
        var secondResponse = await client.PostAsJsonAsync(
            "/api/trainings",
            request with { Title = "C# Avançado" });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        using var error = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Já existe um treinamento com esta data de início.",
            error.RootElement.GetProperty("errors").GetProperty("startDate")[0].GetString());
    }

    [Theory]
    [InlineData(0, "A carga horária deve ser maior que zero.")]
    [InlineData(-1, "A carga horária deve ser maior que zero.")]
    [InlineData(5, "A carga horária não pode ser maior que 4 horas.")]
    public async Task ReturnsBadRequestWhenDurationHoursIsOutOfRange(int durationHours, string expectedMessage)
    {
        using var factory = new TrainingCatalogApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateTrainingRequest(
            "Fundamentos de C#",
            "Introdução ao C#",
            "2026-09-20",
            durationHours);

        var response = await client.PostAsJsonAsync("/api/trainings", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            expectedMessage,
            error.RootElement.GetProperty("errors").GetProperty("durationHours")[0].GetString());
    }
}