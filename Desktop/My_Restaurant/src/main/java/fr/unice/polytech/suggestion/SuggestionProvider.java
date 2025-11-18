package fr.unice.polytech.suggestion;

import java.util.List;

public interface SuggestionProvider {
    List<DishInfo> getSuggestions(String keyword);
}