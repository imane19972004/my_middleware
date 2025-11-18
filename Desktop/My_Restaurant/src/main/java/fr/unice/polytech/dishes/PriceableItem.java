package fr.unice.polytech.dishes;

public class PriceableItem {
    private String name;
    private double price;
    
    
    public PriceableItem(String name) {
      this.name = name;
      price = 4;
    }

    public PriceableItem(String name, double price) {
      this.name = name;
      this.price = price;
    }

    public String getName() {
      return name;
    }

    public void setName(String name) {
      this.name = name;
    }
    public double getPrice() {
      return price;
    }
    public void setPrice(double price) {
      this.price = price;
    }

    @Override
    public String toString() {
      return "PriceableItem [name=" + name + ", price=" + price + "]";
    }
  
}