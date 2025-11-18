package fr.unice.polytech.services.order.handlers;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import fr.unice.polytech.api.dto.TimeSlotDTO;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.restaurants.RestaurantManager;
import fr.unice.polytech.restaurants.TimeSlot;
import fr.unice.polytech.services.order.mappers.OrderMapper;

import java.io.IOException;
import java.io.OutputStream;
import java.net.URI;
import java.time.DayOfWeek;
import java.time.LocalTime;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

/**
 * Handler for TimeSlot endpoints
 * 
 * GET /api/timeslots?restaurantId={id}
 * 
 * TD requirement: "visualisant les heures de livraisons possibles 
 * qui évoluent en fonction de la commande et des autres commandes"
 */
public class TimeSlotHandler implements HttpHandler {
    
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final RestaurantManager restaurantManager;
    
    public TimeSlotHandler(RestaurantManager restaurantManager) {
        this.restaurantManager = restaurantManager;
        initializeMockTimeSlots();
    }
    
    @Override
    public void handle(HttpExchange exchange) throws IOException {
        String method = exchange.getRequestMethod();
        
        try {
            if ("GET".equals(method)) {
                handleGetTimeSlots(exchange);
            } else {
                sendResponse(exchange, 405, "{\"error\": \"Method not allowed\"}");
            }
        } catch (Exception e) {
            e.printStackTrace();
            sendResponse(exchange, 500, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleGetTimeSlots(HttpExchange exchange) throws IOException {
        URI uri = exchange.getRequestURI();
        Map<String, String> queryParams = parseQueryParams(uri.getQuery());
        
        String restaurantIdStr = queryParams.get("restaurantId");
        if (restaurantIdStr == null || restaurantIdStr.isEmpty()) {
            sendResponse(exchange, 400, "{\"error\": \"Restaurant ID is required\"}");
            return;
        }
        
        try {
            long restaurantId = Long.parseLong(restaurantIdStr);
            
            // Find restaurant
            Restaurant restaurant = restaurantManager.getAllRestaurants().stream()
                .filter(r -> r.hashCode() == restaurantId)
                .findFirst()
                .orElse(null);
            
            if (restaurant == null) {
                sendResponse(exchange, 404, "{\"error\": \"Restaurant not found\"}");
                return;
            }
            
            // Get available time slots
            List<TimeSlot> availableSlots = restaurantManager.getAvailableTimeSlots(restaurant);
            
            // Convert to DTOs with capacity info
            List<TimeSlotDTO> slotDTOs = availableSlots.stream()
                .map(slot -> {
                    int capacity = restaurant.getCapacity(slot);
                    return OrderMapper.timeSlotToDTO(slot, capacity);
                })
                .filter(dto -> dto.getAvailableCapacity() > 0) // Only return slots with capacity
                .collect(Collectors.toList());
            
            String jsonResponse = objectMapper.writeValueAsString(slotDTOs);
            sendResponse(exchange, 200, jsonResponse);
            
        } catch (NumberFormatException e) {
            sendResponse(exchange, 400, "{\"error\": \"Invalid restaurant ID\"}");
        }
    }
    
    private Map<String, String> parseQueryParams(String query) {
        Map<String, String> params = new HashMap<>();
        if (query == null || query.isEmpty()) {
            return params;
        }
        
        for (String param : query.split("&")) {
            String[] pair = param.split("=");
            if (pair.length == 2) {
                params.put(pair[0], pair[1]);
            }
        }
        return params;
    }
    
    private void sendResponse(HttpExchange exchange, int statusCode, String response) throws IOException {
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.sendResponseHeaders(statusCode, response.getBytes().length);
        
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(response.getBytes());
        }
    }
    
    // ========== MOCK DATA ==========
    
    private void initializeMockTimeSlots() {
        // Create time slots for all restaurants
        for (Restaurant restaurant : restaurantManager.getAllRestaurants()) {
            // Lunch time slots (12:00 - 14:00)
            TimeSlot lunch1 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(12, 0), LocalTime.of(12, 30));
            TimeSlot lunch2 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(12, 30), LocalTime.of(13, 0));
            TimeSlot lunch3 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(13, 0), LocalTime.of(13, 30));
            TimeSlot lunch4 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(13, 30), LocalTime.of(14, 0));
            
            // Dinner time slots (19:00 - 21:00)
            TimeSlot dinner1 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(19, 0), LocalTime.of(19, 30));
            TimeSlot dinner2 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(19, 30), LocalTime.of(20, 0));
            TimeSlot dinner3 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(20, 0), LocalTime.of(20, 30));
            TimeSlot dinner4 = new TimeSlot(DayOfWeek.MONDAY, LocalTime.of(20, 30), LocalTime.of(21, 0));
            
            // Set capacity for each slot (simulate dynamic availability)
            restaurant.setCapacity(lunch1, 5);
            restaurant.setCapacity(lunch2, 3);
            restaurant.setCapacity(lunch3, 2);
            restaurant.setCapacity(lunch4, 4);
            restaurant.setCapacity(dinner1, 6);
            restaurant.setCapacity(dinner2, 4);
            restaurant.setCapacity(dinner3, 1); // Almost full
            restaurant.setCapacity(dinner4, 5);
        }
    }
}
