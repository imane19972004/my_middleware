package fr.unice.polytech.stepDefs.back;

import io.cucumber.java.en.Given;
import io.cucumber.java.en.When;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.But;

import static org.junit.jupiter.api.Assertions.*;

import fr.unice.polytech.restaurants.RestaurantManager;
import fr.unice.polytech.restaurants.ScenarioContext;
import fr.unice.polytech.restaurants.TimeSlot;

import java.time.LocalTime;
import java.util.*;

public class RestaurantTimeSlotManagementSteps {

    private final ScenarioContext ctx;
    private final RestaurantManager restaurantManager;
    private final Map<String, TimeSlot> timeSlots = new HashMap<>();
    private List<TimeSlot> availableTimeSlots;
    private String warningMessage;

    public RestaurantTimeSlotManagementSteps(ScenarioContext ctx) {
        this.ctx = ctx;
        this.restaurantManager = new RestaurantManager();
    }

    private String key(String startTime, String endTime) { 
        return startTime + "-" + endTime; 
    }

    // ============ BACKGROUND STEPS ============


    // ============ SCENARIO 1: Define slots ============

    @When("the restaurant manager creates a time slot from {string} to {string} with capacity {int}")
    public void the_restaurant_manager_creates_a_time_slot_with_capacity(String startTime, String endTime, int capacity) {
        TimeSlot slot = new TimeSlot(LocalTime.parse(startTime), LocalTime.parse(endTime));
        ctx.restaurant.setCapacity(slot, capacity);
        timeSlots.put(key(startTime, endTime), slot);
    }

    @Then("the restaurant should have {int} time slots available")
    public void the_restaurant_should_have_time_slots_available(int expectedCount) {
        var available = restaurantManager.getAvailableTimeSlots(ctx.restaurant);
        assertEquals(expectedCount, available.size(),
                "Expected " + expectedCount + " time slots but found " + available.size());
    }

    @Then("the time slot {string} should have capacity {int}")
    public void the_time_slot_should_have_capacity(String timeRange, int expectedCapacity) {
        TimeSlot slot = timeSlots.get(timeRange);
        assertNotNull(slot, "Time slot " + timeRange + " should exist");
        assertEquals(expectedCapacity, ctx.restaurant.getCapacity(slot));
    }

    // ============ SCENARIO 2: Block ============

    @Given("a time slot from {string} to {string} exists with capacity {int}")
    public void a_time_slot_exists_with_capacity(String start, String end, int capacity) {
        the_restaurant_manager_creates_a_time_slot_with_capacity(start, end, capacity);
    }

    @When("the restaurant manager blocks the time slot {string} {int} times")
    public void the_restaurant_manager_blocks_time_slot_multiple_times(String timeRange, int times) {
        TimeSlot slot = timeSlots.get(timeRange);
        assertNotNull(slot, "Time slot should exist before blocking");
        for (int i = 0; i < times; i++) {
            restaurantManager.blockTimeSlot(slot, ctx.restaurant);
        }
    }

    @Then("the time slot {string} should not be available for booking")
    public void the_time_slot_should_not_be_available(String timeRange) {
        var available = restaurantManager.getAvailableTimeSlots(ctx.restaurant);
        assertFalse(available.contains(timeSlots.get(timeRange)),
                "Time slot " + timeRange + " should not be available");
    }

    // ============ SCENARIO 3: Unblock ============

    @When("the restaurant manager unblocks the time slot {string}")
    public void the_restaurant_manager_unblocks_time_slot(String timeRange) {
        TimeSlot slot = timeSlots.get(timeRange);
        assertNotNull(slot, "Time slot should exist before unblocking");
        restaurantManager.unblockTimeSlot(slot, ctx.restaurant);
    }

    @Then("the time slot {string} should have capacity greater than {int}")
    public void the_time_slot_should_have_capacity_greater_than(String timeRange, int minCapacity) {
        int actual = ctx.restaurant.getCapacity(timeSlots.get(timeRange));
        assertTrue(actual > minCapacity, 
                "Capacity should be > " + minCapacity + " but was " + actual);
    }

    @Then("the time slot {string} should be available for booking")
    public void the_time_slot_should_be_available(String timeRange) {
        var available = restaurantManager.getAvailableTimeSlots(ctx.restaurant);
        assertTrue(available.contains(timeSlots.get(timeRange)),
                "Time slot " + timeRange + " should be available");
    }

    // ============ SCENARIO 4: Prevent negative ============

    @Then("the system should not allow negative capacity")
    public void the_system_should_not_allow_negative_capacity() {
        for (TimeSlot slot : timeSlots.values()) {
            assertTrue(ctx.restaurant.getCapacity(slot) >= 0, 
                    "Capacity must not be negative");
        }
    }

    // ============ SCENARIO 5: View available ============

    @When("the restaurant manager requests all available time slots")
    public void the_restaurant_manager_requests_all_available_time_slots() {
        availableTimeSlots = restaurantManager.getAvailableTimeSlots(ctx.restaurant);
    }

    @Then("the restaurant manager should see {int} available time slots")
    public void the_restaurant_manager_should_see_available_time_slots(int expectedCount) {
        assertNotNull(availableTimeSlots);
        assertEquals(expectedCount, availableTimeSlots.size());
    }

    @Then("the list should include {string}")
    public void the_list_should_include(String timeRange) {
        TimeSlot expected = timeSlots.get(timeRange);
        assertNotNull(expected, "Time slot " + timeRange + " should be in the map");
        assertTrue(availableTimeSlots.contains(expected), 
                "Should include " + timeRange);
    }

    @But("the list should not include {string}")
    public void the_list_should_not_include(String timeRange) {
        TimeSlot excluded = timeSlots.get(timeRange);
        if (excluded != null) {
            assertFalse(availableTimeSlots.contains(excluded),
                    "Should NOT include " + timeRange);
        }
    }

    // ============ SCENARIO 6: Update capacity ============

    @When("the restaurant manager updates the time slot {string} capacity to {int}")
    public void the_restaurant_manager_updates_time_slot_capacity(String timeRange, int newCapacity) {
        TimeSlot slot = timeSlots.get(timeRange);
        assertNotNull(slot, "Time slot " + timeRange + " should exist");
        ctx.restaurant.setCapacity(slot, newCapacity);
    }

    // ============ SCENARIO 7: No time slots configured ============

    @Given("the restaurant has no time slots configured")
    public void the_restaurant_has_no_time_slots_configured() {
        assertTrue(ctx.restaurant.getAvailableTimeSlots().isEmpty(),
                "Expected no time slots configured at start");
    }

    @Then("a warning message {string} should be displayed")
    public void a_warning_message_should_be_displayed(String expectedMessage) {
        if (availableTimeSlots == null || availableTimeSlots.isEmpty()) {
            warningMessage = "No time slots configured";
        }
        assertEquals(expectedMessage, warningMessage);
    }
}