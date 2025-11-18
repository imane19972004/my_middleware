package fr.unice.polytech.api.gateway.utils;

import java.nio.file.Path;

public class FileSecurity {

    public static boolean isSafe(Path base, Path resolved) {
        return resolved.normalize().startsWith(base);
    }
}
