package fr.unice.polytech.services.order.mappers;

import fr.unice.polytech.api.dto.DishDTO;
import fr.unice.polytech.api.dto.OrderDTO;
import fr.unice.polytech.api.dto.TimeSlotDTO;
import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.restaurants.TimeSlot;

import java.util.List;
import java.util.stream.Collectors;

/**
 * Mapper to convert domain entities to DTOs
 */
public class OrderMapper {
    
    public static OrderDTO toDTO(Order order) {
        OrderDTO dto = new OrderDTO();
        dto.setId((long) order.hashCode());
        dto.setStudentId(order.getStudentAccount().getStudentID());
        dto.setRestaurantId((long) order.getRestaurant().hashCode());
        dto.setTotalAmount(order.getAmount());
        dto.setStatus(order.getOrderStatus().toString());
        dto.setDeliveryLocation(order.getDeliveryLocation() != null ? 
                                order.getDeliveryLocation().toString() : null);
        
        // Convert dishes
        List<DishDTO> dishDTOs = order.getDishes().stream()
                .map(OrderMapper::dishToDTO)
                .collect(Collectors.toList());
        dto.setDishes(dishDTOs);
        
        return dto;
    }
    
    public static TimeSlotDTO timeSlotToDTO(TimeSlot slot, int capacity) {
        TimeSlotDTO dto = new TimeSlotDTO();
        dto.setStartTime(slot.getStartTime().toString());
        dto.setEndTime(slot.getEndTime().toString());
        dto.setAvailableCapacity(capacity);
        dto.setDayOfWeek(slot.getDayOfWeek() != null ? slot.getDayOfWeek().toString() : null);
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
}