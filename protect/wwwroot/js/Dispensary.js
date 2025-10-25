
        // Sample medication data
    const medications = [
    {
        id: 1,
    name: "Panado Extra",
    active: "Paracetamol 500mg + Caffeine 65mg",
    stock: 256,
    reorder: 100,
    forms: ["Tablet", "Capsule"],
    schedule: "S0"
            },
    {
        id: 2,
    name: "Amoxicillin 500mg",
    active: "Amoxicillin",
    stock: 142,
    reorder: 50,
    forms: ["Capsule"],
    schedule: "S4"
            },
    {
        id: 3,
    name: "Lisinopril 10mg",
    active: "Lisinopril",
    stock: 89,
    reorder: 30,
    forms: ["Tablet"],
    schedule: "S2"
            },
    {
        id: 4,
    name: "Ventolin Inhaler",
    active: "Salbutamol 100mcg",
    stock: 75,
    reorder: 30,
    forms: ["Inhaler"],
    schedule: "S3"
            },
    {
        id: 5,
    name: "Augmentin 625mg",
    active: "Amoxicillin 500mg + Clavulanic acid 125mg",
    stock: 68,
    reorder: 25,
    forms: ["Tablet"],
    schedule: "S4"
            }
    ];

    const commonInstructions = [
    "Take 1 tablet daily",
    "Take 1 tablet twice daily",
    "Take 2 tablets at bedtime",
    "Take as needed for pain",
    "Other (specify)"
    ];

    // Track which medication row is being rejected
    let currentRejectionRow = null;

    // Initialize with sample data
    document.addEventListener('DOMContentLoaded', function() {
        // Load sample prescription items
        addMedicationRowWithData({
            id: 2,
            name: "Amoxicillin 500mg",
            form: "Capsule",
            qty: 14,
            instructions: "Take 1 capsule twice daily for 7 days",
            repeats: 2,
            prescription: "prescription_123.pdf",
            doctor: "Dr. Johnson",
            dateIssued: "2023-06-15"
        });

    addMedicationRowWithData({
        id: 3,
    name: "Lisinopril 10mg",
    form: "Tablet",
    qty: 30,
    instructions: "Take 1 tablet daily",
    repeats: 0,
    prescription: "prescription_123.pdf",
    doctor: "Dr. Williams",
    dateIssued: "2023-06-15"
            });

    // Add medication button
    document.getElementById('addMedBtn').addEventListener('click', addMedicationRow);
        });

    // Toggle sidebar collapse
    function toggleSidebar() {
            const sidebar = document.getElementById('sidebar');
    const icon = document.getElementById('sidebarIcon');

    sidebar.classList.toggle('collapsed');
    if (sidebar.classList.contains('collapsed')) {
        icon.classList.remove('fa-chevron-left');
    icon.classList.add('fa-chevron-right');
            } else {
        icon.classList.remove('fa-chevron-right');
    icon.classList.add('fa-chevron-left');
            }
        }

    // Add new empty medication row
    function addMedicationRow() {
            const tbody = document.getElementById('medsTableBody');
    const rowId = Date.now();

    // Create datalist options
    let medOptions = '';
            medications.forEach(med => {
        medOptions += `<option value="${med.name} (${med.schedule})" data-id="${med.id}">${med.active}</option>`;
            });

    // Instruction options
    let instructionOptions = '';
            commonInstructions.forEach(inst => {
        instructionOptions += `<option value="${inst}">${inst}</option>`;
            });

    const row = document.createElement('tr');
    row.id = `row-${rowId}`;
    row.innerHTML = `
    <td><input type="checkbox" class="dispense-checkbox"></td>
    <td>
        <div class="searchable-select">
            <input list="meds-list-${rowId}" id="med-input-${rowId}"
                placeholder="Search or select medication..."
                oninput="filterMeds(this, ${rowId})">
                <datalist id="meds-list-${rowId}">
                    ${medOptions}
                </datalist>
        </div>
        <div class="med-details" id="details-${rowId}"></div>
        <div class="prescription-info" id="prescription-${rowId}"></div>
    </td>
    <td>
        <select class="form-select" id="form-${rowId}">
            <option value="">Select form</option>
        </select>
    </td>
    <td><input type="number" min="1" value="1" class="qty-input"></td>
    <td>
        <select class="inst-select" onchange="handleInstructionSelect(this, ${rowId})">
            ${instructionOptions}
        </select>
        <div id="custom-inst-${rowId}" style="display:none; margin-top:5px;">
            <textarea placeholder="Enter custom instructions..." rows="2"></textarea>
        </div>
    </td>
    <td>
        <input type="number" class="repeats-input" min="0" value="0">
    </td>
    <td><span class="badge bg-warning">Pending</span></td>
    <td>
        <button class="action-btn reject-btn" onclick="showRejectionModal(this)"><i class="fas fa-ban"></i></button>
        <button class="action-btn remove-btn" onclick="removeMedication(this)"><i class="fas fa-trash"></i></button>
    </td>
    `;

    tbody.appendChild(row);

    // Add event listener for medication selection
    document.getElementById(`med-input-${rowId}`).addEventListener('change', function() {
                const selectedOption = document.querySelector(`#meds-list-${rowId} option[value="${this.value}"]`);
    if (selectedOption) {
        updateMedDetails(selectedOption.getAttribute('data-id'), rowId);
                }
            });
        }

    // Add row with existing medication data
    function addMedicationRowWithData(medData) {
            const tbody = document.getElementById('medsTableBody');
    const rowId = Date.now();
            
            const med = medications.find(m => m.id === medData.id);

    const row = document.createElement('tr');
    row.id = `row-${rowId}`;
    row.innerHTML = `
    <td><input type="checkbox" class="dispense-checkbox"></td>
    <td>
        <div class="searchable-select">
            <input list="meds-list-${rowId}" id="med-input-${rowId}"
                value="${medData.name}"
                placeholder="Search or select medication..."
                oninput="filterMeds(this, ${rowId})">
                <datalist id="meds-list-${rowId}">
                    ${medications.map(m =>
                        `<option value="${m.name} (${m.schedule})" data-id="${m.id}">${m.active}</option>`
                    ).join('')}
                </datalist>
        </div>
        <div class="med-details">
            <small><strong>Active:</strong> ${med?.active || ''}</small><br>
                <small><strong>Stock:</strong> ${med?.stock || ''}</small>
        </div>
        <div class="prescription-info">
            <small><strong>Prescription:</strong> ${medData.prescription}</small><br>
                <small><strong>Doctor:</strong> ${medData.doctor || 'Dr. Smith'}</small><br>
                    <small><strong>Date Issued:</strong> ${medData.dateIssued}</small>
                </div>
            </td>
            <td>
                <select class="form-select" id="form-${rowId}">
                    <option value="">Select form</option>
                    ${med?.forms.map(form =>
                        `<option value="${form}" ${form === medData.form ? 'selected' : ''}>${form}</option>`
                    ).join('')}
                </select>
            </td>
            <td><input type="number" min="1" value="${medData.qty}" class="qty-input"></td>
            <td>
                <select class="inst-select" onchange="handleInstructionSelect(this, ${rowId})">
                    ${commonInstructions.map(inst =>
                        `<option value="${inst}" ${inst === medData.instructions ? 'selected' : ''}>${inst}</option>`
                    ).join('')}
                </select>
                <div id="custom-inst-${rowId}" style="display:none; margin-top:5px;">
                    <textarea placeholder="Enter custom instructions..." rows="2"></textarea>
                </div>
            </td>
            <td>
                <input type="number" class="repeats-input" min="0" value="${medData.repeats}">
            </td>
            <td><span class="badge bg-warning">Pending</span></td>
            <td>
                <button class="action-btn reject-btn" onclick="showRejectionModal(this)"><i class="fas fa-ban"></i></button>
                <button class="action-btn remove-btn" onclick="removeMedication(this)"><i class="fas fa-trash"></i></button>
            </td>
            `;

            tbody.appendChild(row);

            // Add allergy class if needed
            if (medData.name.includes("Amoxicillin")) {
                row.classList.add('has-allergy');
            document.querySelector('.allergy-alert').style.display = 'block';
            }

            // Add low stock class if needed
            if (med?.stock < med?.reorder) {
                row.classList.add('low-stock');
            document.querySelector('.stock-alert').style.display = 'block';
            }
        }

            // Filter medications as user types
            function filterMeds(input, rowId) {
            const searchTerm = input.value.toLowerCase();
            const options = document.querySelectorAll(`#meds-list-${rowId} option`);
            
            options.forEach(option => {
                const text = option.value.toLowerCase();
            option.style.display = text.includes(searchTerm) ? 'block' : 'none';
            });
        }

            // Update medication details when selected
            function updateMedDetails(medId, rowId) {
            const detailsDiv = document.querySelector(`#row-${rowId} .med-details`);
            const prescriptionDiv = document.querySelector(`#row-${rowId} .prescription-info`);
            const formSelect = document.getElementById(`form-${rowId}`);
            const row = document.getElementById(`row-${rowId}`);
            
            const med = medications.find(m => m.id == medId);
            if (med) {
                detailsDiv.innerHTML = `
                    <small><strong>Active:</strong> ${med.active}</small><br>
                    <small><strong>Stock:</strong> ${med.stock}</small>
                `;

            // Add prescription info (sample data)
            prescriptionDiv.innerHTML = `
            <small><strong>Prescription:</strong> prescription_${Math.floor(Math.random() * 1000)}.pdf</small><br>
                <small><strong>Doctor:</strong> Dr. ${['Smith', 'Johnson', 'Williams', 'Brown', 'Jones'][Math.floor(Math.random() * 5)]}</small><br>
                    <small><strong>Date Issued:</strong> ${new Date().toISOString().split('T')[0]}</small>
                    `;

                    formSelect.innerHTML = '<option value="">Select form</option>';
                med.forms.forEach(form => {
                        formSelect.innerHTML += `<option value="${form}">${form}</option>`;
                });

                    // Check for allergies
                    if (med.name.includes("Amoxicillin")) {
                        row.classList.add('has-allergy');
                    document.querySelector('.allergy-alert').style.display = 'block';
                } else {
                        row.classList.remove('has-allergy');
                }

                    // Check for low stock
                    if (med.stock < med.reorder) {
                        row.classList.add('low-stock');
                    document.querySelector('.stock-alert').style.display = 'block';
                } else {
                        row.classList.remove('low-stock');
                }
            }
        }

                    // Handle instruction selection
                    function handleInstructionSelect(select, rowId) {
            const customDiv = document.getElementById(`custom-inst-${rowId}`);
                    customDiv.style.display = select.value === "Other (specify)" ? 'block' : 'none';
        }

                    // Show rejection modal
                    function showRejectionModal(button) {
                        currentRejectionRow = button.closest('tr');
                    document.getElementById('rejectionModal').style.display = 'flex';
        }

                    // Close modal
                    function closeModal() {
                        document.getElementById('rejectionModal').style.display = 'none';
            document.querySelectorAll('input[name="rejectionReason"]').forEach(radio => {
                        radio.checked = false;
            });
        }

                    // Confirm rejection
                    function confirmRejection() {
            const selectedReason = document.querySelector('input[name="rejectionReason"]:checked');

                    if (!selectedReason) {
                        alert('Please select a rejection reason');
                    return;
            }

                    // Update the row
                    currentRejectionRow.classList.add('rejected-row');

                    // Change status badge
                    const statusCell = currentRejectionRow.querySelector('td:nth-child(7)');
                    statusCell.innerHTML = '<span class="badge bg-danger">Rejected</span>';

                    // Hide checkbox and reject button
                    const checkbox = currentRejectionRow.querySelector('.dispense-checkbox');
                    const rejectBtn = currentRejectionRow.querySelector('.reject-btn');
                    if (checkbox) checkbox.style.display = 'none';
                    if (rejectBtn) rejectBtn.style.display = 'none';

                    closeModal();
        }

                    // Remove medication row
                    function removeMedication(button) {
            if (confirm('Are you sure you want to remove this medication?')) {
                const row = button.closest('tr');
                    const hadAllergy = row.classList.contains('has-allergy');
                    const hadLowStock = row.classList.contains('low-stock');

                    row.remove();

                    // Update alerts if needed
                    if (hadAllergy && !document.querySelector('.has-allergy')) {
                        document.querySelector('.allergy-alert').style.display = 'none';
                }

                    if (hadLowStock && !document.querySelector('.low-stock')) {
                        document.querySelector('.stock-alert').style.display = 'none';
                }
            }
        }

                    // Mark checked medications as dispensed
                    function markAsDispensed() {
            const checkboxes = document.querySelectorAll('.dispense-checkbox:checked');

                    if (checkboxes.length === 0) {
                        alert('Please select at least one medication to mark as dispensed');
                    return;
            }

                    if (confirm(`Mark ${checkboxes.length} medication(s) as dispensed?`)) {
                        checkboxes.forEach(checkbox => {
                            const row = checkbox.closest('tr');
                            const repeatsInput = row.querySelector('.repeats-input');

                            if (repeatsInput) {
                                let repeatsLeft = parseInt(repeatsInput.value);

                                if (repeatsLeft > 0) {
                                    // Decrease repeats if available
                                    repeatsLeft--;
                                    repeatsInput.value = repeatsLeft;

                                    // Keep the row active if there are repeats left
                                    if (repeatsLeft > 0) {
                                        return;
                                    }
                                }
                            }

                            // If no repeats left, mark as dispensed
                            row.classList.add('dispensed-row');

                            // Update status
                            const statusCell = row.querySelector('td:nth-child(7)');
                            statusCell.innerHTML = '<span class="badge bg-success">Dispensed</span>';

                            // Remove checkbox and action buttons
                            checkbox.remove();
                            const actionButtons = row.querySelectorAll('.action-btn');
                            actionButtons.forEach(btn => btn.remove());
                        });
            }
        }
              