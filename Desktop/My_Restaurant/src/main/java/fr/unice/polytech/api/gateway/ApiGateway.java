package fr.unice.polytech.api.gateway;

import com.sun.net.httpserver.HttpServer;
import fr.unice.polytech.api.gateway.utils.StaticFileHandler;

import java.io.IOException;
import java.net.InetSocketAddress;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.logging.Logger;

public class ApiGateway {

    private static final int PORT = 8080;
    public static final Path FRONTEND_DIR = Paths.get("src", "main", "frontEnd").toAbsolutePath();
    private static final Logger LOGGER = Logger.getLogger(ApiGateway.class.getName());

    public static void main(String[] args) throws IOException {

        HttpServer server = HttpServer.create(new InetSocketAddress(PORT), 0);

        server.createContext("/api", GatewayRouter::routeRequest);
        server.createContext("/", StaticFileHandler::handle);

        server.start();

        LOGGER.info("🚪 API Gateway started on http://localhost:" + PORT);
        LOGGER.info("📂 Serving static files from: " + FRONTEND_DIR);
        LOGGER.info("📍 Routes handled by GatewayRouter.");
    }
}
