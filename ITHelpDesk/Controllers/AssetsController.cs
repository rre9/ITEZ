using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Controllers;

[Authorize(Roles = "Admin,Support")]
public class AssetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AssetsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // Dashboard - عرض ملخص جميع الأصول
    [Authorize(Roles = "Admin,Support,Security")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var accessPointsCount = await _context.AccessPoints.CountAsync();
            var computersCount = await _context.Computers.CountAsync();
            var serversCount = await _context.Servers.CountAsync();
            var virtualHostsCount = await _context.VirtualHosts.CountAsync();
            var virtualMachinesCount = await _context.VirtualMachines.CountAsync();
            var workstationsCount = await _context.Workstations.CountAsync();
            var smartphonesCount = await _context.Smartphones.CountAsync();
            var tabletsCount = await _context.Tablets.CountAsync();
            var printersCount = await _context.Printers.CountAsync();
            var routersCount = await _context.Routers.CountAsync();
            var ciscoRoutersCount = await _context.CiscoRouters.CountAsync();
            var switchesCount = await _context.Switches.CountAsync();
            var ciscoCatosSwitchesCount = await _context.CiscoCatosSwitches.CountAsync();
            var ciscoSwitchesCount = await _context.CiscoSwitches.CountAsync();

            var viewModel = new
            {
                AccessPoints = accessPointsCount,
                Computers = computersCount,
                Servers = serversCount,
                VirtualHosts = virtualHostsCount,
                VirtualMachines = virtualMachinesCount,
                Workstations = workstationsCount,
                Smartphones = smartphonesCount,
                Tablets = tabletsCount,
                Printers = printersCount,
                Routers = routersCount,
                CiscoRouters = ciscoRoutersCount,
                Switches = switchesCount,
                CiscoCatosSwitches = ciscoCatosSwitchesCount,
                CiscoSwitches = ciscoSwitchesCount
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assets dashboard");
            TempData["Toast"] = "حدث خطأ في تحميل لوحة التحكم";
            return RedirectToAction("Index", "Home");
        }
    }

    // #region Access Points

    // GET: Assets/AccessPoints
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> AccessPoints()
    {
        var accessPoints = await _context.AccessPoints
            .Include(a => a.Product)
            .Include(a => a.Vendor)
            .Include(a => a.AssetState)
            .Include(a => a.NetworkDetails)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View(accessPoints);
    }

    // GET: Assets/CreateAccessPoint
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> CreateAccessPoint()
    {
        ViewBag.Products = await _context.Products
            .Where(p => p.ProductType == "Access Point")
            .ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = await _context.AssetStates.ToListAsync();

        return View();
    }

    // POST: Assets/CreateAccessPoint
    [Authorize(Roles = "Admin,Support")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccessPoint(AccessPoint model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = await _context.AssetStates.ToListAsync();
            return View(model);
        }

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Create AssetState if not selected
            if (!model.AssetStateId.HasValue)
            {
                var assetState = new AssetState
                {
                    Status = AssetStatusEnum.InStore,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AssetStates.Add(assetState);
                await _context.SaveChangesAsync();
                model.AssetStateId = assetState.Id;
            }

            model.CreatedById = currentUser?.Id;
            model.CreatedAt = DateTime.UtcNow;

            _context.AccessPoints.Add(model);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ تم إضافة نقطة الوصول '{model.Name}' بنجاح";
            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access point");
            ModelState.AddModelError("", "حدث خطأ في حفظ البيانات");
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = await _context.AssetStates.ToListAsync();
            return View(model);
        }
    }

    // GET: Assets/EditAccessPoint/5
    [Authorize(Roles = "Admin,Support")]
    public async Task<IActionResult> EditAccessPoint(int id)
    {
        var accessPoint = await _context.AccessPoints.FindAsync(id);
        if (accessPoint == null)
            return NotFound();

        ViewBag.Products = await _context.Products
            .Where(p => p.ProductType == "Access Point")
            .ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = await _context.AssetStates.ToListAsync();

        return View(accessPoint);
    }

    // POST: Assets/EditAccessPoint/5
    [Authorize(Roles = "Admin,Support")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccessPoint(int id, AccessPoint model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = await _context.AssetStates.ToListAsync();
            return View(model);
        }

        try
        {
            model.UpdatedAt = DateTime.UtcNow;
            _context.Update(model);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ تم تحديث نقطة الوصول '{model.Name}' بنجاح";
            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access point");
            ModelState.AddModelError("", "حدث خطأ في حفظ البيانات");
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = await _context.AssetStates.ToListAsync();
            return View(model);
        }
    }

    // POST: Assets/DeleteAccessPoint/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccessPoint(int id)
    {
        var accessPoint = await _context.AccessPoints.FindAsync(id);
        if (accessPoint == null)
            return NotFound();

        try
        {
            _context.AccessPoints.Remove(accessPoint);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ تم حذف نقطة الوصول بنجاح";
            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting access point");
            TempData["Toast"] = "❌ حدث خطأ في حذف البيانات";
            return RedirectToAction(nameof(AccessPoints));
        }
    }

    // #endregion

    // #region Products Management

    // API: Get products by type
    [Authorize(Roles = "Admin,Support")]
    [HttpGet]
    public async Task<IActionResult> GetProductsByType(string type)
    {
        var products = await _context.Products
            .Where(p => p.ProductType == type)
            .Select(p => new { id = p.Id, name = p.ProductName, manufacturer = p.Manufacturer })
            .ToListAsync();

        return Json(products);
    }

    // POST: Assets/CreateProduct
    [Authorize(Roles = "Admin,Support")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(string productType, string productName, string manufacturer, string? partNo, decimal cost)
    {
        try
        {
            var product = new Product
            {
                ProductType = productType,
                ProductName = productName,
                Manufacturer = manufacturer,
                PartNo = partNo,
                Cost = cost,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = product.Id, name = product.ProductName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Json(new { success = false, message = "حدث خطأ في إنشاء المنتج" });
        }
    }

    // #endregion

    // #region Vendors Management

    // API: Get all vendors
    [Authorize(Roles = "Admin,Support")]
    [HttpGet]
    public async Task<IActionResult> GetVendors()
    {
        var vendors = await _context.Vendors
            .Select(v => new { id = v.Id, name = v.VendorName })
            .ToListAsync();

        return Json(vendors);
    }

    // POST: Assets/CreateVendor
    [Authorize(Roles = "Admin,Support")]
    [HttpPost]
    public async Task<IActionResult> CreateVendor(string vendorName, string? phone, string? email)
    {
        try
        {
            var vendor = new Vendor
            {
                VendorName = vendorName,
                Currency = "SR",
                PhoneNo = phone,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = vendor.Id, name = vendor.VendorName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vendor");
            return Json(new { success = false, message = "حدث خطأ في إنشاء البائع" });
        }
    }

    // #endregion
}
