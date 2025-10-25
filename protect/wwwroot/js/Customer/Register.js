/***** Track selected allergy IDs *****/
let allergies = []; // array of allergy IDs

const allergySelect = document.getElementById('allergySelect');
const allergyCart = document.getElementById('allergyCart');
const hiddenAllergiesInput = document.getElementById('SelectedAllergies');

/***** Add selected allergy *****/
document.getElementById('addAllergyBtn').addEventListener('click', () => {
    const chosenId = allergySelect.value;
    if (!chosenId || allergies.includes(chosenId)) return;

    allergies.push(chosenId);
    updateAllergyCart();
});

/***** Remove allergy by index *****/
function removeAllergy(i) {
    allergies.splice(i, 1);
    updateAllergyCart();
}

/***** Render allergy cart UI *****/
function updateAllergyCart() {
    allergyCart.innerHTML = '';

    allergies.forEach((id, i) => {
        // Get the display name from the select option
        const option = document.querySelector(`#allergySelect option[value="${id}"]`);
        const text = option ? option.textContent : id;

        const li = document.createElement('li');
        li.textContent = text;

        const btn = document.createElement('button');
        btn.type = 'button';
        btn.textContent = '×';
        btn.className = 'remove-btn';
        btn.onclick = () => removeAllergy(i);

        li.appendChild(btn);
        allergyCart.appendChild(li);
    });

    // Update hidden input for MVC model binding
    hiddenAllergiesInput.value = allergies.join(',');
}

/***** Form validation and submission *****/
document.getElementById('registerForm').addEventListener('submit', function (e) {
    // Clear previous errors
    document.querySelectorAll('.error-message').forEach(el => el.textContent = '');

    const name = document.getElementById('Name').value.trim();
    const surname = document.getElementById('Surname').value.trim();
    const idNumber = document.getElementById('IdNumber').value.trim();
    const email = document.getElementById('Email').value.trim();
    const cellphone = document.getElementById('Cellphone').value.trim();
    const password = document.getElementById('Password').value;
    const confirmPassword = document.getElementById('ConfirmPassword').value;

    let hasError = false;

    if (!name) {
        document.querySelector('[data-valmsg-for="Name"]').textContent = 'Name is required.';
        hasError = true;
    }

    if (!surname) {
        document.querySelector('[data-valmsg-for="Surname"]').textContent = 'Surname is required.';
        hasError = true;
    }

    if (!idNumber) {
        document.querySelector('[data-valmsg-for="IdNumber"]').textContent = 'ID Number is required.';
        hasError = true;
    } else if (!/^\d{13}$/.test(idNumber)) {
        document.querySelector('[data-valmsg-for="IdNumber"]').textContent = 'ID must be 13 digits.';
        hasError = true;
    }

    if (!email) {
        document.querySelector('[data-valmsg-for="Email"]').textContent = 'Email is required.';
        hasError = true;
    } else if (!/^\S+@\S+\.\S+$/.test(email)) {
        document.querySelector('[data-valmsg-for="Email"]').textContent = 'Enter a valid email address.';
        hasError = true;
    }

    if (!cellphone) {
        document.querySelector('[data-valmsg-for="Cellphone"]').textContent = 'Cellphone is required.';
        hasError = true;
    } else if (!/^\d{10}$/.test(cellphone)) {
        document.querySelector('[data-valmsg-for="Cellphone"]').textContent = 'Enter a 10-digit cellphone number.';
        hasError = true;
    }

    if (!password) {
        document.querySelector('[data-valmsg-for="Password"]').textContent = 'Password is required.';
        hasError = true;
    }

    if (!confirmPassword) {
        document.querySelector('[data-valmsg-for="ConfirmPassword"]').textContent = 'Please confirm password.';
        hasError = true;
    } else if (password !== confirmPassword) {
        document.querySelector('[data-valmsg-for="ConfirmPassword"]').textContent = 'Passwords do not match.';
        hasError = true;
    }

    if (hasError) {
        e.preventDefault(); // stop form submission
        return;
    }

    // Push allergy IDs to hidden field before submission
    hiddenAllergiesInput.value = allergies.join(',');
    // Form will submit normally to MVC
});

/***** Toggle password visibility *****/
function togglePassword(fieldId, icon) {
    const field = document.getElementById(fieldId);
    const isPass = field.type === 'password';
    field.type = isPass ? 'text' : 'password';
}
