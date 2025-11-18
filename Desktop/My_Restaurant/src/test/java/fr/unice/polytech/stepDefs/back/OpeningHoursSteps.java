package fr.unice.polytech.stepDefs.back;

import fr.unice.polytech.restaurants.OpeningHours;
import fr.unice.polytech.restaurants.Restaurant;
import io.cucumber.java.en.*;

import java.time.DayOfWeek;
import java.time.LocalTime;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

public class OpeningHoursSteps {

    private Restaurant restaurant;
    private Exception thrownException;


    @Given("a restaurant named {string}")
    public void a_restaurant_named(String name) {
        this.restaurant = new Restaurant(name);
    }

    @Given("the restaurant has no opening hours for {string}")
    public void the_restaurant_has_no_opening_hours_for(String day) {
        DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
        boolean hasHours = restaurant.getOpeningHours().stream().anyMatch(oh -> oh.getDay() == dayEnum);
        assertFalse(hasHours, "The restaurant should not have hours for " + day + " at this stage.");
    }

    @Given("the restaurant is open on {string} from {string} to {string}")
    public void the_restaurant_is_open_on_from(String day, String start, String end) {
        DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
        OpeningHours hours = new OpeningHours(dayEnum, LocalTime.parse(start), LocalTime.parse(end));
        restaurant.addOpeningHours(hours);
    }

    @When("the owner adds opening hours for {string} from {string} to {string}")
    public void the_owner_adds_opening_hours_for_from(String day, String start, String end) {
        try {
            DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
            OpeningHours hours = new OpeningHours(dayEnum, LocalTime.parse(start), LocalTime.parse(end));
            restaurant.addOpeningHours(hours);
        } catch (IllegalArgumentException e) {
            this.thrownException = e;
        }
    }

    @When("the owner updates the opening hours for {string} to be from {string} to {string}")
    public void the_owner_updates_the_opening_hours_for_to_be_from(String day, String start, String end) {
        try {
            DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
            OpeningHours hours = new OpeningHours(dayEnum, LocalTime.parse(start), LocalTime.parse(end));
            restaurant.updateOpeningHours(hours);
        } catch (IllegalArgumentException e) {
            this.thrownException = e;
        }
    }

    @When("the owner tries to add new opening hours for {string} from {string} to {string}")
    public void the_owner_tries_to_add_new_opening_hours_for_from(String day, String start, String end) {
        try {
            DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
            OpeningHours hours = new OpeningHours(dayEnum, LocalTime.parse(start), LocalTime.parse(end));
            this.restaurant.addOpeningHours(hours);
        } catch (IllegalArgumentException e) {
            this.thrownException = e;
        }
    }

    @When("the owner tries to update the opening hours for {string} from {string} to {string}")
    public void the_owner_tries_to_update_the_opening_hours_for_from(String day, String start, String end) {
        try {
            DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
            OpeningHours hours = new OpeningHours(dayEnum, LocalTime.parse(start), LocalTime.parse(end));
            this.restaurant.updateOpeningHours(hours);
        } catch (IllegalArgumentException e) {
            this.thrownException = e;
        }
    }

    @Then("the restaurant schedule should contain hours for {string} from {string} to {string}")
    public void the_restaurant_schedule_should_contain_hours_for_from(String day, String start, String end) {
        the_restaurant_schedule_for_should_be_from_to(day, start, end);
    }

    @Then("the restaurant schedule for {string} should be from {string} to {string}")
    public void the_restaurant_schedule_for_should_be_from_to(String day, String startTime, String endTime) {
        DayOfWeek dayEnum = DayOfWeek.valueOf(day.toUpperCase());
        LocalTime expectedStart = LocalTime.parse(startTime);
        LocalTime expectedEnd = LocalTime.parse(endTime);

        Optional<OpeningHours> result = this.restaurant.getOpeningHours().stream()
                .filter(oh -> oh.getDay() == dayEnum)
                .findFirst();

        assertTrue(result.isPresent(), "No opening hours found for " + day);
        assertEquals(expectedStart, result.get().getOpeningTime(), "The opening time is incorrect.");
        assertEquals(expectedEnd, result.get().getClosingTime(), "The closing time is incorrect.");
    }

    @Then("the system should reject the operation with an error message containing {string}")
    public void the_system_should_reject_the_operation_with_an_error_message_containing(String expectedErrorMessage) {
        assertNotNull(this.thrownException, "Expected an exception to be thrown, but it wasn't.");
        assertTrue(
                this.thrownException.getMessage().contains(expectedErrorMessage),
                "The exception message was: '" + this.thrownException.getMessage() + "'"
        );
    }
}