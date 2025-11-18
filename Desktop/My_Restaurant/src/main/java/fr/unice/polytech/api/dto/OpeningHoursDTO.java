package fr.unice.polytech.api.dto;


import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * DTO for restaurant opening hours
 * 
 * TD requirement: Filtrer par "disponibilité horaire"
 */
public class OpeningHoursDTO {
    
    @JsonProperty("day")
    private String day; // MONDAY, TUESDAY, WEDNESDAY, etc.
    
    @JsonProperty("openingTime")
    private String openingTime; // Format: "11:30"
    
    @JsonProperty("closingTime")
    private String closingTime; // Format: "14:00"
    
    // ========== Constructors ==========
    
    public OpeningHoursDTO() {
        // Required by Jackson
    }
    
    public OpeningHoursDTO(String day, String openingTime, String closingTime) {
        this.day = day;
        this.openingTime = openingTime;
        this.closingTime = closingTime;
    }
    
    // ========== Getters/Setters ==========
    
    public String getDay() {
        return day;
    }
    
    public void setDay(String day) {
        this.day = day;
    }
    
    public String getOpeningTime() {
        return openingTime;
    }
    
    public void setOpeningTime(String openingTime) {
        this.openingTime = openingTime;
    }
    
    public String getClosingTime() {
        return closingTime;
    }
    
    public void setClosingTime(String closingTime) {
        this.closingTime = closingTime;
    }
    
    // ========== toString ==========
    
    @Override
    public String toString() {
        return "OpeningHoursDTO{" +
                "day='" + day + '\'' +
                ", openingTime='" + openingTime + '\'' +
                ", closingTime='" + closingTime + '\'' +
                '}';
    }
}