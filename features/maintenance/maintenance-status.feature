Feature: Maintenance - Maintenance status

  User Story:
    As a vehicle owner
    I want the system to show me which maintenance items are due or overdue
    So that I can take action before problems happen

  Business rules:
    - Only active plan items are included.
    - Next due date and/or km are calculated from a baseline:
      - last service expense entry that matches the plan title (case-insensitive), otherwise the car baseline.
    - Overdue rules:
      - overdue by time when NextDueDate is on or before today (UTC)
      - overdue by km when NextDueKm is on or before current car km

  Scenario: Get maintenance status returns not found when car does not exist
    Given no car exists with id "{carId}"
    When the client GETs "/api/cars/{carId}/maintenance-status"
    Then the response status is 404

  Scenario: Maintenance status lists status items for active plans
    Given a registered car exists with id "{carId}"
    And the car has at least one active maintenance plan item
    When the client GETs "/api/cars/{carId}/maintenance-status"
    Then the response status is 200
    And the response JSON is an array of MaintenanceStatusDto items
    And each item contains:
      | planItemId |
      | title      |
      | overdue    |

