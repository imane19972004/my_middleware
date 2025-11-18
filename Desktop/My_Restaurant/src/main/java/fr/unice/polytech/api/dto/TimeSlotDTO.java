package fr.unice.polytech.api.dto;



import com.fasterxml.jackson.annotation.JsonProperty;

/**
 * DTO for time slot information
 * 
 * TD requirement: "visualisant les heures de livraisons possibles 
 * qui évoluent en fonction de la commande"
 */
public class TimeSlotDTO {
    
    @JsonProperty("startTime")
    private String startTime; // Format: "12:00"
    
    @JsonProperty("endTime")
    private String endTime;   // Format: "12:30"
    
    @JsonProperty("availableCapacity")
    private int availableCapacity;
    
    @JsonProperty("dayOfWeek")
    private String dayOfWeek; // MONDAY, TUESDAY, etc.
    
    // ========== Constructors ==========
    
    public TimeSlotDTO() {
        // Required by Jackson
    }
    
    public TimeSlotDTO(String startTime, String endTime, int availableCapacity) {
        this.startTime = startTime;
        this.endTime = endTime;
        this.availableCapacity = availableCapacity;
    }
    
    // ========== Getters/Setters ==========
    
    public String getStartTime() {
        return startTime;
    }
    
    public void setStartTime(String startTime) {
        this.startTime = startTime;
    }
    
    public String getEndTime() {
        return endTime;
    }
    
    public void setEndTime(String endTime) {
        this.endTime = endTime;
    }
    
    public int getAvailableCapacity() {
        return availableCapacity;
    }
    
    public void setAvailableCapacity(int availableCapacity) {
        this.availableCapacity = availableCapacity;
    }
    
    public String getDayOfWeek() {
        return dayOfWeek;
    }
    
    public void setDayOfWeek(String dayOfWeek) {
        this.dayOfWeek = dayOfWeek;
    }
    
    // ========== toString ==========
    
    @Override
    public String toString() {
        return "TimeSlotDTO{" +
                "startTime='" + startTime + '\'' +
                ", endTime='" + endTime + '\'' +
                ", availableCapacity=" + availableCapacity +
                ", dayOfWeek='" + dayOfWeek + '\'' +
                '}';
    }
}