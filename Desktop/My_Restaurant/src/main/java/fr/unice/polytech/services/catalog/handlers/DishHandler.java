package fr.unice.polytech.services.catalog.handlers;


import com.fasterxml.jackson.databind.ObjectMapper;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import fr.unice.polytech.api.dto.DishDTO;
import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.dishes.DishCategory;
import fr.unice.polytech.dishes.DishType;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;

/**
 * Handler for Dish endpoints
 * 
 * POST /api/dishes - Add a new dish
 */
public class DishHandler implements HttpHandler {
    
    private final ObjectMapper objectMapper = new ObjectMapper();
    
    @Override
    public void handle(HttpExchange exchange) throws IOException {
        String method = exchange.getRequestMethod();
        
        try {
            if ("POST".equals(method)) {
                handleAddDish(exchange);
            } else {
                sendResponse(exchange, 405, "{\"error\": \"Method not allowed\"}");
            }
        } catch (Exception e) {
            e.printStackTrace();
            sendResponse(exchange, 500, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleAddDish(HttpExchange exchange) throws IOException {
        // Read request body
        InputStream requestBody = exchange.getRequestBody();
        String body = new String(requestBody.readAllBytes(), StandardCharsets.UTF_8);
        
        // Parse JSON to DishDTO
        DishDTO dishDTO = objectMapper.readValue(body, DishDTO.class);
        
        // Validate required fields
        if (dishDTO.getName() == null || dishDTO.getName().isEmpty()) {
            sendResponse(exchange, 400, "{\"error\": \"Dish name is required\"}");
            return;
        }
        
        if (dishDTO.getPrice() <= 0) {
            sendResponse(exchange, 400, "{\"error\": \"Dish price must be positive\"}");
            return;
        }
        
        // Create domain object
        Dish dish = new Dish(
            dishDTO.getName(), 
            dishDTO.getDescription() != null ? dishDTO.getDescription() : "",
            dishDTO.getPrice()
        );
        
        // Set optional fields
        if (dishDTO.getCategory() != null) {
            try {
                dish.setCategory(DishCategory.valueOf(dishDTO.getCategory()));
            } catch (IllegalArgumentException e) {
                sendResponse(exchange, 400, "{\"error\": \"Invalid category: " + dishDTO.getCategory() + "\"}");
                return;
            }
        }
        
        if (dishDTO.getDishType() != null) {
            try {
                dish.setCuisineType(DishType.valueOf(dishDTO.getDishType()));
            } catch (IllegalArgumentException e) {
                sendResponse(exchange, 400, "{\"error\": \"Invalid dish type: " + dishDTO.getDishType() + "\"}");
                return;
            }
        }
        
        // Mock: In reality, save to repository
        System.out.println("✅ Dish created: " + dish);
        
        // Return created dish with ID
        dishDTO.setId((long) dish.hashCode());
        String jsonResponse = objectMapper.writeValueAsString(dishDTO);
        
        sendResponse(exchange, 201, jsonResponse);
    }
    
    private void sendResponse(HttpExchange exchange, int statusCode, String response) throws IOException {
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.sendResponseHeaders(statusCode, response.getBytes().length);
        
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(response.getBytes());
        }
    }
}