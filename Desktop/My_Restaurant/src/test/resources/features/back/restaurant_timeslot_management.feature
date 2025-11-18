Feature: Restaurant TimeSlot Management
  As a restaurant manager
  I want to manage available time slots for orders
  So that I can control order capacity throughout the day

  Background:
    Given a restaurant "Le Bistrot" exists
    And the restaurant manager is logged in to "Le Bistrot"

  Scenario: Define available time slots with capacity
    When the restaurant manager creates a time slot from "12:00" to "12:30" with capacity 10
    And the restaurant manager creates a time slot from "12:30" to "13:00" with capacity 15
    Then the restaurant should have 2 time slots available
    And the time slot "12:00-12:30" should have capacity 10
    And the time slot "12:30-13:00" should have capacity 15

  Scenario: Block a time slot when capacity is full
    Given a time slot from "12:00" to "12:30" exists with capacity 3
    When the restaurant manager blocks the time slot "12:00-12:30" 3 times
    Then the time slot "12:00-12:30" should have capacity 0
    And the time slot "12:00-12:30" should not be available for booking

  Scenario: Unblock a previously blocked time slot
    Given a time slot from "12:00" to "12:30" exists with capacity 0
    When the restaurant manager unblocks the time slot "12:00-12:30"
    Then the time slot "12:00-12:30" should have capacity greater than 0
    And the time slot "12:00-12:30" should be available for booking

  Scenario: Prevent negative capacity
    Given a time slot from "12:00" to "12:30" exists with capacity 1
    When the restaurant manager blocks the time slot "12:00-12:30" 5 times
    Then the time slot "12:00-12:30" should have capacity 0
    And the system should not allow negative capacity

  Scenario: View all available time slots for a specific day
    Given a time slot from "11:30" to "12:00" exists with capacity 5
    And a time slot from "12:00" to "12:30" exists with capacity 0
    And a time slot from "12:30" to "13:00" exists with capacity 10
    When the restaurant manager requests all available time slots
    Then the restaurant manager should see 2 available time slots
    And the list should include "11:30-12:00"
    And the list should include "12:30-13:00"
    But the list should not include "12:00-12:30"

  Scenario: Update time slot capacity
    Given a time slot from "12:00" to "12:30" exists with capacity 10
    When the restaurant manager updates the time slot "12:00-12:30" capacity to 20
    Then the time slot "12:00-12:30" should have capacity 20

  Scenario: Restaurant with no time slots configured
    Given the restaurant has no time slots configured
    When the restaurant manager requests all available time slots
    Then the restaurant manager should see 0 available time slots
    And a warning message "No time slots configured" should be displayed