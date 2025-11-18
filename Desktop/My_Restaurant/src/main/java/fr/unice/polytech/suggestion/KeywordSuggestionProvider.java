package fr.unice.polytech.suggestion;

import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

public class KeywordSuggestionProvider implements SuggestionProvider {

    private final Map<String, DishInfo> keywordMap;

    public KeywordSuggestionProvider() {
        keywordMap = Map.of(
                "pizza", new DishInfo("Pizza", "A classic Italian dish", DishType.ITALIAN, DishCategory.MAIN_COURSE),
                "burger", new DishInfo("Burger", "An American classic", DishType.AMERICAN, DishCategory.MAIN_COURSE),
                "sushi", new DishInfo("Sushi", "A Japanese specialty", DishType.JAPANESE, DishCategory.MAIN_COURSE),
                "salade", new DishInfo("Salad", "A healthy starter", DishType.GENERAL, DishCategory.STARTER)
        );
    }

    @Override
    public List<DishInfo> getSuggestions(String keyword) {
        return keywordMap.entrySet().stream()
                .filter(entry -> entry.getKey().contains(keyword.toLowerCase()))
                .map(Map.Entry::getValue)
                .collect(Collectors.toList());
    }
}