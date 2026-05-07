Feature: Cars - Delete car

  User Story:
    As a vehicle owner
    I want to delete a car I no longer track
    So that it is removed from my registry

  Scenario: Delete existing car
    Given a registered car exists with id "{carId}"
    When the client DELETEs "/api/cars/{carId}"
    Then the response status is 204

  Scenario: Delete returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client DELETEs "/api/cars/{carId}"
    Then the response status is 404

