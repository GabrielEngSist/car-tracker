Feature: Faults - Fault envelope contract

  Scenario: Validation or business fault returns the fault envelope
    When the client sends a request that fails validation or a business rule
    Then the response status is 400
    And the response content type is "application/json"
    And the response JSON contains:
      | type  | "https://tools.ietf.org/html/rfc9110#section-15.5.1" |
      | title | "Request failed validation or could not be processed." |
    And the response JSON contains "faults" as an array
    And each "faults[]" item contains:
      | code         |
      | propertyName |
      | message      |

  Scenario: Unexpected fault returns the fault envelope and status 500
    When the client sends a request that triggers an unexpected server error
    Then the response status is 500
    And the response content type is "application/json"
    And the response JSON contains:
      | type  | "https://tools.ietf.org/html/rfc9110#section-15.6.1" |
      | title | "An unexpected error occurred while processing the request." |
    And the response JSON contains "faults" as an array
    And the response JSON contains a fault with:
      | code | "UNEXPECTED" |

