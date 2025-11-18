package fr.unice.polytech.stepDefs.back;
import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderManager;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.paymentProcessing.*;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.restaurants.TimeSlot;
import fr.unice.polytech.users.DeliveryLocation;
import fr.unice.polytech.users.StudentAccount;
import io.cucumber.datatable.DataTable;
import io.cucumber.java.Before;
import io.cucumber.java.en.Given;
import io.cucumber.java.en.Then;
import io.cucumber.java.en.When;

import java.time.LocalTime;
import java.util.*;


import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

public class e2eWorkflowSteps {
    private StudentAccount currentStudent;
    private Restaurant currentRestaurant;
    private DeliveryLocation currentDeliveryLocation;
    private TimeSlot currentTimeSlot;
    private final Map<String, Dish> availableDishes = new HashMap<>();
    private final List<Dish> selectedDishes = new ArrayList<>();
    private Order currentOrder;
    private double initialBalance;

    private IPaymentService mockPaymentService;
    private PaymentProcessorFactory paymentProcessorFactory;
    private OrderManager orderManager;
    private int timeSlotCapacityBeforeSelection;
    private int timeSlotCapacityAfterSelection;

    @Before
    public void setupScenario() throws Exception {
        // Reset state before each scenario
        currentStudent = null;
        currentRestaurant = null;
        currentDeliveryLocation = null;
        currentTimeSlot = null;
        currentOrder = null;
        availableDishes.clear();
        selectedDishes.clear();

        mockPaymentService = mock(IPaymentService.class);
        paymentProcessorFactory = new PaymentProcessorFactory(mockPaymentService);
        orderManager = new OrderManager(paymentProcessorFactory);
        timeSlotCapacityBeforeSelection = -1;
        timeSlotCapacityAfterSelection = -1;

    }

    @Given("a student named {string} with ID {string} and email {string} exists")
    public void a_student_named_with_id_and_email_exists(String nameSurname, String studentId, String email) {
        String[] names = nameSurname.split(" ", 2);
        currentStudent = new StudentAccount.Builder(names[0], names[1])
                .studentId(studentId)
                .email(email)
                .build();
    }

    @Given("the student has a saved delivery location {string} at {string}")
    public void the_student_has_a_saved_delivery_location_at(String locName, String addressDetails) {
        String[] parts = addressDetails.split(", ");
        if (parts.length < 3) fail("Address details format is incorrect: " + addressDetails);
        currentDeliveryLocation = new DeliveryLocation(locName, parts[0], parts[1], parts[2]);
        currentStudent.addDeliveryLocation(currentDeliveryLocation);
    }

    @Given("the student has bank info {string}, CVV {int}, expiring {string}")
    public void the_student_has_bank_info_cvv_expiring(String cardNumber, Integer cvv, String expiryDate) {
        String[] parts = expiryDate.split("/");
        int month = Integer.parseInt(parts[0]);
        int year = Integer.parseInt(parts[1]);
        currentStudent = new StudentAccount.Builder(currentStudent.getName(), currentStudent.getSurname())
                .studentId(currentStudent.getStudentID())
                .email(currentStudent.getEmail())
                .balance(currentStudent.getBalance())
                .deliveryLocations(currentStudent.getDeliveryLocations())
                .bankInfo(cardNumber, cvv, month, 2000 + year) // Assuming year is YY format
                .build();
    }

    @Given("the student has an internal balance of {double} euros")
    public void the_student_has_an_internal_balance_of_euros(Double balance) {
        initialBalance = balance;
        currentStudent = new StudentAccount.Builder(currentStudent.getName(), currentStudent.getSurname())
                .studentId(currentStudent.getStudentID())
                .email(currentStudent.getEmail())
                .bankInfo(
                        currentStudent.getBankInfo().getCardNumber(),
                        currentStudent.getBankInfo().getCVV(),
                        currentStudent.getBankInfo().getExpirationDate().getMonthValue(),
                        currentStudent.getBankInfo().getExpirationDate().getYear()
                )
                .deliveryLocations(currentStudent.getDeliveryLocations())
                .balance(balance)
                .build();
    }

    @Given("a restaurant named {string} exists")
    public void a_restaurant_named_exists(String restaurantName) {
        currentRestaurant = new Restaurant(restaurantName);
    }

    @Given("{string} offers the following dishes:")
    public void offers_the_following_dishes(String restaurantName, DataTable dataTable) {
        assertEquals(restaurantName, currentRestaurant.getRestaurantName());
        List<Map<String, String>> rows = dataTable.asMaps(String.class, String.class);
        for (Map<String, String> row : rows) {
            String name = row.get("name");
            String description = row.get("description");
            double price = Double.parseDouble(row.get("price"));
            currentRestaurant.addDish(name,description,price);
            availableDishes.put(name, currentRestaurant.findDishByName(name));
        }
    }

    @Given("{string} has an available time slot {string} with capacity {int}")
    public void has_an_available_time_slot_with_capacity(String restaurantName, String timeRange, Integer capacity) {
        String[] times = timeRange.split("-");
        currentTimeSlot = new TimeSlot(LocalTime.parse(times[0]), LocalTime.parse(times[1]));
        currentRestaurant.setCapacity(currentTimeSlot, capacity);
        assertTrue(currentRestaurant.getAvailableTimeSlots().contains(currentTimeSlot), "Time slot should be initially available");
    }

    @Given("{word} has selected the following items from {string}:")
    public void user_has_selected_following_items(String userName, String restaurantName, DataTable dataTable) {

        List<Map<String, String>> rows = dataTable.asMaps(String.class, String.class);
        for (Map<String, String> row : rows) {
            String itemName = row.get("item");
            int quantity = Integer.parseInt(row.get("quantity"));
            Dish dish = availableDishes.get(itemName);
            assertNotNull(dish, "Dish " + itemName + " should exist in the available dishes map for restaurant " + restaurantName);
            for (int i = 0; i < quantity; i++) {
                selectedDishes.add(dish);
            }
        }
    }

    @When("{word} creates an order for {string} with delivery to {string}")
    public void user_creates_an_order(String userName, String restaurantName, String locationName){
        orderManager.createOrder(selectedDishes, currentStudent, currentDeliveryLocation, currentRestaurant);

        List<Order> pendingOrders =orderManager.getPendingOrders();
        Optional<Order> foundOrder = pendingOrders.stream()
                .filter(o -> o.getStudentAccount().equals(currentStudent))
                .findFirst();
        currentOrder = foundOrder.get();
    }

    @When("{word} chooses a time slot from those that are available")
    public void user_chooses_available_time_slot(String userName) {
        List<TimeSlot> availableSlots = currentRestaurant.getAvailableTimeSlots();

        if (currentTimeSlot == null) {
            currentTimeSlot = availableSlots.get(0);
        }

        timeSlotCapacityBeforeSelection = currentRestaurant.getCapacity(currentTimeSlot);

        currentRestaurant.blockTimeSlot(currentTimeSlot);

        timeSlotCapacityAfterSelection = currentRestaurant.getCapacity(currentTimeSlot);
    }

    @When("the external payment system approves the payment on the first attempt")
    public void external_payment_approves_first_attempt() {
        when(mockPaymentService.processExternalPayment(any(Order.class))).thenReturn(true);
    }

    @When("the external payment system rejects the payment on all attempts")
    public void external_payment_rejects_on_all_attempts() {
        when(mockPaymentService.processExternalPayment(any(Order.class))).thenReturn(false);
    }

    @When("{word} initiates the payment for the order using {word} method")
    public void user_initiates_payment(String userName, String paymentMethodStr) {
        PaymentMethod paymentMethod = PaymentMethod.valueOf(paymentMethodStr.toUpperCase());
        orderManager.initiatePayment(currentOrder, paymentMethod);
        if(currentOrder.getOrderStatus().equals(OrderStatus.CANCELED)){
            currentRestaurant.unblockTimeSlot(currentTimeSlot);
            timeSlotCapacityAfterSelection = timeSlotCapacityBeforeSelection;
        }
    }

    @Then("a validated order should exist for {word} with total amount {double}")
    public void a_validated_order_should_exist(String userName, Double expectedAmount) {
        assertNotNull(currentOrder, "A pending order for the student should exist");
        assertEquals(OrderStatus.VALIDATED, currentOrder.getOrderStatus(), "Order status should initially be PENDING");
        assertEquals(expectedAmount, currentOrder.getAmount(), 0.01, "Order amount should match calculated total");
        assertEquals(currentRestaurant, currentOrder.getRestaurant(), "Order restaurant should match");
        assertEquals(currentDeliveryLocation, currentOrder.getDeliveryLocation(), "Order delivery location should match");
        assertEquals(selectedDishes.size(), currentOrder.getDishes().size(), "Number of dishes in order should match selection");
    }

    @Then("the order status should become {word}")
    public void order_status_should_become(String expectedStatusStr) {
        assertNotNull(currentOrder, "Current order should not be null");
        OrderStatus expectedStatus = OrderStatus.valueOf(expectedStatusStr.toUpperCase());
        assertEquals(expectedStatus, currentOrder.getOrderStatus());
    }

    @Then("the order should be registered successfully with {string}")
    public void order_should_be_registered(String restaurantName){
        assertNotNull(currentOrder, "Order context must be set");
        assertEquals(OrderStatus.VALIDATED, currentOrder.getOrderStatus(), "Order must be VALIDATED to be registered");
        boolean registered = orderManager.registerOrder(currentOrder, currentRestaurant);
        assertTrue(registered, "Order registration should return true for a validated order");

        List<Order> pendingOrders = orderManager.getPendingOrders();
        assertFalse(pendingOrders.contains(currentOrder), "Order should be removed from pending list after registration");

        List<Order> registeredOrders = orderManager.getRegisteredOrders();
        assertTrue(registeredOrders.contains(currentOrder), "Order should be added to registered list after registration");
    }

    @Then("the time slot should remain blocked")
    public void the_time_slot_should_remain_blocked() {
        assertNotNull(currentRestaurant, "Restaurant context must be set");
        assertNotNull(currentTimeSlot, "A time slot must have been selected");
        assertTrue(timeSlotCapacityBeforeSelection >= 0, "Time slot capacity before selection should be recorded");
        assertTrue(timeSlotCapacityAfterSelection >= 0, "Time slot capacity after selection should be recorded");

        int currentCapacity = currentRestaurant.getCapacity(currentTimeSlot);
        assertEquals(timeSlotCapacityAfterSelection, currentCapacity, "Time slot capacity should remain unchanged after being blocked");
        assertEquals(timeSlotCapacityBeforeSelection - 1, currentCapacity, "Time slot should still reflect the blocked state");
    }

    @Then("the time slot should not remain blocked")
    public void the_time_slot_should_not_remain_blocked() {
        assertNotNull(currentRestaurant, "Restaurant context must be set");
        assertNotNull(currentTimeSlot, "A time slot must have been selected");
        assertTrue(timeSlotCapacityBeforeSelection >= 0, "Time slot capacity before selection should be recorded");
        assertTrue(timeSlotCapacityAfterSelection >= 0, "Time slot capacity after selection should be recorded");

        int currentCapacity = currentRestaurant.getCapacity(currentTimeSlot);
        assertEquals(timeSlotCapacityBeforeSelection, currentCapacity, "Time slot capacity should remain unchanged after being blocked");
    }

    @Then("the payment should be debited from {word}'s balance")
    public void payment_should_be_debited(String userName) {
        assertNotNull(currentOrder, "Order context must be set");
        assertEquals(OrderStatus.VALIDATED, currentOrder.getOrderStatus(), "Order must be validated if debit was successful");
        assertTrue(currentStudent.getBalance() < initialBalance, "Balance should have decreased");
    }

    @Then("{word}'s balance should be {double} euros")
    public void user_balance_should_be(String userName, Double expectedBalance) {
        assertEquals(expectedBalance, currentStudent.getBalance(), 0.01, "Student balance should be the expected value");
    }

    @Then("the payment attempt should fail due to insufficient balance")
    public void payment_attempt_should_fail_insufficient_balance() {
        assertNotNull(currentOrder, "Order context must be set");
        assertEquals(OrderStatus.CANCELED, currentOrder.getOrderStatus(), "Order status should be CANCELED after failed internal payment");
    }

    @Then("the payment attempt should fail due to external decline")
    public void payment_attempt_should_fail_due_to_external_decline() {
        assertNotNull(currentOrder, "Order context must be set");
        assertEquals(OrderStatus.CANCELED, currentOrder.getOrderStatus(), "Order status should be CANCELED after failed external payment");
    }

    @Then("{word}'s balance should remain {double} euros")
    public void user_balance_should_remain(String userName, Double expectedBalance) {
        assertEquals(expectedBalance, currentStudent.getBalance(), 0.01, "Balance should not have changed from initial");
    }

    @Then("the order should not be registered with {string}")
    public void order_should_not_be_registered(String restaurantName){
        assertNotNull(currentOrder, "Order context must be set");
        assertNotEquals(OrderStatus.VALIDATED, currentOrder.getOrderStatus(), "Order status should not be VALIDATED");

        boolean registered = orderManager.registerOrder(currentOrder, currentRestaurant);
        assertFalse(registered, "Order registration should return false for non-validated orders");

        List<Order> registeredOrders = orderManager.getRegisteredOrders();
        assertFalse(registeredOrders.contains(currentOrder), "Order should not be in the registered list");

        List<Order> pendingOrders = orderManager.getPendingOrders();
        assertFalse(pendingOrders.contains(currentOrder), "Canceled order should be removed from pending list");
    }

}
