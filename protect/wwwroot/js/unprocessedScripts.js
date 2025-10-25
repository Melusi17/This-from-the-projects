document.addEventListener('DOMContentLoaded', function () {
    setupEventListeners();
    setupCardListeners(); // setup buttons & toggles for static cards
});

function setupEventListeners() {
    const searchBtn = document.querySelector('.search-bar button');
    const input = document.getElementById('searchInput');

    if (searchBtn) {
        searchBtn.addEventListener('click', searchScripts);
    }

    if (input) {
        input.addEventListener('keyup', function (e) {
            if (e.key === 'Enter') searchScripts();
        });
    }
}

function setupCardListeners() {
    const cards = document.querySelectorAll('.script-card');

    cards.forEach(card => {
        const scriptId = card.dataset.scriptId;

        // Toggle details on card click
        card.addEventListener('click', function (e) {
            if (!e.target.closest('button') && !e.target.closest('.prescription-option')) {
                card.classList.toggle('expanded');
            }
        });

        // Prescription selection
        card.querySelectorAll('.prescription-option').forEach(opt => {
            opt.addEventListener('click', function (e) {
                e.stopPropagation();
                opt.parentNode.querySelectorAll('.prescription-option').forEach(o => o.classList.remove('selected'));
                opt.classList.add('selected');
            });
        });

        // Email button
        const emailBtn = card.querySelector('.email-btn');
        if (emailBtn) {
            emailBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                const patient = card.querySelector('.script-patient').textContent;
                alert(`Email sent to patient: ${patient} (Script ID: ${scriptId})`);
            });
        }

        // Dispense button
        const dispenseBtn = card.querySelector('.dispense-btn');
        if (dispenseBtn) {
            dispenseBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                const selected = card.querySelector('.prescription-option.selected');
                if (!selected) return alert('Please select a prescription first');

                const pdf = selected.dataset.pdf;
                const doctor = selected.querySelector('strong').textContent;

                alert(`Dispensing prescription from ${doctor}\nPDF: ${pdf}`);
                const status = card.querySelector('.script-status');
                status.className = 'script-status status-ready';
                status.textContent = 'Processing';
                card.classList.remove('expanded');
            });
        }
    });
}

function searchScripts() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const cards = document.querySelectorAll('.script-card');
    const loading = document.getElementById('loading');
    const scriptsList = document.getElementById('scriptsList');

    loading.style.display = 'block';
    scriptsList.style.opacity = '0.5';

    setTimeout(() => {
        let foundAny = false;
        cards.forEach(card => {
            const patientText = card.querySelector('.script-patient').textContent.toLowerCase();
            const shouldShow = patientText.includes(searchTerm);
            card.style.display = shouldShow ? 'block' : 'none';
            if (shouldShow) foundAny = true;
        });

        if (!foundAny) {
            scriptsList.innerHTML = '<div class="no-results">No prescriptions found matching your search</div>';
        }

        loading.style.display = 'none';
        scriptsList.style.opacity = '1';
    }, 800);
}
