
-- ==============================================
-- SophiaTech Eats - SQLite Database Schema
-- ==============================================

-- ==========================
-- UTILISATEURS & ÉTUDIANTS
-- ==========================
CREATE TABLE UserAccount (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    surname TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL
);

CREATE TABLE StudentAccount (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    studentID TEXT NOT NULL,
    balance REAL DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES UserAccount(id) ON DELETE CASCADE
);

CREATE TABLE BankInfo (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    student_id INTEGER NOT NULL,
    cardNumber TEXT NOT NULL,
    cvv INTEGER NOT NULL,
    expirationDate TEXT NOT NULL,
    FOREIGN KEY (student_id) REFERENCES StudentAccount(id) ON DELETE CASCADE
);

CREATE TABLE DeliveryLocation (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    student_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    address TEXT NOT NULL,
    city TEXT NOT NULL,
    zipCode TEXT NOT NULL,
    FOREIGN KEY (student_id) REFERENCES StudentAccount(id) ON DELETE CASCADE
);

-- ==========================
-- RESTAURANTS & PLATS
-- ==========================
CREATE TABLE Restaurant (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    restaurantName TEXT NOT NULL,
    cuisineType TEXT,
    capacityPerSlot INTEGER,
    priceRange TEXT
);

CREATE TABLE TimeSlot (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    restaurant_id INTEGER NOT NULL,
    dayOfWeek TEXT NOT NULL,
    startTime TEXT NOT NULL,
    endTime TEXT NOT NULL,
    FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);

CREATE TABLE Dish (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    restaurant_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    price REAL NOT NULL,
    FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id) ON DELETE CASCADE
);

CREATE TABLE Topping (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    dish_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    price REAL DEFAULT 0,
    FOREIGN KEY (dish_id) REFERENCES Dish(id) ON DELETE CASCADE
);

-- ==========================
-- COMMANDES
-- ==========================
CREATE TABLE "Order" (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    student_id INTEGER NOT NULL,
    restaurant_id INTEGER NOT NULL,
    deliveryLocation_id INTEGER,
    status TEXT CHECK(status IN ('CREATED','VALIDATED','CANCELLED','PAID')) DEFAULT 'CREATED',
    totalAmount REAL DEFAULT 0,
    dateTime TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES StudentAccount(id),
    FOREIGN KEY (restaurant_id) REFERENCES Restaurant(id),
    FOREIGN KEY (deliveryLocation_id) REFERENCES DeliveryLocation(id)
);

CREATE TABLE OrderDish (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    dish_id INTEGER NOT NULL,
    quantity INTEGER DEFAULT 1,
    price REAL NOT NULL,
    FOREIGN KEY (order_id) REFERENCES "Order"(id) ON DELETE CASCADE,
    FOREIGN KEY (dish_id) REFERENCES Dish(id)
);

CREATE TABLE OrderDishTopping (
    orderDish_id INTEGER NOT NULL,
    topping_id INTEGER NOT NULL,
    PRIMARY KEY (orderDish_id, topping_id),
    FOREIGN KEY (orderDish_id) REFERENCES OrderDish(id) ON DELETE CASCADE,
    FOREIGN KEY (topping_id) REFERENCES Topping(id)
);

-- ==========================
-- PAIEMENT
-- ==========================
CREATE TABLE Payment (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    method TEXT CHECK(method IN ('INTERNAL','EXTERNAL')) NOT NULL,
    status TEXT CHECK(status IN ('PENDING','SUCCESS','FAILED')) DEFAULT 'PENDING',
    FOREIGN KEY (order_id) REFERENCES "Order"(id) ON DELETE CASCADE
);

-- ==========================
-- DONNÉES D'EXEMPLE
-- ==========================
INSERT INTO UserAccount (name, surname, email) VALUES
('Alice', 'Dupont', 'alice@etu.univ.fr'),
('Bob', 'Martin', 'bob@etu.univ.fr');

INSERT INTO StudentAccount (user_id, studentID, balance) VALUES
(1, 'STD2025001', 25.00),
(2, 'STD2025002', 18.50);

INSERT INTO Restaurant (restaurantName, cuisineType, capacityPerSlot, priceRange) VALUES
('Crous Méditerranée', 'French', 10, '€'),
('Pizza Campus', 'Italian', 8, '€€');

INSERT INTO Dish (restaurant_id, name, description, price) VALUES
(1, 'Gratin Dauphinois', 'Creamy potato gratin', 6.50),
(1, 'Chicken Provençal', 'Herbs and tomato sauce', 8.00),
(2, 'Pizza Margherita', 'Classic with mozzarella and tomato', 7.00),
(2, 'Pizza 4 Fromages', 'Mozzarella, gorgonzola, parmesan, emmental', 8.50);

INSERT INTO Topping (dish_id, name, price) VALUES
(3, 'Extra Cheese', 1.00),
(3, 'Olives', 0.50),
(4, 'Mushrooms', 0.80);

INSERT INTO DeliveryLocation (student_id, name, address, city, zipCode) VALUES
(1, 'Campus Dorm A', '12 Rue des Études', 'Nice', '06000'),
(2, 'Campus Dorm B', '15 Avenue de la Mer', 'Nice', '06000');

INSERT INTO TimeSlot (restaurant_id, dayOfWeek, startTime, endTime) VALUES
(1, 'MONDAY', '11:30', '14:00'),
(1, 'MONDAY', '18:30', '21:00'),
(2, 'MONDAY', '11:00', '14:00'),
(2, 'MONDAY', '18:00', '22:00');
