package fr.unice.polytech.services.order.handlers;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import fr.unice.polytech.api.dto.OrderDTO;
import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderManager;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.restaurants.RestaurantManager;
import fr.unice.polytech.services.order.mappers.OrderMapper;
import fr.unice.polytech.users.DeliveryLocation;
import fr.unice.polytech.users.StudentAccount;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * Handler for Order endpoints
 * 
 * POST /api/orders - Create a new order
 * GET  /api/orders/{id} - Get order by ID
 */
public class OrderHandler implements HttpHandler {
    
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final OrderManager orderManager;
    private final RestaurantManager restaurantManager;
    
    // Mock student for testing
    private final StudentAccount mockStudent;
    
    public OrderHandler(OrderManager orderManager, RestaurantManager restaurantManager) {
        this.orderManager = orderManager;
        this.restaurantManager = restaurantManager;
        
        // Create mock student with delivery location
        DeliveryLocation location = new DeliveryLocation(
            "Campus Sophia", 
            "930 Route des Colles", 
            "Biot", 
            "06410"
        );
        
        this.mockStudent = new StudentAccount.Builder("John", "Doe")
            .email("john.doe@etu.unice.fr")
            .studentId("S12345")
            .balance(100.0)
            .addDeliveryLocation(location)
            .bankInfo("1234567890123456", 123, 12, 2026)
            .build();
        
        // Create mock restaurants
        initializeMockRestaurants();
    }
    
    @Override
    public void handle(HttpExchange exchange) throws IOException {
        String method = exchange.getRequestMethod();
        String path = exchange.getRequestURI().getPath();
        
        try {
            if ("POST".equals(method)) {
                handleCreateOrder(exchange);
            } else if ("GET".equals(method) && path.matches("/api/orders/\\d+")) {
                handleGetOrder(exchange);
            } else {
                sendResponse(exchange, 405, "{\"error\": \"Method not allowed\"}");
            }
        } catch (Exception e) {
            e.printStackTrace();
            sendResponse(exchange, 500, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleCreateOrder(HttpExchange exchange) throws IOException {
        InputStream requestBody = exchange.getRequestBody();
        String body = new String(requestBody.readAllBytes(), StandardCharsets.UTF_8);
        
        OrderDTO orderDTO = objectMapper.readValue(body, OrderDTO.class);
        
        // Validate required fields
        if (orderDTO.getRestaurantId() == null) {
            sendResponse(exchange, 400, "{\"error\": \"Restaurant ID is required\"}");
            return;
        }
        
        if (orderDTO.getDishes() == null || orderDTO.getDishes().isEmpty()) {
            sendResponse(exchange, 400, "{\"error\": \"Order must contain at least one dish\"}");
            return;
        }
        
        // Find restaurant
        Restaurant restaurant = findRestaurantById(orderDTO.getRestaurantId());
        if (restaurant == null) {
            sendResponse(exchange, 404, "{\"error\": \"Restaurant not found\"}");
            return;
        }
        
        // Convert dish DTOs to domain objects
        List<Dish> dishes = new ArrayList<>();
        for (var dishDTO : orderDTO.getDishes()) {
            Dish dish = restaurant.findDishByName(dishDTO.getName());
            if (dish == null) {
                // If not found, create a simple dish from DTO
                dish = new Dish(dishDTO.getName(), dishDTO.getPrice());
            }
            dishes.add(dish);
        }
        
        // Get delivery location (use first one for mock)
        DeliveryLocation deliveryLocation = mockStudent.getDeliveryLocations().get(0);
        
        try {
            // Create order
            orderManager.createOrder(dishes, mockStudent, deliveryLocation, restaurant);
            
            // Get the pending order (last created)
            List<Order> pendingOrders = orderManager.getPendingOrders();
            if (pendingOrders.isEmpty()) {
                sendResponse(exchange, 500, "{\"error\": \"Failed to create order\"}");
                return;
            }
            
            Order createdOrder = pendingOrders.get(pendingOrders.size() - 1);
            
            // Convert to DTO
            OrderDTO responseDTO = OrderMapper.toDTO(createdOrder);
            String jsonResponse = objectMapper.writeValueAsString(responseDTO);
            
            sendResponse(exchange, 201, jsonResponse);
            
        } catch (IllegalArgumentException e) {
            sendResponse(exchange, 400, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleGetOrder(HttpExchange exchange) throws IOException {
        String path = exchange.getRequestURI().getPath();
        String idStr = path.substring(path.lastIndexOf('/') + 1);
        
        try {
            long id = Long.parseLong(idStr);
            
            // Search in registered orders
            Optional<Order> order = orderManager.getRegisteredOrders().stream()
                .filter(o -> o.hashCode() == id)
                .findFirst();
            
            // If not found, search in pending orders
            if (!order.isPresent()) {
                order = orderManager.getPendingOrders().stream()
                    .filter(o -> o.hashCode() == id)
                    .findFirst();
            }
            
            if (order.isPresent()) {
                OrderDTO dto = OrderMapper.toDTO(order.get());
                String jsonResponse = objectMapper.writeValueAsString(dto);
                sendResponse(exchange, 200, jsonResponse);
            } else {
                sendResponse(exchange, 404, "{\"error\": \"Order not found\"}");
            }
        } catch (NumberFormatException e) {
            sendResponse(exchange, 400, "{\"error\": \"Invalid order ID\"}");
        }
    }
    
    private Restaurant findRestaurantById(Long id) {
        return restaurantManager.getAllRestaurants().stream()
            .filter(r -> r.hashCode() == id)
            .findFirst()
            .orElse(null);
    }
    
    private void sendResponse(HttpExchange exchange, int statusCode, String response) throws IOException {
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.sendResponseHeaders(statusCode, response.getBytes().length);
        
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(response.getBytes());
        }
    }
    
    private void initializeMockRestaurants() {
        // Italian Restaurant
        Restaurant italian = new Restaurant("La Bella Vita");
        italian.addDish("Margherita Pizza", "Classic tomato and mozzarella", 12.50);
        italian.addDish("Carbonara", "Creamy pasta with bacon", 14.00);
        restaurantManager.addRestaurant(italian);
        
        // Japanese Restaurant
        Restaurant japanese = new Restaurant("Sakura Sushi");
        japanese.addDish("California Roll", "Fresh sushi roll", 13.00);
        japanese.addDish("Miso Soup", "Traditional soup", 4.50);
        restaurantManager.addRestaurant(japanese);
        
        // Print restaurant IDs for testing
        System.out.println("\n📋 Available Restaurants:");
        for (Restaurant r : restaurantManager.getAllRestaurants()) {
            System.out.println("   ID: " + r.hashCode() + " - " + r.getRestaurantName());
        }
        System.out.println();
    }
}