function toggleDetails(card) {
    const medList = card.querySelector('.medication-list');
    medList.style.display = medList.style.display === 'block' ? 'none' : 'block';
}

function filterCards() {
    const input = document.getElementById("searchInput").value.toLowerCase();
    const cards = document.querySelectorAll(".order-card");

    cards.forEach(card => {
        const text = card.innerText.toLowerCase();
        card.style.display = text.includes(input) ? "block" : "none";
    });
}