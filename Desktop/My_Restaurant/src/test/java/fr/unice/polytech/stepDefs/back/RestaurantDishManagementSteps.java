package fr.unice.polytech.stepDefs.back;

import fr.unice.polytech.restaurants.ScenarioContext;
import io.cucumber.java.en.*;
import io.cucumber.datatable.DataTable;

import static org.junit.jupiter.api.Assertions.*;

import fr.unice.polytech.dishes.*;
import java.util.*;

public class RestaurantDishManagementSteps {
    private final ScenarioContext ctx;

    public RestaurantDishManagementSteps(ScenarioContext ctx) {
        this.ctx = ctx;
    }

    // On mémorise juste le dernier plat manipulé
    private String lastDishName;
    private final List<String> currentDishTags = new ArrayList<>();
    private String currentAllergenInfo;
    private final Map<String, Double> extraOptions = new HashMap<>();

    // ============ BACKGROUND STEPS ============



    // ============ SCENARIO 1: Add a new dish ============

    @When("the restaurant manager adds a new dish with the following details:")
    public void the_restaurant_manager_adds_a_new_dish_with_details(DataTable dataTable) {
        Map<String, String> dishData = dataTable.asMap(String.class, String.class);

        String name = dishData.get("name");
        String description = dishData.get("description");
        double price = Double.parseDouble(dishData.get("price"));

        ctx.restaurant.addDish(name, description, price);
        lastDishName = name;

        if (dishData.containsKey("category")) {
            Dish dish = ctx.restaurant.findDishByName(lastDishName);
            assertNotNull(dish, "Dish should exist after being added");
            String category = dishData.get("category").replace(" ", "_").toUpperCase();
            dish.setCategory(DishCategory.valueOf(category));
        }
    }

    @Then("the dish {string} should be added to the menu")
    public void the_dish_should_be_added_to_menu(String dishName) {
        Dish dish = ctx.restaurant.findDishByName(dishName);
        assertNotNull(dish, "Dish should be found in the menu");
        assertEquals(dishName, dish.getName());
    }

    @Then("the dish should have price {double} euros")
    public void the_dish_should_have_price_euros(double expectedPrice) {
        assertNotNull(lastDishName, "A dish should have been created earlier");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Last dish should exist");
        assertEquals(expectedPrice, dish.getPrice(), 0.01);
    }

    // ============ SCENARIO 2: Dietary tags ============

    @When("the restaurant manager tags the dish as {string} and {string}")
    public void the_restaurant_manager_tags_the_dish_as(String tag1, String tag2) {
        assertNotNull(lastDishName, "A dish should have been created earlier");
        currentDishTags.clear();
        currentDishTags.add(tag1);
        currentDishTags.add(tag2);
    }

    @Then("the dish {string} should have tag {string}")
    public void the_dish_should_have_tag(String dishName, String expectedTag) {
        Dish dish = ctx.restaurant.findDishByName(dishName);
        assertNotNull(dish, "Dish should exist");
        assertTrue(currentDishTags.contains(expectedTag),
                "Tag '" + expectedTag + "' should be present");
    }

    // ============ SCENARIO 3: Toppings ============

    @Given("a dish {string} exists with price {double}")
    public void a_dish_exists_with_price(String dishName, double price) {
        ctx.restaurant.addDish(dishName, "Test dish", price);
        lastDishName = dishName;
    }

    @When("the restaurant manager adds a topping {string} with price {double}")
    public void the_restaurant_manager_adds_a_topping_with_price(String toppingName, double price) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        dish.addTopping(new Topping(toppingName, price));
    }

    @Then("the dish should have {int} toppings available")
    public void the_dish_should_have_toppings_available(int expectedCount) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        assertEquals(expectedCount, dish.getToppings().size());
    }

    @Then("topping {string} should cost {double} euros")
    public void topping_should_cost_euros(String toppingName, double expectedPrice) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        Topping topping = dish.getToppings().stream()
                .filter(t -> t.getName().equals(toppingName))
                .findFirst().orElse(null);
        assertNotNull(topping, "Topping '" + toppingName + "' should exist");
        assertEquals(expectedPrice, topping.getPrice(), 0.01);
    }

    // ============ SCENARIO 4: Update dish ============

    @When("the restaurant manager updates the dish price to {double}")
    public void the_restaurant_manager_updates_the_dish_price_to(double newPrice) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        dish.setPrice(newPrice);
    }

    @When("the restaurant manager updates the description to {string}")
    public void the_restaurant_manager_updates_the_description_to(String newDescription) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        dish.setDescription(newDescription);
    }

    @Then("the dish {string} should have price {double}")
    public void the_dish_should_have_price(String dishName, double expectedPrice) {
        Dish dish = ctx.restaurant.findDishByName(dishName);
        assertNotNull(dish, "Dish should be found");
        assertEquals(expectedPrice, dish.getPrice(), 0.01);
    }

    @Then("the dish description should be {string}")
    public void the_dish_description_should_be(String expectedDescription) {
        assertNotNull(lastDishName, "No current dish context");
        Dish dish = ctx.restaurant.findDishByName(lastDishName);
        assertNotNull(dish, "Dish should exist");
        assertEquals(expectedDescription, dish.getDescription());
    }

    // ============ SCENARIO 5: Remove dish ============

    @Given("a dish {string} exists in the menu")
    public void a_dish_exists_in_the_menu(String dishName) {
        ctx.restaurant.addDish(dishName, "Test dish", 10.00);
        lastDishName = dishName;
    }

    @When("the restaurant manager removes the dish {string} from the menu")
    public void the_restaurant_manager_removes_the_dish_from_menu(String dishName) {
        ctx.restaurant.removeDish(dishName);
        if (dishName.equals(lastDishName)) lastDishName = null;
    }

    @Then("the dish {string} should not be available")
    public void the_dish_should_not_be_available(String dishName) {
        Dish dish = ctx.restaurant.findDishByName(dishName);
        assertNull(dish, "Dish should not be found in menu");
    }

    @Then("customers should not see {string} in the menu")
    public void customers_should_not_see_in_menu(String dishName) {
        List<Dish> allDishes = ctx.restaurant.getDishes();
        boolean found = allDishes.stream().anyMatch(d -> d.getName().equals(dishName));
        assertFalse(found, "Dish should not be visible in menu");
    }

    // ============ SCENARIO 6: Extra options ============

    @When("the restaurant manager defines an extra option {string} with price {double}")
    public void the_restaurant_manager_defines_an_extra_option_with_price(String extraName, double price) {
        extraOptions.put(extraName, price);
    }

    @Then("the restaurant should have {int} extra options available")
    public void the_restaurant_should_have_extra_options_available(int expectedCount) {
        assertEquals(expectedCount, extraOptions.size());
    }

    @Then("option {string} should cost {double} euros")
    public void option_should_cost_euros(String optionName, double expectedPrice) {
        assertTrue(extraOptions.containsKey(optionName),
                "Extra option '" + optionName + "' should exist");
        assertEquals(expectedPrice, extraOptions.get(optionName), 0.01);
    }

    // ============ SCENARIO 7: Allergen information ============

    @When("the restaurant manager adds allergen information {string}")
    public void the_restaurant_manager_adds_allergen_information(String allergenInfo) {
        assertNotNull(lastDishName, "No current dish context");
        currentAllergenInfo = allergenInfo;
    }

    @Then("the dish should display allergen warning")
    public void the_dish_should_display_allergen_warning() {
        assertNotNull(lastDishName, "No current dish context");
        assertNotNull(currentAllergenInfo, "Allergen information should be set");
    }

    @Then("the warning should mention {string}")
    public void the_warning_should_mention(String allergen) {
        assertNotNull(currentAllergenInfo, "Allergen information should exist");
        assertTrue(currentAllergenInfo.toLowerCase().contains(allergen.toLowerCase()),
                "Allergen warning should mention '" + allergen + "'");
    }
}