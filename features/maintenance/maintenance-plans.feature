Feature: Maintenance - Maintenance plans for a car

  User Story:
    As a vehicle owner
    I want to manage maintenance plan items for a car
    So that the system can calculate what is due or overdue

  Business rules:
    - Title is required on creation.
    - At least one interval must be provided (km and/or days).
    - Intervals must be > 0 when provided.

  Scenario: List maintenance plans returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/maintenance-plans"
    Then the response status is 404

  Scenario: Create plan item returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client POSTs "/api/cars/{carId}/maintenance-plans" with:
      | Title              | "Oil change" |
      | DueKmInterval      | 10000        |
      | DueTimeIntervalDays| null         |
      | Active             | true         |
    Then the response status is 404

  Scenario Outline: Reject invalid plan item creation (<code>)
    Given a registered car exists with id "{carId}"
    When the client POSTs "/api/cars/{carId}/maintenance-plans" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code              | field               | payload                               |
      | TITLE_REQUIRED    | Title               | { "title":"   ","active":true }       |
      | INTERVAL_REQUIRED | DueKmInterval       | { "title":"Oil","active":true }       |
      | DUE_KM_INVALID    | DueKmInterval       | { "title":"Oil","dueKmInterval":0,"active":true } |
      | DUE_DAYS_INVALID  | DueTimeIntervalDays | { "title":"Oil","dueTimeIntervalDays":0,"active":true } |

  Scenario Outline: Reject invalid plan item update (<code>)
    Given a registered car exists with a maintenance plan item "{planId}"
    When the client PATCHes "/api/cars/{carId}/maintenance-plans/{planId}" with <payload>
    Then the response status is 400
    And the response JSON contains faults including:
      | code   | propertyName |
      | <code> | <field>      |

    Examples:
      | code              | field          | payload                 |
      | TITLE_EMPTY       | Title          | { "title": "   " }      |
      | DUE_KM_INVALID    | DueKmInterval  | { "dueKmInterval": 0 }  |
      | DUE_DAYS_INVALID  | DueTimeIntervalDays | { "dueTimeIntervalDays": 0 } |
      | INTERVAL_REQUIRED | DueKmInterval  | { "dueKmInterval": null, "dueTimeIntervalDays": null } |

