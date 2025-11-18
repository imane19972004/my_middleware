Feature: End-to-End Order Placement and Confirmation

  As a student user
  I want to select items from a restaurant menu, place an order, pay for it, and have it confirmed
  So that my food order is processed correctly

  Background:
    Given a student named "Jordan Smith" with ID "ID-42" and email "jordan.smith@etu.unice.fr" exists
    And the student has a saved delivery location "Home" at "123 Main St, Nice, 06000"
    And the student has bank info "1234 5678 9012 3456", CVV 123, expiring "12/30"
    And the student has an internal balance of 30.00 euros
    And a restaurant named "Pizza Palace" exists
    And "Pizza Palace" offers the following dishes:
      | name             | description                       | price |
      | Margherita Pizza | Classic tomato and cheese pizza | 12.50 |
      | Coke             | Carbonated soft drink           | 2.00  |
    And "Pizza Palace" has an available time slot "12:00-12:30" with capacity 5

  Scenario: Successful order using External Payment
    Given Jordan has selected the following items from "Pizza Palace":
      | item             | quantity |
      | Margherita Pizza | 1        |
      | Coke             | 1        |
    When Jordan creates an order for "Pizza Palace" with delivery to "Home"
    And Jordan chooses a time slot from those that are available
    And the external payment system approves the payment on the first attempt
    And Jordan initiates the payment for the order using EXTERNAL method
    Then a validated order should exist for Jordan with total amount 14.50
    And the order status should become VALIDATED
    And the order should be registered successfully with "Pizza Palace"
    And the time slot should remain blocked

  Scenario: Failed order using External Payment (Payment Declined)
    Given Jordan has selected the following items from "Pizza Palace":
      | item             | quantity |
      | Margherita Pizza | 1        |
      | Coke             | 1        |
    When Jordan creates an order for "Pizza Palace" with delivery to "Home"
    And Jordan chooses a time slot from those that are available
    And the external payment system rejects the payment on all attempts
    And Jordan initiates the payment for the order using EXTERNAL method
    Then the payment attempt should fail due to external decline
    And the order status should become CANCELED
    And Jordan's balance should remain 30.00 euros
    And the order should not be registered with "Pizza Palace"
    And the time slot should not remain blocked

  Scenario: Successful order using Internal Payment (Sufficient Balance)
    Given Jordan has selected the following items from "Pizza Palace":
      | item             | quantity |
      | Margherita Pizza | 1        |
      | Coke             | 1        |
    When Jordan creates an order for "Pizza Palace" with delivery to "Home"
    And Jordan chooses a time slot from those that are available
    And Jordan initiates the payment for the order using INTERNAL method
    Then a validated order should exist for Jordan with total amount 14.50
    And the payment should be debited from Jordan's balance
    And Jordan's balance should be 15.50 euros
    And the order status should become VALIDATED
    And the order should be registered successfully with "Pizza Palace"
    And the time slot should remain blocked

  Scenario: Failed order using Internal Payment (Insufficient Balance)
    Given Jordan has selected the following items from "Pizza Palace":
      | item             | quantity |
      | Margherita Pizza | 2        |
      | Coke             | 3        |
    When Jordan creates an order for "Pizza Palace" with delivery to "Home"
    And Jordan chooses a time slot from those that are available
    And Jordan initiates the payment for the order using INTERNAL method
    Then the payment attempt should fail due to insufficient balance
    And Jordan's balance should remain 30.00 euros
    And the order status should become CANCELED
    And the order should not be registered with "Pizza Palace"
    And the time slot should not remain blocked
