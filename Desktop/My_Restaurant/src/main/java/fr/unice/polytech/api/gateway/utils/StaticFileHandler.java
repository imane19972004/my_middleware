package fr.unice.polytech.api.gateway.utils;

import com.sun.net.httpserver.HttpExchange;

import java.io.*;
import java.net.URI;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;

import static fr.unice.polytech.api.gateway.ApiGateway.FRONTEND_DIR;

public class StaticFileHandler {

    public static void handle(HttpExchange exchange) throws IOException {

        if (!exchange.getRequestMethod().equalsIgnoreCase("GET") &&
                !exchange.getRequestMethod().equalsIgnoreCase("HEAD")) {
            exchange.sendResponseHeaders(405, -1);
            return;
        }

        URI uri = exchange.getRequestURI();
        String rawPath = uri.getPath();

        if (rawPath != null && rawPath.startsWith("/api")) {
            exchange.sendResponseHeaders(404, -1);
            return;
        }

        String path = (rawPath == null || rawPath.equals("/"))
                ? "/index.html"
                : URLDecoder.decode(rawPath, StandardCharsets.UTF_8);

        Path resolvedPath = FRONTEND_DIR.resolve(path.substring(1)).normalize();

        if (!FileSecurity.isSafe(FRONTEND_DIR, resolvedPath)) {
            exchange.sendResponseHeaders(403, -1);
            return;
        }

        File file = resolvedPath.toFile();

        // If file not found → try fallback classpath or index.html fallback
        if (!file.exists() || file.isDirectory()) {

            if (!ClasspathStaticLoader.tryServe(path, exchange)) {

                Path fallback = FRONTEND_DIR.resolve("index.html");
                if (Files.exists(fallback)) file = fallback.toFile();
                else {
                    exchange.sendResponseHeaders(404, -1);
                    return;
                }
            } else return;
        }

        String contentType = MimeTypes.fromFilename(file.getName());
        exchange.getResponseHeaders().set("Content-Type", contentType);
        exchange.getResponseHeaders().set("Cache-Control", "max-age=3600");

        byte[] bytes = Files.readAllBytes(file.toPath());
        exchange.sendResponseHeaders(200, bytes.length);

        try (OutputStream os = exchange.getResponseBody()) {
            os.write(bytes);
        }
    }
}
