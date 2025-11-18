package fr.unice.polytech.restaurants;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.dishes.DishCategory;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;

import java.time.DayOfWeek;
import java.time.LocalTime;
import java.util.Arrays;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("Restaurant Tests")
class RestaurantTest {

    private Restaurant restaurant;
    private Dish pizza;
    private Dish pasta;
    private TimeSlot slot1;
    private TimeSlot slot2;
    private OpeningHours mondayHours;
    private OpeningHours mondayUpdatedHours;


    @BeforeEach
    void setUp() {
        restaurant = new Restaurant("La Bella Italia");

        // Create sample dishes
        pizza = new Dish("Margherita", "Classic pizza with tomato and mozzarella", 12.50);
        pizza.setCategory(DishCategory.MAIN_COURSE);

        pasta = new Dish("Carbonara", "Pasta with eggs, cheese, and bacon", 10.00);
        pasta.setCategory(DishCategory.MAIN_COURSE);

        // Create sample time slots
        slot1 = new TimeSlot(LocalTime.of(12, 0), LocalTime.of(12, 30));
        slot2 = new TimeSlot(LocalTime.of(12, 30), LocalTime.of(13, 0));
        mondayHours = new OpeningHours(DayOfWeek.MONDAY, LocalTime.of(12, 0), LocalTime.of(14, 30));
        mondayUpdatedHours = new OpeningHours(DayOfWeek.MONDAY, LocalTime.of(19, 0), LocalTime.of(22, 0));

    }

    // ==================== CONSTRUCTOR TESTS ====================

    @Nested
    @DisplayName("Constructor Tests")
    class ConstructorTests {

        @Test
        @DisplayName("Should create restaurant with valid name")
        void shouldCreateRestaurantWithValidName() {
            Restaurant r = new Restaurant("Test Restaurant");
            assertEquals("Test Restaurant", r.getRestaurantName());
            assertNotNull(r.getDishes());
            assertTrue(r.getDishes().isEmpty());
            assertTrue(r.getAvailableTimeSlots().isEmpty());
        }

        @Test
        @DisplayName("Should throw exception when name is null")
        void shouldThrowExceptionWhenNameIsNull() {
            Exception exception = assertThrows(IllegalArgumentException.class,
                    () -> new Restaurant(null));
            assertEquals("Restaurant name cannot be null or empty", exception.getMessage());
        }

        @Test
        @DisplayName("Should throw exception when name is empty")
        void shouldThrowExceptionWhenNameIsEmpty() {
            Exception exception = assertThrows(IllegalArgumentException.class,
                    () -> new Restaurant(""));
            assertEquals("Restaurant name cannot be null or empty", exception.getMessage());
        }
    }

    // ==================== BUILDER PATTERN TESTS ====================

    @Nested
    @DisplayName("Builder Pattern Tests")
    class BuilderTests {

        @Test
        @DisplayName("Should build restaurant with dishes using builder")
        void shouldBuildRestaurantWithDishes() {
            Restaurant r = new Restaurant.Builder("Pizza Place")
                    .withDish(pizza)
                    .withDish(pasta)
                    .build();

            assertEquals("Pizza Place", r.getRestaurantName());
            assertEquals(2, r.getDishes().size());
            assertTrue(r.getDishes().contains(pizza));
            assertTrue(r.getDishes().contains(pasta));
        }

        @Test
        @DisplayName("Should build restaurant with list of dishes")
        void shouldBuildRestaurantWithDishList() {
            List<Dish> dishes = Arrays.asList(pizza, pasta);
            Restaurant r = new Restaurant.Builder("Italian Restaurant")
                    .withDishes(dishes)
                    .build();

            assertEquals(2, r.getDishes().size());
        }

        @Test
        @DisplayName("Should build restaurant with time slots")
        void shouldBuildRestaurantWithTimeSlots() {
            Restaurant r = new Restaurant.Builder("Time Restaurant")
                    .withTimeSlot(slot1)
                    .withTimeSlot(slot2)
                    .build();

            assertEquals("Time Restaurant", r.getRestaurantName());
        }

        @Test
        @DisplayName("Should ignore null values in builder")
        void shouldIgnoreNullValuesInBuilder() {
            Restaurant r = new Restaurant.Builder("Null Test")
                    .withDish(null)
                    .withTimeSlot(null)
                    .build();

            assertTrue(r.getDishes().isEmpty());
        }

        @Test
        @DisplayName("Should throw exception when builder name is null")
        void shouldThrowExceptionWhenBuilderNameIsNull() {
            assertThrows(IllegalArgumentException.class,
                    () -> new Restaurant.Builder(null));
        }
    }

    // ==================== DISH MANAGEMENT TESTS ====================

    @Nested
    @DisplayName("Dish Management Tests")
    class DishManagementTests {

        @Test
        @DisplayName("Should add dish successfully")
        void shouldAddDishSuccessfully() {

            assertEquals(0, restaurant.getDishes().size());
            restaurant.addDish("Margherita", "Classic pizza with tomato and mozzarella", 12.50);
            assertEquals(1, restaurant.getDishes().size());
        }




        /*
        *
        @Test
        @DisplayName("Should add multiple dishes at once")
        void shouldAddMultipleDishes() {
            List<Dish> dishes = Arrays.asList(pizza, pasta);
            restaurant.addDishes(dishes);
            assertEquals(2, restaurant.getDishes().size());
        }
        @Test
        @DisplayName("Should throw exception when adding null dish list")
        void shouldThrowExceptionWhenAddingNullDishList() {
            assertThrows(IllegalArgumentException.class,
                    () -> restaurant.addDishes(null));
        }
        * */



        @Test
        @DisplayName("Should update existing dish")
        void shouldUpdateExistingDish() {
            restaurant.addDish("Margherita", "Classic pizza with tomato and mozzarella", 12.50);
            Dish pizza = restaurant.getDishes().get(0);
            restaurant.updateDish(pizza, "new Description");


            assertEquals(restaurant.getDishes().get(0).getDescription(),"new Description");
            assertEquals(1, restaurant.getDishes().size());
        }



        @Test
        @DisplayName("Should return immutable copy of dishes")
        void shouldReturnImmutableCopyOfDishes() {
            restaurant.addDish("Margherita", "Classic pizza with tomato and mozzarella", 12.50);
            List<Dish> dishes = restaurant.getDishes();

            // Modifying returned list should not affect restaurant's internal list
            dishes.add(pasta);

            assertEquals(1, restaurant.getDishes().size());
        }
    }

    // ==================== CAPACITY MANAGEMENT TESTS ====================

    @Nested
    @DisplayName("Capacity Management Tests")
    class CapacityManagementTests {

        @Test
        @DisplayName("Should set capacity for time slot")
        void shouldSetCapacityForTimeSlot() {
            restaurant.setCapacity(slot1, 10);
            assertEquals(10, restaurant.getCapacity(slot1));
        }

        @Test
        @DisplayName("Should return 0 for non-existent time slot")
        void shouldReturnZeroForNonExistentTimeSlot() {
            assertEquals(0, restaurant.getCapacity(slot1));
        }

        @Test
        @DisplayName("Should throw exception when setting capacity with null slot")
        void shouldThrowExceptionWhenSettingCapacityWithNullSlot() {
            assertThrows(IllegalArgumentException.class,
                    () -> restaurant.setCapacity(null, 10));
        }

        @Test
        @DisplayName("Should throw exception when setting negative capacity")
        void shouldThrowExceptionWhenSettingNegativeCapacity() {
            assertThrows(IllegalArgumentException.class,
                    () -> restaurant.setCapacity(slot1, -5));
        }

        @Test
        @DisplayName("Should decrease capacity correctly")
        void shouldDecreaseCapacityCorrectly() {
            restaurant.setCapacity(slot1, 10);
            restaurant.decreaseCapacity(slot1);
            assertEquals(9, restaurant.getCapacity(slot1));
        }

        @Test
        @DisplayName("Should not decrease capacity below zero")
        void shouldNotDecreaseCapacityBelowZero() {
            restaurant.setCapacity(slot1, 1);
            restaurant.decreaseCapacity(slot1);
            assertEquals(0, restaurant.getCapacity(slot1));
            // Tenter de diminuer encore
            restaurant.decreaseCapacity(slot1);
            assertEquals(0, restaurant.getCapacity(slot1)); // Reste à 0
        }

        @Test
        @DisplayName("Should increase capacity correctly")
        void shouldIncreaseCapacityCorrectly() {
            restaurant.setCapacity(slot1, 10);
            restaurant.increaseCapacity(slot1);
            assertEquals(11, restaurant.getCapacity(slot1));
        }

        @Test
        @DisplayName("Should increase capacity from zero")
        void shouldIncreaseCapacityFromZero() {
            restaurant.increaseCapacity(slot1);
            assertEquals(1, restaurant.getCapacity(slot1)); // Default is 5, +1 = 6
        }

        @Test
        @DisplayName("Should get all capacities")
        void shouldGetAllCapacities() {
            restaurant.setCapacity(slot1, 10);
            restaurant.setCapacity(slot2, 15);

            Map<TimeSlot, Integer> capacities = restaurant.getAllCapacities();

            assertEquals(2, capacities.size());
            assertEquals(10, capacities.get(slot1));
            assertEquals(15, capacities.get(slot2));
        }

        @Test
        @DisplayName("Should return only available time slots with capacity > 0")
        void shouldReturnOnlyAvailableTimeSlots() {
            restaurant.setCapacity(slot1, 10);
            restaurant.setCapacity(slot2, 0);

            List<TimeSlot> availableSlots = restaurant.getAvailableTimeSlots();

            assertEquals(1, availableSlots.size());
            assertTrue(availableSlots.contains(slot1));
            assertFalse(availableSlots.contains(slot2));
        }
    }

    @Test
    @DisplayName("Should block time slot by reducing capacity to zero")
    void shouldBlockTimeSlotByReducingCapacity() {
        restaurant.setCapacity(slot1, 5);

        // Bloquer en réduisant la capacité
        for (int i = 0; i < 5; i++) {
            restaurant.blockTimeSlot(slot1);
        }

        assertEquals(0, restaurant.getCapacity(slot1));
        assertFalse(restaurant.getAvailableTimeSlots().contains(slot1));
    }

    @Test
    @DisplayName("Should unblock time slot by increasing capacity")
    void shouldUnblockTimeSlotByIncreasingCapacity() {
        restaurant.setCapacity(slot1, 0);
        restaurant.unblockTimeSlot(slot1);
        assertTrue(restaurant.getCapacity(slot1) > 0);
        assertTrue(restaurant.getAvailableTimeSlots().contains(slot1));
    }

    // ==================== SETTER TESTS ====================

    @Nested
    @DisplayName("Setter Tests")
    class SetterTests {

        @Test
        @DisplayName("Should set restaurant name successfully")
        void shouldSetRestaurantNameSuccessfully() {
            restaurant.setRestaurantName("New Name");
            assertEquals("New Name", restaurant.getRestaurantName());
        }

        @Test
        @DisplayName("Should throw exception when setting null name")
        void shouldThrowExceptionWhenSettingNullName() {
            assertThrows(IllegalArgumentException.class,
                    () -> restaurant.setRestaurantName(null));
        }

        @Test
        @DisplayName("Should throw exception when setting empty name")
        void shouldThrowExceptionWhenSettingEmptyName() {
            assertThrows(IllegalArgumentException.class,
                    () -> restaurant.setRestaurantName(""));
        }
    }

    // ==================== EQUALS AND HASHCODE TESTS ====================

    @Nested
    @DisplayName("Equals and HashCode Tests")
    class EqualsAndHashCodeTests {

        @Test
        @DisplayName("Should be equal when names are same")
        void shouldBeEqualWhenNamesAreSame() {
            Restaurant r1 = new Restaurant("Test");
            Restaurant r2 = new Restaurant("Test");

            assertEquals(r1, r2);
            assertEquals(r1.hashCode(), r2.hashCode());
        }

        @Test
        @DisplayName("Should not be equal when names are different")
        void shouldNotBeEqualWhenNamesAreDifferent() {
            Restaurant r1 = new Restaurant("Test1");
            Restaurant r2 = new Restaurant("Test2");

            assertNotEquals(r1, r2);
        }

        @Test
        @DisplayName("Should be equal to itself")
        void shouldBeEqualToItself() {
            assertEquals(restaurant, restaurant);
        }

        @Test
        @DisplayName("Should not be equal to null")
        void shouldNotBeEqualToNull() {
            assertNotEquals(restaurant, null);
        }

        @Test
        @DisplayName("Should not be equal to different class")
        void shouldNotBeEqualToDifferentClass() {
            assertNotEquals(restaurant, "String");
        }
    }

    @Test
    void constructor_ShouldInitializeEmptyOpeningHoursList() {
        assertNotNull(restaurant.getOpeningHours());
        assertTrue(restaurant.getOpeningHours().isEmpty());
    }


    @Test
    void addOpeningHours_ShouldAddHours_WhenDayDoesNotExist() {
        restaurant.addOpeningHours(mondayHours);
        assertEquals(1, restaurant.getOpeningHours().size());
        assertTrue(restaurant.getOpeningHours().contains(mondayHours));
    }

    @Test
    void addOpeningHours_ShouldThrowException_WhenHoursAreNull() {
        assertThrows(IllegalArgumentException.class, () -> restaurant.addOpeningHours(null));
    }

    @Test
    void addOpeningHours_ShouldThrowException_WhenDayAlreadyExists() {
        restaurant.addOpeningHours(mondayHours);
        assertThrows(IllegalArgumentException.class, () -> restaurant.addOpeningHours(mondayUpdatedHours));
    }


    @Test
    void updateOpeningHours_ShouldUpdateHours_WhenDayExists() {
        restaurant.addOpeningHours(mondayHours);

        restaurant.updateOpeningHours(mondayUpdatedHours);

        assertEquals(1, restaurant.getOpeningHours().size());
        assertFalse(restaurant.getOpeningHours().contains(mondayHours));
        assertTrue(restaurant.getOpeningHours().contains(mondayUpdatedHours));
    }

    @Test
    void updateOpeningHours_ShouldThrowException_WhenHoursAreNull() {
        assertThrows(IllegalArgumentException.class, () -> restaurant.updateOpeningHours(null));
    }

    @Test
    void updateOpeningHours_ShouldThrowException_WhenDayDoesNotExist() {
        assertThrows(IllegalArgumentException.class, () -> restaurant.updateOpeningHours(mondayHours));
    }


    @Test
    void setOpeningHours_ShouldThrowException_WhenListIsNull() {
        assertThrows(IllegalArgumentException.class, () -> restaurant.setOpeningHours(null));
    }


    @Test
    void setOpeningHours_ShouldReplaceExistingListWithNewList() {
        OpeningHours monday = new OpeningHours(DayOfWeek.MONDAY, LocalTime.of(12, 0), LocalTime.of(14, 0));
        OpeningHours tuesday = new OpeningHours(DayOfWeek.TUESDAY, LocalTime.of(19, 0), LocalTime.of(22, 0));
        List<OpeningHours> newSchedule = List.of(monday, tuesday);

        restaurant.setOpeningHours(newSchedule);

        assertEquals(2, restaurant.getOpeningHours().size());
        assertEquals(newSchedule, restaurant.getOpeningHours());
    }


}