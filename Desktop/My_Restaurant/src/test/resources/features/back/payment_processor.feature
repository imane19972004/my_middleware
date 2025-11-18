Feature: Payment validation for an order
  In order to validate or not of an order
  I want to receive the final decision from the payment process

  Background:
    Given a pending order
    And a payment processor

  Scenario: Payment accepted on the first attempt
    When the external provider approves first payment request
    Then the payment processor validate the payment

  Scenario: Payment initially rejected then accepted on the second attempt
    When the external provider approves the second payment attempt after rejecting the first one
    Then the payment processor validate the payment

  Scenario: Payment rejected twice then accepted on the third attempt
    When the external provider approves the third payment attempt after rejecting the firsts ones
    Then the payment processor validate the payment

  Scenario: Payment permanently rejected after three attempts
    When the external provider reject the third payment attempt after rejecting the firsts ones
    Then the payment processor cancels the payment