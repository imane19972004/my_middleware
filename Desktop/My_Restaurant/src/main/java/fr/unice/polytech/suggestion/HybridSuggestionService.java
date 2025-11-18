package fr.unice.polytech.suggestion;

import fr.unice.polytech.dishes.Dish;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;

public class HybridSuggestionService {

    private final KeywordSuggestionProvider keywordProvider;
    private final HistorySuggestionProvider historyProvider;

    public HybridSuggestionService() {
        this.keywordProvider = new KeywordSuggestionProvider();
        this.historyProvider = new HistorySuggestionProvider();
    }

    // Constructor for testing
    public HybridSuggestionService(KeywordSuggestionProvider keywordProvider, HistorySuggestionProvider historyProvider) {
        this.keywordProvider = keywordProvider;
        this.historyProvider = historyProvider;
    }

    public void learnFrom(Dish dish) {
        historyProvider.learnFrom(dish);
    }

    public List<DishInfo> getSuggestions(String keyword) {
        List<DishInfo> historySuggestions = historyProvider.getSuggestions(keyword);
        List<DishInfo> keywordSuggestions = keywordProvider.getSuggestions(keyword);

        // Merge and remove duplicates, giving priority to history
        return Stream.concat(historySuggestions.stream(), keywordSuggestions.stream())
                .distinct()
                .collect(Collectors.toList());
    }
}