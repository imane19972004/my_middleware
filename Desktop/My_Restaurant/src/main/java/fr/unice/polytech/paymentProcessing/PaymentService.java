package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;

public class PaymentService implements IPaymentService{
    MockedExternalPaymentSystem externalPaymentSystem;

    public PaymentService(MockedExternalPaymentSystem externalPaymentSystem, Order order){
        this.externalPaymentSystem = externalPaymentSystem != null
                ? externalPaymentSystem
                : new MockedExternalPaymentSystem(null);
    }


    public PaymentService(){
        this.externalPaymentSystem = new MockedExternalPaymentSystem(null);
    }

    public PaymentService(Order order) {
        externalPaymentSystem = new MockedExternalPaymentSystem(order);
    }

    @Override
    public boolean processExternalPayment(Order order) {
        return externalPaymentSystem.mockedCheckingInformation(order);
    }



}
