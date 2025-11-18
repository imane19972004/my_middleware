package fr.unice.polytech.restaurants;

import io.cucumber.java.en.Given;

import static org.junit.jupiter.api.Assertions.*; // <-- remplace l'import JUnit4

public class CommonSteps {
    private final ScenarioContext ctx;

    public CommonSteps(ScenarioContext ctx) { this.ctx = ctx; }

    @Given("a restaurant {string} exists")
    public void a_restaurant_exists(String restaurantName) {
        ctx.restaurant = new Restaurant(restaurantName);
    }

    @Given("the restaurant manager is logged in to {string}")
    public void the_restaurant_manager_is_logged_in_to(String restaurantName) {
        assertNotNull(ctx.restaurant);
        assertEquals(restaurantName, ctx.restaurant.getRestaurantName());
        ctx.managerLoggedIn = true;
    }

}
