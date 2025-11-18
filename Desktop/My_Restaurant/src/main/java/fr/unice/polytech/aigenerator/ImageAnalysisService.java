package fr.unice.polytech.aigenerator;

import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;

public interface ImageAnalysisService {
    AnalysisResult analyze(byte[] image);

    class AnalysisResult {
        private final String generatedName;
        private final String generatedDescription;
        private final DishType dishType;
        private final DishCategory dishCategory;
        private final double confidence;

        public AnalysisResult(String name, String description, DishType type, DishCategory category, double confidence) {
            this.generatedName = name;
            this.generatedDescription = description;
            this.dishType = type;
            this.dishCategory = category;
            this.confidence = confidence;
        }

        public String getGeneratedName() { return generatedName; }
        public String getGeneratedDescription() { return generatedDescription; }
        public DishType getDishType() { return dishType; }
        public DishCategory getDishCategory() { return dishCategory; }
        public double getConfidence() { return confidence; }
    }
}