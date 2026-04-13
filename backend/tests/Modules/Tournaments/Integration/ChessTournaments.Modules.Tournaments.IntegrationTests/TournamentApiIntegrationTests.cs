using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChessTournaments.Modules.Tournaments.IntegrationTests;

public class TournamentApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TournamentApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTournament_Should_Return_Created_Status()
    {
        // Arrange
        var tournamentData = new
        {
            name = "Integration Test Tournament",
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = Guid.NewGuid().ToString(),
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 16,
            minPlayers = 4,
            timeInMinutes = 15,
            incrementInSeconds = 10,
            numberOfRounds = 5,
            entryFee = 0,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.GetProperty("name").GetString().Should().Be("Integration Test Tournament");
    }

    [Fact]
    public async Task CreateTournament_With_Invalid_Data_Should_Return_BadRequest()
    {
        // Arrange
        var tournamentData = new
        {
            name = "", // Empty name should fail validation
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = Guid.NewGuid().ToString(),
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 16,
            timeInMinutes = 15,
            incrementInSeconds = 10,
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllTournaments_Should_Return_Ok()
    {
        // Act
        var response = await _client.GetAsync("/api/tournaments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var tournaments = JsonSerializer.Deserialize<JsonElement>(content);
        tournaments.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetTournamentById_Should_Return_Tournament()
    {
        // Arrange - Create a tournament first
        var tournamentData = new
        {
            name = "Test Tournament",
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = Guid.NewGuid().ToString(),
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 16,
            minPlayers = 4,
            timeInMinutes = 15,
            incrementInSeconds = 10,
            numberOfRounds = 5,
            entryFee = 0,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(createContent);
        var tournamentId = tournament.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.GetProperty("id").GetString().Should().Be(tournamentId);
    }

    [Fact]
    public async Task GetTournamentById_With_Invalid_Id_Should_Return_NotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OpenRegistration_Should_Change_Status_To_Registration()
    {
        // Arrange - Create tournament
        var tournamentId = await CreateTournamentAsync();

        // Act
        var response = await _client.PostAsync(
            $"/api/tournaments/{tournamentId}/open-registration",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.GetProperty("status").GetString().Should().Be("Registration");
    }

    [Fact]
    public async Task RegisterPlayer_Should_Add_Player_To_Tournament()
    {
        // Arrange - Create tournament and open registration
        var tournamentId = await CreateTournamentAsync();
        await _client.PostAsync($"/api/tournaments/{tournamentId}/open-registration", null);

        var playerData = new
        {
            tournamentId = tournamentId,
            playerId = Guid.NewGuid().ToString(),
            playerName = "Test Player",
            rating = 1500,
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/tournaments/{tournamentId}/players",
            playerData
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var playersResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}/players");
        var content = await playersResponse.Content.ReadAsStringAsync();
        var players = JsonSerializer.Deserialize<JsonElement>(content);
        players.GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task UpdateTournament_Should_Update_Details()
    {
        // Arrange - Create tournament
        var tournamentId = await CreateTournamentAsync();

        var updateData = new
        {
            tournamentId = tournamentId,
            name = "Updated Tournament Name",
            description = "Updated Description",
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tournaments/{tournamentId}", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.GetProperty("name").GetString().Should().Be("Updated Tournament Name");
        tournament.GetProperty("description").GetString().Should().Be("Updated Description");
    }

    [Fact]
    public async Task CloseRegistration_With_Minimum_Players_Should_Succeed()
    {
        // Arrange - Create tournament, open registration, register players
        var tournamentId = await CreateTournamentAsync();
        await _client.PostAsync($"/api/tournaments/{tournamentId}/open-registration", null);

        // Register 4 players (minimum)
        for (int i = 0; i < 4; i++)
        {
            var playerData = new
            {
                tournamentId = tournamentId,
                playerId = Guid.NewGuid().ToString(),
                playerName = $"Player {i + 1}",
                rating = 1500 + i * 10,
            };
            await _client.PostAsJsonAsync($"/api/tournaments/{tournamentId}/players", playerData);
        }

        // Act
        var response = await _client.PostAsync(
            $"/api/tournaments/{tournamentId}/close-registration",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.GetProperty("status").GetString().Should().Be("InProgress");
    }

    [Fact]
    public async Task CloseRegistration_Without_Minimum_Players_Should_Fail()
    {
        // Arrange - Create tournament, open registration
        var tournamentId = await CreateTournamentAsync();
        await _client.PostAsync($"/api/tournaments/{tournamentId}/open-registration", null);

        // Only register 2 players (less than minimum of 4)
        for (int i = 0; i < 2; i++)
        {
            var playerData = new
            {
                tournamentId = tournamentId,
                playerId = Guid.NewGuid().ToString(),
                playerName = $"Player {i + 1}",
                rating = 1500,
            };
            await _client.PostAsJsonAsync($"/api/tournaments/{tournamentId}/players", playerData);
        }

        // Act
        var response = await _client.PostAsync(
            $"/api/tournaments/{tournamentId}/close-registration",
            null
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelTournament_In_Draft_Should_Succeed()
    {
        // Arrange - Create tournament
        var tournamentId = await CreateTournamentAsync();

        // Act
        var response = await _client.PostAsync($"/api/tournaments/{tournamentId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        var content = await getResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.GetProperty("status").GetString().Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetTournamentPlayers_Should_Return_Ordered_List()
    {
        // Arrange - Create tournament, open registration, register players
        var tournamentId = await CreateTournamentAsync();
        await _client.PostAsync($"/api/tournaments/{tournamentId}/open-registration", null);

        var ratings = new[] { 1400, 1600, 1500, 1550 };
        foreach (var rating in ratings)
        {
            var playerData = new
            {
                tournamentId = tournamentId,
                playerId = Guid.NewGuid().ToString(),
                playerName = $"Player {rating}",
                rating = rating,
            };
            await _client.PostAsJsonAsync($"/api/tournaments/{tournamentId}/players", playerData);
        }

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}/players");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var players = JsonSerializer.Deserialize<JsonElement>(content);
        players.GetArrayLength().Should().Be(4);

        // Verify players are ordered by rating descending
        var playerRatings = new List<int>();
        foreach (var player in players.EnumerateArray())
        {
            playerRatings.Add(player.GetProperty("rating").GetInt32());
        }

        playerRatings.Should().BeInDescendingOrder();
    }

    private async Task<string> CreateTournamentAsync()
    {
        var tournamentData = new
        {
            name = "Test Tournament",
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = Guid.NewGuid().ToString(),
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 20,
            minPlayers = 4,
            timeInMinutes = 15,
            incrementInSeconds = 10,
            numberOfRounds = 5,
            entryFee = 0,
        };

        var response = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);
        var content = await response.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        return tournament.GetProperty("id").GetString()!;
    }
}
