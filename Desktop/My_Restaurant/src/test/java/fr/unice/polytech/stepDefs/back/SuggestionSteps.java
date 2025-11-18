package fr.unice.polytech.stepDefs.back;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.suggestion.DishInfo;
import fr.unice.polytech.suggestion.HybridSuggestionService;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;
import java.util.List;
import static org.junit.jupiter.api.Assertions.*;

public class SuggestionSteps {

    private final HybridSuggestionService suggestionService = new HybridSuggestionService();
    private List<DishInfo> suggestions;

    @Given("the suggestion system has a predefined keyword for {string}")
    public void the_suggestion_system_has_a_predefined_keyword_for(String keyword) {
        // This is handled by the KeywordSuggestionProvider's constructor
    }

    @Given("a dish named {string} with the description {string} has been added to the system")
    public void a_dish_named_with_description_has_been_added(String name, String description) {
        Dish dish = new Dish(name, description, 10.0);
        suggestionService.learnFrom(dish);
    }

    @When("the owner types {string} to create a new dish")
    public void the_owner_types_to_create_a_new_dish(String keyword) {
        suggestions = suggestionService.getSuggestions(keyword);
    }

    @Then("the system should suggest a dish named {string} with the description {string}")
    public void the_system_should_suggest_a_dish_named(String expectedName, String expectedDescription) {
        assertFalse(suggestions.isEmpty(), "Expected suggestions but found none.");
        DishInfo topSuggestion = suggestions.get(0);
        assertEquals(expectedName, topSuggestion.name);
        assertEquals(expectedDescription, topSuggestion.description);
    }

    @Then("the system should suggest a dish named {string} with the description {string} based on historical data")
    public void the_system_should_suggest_a_dish_named_based_on_historical_data(String expectedName, String expectedDescription) {
        assertFalse(suggestions.isEmpty(), "Expected suggestions but found none.");
        // History suggestions are prioritized, so it should be the first result
        DishInfo topSuggestion = suggestions.get(0);
        assertEquals(expectedName, topSuggestion.name);
        assertEquals(expectedDescription, topSuggestion.description);
    }
}