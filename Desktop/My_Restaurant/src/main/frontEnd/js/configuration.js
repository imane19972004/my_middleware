// ============================================
// confirmation.js - Page de confirmation
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    const summary = document.getElementById('summary');
    summary.innerHTML = `
        <div class="success-message">
            <h2>✅ Commande Confirmée !</h2>
            <p>Votre commande a été enregistrée avec succès.</p>
            <p>Vous recevrez une notification lorsqu'elle sera prête.</p>
        </div>
    `;
});