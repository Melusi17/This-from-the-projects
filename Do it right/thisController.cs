using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.PharmacistVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    [Authorize(Policy = "Pharmacist")]
    public class PharmacistDispensingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacistDispensingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Main dispensing dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = new PharmacistDispensingDashboardVM
                {
                    PendingOrders = await _context.Orders
                        .Where(o => o.Status == "Ordered" || o.Status == "Processing")
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.ApplicationUser)
                        .Include(o => o.OrderLines)
                        .OrderBy(o => o.OrderDate)
                        .ToListAsync(),

                    ReadyForCollection = await _context.Orders
                        .Where(o => o.Status == "Ready for Collection")
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.ApplicationUser)
                        .Include(o => o.OrderLines)
                        .OrderBy(o => o.OrderDate)
                        .ToListAsync(),

                    TodayDispensed = await _context.Orders
                        .Where(o => o.Status == "Dispensed" && 
                                   o.OrderDate.Date == DateTime.Today)
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.ApplicationUser)
                        .CountAsync()
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return View(new PharmacistDispensingDashboardVM());
            }
        }

        // GET: Order details for dispensing
        public async Task<IActionResult> DispenseOrder(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.ScriptLine)
                            .ThenInclude(sl => sl.Prescriptions)
                                .ThenInclude(p => p.Doctors)
                    .Include(o => o.Pharmacist)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                // Check customer allergies for all medications in order
                var customerAllergies = await _context.Custormer_Allergies
                    .Where(ca => ca.CustomerID == order.CustomerID)
                    .Include(ca => ca.Active_Ingredient)
                    .Select(ca => ca.Active_Ingredient.Name)
                    .ToListAsync();

                var viewModel = new DispenseOrderVM
                {
                    OrderID = order.OrderID,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    CustomerName = $"{order.Customer.ApplicationUser.Name} {order.Customer.ApplicationUser.Surname}",
                    CustomerEmail = order.Customer.ApplicationUser.Email,
                    CustomerIDNumber = order.Customer.ApplicationUser.IDNumber,
                    CustomerAllergies = customerAllergies,
                    CurrentStatus = order.Status,
                    TotalAmount = decimal.Parse(order.TotalDue),
                    VAT = order.VAT,
                    OrderLines = order.OrderLines.Select(ol => new DispenseOrderLineVM
                    {
                        OrderLineID = ol.OrderLineID,
                        MedicationID = ol.MedicationID,
                        MedicationName = ol.Medications.MedicationName,
                        Quantity = ol.Quantity,
                        ItemPrice = ol.ItemPrice,
                        LineTotal = ol.ItemPrice * ol.Quantity,
                        Instructions = ol.ScriptLine?.Instructions ?? "Take as directed",
                        DoctorName = ol.ScriptLine?.Prescriptions?.Doctors != null ? 
                            $"Dr. {ol.ScriptLine.Prescriptions.Doctors.Name} {ol.ScriptLine.Prescriptions.Doctors.Surname}" : "N/A",
                        Schedule = ol.Medications.Schedule,
                        CurrentStock = ol.Medications.QuantityOnHand,
                        IsLowStock = ol.Medications.QuantityOnHand <= ol.Medications.ReOrderLevel,
                        Status = ol.Status,
                        CanDispense = ol.Status == "Ordered" || ol.Status == "Processing",
                        RejectionReason = ol.RejectionReason
                    }).ToList(),
                    AllItemsReady = order.OrderLines.All(ol => 
                        ol.Status == "Ready for Dispensing" || ol.Status == "Dispensed"),
                    AnyItemsRejected = order.OrderLines.Any(ol => ol.Status == "Rejected")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Update order line status (Dispense/Reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderLineStatus(int orderLineId, string action, string rejectionReason = "")
        {
            try
            {
                var orderLine = await _context.OrderLines
                    .Include(ol => ol.Medications)
                    .Include(ol => ol.Order)
                    .FirstOrDefaultAsync(ol => ol.OrderLineID == orderLineId);

                if (orderLine == null)
                {
                    return Json(new { success = false, message = "Order line not found" });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pharmacist = await _context.Pharmacists
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUserId);

                if (pharmacist == null)
                {
                    return Json(new { success = false, message = "Pharmacist not found" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    switch (action.ToLower())
                    {
                        case "dispense":
                            // Check stock availability
                            if (orderLine.Medications.QuantityOnHand < orderLine.Quantity)
                            {
                                return Json(new { 
                                    success = false, 
                                    message = $"Insufficient stock. Available: {orderLine.Medications.QuantityOnHand}, Required: {orderLine.Quantity}" 
                                });
                            }

                            // Update stock
                            orderLine.Medications.QuantityOnHand -= orderLine.Quantity;
                            
                            // Update order line status
                            orderLine.Status = "Dispensed";
                            
                            // Set dispensing pharmacist if not set
                            if (orderLine.Order.PharmacistID == null)
                            {
                                orderLine.Order.PharmacistID = pharmacist.PharmacistID;
                            }

                            break;

                        case "ready":
                            orderLine.Status = "Ready for Dispensing";
                            break;

                        case "reject":
                            if (string.IsNullOrWhiteSpace(rejectionReason))
                            {
                                return Json(new { success = false, message = "Rejection reason is required" });
                            }

                            orderLine.Status = "Rejected";
                            orderLine.RejectionReason = rejectionReason;
                            break;

                        case "processing":
                            orderLine.Status = "Processing";
                            break;

                        default:
                            return Json(new { success = false, message = "Invalid action" });
                    }

                    await _context.SaveChangesAsync();

                    // Check if all order lines are now dispensed
                    var order = await _context.Orders
                        .Include(o => o.OrderLines)
                        .FirstOrDefaultAsync(o => o.OrderID == orderLine.OrderID);

                    if (order != null)
                    {
                        UpdateOverallOrderStatus(order);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Json(new { 
                        success = true, 
                        message = $"Order line {action} successfully",
                        newStatus = orderLine.Status,
                        orderStatus = order?.Status
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Error updating order line: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Dispense entire order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispenseCompleteOrder(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pharmacist = await _context.Pharmacists
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUserId);

                if (pharmacist == null)
                {
                    TempData["ErrorMessage"] = "Pharmacist not found";
                    return RedirectToAction(nameof(DispenseOrder), new { id = orderId });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Check stock for all order lines
                    var insufficientStockLines = order.OrderLines
                        .Where(ol => ol.Status != "Dispensed" && ol.Status != "Rejected")
                        .Where(ol => ol.Medications.QuantityOnHand < ol.Quantity)
                        .ToList();

                    if (insufficientStockLines.Any())
                    {
                        var medicationNames = insufficientStockLines
                            .Select(ol => ol.Medications.MedicationName)
                            .ToList();
                        
                        TempData["ErrorMessage"] = $"Insufficient stock for: {string.Join(", ", medicationNames)}";
                        return RedirectToAction(nameof(DispenseOrder), new { id = orderId });
                    }

                    // Dispense all non-rejected order lines
                    foreach (var orderLine in order.OrderLines.Where(ol => 
                        ol.Status != "Dispensed" && ol.Status != "Rejected"))
                    {
                        orderLine.Medications.QuantityOnHand -= orderLine.Quantity;
                        orderLine.Status = "Dispensed";
                    }

                    // Update order status and set pharmacist
                    order.Status = "Dispensed";
                    order.PharmacistID = pharmacist.PharmacistID;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Order {order.OrderNumber} dispensed successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Error dispensing order: {ex.Message}";
                    return RedirectToAction(nameof(DispenseOrder), new { id = orderId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Order history and reporting
        public async Task<IActionResult> DispensingHistory(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .Include(o => o.Pharmacist)
                        .ThenInclude(p => p.ApplicationUser)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .Where(o => o.Status == "Dispensed");

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var viewModel = new DispensingHistoryVM
                {
                    Orders = orders,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalDispensed = orders.Count,
                    TotalRevenue = orders.Sum(o => decimal.Parse(o.TotalDue))
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading history: {ex.Message}";
                return View(new DispensingHistoryVM());
            }
        }

        // API: Get low stock medications
        [HttpGet]
        public async Task<JsonResult> GetLowStockMedications()
        {
            try
            {
                var lowStockMedications = await _context.Medications
                    .Where(m => m.QuantityOnHand <= m.ReOrderLevel)
                    .Select(m => new
                    {
                        m.MedcationID,
                        m.MedicationName,
                        m.QuantityOnHand,
                        m.ReOrderLevel,
                        Urgency = m.QuantityOnHand == 0 ? "Out of Stock" : 
                                 m.QuantityOnHand <= 5 ? "Critical" : "Low"
                    })
                    .ToListAsync();

                return Json(new { success = true, medications = lowStockMedications });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to update overall order status
        private void UpdateOverallOrderStatus(Order order)
        {
            var orderLines = order.OrderLines;

            if (orderLines.All(ol => ol.Status == "Dispensed"))
            {
                order.Status = "Dispensed";
            }
            else if (orderLines.All(ol => ol.Status == "Rejected"))
            {
                order.Status = "Rejected";
            }
            else if (orderLines.Any(ol => ol.Status == "Ready for Dispensing") && 
                     orderLines.Any(ol => ol.Status != "Dispensed" && ol.Status != "Rejected"))
            {
                order.Status = "Ready for Collection";
            }
            else if (orderLines.Any(ol => ol.Status == "Processing" || ol.Status == "Ready for Dispensing"))
            {
                order.Status = "Processing";
            }
            else
            {
                order.Status = "Ordered";
            }
        }
    }
}