package fr.unice.polytech.restaurants;

import java.time.DayOfWeek;
import java.time.LocalTime;

public class OpeningHours {
    private DayOfWeek day;
    private LocalTime openingTime;
    private LocalTime closingTime;

    public OpeningHours(DayOfWeek day, LocalTime openingTime, LocalTime closingTime) {
        if (openingTime.isAfter(closingTime)) {
            throw new IllegalArgumentException("Opening time cannot be after closing time");
        }
        this.day = day;
        this.openingTime = openingTime;
        this.closingTime = closingTime;
    }

    public DayOfWeek getDay() {
        return day;
    }

    public LocalTime getOpeningTime() {
        return openingTime;
    }

    public LocalTime getClosingTime() {
        return closingTime;
    }

    @Override
    public String toString() {
        return day + ": " + openingTime + " - " + closingTime;
    }
}
