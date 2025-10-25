/*alert("Login.js loaded");*/
document.getElementById('loginForm').addEventListener('submit', function (e) {
    e.preventDefault();
    clearErrors();

    const email = document.getElementById('email');
    const password = document.getElementById('password');
    let valid = true;

    // Email validation
    if (!email.value.trim()) {
        showError(email, "Email is required");
        valid = false;
    } else if (!validateEmail(email.value.trim())) {
        showError(email, "Enter a valid email address");
        valid = false;
    }

    // Password validation
    if (!password.value.trim()) {
        showError(password, "Password is required");
        valid = false;
    } else if (password.value.trim().length !== 13) {
        showError(password, "Password must be exactly 13 digits");
        valid = false;
    }

    if (!valid) return;

    const inputEmail = email.value.trim();
    const inputPassword = password.value.trim();

    // 🔐 Hardcoded user credentials
    const hardcodedUser = {
        email: "admin@example.com",
        password: "1234567890123",
        name: "Admin User"
    };

    // ✅ Check against hardcoded user
    if (inputEmail === hardcodedUser.email && inputPassword === hardcodedUser.password) {
        localStorage.setItem("loggedInCustomerName", hardcodedUser.name);
        alert("Login successful (hardcoded user)!");
        window.location.href = "CustomerDashboard.html";
        return;
    }

    // 🔎 Check LocalStorage for registered user
    const users = JSON.parse(localStorage.getItem("customers")) || [];
    const userMatch = users.find(u => u.email === inputEmail && u.password === inputPassword);

    if (userMatch) {
        localStorage.setItem("loggedInCustomerName", userMatch.name);
        window.location.href = "CustomerDashboard.html";
    } else {
        showError(password, "Invalid email or password");
    }
});

function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email.toLowerCase());
}

function showError(input, message) {
    const error = document.createElement('small');
    error.className = 'error-message';
    error.innerText = message;
    input.parentElement.appendChild(error);
    input.style.borderColor = '#e74c3c';
}

function clearErrors() {
    document.querySelectorAll('.error-message').forEach(el => el.remove());
    document.querySelectorAll('.input-group input').forEach(input => {
        input.style.borderColor = '#ccc';
    });
}
