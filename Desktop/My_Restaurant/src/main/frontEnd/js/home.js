// Home page JavaScript - Restaurant loading and interaction

const API_BASE_URL = 'http://localhost:8080';

/**
 * Escape HTML to prevent XSS attacks
 * @param {string} str - String to escape
 * @returns {string} - Escaped HTML string
 */
function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

/**
 * Load and display restaurants from the API
 */
async function loadRestaurants() {
    const container = document.getElementById('restaurants-grid');

    try {
        const response = await fetch(`${API_BASE_URL}/api/restaurants`);

        if (!response.ok) {
            const text = await response.text();
            throw new Error(`Server returned ${response.status}: ${text}`);
        }

        let data = await response.json();

        // Handle different response formats from the API
        let restaurants = data;
        if (!Array.isArray(restaurants)) {
            if (typeof restaurants === 'string') {
                // If response is a JSON string, parse it
                restaurants = JSON.parse(restaurants);
            } else if (restaurants && typeof restaurants === 'object' && Array.isArray(restaurants.restaurants)) {
                // If response is wrapped in an object
                restaurants = restaurants.restaurants;
            } else {
                throw new Error('Unexpected response format');
            }
        }

        if (!Array.isArray(restaurants)) {
            throw new Error('Response is not an array');
        }

        if (restaurants.length === 0) {
            container.innerHTML = '<p class="loading">No restaurants available at the moment</p>';
            return;
        }

        // Display all restaurants
        displayRestaurants(restaurants);

    } catch (error) {
        console.error('Error loading restaurants:', error);
        container.innerHTML = `
            <div class="error-message">
                <strong>😢 Oops! Unable to load restaurants</strong>
                <p style="margin-top: 0.5rem; font-size: 0.9rem;">${escapeHtml(error.message)}</p>
            </div>
        `;
    }
}

/**
 * Display restaurants in the grid
 * @param {Array} restaurants - Array of restaurant objects
 */
function displayRestaurants(restaurants) {
    const container = document.getElementById('restaurants-grid');

    container.innerHTML = restaurants.map(restaurant => {
        const name = escapeHtml(restaurant.name || 'Unknown Restaurant');
        const cuisine = escapeHtml(restaurant.cuisineType || restaurant.cuisine || 'Various');
        const dishes = Array.isArray(restaurant.dishes) ? restaurant.dishes : [];

        // Generate dishes HTML
        const dishesHtml = dishes.length > 0
            ? `<ul class="dishes-list">
                ${dishes.slice(0, 5).map(dish => `
                    <li>
                        <span class="dish-name">${escapeHtml(dish.name || 'Dish')}</span>
                        <span class="dish-price">${dish.price ? dish.price + '€' : 'N/A'}</span>
                    </li>
                `).join('')}
                ${dishes.length > 5 ? `<li style="text-align: center; color: #999; font-style: italic;">+${dishes.length - 5} more dishes...</li>` : ''}
               </ul>`
            : '<p style="color: #999; font-style: italic; text-align: center;">No menu available</p>';

        return `
            <div class="restaurant-card">
                <div class="restaurant-header">
                    <h3>${name}</h3>
                    <div class="restaurant-meta">
                        <span class="badge">${cuisine}</span>
                        ${dishes.length > 0 ? `<span>${dishes.length} dishes</span>` : ''}
                    </div>
                </div>
                <div class="restaurant-body">
                    ${dishesHtml}
                </div>
            </div>
        `;
    }).join('');
}

/**
 * Setup smooth scrolling for anchor links
 */
function setupSmoothScrolling() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });
}

/**
 * Initialize the page when DOM is ready
 */
document.addEventListener('DOMContentLoaded', () => {
    loadRestaurants();
    setupSmoothScrolling();
});