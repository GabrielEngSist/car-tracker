Feature: Fuelings - List all fuelings

  User Story:
    As a vehicle owner
    I want to list all fuelings across cars
    So that I can review my fueling history globally

  Scenario: List all fuelings
    When the client GETs "/api/fuelings"
    Then the response status is 200
    And the response JSON is an array of FuelingEntryDto items

