package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;

public class PaymentProcessor implements IPaymentProcessor{

    private final Order order;
    private final IPaymentService paymentService;

    public PaymentProcessor(Order order) {
        this(order, new PaymentService(order));
    }

    public PaymentProcessor(Order order, IPaymentService paymentService) {
        this.order = order;
        this.paymentService = paymentService;
    }


    public OrderStatus processPayment() {
        return processPayment(order);
    }


    public OrderStatus processPayment(Order order){
        for (int i = 0; i < 3; i++) {
            boolean paymentSuccessful = paymentService.processExternalPayment(order);
            if (paymentSuccessful) {
                return OrderStatus.VALIDATED;
            }
        }
        return OrderStatus.CANCELED;
    }

}
