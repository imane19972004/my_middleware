package fr.unice.polytech.suggestion;

import fr.unice.polytech.dishes.Dish;
import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

public class HistorySuggestionProvider implements SuggestionProvider {

    private final List<Dish> history = new ArrayList<>();

    public void learnFrom(Dish dish) {
        history.add(dish);
    }

    @Override
    public List<DishInfo> getSuggestions(String keyword) {
        return history.stream()
                .filter(dish -> dish.getName().toLowerCase().contains(keyword.toLowerCase()))
                .map(dish -> new DishInfo(dish.getName(), dish.getDescription(), dish.getCuisineType(), dish.getCategory()))
                .collect(Collectors.toList());
    }
}