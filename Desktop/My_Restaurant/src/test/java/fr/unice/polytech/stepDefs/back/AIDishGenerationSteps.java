package fr.unice.polytech.stepDefs.back;

import fr.unice.polytech.aigenerator.DishInfoGenerator;
import fr.unice.polytech.aigenerator.ImageAnalysisService;
import fr.unice.polytech.aigenerator.MockImageAnalysisService;
import fr.unice.polytech.aigenerator.ValidatedDishInfo;
import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;

import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

public class AIDishGenerationSteps {

    private byte[] currentImage;
    private final MockImageAnalysisService mockService = new MockImageAnalysisService();
    private final DishInfoGenerator infoGenerator = new DishInfoGenerator(mockService);
    private Optional<ValidatedDishInfo> finalDishInfoOptional;

    @Given("the system receives an image of a {string}")
    public void the_system_receives_an_image_of_a(String imageName) {
        this.currentImage = imageName.getBytes();
    }

    @Given("the AI analysis generates {string}, {string}, {string}, {string} with {int}% confidence")
    public void the_ai_analysis_generates_info_with_confidence(String name, String description, String dishType, String dishCategory, int confidence) {
        mockService.setMockResult(new ImageAnalysisService.AnalysisResult(
                name,
                description,
                DishType.valueOf(dishType),
                DishCategory.valueOf(dishCategory),
                confidence / 100.0
        ));
    }

    @Given("the AI analysis generates information with only {int}% confidence")
    public void the_ai_analysis_generates_low_confidence_info(int confidence) {
        mockService.setMockResult(new ImageAnalysisService.AnalysisResult(
                "Uncertain Dish", "AI generated description...", DishType.GENERAL, DishCategory.MAIN_COURSE, confidence / 100.0
        ));
    }

    @When("the system processes the image to generate dish information")
    public void the_system_processes_the_image() {
        finalDishInfoOptional = infoGenerator.generateInfoFromImage(currentImage);
    }

    @Then("the user should see the accurate dish name {string} and description {string}")
    public void the_user_should_see_accurate_information(String expectedName, String expectedDescription) {
        assertTrue(finalDishInfoOptional.isPresent(), "Expected a valid dish info, but got empty.");
        ValidatedDishInfo finalDishInfo = finalDishInfoOptional.get();
        assertEquals(expectedName, finalDishInfo.name);
        assertEquals(expectedDescription, finalDishInfo.description);
    }

    @Then("the user should see a fallback message instead of the AI-generated content")
    public void the_user_should_see_a_fallback_message() {
        assertTrue(finalDishInfoOptional.isEmpty(), "Expected an empty result due to low confidence.");
    }
}