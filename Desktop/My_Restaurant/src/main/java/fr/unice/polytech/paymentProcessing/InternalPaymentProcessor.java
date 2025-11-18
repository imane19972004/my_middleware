package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.users.StudentAccount;

public class InternalPaymentProcessor implements IPaymentProcessor {
    private final Order order;

    public InternalPaymentProcessor(Order order) {
        this.order = order;
    }

    @Override
    public OrderStatus processPayment(Order order) {
        StudentAccount client = order.getStudentAccount();
        double orderTotal = order.getAmount();
        boolean ok = client.debit(orderTotal);
        OrderStatus status = ok ? OrderStatus.VALIDATED : OrderStatus.CANCELED;
        order.setOrderStatus(status);
        return status;
    }


    public OrderStatus updatePaymentStatus(Order order) {
        OrderStatus status = processPayment(order);
        if (status == OrderStatus.VALIDATED) {
            return  status;
        }
        int i = 0;
        while (i<2 && status == OrderStatus.CANCELED) {
            status = processPayment(order);
            i++;
        }
        return status;
    }


}
