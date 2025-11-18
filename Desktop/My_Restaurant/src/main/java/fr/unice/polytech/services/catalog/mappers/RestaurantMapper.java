package fr.unice.polytech.services.catalog.mappers;

import fr.unice.polytech.api.dto.DishDTO;
import fr.unice.polytech.api.dto.OpeningHoursDTO;
import fr.unice.polytech.api.dto.RestaurantDTO;
import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.restaurants.OpeningHours;
import fr.unice.polytech.restaurants.Restaurant;

import java.util.List;
import java.util.stream.Collectors;

/**
 * Mapper to convert domain entities to DTOs
 */
public class RestaurantMapper {
    
    public static RestaurantDTO toDTO(Restaurant restaurant) {
        RestaurantDTO dto = new RestaurantDTO();
        dto.setId((long) restaurant.hashCode()); // Simple ID generation
        dto.setName(restaurant.getRestaurantName());
        dto.setCuisineType(restaurant.getCuisineType() != null ? 
                           restaurant.getCuisineType().toString() : "GENERAL");
        
        // Convert dishes
        List<DishDTO> dishDTOs = restaurant.getDishes().stream()
                .map(RestaurantMapper::dishToDTO)
                .collect(Collectors.toList());
        dto.setDishes(dishDTOs);
        
        // Convert opening hours
        List<OpeningHoursDTO> hoursDTOs = restaurant.getOpeningHours().stream()
                .map(RestaurantMapper::openingHoursToDTO)
                .collect(Collectors.toList());
        dto.setOpeningHours(hoursDTOs);
        
        return dto;
    }
    
    private static DishDTO dishToDTO(Dish dish) {
        DishDTO dto = new DishDTO();
        dto.setId((long) dish.hashCode());
        dto.setName(dish.getName());
        dto.setDescription(dish.getDescription());
        dto.setPrice(dish.getPrice());
        dto.setCategory(dish.getCategory() != null ? dish.getCategory().toString() : null);
        dto.setDishType(dish.getCuisineType() != null ? dish.getCuisineType().toString() : "GENERAL");
        return dto;
    }
    
    private static OpeningHoursDTO openingHoursToDTO(OpeningHours hours) {
        return new OpeningHoursDTO(
            hours.getDay().toString(),
            hours.getOpeningTime().toString(),
            hours.getClosingTime().toString()
        );
    }
}