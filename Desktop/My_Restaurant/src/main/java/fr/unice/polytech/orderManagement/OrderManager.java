package fr.unice.polytech.orderManagement;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.paymentProcessing.*;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.users.DeliveryLocation;
import fr.unice.polytech.users.StudentAccount;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class OrderManager {


    private List<Order> registeredOrders;
    private List<Order> pendingOrders;
    private final PaymentProcessorFactory paymentProcessorFactory;

    public OrderManager(){
        this(new PaymentProcessorFactory());

    }
    public OrderManager(PaymentProcessorFactory paymentProcessorFactory) {
        this.paymentProcessorFactory = paymentProcessorFactory;
        registeredOrders = new java.util.ArrayList<>();
        pendingOrders = new java.util.ArrayList<>();
    }

    public void createOrder(List<Dish> dishes, StudentAccount studentAccount, DeliveryLocation deliveryLocation, Restaurant restaurant) {
        if (dishes == null || dishes.isEmpty()) {
            throw new IllegalArgumentException("Empty cart");
        }
        if (deliveryLocation == null) {
            throw new IllegalArgumentException("Missing delivery address");
        }
        if (!studentAccount.hasDeliveryLocation(deliveryLocation)) {
            throw new IllegalArgumentException("Order creation failed: Delivery location is not among the student's saved locations.");
        }
        Order order = new Order.Builder(studentAccount)
                .dishes(dishes)
                .amount(calculateTotalAmount(dishes))
                .deliveryLocation(deliveryLocation)
                .restaurant(restaurant)
                .orderStatus(OrderStatus.PENDING)
                .build();

        pendingOrders.add(order);

    }


    public void initiatePayment(Order order, PaymentMethod paymentMethod) {
        if (paymentMethod == null) {
            throw new IllegalArgumentException("Payment method must be provided");
        }
        /*
        * */
        //Creattion du processeur de paiement via la factory
        IPaymentProcessor processor = paymentProcessorFactory.createProcessor(order, paymentMethod);

        order.setPaymentMethod(paymentMethod);
        OrderStatus status = processor.processPayment(order);
        order.setOrderStatus(status);

    }

    private void dropOrder(Order order) {
        pendingOrders.remove(order);
    }



    public boolean registerOrder(Order order, Restaurant restaurant) {
        if (order.getOrderStatus() == OrderStatus.VALIDATED) {
            registeredOrders.add(order);
            pendingOrders.remove(order);
            restaurant.addOrder(order);
            return true;
        } else if (order.getOrderStatus() == OrderStatus.CANCELED) {
            dropOrder(order);
        }
        return false;
    }


    private double calculateTotalAmount(List<Dish> dishes) {
        return dishes.stream().mapToDouble(Dish::getPrice).sum();
    }

    public List<Order> getRegisteredOrders() {
        return registeredOrders;
    }
    public List<Order> getPendingOrders() {
        return pendingOrders;
    }




}
