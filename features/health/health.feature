Feature: Health - Service health check

  Scenario: Service is healthy
    When the client GETs "/api/health"
    Then the response status is 200
    And the response JSON contains:
      | status | "ok" |

