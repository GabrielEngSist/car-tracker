Feature: Cars - Get car by id

  User Story:
    As a vehicle owner
    I want to retrieve a car by its identifier
    So that I can view its current details

  Scenario: Get existing car by id
    Given a registered car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}"
    Then the response status is 200
    And the response body is a CarDto with:
      | Id | {carId} |

  Scenario: Get car by id returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}"
    Then the response status is 404

