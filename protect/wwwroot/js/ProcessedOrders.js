// Sample patient data - all prescriptions are processed in this version
const patients = [
    {
        id: "9001011234081",
        name: "John Doe",
        prescriptions: [
            { date: "2025-01-15", file: "prescription_001.pdf", processed: true, processedDate: "2025-01-16" },
            { date: "2025-02-22", file: "prescription_002.pdf", processed: true, processedDate: "2025-02-23" }
        ]
    },
    {
        id: "8902021234082",
        name: "Jane Smith",
        prescriptions: [
            { date: "2025-03-10", file: "prescription_003.pdf", processed: true, processedDate: "2025-03-11" }
        ]
    },
    {
        id: "9103031234083",
        name: "Robert Johnson",
        prescriptions: [
            { date: "2025-04-05", file: "prescription_004.pdf", processed: true, processedDate: "2025-04-06" },
            { date: "2025-05-12", file: "prescription_005.pdf", processed: true, processedDate: "2025-05-13" }
        ]
    },
    {
        id: "9204041234084",
        name: "Sarah Williams",
        prescriptions: []
    }
];

// Initialize with all patients
document.addEventListener('DOMContentLoaded', function () {
    displayPatients(patients);
});

// Toggle sidebar
function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('collapsed');

    const toggleBtn = document.querySelector('.toggle-sidebar i');
    if (sidebar.classList.contains('collapsed')) {
        toggleBtn.classList.remove('fa-chevron-left');
        toggleBtn.classList.add('fa-chevron-right');
    } else {
        toggleBtn.classList.remove('fa-chevron-right');
        toggleBtn.classList.add('fa-chevron-left');
    }
}

// Search patients by name or ID
function searchPatients() {
    const searchTerm = document.getElementById('patientSearch').value.toLowerCase();
    if (searchTerm.trim() === '') {
        displayPatients(patients);
        return;
    }

    const filteredPatients = patients.filter(patient =>
        patient.name.toLowerCase().includes(searchTerm) ||
        patient.id.includes(searchTerm)
    );

    displayPatients(filteredPatients);
}

// Display patients in cards
function displayPatients(patientList) {
    const container = document.getElementById('patientCards');
    container.innerHTML = '';

    if (patientList.length === 0) {
        document.getElementById('noResults').style.display = 'block';
        document.getElementById('prescriptionTableContainer').style.display = 'none';
        return;
    }

    document.getElementById('noResults').style.display = 'none';

    patientList.forEach(patient => {
        const card = document.createElement('div');
        card.className = 'patient-card';
        card.innerHTML = `
                    <div class="patient-name">${patient.name}</div>
                    <div class="patient-id">ID: ${patient.id}</div>
                `;

        card.addEventListener('click', function () {
            // Remove active class from all cards
            document.querySelectorAll('.patient-card').forEach(c => {
                c.classList.remove('active');
            });

            // Add active class to clicked card
            this.classList.add('active');

            // Show prescriptions for this patient
            showPrescriptions(patient);
        });

        container.appendChild(card);
    });
}

// Show prescriptions for selected patient
function showPrescriptions(patient) {
    document.getElementById('selectedPatientName').textContent = `Prescriptions for ${patient.name}`;
    const tableBody = document.getElementById('prescriptionTableBody');
    tableBody.innerHTML = '';

    if (patient.prescriptions.length === 0) {
        tableBody.innerHTML = `
                    <tr>
                        <td colspan="4" style="text-align: center; color: #95a5a6; font-style: italic;">
                            No prescriptions found for this patient
                        </td>
                    </tr>
                `;
    } else {
        patient.prescriptions.forEach(prescription => {
            const row = document.createElement('tr');
            row.innerHTML = `
                        <td>${formatDate(prescription.date)}</td>
                        <td class="prescription-file">
                            <i class="fas fa-file-pdf"></i> ${prescription.file}
                        </td>
                        <td>${formatDate(prescription.processedDate)}</td>
                        <td>
                            <span class="process-status ${prescription.processed ? 'process-yes' : 'process-no'}">
                                ${prescription.processed ? 'Processed' : 'Pending'}
                            </span>
                        </td>
                    `;
            tableBody.appendChild(row);
        });
    }

    document.getElementById('prescriptionTableContainer').style.display = 'block';
}

// Format date as DD/MM/YYYY
function formatDate(dateString) {
    const date = new Date(dateString);
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}