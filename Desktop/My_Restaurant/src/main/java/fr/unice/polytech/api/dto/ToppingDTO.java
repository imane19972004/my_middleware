package fr.unice.polytech.api.dto;



import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * DTO for dish toppings (paid extensions)
 * Example: Extra cheese, bacon, olives, etc.
 */
public class ToppingDTO {
    
    @JsonProperty("name")
    private String name;
    
    @JsonProperty("price")
    private double price;
    
    // ========== Constructors ==========
    
    public ToppingDTO() {
        // Required by Jackson
    }
    
    public ToppingDTO(String name, double price) {
        this.name = name;
        this.price = price;
    }
    
    // ========== Getters/Setters ==========
    
    public String getName() {
        return name;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    
    public double getPrice() {
        return price;
    }
    
    public void setPrice(double price) {
        this.price = price;
    }
    
    // ========== toString ==========
    
    @Override
    public String toString() {
        return "ToppingDTO{" +
                "name='" + name + '\'' +
                ", price=" + price +
                '}';
    }
}