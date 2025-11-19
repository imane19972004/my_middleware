package fr.unice.polytech.services.catalog;

import com.sun.net.httpserver.HttpServer;
import fr.unice.polytech.services.catalog.handlers.DishHandler;
import fr.unice.polytech.services.catalog.handlers.RestaurantHandler;

import java.io.IOException;
import java.net.InetSocketAddress;

/**
 * Catalog Service - REST API for restaurants and dishes
 * Port: 8081
 * 
 * Endpoints:
 * - GET  /api/restaurants?cuisineType=ITALIAN&hasVegetarian=true
 * - GET  /api/restaurants/{id}
 * - POST /api/dishes
 */
public class CatalogServer {
    
    private static final int PORT = 8081;
    private final HttpServer server;
    
    public CatalogServer() throws IOException {
        server = HttpServer.create(new InetSocketAddress(PORT), 0);
        registerHandlers();
    }
    
    private void registerHandlers() {
        // Restaurant endpoints
        server.createContext("/api/restaurants", new RestaurantHandler());
        
        // Dish endpoints
        server.createContext("/api/dishes", new DishHandler());
    }
    
    public void start() {
        server.setExecutor(null); // creates a default executor
        server.start();
        System.out.println("Catalog Service started on port " + PORT);
        System.out.println(" Available endpoints:");
        System.out.println("   GET  http://localhost:" + PORT + "/api/restaurants");
        System.out.println("   GET  http://localhost:" + PORT + "/api/restaurants/{id}");
        System.out.println("   POST http://localhost:" + PORT + "/api/dishes");
    }
    
    public void stop() {
        server.stop(0);
        System.out.println(" Catalog Service stopped");
    }
    
    public static void main(String[] args) {
        try {
            CatalogServer catalogServer = new CatalogServer();
            catalogServer.start();
            
            // Keep server running
            System.out.println("\nPress Enter to stop the server...");
            System.in.read();
            catalogServer.stop();
            
        } catch (IOException e) {
            System.err.println("Failed to start Catalog Service: " + e.getMessage());
            e.printStackTrace();
        }
    }
}



/*Centraliser la gestion: */
//=======Encapsulation des appels API ======
//regrouper toute la logique des requêtes API au même endroit, au lieu de mélanger ça avec le reste du code.
//Pour etre plus organisé et plus facile à maintenir.
//Ceci à travers des handlers qui s'occupent des partie de l'API (un handler = une seule partie de l'API).



//======Logging pour la gestion des erreurs ======
//Tous les handlers contient des loggings pour suivre les requêtes et les erreurs.
//Ceci est dans les blocs catch 'e.printStackTrace()'