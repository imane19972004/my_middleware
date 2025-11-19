// ============================================
// restaurants.js - Gestion de la page restaurants
// ============================================

let allRestaurants = [];

// ========== CHARGEMENT INITIAL ==========
document.addEventListener('DOMContentLoaded', () => {
    loadRestaurants();
    setupFilters();
});

// ========== PLACEHOLDERS DE CHARGEMENT ==========
function showLoadingPlaceholders() {
    const container = document.getElementById('restaurants-list');
    container.innerHTML = `
        <div class="skeleton skeleton-card"></div>
        <div class="skeleton skeleton-card"></div>
        <div class="skeleton skeleton-card"></div>
    `;
}

function showError(message) {
    const container = document.getElementById('restaurants-list');
    container.innerHTML = `
        <div style="text-align: center; padding: 2rem; color: var(--error);">
            <p>❌ ${message}</p>
            <button onclick="loadRestaurants()" class="btn">Réessayer</button>
        </div>
    `;
}

// ========== CHARGEMENT DES RESTAURANTS ==========
async function loadRestaurants(filters = {}) {
    showLoadingPlaceholders();
    
    try {
        const restaurants = await API.getRestaurants(filters);
        allRestaurants = restaurants;
        displayRestaurants(restaurants);
    } catch (error) {
        console.error('Erreur chargement restaurants:', error);
        showError('Impossible de charger les restaurants. Vérifiez que le serveur est démarré.');
    }
}

// ========== AFFICHAGE DES RESTAURANTS ==========
function displayRestaurants(restaurants) {
    const container = document.getElementById('restaurants-list');
    
    if (!restaurants || restaurants.length === 0) {
        container.innerHTML = '<p>Aucun restaurant trouvé.</p>';
        return;
    }
    
    container.innerHTML = restaurants.map(restaurant => `
        <div class="restaurant">
            <h3>${restaurant.name}</h3>
            <p><strong>Cuisine:</strong> ${restaurant.cuisineType || 'N/A'}</p>
            <p><strong>Plats:</strong> ${restaurant.dishes ? restaurant.dishes.length : 0}</p>
            <button onclick="viewRestaurant(${restaurant.id})" class="btn">
                Voir le menu
            </button>
        </div>
    `).join('');
}

// ========== CONFIGURATION DES FILTRES ==========
function setupFilters() {
    const filterElements = [
        'availability',
        'diet',
        'price',
        'cuisine',
        'type'
    ];
    
    // Appliquer debounce sur tous les filtres
    filterElements.forEach(filterId => {
        const element = document.getElementById(filterId);
        if (element) {
            element.addEventListener('change', debounce(applyFilters, 300));
        }
    });
}

// ========== APPLICATION DES FILTRES ==========
function applyFilters() {
    const filters = {
        availability: document.getElementById('availability')?.value,
        diet: document.getElementById('diet')?.value,
        price: document.getElementById('price')?.value,
        cuisineType: document.getElementById('cuisine')?.value,
        type: document.getElementById('type')?.value
    };
    
    // Supprimer les valeurs vides
    Object.keys(filters).forEach(key => {
        if (!filters[key]) delete filters[key];
    });
    
    loadRestaurants(filters);
}

// ========== VOIR UN RESTAURANT ==========
function viewRestaurant(restaurantId) {
    // Sauvegarder l'ID du restaurant sélectionné
    sessionStorage.setItem('selectedRestaurantId', restaurantId);
    window.location.href = 'order.html';
}