package fr.unice.polytech.users; // Assuming this package

public abstract class  UserAccount {
    
    private String name;
    private String surname;
    private String email;

    public UserAccount(String name, String surname, String email) {
        this.name = name;
        this.surname = surname;
        this.email = email;
    }
    public UserAccount(){}
    
    public String getName() {
        return name;
    }

    public String getEmail() {
        return email;
    }

    // Note: It's generally unsafe to expose the surname directly via a getter
    // but included here based on the attribute existence in the diagram.
    public String getSurname() {
        return surname;
    }
    
    // Setters (Essential for registration/profile updates)
    public void setName(String name) {
        this.name = name;
    }
    
    public void setEmail(String email) {
        this.email = email;
    }
    
    public void setSurname(String surname) {
        this.surname = surname;
    }
}