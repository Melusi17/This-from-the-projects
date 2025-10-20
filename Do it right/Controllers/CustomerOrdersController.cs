using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Customer")]
    public class CustomerOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to get current user ID
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // DEBUG ACTION - Check what's actually in the database
        public async Task<IActionResult> Debug()
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Content("No user logged in");
            }

            var debugInfo = $"=== DEBUG INFO for User: {currentUserId} ===\n\n";

            // Check prescriptions for this user
            var userPrescriptions = await _context.Prescriptions
                .Where(p => p.ApplicationUserId == currentUserId)
                .ToListAsync();

            debugInfo += $"PRESCRIPTIONS FOUND: {userPrescriptions.Count}\n";
            foreach (var p in userPrescriptions)
            {
                debugInfo += $"  Prescription {p.PrescriptionID}: Status = '{p.Status}' Date = {p.DateIssued}\n";
            }

            debugInfo += "\n";

            // Check scriptlines for this user
            var userScriptLines = await _context.ScriptLines
                .Include(sl => sl.Prescriptions)
                .Include(sl => sl.Medications)
                .Where(sl => sl.Prescriptions.ApplicationUserId == currentUserId)
                .ToListAsync();

            debugInfo += $"SCRIPTLINES FOUND: {userScriptLines.Count}\n";
            foreach (var sl in userScriptLines)
            {
                debugInfo += $"  ScriptLine {sl.ScriptLineID}: Status = '{sl.Status}' RepeatsLeft = {sl.RepeatsLeft} Medication = {sl.Medications?.MedicationName} Prescription = {sl.PrescriptionID}\n";
            }

            debugInfo += "\n";

            // Check what the current query returns (UPDATED - removed repeats requirement)
            var availableScriptLines = await _context.ScriptLines
                .Include(sl => sl.Medications)
                .Include(sl => sl.Prescriptions)
                    .ThenInclude(p => p.Doctors)
                .Where(sl => sl.Prescriptions.ApplicationUserId == currentUserId &&
                            sl.Status == "Approved") // ← REMOVED: && sl.RepeatsLeft > 0
                .ToListAsync();

            debugInfo += $"AVAILABLE FOR ORDERING: {availableScriptLines.Count}\n";
            foreach (var sl in availableScriptLines)
            {
                debugInfo += $"  ScriptLine {sl.ScriptLineID}: {sl.Medications?.MedicationName} - Status: {sl.Status}, RepeatsLeft: {sl.RepeatsLeft}\n";
            }

            return Content(debugInfo);
        }

        // GET: Display available medications from approved scriptlines (UPDATED)
        public async Task<IActionResult> PlaceOrder()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // UPDATED QUERY: Removed RepeatsLeft > 0 requirement
                var availableScriptLines = await _context.ScriptLines
                    .Include(sl => sl.Medications)
                    .Include(sl => sl.Prescriptions)
                        .ThenInclude(p => p.Doctors)
                    .Where(sl => sl.Prescriptions.ApplicationUserId == currentUserId &&
                                sl.Status == "Approved") // ← Only status check now
                    .Select(sl => new CustomerOrderVM
                    {
                        ScriptLineID = sl.ScriptLineID,
                        PrescriptionID = sl.PrescriptionID,
                        MedicationID = sl.MedicationID,
                        Quantity = sl.Quantity,
                        TotalRepeats = sl.Repeats,
                        RepeatsLeft = sl.RepeatsLeft,
                        Instructions = sl.Instructions ?? string.Empty,
                        MedicationName = sl.Medications.MedicationName ?? string.Empty,
                        CurrentPrice = sl.Medications.CurrentPrice,
                        Schedule = sl.Medications.Schedule ?? string.Empty,
                        DoctorName = sl.Prescriptions.Doctors.Name ?? string.Empty,
                        DoctorSurname = sl.Prescriptions.Doctors.Surname ?? string.Empty
                    })
                    .ToListAsync();

                return View(availableScriptLines);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading medications. Please try again.";
                return View(new List<CustomerOrderVM>());
            }
        }

        // POST: Create order from selected medications (UPDATED validation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderFormVM model)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId) || model.SelectedScriptLineIds == null || !model.SelectedScriptLineIds.Any())
            {
                TempData["Error"] = "Please select at least one medication to order.";
                return RedirectToAction("PlaceOrder");
            }

            // UPDATED VALIDATION: Removed RepeatsLeft check for initial order
            var validScriptLines = await _context.ScriptLines
                .Include(sl => sl.Medications)
                .Include(sl => sl.Prescriptions)
                .Where(sl => model.SelectedScriptLineIds.Contains(sl.ScriptLineID) &&
                            sl.Prescriptions.ApplicationUserId == currentUserId &&
                            sl.Status == "Approved") // ← Only status check for initial order
                .ToListAsync();

            if (!validScriptLines.Any())
            {
                TempData["Error"] = "Selected medications are no longer available.";
                return RedirectToAction("PlaceOrder");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get customer ID
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == currentUserId);

                if (customer == null)
                {
                    TempData["Error"] = "Customer profile not found.";
                    return RedirectToAction("PlaceOrder");
                }

                // Create order
                var order = new Order
                {
                    CustomerID = customer.CustormerID,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    VAT = 15 // 15% VAT
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Save to get OrderID

                decimal subtotal = 0;

                // Create order lines and update script lines
                foreach (var scriptLine in validScriptLines)
                {
                    // IMPORTANT: Only decrement repeats if it actually has repeats
                    // For one-time medications (Repeats = 0), don't touch RepeatsLeft
                    if (scriptLine.Repeats > 0 && scriptLine.RepeatsLeft > 0)
                    {
                        scriptLine.RepeatsLeft--;
                    }

                    // Create order line
                    var orderLine = new OrderLine
                    {
                        OrderID = order.OrderID,
                        ScriptLineID = scriptLine.ScriptLineID,
                        MedicationID = scriptLine.MedicationID,
                        Quantity = scriptLine.Quantity,
                        ItemPrice = (int)scriptLine.Medications.CurrentPrice,
                        Status = "Ordered" // Set initial status
                    };

                    _context.OrderLines.Add(orderLine);
                    subtotal += scriptLine.Medications.CurrentPrice * scriptLine.Quantity;
                }

                order.TotalDue = (subtotal + (subtotal * 0.15m)).ToString("F2");
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while placing your order. Please try again.";
                return RedirectToAction("PlaceOrder");
            }
        }

        // Order confirmation page (unchanged)
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var currentUserId = GetCurrentUserId();

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Medications)
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.ScriptLine)
                        .ThenInclude(sl => sl.Prescriptions)
                            .ThenInclude(p => p.Doctors)
                .Include(o => o.Customer)
                    .ThenInclude(c => c.ApplicationUser)
                .Where(o => o.OrderID == orderId &&
                           o.Customer.ApplicationUserId == currentUserId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderConfirmationVM
            {
                OrderID = order.OrderID,
                OrderNumber = $"ORD{order.OrderID:D6}",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = decimal.Parse(order.TotalDue),
                CustomerName = $"{order.Customer.ApplicationUser.Name} {order.Customer.ApplicationUser.Surname}",
                CustomerEmail = order.Customer.ApplicationUser.Email ?? string.Empty,
                OrderLines = order.OrderLines.Select(ol => new OrderLineDetailVM
                {
                    MedicationName = ol.Medications.MedicationName ?? string.Empty,
                    Quantity = ol.Quantity,
                    Price = ol.ItemPrice,
                    DoctorName = $"Dr. {ol.ScriptLine.Prescriptions.Doctors.Name} {ol.ScriptLine.Prescriptions.Doctors.Surname}",
                    Instructions = ol.ScriptLine.Instructions ?? string.Empty
                }).ToList()
            };

            return View(viewModel);
        }

        // Order history (unchanged)
        public async Task<IActionResult> OrderHistory()
        {
            var currentUserId = GetCurrentUserId();

            var orders = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.ApplicationUser)
                .Where(o => o.Customer.ApplicationUserId == currentUserId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderConfirmationVM
                {
                    OrderID = o.OrderID,
                    OrderNumber = $"ORD{o.OrderID:D6}",
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = decimal.Parse(o.TotalDue),
                    CustomerName = $"{o.Customer.ApplicationUser.Name} {o.Customer.ApplicationUser.Surname}",
                    CustomerEmail = o.Customer.ApplicationUser.Email ?? string.Empty
                })
                .ToListAsync();

            return View(orders);
        }

        // NEW: Method specifically for refills (only shows medications with repeats)
        public async Task<IActionResult> RequestRefill()
        {
            var currentUserId = GetCurrentUserId();

            var refillScriptLines = await _context.ScriptLines
                .Include(sl => sl.Medications)
                .Include(sl => sl.Prescriptions)
                    .ThenInclude(p => p.Doctors)
                .Where(sl => sl.Prescriptions.ApplicationUserId == currentUserId &&
                            sl.Status == "Approved" &&
                            sl.RepeatsLeft > 0) // ← This is where repeats matter
                .Select(sl => new CustomerOrderVM
                {
                    ScriptLineID = sl.ScriptLineID,
                    PrescriptionID = sl.PrescriptionID,
                    MedicationID = sl.MedicationID,
                    Quantity = sl.Quantity,
                    TotalRepeats = sl.Repeats,
                    RepeatsLeft = sl.RepeatsLeft,
                    Instructions = sl.Instructions ?? string.Empty,
                    MedicationName = sl.Medications.MedicationName ?? string.Empty,
                    CurrentPrice = sl.Medications.CurrentPrice,
                    Schedule = sl.Medications.Schedule ?? string.Empty,
                    DoctorName = sl.Prescriptions.Doctors.Name ?? string.Empty,
                    DoctorSurname = sl.Prescriptions.Doctors.Surname ?? string.Empty
                })
                .ToListAsync();

            return View(refillScriptLines);
        }
    }
}