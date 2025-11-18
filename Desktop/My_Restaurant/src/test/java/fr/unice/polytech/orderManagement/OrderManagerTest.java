// language: java
package fr.unice.polytech.orderManagement;

import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.paymentProcessing.*;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.users.DeliveryLocation;
import fr.unice.polytech.users.StudentAccount;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.lang.reflect.Field;
import java.time.LocalDate;
import java.time.YearMonth;
import java.util.Arrays;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.*;

class OrderManagerTest {

    private OrderManager orderManager;
    private StudentAccount mockStudentAccount;
    private Restaurant mockRestaurant;
    private DeliveryLocation mockDeliveryLocation;
    private List<Dish> mockDishes;
    private Dish mockDish1;
    private Dish mockDish2;
    private BankInfo mockBankInfo;

    @BeforeEach
    void setUp() {
        orderManager = new OrderManager();
        mockStudentAccount = mock(StudentAccount.class);
        mockRestaurant = mock(Restaurant.class);
        mockDeliveryLocation = mock(DeliveryLocation.class);
        mockBankInfo = mock(BankInfo.class);

        mockDish1 = mock(Dish.class);
        mockDish2 = mock(Dish.class);
        when(mockDish1.getPrice()).thenReturn(15.50);
        when(mockDish2.getPrice()).thenReturn(12.00);

        when(mockBankInfo.getExpirationDate()).thenReturn(YearMonth.from(LocalDate.now().plusYears(2)));
        when(mockBankInfo.getCardNumber()).thenReturn("1234567890123456");
        when(mockBankInfo.getCVV()).thenReturn(123);


        when(mockStudentAccount.getBankInfo()).thenReturn(mockBankInfo);
        when(mockStudentAccount.hasDeliveryLocation(mockDeliveryLocation)).thenReturn(true);
        mockDishes = Arrays.asList(mockDish1, mockDish2);
    }

    @Test
    void testCreateOrder() throws Exception {
        orderManager.createOrder(mockDishes, mockStudentAccount, mockDeliveryLocation, mockRestaurant);


        Field pendingOrdersField = OrderManager.class.getDeclaredField("pendingOrders");
        pendingOrdersField.setAccessible(true);
        List<Order> pendingOrders = (List<Order>) pendingOrdersField.get(orderManager);

        assertEquals(1, pendingOrders.size());
        Order createdOrder = pendingOrders.get(0);
        assertEquals(mockStudentAccount, createdOrder.getStudentAccount());
        assertEquals(27.50, createdOrder.getAmount());
        assertEquals(mockDishes, createdOrder.getDishes());
        assertEquals(mockDeliveryLocation, createdOrder.getDeliveryLocation());
        assertEquals(mockRestaurant, createdOrder.getRestaurant());
        assertEquals(OrderStatus.PENDING, createdOrder.getOrderStatus());
    }

    @Test
    void testInitiatePaymentInvokesFactoryAndProcessor() throws Exception {
        PaymentProcessorFactory factory = mock(PaymentProcessorFactory.class);
        IPaymentProcessor processor = mock(IPaymentProcessor.class);
        when(factory.createProcessor(any(Order.class), eq(PaymentMethod.EXTERNAL)))
                .thenReturn(processor);
        when(processor.processPayment(any(Order.class)))
                .thenReturn(OrderStatus.VALIDATED);

        OrderManager manager = new OrderManager(factory);

        manager.createOrder(mockDishes, mockStudentAccount, mockDeliveryLocation, mockRestaurant);

        Field f = OrderManager.class.getDeclaredField("pendingOrders");
        f.setAccessible(true);
        @SuppressWarnings("unchecked")
        List<Order> pending = (List<Order>) f.get(manager);
        assertEquals(1, pending.size());
        Order order = pending.get(0);

        manager.initiatePayment(order, PaymentMethod.EXTERNAL);

        verify(factory).createProcessor(order, PaymentMethod.EXTERNAL);
        verify(processor).processPayment(order);
        assertEquals(OrderStatus.VALIDATED, order.getOrderStatus());
        assertEquals(1, pending.size());
    }

    @Test
    void testRegisterValidatedOrder() throws Exception {
        Order order = new Order.Builder(mockStudentAccount)
                .orderStatus(OrderStatus.VALIDATED)
                .build();

        Field pendingOrdersField = OrderManager.class.getDeclaredField("pendingOrders");
        pendingOrdersField.setAccessible(true);
        List<Order> pendingOrders = (List<Order>) pendingOrdersField.get(orderManager);
        pendingOrders.add(order);

        boolean result = orderManager.registerOrder(order, mockRestaurant);

        assertTrue(result);
        assertEquals(0, pendingOrders.size());

        Field registeredOrdersField = OrderManager.class.getDeclaredField("registeredOrders");
        registeredOrdersField.setAccessible(true);
        List<Order> registeredOrders = (List<Order>) registeredOrdersField.get(orderManager);
        assertEquals(1, registeredOrders.size());
        assertTrue(registeredOrders.contains(order));
    }

    @Test
    void testRegisterNonValidatedOrder() {
        Order order = new Order.Builder(mockStudentAccount)
                .orderStatus(OrderStatus.PENDING)
                .build();

        boolean result = orderManager.registerOrder(order, mockRestaurant);

        assertFalse(result);
    }

    @Test
    void testCalculateTotalAmount() {
        orderManager.createOrder(mockDishes, mockStudentAccount, mockDeliveryLocation, mockRestaurant);

        verify(mockDish1).getPrice();
        verify(mockDish2).getPrice();
    }

    @Test
    void initiatePaymentUsesFactoryAndUpdatesOrderStatus() throws NoSuchFieldException, IllegalAccessException {
        PaymentProcessorFactory factory = mock(PaymentProcessorFactory.class);
        OrderManager managerWithFactory = new OrderManager(factory);
        Order order = new Order.Builder(mockStudentAccount)
                .amount(12.0)
                .build();

        Field pendingOrdersField = OrderManager.class.getDeclaredField("pendingOrders");
        pendingOrdersField.setAccessible(true);
        List<Order> pendingOrders = (List<Order>) pendingOrdersField.get(managerWithFactory);
        pendingOrders.add(order);

        IPaymentProcessor processor = mock(IPaymentProcessor.class);
        when(factory.createProcessor(order, PaymentMethod.EXTERNAL)).thenReturn(processor);
        when(processor.processPayment(order)).thenReturn(OrderStatus.VALIDATED);

        managerWithFactory.initiatePayment(order, PaymentMethod.EXTERNAL);

        verify(factory).createProcessor(order, PaymentMethod.EXTERNAL);
        verify(processor).processPayment(order);
        assertEquals(OrderStatus.VALIDATED, order.getOrderStatus());
    }

    @Test
    void initiatePaymentThrowsWhenPaymentMethodMissing() {
        Order order = new Order.Builder(mockStudentAccount)
                .amount(10.0)
                .build();

        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class,
                () -> orderManager.initiatePayment(order, null));

        assertTrue(exception.getMessage().contains("Payment method must be provided"));
        assertEquals(OrderStatus.PENDING, order.getOrderStatus());
    }

    @Test
    void createOrderFailsForUnknownDeliveryLocation() {
        DeliveryLocation otherLocation = mock(DeliveryLocation.class);
        when(mockStudentAccount.hasDeliveryLocation(otherLocation)).thenReturn(false);

        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class,
                () -> orderManager.createOrder(mockDishes, mockStudentAccount, otherLocation, mockRestaurant));

        assertTrue(exception.getMessage().contains("saved locations"));
    }

}
