Feature: Expense Entries - Entries for a car

  User Story:
    As a vehicle owner
    I want to record expense entries for a car
    So that I can understand the car's cost over time

  Scenario: List expense entries returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/entries"
    Then the response status is 404

  Scenario: Create expense entry returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client POSTs "/api/cars/{carId}/entries" with:
      | Type        | "Service"    |
      | Title       | "Oil change" |
      | Price       | 100.00       |
      | PerformedAt | "2026-01-01"  |
      | KmAtService | 1000         |
    Then the response status is 404

  Scenario: Update expense entry returns not found when entry does not exist
    Given a registered car exists with id "{carId}"
    And no expense entry exists with id "{entryId}" for that car
    When the client PATCHes "/api/cars/{carId}/entries/{entryId}" with:
      | Price | 120.00 |
    Then the response status is 404

  Scenario Outline: Reject invalid expense entry creation (<code>)
    Given a registered car exists with id "{carId}"
    When the client POSTs "/api/cars/{carId}/entries" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code                | field       | payload                                                                 |
      | TITLE_REQUIRED      | Title       | { "type":"Service","title":"   ","price":10,"performedAt":"2026-01-01","kmAtService":1 } |
      | PRICE_INVALID       | Price       | { "type":"Service","title":"Oil","price":-1,"performedAt":"2026-01-01","kmAtService":1 } |
      | KM_AT_SERVICE_INVALID | KmAtService | { "type":"Service","title":"Oil","price":10,"performedAt":"2026-01-01","kmAtService":-1 } |

  Scenario Outline: Reject invalid expense entry update (<code>)
    Given a registered car exists with an expense entry "{entryId}"
    When the client PATCHes "/api/cars/{carId}/entries/{entryId}" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code                | field       | payload            |
      | TITLE_EMPTY         | Title       | { "title": "   " } |
      | PRICE_INVALID       | Price       | { "price": -1 }    |
      | KM_AT_SERVICE_INVALID | KmAtService | { "kmAtService": -1 } |

