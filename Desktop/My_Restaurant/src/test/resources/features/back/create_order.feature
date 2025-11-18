Feature: Order creation and validation
  As a student
  I want to create an order from a restaurant
  So that I can get my meal delivered

  Background:
    Given a client named "Alex"
    And "Alex" has a student credit balance of "30.00"

  Scenario: Successful order creation with external payment
    Given Alex has the following items in the cart:
      | item   | unit price |
      | Burger | 8.50       |
      | Fries  | 3.00       |
    When Alex selects the delivery location "10 Rue de France, Nice"
    And Alex chooses the saved payment method "Visa"
    And Alex confirms the order
    Then the order should be created with status "VALIDATED"
    And Alex should see the order total of "11.50"
    And Alex should receive an order confirmation notification


     #  Invalid delivery location
  Scenario: Order creation fails when delivery location not saved
    Given Alex has the following items in the cart:
      | item   | unit price |
      | Pizza  | 9.00       |
    When Alex selects the delivery location "Unknown Street, Nice"
    And Alex chooses the saved payment method "Visa"
    And Alex confirms the order
    Then an error should be raised with message containing "Missing delivery address"