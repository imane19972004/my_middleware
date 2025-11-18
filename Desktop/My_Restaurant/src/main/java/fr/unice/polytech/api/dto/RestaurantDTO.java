package fr.unice.polytech.api.dto;

import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/**
 * DTO for transferring restaurant data via REST APIs
 * Used by Catalog Service to send restaurant information to frontend 
 * Corresponds to TD requirement: "consulter les restaurants et leurs menus"
 */
public class RestaurantDTO {
    
    @JsonProperty("id")
    private Long id;
    
    @JsonProperty("name")
    private String name;
    
    @JsonProperty("cuisineType")
    private String cuisineType; // ITALIAN, FRENCH, JAPANESE, etc.
    
    @JsonProperty("dishes")
    private List<DishDTO> dishes;
    
    @JsonProperty("openingHours")
    private List<OpeningHoursDTO> openingHours;
    
    // ========== Constructors ==========
    
    public RestaurantDTO() {
        // Required by Jackson for deserialization
    }
    
    public RestaurantDTO(Long id, String name, String cuisineType) {
        this.id = id;
        this.name = name;
        this.cuisineType = cuisineType;
    }
    
    // ========== Getters/Setters (required by Jackson) ==========
    
    public Long getId() {
        return id;
    }
    
    public void setId(Long id) {
        this.id = id;
    }
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public String getCuisineType() {
        return cuisineType;
    }
    
    public void setCuisineType(String cuisineType) {
        this.cuisineType = cuisineType;
    }
    
    public List<DishDTO> getDishes() {
        return dishes;
    }
    
    public void setDishes(List<DishDTO> dishes) {
        this.dishes = dishes;
    }
    
    public List<OpeningHoursDTO> getOpeningHours() {
        return openingHours;
    }
    
    public void setOpeningHours(List<OpeningHoursDTO> openingHours) {
        this.openingHours = openingHours;
    }
    
    // ========== toString for debugging ==========
    
    @Override
    public String toString() {
        return "RestaurantDTO{" +
                "id=" + id +
                ", name='" + name + '\'' +
                ", cuisineType='" + cuisineType + '\'' +
                ", dishes=" + (dishes != null ? dishes.size() + " dishes" : "no dishes") +
                '}';
    }
}