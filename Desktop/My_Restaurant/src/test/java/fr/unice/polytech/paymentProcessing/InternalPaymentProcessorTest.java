package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.orderManagement.OrderStatus;
import fr.unice.polytech.users.StudentAccount;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;
import static org.junit.jupiter.api.Assertions.assertEquals;

public class InternalPaymentProcessorTest {
    InternalPaymentProcessor processor;
        @Test
        void testProcessPayment_Success() {
            StudentAccount client = new StudentAccount.Builder("Alice", "Smith").balance(100.0).build();
            Order order = new Order.Builder(client).amount(50.0).build();
            processor = new InternalPaymentProcessor(order);
            OrderStatus status = processor.processPayment(order);
            assertEquals(100.0, client.getBalance() + order.getAmount());
            System.out.println(order.getOrderStatus());
            assertSame(OrderStatus.VALIDATED,status);
            assertEquals(50.0, client.getBalance());
        }

        @Test
        void testProcessPayment_Failure_InsufficientFunds() {
            StudentAccount client = new StudentAccount.Builder("Bob", "Brown").balance(30.0).build();
            Order order = new Order.Builder(client).amount(50.0).build();
            InternalPaymentProcessor processor = new InternalPaymentProcessor(order);
            assertSame(OrderStatus.CANCELED, processor.processPayment(order) );
            assertEquals(30.0, client.getBalance());
        }

}
