 // Track which medication row is being rejected
    let currentRejectionRow = null;

    // Show rejection modal
    function showRejectionModal(button) {
        currentRejectionRow = button.closest('tr');
    document.getElementById('rejectionModal').style.display = 'flex';
        }

    // Close modal
    function closeModal() {
        document.getElementById('rejectionModal').style.display = 'none';
            // Clear selection
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
    const statusCell = currentRejectionRow.querySelector('td:nth-child(8)');
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
            // Add dispensed class
            row.classList.add('dispensed-row');

            // Update status
            const statusCell = row.querySelector('td:nth-child(8)');
            statusCell.innerHTML = '<span class="badge bg-success">Dispensed</span>';

            // Remove checkbox and action buttons
            checkbox.remove();
            const actionButtons = row.querySelectorAll('.action-btn');
            actionButtons.forEach(btn => btn.remove());
        });
            }
        }

    // Show/hide alerts based on medication rows
    document.addEventListener('DOMContentLoaded', function() {
            // Check for allergy warnings
            if (document.querySelector('.has-allergy')) {
        document.querySelector('.allergy-alert').style.display = 'block';
            } else {
        document.querySelector('.allergy-alert').style.display = 'none';
            }

    // Check for low stock
    if (document.querySelector('.low-stock')) {
        document.querySelector('.stock-alert').style.display = 'block';
            } else {
        document.querySelector('.stock-alert').style.display = 'none';
            }
        });
