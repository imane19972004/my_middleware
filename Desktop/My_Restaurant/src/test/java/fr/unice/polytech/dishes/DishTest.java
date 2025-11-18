package fr.unice.polytech.dishes;

import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;


class DishTest {

    @Test
    void testDishCreationAndGetters() {
        Dish pizza = new Dish("Margherita", "Simple cheese pizza", 12.50);

        assertEquals("Margherita", pizza.getName(), "The name should be set correctly.");
        assertEquals("Simple cheese pizza", pizza.getDescription(), "The description should be set correctly.");
        assertEquals(12.50, pizza.getPrice(), "The price should be set correctly.");
        assertTrue(pizza.getToppings().isEmpty(), "The toppings list should be initialized empty.");
        assertNull(pizza.getCategory(), "The category should be null initially."); // Added check for category
    }


    @Test
    void testSetName() {
        Dish dish = new Dish("Old Name", "Desc", 5.0);
        dish.setName("New Name");
        assertEquals("New Name", dish.getName(), "setName should update the dish's name.");
    }

    @Test
    void testSetDescription() {
        Dish dish = new Dish("Name", "Old Desc", 5.0);
        dish.setDescription("New Description");
        assertEquals("New Description", dish.getDescription(), "setDescription should update the dish's description.");
    }

    @Test
    void testSetPrice() {
        Dish dish = new Dish("Name", "Desc", 5.0);
        dish.setPrice(9.99);
        assertEquals(9.99, dish.getPrice(), 0.001, "setPrice should update the dish's base price.");
        // Use delta (0.001) for double comparisons
    }


    @Test
    void testUpdateDetails() {
        Dish salad = new Dish("Garden Salad", "Simple greens", 7.00);

        salad.updateDetails("Caesar Salad", "Classic romaine with croutons", 9.50);

        assertEquals("Caesar Salad", salad.getName(), "updateDetails should update the name.");
        assertEquals("Classic romaine with croutons", salad.getDescription(), "updateDetails should update the description.");
        assertEquals(9.50, salad.getPrice(), 0.001, "updateDetails should update the price.");
    }


    @Test
    void testAddTopping() {
        Dish fries = new Dish("Fries", "Classic fries", 3.00);
        Topping cheese = new Topping("Cheese Sauce", 1.50);

        fries.addTopping(cheese);

        assertEquals(1, fries.getToppings().size(), "One topping should have been added.");
        assertEquals(cheese, fries.getToppings().get(0), "The correct topping object should be in the list.");

        Topping bacon = new Topping("Bacon Bits", 2.00);
        fries.addTopping(bacon);
        assertEquals(2, fries.getToppings().size(), "A second topping should have been added.");
    }

    @Test
    void testSetCategory() {
        Dish coke = new Dish("Coke", "Carbonated drink", 2.00);
        coke.setCategory(DishCategory.DRINK);

        assertEquals(DishCategory.DRINK, coke.getCategory(), "The category should be set to DRINK.");

        coke.setCategory(DishCategory.DESSERT);
        assertEquals(DishCategory.DESSERT, coke.getCategory(), "The category should be changeable.");
    }

    @Test
    void testToString() {
        Dish soup = new Dish("Tomato Soup", "Warm tomato soup", 5.0);
        soup.setCategory(DishCategory.STARTER);

        String expectedWithCategory = "Dish [name=Tomato Soup, price=5.0, category=STARTER]";
        assertEquals(expectedWithCategory, soup.toString(), "toString should include name, price, and category.");

        Dish water = new Dish("Water", "Plain water", 0.0);
        String expectedWithoutCategory = "Dish [name=Water, price=0.0, category=N/A]";
        assertEquals(expectedWithoutCategory, water.toString(), "toString should handle null category with 'N/A'.");
    }
}