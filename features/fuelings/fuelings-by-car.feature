Feature: Fuelings - Fuelings for a car

  User Story:
    As a vehicle owner
    I want to view, create, update and delete fuelings for a specific car
    So that my fueling history stays accurate

  Scenario: List fuelings returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/fuelings"
    Then the response status is 404

  Scenario: Create fueling returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client POSTs "/api/cars/{carId}/fuelings" with:
      | PerformedAt  | "2026-01-01" |
      | KmAtFueling  | 1000         |
      | Liters       | 30.5         |
      | TotalPrice   | 200.00       |
    Then the response status is 404

  Scenario: Update fueling returns not found when entry does not exist
    Given a registered car exists with id "{carId}"
    And no fueling exists with id "{fuelingId}" for that car
    When the client PATCHes "/api/cars/{carId}/fuelings/{fuelingId}" with:
      | Liters | 33.0 |
    Then the response status is 404

  Scenario Outline: Reject invalid fueling updates (<code>)
    Given a registered car exists with a fueling entry "{fuelingId}"
    When the client PATCHes "/api/cars/{carId}/fuelings/{fuelingId}" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code                | field       | payload                 |
      | INVALID_KM          | KmAtFueling | { "kmAtFueling": -1 }   |
      | INVALID_LITERS      | Liters      | { "liters": 0 }         |
      | INVALID_TOTAL_PRICE | TotalPrice  | { "totalPrice": -1 }    |

