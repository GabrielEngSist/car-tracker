Feature: Cars - Car registry snapshot

  User Story:
    As a vehicle owner
    I want a single registry view that includes my car and its related records
    So that I can see everything linked to the car in one place

  Scenario: Get registry for existing car
    Given a registered car exists with id "{carId}"
    And the car has expense entries and maintenance plan items
    When the client GETs "/api/cars/{carId}/registry"
    Then the response status is 200
    And the response body is a CarRegistryDto containing:
      | Car.Id | {carId} |
    And the response contains:
      | ExpenseEntries        |
      | MaintenancePlanItems  |

  Scenario: Get registry returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/registry"
    Then the response status is 404

