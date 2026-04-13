using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChessTournaments.Modules.Tournaments.IntegrationTests.StepDefinitions;

[Binding]
public class TournamentLifecycleSteps : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _lastResponse;
    private string? _currentTournamentId;

    public TournamentLifecycleSteps(
        WebApplicationFactory<Program> factory,
        ScenarioContext scenarioContext
    )
    {
        _factory = factory;
        _scenarioContext = scenarioContext;
        _client = _factory.CreateClient();
    }

    [Given(@"the API is running")]
    public void GivenTheApiIsRunning()
    {
        // API is running through WebApplicationFactory
        _client.BaseAddress.Should().NotBeNull();
    }

    [Given(@"I am authenticated as an admin")]
    public void GivenIAmAuthenticatedAsAnAdmin()
    {
        // In a real scenario, you would get a valid JWT token
        // For this example, we'll use a mock token or configure test authentication
        // _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // For now, we'll skip authentication in tests
        // You can configure test authentication in the WebApplicationFactory
    }

    [When(@"I create a tournament with the following details:")]
    public async Task WhenICreateATournamentWithTheFollowingDetails(Table table)
    {
        var tournamentData = new
        {
            name = table.Rows[0]["Value"],
            description = table.Rows[1]["Value"],
            startDate = DateTime.UtcNow.AddDays(7),
            location = table.Rows[2]["Value"],
            organizerId = "test-organizer",
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = int.Parse(table.Rows[3]["Value"]),
            timeInMinutes = 15,
            incrementInSeconds = 10,
        };

        _lastResponse = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);

        if (_lastResponse.IsSuccessStatusCode)
        {
            var responseContent = await _lastResponse.Content.ReadAsStringAsync();
            var tournament = JsonSerializer.Deserialize<JsonElement>(responseContent);
            _currentTournamentId = tournament.GetProperty("id").GetString();
            _scenarioContext["TournamentId"] = _currentTournamentId;
        }
    }

    [Given(@"a tournament exists in ""([^""]*)"" status")]
    public async Task GivenATournamentExistsInStatus(string status)
    {
        // Create a tournament
        var tournamentData = new
        {
            name = "Test Tournament",
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = "test-organizer",
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 16,
            timeInMinutes = 15,
            incrementInSeconds = 10,
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);
        createResponse.EnsureSuccessStatusCode();

        var responseContent = await createResponse.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(responseContent);
        _currentTournamentId = tournament.GetProperty("id").GetString();
        _scenarioContext["TournamentId"] = _currentTournamentId;

        // Move tournament to desired status
        switch (status)
        {
            case "Registration":
                await _client.PostAsync(
                    $"/api/tournaments/{_currentTournamentId}/open-registration",
                    null
                );
                break;
            case "InProgress":
                await _client.PostAsync(
                    $"/api/tournaments/{_currentTournamentId}/open-registration",
                    null
                );
                await _client.PostAsync(
                    $"/api/tournaments/{_currentTournamentId}/close-registration",
                    null
                );
                break;
            case "Completed":
                await _client.PostAsync(
                    $"/api/tournaments/{_currentTournamentId}/open-registration",
                    null
                );
                await _client.PostAsync(
                    $"/api/tournaments/{_currentTournamentId}/close-registration",
                    null
                );
                await _client.PostAsync($"/api/tournaments/{_currentTournamentId}/complete", null);
                break;
        }
    }

    [When(@"I open registration for the tournament")]
    public async Task WhenIOpenRegistrationForTheTournament()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.PostAsync(
            $"/api/tournaments/{tournamentId}/open-registration",
            null
        );
    }

    [When(@"I close registration for the tournament")]
    public async Task WhenICloseRegistrationForTheTournament()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.PostAsync(
            $"/api/tournaments/{tournamentId}/close-registration",
            null
        );
    }

    [When(@"I complete the tournament")]
    public async Task WhenICompleteTheTournament()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.PostAsync($"/api/tournaments/{tournamentId}/complete", null);
    }

    [When(@"I cancel the tournament")]
    [When(@"I attempt to cancel the tournament")]
    public async Task WhenICancelTheTournament()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.PostAsync($"/api/tournaments/{tournamentId}/cancel", null);
    }

    [Then(@"the tournament should be created successfully")]
    public void ThenTheTournamentShouldBeCreatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Then(@"the tournament status should be ""([^""]*)""")]
    public async Task ThenTheTournamentStatusShouldBe(string expectedStatus)
    {
        var tournamentId = GetCurrentTournamentId();
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        var actualStatus = tournament.GetProperty("status").GetString();

        actualStatus.Should().Be(expectedStatus);
    }

    [Then(@"the request should fail with status code (\d+)")]
    public void ThenTheRequestShouldFailWithStatusCode(int expectedStatusCode)
    {
        _lastResponse.Should().NotBeNull();
        ((int)_lastResponse!.StatusCode).Should().Be(expectedStatusCode);
    }

    [When(@"I register a player ""([^""]*)"" with rating (\d+)")]
    public async Task WhenIRegisterAPlayerWithRating(string playerName, int rating)
    {
        var tournamentId = GetCurrentTournamentId();
        var playerData = new
        {
            tournamentId = tournamentId,
            playerId = Guid.NewGuid().ToString(),
            playerName = playerName,
            rating = rating,
        };

        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/tournaments/{tournamentId}/players",
            playerData
        );

        if (_lastResponse.IsSuccessStatusCode)
        {
            _scenarioContext[$"Player_{playerName}"] = playerData.playerId;
        }
    }

    [When(@"I attempt to register a player ""([^""]*)"" with rating (\d+)")]
    public async Task WhenIAttemptToRegisterAPlayerWithRating(string playerName, int rating)
    {
        await WhenIRegisterAPlayerWithRating(playerName, rating);
    }

    [When(@"I register a player ""([^""]*)"" without rating")]
    public async Task WhenIRegisterAPlayerWithoutRating(string playerName)
    {
        var tournamentId = GetCurrentTournamentId();
        var playerData = new
        {
            tournamentId = tournamentId,
            playerId = Guid.NewGuid().ToString(),
            playerName = playerName,
            rating = (int?)null,
        };

        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/tournaments/{tournamentId}/players",
            playerData
        );

        if (_lastResponse.IsSuccessStatusCode)
        {
            _scenarioContext[$"Player_{playerName}"] = playerData.playerId;
        }
    }

    [When(@"I attempt to register the same player again")]
    public async Task WhenIAttemptToRegisterTheSamePlayerAgain()
    {
        var tournamentId = GetCurrentTournamentId();
        var playerId = _scenarioContext.Get<string>("Player_John Doe");
        var playerData = new
        {
            tournamentId = tournamentId,
            playerId = playerId,
            playerName = "John Doe",
            rating = 1500,
        };

        _lastResponse = await _client.PostAsJsonAsync(
            $"/api/tournaments/{tournamentId}/players",
            playerData
        );
    }

    [When(@"I attempt to close registration for the tournament")]
    public async Task WhenIAttemptToCloseRegistrationForTheTournament()
    {
        await WhenICloseRegistrationForTheTournament();
    }

    [When(@"I update the tournament with the following details:")]
    public async Task WhenIUpdateTheTournamentWithTheFollowingDetails(Table table)
    {
        var tournamentId = GetCurrentTournamentId();
        var updateData = new
        {
            tournamentId = tournamentId,
            name = table.Rows[0]["Value"],
            description = table.Rows[1]["Value"],
        };

        _lastResponse = await _client.PutAsJsonAsync(
            $"/api/tournaments/{tournamentId}",
            updateData
        );
    }

    [When(@"I attempt to create a tournament with empty name")]
    public async Task WhenIAttemptToCreateATournamentWithEmptyName()
    {
        var tournamentData = new
        {
            name = "",
            description = "Test Description",
            startDate = DateTime.UtcNow.AddDays(7),
            location = "Test Location",
            organizerId = "test-organizer",
            format = TournamentFormat.Swiss,
            timeControl = TimeControl.Rapid,
            maxPlayers = 16,
            timeInMinutes = 15,
            incrementInSeconds = 10,
        };

        _lastResponse = await _client.PostAsJsonAsync("/api/tournaments", tournamentData);
    }

    [When(@"I request all tournaments")]
    public async Task WhenIRequestAllTournaments()
    {
        _lastResponse = await _client.GetAsync("/api/tournaments");
        if (_lastResponse.IsSuccessStatusCode)
        {
            var content = await _lastResponse.Content.ReadAsStringAsync();
            _scenarioContext["TournamentsResponse"] = content;
        }
    }

    [When(@"I request the tournament by ID")]
    public async Task WhenIRequestTheTournamentById()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        if (_lastResponse.IsSuccessStatusCode)
        {
            var content = await _lastResponse.Content.ReadAsStringAsync();
            _scenarioContext["TournamentDetails"] = content;
        }
    }

    [When(@"I request the tournament players")]
    public async Task WhenIRequestTheTournamentPlayers()
    {
        var tournamentId = GetCurrentTournamentId();
        _lastResponse = await _client.GetAsync($"/api/tournaments/{tournamentId}/players");
        if (_lastResponse.IsSuccessStatusCode)
        {
            var content = await _lastResponse.Content.ReadAsStringAsync();
            _scenarioContext["PlayersResponse"] = content;
        }
    }

    [Given(@"(\d+) tournaments exist")]
    public async Task GivenTournamentsExist(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var tournamentData = new
            {
                name = $"Tournament {i + 1}",
                description = $"Description {i + 1}",
                startDate = DateTime.UtcNow.AddDays(7),
                location = "Test Location",
                organizerId = "test-organizer",
                format = TournamentFormat.Swiss,
                timeControl = TimeControl.Rapid,
                maxPlayers = 16,
                timeInMinutes = 15,
                incrementInSeconds = 10,
            };

            await _client.PostAsJsonAsync("/api/tournaments", tournamentData);
        }
    }

    [Given(@"the tournament requires minimum (\d+) players")]
    public void GivenTheTournamentRequiresMinimumPlayers(int minPlayers)
    {
        // This is already set in the tournament creation with minPlayers = 4 by default
        // We can store this in scenario context if needed for validation
        _scenarioContext["MinPlayers"] = minPlayers;
    }

    [Then(@"the tournament should have (\d+) players?")]
    public async Task ThenTheTournamentShouldHavePlayers(int expectedCount)
    {
        var tournamentId = GetCurrentTournamentId();
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}/players");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var players = JsonSerializer.Deserialize<JsonElement>(content);
        players.GetArrayLength().Should().Be(expectedCount);
    }

    [Then(@"the tournament name should be ""([^""]*)""")]
    public async Task ThenTheTournamentNameShouldBe(string expectedName)
    {
        var tournamentId = GetCurrentTournamentId();
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        var actualName = tournament.GetProperty("name").GetString();

        actualName.Should().Be(expectedName);
    }

    [Then(@"the tournament description should be ""([^""]*)""")]
    public async Task ThenTheTournamentDescriptionShouldBe(string expectedDescription)
    {
        var tournamentId = GetCurrentTournamentId();
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        var actualDescription = tournament.GetProperty("description").GetString();

        actualDescription.Should().Be(expectedDescription);
    }

    [Then(@"I should receive (\d+) tournaments?")]
    public void ThenIShouldReceiveTournaments(int expectedCount)
    {
        var content = _scenarioContext.Get<string>("TournamentsResponse");
        var tournaments = JsonSerializer.Deserialize<JsonElement>(content);
        tournaments.GetArrayLength().Should().Be(expectedCount);
    }

    [Then(@"I should receive the tournament details")]
    public void ThenIShouldReceiveTheTournamentDetails()
    {
        var content = _scenarioContext.Get<string>("TournamentDetails");
        var tournament = JsonSerializer.Deserialize<JsonElement>(content);
        tournament.ValueKind.Should().Be(JsonValueKind.Object);
        tournament.TryGetProperty("id", out _).Should().BeTrue();
        tournament.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Then(@"I should receive (\d+) players?")]
    public void ThenIShouldReceivePlayers(int expectedCount)
    {
        var content = _scenarioContext.Get<string>("PlayersResponse");
        var players = JsonSerializer.Deserialize<JsonElement>(content);
        players.GetArrayLength().Should().Be(expectedCount);
    }

    [Then(@"the players should be ordered by rating")]
    public void ThenThePlayersShouldBeOrderedByRating()
    {
        var content = _scenarioContext.Get<string>("PlayersResponse");
        var players = JsonSerializer.Deserialize<JsonElement>(content);

        var ratings = new List<int>();
        foreach (var player in players.EnumerateArray())
        {
            if (
                player.TryGetProperty("rating", out var rating)
                && rating.ValueKind != JsonValueKind.Null
            )
            {
                ratings.Add(rating.GetInt32());
            }
        }

        // Should be in descending order
        for (int i = 0; i < ratings.Count - 1; i++)
        {
            ratings[i].Should().BeGreaterThanOrEqualTo(ratings[i + 1]);
        }
    }

    [Then(@"the player should have no rating")]
    public async Task ThenThePlayerShouldHaveNoRating()
    {
        var tournamentId = GetCurrentTournamentId();
        var response = await _client.GetAsync($"/api/tournaments/{tournamentId}/players");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var players = JsonSerializer.Deserialize<JsonElement>(content);
        var player = players.EnumerateArray().First();

        var hasRating = player.TryGetProperty("rating", out var rating);
        if (hasRating)
        {
            rating.ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    private string GetCurrentTournamentId()
    {
        return _currentTournamentId
            ?? _scenarioContext.Get<string>("TournamentId")
            ?? throw new InvalidOperationException("No tournament ID found in context");
    }
}
