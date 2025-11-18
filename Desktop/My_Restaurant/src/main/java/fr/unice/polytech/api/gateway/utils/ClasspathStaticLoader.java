package fr.unice.polytech.api.gateway.utils;

import com.sun.net.httpserver.HttpExchange;

import java.io.InputStream;
import java.io.OutputStream;

public class ClasspathStaticLoader {

    public static boolean tryServe(String path, HttpExchange exchange) {
        try {
            String resource = "/frontEnd" + (path.startsWith("/") ? path : "/" + path);

            InputStream is = ClasspathStaticLoader.class.getResourceAsStream(resource);
            if (is == null) return false;

            String name = resource.substring(resource.lastIndexOf("/") + 1);
            String contentType = MimeTypes.fromFilename(name);

            byte[] bytes = is.readAllBytes();

            exchange.getResponseHeaders().set("Content-Type", contentType);
            exchange.sendResponseHeaders(200, bytes.length);

            try (OutputStream os = exchange.getResponseBody()) {
                os.write(bytes);
            }
            return true;

        } catch (Exception e) {
            return false;
        }
    }
}
