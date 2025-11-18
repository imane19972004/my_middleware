package fr.unice.polytech.restaurants;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;

public class DishManager {

    public Dish createDish(String name, String description, double price) {
        return new Dish(name, description, price);

    }

    public void updatePrice(Dish dish, double newPrice) {
        dish.setPrice(newPrice);
    }

    public void updateDescription(Dish dish, String newDescription) {
        dish.setDescription(newDescription);
    }


    public void updateDishCategory(Dish dish, DishCategory newCategory) {
        dish.setCategory(newCategory);
    }


    public void updateDishType(Dish dish, DishType dishType) {
        dish.setCuisineType(dishType);
    }
}
