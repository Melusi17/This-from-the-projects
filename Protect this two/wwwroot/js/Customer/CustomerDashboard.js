document.addEventListener("DOMContentLoaded", function () {
    const customerName = localStorage.getItem("loggedInCustomerName") || "Customer";
    document.getElementById("welcomeText").textContent = `Welcome, ${customerName}!`;
});

function logout() {
    localStorage.removeItem("loggedInCustomerName");
    window.location.href = "/Customer/Login.html";
}

function toggleProfileMenu() {
    const menu = document.getElementById("profileMenu");
    menu.classList.toggle("hidden");
}
let orders = []; // Array to store order data
// Add other pages navigation here if needed
    //Done with that
function attachUploadHandler() {
    const form = document.getElementById("prescriptionForm");

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const fileInput = document.getElementById("prescriptionFile");
        const date = document.getElementById("prescriptionDate").value;
        const doctor = document.getElementById("doctorName").value;
        const shouldDispense = document.getElementById("dispenseCheckbox").checked;

        if (!fileInput.files.length) {
            alert("Please select a prescription file to upload.");
            return;
        }

        const fileName = fileInput.files[0].name;

        const newEntry = {
            date,
            doctor,
            fileName,
            dispense: shouldDispense ? "Yes" : "No"
        };

        // Store in localStorage
        const prescriptions = JSON.parse(localStorage.getItem("prescriptions") || "[]");
        prescriptions.push(newEntry);
        localStorage.setItem("prescriptions", JSON.stringify(prescriptions));

        // Reset and update
        form.reset();
        document.getElementById("uploadStatus").innerHTML = `
            <p style="color: green;">
                ✅ Prescription uploaded successfully!
            </p>
        `;
        renderUnprocessedTable();
    });
}

function renderUnprocessedTable() {
    const tbody = document.getElementById("unprocessedBody");
    if (!tbody) return;

    const prescriptions = JSON.parse(localStorage.getItem("prescriptions") || "[]");

    tbody.innerHTML = ""; // Clear old rows

    prescriptions.forEach((p, index) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${p.date}</td>
            <td>${p.fileName}</td>
            <td>${p.dispense}</td>
            <td>
                <button onclick="editPrescription(${index})">Edit</button>
                <button onclick="deletePrescription(${index})">Remove</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

function deletePrescription(index) {
    const prescriptions = JSON.parse(localStorage.getItem("prescriptions") || "[]");
    prescriptions.splice(index, 1);
    localStorage.setItem("prescriptions", JSON.stringify(prescriptions));
    renderUnprocessedTable();
}

function editPrescription(index) {
    const prescriptions = JSON.parse(localStorage.getItem("prescriptions") || "[]");
    const p = prescriptions[index];

    document.getElementById("prescriptionDate").value = p.date;
    document.getElementById("doctorName").value = p.doctor;
    document.getElementById("dispenseCheckbox").checked = p.dispense === "Yes";

    prescriptions.splice(index, 1); // remove old record so edited will replace
    localStorage.setItem("prescriptions", JSON.stringify(prescriptions));
    renderUnprocessedTable();
}
function openOrderModal(date, doctor) {
    document.getElementById("modalDate").textContent = date;
    document.getElementById("modalDoctor").textContent = doctor;
    document.getElementById("orderModal").style.display = "flex";
}

function closeModal() {
    document.getElementById("orderModal").style.display = "none";
}

function updateTotal() {
    let total = 0;
    const checkboxes = document.querySelectorAll(".med-check");
    checkboxes.forEach(cb => {
        if (cb.checked) {
            total += parseFloat(cb.getAttribute("data-price"));
        }
    });

    const vat = total * 0.15; // 15% VAT
    const totalDue = (total + vat).toFixed(2);
    document.getElementById("totalDue").textContent = totalDue;
}

function placeMedicationOrder() {
    alert("Medication order has been placed successfully.");
    closeModal();
}
function attachUploadHandler() {
    const form = document.getElementById("prescriptionForm");
    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const prescriptionDate = document.getElementById("prescriptionDate").value;
        const doctorName = document.getElementById("doctorName").value;
        const shouldDispense = document.getElementById("dispenseCheckbox").checked;

        if (!prescriptionDate || !doctorName) {
            alert("Please fill in all fields.");
            return;
        }

        // Simulate placing an order
        const order = {
            orderDate: prescriptionDate,
            doctor: doctorName,
            medication: "Medication XYZ", // Hardcoded medication for simplicity
            status: "Ready for pickup",
        };

        orders.push(order); // Store the order in the orders array

        alert("Order placed successfully!");

        // Navigate back to Track Orders to display the new order
        navigateTo("trackOrders");
    });
}

function displayOrders() {
    const orderList = document.getElementById("orderList");

    // Clear the current list before adding new rows
    orderList.innerHTML = "";

    // Loop through the orders and add them as rows in the table
    orders.forEach((order) => {
        const row = document.createElement("tr");

        const dateCell = document.createElement("td");
        dateCell.textContent = order.orderDate;
        row.appendChild(dateCell);

        const doctorCell = document.createElement("td");
        doctorCell.textContent = order.doctor;
        row.appendChild(doctorCell);

        const medicationCell = document.createElement("td");
        medicationCell.textContent = order.medication;
        row.appendChild(medicationCell);

        const statusCell = document.createElement("td");
        statusCell.textContent = order.status;
        row.appendChild(statusCell);

        orderList.appendChild(row);
    });

}
function requestRefill(repeatCountId, btn) {
    const countElem = document.getElementById(repeatCountId);
    let currentRepeats = parseInt(countElem.textContent);

    if (currentRepeats > 0) {
        currentRepeats -= 1;
        countElem.textContent = currentRepeats;
        alert("✅ Refill request placed!");

        if (currentRepeats === 0) {
            btn.disabled = true;
            btn.textContent = "No repeats left";
            btn.style.backgroundColor = "#ccc";
        }
    }
}
async function generatePDFReport() {
    const { jsPDF } = window.jspdf;
    const selectedDate = document.getElementById("reportDate").value;
    if (!selectedDate) {
        alert("Please select a date.");
        return;
    }

    // 🔹 Sample/mock data - replace with dynamic values if needed
    const dispensedData = [
        {
            patient: "Sarah Nkosi",
            doctor: "Dr. John Smith",
            medication: "Amoxicillin 250mg",
            qty: 20,
            instruction: "1 capsule 3x a day",
            repeats: 1,
            price: 80,
            date: "2025-05-13"
        },
        {
            patient: "Sarah Nkosi",
            doctor: "Dr. John Smith",
            medication: "Ibuprofen 200mg",
            qty: 15,
            instruction: "1 tablet twice a day",
            repeats: 0,
            price: 45,
            date: "2025-05-13"
        }
        // Add more if needed
    ];

    const filtered = dispensedData.filter(d => d.date === selectedDate);
    if (filtered.length === 0) {
        alert("No records found for selected date.");
        return;
    }

    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text(`Medication Report - ${selectedDate}`, 14, 20);
    doc.setFontSize(12);

    let y = 30;

    const grouped = {};
    filtered.forEach(entry => {
        const { patient, doctor, medication } = entry;
        if (!grouped[patient]) grouped[patient] = {};
        if (!grouped[patient][doctor]) grouped[patient][doctor] = [];
        grouped[patient][doctor].push(entry);
    });

    for (const patient in grouped) {
        doc.setFont(undefined, "bold");
        doc.text(`Patient: ${patient}`, 14, y);
        y += 8;

        for (const doctor in grouped[patient]) {
            doc.setFont(undefined, "normal");
            doc.text(`Doctor: ${doctor}`, 20, y);
            y += 8;

            grouped[patient][doctor].forEach(med => {
                doc.text(`• ${med.medication}`, 26, y);
                y += 6;
                doc.text(`  Qty: ${med.qty} | Repeats: ${med.repeats} | Instruction: ${med.instruction}`, 30, y);
                y += 6;
                doc.text(`  Price: R${med.price.toFixed(2)}`, 30, y);
                y += 10;

                if (y > 270) {
                    doc.addPage();
                    y = 20;
                }
            });
        }
    }

    doc.save(`Medication_Report_${selectedDate}.pdf`);
}
const placeOrder = [];

function addToOrder(doctorName, medId, qtyId) {
    const medication = document.getElementById(medId).value;
    const quantity = document.getElementById(qtyId).value;

    if (quantity < 1) {
        alert("Quantity must be at least 1.");
        return;
    }

    orders.push({
        doctor: doctorName,
        medication: medication,
        quantity: quantity
    });

    updateOrderList();
}

function updateOrderList() {
    const list = document.getElementById('order-list');
    if (!list) return; // If summary not loaded, skip

    list.innerHTML = '';

    orders.forEach((item, index) => {
        list.innerHTML += `
            <div class="order-item">
                ${index + 1}. <strong>${item.medication}</strong> (x${item.quantity}) from <em>${item.doctor}</em>
            </div>`;
    });
}

function submitOrder() {
    if (orders.length === 0) {
        alert("Please add at least one medication to your order.");
        return;
    }

    console.log("Submitting order:", orders);
    alert("Order submitted successfully!");

    orders.length = 0;
    updateOrderList();
}

function toggleProfileMenu() {
    const menu = document.getElementById('profileMenu');
    menu.classList.toggle('hidden');
}

function logout() {
    alert('Logging out...');
    // Add your logout logic here
}
document.getElementById('orderDate').valueAsDate = new Date();

/*Multiple medication order*/
// Sample medication data per doctor
const medicationData = {
    1: [
        { name: "Panado", price: 10.00, instructions: "Take 1 tab 3 times/day" },
        { name: "Amoxicillin", price: 50.00, instructions: "Take 1 tab 2 times/day" }
    ],
    2: [
        { name: "Ibuprofen", price: 20.00, instructions: "Take 1 tab after meals" },
        { name: "Loratadine", price: 25.00, instructions: "Take 1 tab daily" }
    ]
};

// Set date to today
window.onload = function () {
    document.getElementById("orderDate").valueAsDate = new Date();
};

function addMedications() {
    const doctorSelect = document.getElementById("doctorSelect");
    const doctorId = doctorSelect.value;
    const doctorName = doctorSelect.options[doctorSelect.selectedIndex].text;

    if (!doctorId || !medicationData[doctorId]) return;

    // Show the table
    const table = document.getElementById("medicationTable");
    table.style.display = 'table';

    const tbody = document.getElementById("medicationBody");

    // Remove previous rows from the same doctor (optional: or clear all)
    medicationData[doctorId].forEach(med => {
        const row = document.createElement("tr");
        row.innerHTML = `
                    <td><input type="checkbox" onchange="updateTotal()"></td>
                    <td>${doctorName}</td>
                    <td>${med.name}</td>
                    <td><input type="number" value="1" min="1" onchange="updateTotal()"></td>
                    <td><input type="number" value="0" min="0"></td>
                    <td>${med.instructions}</td>
                    <td class="price">${med.price.toFixed(2)}</td>
                `;
        tbody.appendChild(row);
    });
}

function updateTotal() {
    let subtotal = 0;
    const rows = document.querySelectorAll("#medicationBody tr");

    rows.forEach(row => {
        const checkbox = row.querySelector("input[type='checkbox']");
        const qtyInput = row.cells[3].querySelector("input");
        const price = parseFloat(row.querySelector(".price").textContent);

        if (checkbox.checked) {
            const qty = parseInt(qtyInput.value);
            subtotal += qty * price;
        }
    });

    const tax = subtotal * 0.15;
    const total = subtotal + tax;

    document.getElementById("subtotal").textContent = subtotal.toFixed(2);
    document.getElementById("tax").textContent = tax.toFixed(2);
    document.getElementById("total").textContent = total.toFixed(2);
}
