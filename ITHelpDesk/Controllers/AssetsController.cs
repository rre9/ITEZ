using ITHelpDesk.Data;
using ITHelpDesk.Models;
using ITHelpDesk.Models.Assets;
using ITHelpDesk.ViewModels.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelpDesk.Controllers;

[Authorize] // allow any authenticated user to access assets to avoid Access Denied during navigation
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
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Access Points
            var accessPointsTotal = await _context.AccessPoints.CountAsync();
            var accessPointsInUse = await _context.AccessPoints.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var accessPointsInStore = await _context.AccessPoints.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var accessPointsInRepair = await _context.AccessPoints.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var accessPointsOthers = await _context.AccessPoints.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Computers
            var computersTotal = await _context.Computers.CountAsync();
            var computersInUse = await _context.Computers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var computersInStore = await _context.Computers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var computersInRepair = await _context.Computers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var computersOthers = await _context.Computers.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Servers
            var serversTotal = await _context.Servers.CountAsync();
            var serversInUse = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var serversInStore = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var serversInRepair = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var serversOthers = await _context.Servers.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Smartphones
            var smartphonesTotal = await _context.Smartphones.CountAsync();
            var smartphonesInUse = await _context.Smartphones.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var smartphonesInStore = await _context.Smartphones.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var smartphonesInRepair = await _context.Smartphones.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var smartphonesOthers = await _context.Smartphones.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Printers
            var printersTotal = await _context.Printers.CountAsync();
            var printersInUse = await _context.Printers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var printersInStore = await _context.Printers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var printersInRepair = await _context.Printers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var printersOthers = await _context.Printers.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Routers (combined)
            var routersTotal = await _context.Routers.CountAsync() + await _context.CiscoRouters.CountAsync();
            var routersInUse = await _context.Routers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse) +
                               await _context.CiscoRouters.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var routersInStore = await _context.Routers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore) +
                                 await _context.CiscoRouters.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var routersInRepair = await _context.Routers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair) +
                                  await _context.CiscoRouters.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var routersOthers = await _context.Routers.CountAsync(a => a.AssetState != null && 
                                    (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed)) +
                                await _context.CiscoRouters.CountAsync(a => a.AssetState != null && 
                                    (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Switches (combined - all types)
            var switchesTotal = await _context.Switches.CountAsync() + 
                                await _context.CiscoSwitches.CountAsync() + 
                                await _context.CiscoCatosSwitches.CountAsync();
            var switchesInUse = await _context.Switches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse) +
                                await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse) +
                                await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var switchesInStore = await _context.Switches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore) +
                                  await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore) +
                                  await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var switchesInRepair = await _context.Switches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair) +
                                   await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair) +
                                   await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var switchesOthers = await _context.Switches.CountAsync(a => a.AssetState != null && 
                                     (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed)) +
                                 await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && 
                                     (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed)) +
                                 await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && 
                                     (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Cisco Catos Switches
            var ciscoCatosSwitchesTotal = await _context.CiscoCatosSwitches.CountAsync();
            var ciscoCatosSwitchesInUse = await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var ciscoCatosSwitchesInStore = await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var ciscoCatosSwitchesInRepair = await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var ciscoCatosSwitchesOthers = await _context.CiscoCatosSwitches.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Cisco Switches
            var ciscoSwitchesTotal = await _context.CiscoSwitches.CountAsync();
            var ciscoSwitchesInUse = await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var ciscoSwitchesInStore = await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var ciscoSwitchesInRepair = await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var ciscoSwitchesOthers = await _context.CiscoSwitches.CountAsync(a => a.AssetState != null && 
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            var viewModel = new
            {
                AccessPoints = new
                {
                    Total = accessPointsTotal,
                    InUse = accessPointsInUse,
                    InStore = accessPointsInStore,
                    InRepair = accessPointsInRepair,
                    Others = accessPointsOthers
                },
                Computers = new
                {
                    Total = computersTotal,
                    InUse = computersInUse,
                    InStore = computersInStore,
                    InRepair = computersInRepair,
                    Others = computersOthers
                },
                Servers = new
                {
                    Total = serversTotal,
                    InUse = serversInUse,
                    InStore = serversInStore,
                    InRepair = serversInRepair,
                    Others = serversOthers
                },
                Smartphones = new
                {
                    Total = smartphonesTotal,
                    InUse = smartphonesInUse,
                    InStore = smartphonesInStore,
                    InRepair = smartphonesInRepair,
                    Others = smartphonesOthers
                },
                Printers = new
                {
                    Total = printersTotal,
                    InUse = printersInUse,
                    InStore = printersInStore,
                    InRepair = printersInRepair,
                    Others = printersOthers
                },
                Routers = new
                {
                    Total = routersTotal,
                    InUse = routersInUse,
                    InStore = routersInStore,
                    InRepair = routersInRepair,
                    Others = routersOthers
                },
                Switches = new
                {
                    Total = switchesTotal,
                    InUse = switchesInUse,
                    InStore = switchesInStore,
                    InRepair = switchesInRepair,
                    Others = switchesOthers
                },
                CiscoCatosSwitches = new
                {
                    Total = ciscoCatosSwitchesTotal,
                    InUse = ciscoCatosSwitchesInUse,
                    InStore = ciscoCatosSwitchesInStore,
                    InRepair = ciscoCatosSwitchesInRepair,
                    Others = ciscoCatosSwitchesOthers
                },
                CiscoSwitches = new
                {
                    Total = ciscoSwitchesTotal,
                    InUse = ciscoSwitchesInUse,
                    InStore = ciscoSwitchesInStore,
                    InRepair = ciscoSwitchesInRepair,
                    Others = ciscoSwitchesOthers
                }
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
    [Authorize(Roles = "Admin,Support,IT")]
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
    [Authorize(Roles = "Admin,Support,IT")]
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
    [Authorize(Roles = "Admin,Support,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccessPoint(AccessPointCreateViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View(viewModel);
        }

        try
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // 1. Create AssetState
            var assetState = new AssetState
            {
                Status = viewModel.AssetStatus,
                AssociatedTo = viewModel.AssociatedTo,
                Site = viewModel.Site,
                UserId = viewModel.UserId,
                Department = viewModel.Department,
                StateComments = viewModel.StateComments,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetStates.Add(assetState);
            await _context.SaveChangesAsync();

            // 2. Create NetworkDetails (if any network field is provided)
            NetworkDetails? networkDetails = null;
            if (!string.IsNullOrWhiteSpace(viewModel.IPAddress) ||
                !string.IsNullOrWhiteSpace(viewModel.MACAddress) ||
                !string.IsNullOrWhiteSpace(viewModel.NIC) ||
                !string.IsNullOrWhiteSpace(viewModel.Network) ||
                !string.IsNullOrWhiteSpace(viewModel.DefaultGateway) ||
                viewModel.DHCPEnabled ||
                !string.IsNullOrWhiteSpace(viewModel.DHCPServer) ||
                !string.IsNullOrWhiteSpace(viewModel.DNSHostname))
            {
                networkDetails = new NetworkDetails
                {
                    IPAddress = viewModel.IPAddress,
                    MACAddress = viewModel.MACAddress,
                    NIC = viewModel.NIC,
                    Network = viewModel.Network,
                    DefaultGateway = viewModel.DefaultGateway,
                    DHCPEnabled = viewModel.DHCPEnabled,
                    DHCPServer = viewModel.DHCPServer,
                    DNSHostname = viewModel.DNSHostname,
                    CreatedAt = DateTime.UtcNow
                };
                _context.NetworkDetails.Add(networkDetails);
                await _context.SaveChangesAsync();
            }

            // 3. Create AccessPoint
            var accessPoint = new AccessPoint
            {
                Name = viewModel.Name,
                ProductId = viewModel.ProductId,
                SerialNumber = viewModel.SerialNumber,
                AssetTag = viewModel.AssetTag,
                VendorId = viewModel.VendorId,
                PurchaseCost = viewModel.PurchaseCost,
                ExpiryDate = viewModel.ExpiryDate,
                Location = viewModel.Location,
                AcquisitionDate = viewModel.AcquisitionDate,
                WarrantyExpiryDate = viewModel.WarrantyExpiryDate,
                AssetStateId = assetState.Id,
                NetworkDetailsId = networkDetails?.Id,
                CreatedById = currentUser?.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccessPoints.Add(accessPoint);
            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ Access Point '{accessPoint.Name}' created successfully";
            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access point");
            ModelState.AddModelError("", "Error saving data. Please try again.");
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            return View(viewModel);
        }
    }

    // GET: Assets/EditAccessPoint/5
    [Authorize(Roles = "Admin,Support,IT")]
    public async Task<IActionResult> EditAccessPoint(int id)
    {
        var accessPoint = await _context.AccessPoints
            .Include(a => a.NetworkDetails)
            .Include(a => a.AssetState)
            .Include(a => a.Product)
            .Include(a => a.Vendor)
            .FirstOrDefaultAsync(a => a.Id == id);
            
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
    [Authorize(Roles = "Admin,Support,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccessPoint(int id, AccessPoint model)
    {
        if (id != model.Id)
            return NotFound();

        // Remove validation errors for navigation properties
        ModelState.Remove("Product");
        ModelState.Remove("Vendor");
        ModelState.Remove("AssetState");
        ModelState.Remove("NetworkDetails");

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
            var existingAccessPoint = await _context.AccessPoints
                .Include(a => a.NetworkDetails)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingAccessPoint == null)
                return NotFound();

            // Update basic properties
            existingAccessPoint.Name = model.Name;
            existingAccessPoint.ProductId = model.ProductId;
            existingAccessPoint.SerialNumber = model.SerialNumber;
            existingAccessPoint.AssetTag = model.AssetTag;
            existingAccessPoint.VendorId = model.VendorId;
            existingAccessPoint.PurchaseCost = model.PurchaseCost;
            existingAccessPoint.ExpiryDate = model.ExpiryDate;
            existingAccessPoint.Location = model.Location;
            existingAccessPoint.AcquisitionDate = model.AcquisitionDate;
            existingAccessPoint.WarrantyExpiryDate = model.WarrantyExpiryDate;
            existingAccessPoint.AssetStateId = model.AssetStateId;
            existingAccessPoint.UpdatedAt = DateTime.UtcNow;

            // Update NetworkDetails
            if (existingAccessPoint.NetworkDetails == null)
            {
                existingAccessPoint.NetworkDetails = new NetworkDetails();
            }

            var networkDetailsForm = Request.Form;
            existingAccessPoint.NetworkDetails.IPAddress = networkDetailsForm["NetworkDetails.IPAddress"];
            existingAccessPoint.NetworkDetails.MACAddress = networkDetailsForm["NetworkDetails.MACAddress"];
            existingAccessPoint.NetworkDetails.NIC = networkDetailsForm["NetworkDetails.NIC"];
            existingAccessPoint.NetworkDetails.Network = networkDetailsForm["NetworkDetails.Network"];
            existingAccessPoint.NetworkDetails.DefaultGateway = networkDetailsForm["NetworkDetails.DefaultGateway"];
            existingAccessPoint.NetworkDetails.DHCPEnabled = networkDetailsForm["NetworkDetails.DHCPEnabled"] == "true";
            existingAccessPoint.NetworkDetails.DHCPServer = networkDetailsForm["NetworkDetails.DHCPServer"];
            existingAccessPoint.NetworkDetails.DNSHostname = networkDetailsForm["NetworkDetails.DNSHostname"];

            await _context.SaveChangesAsync();

            TempData["Toast"] = $"✅ Access Point '{model.Name}' updated successfully";
            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access point");
            ModelState.AddModelError("", "Error saving data: " + ex.Message);
            ViewBag.Products = await _context.Products
                .Where(p => p.ProductType == "Access Point")
                .ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = await _context.AssetStates.ToListAsync();
            return View(model);
        }
    }

    // POST: Assets/DeleteAccessPoint/5
    [Authorize(Roles = "Admin,IT")]
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

    // POST: Assets/CreateVendor - إنشاء مورد جديد عبر API
    [HttpPost]
    [Authorize(Roles = "Admin,Support,IT")]
    public async Task<IActionResult> CreateVendor([FromBody] VendorDto vendorDto)
    {
        try
        {
            _logger.LogInformation("CreateVendor called with data: {@VendorDto}", vendorDto);

            if (vendorDto == null || string.IsNullOrWhiteSpace(vendorDto.VendorName))
            {
                return Json(new { success = false, message = "يجب إدخال اسم المورد" });
            }

            var vendor = new Vendor
            {
                VendorName = vendorDto.VendorName,
                Currency = vendorDto.Currency ?? "SR",
                DoorNumber = vendorDto.DoorNumber,
                Landmark = vendorDto.Landmark,
                PostalCode = vendorDto.PostalCode,
                Country = vendorDto.Country,
                Fax = vendorDto.Fax,
                FirstName = vendorDto.FirstName,
                Street = vendorDto.Street,
                City = vendorDto.City,
                StateProvince = vendorDto.StateProvince,
                PhoneNo = vendorDto.PhoneNo,
                Email = vendorDto.Email,
                Description = vendorDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vendor created successfully with ID: {VendorId}", vendor.Id);

            return Json(new { success = true, id = vendor.Id, name = vendor.VendorName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vendor: {Message}", ex.Message);
            return StatusCode(500, new { success = false, message = $"خطأ: {ex.Message}" });
        }
    }

    // POST: Assets/CreateProduct - إنشاء منتج جديد عبر API
    [HttpPost]
    [Authorize(Roles = "Admin,Support,IT")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
    {
        try
        {
            _logger.LogInformation("CreateProduct called with data: {@ProductDto}", productDto);

            if (productDto == null)
            {
                return Json(new { success = false, message = "لم يتم استلام البيانات" });
            }

            if (string.IsNullOrWhiteSpace(productDto.ProductName) ||
                string.IsNullOrWhiteSpace(productDto.Manufacturer) ||
                string.IsNullOrWhiteSpace(productDto.ProductType))
            {
                return Json(new { success = false, message = "يجب إدخال جميع الحقول المطلوبة" });
            }

            var product = new Product
            {
                ProductType = productDto.ProductType,
                ProductName = productDto.ProductName,
                Manufacturer = productDto.Manufacturer,
                PartNo = productDto.PartNo,
                Cost = productDto.Cost,
                Description = productDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            return Json(new
            {
                success = true,
                id = product.Id,
                productName = product.ProductName,
                manufacturer = product.Manufacturer
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {Message}", ex.Message);
            return StatusCode(500, new { success = false, message = $"خطأ: {ex.Message}" });
        }
    }

    // DTO class for Product creation
    public class ProductDto
    {
        public string ProductType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public decimal Cost { get; set; }
        public string? Description { get; set; }
    }

    // DTO class for Vendor creation
    public class VendorDto
    {
        public string VendorName { get; set; } = string.Empty;
        public string? Currency { get; set; }
        public string? DoorNumber { get; set; }
        public string? Landmark { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Fax { get; set; }
        public string? FirstName { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PhoneNo { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
    }

    // #endregion
}
