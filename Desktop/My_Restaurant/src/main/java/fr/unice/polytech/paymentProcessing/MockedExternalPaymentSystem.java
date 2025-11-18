package fr.unice.polytech.paymentProcessing;

import fr.unice.polytech.orderManagement.Order;
import fr.unice.polytech.users.StudentAccount;
//import org.mockito.internal.matchers.Or;

import java.time.YearMonth;
import java.util.Random;

public class MockedExternalPaymentSystem {

    private Order order;
    private final Random random = new Random();


    public MockedExternalPaymentSystem(Order order) {
        this.order = order;
    }

    public boolean mockedCheckingInformation(Order givenOrder){
        this.order = givenOrder;
        BankInfo bankInfo = ((StudentAccount)order.getStudentAccount()).getBankInfo();

        if (bankInfo.getExpirationDate().isBefore(YearMonth.now())) {
            return false;
        }

        return random.nextDouble() < 0.8;

    }
}
