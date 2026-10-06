const API_BASE_URL = 'https://localhost:7259'; 

let deck = [];

const searchInput = document.getElementById('searchInput');
const searchBtn = document.getElementById('searchBtn');
const resultsContainer = document.getElementById('results');
const deckListContainer = document.getElementById('deckList');
const deckCountSpan = document.getElementById('deckCount');

searchBtn.addEventListener('click', searchCards);
searchInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') searchCards();
});

async function searchCards() {
    const query = searchInput.value.trim();
    if (!query) return;

    resultsContainer.innerHTML = '<p>Searching...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/api/cards/search?name=${encodeURIComponent(query)}`);
        
        if (!response.ok) {
            throw new Error('Failed to fetch cards from API.');
        }

        const cards = await response.json();
        displayResults(cards);
    } catch (error) {
        console.error(error);
        resultsContainer.innerHTML = '<p style="color: red;">Error fetching cards. Make sure your C# API is running and CORS is allowed if needed.</p>';
    }
}

function displayResults(cards) {
    resultsContainer.innerHTML = '';

    if (cards.length === 0) {
        resultsContainer.innerHTML = '<p>No cards found.</p>';
        return;
    }

    cards.forEach(card => {
        const cardDiv = document.createElement('div');
        cardDiv.className = 'card-item';

        const imgSrc = card.imageUri ? card.imageUri : 'https://via.placeholder.com/150?text=No+Image';

        cardDiv.innerHTML = `
            <div>
                <img src="${imgSrc}" alt="${card.name}">
                <h4>${card.name}</h4>
            </div>
            <button onclick="addToDeck('${escapeHtml(card.name)}')">Add to Deck</button>
        `;
        resultsContainer.appendChild(cardDiv);
    });
}

function addToDeck(cardName) {
    deck.push(cardName);
    updateDeckUI();
}

function removeFromDeck(index) {
    deck.splice(index, 1);
    updateDeckUI();
}

function updateDeckUI() {
    deckListContainer.innerHTML = '';
    deckCountSpan.textContent = deck.length;

    deck.forEach((cardName, index) => {
        const li = document.createElement('li');
        li.innerHTML = `
            <span>${cardName}</span>
            <button onclick="removeFromDeck(${index})">X</button>
        `;
        deckListContainer.appendChild(li);
    });
}

function escapeHtml(text) {
    return text.replace(/"/g, '&quot;');
}