package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;

public class PaymentProcessorFactory {

    private final IPaymentService externalPaymentService;

    public PaymentProcessorFactory() {
        this(new PaymentService());
    }

    public PaymentProcessorFactory(IPaymentService externalPaymentService) {
        this.externalPaymentService = externalPaymentService;
    }

    public IPaymentProcessor createProcessor(Order order, PaymentMethod paymentMethod) {
        if (paymentMethod == null) {
            throw new IllegalArgumentException("Unsupported payment method: null");
        }
        switch (paymentMethod) {
            case EXTERNAL:
                return new PaymentProcessor(order, externalPaymentService);
            case INTERNAL:
                return new InternalPaymentProcessor(order);
            default:
                throw new IllegalArgumentException("Unsupported payment method: " + paymentMethod);
        }
    }
}