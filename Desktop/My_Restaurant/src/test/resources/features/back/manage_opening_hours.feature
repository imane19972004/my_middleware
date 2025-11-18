Feature: Manage Restaurant Opening Hours
  As a restaurant owner
  I want to define and modify the opening hours for each day of the week
  So that the schedule is always up to date

  Background:
    Given a restaurant named "Le Gourmet Test"

  Scenario: Successfully add opening hours for a new day
    Given the restaurant has no opening hours for "MONDAY"
    When the owner adds opening hours for "MONDAY" from "12:00" to "14:30"
    Then the restaurant schedule should contain hours for "MONDAY" from "12:00" to "14:30"

  Scenario: Successfully update existing opening hours
    Given the restaurant is open on "TUESDAY" from "19:00" to "22:00"
    When the owner updates the opening hours for "TUESDAY" to be from "18:30" to "23:00"
    Then the restaurant schedule for "TUESDAY" should be from "18:30" to "23:00"

  Scenario: Prevent adding hours for a day that is already scheduled
    Given the restaurant is open on "WEDNESDAY" from "10:00" to "12:00"
    When the owner tries to add new opening hours for "WEDNESDAY" from "09:00" to "11:00"
    Then the system should reject the operation with an error message containing "already exist"

  Scenario: Prevent updating hours for a day that is not scheduled
    Given the restaurant has no opening hours for "THURSDAY"
    When the owner tries to update the opening hours for "THURSDAY" from "09:00" to "11:00"
    Then the system should reject the operation with an error message containing "No entry found for"