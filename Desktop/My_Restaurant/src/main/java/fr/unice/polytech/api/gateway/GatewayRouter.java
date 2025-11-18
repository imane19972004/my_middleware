package fr.unice.polytech.api.gateway;

import com.sun.net.httpserver.HttpExchange;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.List;
import java.util.Map;
import java.util.logging.Logger;

/**
 * GatewayRouter - Handles request routing and forwarding to appropriate microservices
 */
public class GatewayRouter {

    private static final Logger LOGGER = Logger.getLogger(GatewayRouter.class.getName());
    private static final String CATALOG_BASE_URL = "http://127.0.0.1:8081";
    private static final String ORDER_BASE_URL = "http://127.0.0.1:8082";

    /**
     * Routes the incoming API request to the appropriate microservice
     *
     * @param exchange The HTTP exchange object
     * @throws IOException if an I/O error occurs
     */
    public static void routeRequest(HttpExchange exchange) throws IOException {
        String path = exchange.getRequestURI().getPath();
        String method = exchange.getRequestMethod();

        LOGGER.info("➡️ Incoming request: " + method + " " + path);

        // Handle CORS preflight
        if ("OPTIONS".equalsIgnoreCase(method)) {
            addCORSHeaders(exchange);
            exchange.sendResponseHeaders(204, -1);
            exchange.close();
            return;
        }

        // Determine target service based on path
        String targetBaseUrl = determineTargetService(path);

        if (targetBaseUrl == null) {
            exchange.sendResponseHeaders(404, -1);
            exchange.close();
            LOGGER.warning("⚠️ No service found for path: " + path);
            return;
        }

        forwardRequest(exchange, targetBaseUrl + path);
    }

    /**
     * Determines which microservice should handle the request based on the path
     *
     * @param path The request path
     * @return The base URL of the target service, or null if no match
     */
    private static String determineTargetService(String path) {
        if (path.startsWith("/api/restaurants") || path.startsWith("/api/dishes")) {
            return CATALOG_BASE_URL;
        } else if (path.startsWith("/api/orders")
                || path.startsWith("/api/timeslots")
                || path.startsWith("/api/payment")) {
            return ORDER_BASE_URL;
        }
        return null;
    }

    /**
     * Forwards the request to the target microservice
     *
     * @param exchange  The HTTP exchange object
     * @param targetUrl The full URL of the target service
     * @throws IOException if an I/O error occurs
     */
    private static void forwardRequest(HttpExchange exchange, String targetUrl) throws IOException {
        // Append query parameters if they exist
        String query = exchange.getRequestURI().getQuery();
        if (query != null && !query.isEmpty()) {
            targetUrl += "?" + query;
        }

        URL url = new URL(targetUrl);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod(exchange.getRequestMethod());

        // Copy headers from client to target service
        copyRequestHeaders(exchange, conn);

        // Copy body for POST/PUT/PATCH requests
        if (List.of("POST", "PUT", "PATCH").contains(exchange.getRequestMethod())) {
            conn.setDoOutput(true);
            try (OutputStream os = conn.getOutputStream();
                 InputStream is = exchange.getRequestBody()) {
                is.transferTo(os);
            }
        }

        // Read response from target service
        int status = conn.getResponseCode();
        InputStream responseStream = (status < HttpURLConnection.HTTP_BAD_REQUEST)
                ? conn.getInputStream()
                : conn.getErrorStream();

        byte[] responseBytes = responseStream != null ? responseStream.readAllBytes() : new byte[0];

        // Set response headers
        addCORSHeaders(exchange);
        exchange.getResponseHeaders().set("Content-Type",
                conn.getContentType() != null ? conn.getContentType() : "application/json");

        // Send response to client
        exchange.sendResponseHeaders(status, responseBytes.length);
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(responseBytes);
        }
        exchange.close();

        LOGGER.info("✅ Forwarded " + exchange.getRequestMethod() + " "
                + exchange.getRequestURI() + " → " + targetUrl + " (Status: " + status + ")");
    }

    /**
     * Copies request headers from the client to the target service connection
     *
     * @param exchange The HTTP exchange object
     * @param conn     The HttpURLConnection to the target service
     */
    private static void copyRequestHeaders(HttpExchange exchange, HttpURLConnection conn) {
        for (Map.Entry<String, List<String>> header : exchange.getRequestHeaders().entrySet()) {
            for (String value : header.getValue()) {
                conn.addRequestProperty(header.getKey(), value);
            }
        }
    }

    /**
     * Adds CORS headers to the response
     *
     * @param exchange The HTTP exchange object
     */
    private static void addCORSHeaders(HttpExchange exchange) {
        exchange.getResponseHeaders().add("Access-Control-Allow-Origin", "*");
        exchange.getResponseHeaders().add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        exchange.getResponseHeaders().add("Access-Control-Allow-Headers", "Content-Type, Authorization");
    }
}