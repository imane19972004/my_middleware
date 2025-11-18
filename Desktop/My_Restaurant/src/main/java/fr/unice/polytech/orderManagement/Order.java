package fr.unice.polytech.orderManagement;


import fr.unice.polytech.dishes.Dish;
import fr.unice.polytech.paymentProcessing.PaymentMethod;
import fr.unice.polytech.restaurants.Restaurant;
import fr.unice.polytech.users.DeliveryLocation;
import fr.unice.polytech.users.StudentAccount;
import fr.unice.polytech.users.UserAccount;

import java.util.List;
import java.util.Objects;

public class Order {
    private StudentAccount studentAccount;
    private Restaurant restaurant;
    private double amount;
    private OrderStatus orderStatus;
    private List<Dish> dishes;
    private DeliveryLocation deliveryLocation;
    private PaymentMethod paymentMethod;


    private Order(Builder builder) {
        this.studentAccount = builder.studentAccount;
        this.amount = builder.amount;
        this.dishes = builder.dishes;
        this.deliveryLocation = builder.deliveryLocation;
        this.orderStatus = builder.orderStatus != null ? builder.orderStatus : OrderStatus.PENDING;
        this.restaurant = builder.restaurant;
    }

    public Restaurant getRestaurant() {
        return restaurant;
    }

    public StudentAccount getStudentAccount() {
        return studentAccount;
    }

    public double getAmount() {
        return amount;
    }

    public OrderStatus getOrderStatus() {
        return orderStatus;
    }

    public void setOrderStatus(OrderStatus orderStatus) {
        this.orderStatus = orderStatus;
    }

    public List<Dish> getDishes() {
        return dishes;
    }

    public void setDishes(List<Dish> dishes) {
        this.dishes = dishes;
    }

    public DeliveryLocation getDeliveryLocation() {
        return deliveryLocation;
    }

    public void setDeliveryLocation(DeliveryLocation deliveryLocation) {
        this.deliveryLocation = deliveryLocation;
    }

    public PaymentMethod getPaymentMethod(UserAccount user) {
        if ( (user instanceof Restaurant) || !user.equals(this.studentAccount)) {
            throw new IllegalArgumentException("Access denied: User does not own this order.");
        }
        return paymentMethod;
    }

    public void setPaymentMethod(PaymentMethod paymentMethod) {
        this.paymentMethod = paymentMethod;
    }

    public static class Builder {
        private StudentAccount studentAccount;
        private double amount;
        private Restaurant restaurant;
        private List<Dish> dishes;
        private DeliveryLocation deliveryLocation;
        private OrderStatus orderStatus;

        public Builder(StudentAccount studentAccount) {
            this.studentAccount = studentAccount;
        }

        public Builder deliveryLocation(DeliveryLocation deliveryLocation) {
            this.deliveryLocation = deliveryLocation;
            return this;
        }

        public Builder orderStatus(OrderStatus orderStatus) {
            this.orderStatus = orderStatus;
            return this;
        }

        public Builder amount(double amount) {
            this.amount = amount;
            return this;
        }
        public Builder dishes(List<Dish> dishes) {
            this.dishes = dishes;
            return this;
        }
        public Builder restaurant(Restaurant restaurant) {
            this.restaurant = restaurant;
            return this;
        }


        public Order build() {
            return new Order(this);
        }


        @Override
        public boolean equals(Object o) {
            if (o == null || getClass() != o.getClass()) return false;
            Builder builder = (Builder) o;
            return Double.compare(amount, builder.amount) == 0 && Objects.equals(studentAccount, builder.studentAccount) && Objects.equals(restaurant, builder.restaurant) && Objects.equals(dishes, builder.dishes) && Objects.equals(deliveryLocation, builder.deliveryLocation) && orderStatus == builder.orderStatus;
        }

        @Override
        public int hashCode() {
            return Objects.hash(studentAccount, amount, restaurant, dishes, deliveryLocation, orderStatus);
        }
    }
}
