package fr.unice.polytech.dishes;

public enum DishCategory {
    STARTER,
    MAIN_COURSE,
    DESSERT,
    DRINK;

    private String categoryName;

    public String getCategory(){
        return categoryName;
    }
}
