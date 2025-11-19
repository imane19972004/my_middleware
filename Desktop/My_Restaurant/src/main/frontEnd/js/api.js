//nouvelle Amélioration pour réduire les appels inutiles : Debounce / Throtlle
//api.js sert à centrer toutes les fonctions liées aux appels API(contre loadRestaurants pour chaque changement par ex: filtrage)

// ============================================
// api.js - Utilitaires pour les appels API
// ============================================

const API_CATALOG_URL = 'http://localhost:8081';
const API_ORDER_URL = 'http://localhost:8082';

// ========== DEBOUNCE ==========
// Attend que l'utilisateur arrête d'interagir avant d'appeler l'API
function debounce(func, delay = 300) {
    let timeoutId;
    return function(...args) {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => func.apply(this, args), delay);
    };
}

// ========== THROTTLE ==========
// Limite la fréquence d'appels (ex: max 1 appel par seconde)
function throttle(func, limit = 1000) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// ========== CACHE SIMPLE ==========
const apiCache = new Map();

function getCachedData(key) {
    if (apiCache.has(key)) {
        const cached = apiCache.get(key);
        const now = Date.now();
        // Cache valide pendant 5 minutes
        if (now - cached.timestamp < 300000) {
            console.log('📦 Cache hit:', key);
            return cached.data;
        } else {
            apiCache.delete(key); // Supprimer cache expiré
        }
    }
    return null;
}

function setCachedData(key, data) {
    apiCache.set(key, {
        data: data,
        timestamp: Date.now()
    });
}

// ========== FETCH AVEC CACHE ==========
async function fetchWithCache(url, options = {}) {
    const cacheKey = url + JSON.stringify(options);
    
    // Vérifier le cache
    const cached = getCachedData(cacheKey);
    if (cached) return cached;
    
    try {
        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        const data = await response.json();
        
        // Sauvegarder en cache
        setCachedData(cacheKey, data);
        
        return data;
    } catch (error) {
        console.error('❌ Fetch error:', error);
        throw error;
    }
}

// ========== FETCH AVEC RETRY ==========
async function fetchWithRetry(url, options = {}, maxRetries = 3) {
    for (let i = 0; i < maxRetries; i++) {
        try {
            const response = await fetch(url, options);
            if (!response.ok && i < maxRetries - 1) {
                console.warn(`⚠️ Tentative ${i + 1} échouée, réessai...`);
                await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1)));
                continue;
            }
            return await response.json();
        } catch (error) {
            if (i === maxRetries - 1) throw error;
            console.warn(`⚠️ Tentative ${i + 1} échouée, réessai...`);
            await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1)));
        }
    }
}

// ========== API ENDPOINTS ==========
const API = {
    // Catalog Service (Port 8081)
    getRestaurants: (filters = {}) => {
        const params = new URLSearchParams(filters);
        return fetchWithCache(`${API_CATALOG_URL}/api/restaurants?${params}`);
    },
    
    getRestaurantById: (id) => {
        return fetchWithCache(`${API_CATALOG_URL}/api/restaurants/${id}`);
    },
    
    // Order Service (Port 8082)
    createOrder: (orderData) => {
        return fetch(`${API_ORDER_URL}/api/orders`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        }).then(res => res.json());
    },
    
    getTimeSlots: (restaurantId) => {
        return fetchWithCache(`${API_ORDER_URL}/api/timeslots?restaurantId=${restaurantId}`);
    },
    
    processPayment: (paymentData) => {
        return fetch(`${API_ORDER_URL}/api/payment`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(paymentData)
        }).then(res => res.json());
    }
};