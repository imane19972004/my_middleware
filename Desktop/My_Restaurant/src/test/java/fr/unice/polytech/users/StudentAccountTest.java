package fr.unice.polytech.users;

//import io.cucumber.java.an.E;
import org.junit.jupiter.api.Test;

import fr.unice.polytech.paymentProcessing.BankInfo;

import static org.junit.jupiter.api.Assertions.*;

class StudentAccountTest {

    private final String NAME = "Alice";
    private final String SURNAME = "Smith";
    private final String EMAIL = "alice.smith@etu.unice.fr";
    private final String ID = "22400632";

    // Test 1: Verify that Builder Creational Pattern has been well implented
    @Test
    void testStudentAccountCreation() {
        StudentAccount student = new StudentAccount.Builder(NAME, SURNAME)
                .email(EMAIL)
                .studentId(ID)
                .bankInfo("3151 2136 8946 4151", 401, 5,28)
                .build();

        BankInfo bankInfo = new BankInfo("3151 2136 8946 4151", 401, 5,28);

        assertEquals(NAME, student.getName(), "Name should be inherited correctly.");
        assertEquals(SURNAME, student.getSurname(), "Surname should be inherited correctly.");
        assertEquals(EMAIL, student.getEmail(), "Email should be inherited correctly.");
        assertEquals(bankInfo, student.getBankInfo());
        assertEquals(ID, student.getStudentID());
    }

}