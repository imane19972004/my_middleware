package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.users.StudentAccount;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.mockito.Mockito.*;


class PaymentProcessorTest {

    private final String NAME = "Alice";
    private final String SURNAME = "Smith";
    private final String EMAIL = "alice.smith@etu.unice.fr";
    private final String ID = "22400632";

    private Order order;
    private IPaymentService paymentService;
    private PaymentProcessor processor;

    @BeforeEach
    void setUp() {
        StudentAccount student = new StudentAccount.Builder(NAME, SURNAME)
                .email(EMAIL)
                .studentId(ID)
                .bankInfo("3151 2136 8946 4151", 401, 5,28)
                .build();

        order = new Order.Builder(student).build();

        paymentService = mock(IPaymentService.class);
        processor = new PaymentProcessor(order, paymentService);
    }


    @Test
    void processPayment_ShouldValidate_OnFirstAttempt() {
        when(paymentService.processExternalPayment(order)).thenReturn(true);

        OrderStatus status = processor.processPayment(order);

        assertEquals(OrderStatus.VALIDATED, status);
        verify(paymentService, times(1)).processExternalPayment(order);
    }

    @Test
    void processPayment_ShouldValidate_OnSecondAttempt() {
        when(paymentService.processExternalPayment(order))
                .thenReturn(false)
                .thenReturn(true);

        OrderStatus status = processor.processPayment(order);

        assertEquals(OrderStatus.VALIDATED, status);
        verify(paymentService, times(2)).processExternalPayment(order);
    }

    @Test
    void processPayment_ShouldValidate_OnThirdAttempt() {
        when(paymentService.processExternalPayment(order))
                .thenReturn(false)
                .thenReturn(false)
                .thenReturn(true);

        OrderStatus status = processor.processPayment(order);

        assertEquals(OrderStatus.VALIDATED, status);
        verify(paymentService, times(3)).processExternalPayment(order);
    }

    @Test
    void processPayment_ShouldCancel_AfterThreeFailedAttempts() {
        when(paymentService.processExternalPayment(order)).thenReturn(false);

        OrderStatus status = processor.processPayment(order);

        assertEquals(OrderStatus.CANCELED, status);
        verify(paymentService, times(3)).processExternalPayment(order);
    }

    @Test
    void processPayment_ShouldUseTheCorrectOrderInstance() {
        StudentAccount otherStudent = new StudentAccount.Builder("Bob", "Doe")
                .email("other." + EMAIL)
                .studentId(ID + "1")
                .build();
        Order otherOrder = new Order.Builder(otherStudent).build();

        when(paymentService.processExternalPayment(otherOrder)).thenReturn(true);

        OrderStatus status = processor.processPayment(otherOrder);

        assertEquals(OrderStatus.VALIDATED, status);
        verify(paymentService).processExternalPayment(otherOrder);
        verify(paymentService, never()).processExternalPayment(order);
    }
}

