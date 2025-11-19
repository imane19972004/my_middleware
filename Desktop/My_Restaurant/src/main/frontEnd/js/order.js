// ============================================
// order.js - Gestion de la page de commande
// ============================================

let selectedRestaurant = null;
let cart = [];
let currentDish = null;

// ========== CHARGEMENT INITIAL ==========
document.addEventListener('DOMContentLoaded', async () => {
    const restaurantId = sessionStorage.getItem('selectedRestaurantId');
    
    if (!restaurantId) {
        document.getElementById('menu').innerHTML = 
            '<p>⚠️ Aucun restaurant sélectionné. <a href="restaurants.html">Retour</a></p>';
        return;
    }
    
    await loadRestaurant(restaurantId);
    await loadDeliverySlots(restaurantId);
    setupEventListeners();
});

// ========== CHARGEMENT DU RESTAURANT ==========
async function loadRestaurant(restaurantId) {
    try {
        selectedRestaurant = await API.getRestaurantById(restaurantId);
        displayMenu(selectedRestaurant);
    } catch (error) {
        console.error('Erreur chargement restaurant:', error);
        document.getElementById('menu').innerHTML = 
            '<p>❌ Impossible de charger le menu.</p>';
    }
}

// ========== AFFICHAGE DU MENU ==========
function displayMenu(restaurant) {
    const menuContainer = document.getElementById('menu');
    
    if (!restaurant.dishes || restaurant.dishes.length === 0) {
        menuContainer.innerHTML = '<p>Aucun plat disponible.</p>';
        return;
    }
    
    menuContainer.innerHTML = `
        <h2>${restaurant.name}</h2>
        <div id="dishes-list">
            ${restaurant.dishes.map(dish => `
                <div class="dish">
                    <h3>${dish.name}</h3>
                    <p>${dish.description || ''}</p>
                    <p><strong>${dish.price}€</strong></p>
                    ${dish.toppings && dish.toppings.length > 0 
                        ? `<button onclick="selectDishWithToppings(${JSON.stringify(dish).replace(/"/g, '&quot;')})" class="btn">
                            Ajouter avec options
                           </button>`
                        : `<button onclick="addToCart(${JSON.stringify(dish).replace(/"/g, '&quot;')})" class="btn">
                            Ajouter
                           </button>`
                    }
                </div>
            `).join('')}
        </div>
    `;
}

// ========== GESTION DES TOPPINGS ==========
function selectDishWithToppings(dish) {
    currentDish = dish;
    const toppingsSection = document.getElementById('toppings-section');
    const toppingsList = document.getElementById('toppings-list');
    
    toppingsList.innerHTML = dish.toppings.map((topping, index) => `
        <label>
            <input type="checkbox" class="topping-checkbox" data-topping='${JSON.stringify(topping)}'>
            ${topping.name} (+${topping.price}€)
        </label>
    `).join('');
    
    toppingsSection.classList.remove('hidden');
}

// ========== AJOUT AU PANIER ==========
function addToCart(dish, selectedToppings = []) {
    cart.push({
        ...dish,
        selectedToppings: selectedToppings
    });
    
    updateCartDisplay();
    
    // Masquer la section toppings
    document.getElementById('toppings-section').classList.add('hidden');
}

// ========== MISE À JOUR DU PANIER ==========
function updateCartDisplay() {
    const cartList = document.getElementById('cart-list');
    const totalElement = document.getElementById('total');
    
    if (cart.length === 0) {
        cartList.innerHTML = '<li>Panier vide</li>';
        totalElement.textContent = '0';
        return;
    }
    
    let total = 0;
    cartList.innerHTML = cart.map((item, index) => {
        const toppingsPrice = item.selectedToppings 
            ? item.selectedToppings.reduce((sum, t) => sum + t.price, 0) 
            : 0;
        const itemTotal = item.price + toppingsPrice;
        total += itemTotal;
        
        return `
            <li>
                ${item.name} - ${itemTotal.toFixed(2)}€
                ${item.selectedToppings && item.selectedToppings.length > 0 
                    ? `<br><small>+ ${item.selectedToppings.map(t => t.name).join(', ')}</small>` 
                    : ''}
                <button onclick="removeFromCart(${index})">❌</button>
            </li>
        `;
    }).join('');
    
    totalElement.textContent = total.toFixed(2);
}

function removeFromCart(index) {
    cart.splice(index, 1);
    updateCartDisplay();
}

// ========== CHARGEMENT DES CRÉNEAUX ==========
async function loadDeliverySlots(restaurantId) {
    try {
        const slots = await API.getTimeSlots(restaurantId);
        displayDeliverySlots(slots);
    } catch (error) {
        console.error('Erreur chargement créneaux:', error);
    }
}

function displayDeliverySlots(slots) {
    const select = document.getElementById('delivery-slot');
    
    if (!slots || slots.length === 0) {
        select.innerHTML = '<option>Aucun créneau disponible</option>';
        return;
    }
    
    select.innerHTML = slots.map(slot => `
        <option value="${slot.startTime}-${slot.endTime}">
            ${slot.startTime} - ${slot.endTime} (${slot.availableCapacity} places)
        </option>
    `).join('');
}

// ========== CONFIGURATION DES ÉVÉNEMENTS ==========
function setupEventListeners() {
    // Ajouter avec toppings
    document.getElementById('add-with-toppings')?.addEventListener('click', () => {
        const checkboxes = document.querySelectorAll('.topping-checkbox:checked');
        const selectedToppings = Array.from(checkboxes).map(cb => JSON.parse(cb.dataset.topping));
        addToCart(currentDish, selectedToppings);
    });
    
    // Confirmer la commande
    document.getElementById('btn-confirm')?.addEventListener('click', confirmOrder);
}

// ========== CONFIRMATION DE COMMANDE ==========
async function confirmOrder() {
    if (cart.length === 0) {
        alert('Votre panier est vide !');
        return;
    }
    
    const orderData = {
        restaurantId: selectedRestaurant.id,
        dishes: cart.map(item => ({
            name: item.name,
            price: item.price
        })),
        deliveryLocation: "Campus Sophia" // À adapter
    };
    
    try {
        const order = await API.createOrder(orderData);
        alert(`✅ Commande créée ! ID: ${order.id}`);
        window.location.href = 'confirmation.html';
    } catch (error) {
        console.error('Erreur création commande:', error);
        alert('❌ Erreur lors de la création de la commande');
    }
}