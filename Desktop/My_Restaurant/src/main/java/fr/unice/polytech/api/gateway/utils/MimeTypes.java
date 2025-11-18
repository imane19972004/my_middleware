package fr.unice.polytech.api.gateway.utils;

public class MimeTypes {

    public static String fromFilename(String filename) {
        filename = filename.toLowerCase();

        if (filename.endsWith(".html") || filename.endsWith(".htm")) return "text/html; charset=UTF-8";
        if (filename.endsWith(".css")) return "text/css; charset=UTF-8";
        if (filename.endsWith(".js")) return "application/javascript; charset=UTF-8";
        if (filename.endsWith(".json")) return "application/json";
        if (filename.endsWith(".png")) return "image/png";
        if (filename.endsWith(".jpg") || filename.endsWith(".jpeg")) return "image/jpeg";
        if (filename.endsWith(".svg")) return "image/svg+xml";
        if (filename.endsWith(".gif")) return "image/gif";
        if (filename.endsWith(".ico")) return "image/x-icon";

        return "application/octet-stream";
    }
}
