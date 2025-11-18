package fr.unice.polytech.stepDefs.back;

import io.cucumber.datatable.DataTable;
import io.cucumber.java.en.*;
import org.junit.jupiter.api.Assertions;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.orderManagement.*;
import fr.unice.polytech.paymentProcessing.*;
import fr.unice.polytech.restaurants.*;
import fr.unice.polytech.users.*;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.*;

/**
 * Step definitions for create_order.feature (no quantity management)
 * Uses the real OrderManager and payment flow.
 */
public class CreateOrderSteps {

    private StudentAccount.Builder accountBuilder;
    private StudentAccount studentAccount;
    private Restaurant restaurant;
    private DeliveryLocation deliveryLocation;
    private PaymentMethod paymentMethod;

    private List<Dish> cartDishes;
    private OrderManager orderManager;
    private Order currentOrder;
    private Exception lastException;

    // ============ Setup ============

    @Given("a client named {string}")
    public void a_client_named(String name) {
        accountBuilder = new StudentAccount.Builder(name, "Doe");
        restaurant = new Restaurant("Test Restaurant");
        orderManager = new OrderManager(new PaymentProcessorFactory());
        cartDishes = new ArrayList<>();
        paymentMethod = PaymentMethod.EXTERNAL; // default
        deliveryLocation = null;
        currentOrder = null;
        lastException = null;
    }

    @Given("{string} has a student credit balance of {string}")
    public void has_student_credit_balance(String name, String balance) {
        DeliveryLocation loc1 = new DeliveryLocation("Home", "10 Rue de France", "Nice", "06000");
        DeliveryLocation loc2 = new DeliveryLocation("Dorm", "50 Avenue Jean Medecin", "Nice", "06000");
        accountBuilder.balance(Double.parseDouble(balance))
                .bankInfo("4242424242424242", 123, 12, 2030)
                .deliveryLocations(List.of(loc1, loc2));
        studentAccount = accountBuilder.build();
    }

    // ============ Cart ============

    @Given("{string} has the following items in the cart:")
    public void has_the_following_items_in_the_cart(String name, DataTable dataTable) {
        cartDishes.clear();
        List<Map<String, String>> rows = dataTable.asMaps(String.class, String.class);

        for (Map<String, String> row : rows) {
            String itemName = row.get("item");
            double price = Double.parseDouble(row.get("unit price").trim());
            Dish dish = new Dish(itemName, "desc", price);
            cartDishes.add(dish);
        }
        currentOrder = new Order.Builder(studentAccount).build();
    }

    @Given("Alex has the following items in the cart:")
    public void alex_has_the_following_items_in_the_cart(DataTable dataTable) {
        has_the_following_items_in_the_cart("Alex", dataTable);
    }

    // ============ Delivery & Payment ============
    @When("{string} selects the delivery location {string}")
    public void selects_the_delivery_location(String name, String address) {
        try {
            deliveryLocation = studentAccount.getDeliveryLocations().stream()
                    .filter(loc -> address.toLowerCase().contains(loc.getAddress().toLowerCase()))
                    .findFirst()
                    .orElseThrow(() ->
                            new IllegalArgumentException("Selected address not found in prerecorded locations: " + address));
        } catch (Exception e) {
            lastException = e;
            deliveryLocation = null;
        }
    }

    @When("Alex selects the delivery location {string}")
    public void alex_selects_the_delivery_location(String address) {
        selects_the_delivery_location("Alex", address);
    }

    @When("{string} chooses the saved payment method {string}")
    public void chooses_the_saved_payment_method(String name, String method) {
        if (method.equalsIgnoreCase("Visa") || method.equalsIgnoreCase("Card"))
            paymentMethod = PaymentMethod.EXTERNAL;
        else
            paymentMethod = PaymentMethod.INTERNAL;
    }

    @When("Alex chooses the saved payment method {string}")
    public void alex_chooses_the_saved_payment_method(String method) {
        chooses_the_saved_payment_method("Alex", method);
    }

    // ============ Confirm Order ============

    @When("{string} confirms the order")
    public void confirms_the_order(String name) {
        try {
            // Create the order
            orderManager.createOrder(cartDishes, studentAccount, deliveryLocation, restaurant);

            // Retrieve last created order
            List<Order> pendingOrders = orderManager.getPendingOrders();
            currentOrder = pendingOrders.isEmpty() ? null : pendingOrders.get(pendingOrders.size() - 1);

            // Initiate payment (handled by OrderManager)
            if (currentOrder != null && paymentMethod != null) {
                orderManager.initiatePayment(currentOrder, paymentMethod);
            }

            // Register validated order
            if (currentOrder != null && currentOrder.getOrderStatus() == OrderStatus.VALIDATED) {
                orderManager.registerOrder(currentOrder,restaurant);
            }

        } catch (Exception e) {

            lastException = e;
            currentOrder = null;
        }
    }

    // Small alias so the step "When Alex confirms the order" works too
    @When("Alex confirms the order")
    public void alex_confirms_the_order() {
        confirms_the_order("Alex");
    }

    // ============ Assertions ============

    @Then("the order should be created with status {string}")
    public void the_order_should_be_created_with_status(String expectedStatus) {
        Assertions.assertNotNull(currentOrder, "Order should exist");

        OrderStatus expected = switch (expectedStatus.toUpperCase()) {
            case "CONFIRMED" -> OrderStatus.VALIDATED;
            case "REJECTED" -> OrderStatus.CANCELED;
            default -> OrderStatus.valueOf(expectedStatus.toUpperCase());
        };

        Assertions.assertEquals(expected, currentOrder.getOrderStatus(),
                "Expected order status " + expected + " but was " + currentOrder.getOrderStatus());
    }

    @Then("Alex should see the order total of {string}")
    public void alex_should_see_the_order_total_of(String expectedTotal) {
        Assertions.assertNotNull(currentOrder, "Order should exist");
        BigDecimal expected = new BigDecimal(expectedTotal).setScale(2, RoundingMode.HALF_UP);
        BigDecimal actual = BigDecimal.valueOf(currentOrder.getAmount()).setScale(2, RoundingMode.HALF_UP);
        Assertions.assertEquals(expected, actual, "Order total mismatch");
    }

    @Then("Alex should receive an order confirmation notification")
    public void alex_should_receive_an_order_confirmation_notification() {
        Assertions.assertEquals(OrderStatus.VALIDATED, currentOrder.getOrderStatus(),
                "Expected notification only for validated orders");
    }

    @Then("an error should be raised with message containing {string}")
    public void an_error_should_be_raised(String expectedMessage) {
        Assertions.assertNotNull(lastException, "Expected an exception to be thrown");
        Assertions.assertTrue(lastException.getMessage().contains(expectedMessage),
                "Expected message to contain: \"" + expectedMessage + "\" but got: \""
                        + (lastException != null ? lastException.getMessage() : "null") + "\"");
    }



}
