Feature: Restaurant Dish Management
  As a restaurant manager
  I want to manage my dishes
  So that customers can see my current menu

  Background:
    Given a restaurant "Pizza House" exists
    And the restaurant manager is logged in to "Pizza House"

  Scenario: Add a new dish successfully
    When the restaurant manager adds a new dish with the following details:
      | name        | Margherita Pizza           |
      | description | Classic tomato and cheese  |
      | price       | 12.50                      |
      | category    | Main Course                |
      | type        | Pizza                      |
    Then the dish "Margherita Pizza" should be added to the menu
    And the dish should have price 12.50 euros

  Scenario: Add a dish with dietary tags
    When the restaurant manager adds a new dish with the following details:
      | name        | Vegan Salad       |
      | description | Fresh vegetables  |
      | price       | 8.00              |
      | category    | Starter           |
    And the restaurant manager tags the dish as "vegetarian" and "vegan"
    Then the dish "Vegan Salad" should have tag "vegetarian"
    And the dish "Vegan Salad" should have tag "vegan"

  Scenario: Add paid toppings to a dish
    Given a dish "Margherita Pizza" exists with price 12.50
    When the restaurant manager adds a topping "Extra Cheese" with price 2.00
    And the restaurant manager adds a topping "Olives" with price 1.50
    Then the dish should have 2 toppings available
    And topping "Extra Cheese" should cost 2.00 euros

  Scenario: Update an existing dish
    Given a dish "Margherita Pizza" exists with price 12.50
    When the restaurant manager updates the dish price to 13.00
    And the restaurant manager updates the description to "Classic Italian pizza"
    Then the dish "Margherita Pizza" should have price 13.00
    And the dish description should be "Classic Italian pizza"

  Scenario: Remove a dish from menu
    Given a dish "Old Pasta" exists in the menu
    When the restaurant manager removes the dish "Old Pasta" from the menu
    Then the dish "Old Pasta" should not be available
    And customers should not see "Old Pasta" in the menu

  Scenario: Add extra options to restaurant
    When the restaurant manager defines an extra option "Fries portion" with price 3.50
    And the restaurant manager defines an extra option "Fruit compote" with price 2.00
    Then the restaurant should have 2 extra options available
    And option "Fries portion" should cost 3.50 euros

  Scenario: Dish with allergen information
    When the restaurant manager adds a new dish with the following details:
      | name        | Peanut Cookies        |
      | description | Homemade cookies      |
      | price       | 4.50                  |
      | category    | Dessert               |
    And the restaurant manager adds allergen information "May contain traces of peanuts"
    Then the dish should display allergen warning
    And the warning should mention "peanuts"