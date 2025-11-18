package fr.unice.polytech.restaurants;

import fr.unice.polytech.dishes.Dish;

import fr.unice.polytech.dishes.DishType;
import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.orderManagement.Order;

import fr.unice.polytech.users.UserAccount;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class Restaurant extends UserAccount {
    private String restaurantName;
    private List<Dish> dishes;
    private List<Order> orders;
   //Simple initialisation 
    private List<OpeningHours> openingHours;
    private Map<TimeSlot, Integer> capacityByTimeSlot;
    private EstablishmentType establishmentType;
    private DishType cuisineType;
    private final DishManager dishManager = new DishManager();




   //Simple initialisation
    public Restaurant(String restaurantName) {
        if (restaurantName == null || restaurantName.isEmpty()) {
            throw new IllegalArgumentException("Restaurant name cannot be null or empty");
        }
        this.restaurantName = restaurantName;
        this.dishes = new ArrayList<>();
        orders = new ArrayList<>();
        this.capacityByTimeSlot = new HashMap<>();
        this.openingHours = new ArrayList<>();
    }
    
    
    //Private constructor for Builder pattern.
    //Avoid public Restaurant(String restaurantName, List<Dish> dishes, List<TimeSlot> availableTimeSlots) {..}
    private Restaurant(Builder builder) {
        this.restaurantName = builder.restaurantName;
        this.dishes = new ArrayList<>(builder.dishes);
        orders = new ArrayList<>();
        this.capacityByTimeSlot = new HashMap<>();
        this.cuisineType = builder.cuisineType;
        this.openingHours = new ArrayList<>(builder.openingHours);
    }
    


    
    public String getRestaurantName() {
        return restaurantName;
    }
    
    
    //return a copy of the dishes/TimeSlot list to prevent external modification.
    public List<Dish> getDishes() {
        return new ArrayList<>(dishes);
    }
    
   
    public List<TimeSlot> getAvailableTimeSlots() {
        List<TimeSlot> availableSlots = new ArrayList<>();
        for (Map.Entry<TimeSlot, Integer> entry : capacityByTimeSlot.entrySet()) {
            if (entry.getValue() > 0) {
                availableSlots.add(entry.getKey());
            }
        }
        return availableSlots;
    }
    
   
    
    public void setRestaurantName(String restaurantName) {
        if (restaurantName == null || restaurantName.isEmpty()) {
            throw new IllegalArgumentException("Restaurant name cannot be null or empty");
        }
        this.restaurantName = restaurantName;
    }


     //======BLOCK A TIME SLOTS MANAGEMENT METHODS===========
    public void blockTimeSlot(TimeSlot slot){
        if(slot == null) throw new IllegalArgumentException("TimeSlot cannot be null");
        decreaseCapacity(slot);
    }

    public void unblockTimeSlot(TimeSlot slot){
        if(slot == null) throw new IllegalArgumentException("TimeSlot cannot be null");
        increaseCapacity(slot);
    }


    //======= Capacity by slot =====

    public void setCapacity(TimeSlot slot, int capacity) {
        if (slot == null) throw new IllegalArgumentException("TimeSlot cannot be null");
        if (capacity < 0) throw new IllegalArgumentException("Capacity cannot be negative");
        capacityByTimeSlot.put(slot, capacity);
    }

    public void setOpeningHours(List<OpeningHours> openingHours) {
        if (openingHours == null) {
            throw new IllegalArgumentException("Opening hours list cannot be null.");
        }
        this.openingHours = new ArrayList<>(openingHours);
    }

    public void addOpeningHours(OpeningHours newOpeningHours) {
        if (newOpeningHours == null) {
            throw new IllegalArgumentException("Opening hours cannot be null.");
        }
        for (OpeningHours existingHours : this.openingHours) {
            if (existingHours.getDay() == newOpeningHours.getDay()) {
                throw new IllegalArgumentException(
                        "Opening hours for " + newOpeningHours.getDay() + " already exist."
                );
            }
        }
        this.openingHours.add(newOpeningHours);
    }

    public void updateOpeningHours(OpeningHours updatedOpeningHours) {
        if (updatedOpeningHours == null) {
            throw new IllegalArgumentException("Opening hours cannot be null.");
        }
        boolean dayFound = false;
        for (OpeningHours existingHours : this.openingHours) {
            if (existingHours.getDay() == updatedOpeningHours.getDay()) {
                dayFound = true;
                break;
            }
        }

        if (!dayFound) {
            throw new IllegalArgumentException("Cannot update opening hours: No entry found for " + updatedOpeningHours.getDay());
        }

        this.openingHours.removeIf(oh -> oh.getDay() == updatedOpeningHours.getDay());
        this.openingHours.add(updatedOpeningHours);
    }



    public int getCapacity(TimeSlot slot) {
        return capacityByTimeSlot.getOrDefault(slot, 0);
    }

    public Map<TimeSlot, Integer> getAllCapacities() {
        return new HashMap<>(capacityByTimeSlot);
    }

    public void decreaseCapacity(TimeSlot slot) {
        if (slot == null) throw new IllegalArgumentException("TimeSlot cannot be null");
        if (!capacityByTimeSlot.containsKey(slot)) return;
        int capacity= capacityByTimeSlot.get(slot);
        if (capacity > 0) { // Prevent negative capacity
            capacityByTimeSlot.put(slot, capacity - 1);
        } else {
            System.out.println(" No capacity left for slot " + slot);
        }
    }

    public void increaseCapacity(TimeSlot slot) {
        if (slot == null) throw new IllegalArgumentException("TimeSlot cannot be null");
        capacityByTimeSlot.put(slot, capacityByTimeSlot.getOrDefault(slot, 0) + 1);
    }




    // ========== DISH MANAGEMENT METHODS ==========
    public void addDish(String name, String description, double price) {
        if (name == null || name.isEmpty()) {
            throw new IllegalArgumentException("Dish name cannot be null or empty");
        }
        if (description == null) {
            throw new IllegalArgumentException("Dish description cannot be null");
        }
        if (price < 0) {
            throw new IllegalArgumentException("Dish price cannot be negative");
        }
        Dish dish = dishManager.createDish(name, description, price);
        dishes.add(dish);
    }

    public void updateDish(Dish oldDish, String description) {
        if (!dishes.contains(oldDish)){
            throw new IllegalArgumentException("Dish not found in the menu");
        }
        if (description == null) {
            throw new IllegalArgumentException("Dish description cannot be null");
        }

        dishManager.updateDescription(oldDish,description);
    }


    public void updateDish(Dish oldDish, int price) {
        if (!dishes.contains(oldDish)){
            throw new IllegalArgumentException("Dish not found in the menu");
        }
        if (price<0) {
            throw new IllegalArgumentException("Dish description cannot be null");
        }

        dishManager.updatePrice(oldDish,price);
    }


    public void updateDish(Dish oldDish, DishCategory dishCategory) {
        if (!dishes.contains(oldDish)){
            throw new IllegalArgumentException("Dish not found in the menu");
        }
        if (dishCategory == null) {
            throw new IllegalArgumentException("Dish category cannot be null");
        }

        dishManager.updateDishCategory(oldDish,dishCategory);
    }

    public void updateDish(Dish oldDish, DishType dishType) {
        if (!dishes.contains(oldDish)){
            throw new IllegalArgumentException("Dish not found in the menu");
        }
        if (dishType == null) {
            throw new IllegalArgumentException("Dish category cannot be null");
        }

        dishManager.updateDishType(oldDish,dishType);
    }




    public void addOrder(Order order) {
        orders.add(order);
    }
    
    



    @Override
    public String toString() {
        return "Restaurant{" +
                "restaurantName='" + restaurantName + '\'' +
                ", dishCount=" + dishes.size() +
                // ", availableTimeSlotsCount=" + getAvailableTimeSlotCount() +
                '}';
    }
    
    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Restaurant that = (Restaurant) o;
        return restaurantName.equals(that.restaurantName);
    }
    
    @Override
    public int hashCode() {
        return restaurantName.hashCode();
    }


    public DishType getCuisineType() {
        return cuisineType;
    }

    public List<OpeningHours> getOpeningHours() {
        return openingHours;
    }

    /**
     * Retire un plat du menu par son nom
     * @param dishName Le nom du plat à retirer
     * @throws IllegalArgumentException si le nom est null ou vide
     */
    public void removeDish(String dishName) {
        if (dishName == null || dishName.isEmpty()) {
            throw new IllegalArgumentException("Dish name cannot be null or empty");
        }
        dishes.removeIf(dish -> dish.getName().equals(dishName));
    }

    /**
     * Recherche un plat dans le menu par son nom
     * @param dishName Le nom du plat recherché
     * @return Le plat trouvé ou null si aucun plat ne correspond
     * @throws IllegalArgumentException si le nom est null ou vide
     */
    public Dish findDishByName(String dishName) {
        if (dishName == null || dishName.isEmpty()) {
            throw new IllegalArgumentException("Dish name cannot be null or empty");
        }
        return dishes.stream()
                .filter(dish -> dish.getName().equals(dishName))
                .findFirst()
                .orElse(null);
    }

    public List<Order> getOrders() {
        return orders;
    }

    // ========== BUILDER PATTERN ==========

    /**
     * Builder class for constructing Restaurant objects with many optional parameters.
     * We use this class when we  need to create a Restaurant with initial dishes and time slots.
     */
    public static class Builder {
        private final String restaurantName;
        private List<Dish> dishes = new ArrayList<>();
        private List<TimeSlot> availableTimeSlots = new ArrayList<>();
        private DishType cuisineType;
        private List<OpeningHours> openingHours = new ArrayList<>();

        public Builder withCuisineType(DishType cuisineType) {
            this.cuisineType = cuisineType;
            return this;
        }
        public Builder(String restaurantName) {
            if (restaurantName == null || restaurantName.isEmpty()) {
                throw new IllegalArgumentException("Restaurant name is required");
            }
            this.restaurantName = restaurantName;
        }

        public Builder withDish(Dish dish) {
            if (dish != null) {
                this.dishes.add(dish);
            }
            return this;
        }

        public Builder withDishes(List<Dish> dishes) {
            if (dishes != null) {
                this.dishes.addAll(dishes);
            }
            return this;
        }

        public Builder withTimeSlot(TimeSlot timeSlot) {
            if (timeSlot != null) {
                this.availableTimeSlots.add(timeSlot);
            }
            return this;
        }

        public Builder withTimeSlots(List<TimeSlot> timeSlots) {
            if (timeSlots != null) {
                this.availableTimeSlots.addAll(timeSlots);
            }
            return this;
        }

        public Builder withOpeningHours(List<OpeningHours> hours) {
            if (hours != null) {
                this.openingHours.addAll(hours);
            }
            return this;
        }
        public Restaurant build() {
            return new Restaurant(this);
        }
    }

}

