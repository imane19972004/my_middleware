package fr.unice.polytech.api.dto;

import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/**
 * DTO for dish information
 * Used by Catalog Service to transfer dish data
 */
public class DishDTO {
    
    @JsonProperty("id")
    private Long id;
    
    @JsonProperty("name")
    private String name;
    
    @JsonProperty("description")
    private String description;
    
    @JsonProperty("price")
    private double price;
    
    @JsonProperty("category")
    private String category; // STARTER, MAIN_COURSE, DESSERT, DRINK
    
    @JsonProperty("dishType")
    private String dishType; // ITALIAN, FRENCH, JAPANESE, etc.
    
    @JsonProperty("toppings")
    private List<ToppingDTO> toppings;
    
    // ========== Constructors ==========
    
    public DishDTO() {
        // Required by Jackson
    }
    
    public DishDTO(String name, String description, double price) {
        this.name = name;
        this.description = description;
        this.price = price;
    }
    
    // ========== Getters/Setters ==========
    
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
    
    public String getDescription() {
        return description;
    }
    
    public void setDescription(String description) {
        this.description = description;
    }
    
    public double getPrice() {
        return price;
    }
    
    public void setPrice(double price) {
        this.price = price;
    }
    
    public String getCategory() {
        return category;
    }
    
    public void setCategory(String category) {
        this.category = category;
    }
    
    public String getDishType() {
        return dishType;
    }
    
    public void setDishType(String dishType) {
        this.dishType = dishType;
    }
    
    public List<ToppingDTO> getToppings() {
        return toppings;
    }
    
    public void setToppings(List<ToppingDTO> toppings) {
        this.toppings = toppings;
    }
    
    // ========== toString ==========
    
    @Override
    public String toString() {
        return "DishDTO{" +
                "id=" + id +
                ", name='" + name + '\'' +
                ", price=" + price +
                ", category='" + category + '\'' +
                '}';
    }
}
