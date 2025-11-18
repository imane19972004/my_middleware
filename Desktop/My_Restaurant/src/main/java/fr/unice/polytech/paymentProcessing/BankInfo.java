package fr.unice.polytech.paymentProcessing;

import java.time.YearMonth;
import java.util.Objects;

public class BankInfo {
    private String cardNumber;
    private int CVV;
    private YearMonth expirationDate;

    public BankInfo(String cardNumber, int CVV, int month, int year) {
        this.cardNumber = cardNumber;
        this.CVV = CVV;
        this.expirationDate = YearMonth.of(year, month);
    }

    public void setCardNumber(String cardNumber) {
        this.cardNumber = cardNumber;
    }

    public void setCVV(int CVV) {
        this.CVV = CVV;
    }

    public void setExpirationDate(YearMonth expirationDate) {
        this.expirationDate = expirationDate;
    }

    public String getCardNumber() {
        return cardNumber;
    }

    public int getCVV() {
        return CVV;
    }

    public YearMonth getExpirationDate() {
        return expirationDate;
    }

    @Override
    public boolean equals(Object o) {
        if (o == null || getClass() != o.getClass()) return false;
        BankInfo bankInfo = (BankInfo) o;
        return CVV == bankInfo.CVV && Objects.equals(cardNumber, bankInfo.cardNumber) && Objects.equals(expirationDate, bankInfo.expirationDate);
    }

    @Override
    public int hashCode() {
        return Objects.hash(cardNumber, CVV, expirationDate);
    }
}
