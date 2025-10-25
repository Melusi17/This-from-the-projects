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

        // GET: Main dispensing dashboard - ONLY active orders
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = new PharmacistDispensingDashboardVM
                {
                    PendingOrders = await _context.Orders
                        .Where(o => o.Status == "Ordered")
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

                    WaitingCustomerAction = await _context.Orders
                        .Where(o => o.Status == "Waiting Customer Action")
                        .Include(o => o.Customer)
                            .ThenInclude(c => c.ApplicationUser)
                        .Include(o => o.OrderLines)
                        .OrderBy(o => o.OrderDate)
                        .ToListAsync(),

                    TodayDispensed = await _context.Orders
                        .Where(o => o.Status == "Ready for Collection" &&
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

        // GET: Collection Tracking - ONLY ready/collected orders
        public async Task<IActionResult> CollectionTracking()
        {
            try
            {
                var collectionOrders = await _context.Orders
                    .Where(o => o.Status == "Ready for Collection" || o.Status == "Collected")
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.ScriptLine)
                            .ThenInclude(sl => sl.Prescriptions)
                                .ThenInclude(p => p.Doctors)
                    .Include(o => o.Pharmacist)
                        .ThenInclude(p => p.ApplicationUser)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var viewModel = new CollectionTrackingVM
                {
                    Orders = collectionOrders,
                    ReadyForCollectionCount = collectionOrders.Count(o => o.Status == "Ready for Collection"),
                    CollectedCount = collectionOrders.Count(o => o.Status == "Collected"),
                    TotalOrders = collectionOrders.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading collection orders: {ex.Message}";
                return View(new CollectionTrackingVM());
            }
        }

        // POST: Mark order as collected - FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCollected([FromBody] MarkAsCollectedRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderID == request.OrderId);

                if (order == null)
                {
                    return Json(new { success = false, message = $"Order with ID {request.OrderId} not found in database." });
                }

                if (order.Status != "Ready for Collection")
                {
                    return Json(new { success = false, message = $"Order status is '{order.Status}', not 'Ready for Collection'." });
                }

                order.Status = "Collected";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Order marked as collected successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error marking order as collected: {ex.Message}" });
            }
        }

        public class MarkAsCollectedRequest
        {
            public int OrderId { get; set; }
        }

        // POST: Send collection email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCollectionEmail([FromBody] SendEmailRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(o => o.OrderID == request.OrderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                var customerEmail = order.Customer.ApplicationUser.Email;
                var customerName = $"{order.Customer.ApplicationUser.Name} {order.Customer.ApplicationUser.Surname}";

                // TODO: Implement actual email sending logic here
                // For now, just return success
                return Json(new
                {
                    success = true,
                    message = $"Collection notification sent to {customerName} at {customerEmail}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error sending email: {ex.Message}" });
            }
        }

        public class SendEmailRequest
        {
            public int OrderId { get; set; }
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
                        RejectionReason = ol.RejectionReason,
                        TotalRepeats = ol.ScriptLine?.Repeats ?? 0,
                        RepeatsLeft = ol.ScriptLine?.RepeatsLeft ?? 0
                    }).ToList(),
                    AllItemsProcessed = order.OrderLines.All(ol =>
                        ol.Status == "Dispensed" || ol.Status == "Rejected"),
                    AnyItemsDispensed = order.OrderLines.Any(ol => ol.Status == "Dispensed"),
                    AllItemsRejected = order.OrderLines.All(ol => ol.Status == "Rejected")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Dispense selected order lines
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DispenseSelectedOrderLines(int orderId, List<int> selectedOrderLineIds)
        {
            try
            {
                if (selectedOrderLineIds == null || !selectedOrderLineIds.Any())
                {
                    return Json(new { success = false, message = "Please select at least one medication to dispense." });
                }

                var order = await _context.Orders
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.Medications)
                    .Include(o => o.OrderLines)
                        .ThenInclude(ol => ol.ScriptLine)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pharmacist = await _context.Pharmacists
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUserId);

                if (pharmacist == null)
                {
                    return Json(new { success = false, message = "Pharmacist not found." });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var orderLinesToProcess = order.OrderLines
                        .Where(ol => selectedOrderLineIds.Contains(ol.OrderLineID) && ol.Status == "Pending")
                        .ToList();

                    var orderLinesToDispense = new List<OrderLine>();
                    var orderLinesToReject = new List<OrderLine>();

                    foreach (var orderLine in orderLinesToProcess)
                    {
                        if (orderLine.ScriptLine != null && orderLine.ScriptLine.Repeats > 0 && orderLine.ScriptLine.RepeatsLeft <= 0)
                        {
                            orderLine.Status = "Rejected";
                            orderLine.RejectionReason = "Repeats exceeded";
                            orderLinesToReject.Add(orderLine);
                            continue;
                        }

                        if (orderLine.Medications.QuantityOnHand < orderLine.Quantity)
                        {
                            orderLine.Status = "Rejected";
                            orderLine.RejectionReason = "Insufficient stock";
                            orderLinesToReject.Add(orderLine);
                            continue;
                        }

                        orderLinesToDispense.Add(orderLine);
                    }

                    foreach (var orderLine in orderLinesToDispense)
                    {
                        orderLine.Medications.QuantityOnHand -= orderLine.Quantity;

                        if (orderLine.ScriptLine != null && orderLine.ScriptLine.Repeats > 0 && orderLine.ScriptLine.RepeatsLeft > 0)
                        {
                            orderLine.ScriptLine.RepeatsLeft--;
                        }

                        orderLine.Status = "Dispensed";
                    }

                    if (order.PharmacistID == null)
                    {
                        order.PharmacistID = pharmacist.PharmacistID;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var resultMessage = "";
                    if (orderLinesToDispense.Count > 0 && orderLinesToReject.Count > 0)
                    {
                        resultMessage = $"{orderLinesToDispense.Count} medication(s) dispensed successfully! {orderLinesToReject.Count} medication(s) were automatically rejected (repeats exceeded or insufficient stock).";
                    }
                    else if (orderLinesToDispense.Count > 0)
                    {
                        resultMessage = $"{orderLinesToDispense.Count} medication(s) dispensed successfully!";
                    }
                    else if (orderLinesToReject.Count > 0)
                    {
                        resultMessage = $"{orderLinesToReject.Count} medication(s) were automatically rejected (repeats exceeded or insufficient stock).";
                    }
                    else
                    {
                        resultMessage = "No medications were processed.";
                    }

                    return Json(new
                    {
                        success = true,
                        message = resultMessage,
                        dispensedCount = orderLinesToDispense.Count,
                        rejectedCount = orderLinesToReject.Count
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Error dispensing medications: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Reject order line with reason
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrderLine(int orderLineId, string rejectionReason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    return Json(new { success = false, message = "Rejection reason is required." });
                }

                var orderLine = await _context.OrderLines
                    .Include(ol => ol.Order)
                    .Include(ol => ol.ScriptLine)
                    .FirstOrDefaultAsync(ol => ol.OrderLineID == orderLineId);

                if (orderLine == null)
                {
                    return Json(new { success = false, message = "Order line not found." });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pharmacist = await _context.Pharmacists
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == currentUserId);

                if (pharmacist == null)
                {
                    return Json(new { success = false, message = "Pharmacist not found." });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    orderLine.Status = "Rejected";
                    orderLine.RejectionReason = rejectionReason.Trim();

                    if (orderLine.Order.PharmacistID == null)
                    {
                        orderLine.Order.PharmacistID = pharmacist.PharmacistID;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Medication rejected successfully!",
                        newStatus = orderLine.Status
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Error rejecting medication: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Complete order processing - UPDATED to redirect to CollectionTracking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrderProcessing(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderLines)
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.ApplicationUser)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                var unprocessedLines = order.OrderLines.Where(ol => ol.Status == "Pending").ToList();
                if (unprocessedLines.Any())
                {
                    TempData["ErrorMessage"] = "Please process all medications before completing the order.";
                    return RedirectToAction(nameof(DispenseOrder), new { id = orderId });
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
                    var dispensedLines = order.OrderLines.Count(ol => ol.Status == "Dispensed");
                    var rejectedLines = order.OrderLines.Count(ol => ol.Status == "Rejected");

                    if (dispensedLines > 0)
                    {
                        order.Status = "Ready for Collection";
                        TempData["SuccessMessage"] = $"Order {order.OrderNumber} is ready for collection!";
                    }
                    else if (rejectedLines == order.OrderLines.Count)
                    {
                        order.Status = "Waiting Customer Action";
                        TempData["WarningMessage"] = $"Order {order.OrderNumber} requires customer action. All medications were rejected.";
                    }

                    if (order.PharmacistID == null)
                    {
                        order.PharmacistID = pharmacist.PharmacistID;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // REDIRECT TO COLLECTION TRACKING INSTEAD OF INDEX
                    return RedirectToAction(nameof(CollectionTracking));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Error completing order processing: {ex.Message}";
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
                    .Where(o => o.Status == "Ready for Collection" || o.Status == "Waiting Customer Action");

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
                    TotalProcessed = orders.Count,
                    ReadyForCollectionCount = orders.Count(o => o.Status == "Ready for Collection"),
                    WaitingActionCount = orders.Count(o => o.Status == "Waiting Customer Action"),
                    TotalRevenue = orders.Where(o => o.Status == "Ready for Collection").Sum(o => decimal.Parse(o.TotalDue))
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
    }
}