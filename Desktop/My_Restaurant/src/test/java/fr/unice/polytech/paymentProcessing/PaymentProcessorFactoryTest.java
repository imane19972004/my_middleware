package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.users.StudentAccount;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

class PaymentProcessorFactoryTest {
    private StudentAccount student;
    private Order order;

    @BeforeEach
    void setUp() {
        student = new StudentAccount.Builder("Jordan", "Smith")
                .email("jordan.smith@etu.unice.fr")
                .studentId("ID-42")
                .bankInfo("1234 5678 9012 3456", 123, 12, 30)
                .build();
        order = new Order.Builder(student)
                .amount(18.5)
                .build();
    }

    @Test
    void createProcessorForExternalPaymentUsesProvidedPaymentService() {
        IPaymentService paymentService = mock(IPaymentService.class);
        when(paymentService.processExternalPayment(order)).thenReturn(true);
        PaymentProcessorFactory factory = new PaymentProcessorFactory(paymentService);

        IPaymentProcessor processor = factory.createProcessor(order, PaymentMethod.EXTERNAL);

        assertAll(
                () -> assertTrue(processor instanceof PaymentProcessor,
                        "Expected external payments to use PaymentProcessor"),
                () -> assertEquals(OrderStatus.VALIDATED, processor.processPayment(order),
                        "The payment status should reflect the service response")
        );
        verify(paymentService).processExternalPayment(order);
    }

    @Test
    void createProcessorForInternalPaymentReturnsInternalProcessor() {
        PaymentProcessorFactory factory = new PaymentProcessorFactory(mock(IPaymentService.class));

        IPaymentProcessor processor = factory.createProcessor(order, PaymentMethod.INTERNAL);

        assertTrue(processor instanceof InternalPaymentProcessor,
                "Internal payments must use the dedicated processor");
        // Ensure the processor operates on the order by debiting the student's balance.
        double initialBalance = student.getBalance();
        OrderStatus status = processor.processPayment(order);
        assertEquals(OrderStatus.VALIDATED, status);
        assertEquals(initialBalance - order.getAmount(), student.getBalance(), 1e-6,
                "Internal processor should debit the student's balance");
    }

    @Test
    void createProcessorWithUnsupportedMethodThrowsException() {
        PaymentProcessorFactory factory = new PaymentProcessorFactory(mock(IPaymentService.class));

        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class,
                () -> factory.createProcessor(order, null));

        assertTrue(exception.getMessage().contains("Unsupported payment method"));
    }

}