Feature: Reports - Cost per km

  User Story:
    As a vehicle owner
    I want a cost-per-km report
    So that I can understand how much the car costs to operate

  Business rules:
    - basis must be "period" or "lifetime" (BASIS_INVALID)
    - if basis="period", period must be valid (PERIOD_INVALID)
    - distanceRef must be valid (DISTANCE_REF_INVALID)
    - if the car does not exist, return 404

  Scenario: Report returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/reports/cost-per-km?basis=lifetime&distanceRef=total"
    Then the response status is 404

  Scenario Outline: Reject invalid query parameters (<code>)
    Given a registered car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/reports/cost-per-km?basis=<basis>&period=<period>&distanceRef=<distanceRef>"
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code               | field       | basis     | period | distanceRef |
      | BASIS_INVALID      | Basis       | bad       | 1m     | total       |
      | PERIOD_INVALID     | Period      | period    | bad    | total       |
      | DISTANCE_REF_INVALID | DistanceRef | lifetime  |        | bad         |

