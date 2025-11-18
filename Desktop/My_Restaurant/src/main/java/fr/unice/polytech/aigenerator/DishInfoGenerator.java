package fr.unice.polytech.aigenerator;

import java.util.Optional;

public class DishInfoGenerator {

    private final ImageAnalysisService imageAnalysisService;
    private static final double CONFIDENCE_THRESHOLD = 0.75;

    public DishInfoGenerator(ImageAnalysisService imageAnalysisService) {
        this.imageAnalysisService = imageAnalysisService;
    }

    public Optional<ValidatedDishInfo> generateInfoFromImage(byte[] image) {
        ImageAnalysisService.AnalysisResult result = imageAnalysisService.analyze(image);

        if (result.getConfidence() >= CONFIDENCE_THRESHOLD) {
            ValidatedDishInfo info = new ValidatedDishInfo(
                    result.getGeneratedName(),
                    result.getGeneratedDescription(),
                    result.getDishType(),
                    result.getDishCategory()
            );
            return Optional.of(info);
        } else {
            return Optional.empty();
        }
    }
}