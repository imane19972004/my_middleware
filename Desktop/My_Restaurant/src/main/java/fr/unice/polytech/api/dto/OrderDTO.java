package fr.unice.polytech.api.dto;


import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;

/**
 * DTO for order information
 * Used by Order Service to communicate with frontend
 * 
 * TD requirement: "La prise de commande (C5, C6, C7)"
 */
public class OrderDTO {
    
    @JsonProperty("id")
    private Long id;
    
    @JsonProperty("studentId")
    private String studentId;
    
    @JsonProperty("restaurantId")
    private Long restaurantId;
    
    @JsonProperty("dishes")
    private List<DishDTO> dishes;
    
    @JsonProperty("totalAmount")
    private double totalAmount;
    
    @JsonProperty("status")
    private String status; // PENDING, VALIDATED, CANCELED
    
    @JsonProperty("deliveryLocation")
    private String deliveryLocation;
    
    @JsonProperty("timeSlot")
    private TimeSlotDTO timeSlot;
    
    @JsonProperty("paymentMethod")
    private String paymentMethod; // INTERNAL, EXTERNAL
    
    // ========== Constructors ==========
    
    public OrderDTO() {
        // Required by Jackson
    }
    
    public OrderDTO(Long id, String studentId, Long restaurantId) {
        this.id = id;
        this.studentId = studentId;
        this.restaurantId = restaurantId;
    }
    
    // ========== Getters/Setters ==========
    
    public Long getId() {
        return id;
    }
    
    public void setId(Long id) {
        this.id = id;
    }
    
    public String getStudentId() {
        return studentId;
    }
    
    public void setStudentId(String studentId) {
        this.studentId = studentId;
    }
    
    public Long getRestaurantId() {
        return restaurantId;
    }
    
    public void setRestaurantId(Long restaurantId) {
        this.restaurantId = restaurantId;
    }
    
    public List<DishDTO> getDishes() {
        return dishes;
    }
    
    public void setDishes(List<DishDTO> dishes) {
        this.dishes = dishes;
    }
    
    public double getTotalAmount() {
        return totalAmount;
    }
    
    public void setTotalAmount(double totalAmount) {
        this.totalAmount = totalAmount;
    }
    
    public String getStatus() {
        return status;
    }
    
    public void setStatus(String status) {
        this.status = status;
    }
    
    public String getDeliveryLocation() {
        return deliveryLocation;
    }
    
    public void setDeliveryLocation(String deliveryLocation) {
        this.deliveryLocation = deliveryLocation;
    }
    
    public TimeSlotDTO getTimeSlot() {
        return timeSlot;
    }
    
    public void setTimeSlot(TimeSlotDTO timeSlot) {
        this.timeSlot = timeSlot;
    }
    
    public String getPaymentMethod() {
        return paymentMethod;
    }
    
    public void setPaymentMethod(String paymentMethod) {
        this.paymentMethod = paymentMethod;
    }
    
    // ========== toString ==========
    
    @Override
    public String toString() {
        return "OrderDTO{" +
                "id=" + id +
                ", studentId='" + studentId + '\'' +
                ", restaurantId=" + restaurantId +
                ", totalAmount=" + totalAmount +
                ", status='" + status + '\'' +
                '}';
    }
}