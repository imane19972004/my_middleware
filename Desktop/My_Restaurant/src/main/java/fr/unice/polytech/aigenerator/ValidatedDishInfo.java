package fr.unice.polytech.aigenerator;

import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;

public class ValidatedDishInfo {
    public final String name;
    public final String description;
    public final DishType dishType;
    public final DishCategory dishCategory;

    public ValidatedDishInfo(String name, String description, DishType type, DishCategory category) {
        this.name = name;
        this.description = description;
        this.dishType = type;
        this.dishCategory = category;
    }
}