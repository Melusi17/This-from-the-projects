 // Sample data with more medications
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
            },
    {
        id: 6,
    name: "Simvastatin 20mg",
    active: "Simvastatin",
    stock: 120,
    reorder: 40,
    forms: ["Tablet"],
    schedule: "S3"
            },
    {
        id: 7,
    name: "Metformin 850mg",
    active: "Metformin hydrochloride",
    stock: 185,
    reorder: 60,
    forms: ["Tablet"],
    schedule: "S1"
            }
    ];

    const commonInstructions = [
    "Take 1 tablet daily",
    "Take 1 tablet twice daily",
    "Take 2 tablets at bedtime",
    "Take as needed for pain",
    "Other (specify)"
    ];

    // Initialize with one medication row
    document.addEventListener('DOMContentLoaded', function() {
        addMedicationRow();

    // Patient modal
    document.getElementById('addPatientBtn').addEventListener('click', function() {
        document.getElementById('patientModal').style.display = 'flex';
            });

    // Doctor modal
    document.getElementById('addDoctorBtn').addEventListener('click', function() {
        document.getElementById('doctorModal').style.display = 'flex';
            });

    // PDF upload handler
    document.getElementById('prescriptionUpload').addEventListener('change', function(e) {
                const file = e.target.files[0];
    const pdfViewer = document.getElementById('pdfViewer');

    if (file && file.type === 'application/pdf') {
                    const reader = new FileReader();
    reader.onload = function(e) {
        pdfViewer.innerHTML = `
                            <embed src="${e.target.result}" type="application/pdf" width="100%" height="100%">
                        `;
                    };
    reader.readAsDataURL(file);
                } else {
        pdfViewer.innerHTML = '<p class="placeholder">Please upload a valid PDF file.</p>';
                }
            });
        });

    // Function to load existing prescription
    function loadExistingPrescription(prescriptionId) {
            if (!prescriptionId) return;

    // In a real app, this would fetch prescription data from the server
    console.log("Loading prescription:", prescriptionId);

    // For demo purposes, we'll just show a message
    const pdfViewer = document.getElementById('pdfViewer');
    pdfViewer.innerHTML = `
    <div style="text-align: center; padding: 20px;">
        <i class="fas fa-file-prescription" style="font-size: 48px; color: #3498db;"></i>
        <p>Loading prescription #${prescriptionId}</p>
        <p>This would display the prescription PDF in a real application</p>
    </div>
    `;

    // Also populate the medications table (demo only)
    const tbody = document.getElementById('medsTableBody');
    tbody.innerHTML = ''; // Clear existing rows

    // Add sample medications based on prescription ID
    if (prescriptionId === "1") {
                // Sample data for prescription 1
                const sampleMeds = [
    {id: 3, name: "Lisinopril 10mg", form: "Tablet", qty: 30, instructions: "Take 1 tablet daily", repeats: 2},
    {id: 6, name: "Simvastatin 20mg", form: "Tablet", qty: 30, instructions: "Take 1 tablet at bedtime", repeats: 2}
    ];
                
                sampleMeds.forEach(med => {
        addMedicationRowWithData(med);
                });
            } else if (prescriptionId === "2") {
                // Sample data for prescription 2
                const sampleMeds = [
    {id: 2, name: "Amoxicillin 500mg", form: "Capsule", qty: 21, instructions: "Take 1 capsule three times daily", repeats: 0},
    {id: 4, name: "Ventolin Inhaler", form: "Inhaler", qty: 1, instructions: "Use as needed for wheezing", repeats: 0}
    ];
                
                sampleMeds.forEach(med => {
        addMedicationRowWithData(med);
                });
            }
        }

    // Helper function to add medication row with data
    function addMedicationRowWithData(medData) {
            const tbody = document.getElementById('medsTableBody');
    const rowId = Date.now();

    const row = document.createElement('tr');
    row.id = `row-${rowId}`;
    row.innerHTML = `
    <td>
        <div class="searchable-select">
            <input list="meds-list-${rowId}" id="med-input-${rowId}"
                value="${medData.name}"
                placeholder="Search or select medication..."
                oninput="filterMeds(this, ${rowId})">
                <datalist id="meds-list-${rowId}">
                    ${medications.map(med =>
                        `<option value="${med.name} (${med.schedule})" data-id="${med.id}">${med.active}</option>`
                    ).join('')}
                </datalist>
        </div>
    </td>
    <td>
        <select class="form-select" id="form-${rowId}">
            <option value="">Select form</option>
            ${medications.find(m => m.id === medData.id)?.forms.map(form =>
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
        <div class="med-details" id="details-${rowId}">
            <small><strong>Active:</strong> ${medications.find(m => m.id === medData.id)?.active || ''}</small><br>
                <small><strong>Stock:</strong> ${medications.find(m => m.id === medData.id)?.stock || ''}</small><br>
                    <small><strong>Reorder:</strong> ${medications.find(m => m.id === medData.id)?.reorder || ''}</small>
                </div>
            </td>
            <td style="text-align: center;">
                <input type="checkbox" id="repeat-check-${rowId}" onchange="toggleRepeats(${rowId})" ${medData.repeats > 0 ? 'checked' : ''}>
            </td>
            <td>
                <input type="number" id="repeats-left-${rowId}" min="0" value="${medData.repeats}" ${medData.repeats > 0 ? '' : 'disabled'} style="width: 50px;">
            </td>
            <td><button class="remove-btn" onclick="removeRow(${rowId})"><i class="fas fa-trash"></i></button></td>
            `;

            tbody.appendChild(row);
        }

            // Add medication row with searchable select
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
            <td>
                <div class="searchable-select">
                    <input list="meds-list-${rowId}" id="med-input-${rowId}"
                        placeholder="Search or select medication..."
                        oninput="filterMeds(this, ${rowId})">
                        <datalist id="meds-list-${rowId}">
                            ${medOptions}
                        </datalist>
                </div>
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
                <div class="med-details" id="details-${rowId}">
                    <!-- Details will populate here -->
                </div>
            </td>
            <td style="text-align: center;">
                <input type="checkbox" id="repeat-check-${rowId}" onchange="toggleRepeats(${rowId})">
            </td>
            <td>
                <input type="number" id="repeats-left-${rowId}" min="0" value="0" disabled style="width: 50px;">
            </td>
            <td><button class="remove-btn" onclick="removeRow(${rowId})"><i class="fas fa-trash"></i></button></td>
            `;

            tbody.appendChild(row);

            // Add event listener for when a medication is selected
            document.getElementById(`med-input-${rowId}`).addEventListener('change', function() {
                const selectedOption = document.querySelector(`#meds-list-${rowId} option[value="${this.value}"]`);
            if (selectedOption) {
                updateMedDetails(selectedOption.getAttribute('data-id'), rowId);
                }
            });
        }

            // Toggle repeats input based on checkbox
            function toggleRepeats(rowId) {
            const checkbox = document.getElementById(`repeat-check-${rowId}`);
            const repeatsInput = document.getElementById(`repeats-left-${rowId}`);
            repeatsInput.disabled = !checkbox.checked;
            if (!checkbox.checked) {
                repeatsInput.value = 0;
            }
        }

            // Filter medications as user types
            function filterMeds(input, rowId) {
            const searchTerm = input.value.toLowerCase();
            const options = document.querySelectorAll(`#meds-list-${rowId} option`);
            
            options.forEach(option => {
                const text = option.value.toLowerCase();
            if (text.includes(searchTerm)) {
                option.style.display = 'block';
                } else {
                option.style.display = 'none';
                }
            });
        }

            // Update medication details when selected
            function updateMedDetails(medId, rowId) {
            const detailsDiv = document.getElementById(`details-${rowId}`);
            const formSelect = document.getElementById(`form-${rowId}`);

            if (medId) {
                const med = medications.find(m => m.id == medId);

            if (med) {
                // Update details display
                detailsDiv.innerHTML = `
                        <small><strong>Active:</strong> ${med.active}</small><br>
                        <small><strong>Stock:</strong> ${med.stock}</small><br>
                        <small><strong>Reorder:</strong> ${med.reorder}</small>
                    `;

            // Update dosage forms
            formSelect.innerHTML = '<option value="">Select form</option>';
                    med.forms.forEach(form => {
                formSelect.innerHTML += `<option value="${form}">${form}</option>`;
                    });

            return;
                }
            }

            // Clear if no medication selected
            detailsDiv.innerHTML = '';
            formSelect.innerHTML = '<option value="">Select form</option>';
        }

            // Handle instruction selection
            function handleInstructionSelect(select, rowId) {
            const customDiv = document.getElementById(`custom-inst-${rowId}`);
            if (select.value === "Other (specify)") {
                customDiv.style.display = 'block';
            } else {
                customDiv.style.display = 'none';
            }
        }

            // Remove row
            function removeRow(rowId) {
                document.getElementById(`row-${rowId}`).remove();
        }

            // Modal functions
            function closeModal(id) {
                document.getElementById(id).style.display = 'none';
        }

            function saveCustomInstruction() {
                // Implementation would save the custom instruction
                closeModal('instructionModal');
        }

            function saveNewDoctor() {
            // In a real app, this would save the new doctor to the database
            const name = document.getElementById('doctorName').value;
            const practiceNumber = document.getElementById('practiceNumber').value;

            if (name && practiceNumber) {
                // Add to the dropdown (in a real app, this would come from the server)
                const select = document.getElementById('doctorSelect');
            const newOption = document.createElement('option');
            newOption.value = "new";
            newOption.textContent = `${name} (${practiceNumber})`;
            select.appendChild(newOption);

            // Select the new doctor
            select.value = "new";

            closeModal('doctorModal');
            } else {
                alert("Please fill in all required fields");
            }
        }

            // Add medication button
            document.getElementById('addMedBtn').addEventListener('click', addMedicationRow);

