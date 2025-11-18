package fr.unice.polytech.services.order.handlers;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderManager;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.paymentProcessing.PaymentMethod;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.util.Optional;

/**
 * Payment Handler - Acts as a proxy to payment services
 * 
 * POST /api/payment
 * 
 * TD requirement: "Pour le paiement lui-même, un accès à un proxy 
 * vers un service potentiel est suffisant (P1)"
 */
public class PaymentHandler implements HttpHandler {
    
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final OrderManager orderManager;
    
    public PaymentHandler(OrderManager orderManager) {
        this.orderManager = orderManager;
    }
    
    @Override
    public void handle(HttpExchange exchange) throws IOException {
        String method = exchange.getRequestMethod();
        
        try {
            if ("POST".equals(method)) {
                handleProcessPayment(exchange);
            } else {
                sendResponse(exchange, 405, "{\"error\": \"Method not allowed\"}");
            }
        } catch (Exception e) {
            e.printStackTrace();
            sendResponse(exchange, 500, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void handleProcessPayment(HttpExchange exchange) throws IOException {
        InputStream requestBody = exchange.getRequestBody();
        String body = new String(requestBody.readAllBytes(), StandardCharsets.UTF_8);
        
        JsonNode jsonNode = objectMapper.readTree(body);
        
        // Extract payment info
        if (!jsonNode.has("orderId") || !jsonNode.has("paymentMethod")) {
            sendResponse(exchange, 400, "{\"error\": \"Order ID and payment method are required\"}");
            return;
        }
        
        long orderId = jsonNode.get("orderId").asLong();
        String paymentMethodStr = jsonNode.get("paymentMethod").asText();
        
        // Validate payment method
        PaymentMethod paymentMethod;
        try {
            paymentMethod = PaymentMethod.valueOf(paymentMethodStr.toUpperCase());
        } catch (IllegalArgumentException e) {
            sendResponse(exchange, 400, "{\"error\": \"Invalid payment method. Use INTERNAL or EXTERNAL\"}");
            return;
        }
        
        // Find the order
        Optional<Order> orderOpt = orderManager.getPendingOrders().stream()
            .filter(o -> o.hashCode() == orderId)
            .findFirst();
        
        if (!orderOpt.isPresent()) {
            sendResponse(exchange, 404, "{\"error\": \"Order not found or already processed\"}");
            return;
        }
        
        Order order = orderOpt.get();
        
        try {
            // Process payment via OrderManager
            orderManager.initiatePayment(order, paymentMethod);
            
            // Register order if payment succeeded
            boolean registered = orderManager.registerOrder(order, order.getRestaurant());
            
            // Build response
            OrderStatus finalStatus = order.getOrderStatus();
            String message;
            int statusCode;
            
            if (finalStatus == OrderStatus.VALIDATED) {
                message = "Payment successful. Order validated.";
                statusCode = 200;
            } else if (finalStatus == OrderStatus.CANCELED) {
                message = "Payment failed. Order canceled.";
                statusCode = 402; // Payment Required
            } else {
                message = "Payment processing. Order pending.";
                statusCode = 202; // Accepted
            }
            
            String jsonResponse = objectMapper.writeValueAsString(Map.of(
                "orderId", orderId,
                "status", finalStatus.toString(),
                "message", message,
                "paymentMethod", paymentMethod.toString(),
                "registered", registered
            ));
            
            sendResponse(exchange, statusCode, jsonResponse);
            
        } catch (IllegalArgumentException e) {
            sendResponse(exchange, 400, "{\"error\": \"" + e.getMessage() + "\"}");
        }
    }
    
    private void sendResponse(HttpExchange exchange, int statusCode, String response) throws IOException {
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.sendResponseHeaders(statusCode, response.getBytes().length);
        
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(response.getBytes());
        }
    }
    
    // Helper to create Map easily
    private static class Map<K, V> extends java.util.HashMap<K, V> {
        public static <K, V> Map<K, V> of(K k1, V v1, K k2, V v2, K k3, V v3, K k4, V v4, K k5, V v5) {
            Map<K, V> map = new Map<>();
            map.put(k1, v1);
            map.put(k2, v2);
            map.put(k3, v3);
            map.put(k4, v4);
            map.put(k5, v5);
            return map;
        }
    }
}