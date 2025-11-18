package fr.unice.polytech.suggestion;

import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;

public class DishInfo {
    public final String name;
    public final String description;
    public final DishType dishType;
    public final DishCategory dishCategory;

    public DishInfo(String name, String description, DishType dishType, DishCategory dishCategory) {
        this.name = name;
        this.description = description;
        this.dishType = dishType;
        this.dishCategory = dishCategory;
    }
}