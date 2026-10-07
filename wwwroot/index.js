const API_BASE_URL = 'https://localhost:7259'; 

let deck = [];
let currentSearchResults = [];

const searchInput = document.getElementById('searchInput');
const searchBtn = document.getElementById('searchBtn');
const resultsContainer = document.getElementById('results');
const deckListContainer = document.getElementById('deckList');
const deckCountSpan = document.getElementById('deckCount');

const usernameInput = document.getElementById('usernameInput');
const deckNameInput = document.getElementById('deckNameInput');
const saveDeckBtn = document.getElementById('saveDeckBtn');
const saveMiscBtn = document.getElementById('saveMiscBtn');

const lookupUsernameInput = document.getElementById('lookupUsernameInput');
const loadDecksBtn = document.getElementById('loadDecksBtn');
const loadedDecksContainer = document.getElementById('loadedDecksContainer');

searchBtn.addEventListener('click', searchCards);
searchInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') searchCards();
});

saveDeckBtn.addEventListener('click', () => saveDeckToServer(true));
saveMiscBtn.addEventListener('click', () => saveDeckToServer(false));
loadDecksBtn.addEventListener('click', loadUserDecks);

async function searchCards() {
    const query = searchInput.value.trim();
    if (!query) return;

    resultsContainer.innerHTML = '<p>Searching...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/api/cards/search?name=${encodeURIComponent(query)}`);
        
        if (!response.ok) {
            throw new Error('Failed to fetch cards from API.');
        }

        currentSearchResults = await response.json();
        displayResults(currentSearchResults);
    } catch (error) {
        console.error(error);
        resultsContainer.innerHTML = '<p style="color: red;">Error fetching cards.</p>';
    }
}

function displayResults(cards) {
    resultsContainer.innerHTML = '';

    if (cards.length === 0) {
        resultsContainer.innerHTML = '<p>No cards found.</p>';
        return;
    }

    cards.forEach((card, index) => {
        const cardDiv = document.createElement('div');
        cardDiv.className = 'card-item';

        const imgSrc = card.imageUri ? card.imageUri : 'https://via.placeholder.com/150?text=No+Image';

        cardDiv.innerHTML = `
            <div>
                <img src="${imgSrc}" alt="${card.name}">
                <h4>${card.name}</h4>
            </div>
            <button onclick="addToDeck(${index})" class="btn-primary">Add to Deck</button>
        `;
        resultsContainer.appendChild(cardDiv);
    });
}

function addToDeck(index) {
    const card = currentSearchResults[index];
    deck.push(card);
    updateDeckUI();
}

function removeFromDeck(index) {
    deck.splice(index, 1);
    updateDeckUI();
}

function updateDeckUI() {
    deckListContainer.innerHTML = '';
    deckCountSpan.textContent = deck.length;

    deck.forEach((card, index) => {
        const li = document.createElement('li');
        li.innerHTML = `
            <span>${card.name}</span>
            <button onclick="removeFromDeck(${index})">X</button>
        `;
        deckListContainer.appendChild(li);
    });
}

async function saveDeckToServer(isNamedDeck) {
    const username = usernameInput.value.trim();
    const deckName = deckNameInput.value.trim();

    if (!username) {
        alert('Please enter a username.');
        return;
    }

    if (deck.length === 0) {
        alert('Your list is empty.');
        return;
    }

    if (isNamedDeck && !deckName) {
        alert('Please enter a deck name.');
        return;
    }

    const payload = {
        username: username,
        deckName: isNamedDeck ? deckName : 'miscCards',
        deckCards: deck
    };

    try {
        const response = await fetch(`${API_BASE_URL}/api/cards/save`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            throw new Error('Failed to save.');
        }

        alert(isNamedDeck ? 'Deck saved successfully!' : 'Cards saved to misc successfully!');
    } catch (error) {
        console.error(error);
        alert('Error saving to server.');
    }
}

async function loadUserDecks() {
    const username = lookupUsernameInput.value.trim();
    if (!username) {
        alert('Please enter a username to lookup.');
        return;
    }

    loadedDecksContainer.style.display = 'block';
    loadedDecksContainer.textContent = 'Loading...';

    try {
        const response = await fetch(`${API_BASE_URL}/api/cards/decks/${encodeURIComponent(username)}`);

        if (!response.ok) {
            throw new Error('User decks not found.');
        }

        const data = await response.json();
        loadedDecksContainer.textContent = JSON.stringify(data, null, 2);
    } catch (error) {
        console.error(error);
        loadedDecksContainer.textContent = 'Error: User not found or failed to load decks.';
    }
}