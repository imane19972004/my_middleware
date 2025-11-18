package fr.unice.polytech.services.catalog.handlers;


import com.fasterxml.jackson.databind.ObjectMapper;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import fr.unice.polytech.api.dto.RestaurantDTO;
import fr.unice.polytech.dishes.DishType;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.services.catalog.mappers.RestaurantMapper;

import java.io.IOException;
import java.io.OutputStream;
import java.net.URI;
import java.util.*;
import java.util.stream.Collectors;

/**
 * Handler for Restaurant endpoints
 * 
 * GET /api/restaurants?cuisineType=ITALIAN&hasVegetarian=true
 * GET /api/restaurants/{id}
 */
public class RestaurantHandler implements HttpHandler {
    
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final List<Restaurant> mockRestaurants;
    
    public RestaurantHandler() {
        // Initialize mock data
        this.mockRestaurants = createMockRestaurants();
    }
    
    @Override
    public void handle(HttpExchange exchange) throws IOException {
        String method = exchange.getRequestMethod();
        String path = exchange.getRequestURI().getPath();
        
        try {
            if ("GET".equals(method)) {
                if (path.matches("/api/restaurants/\\d+")) {
                    // GET /api/restaurants/{id}
                    handleGetRestaurantById(exchange);
                } else {
                    // GET /api/restaurants with filters
                    handleGetRestaurants(exchange);
                }
            } else {
                sendResponse(exchange, 405, "{\"error\": \"Method not allowed\"}");
            }
        } catch (Exception e) {
            e.printStackTrace();
            sendResponse(exchange, 500, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleGetRestaurants(HttpExchange exchange) throws IOException {
        URI uri = exchange.getRequestURI();
        Map<String, String> queryParams = parseQueryParams(uri.getQuery());
        
        // Filter restaurants
        List<Restaurant> filteredRestaurants = mockRestaurants;
        
        // Filter by cuisine type
        if (queryParams.containsKey("cuisineType")) {
            String cuisineType = queryParams.get("cuisineType");
            filteredRestaurants = filteredRestaurants.stream()
                .filter(r -> r.getCuisineType() != null && 
                            r.getCuisineType().toString().equalsIgnoreCase(cuisineType))
                .collect(Collectors.toList());
        }
        
        // Filter by vegetarian dishes
        if (queryParams.containsKey("hasVegetarian") && 
            "true".equalsIgnoreCase(queryParams.get("hasVegetarian"))) {
            filteredRestaurants = filteredRestaurants.stream()
                .filter(r -> r.getDishes().stream()
                    .anyMatch(d -> d.getCuisineType() == DishType.VEGETARIAN))
                .collect(Collectors.toList());
        }
        
        // Convert to DTOs
        List<RestaurantDTO> dtos = filteredRestaurants.stream()
            .map(RestaurantMapper::toDTO)
            .collect(Collectors.toList());
        
        String jsonResponse = objectMapper.writeValueAsString(dtos);
        sendResponse(exchange, 200, jsonResponse);
    }
    
    private void handleGetRestaurantById(HttpExchange exchange) throws IOException {
        String path = exchange.getRequestURI().getPath();
        String idStr = path.substring(path.lastIndexOf('/') + 1);
        
        try {
            long id = Long.parseLong(idStr);
            Optional<Restaurant> restaurant = mockRestaurants.stream()
                .filter(r -> r.hashCode() == id)
                .findFirst();
            
            if (restaurant.isPresent()) {
                RestaurantDTO dto = RestaurantMapper.toDTO(restaurant.get());
                String jsonResponse = objectMapper.writeValueAsString(dto);
                sendResponse(exchange, 200, jsonResponse);
            } else {
                sendResponse(exchange, 404, "{\"error\": \"Restaurant not found\"}");
            }
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
    
    private List<Restaurant> createMockRestaurants() {
        List<Restaurant> restaurants = new ArrayList<>();
        
        // Italian Restaurant
        Restaurant italian = new Restaurant.Builder("La Bella Vita")
            .withCuisineType(DishType.ITALIAN)
            .build();
        italian.addDish("Margherita Pizza", "Classic tomato and mozzarella", 12.50);
        italian.addDish("Carbonara", "Creamy pasta with bacon", 14.00);
        restaurants.add(italian);
        
        // Vegetarian Restaurant
        Restaurant veggie = new Restaurant.Builder("Green Garden")
            .withCuisineType(DishType.VEGETARIAN)
            .build();
        veggie.addDish("Veggie Burger", "Plant-based burger", 11.00);
        veggie.addDish("Quinoa Salad", "Healthy quinoa bowl", 9.50);
        restaurants.add(veggie);
        
        // Japanese Restaurant
        Restaurant japanese = new Restaurant.Builder("Sakura Sushi")
            .withCuisineType(DishType.JAPANESE)
            .build();
        japanese.addDish("California Roll", "Fresh sushi roll", 13.00);
        japanese.addDish("Miso Soup", "Traditional soup", 4.50);
        restaurants.add(japanese);
        
        return restaurants;
    }
}
