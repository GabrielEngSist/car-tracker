Feature: Cars - Create car

  User Story:
    As a vehicle owner
    I want to create a car
    So that I can track fuelings, expenses and maintenance

  Business rules:
    - Manual creation uses the request fields directly.
    - Automatic registration ("AutoBuscarDados") requires a valid plate and calls external providers.
    - Some invalid cases return a plain bad request message (not the fault envelope).

  Scenario: Create a car manually
    When the client POSTs "/api/cars" with:
      | Model           | "Corolla" |
      | Year            | 2018      |
      | CurrentKm       | 12345     |
      | Name            | "Daily"   |
      | Placa           | "ABC1D23" |
      | AutoBuscarDados | false     |
    Then the response status is 201
    And the response body is a CarDto

  Scenario: Manual creation returns 400 when model is missing
    When the client POSTs "/api/cars" with:
      | Model           | null  |
      | Year            | 2018  |
      | CurrentKm       | 0     |
      | AutoBuscarDados | false |
    Then the response status is 400

  Scenario: Manual creation returns 400 when year is invalid
    When the client POSTs "/api/cars" with:
      | Model           | "Corolla" |
      | Year            | 1800      |
      | CurrentKm       | 0         |
      | AutoBuscarDados | false     |
    Then the response status is 400

  Scenario: Manual creation returns 400 when plate is invalid
    When the client POSTs "/api/cars" with:
      | Model           | "Corolla" |
      | Year            | 2018      |
      | CurrentKm       | 0         |
      | Placa           | "INVALID" |
      | AutoBuscarDados | false     |
    Then the response status is 400

  Scenario: Automatic registration creates a car (happy path)
    Given external providers for plate registration return "ok"
    When the client POSTs "/api/cars" with:
      | Placa           | "ABC1D23" |
      | CurrentKm       | 1000      |
      | AutoBuscarDados | true      |
    Then the response status is 201
    And the response body is a CarDto

  Scenario: Automatic registration returns 400 when plate is missing
    When the client POSTs "/api/cars" with:
      | Placa           | ""   |
      | CurrentKm       | 1000 |
      | AutoBuscarDados | true |
    Then the response status is 400

  Scenario: Automatic registration returns 502 when providers are unreachable
    Given external providers are unreachable
    When the client POSTs "/api/cars" with:
      | Placa           | "ABC1D23" |
      | CurrentKm       | 1000      |
      | AutoBuscarDados | true      |
    Then the response status is 502

