package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;

public interface IPaymentProcessor {
    public OrderStatus processPayment(Order order);
}
