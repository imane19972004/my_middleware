Feature: Hybrid Suggestion Engine for Dish Creation
  As a restaurant owner,
  I want intelligent suggestions when entering a new dish,
  So that I can save time and ensure consistency.

  Scenario: Initial suggestions are based on keywords
    Given the suggestion system has a predefined keyword for "burger"
    When the owner types "bur" to create a new dish
    Then the system should suggest a dish named "Burger" with the description "An American classic"

  Scenario: The system learns from previously added dishes to enhance suggestions
    Given a dish named "Bacon Burger" with the description "A tasty burger with crispy bacon" has been added to the system
    When the owner types "bur" to create a new dish
    Then the system should suggest a dish named "Bacon Burger" with the description "A tasty burger with crispy bacon" based on historical data