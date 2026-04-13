Feature: Tournament Lifecycle
    As a tournament organizer
    I want to manage tournament lifecycle
    So that I can conduct chess tournaments effectively

Background:
    Given the API is running
    And I am authenticated as an admin

Scenario: Create a new tournament
    When I create a tournament with the following details:
        | Field       | Value                    |
        | Name        | Spring Championship 2025 |
        | Description | Annual spring tournament |
        | Location    | New York Chess Club      |
        | MaxPlayers  | 16                       |
    Then the tournament should be created successfully
    And the tournament status should be "Draft"

Scenario: Open tournament registration
    Given a tournament exists in "Draft" status
    When I open registration for the tournament
    Then the tournament status should be "Registration"

Scenario: Close registration and start tournament
    Given a tournament exists in "Registration" status
    When I close registration for the tournament
    Then the tournament status should be "InProgress"

Scenario: Complete a tournament
    Given a tournament exists in "InProgress" status
    When I complete the tournament
    Then the tournament status should be "Completed"

Scenario: Cancel a tournament
    Given a tournament exists in "Draft" status
    When I cancel the tournament
    Then the tournament status should be "Cancelled"

Scenario: Cannot cancel a completed tournament
    Given a tournament exists in "Completed" status
    When I attempt to cancel the tournament
    Then the request should fail with status code 400

Scenario: Full tournament lifecycle
    When I create a tournament with the following details:
        | Field       | Value              |
        | Name        | Summer Tournament  |
        | Description | Test tournament    |
        | Location    | Online             |
        | MaxPlayers  | 8                  |
    And I open registration for the tournament
    And I close registration for the tournament
    And I complete the tournament
    Then the tournament status should be "Completed"

Scenario: Register players to tournament
    Given a tournament exists in "Registration" status
    When I register a player "John Doe" with rating 1500
    And I register a player "Jane Smith" with rating 1600
    Then the tournament should have 2 players

Scenario: Cannot register player when tournament is in Draft status
    Given a tournament exists in "Draft" status
    When I attempt to register a player "John Doe" with rating 1500
    Then the request should fail with status code 400

Scenario: Cannot close registration without minimum players
    Given a tournament exists in "Registration" status
    And the tournament requires minimum 4 players
    When I register a player "Player 1" with rating 1500
    And I register a player "Player 2" with rating 1600
    And I attempt to close registration for the tournament
    Then the request should fail with status code 400

Scenario: Update tournament details
    Given a tournament exists in "Draft" status
    When I update the tournament with the following details:
        | Field       | Value                  |
        | Name        | Updated Tournament     |
        | Description | Updated Description    |
    Then the tournament name should be "Updated Tournament"
    And the tournament description should be "Updated Description"

Scenario: Create tournament with invalid data
    When I attempt to create a tournament with empty name
    Then the request should fail with status code 400

Scenario: Get all tournaments
    Given 3 tournaments exist
    When I request all tournaments
    Then I should receive 3 tournaments

Scenario: Get tournament by ID
    Given a tournament exists in "Draft" status
    When I request the tournament by ID
    Then I should receive the tournament details

Scenario: Get tournament players
    Given a tournament exists in "Registration" status
    When I register a player "Player 1" with rating 1500
    And I register a player "Player 2" with rating 1600
    And I request the tournament players
    Then I should receive 2 players
    And the players should be ordered by rating

Scenario: Register player without rating
    Given a tournament exists in "Registration" status
    When I register a player "Unrated Player" without rating
    Then the tournament should have 1 player
    And the player should have no rating

Scenario: Cannot register duplicate player
    Given a tournament exists in "Registration" status
    When I register a player "John Doe" with rating 1500
    And I attempt to register the same player again
    Then the request should fail with status code 400
