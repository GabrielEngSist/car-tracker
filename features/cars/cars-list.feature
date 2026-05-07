Feature: Cars - List cars

  User Story:
    As a vehicle owner
    I want to list my registered cars
    So that I can select a car to view details and history

  Scenario: List cars when there are no cars
    Given no cars are registered
    When the client GETs "/api/cars"
    Then the response status is 200
    And the response JSON is an empty array

  Scenario: List cars returns basic car information
    Given the following cars are registered:
      | Model   | Year | CurrentKm |
      | Civic   | 2020 | 45000     |
      | Corolla | 2018 | 12345     |
    When the client GETs "/api/cars"
    Then the response status is 200
    And the response JSON is an array of CarDto items

