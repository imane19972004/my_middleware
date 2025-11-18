Feature: AI-Powered Dish Information Generation
  As a user,
  I want to get reliable information about a dish from an image,
  So that I am not exposed to inaccurate AI-generated content.

  Scenario: AI generates dish information with high confidence
    Given the system receives an image of a "Pizza"
    And the AI analysis generates "Pizza Margherita", "A classic Italian pizza with tomato and mozzarella.", "ITALIAN", "MAIN_COURSE" with 95% confidence
    When the system processes the image to generate dish information
    Then the user should see the accurate dish name "Pizza Margherita" and description "A classic Italian pizza with tomato and mozzarella."

  Scenario: AI generates dish information with low confidence
    Given the system receives an image of a "strange dish"
    And the AI analysis generates information with only 40% confidence
    When the system processes the image to generate dish information
    Then the user should see a fallback message instead of the AI-generated content