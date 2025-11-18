package fr.unice.polytech.users; // Assuming this package

import fr.unice.polytech.paymentProcessing.BankInfo;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;

// Note: Requires UserAccount class from above.
public class StudentAccount extends UserAccount {

    private String studentID;
    private double balance = 30 ;
    private BankInfo bankInfo;
    private List<DeliveryLocation> prerecordedLocations = new ArrayList<>();

    /**
     * Constructor for StudentAccount.
     */
    private StudentAccount(Builder builder) {
        super(builder.name, builder.surname, builder.email); // Initialize attributes from UserAccount
        this.studentID = builder.studentID;
        this.bankInfo = builder.bankInfo;
        this.balance = builder.balance;
        this.prerecordedLocations = new ArrayList<>(builder.prerecordedLocations);
    }

    public List<DeliveryLocation> getDeliveryLocations() {
        return Collections.unmodifiableList(prerecordedLocations);
    }

    public boolean hasDeliveryLocation(DeliveryLocation deliveryLocation) {
        return deliveryLocation != null && prerecordedLocations.contains(deliveryLocation);
    }

    public void addDeliveryLocation(DeliveryLocation deliveryLocation) {
        if (deliveryLocation != null && !prerecordedLocations.contains(deliveryLocation)) {
            prerecordedLocations.add(deliveryLocation);
        }
    }
    
    public String getStudentID() {
        return studentID;
    }

    public double getBalance() {
        return balance;
    }

    public BankInfo getBankInfo() {
        return bankInfo;
    }

    public boolean debit(double amount) {
        if (amount <= balance) {
            balance -= amount;
            return true;
        }
        return false;
    }



    public static class Builder {
        private String name;
        private String surname;
        private String email;
        private String studentID;
        private BankInfo bankInfo;
        private double balance = 30;
        private List<DeliveryLocation> prerecordedLocations = new ArrayList<>();

        public Builder(String name, String surname){
            this.name = name;
            this.surname = surname;
        }

        public Builder email(String email){
            this.email = email;
            return this;
        }

        public Builder studentId(String studentID){
            this.studentID = studentID;
            return this;
        }

        public Builder bankInfo(String cardNumber, int CVV, int month, int year){
            this.bankInfo = new BankInfo(cardNumber, CVV, month, year);
            return this;
        }

        public Builder balance(double balance){
            this.balance = balance;
            return this;
        }

        public Builder deliveryLocations(List<DeliveryLocation> deliveryLocations) {
            if (deliveryLocations == null) {
                this.prerecordedLocations = new ArrayList<>();
            } else {
                this.prerecordedLocations = new ArrayList<>(deliveryLocations);
            }
            return this;
        }

        public Builder addDeliveryLocation(DeliveryLocation deliveryLocation) {
            if (deliveryLocation != null) {
                this.prerecordedLocations.add(deliveryLocation);
            }
            return this;
        }

        public StudentAccount build(){
            return new StudentAccount(this);
        }

    }

    @Override
    public boolean equals(Object o) {
        if (o == null || getClass() != o.getClass()) return false;
        StudentAccount that = (StudentAccount) o;
        return Double.compare(balance, that.balance) == 0 && Objects.equals(studentID, that.studentID) && Objects.equals(bankInfo, that.bankInfo) && Objects.equals(prerecordedLocations, that.prerecordedLocations);
    }

    @Override
    public int hashCode() {
        return Objects.hash(studentID, balance, bankInfo, prerecordedLocations);
    }
}