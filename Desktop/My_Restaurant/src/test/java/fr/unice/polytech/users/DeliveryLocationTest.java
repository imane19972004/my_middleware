package fr.unice.polytech.users;

import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

class DeliveryLocationTest {

    private final String NAME = "Office";
    private final String ADDRESS = "123 Main St";
    private final String CITY = "Nice";
    private final String ZIP = "06000";

    // Test 1: Constructor and getters
    @Test
    void testDeliveryLocationCreationAndGetters() {
        DeliveryLocation location = new DeliveryLocation(NAME, ADDRESS, CITY, ZIP);
        
        assertEquals(NAME, location.getName(), "Name should be set correctly.");
        assertEquals(ADDRESS, location.getAddress(), "Address should be set correctly.");
        assertEquals(CITY, location.getCity(), "City should be set correctly.");
        assertEquals(ZIP, location.getZipCode(), "Zip code should be set correctly.");
    }

    // Test 2: Setters
    @Test
    void testSetters() {
        DeliveryLocation location = new DeliveryLocation("Temp", "Temp", "Temp", "Temp");
        
        location.setName("Home");
        location.setAddress("456 Side Ave");
        location.setCity("Cannes");
        location.setZipCode("06400");
        
        assertEquals("Home", location.getName(), "Name setter should update correctly.");
        assertEquals("456 Side Ave", location.getAddress(), "Address setter should update correctly.");
        assertEquals("Cannes", location.getCity(), "City setter should update correctly.");
        assertEquals("06400", location.getZipCode(), "Zip code setter should update correctly.");
    }

    // Test 3: toString representation
    @Test
    void testToString() {
        DeliveryLocation location = new DeliveryLocation(NAME, ADDRESS, CITY, ZIP);
        String expected = "Office: 123 Main St, 06000 Nice";
        
        assertEquals(expected, location.toString(), "toString should format the address correctly.");
    }
}