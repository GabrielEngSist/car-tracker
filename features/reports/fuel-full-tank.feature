Feature: Reports - Fuel full tank efficiency

  User Story:
    As a vehicle owner
    I want a full-tank efficiency report
    So that I can track consumption trends over time

  Business rules:
    - basis must be "period" or "lifetime" (BASIS_INVALID)
    - if basis="period", period must be valid (PERIOD_INVALID)
    - if the car does not exist, return 404

  Scenario: Report returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/reports/fuel-full-tank?basis=lifetime"
    Then the response status is 404

  Scenario Outline: Reject invalid query parameters (<code>)
    Given a registered car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/reports/fuel-full-tank?basis=<basis>&period=<period>"
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code           | field  | basis  | period |
      | BASIS_INVALID  | Basis  | bad    | 1m     |
      | PERIOD_INVALID | Period | period | bad    |

