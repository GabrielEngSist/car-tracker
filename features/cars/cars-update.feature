Feature: Cars - Update car

  User Story:
    As a vehicle owner
    I want to update a car's details
    So that the registry stays accurate over time

  Business rules:
    - If a field is omitted, it is not changed.
    - If "Placa" is provided as blank, it clears the plate.
    - Validations return faults with stable codes and propertyName bindings.

  Scenario: Update an existing car successfully
    Given a registered car exists with id "{carId}"
    When the client PATCHes "/api/cars/{carId}" with:
      | Model     | "Civic"  |
      | Year      | 2020     |
      | CurrentKm | 45000    |
      | Name      | "Daily"  |
      | Placa     | "ABC1D23"|
    Then the response status is 200
    And the response body is a CarDto with:
      | Id | {carId} |

  Scenario: Update returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client PATCHes "/api/cars/{carId}" with:
      | Model | "Anything" |
    Then the response status is 404

  Scenario: Clear plate by sending blank Placa
    Given a registered car exists with id "{carId}" and a plate
    When the client PATCHes "/api/cars/{carId}" with:
      | Placa | "   " |
    Then the response status is 200
    And the response body is a CarDto with:
      | Placa | null |

  Scenario Outline: Reject invalid update fields (<code>)
    Given a registered car exists with id "{carId}"
    When the client PATCHes "/api/cars/{carId}" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code               | field      | payload               |
      | MODEL_EMPTY        | Model      | { "model": "   " }    |
      | YEAR_INVALID       | Year       | { "year": 1800 }      |
      | CURRENT_KM_INVALID | CurrentKm  | { "currentKm": -1 }   |
      | PLATE_INVALID      | Placa      | { "placa": "INVALID" }|

