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
            var computersTotal = await _context.Workstations.CountAsync();
            var computersInUse = await _context.Workstations.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var computersInStore = await _context.Workstations.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var computersInRepair = await _context.Workstations.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var computersOthers = await _context.Workstations.CountAsync(a => a.AssetState != null &&
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Servers
            var serversTotal = await _context.Servers.CountAsync();
            var serversInUse = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var serversInStore = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var serversInRepair = await _context.Servers.CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var serversOthers = await _context.Servers.CountAsync(a => a.AssetState != null &&
                (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Smartphones (Mobile Devices)
            var smartphonesTotal = await _context.MobileDevices.CountAsync();
            var smartphonesInUse = await _context.MobileDevices
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var smartphonesInStore = await _context.MobileDevices
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var smartphonesInRepair = await _context.MobileDevices
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var smartphonesOthers = await _context.MobileDevices
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null &&
                    (a.AssetState.Status == AssetStatusEnum.Expired || a.AssetState.Status == AssetStatusEnum.Disposed));

            // Printers
            var printersTotal = await _context.Printers.CountAsync();
            var printersInUse = await _context.Printers
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InUse);
            var printersInStore = await _context.Printers
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InStore);
            var printersInRepair = await _context.Printers
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null && a.AssetState.Status == AssetStatusEnum.InRepair);
            var printersOthers = await _context.Printers
                .Include(a => a.AssetState)
                .CountAsync(a => a.AssetState != null &&
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

            TempData["SuccessMessage"] = "Access Point created successfully!";
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

            TempData["SuccessMessage"] = "Access Point updated successfully!";
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

            return RedirectToAction(nameof(AccessPoints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting access point");
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

    #region Computers

    // GET: Assets/Computers
    public async Task<IActionResult> Computers()
    {
        var computers = await _context.Workstations
            .Include(c => c.Product)
            .Include(c => c.Vendor)
            .Include(c => c.AssetState)
            .Include(c => c.NetworkDetails)
            .Include(c => c.ComputerInfo)
            .Include(c => c.OperatingSystemInfo)
            .Include(c => c.MemoryDetails)
            .Include(c => c.Processor)
            .Include(c => c.HardDisk)
            .Include(c => c.Keyboard)
            .Include(c => c.Mouse)
            .Include(c => c.Monitor)
            .ToListAsync();

        return View(computers);
    }

    // GET: Assets/CreateComputer
    public async Task<IActionResult> CreateComputer()
    {
        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View();
    }

    // POST: Assets/CreateComputer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateComputer(ComputerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            // Create Computer Info
            ComputerInfo? computerInfo = null;
            if (!string.IsNullOrEmpty(model.ServiceTag) || !string.IsNullOrEmpty(model.ComputerManufacturer))
            {
                computerInfo = new ComputerInfo
                {
                    ServiceTag = model.ServiceTag,
                    Manufacturer = model.ComputerManufacturer,
                    BiosDate = model.BiosDate,
                    Domain = model.Domain,
                    SMBiosVersion = model.SMBiosVersion,
                    BiosVersion = model.BiosVersion,
                    BiosManufacturer = model.BiosManufacturer,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ComputerInfos.Add(computerInfo);
                await _context.SaveChangesAsync();
            }

            // Create Operating System Info
            OperatingSystemInfo? osInfo = null;
            if (!string.IsNullOrEmpty(model.OSName))
            {
                osInfo = new OperatingSystemInfo
                {
                    Name = model.OSName,
                    Version = model.OSVersion,
                    BuildNumber = model.BuildNumber,
                    ServicePack = model.ServicePack,
                    ProductId = model.OSProductId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OperatingSystemInfos.Add(osInfo);
                await _context.SaveChangesAsync();
            }

            // Create Memory Details
            MemoryDetails? memoryDetails = null;
            if (model.RAM.HasValue || model.VirtualMemory.HasValue)
            {
                memoryDetails = new MemoryDetails
                {
                    RAM = model.RAM,
                    VirtualMemory = model.VirtualMemory,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MemoryDetails.Add(memoryDetails);
                await _context.SaveChangesAsync();
            }

            // Create Processor
            Processor? processor = null;
            if (!string.IsNullOrEmpty(model.ProcessorInfo))
            {
                processor = new Processor
                {
                    ProcessorInfo = model.ProcessorInfo,
                    Manufacturer = model.ProcessorManufacturer,
                    ClockSpeedMHz = model.ClockSpeedMHz,
                    NumberOfCores = model.NumberOfCores,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Processors.Add(processor);
                await _context.SaveChangesAsync();
            }

            // Create Hard Disk
            HardDisk? hardDisk = null;
            if (!string.IsNullOrEmpty(model.HardDiskModel))
            {
                hardDisk = new HardDisk
                {
                    Model = model.HardDiskModel,
                    SerialNumber = model.HardDiskSerialNumber,
                    Manufacturer = model.HardDiskManufacturer,
                    CapacityGB = model.CapacityGB,
                    CreatedAt = DateTime.UtcNow
                };
                _context.HardDisks.Add(hardDisk);
                await _context.SaveChangesAsync();
            }

            // Create Keyboard
            Keyboard? keyboard = null;
            if (!string.IsNullOrEmpty(model.KeyboardType))
            {
                keyboard = new Keyboard
                {
                    KeyboardType = model.KeyboardType,
                    Manufacturer = model.KeyboardManufacturer,
                    SerialNumber = model.KeyboardSerialNumber,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Keyboards.Add(keyboard);
                await _context.SaveChangesAsync();
            }

            // Create Mouse
            Mouse? mouse = null;
            if (!string.IsNullOrEmpty(model.MouseType))
            {
                mouse = new Mouse
                {
                    MouseType = model.MouseType,
                    SerialNumber = model.MouseSerialNumber,
                    MouseButtons = model.MouseButtons,
                    Manufacturer = model.MouseManufacturer,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Mice.Add(mouse);
                await _context.SaveChangesAsync();
            }

            // Create Monitor
            ITHelpDesk.Models.Assets.Monitor? monitor = null;
            if (!string.IsNullOrEmpty(model.MonitorType))
            {
                monitor = new ITHelpDesk.Models.Assets.Monitor
                {
                    MonitorType = model.MonitorType,
                    SerialNumber = model.MonitorSerialNumber,
                    Manufacturer = model.MonitorManufacturer,
                    MaxResolution = model.MaxResolution,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Monitors.Add(monitor);
                await _context.SaveChangesAsync();
            }

            // Create Asset State
            var assetState = new AssetState
            {
                Status = (AssetStatusEnum)model.AssetStatus,
                AssociatedTo = model.AssociatedTo,
                Site = model.Site,
                StateComments = model.StateComments,
                UserId = model.UserId,
                Department = model.Department,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetStates.Add(assetState);
            await _context.SaveChangesAsync();

            // Create Network Details
            NetworkDetails? networkDetails = null;
            if (!string.IsNullOrEmpty(model.IPAddress) || !string.IsNullOrEmpty(model.MACAddress))
            {
                networkDetails = new NetworkDetails
                {
                    IPAddress = model.IPAddress,
                    MACAddress = model.MACAddress,
                    NIC = model.NIC,
                    Network = model.Network,
                    DefaultGateway = model.DefaultGateway,
                    DHCPEnabled = model.DHCPEnabled,
                    DHCPServer = model.DHCPServer,
                    DNSHostname = model.DNSHostname,
                    CreatedAt = DateTime.UtcNow
                };
                _context.NetworkDetails.Add(networkDetails);
                await _context.SaveChangesAsync();
            }

            // Create Workstation (Computer)
            var computer = new Workstation
            {
                Name = model.Name,
                ProductId = model.ProductId,
                SerialNumber = model.SerialNumber,
                AssetTag = model.AssetTag,
                VendorId = model.VendorId,
                PurchaseCost = model.PurchaseCost,
                ExpiryDate = model.ExpiryDate,
                Location = model.Location,
                AcquisitionDate = model.AcquisitionDate,
                WarrantyExpiryDate = model.WarrantyExpiryDate,
                AssetStateId = assetState.Id,
                NetworkDetailsId = networkDetails?.Id,
                ComputerInfoId = computerInfo?.Id,
                OperatingSystemInfoId = osInfo?.Id,
                MemoryDetailsId = memoryDetails?.Id,
                ProcessorId = processor?.Id,
                HardDiskId = hardDisk?.Id,
                KeyboardId = keyboard?.Id,
                MouseId = mouse?.Id,
                MonitorId = monitor?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user?.Id
            };

            _context.Workstations.Add(computer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Computer created successfully!";
            return RedirectToAction(nameof(Computers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating computer");
            ModelState.AddModelError("", "An error occurred while creating the computer. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // GET: Assets/EditComputer/5
    public async Task<IActionResult> EditComputer(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var computer = await _context.Workstations
            .Include(c => c.Product)
            .Include(c => c.Vendor)
            .Include(c => c.AssetState)
            .Include(c => c.NetworkDetails)
            .Include(c => c.ComputerInfo)
            .Include(c => c.OperatingSystemInfo)
            .Include(c => c.MemoryDetails)
            .Include(c => c.Processor)
            .Include(c => c.HardDisk)
            .Include(c => c.Keyboard)
            .Include(c => c.Mouse)
            .Include(c => c.Monitor)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (computer == null)
        {
            return NotFound();
        }

        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View(computer);
    }

    // POST: Assets/EditComputer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditComputer(int id, Workstation model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        ModelState.Remove("Product");
        ModelState.Remove("Vendor");
        ModelState.Remove("AssetState");
        ModelState.Remove("NetworkDetails");
        ModelState.Remove("ComputerInfo");
        ModelState.Remove("OperatingSystemInfo");
        ModelState.Remove("MemoryDetails");
        ModelState.Remove("Processor");
        ModelState.Remove("HardDisk");
        ModelState.Remove("Keyboard");
        ModelState.Remove("Mouse");
        ModelState.Remove("Monitor");

        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var existingComputer = await _context.Workstations
                .Include(c => c.NetworkDetails)
                .Include(c => c.ComputerInfo)
                .Include(c => c.OperatingSystemInfo)
                .Include(c => c.MemoryDetails)
                .Include(c => c.Processor)
                .Include(c => c.HardDisk)
                .Include(c => c.Keyboard)
                .Include(c => c.Mouse)
                .Include(c => c.Monitor)
                .Include(c => c.AssetState)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingComputer == null)
            {
                return NotFound();
            }

            // Update basic asset properties
            existingComputer.Name = model.Name;
            existingComputer.ProductId = model.ProductId;
            existingComputer.SerialNumber = model.SerialNumber;
            existingComputer.AssetTag = model.AssetTag;
            existingComputer.VendorId = model.VendorId;
            existingComputer.PurchaseCost = model.PurchaseCost;
            existingComputer.ExpiryDate = model.ExpiryDate;
            existingComputer.Location = model.Location;
            existingComputer.AcquisitionDate = model.AcquisitionDate;
            existingComputer.WarrantyExpiryDate = model.WarrantyExpiryDate;
            existingComputer.UpdatedAt = DateTime.UtcNow;

            // Update Computer Info
            if (existingComputer.ComputerInfo != null)
            {
                existingComputer.ComputerInfo.ServiceTag = Request.Form["ComputerInfo.ServiceTag"];
                existingComputer.ComputerInfo.Manufacturer = Request.Form["ComputerInfo.Manufacturer"];
                existingComputer.ComputerInfo.BiosDate = !string.IsNullOrEmpty(Request.Form["ComputerInfo.BiosDate"])
                    ? DateTime.Parse(Request.Form["ComputerInfo.BiosDate"]!) : null;
                existingComputer.ComputerInfo.Domain = Request.Form["ComputerInfo.Domain"];
                existingComputer.ComputerInfo.SMBiosVersion = Request.Form["ComputerInfo.SMBiosVersion"];
                existingComputer.ComputerInfo.BiosVersion = Request.Form["ComputerInfo.BiosVersion"];
                existingComputer.ComputerInfo.BiosManufacturer = Request.Form["ComputerInfo.BiosManufacturer"];
                existingComputer.ComputerInfo.UpdatedAt = DateTime.UtcNow;
            }

            // Update OS Info
            if (existingComputer.OperatingSystemInfo != null)
            {
                existingComputer.OperatingSystemInfo.Name = Request.Form["OperatingSystemInfo.Name"];
                existingComputer.OperatingSystemInfo.Version = Request.Form["OperatingSystemInfo.Version"];
                existingComputer.OperatingSystemInfo.BuildNumber = Request.Form["OperatingSystemInfo.BuildNumber"];
                existingComputer.OperatingSystemInfo.ServicePack = Request.Form["OperatingSystemInfo.ServicePack"];
                existingComputer.OperatingSystemInfo.ProductId = Request.Form["OperatingSystemInfo.ProductId"];
                existingComputer.OperatingSystemInfo.UpdatedAt = DateTime.UtcNow;
            }

            // Update Memory Details
            if (existingComputer.MemoryDetails != null)
            {
                existingComputer.MemoryDetails.RAM = !string.IsNullOrEmpty(Request.Form["MemoryDetails.RAM"])
                    ? int.Parse(Request.Form["MemoryDetails.RAM"]!) : null;
                existingComputer.MemoryDetails.VirtualMemory = !string.IsNullOrEmpty(Request.Form["MemoryDetails.VirtualMemory"])
                    ? int.Parse(Request.Form["MemoryDetails.VirtualMemory"]!) : null;
                existingComputer.MemoryDetails.UpdatedAt = DateTime.UtcNow;
            }

            // Update Processor
            if (existingComputer.Processor != null)
            {
                existingComputer.Processor.ProcessorInfo = Request.Form["Processor.ProcessorInfo"];
                existingComputer.Processor.Manufacturer = Request.Form["Processor.Manufacturer"];
                existingComputer.Processor.ClockSpeedMHz = !string.IsNullOrEmpty(Request.Form["Processor.ClockSpeedMHz"])
                    ? int.Parse(Request.Form["Processor.ClockSpeedMHz"]!) : null;
                existingComputer.Processor.NumberOfCores = !string.IsNullOrEmpty(Request.Form["Processor.NumberOfCores"])
                    ? int.Parse(Request.Form["Processor.NumberOfCores"]!) : null;
                existingComputer.Processor.UpdatedAt = DateTime.UtcNow;
            }

            // Update Hard Disk
            if (existingComputer.HardDisk != null)
            {
                existingComputer.HardDisk.Model = Request.Form["HardDisk.Model"];
                existingComputer.HardDisk.SerialNumber = Request.Form["HardDisk.SerialNumber"];
                existingComputer.HardDisk.Manufacturer = Request.Form["HardDisk.Manufacturer"];
                existingComputer.HardDisk.CapacityGB = !string.IsNullOrEmpty(Request.Form["HardDisk.CapacityGB"])
                    ? int.Parse(Request.Form["HardDisk.CapacityGB"]!) : null;
                existingComputer.HardDisk.UpdatedAt = DateTime.UtcNow;
            }

            // Update Keyboard
            if (existingComputer.Keyboard != null)
            {
                existingComputer.Keyboard.KeyboardType = Request.Form["Keyboard.KeyboardType"];
                existingComputer.Keyboard.Manufacturer = Request.Form["Keyboard.Manufacturer"];
                existingComputer.Keyboard.SerialNumber = Request.Form["Keyboard.SerialNumber"];
                existingComputer.Keyboard.UpdatedAt = DateTime.UtcNow;
            }

            // Update Mouse
            if (existingComputer.Mouse != null)
            {
                existingComputer.Mouse.MouseType = Request.Form["Mouse.MouseType"];
                existingComputer.Mouse.SerialNumber = Request.Form["Mouse.SerialNumber"];
                existingComputer.Mouse.MouseButtons = !string.IsNullOrEmpty(Request.Form["Mouse.MouseButtons"])
                    ? int.Parse(Request.Form["Mouse.MouseButtons"]!) : null;
                existingComputer.Mouse.Manufacturer = Request.Form["Mouse.Manufacturer"];
                existingComputer.Mouse.UpdatedAt = DateTime.UtcNow;
            }

            // Update Monitor
            if (existingComputer.Monitor != null)
            {
                existingComputer.Monitor.MonitorType = Request.Form["Monitor.MonitorType"];
                existingComputer.Monitor.SerialNumber = Request.Form["Monitor.SerialNumber"];
                existingComputer.Monitor.Manufacturer = Request.Form["Monitor.Manufacturer"];
                existingComputer.Monitor.MaxResolution = Request.Form["Monitor.MaxResolution"];
                existingComputer.Monitor.UpdatedAt = DateTime.UtcNow;
            }

            // Update Asset State
            if (existingComputer.AssetState != null)
            {
                existingComputer.AssetState.Status = (AssetStatusEnum)int.Parse(Request.Form["AssetState.Status"]!);
                existingComputer.AssetState.AssociatedTo = Request.Form["AssetState.AssociatedTo"];
                existingComputer.AssetState.Site = Request.Form["AssetState.Site"];
                existingComputer.AssetState.StateComments = Request.Form["AssetState.StateComments"];
                existingComputer.AssetState.UserId = Request.Form["AssetState.UserId"];
                existingComputer.AssetState.Department = Request.Form["AssetState.Department"];
                existingComputer.AssetState.UpdatedAt = DateTime.UtcNow;
            }

            // Update Network Details
            if (existingComputer.NetworkDetails != null)
            {
                existingComputer.NetworkDetails.IPAddress = Request.Form["NetworkDetails.IPAddress"];
                existingComputer.NetworkDetails.MACAddress = Request.Form["NetworkDetails.MACAddress"];
                existingComputer.NetworkDetails.NIC = Request.Form["NetworkDetails.NIC"];
                existingComputer.NetworkDetails.Network = Request.Form["NetworkDetails.Network"];
                existingComputer.NetworkDetails.DefaultGateway = Request.Form["NetworkDetails.DefaultGateway"];
                existingComputer.NetworkDetails.DHCPEnabled = Request.Form["NetworkDetails.DHCPEnabled"] == "true";
                existingComputer.NetworkDetails.DHCPServer = Request.Form["NetworkDetails.DHCPServer"];
                existingComputer.NetworkDetails.DNSHostname = Request.Form["NetworkDetails.DNSHostname"];
                existingComputer.NetworkDetails.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Computer updated successfully!";
            return RedirectToAction(nameof(Computers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating computer");
            ModelState.AddModelError("", "An error occurred while updating the computer. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // POST: Assets/DeleteComputer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComputer(int id)
    {
        try
        {
            var computer = await _context.Workstations
                .Include(c => c.NetworkDetails)
                .Include(c => c.ComputerInfo)
                .Include(c => c.OperatingSystemInfo)
                .Include(c => c.MemoryDetails)
                .Include(c => c.Processor)
                .Include(c => c.HardDisk)
                .Include(c => c.Keyboard)
                .Include(c => c.Mouse)
                .Include(c => c.Monitor)
                .Include(c => c.AssetState)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                return NotFound();
            }

            // Delete related records
            if (computer.NetworkDetails != null)
                _context.NetworkDetails.Remove(computer.NetworkDetails);

            if (computer.ComputerInfo != null)
                _context.ComputerInfos.Remove(computer.ComputerInfo);

            if (computer.OperatingSystemInfo != null)
                _context.OperatingSystemInfos.Remove(computer.OperatingSystemInfo);

            if (computer.MemoryDetails != null)
                _context.MemoryDetails.Remove(computer.MemoryDetails);

            if (computer.Processor != null)
                _context.Processors.Remove(computer.Processor);

            if (computer.HardDisk != null)
                _context.HardDisks.Remove(computer.HardDisk);

            if (computer.Keyboard != null)
                _context.Keyboards.Remove(computer.Keyboard);

            if (computer.Mouse != null)
                _context.Mice.Remove(computer.Mouse);

            if (computer.Monitor != null)
                _context.Monitors.Remove(computer.Monitor);

            if (computer.AssetState != null)
                _context.AssetStates.Remove(computer.AssetState);

            _context.Workstations.Remove(computer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Computers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting computer");
            TempData["ErrorMessage"] = "An error occurred while deleting the computer.";
            return RedirectToAction(nameof(Computers));
        }
    }

    #endregion

    #region Servers

    // GET: Assets/Servers
    public async Task<IActionResult> Servers()
    {
        var servers = await _context.Servers
            .Include(s => s.Product)
            .Include(s => s.Vendor)
            .Include(s => s.AssetState)
            .Include(s => s.NetworkDetails)
            .Include(s => s.ComputerInfo)
            .Include(s => s.OperatingSystemInfo)
            .Include(s => s.MemoryDetails)
            .Include(s => s.Processor)
            .Include(s => s.HardDisk)
            .Include(s => s.Keyboard)
            .Include(s => s.Mouse)
            .Include(s => s.Monitor)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return View(servers);
    }

    // GET: Assets/CreateServer
    [Authorize(Roles = "Admin,Support,IT")]
    public async Task<IActionResult> CreateServer()
    {
        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View();
    }

    // POST: Assets/CreateServer
    [Authorize(Roles = "Admin,Support,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateServer(ServerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            // Create Computer Info
            ComputerInfo? computerInfo = null;
            if (!string.IsNullOrEmpty(model.ServiceTag) || !string.IsNullOrEmpty(model.ComputerManufacturer))
            {
                computerInfo = new ComputerInfo
                {
                    ServiceTag = model.ServiceTag,
                    Manufacturer = model.ComputerManufacturer,
                    BiosDate = model.BiosDate,
                    Domain = model.Domain,
                    SMBiosVersion = model.SMBiosVersion,
                    BiosVersion = model.BiosVersion,
                    BiosManufacturer = model.BiosManufacturer,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ComputerInfos.Add(computerInfo);
                await _context.SaveChangesAsync();
            }

            // Create Operating System Info
            OperatingSystemInfo? osInfo = null;
            if (!string.IsNullOrEmpty(model.OSName))
            {
                osInfo = new OperatingSystemInfo
                {
                    Name = model.OSName,
                    Version = model.OSVersion,
                    BuildNumber = model.BuildNumber,
                    ServicePack = model.ServicePack,
                    ProductId = model.OSProductId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OperatingSystemInfos.Add(osInfo);
                await _context.SaveChangesAsync();
            }

            // Create Memory Details
            MemoryDetails? memoryDetails = null;
            if (model.RAM.HasValue || model.VirtualMemory.HasValue)
            {
                memoryDetails = new MemoryDetails
                {
                    RAM = model.RAM,
                    VirtualMemory = model.VirtualMemory,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MemoryDetails.Add(memoryDetails);
                await _context.SaveChangesAsync();
            }

            // Create Processor
            Processor? processor = null;
            if (!string.IsNullOrEmpty(model.ProcessorInfo))
            {
                processor = new Processor
                {
                    ProcessorInfo = model.ProcessorInfo,
                    Manufacturer = model.ProcessorManufacturer,
                    ClockSpeedMHz = model.ClockSpeedMHz,
                    NumberOfCores = model.NumberOfCores,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Processors.Add(processor);
                await _context.SaveChangesAsync();
            }

            // Create Hard Disk
            HardDisk? hardDisk = null;
            if (!string.IsNullOrEmpty(model.HardDiskModel))
            {
                hardDisk = new HardDisk
                {
                    Model = model.HardDiskModel,
                    SerialNumber = model.HardDiskSerialNumber,
                    Manufacturer = model.HardDiskManufacturer,
                    CapacityGB = model.CapacityGB,
                    CreatedAt = DateTime.UtcNow
                };
                _context.HardDisks.Add(hardDisk);
                await _context.SaveChangesAsync();
            }

            // Create Keyboard
            Keyboard? keyboard = null;
            if (!string.IsNullOrEmpty(model.KeyboardType))
            {
                keyboard = new Keyboard
                {
                    KeyboardType = model.KeyboardType,
                    Manufacturer = model.KeyboardManufacturer,
                    SerialNumber = model.KeyboardSerialNumber,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Keyboards.Add(keyboard);
                await _context.SaveChangesAsync();
            }

            // Create Mouse
            Mouse? mouse = null;
            if (!string.IsNullOrEmpty(model.MouseType))
            {
                mouse = new Mouse
                {
                    MouseType = model.MouseType,
                    SerialNumber = model.MouseSerialNumber,
                    MouseButtons = model.MouseButtons,
                    Manufacturer = model.MouseManufacturer,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Mice.Add(mouse);
                await _context.SaveChangesAsync();
            }

            // Create Monitor
            ITHelpDesk.Models.Assets.Monitor? monitor = null;
            if (!string.IsNullOrEmpty(model.MonitorType))
            {
                monitor = new ITHelpDesk.Models.Assets.Monitor
                {
                    MonitorType = model.MonitorType,
                    SerialNumber = model.MonitorSerialNumber,
                    Manufacturer = model.MonitorManufacturer,
                    MaxResolution = model.MaxResolution,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Monitors.Add(monitor);
                await _context.SaveChangesAsync();
            }

            // Create Asset State
            var assetState = new AssetState
            {
                Status = model.Status,
                AssociatedTo = model.AssociatedTo,
                Site = model.Site,
                StateComments = model.StateComments,
                UserId = model.UserId,
                Department = model.Department,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetStates.Add(assetState);
            await _context.SaveChangesAsync();

            // Create Network Details
            NetworkDetails? networkDetails = null;
            if (!string.IsNullOrEmpty(model.IPAddress) || !string.IsNullOrEmpty(model.MACAddress))
            {
                networkDetails = new NetworkDetails
                {
                    IPAddress = model.IPAddress,
                    MACAddress = model.MACAddress,
                    NIC = model.NIC,
                    Network = model.Network,
                    DefaultGateway = model.DefaultGateway,
                    DHCPEnabled = model.DHCPEnabled,
                    DHCPServer = model.DHCPServer,
                    DNSHostname = model.DNSHostname,
                    CreatedAt = DateTime.UtcNow
                };
                _context.NetworkDetails.Add(networkDetails);
                await _context.SaveChangesAsync();
            }

            // Create Server
            var server = new Server
            {
                Name = model.Name,
                ProductId = model.ProductId,
                SerialNumber = model.SerialNumber,
                AssetTag = model.AssetTag,
                VendorId = model.VendorId,
                PurchaseCost = model.PurchaseCost ?? 0,
                ExpiryDate = model.ExpiryDate,
                Location = model.Location,
                AcquisitionDate = model.AcquisitionDate,
                WarrantyExpiryDate = model.WarrantyExpiryDate,
                AssetStateId = assetState.Id,
                NetworkDetailsId = networkDetails?.Id,
                ComputerInfoId = computerInfo?.Id,
                OperatingSystemInfoId = osInfo?.Id,
                MemoryDetailsId = memoryDetails?.Id,
                ProcessorId = processor?.Id,
                HardDiskId = hardDisk?.Id,
                KeyboardId = keyboard?.Id,
                MouseId = mouse?.Id,
                MonitorId = monitor?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user?.Id
            };

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Server created successfully!";
            return RedirectToAction(nameof(Servers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating server");
            ModelState.AddModelError("", "An error occurred while creating the server. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // GET: Assets/EditServer/5
    [Authorize(Roles = "Admin,Support,IT")]
    public async Task<IActionResult> EditServer(int id)
    {
        var server = await _context.Servers
            .Include(s => s.Product)
            .Include(s => s.Vendor)
            .Include(s => s.AssetState)
            .Include(s => s.NetworkDetails)
            .Include(s => s.ComputerInfo)
            .Include(s => s.OperatingSystemInfo)
            .Include(s => s.MemoryDetails)
            .Include(s => s.Processor)
            .Include(s => s.HardDisk)
            .Include(s => s.Keyboard)
            .Include(s => s.Mouse)
            .Include(s => s.Monitor)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (server == null)
        {
            return NotFound();
        }

        var model = new ServerCreateViewModel
        {
            Id = server.Id,
            Name = server.Name,
            ProductId = server.ProductId,
            SerialNumber = server.SerialNumber,
            AssetTag = server.AssetTag,
            VendorId = server.VendorId,
            PurchaseCost = server.PurchaseCost,
            ExpiryDate = server.ExpiryDate,
            Location = server.Location,
            AcquisitionDate = server.AcquisitionDate,
            WarrantyExpiryDate = server.WarrantyExpiryDate,
            Status = server.AssetState?.Status ?? AssetStatusEnum.InStore,
            AssociatedTo = server.AssetState?.AssociatedTo,
            Site = server.AssetState?.Site,
            StateComments = server.AssetState?.StateComments,
            UserId = server.AssetState?.UserId,
            Department = server.AssetState?.Department,
            IPAddress = server.NetworkDetails?.IPAddress,
            MACAddress = server.NetworkDetails?.MACAddress,
            NIC = server.NetworkDetails?.NIC,
            Network = server.NetworkDetails?.Network,
            DefaultGateway = server.NetworkDetails?.DefaultGateway,
            DHCPEnabled = server.NetworkDetails?.DHCPEnabled ?? false,
            DHCPServer = server.NetworkDetails?.DHCPServer,
            DNSHostname = server.NetworkDetails?.DNSHostname,
            ServiceTag = server.ComputerInfo?.ServiceTag,
            ComputerManufacturer = server.ComputerInfo?.Manufacturer,
            BiosDate = server.ComputerInfo?.BiosDate,
            Domain = server.ComputerInfo?.Domain,
            SMBiosVersion = server.ComputerInfo?.SMBiosVersion,
            BiosVersion = server.ComputerInfo?.BiosVersion,
            BiosManufacturer = server.ComputerInfo?.BiosManufacturer,
            OSName = server.OperatingSystemInfo?.Name,
            OSVersion = server.OperatingSystemInfo?.Version,
            BuildNumber = server.OperatingSystemInfo?.BuildNumber,
            ServicePack = server.OperatingSystemInfo?.ServicePack,
            OSProductId = server.OperatingSystemInfo?.ProductId,
            RAM = server.MemoryDetails?.RAM,
            VirtualMemory = server.MemoryDetails?.VirtualMemory,
            ProcessorInfo = server.Processor?.ProcessorInfo,
            ProcessorManufacturer = server.Processor?.Manufacturer,
            ClockSpeedMHz = server.Processor?.ClockSpeedMHz,
            NumberOfCores = server.Processor?.NumberOfCores,
            HardDiskModel = server.HardDisk?.Model,
            HardDiskSerialNumber = server.HardDisk?.SerialNumber,
            HardDiskManufacturer = server.HardDisk?.Manufacturer,
            CapacityGB = server.HardDisk?.CapacityGB,
            KeyboardType = server.Keyboard?.KeyboardType,
            KeyboardManufacturer = server.Keyboard?.Manufacturer,
            KeyboardSerialNumber = server.Keyboard?.SerialNumber,
            MouseType = server.Mouse?.MouseType,
            MouseSerialNumber = server.Mouse?.SerialNumber,
            MouseButtons = server.Mouse?.MouseButtons,
            MouseManufacturer = server.Mouse?.Manufacturer,
            MonitorType = server.Monitor?.MonitorType,
            MonitorSerialNumber = server.Monitor?.SerialNumber,
            MonitorManufacturer = server.Monitor?.Manufacturer,
            MaxResolution = server.Monitor?.MaxResolution
        };

        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View(model);
    }

    // POST: Assets/EditServer/5
    [Authorize(Roles = "Admin,Support,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditServer(int id, ServerCreateViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var server = await _context.Servers
                .Include(s => s.NetworkDetails)
                .Include(s => s.ComputerInfo)
                .Include(s => s.OperatingSystemInfo)
                .Include(s => s.MemoryDetails)
                .Include(s => s.Processor)
                .Include(s => s.HardDisk)
                .Include(s => s.Keyboard)
                .Include(s => s.Mouse)
                .Include(s => s.Monitor)
                .Include(s => s.AssetState)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (server == null)
            {
                return NotFound();
            }

            // Update Computer Info
            if (!string.IsNullOrEmpty(model.ServiceTag) || !string.IsNullOrEmpty(model.ComputerManufacturer))
            {
                if (server.ComputerInfo == null)
                {
                    server.ComputerInfo = new ComputerInfo { CreatedAt = DateTime.UtcNow };
                    _context.ComputerInfos.Add(server.ComputerInfo);
                }
                server.ComputerInfo.ServiceTag = model.ServiceTag;
                server.ComputerInfo.Manufacturer = model.ComputerManufacturer;
                server.ComputerInfo.BiosDate = model.BiosDate;
                server.ComputerInfo.Domain = model.Domain;
                server.ComputerInfo.SMBiosVersion = model.SMBiosVersion;
                server.ComputerInfo.BiosVersion = model.BiosVersion;
                server.ComputerInfo.BiosManufacturer = model.BiosManufacturer;
            }

            // Update Operating System Info
            if (!string.IsNullOrEmpty(model.OSName))
            {
                if (server.OperatingSystemInfo == null)
                {
                    server.OperatingSystemInfo = new OperatingSystemInfo { CreatedAt = DateTime.UtcNow };
                    _context.OperatingSystemInfos.Add(server.OperatingSystemInfo);
                }
                server.OperatingSystemInfo.Name = model.OSName;
                server.OperatingSystemInfo.Version = model.OSVersion;
                server.OperatingSystemInfo.BuildNumber = model.BuildNumber;
                server.OperatingSystemInfo.ServicePack = model.ServicePack;
                server.OperatingSystemInfo.ProductId = model.OSProductId;
            }

            // Update Memory Details
            if (model.RAM.HasValue || model.VirtualMemory.HasValue)
            {
                if (server.MemoryDetails == null)
                {
                    server.MemoryDetails = new MemoryDetails { CreatedAt = DateTime.UtcNow };
                    _context.MemoryDetails.Add(server.MemoryDetails);
                }
                server.MemoryDetails.RAM = model.RAM;
                server.MemoryDetails.VirtualMemory = model.VirtualMemory;
            }

            // Update Processor
            if (!string.IsNullOrEmpty(model.ProcessorInfo))
            {
                if (server.Processor == null)
                {
                    server.Processor = new Processor { CreatedAt = DateTime.UtcNow };
                    _context.Processors.Add(server.Processor);
                }
                server.Processor.ProcessorInfo = model.ProcessorInfo;
                server.Processor.Manufacturer = model.ProcessorManufacturer;
                server.Processor.ClockSpeedMHz = model.ClockSpeedMHz;
                server.Processor.NumberOfCores = model.NumberOfCores;
            }

            // Update Hard Disk
            if (!string.IsNullOrEmpty(model.HardDiskModel))
            {
                if (server.HardDisk == null)
                {
                    server.HardDisk = new HardDisk { CreatedAt = DateTime.UtcNow };
                    _context.HardDisks.Add(server.HardDisk);
                }
                server.HardDisk.Model = model.HardDiskModel;
                server.HardDisk.SerialNumber = model.HardDiskSerialNumber;
                server.HardDisk.Manufacturer = model.HardDiskManufacturer;
                server.HardDisk.CapacityGB = model.CapacityGB;
            }

            // Update Keyboard
            if (!string.IsNullOrEmpty(model.KeyboardType))
            {
                if (server.Keyboard == null)
                {
                    server.Keyboard = new Keyboard { CreatedAt = DateTime.UtcNow };
                    _context.Keyboards.Add(server.Keyboard);
                }
                server.Keyboard.KeyboardType = model.KeyboardType;
                server.Keyboard.Manufacturer = model.KeyboardManufacturer;
                server.Keyboard.SerialNumber = model.KeyboardSerialNumber;
            }

            // Update Mouse
            if (!string.IsNullOrEmpty(model.MouseType))
            {
                if (server.Mouse == null)
                {
                    server.Mouse = new Mouse { CreatedAt = DateTime.UtcNow };
                    _context.Mice.Add(server.Mouse);
                }
                server.Mouse.MouseType = model.MouseType;
                server.Mouse.SerialNumber = model.MouseSerialNumber;
                server.Mouse.MouseButtons = model.MouseButtons;
                server.Mouse.Manufacturer = model.MouseManufacturer;
            }

            // Update Monitor
            if (!string.IsNullOrEmpty(model.MonitorType))
            {
                if (server.Monitor == null)
                {
                    server.Monitor = new ITHelpDesk.Models.Assets.Monitor { CreatedAt = DateTime.UtcNow };
                    _context.Monitors.Add(server.Monitor);
                }
                server.Monitor.MonitorType = model.MonitorType;
                server.Monitor.SerialNumber = model.MonitorSerialNumber;
                server.Monitor.Manufacturer = model.MonitorManufacturer;
                server.Monitor.MaxResolution = model.MaxResolution;
            }

            // Update Asset State
            if (server.AssetState != null)
            {
                server.AssetState.Status = model.Status;
                server.AssetState.AssociatedTo = model.AssociatedTo;
                server.AssetState.Site = model.Site;
                server.AssetState.StateComments = model.StateComments;
                server.AssetState.UserId = model.UserId;
                server.AssetState.Department = model.Department;
            }

            // Update Network Details
            if (!string.IsNullOrEmpty(model.IPAddress) || !string.IsNullOrEmpty(model.MACAddress))
            {
                if (server.NetworkDetails == null)
                {
                    server.NetworkDetails = new NetworkDetails { CreatedAt = DateTime.UtcNow };
                    _context.NetworkDetails.Add(server.NetworkDetails);
                }
                server.NetworkDetails.IPAddress = model.IPAddress;
                server.NetworkDetails.MACAddress = model.MACAddress;
                server.NetworkDetails.NIC = model.NIC;
                server.NetworkDetails.Network = model.Network;
                server.NetworkDetails.DefaultGateway = model.DefaultGateway;
                server.NetworkDetails.DHCPEnabled = model.DHCPEnabled;
                server.NetworkDetails.DHCPServer = model.DHCPServer;
                server.NetworkDetails.DNSHostname = model.DNSHostname;
            }

            // Update Server
            server.Name = model.Name;
            server.ProductId = model.ProductId;
            server.SerialNumber = model.SerialNumber;
            server.AssetTag = model.AssetTag;
            server.VendorId = model.VendorId;
            server.PurchaseCost = model.PurchaseCost ?? 0;
            server.ExpiryDate = model.ExpiryDate;
            server.Location = model.Location;
            server.AcquisitionDate = model.AcquisitionDate;
            server.WarrantyExpiryDate = model.WarrantyExpiryDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Server updated successfully!";
            return RedirectToAction(nameof(Servers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating server");
            ModelState.AddModelError("", "An error occurred while updating the server. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // POST: Assets/DeleteServer/5
    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteServer(int id)
    {
        try
        {
            var server = await _context.Servers
                .Include(s => s.NetworkDetails)
                .Include(s => s.ComputerInfo)
                .Include(s => s.OperatingSystemInfo)
                .Include(s => s.MemoryDetails)
                .Include(s => s.Processor)
                .Include(s => s.HardDisk)
                .Include(s => s.Keyboard)
                .Include(s => s.Mouse)
                .Include(s => s.Monitor)
                .Include(s => s.AssetState)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (server == null)
            {
                return NotFound();
            }

            // Delete related records
            if (server.NetworkDetails != null)
                _context.NetworkDetails.Remove(server.NetworkDetails);

            if (server.ComputerInfo != null)
                _context.ComputerInfos.Remove(server.ComputerInfo);

            if (server.OperatingSystemInfo != null)
                _context.OperatingSystemInfos.Remove(server.OperatingSystemInfo);

            if (server.MemoryDetails != null)
                _context.MemoryDetails.Remove(server.MemoryDetails);

            if (server.Processor != null)
                _context.Processors.Remove(server.Processor);

            if (server.HardDisk != null)
                _context.HardDisks.Remove(server.HardDisk);

            if (server.Keyboard != null)
                _context.Keyboards.Remove(server.Keyboard);

            if (server.Mouse != null)
                _context.Mice.Remove(server.Mouse);

            if (server.Monitor != null)
                _context.Monitors.Remove(server.Monitor);

            if (server.AssetState != null)
                _context.AssetStates.Remove(server.AssetState);

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Server deleted successfully!";
            return RedirectToAction(nameof(Servers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting server");
            TempData["ErrorMessage"] = "An error occurred while deleting the server.";
            return RedirectToAction(nameof(Servers));
        }
    }

    #endregion

    #region Mobile Devices

    // GET: Assets/Mobiles
    public async Task<IActionResult> Mobiles()
    {
        var mobiles = await _context.MobileDevices
            .Include(m => m.Product)
            .Include(m => m.Vendor)
            .Include(m => m.AssetState)
            .Include(m => m.NetworkDetails)
            .Include(m => m.MobileDetails)
            .Include(m => m.OperatingSystemInfo)
            .ToListAsync();

        return View(mobiles);
    }

    // GET: Assets/CreateMobile
    public async Task<IActionResult> CreateMobile()
    {
        // Prevent browser caching
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View(new MobileCreateViewModel());
    }

    // GET: Assets/CreateMobileSimple - for testing
    public async Task<IActionResult> CreateMobileSimple()
    {
        return View();
    }

    // POST: Assets/CreateMobile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMobile(MobileCreateViewModel viewModel)
    {
        _logger.LogInformation($"CreateMobile POST called. Name: '{viewModel?.Name}', ProductId: {viewModel?.ProductId}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Errors:");
            foreach (var key in ModelState.Keys)
            {
                var modelState = ModelState[key];
                foreach (var error in modelState.Errors)
                {
                    _logger.LogWarning($"  - {key}: {error.ErrorMessage}");
                }
            }

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(viewModel);
        }

        // Validate Product exists
        var productExists = await _context.Products.AnyAsync(p => p.Id == viewModel.ProductId);
        if (!productExists)
        {
            ModelState.AddModelError("ProductId", "Selected product does not exist.");
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(viewModel);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            // Create Mobile Details
            MobileDetails? mobileDetails = null;
            if (!string.IsNullOrEmpty(viewModel.IMEI) || !string.IsNullOrEmpty(viewModel.Model))
            {
                mobileDetails = new MobileDetails
                {
                    IMEI = viewModel.IMEI,
                    Model = viewModel.Model,
                    ModelNo = viewModel.ModelNo,
                    TotalCapacityGB = viewModel.TotalCapacityGB,
                    AvailableCapacityGB = viewModel.AvailableCapacityGB,
                    ModemFirmwareVersion = viewModel.ModemFirmwareVersion,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MobileDetails.Add(mobileDetails);
                await _context.SaveChangesAsync();
            }

            // Create Operating System Info
            OperatingSystemInfo? osInfo = null;
            if (!string.IsNullOrEmpty(viewModel.OSName))
            {
                osInfo = new OperatingSystemInfo
                {
                    Name = viewModel.OSName,
                    Version = viewModel.OSVersion,
                    BuildNumber = viewModel.BuildNumber,
                    ServicePack = viewModel.ServicePack,
                    ProductId = viewModel.OSProductId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.OperatingSystemInfos.Add(osInfo);
                await _context.SaveChangesAsync();
            }

            // Create Asset State
            var assetState = new AssetState
            {
                Status = (AssetStatusEnum)viewModel.AssetStatus,
                AssociatedTo = viewModel.AssociatedTo,
                Site = viewModel.Site,
                StateComments = viewModel.StateComments,
                UserId = viewModel.UserId,
                Department = viewModel.Department,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetStates.Add(assetState);
            await _context.SaveChangesAsync();

            // Create Network Details
            NetworkDetails? networkDetails = null;
            if (!string.IsNullOrEmpty(viewModel.IPAddress) || !string.IsNullOrEmpty(viewModel.MACAddress))
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

            // Create Mobile Device
            var mobile = new MobileDevice
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
                MobileDetailsId = mobileDetails?.Id,
                OperatingSystemInfoId = osInfo?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user?.Id
            };

            _context.MobileDevices.Add(mobile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mobile device created successfully!";
            return RedirectToAction(nameof(Mobiles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mobile device. Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, ex.StackTrace);

            // If it's a database exception, log inner exception details
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerExceptionType}, Message: {InnerMessage}",
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }

            ModelState.AddModelError("", $"An error occurred while creating the mobile device: {ex.Message}");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(viewModel);
        }
    }

    // GET: Assets/EditMobile/5
    public async Task<IActionResult> EditMobile(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mobile = await _context.MobileDevices
            .Include(m => m.Product)
            .Include(m => m.Vendor)
            .Include(m => m.AssetState)
            .Include(m => m.NetworkDetails)
            .Include(m => m.MobileDetails)
            .Include(m => m.OperatingSystemInfo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mobile == null)
        {
            return NotFound();
        }

        ViewBag.Products = await _context.Products.ToListAsync();
        ViewBag.Vendors = await _context.Vendors.ToListAsync();
        ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

        return View(mobile);
    }

    // POST: Assets/EditMobile/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMobile(int id, MobileDevice model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        ModelState.Remove("Product");
        ModelState.Remove("Vendor");
        ModelState.Remove("AssetState");
        ModelState.Remove("NetworkDetails");
        ModelState.Remove("MobileDetails");
        ModelState.Remove("OperatingSystemInfo");

        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var existingMobile = await _context.MobileDevices
                .Include(m => m.NetworkDetails)
                .Include(m => m.MobileDetails)
                .Include(m => m.OperatingSystemInfo)
                .Include(m => m.AssetState)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMobile == null)
            {
                return NotFound();
            }

            // Update basic asset properties
            existingMobile.Name = model.Name;
            existingMobile.ProductId = model.ProductId;
            existingMobile.SerialNumber = model.SerialNumber;
            existingMobile.AssetTag = model.AssetTag;
            existingMobile.VendorId = model.VendorId;
            existingMobile.PurchaseCost = model.PurchaseCost;
            existingMobile.ExpiryDate = model.ExpiryDate;
            existingMobile.Location = model.Location;
            existingMobile.AcquisitionDate = model.AcquisitionDate;
            existingMobile.WarrantyExpiryDate = model.WarrantyExpiryDate;
            existingMobile.UpdatedAt = DateTime.UtcNow;

            // Update Asset State if provided
            if (model.AssetState != null && existingMobile.AssetState != null)
            {
                existingMobile.AssetState.Status = model.AssetState.Status;
                existingMobile.AssetState.AssociatedTo = model.AssetState.AssociatedTo;
                existingMobile.AssetState.Site = model.AssetState.Site;
                existingMobile.AssetState.StateComments = model.AssetState.StateComments;
                existingMobile.AssetState.UserId = model.AssetState.UserId;
                existingMobile.AssetState.Department = model.AssetState.Department;
                existingMobile.AssetState.UpdatedAt = DateTime.UtcNow;
            }

            // Update Mobile Details if provided
            if (model.MobileDetails != null && existingMobile.MobileDetails != null)
            {
                existingMobile.MobileDetails.IMEI = model.MobileDetails.IMEI;
                existingMobile.MobileDetails.Model = model.MobileDetails.Model;
                existingMobile.MobileDetails.ModelNo = model.MobileDetails.ModelNo;
                existingMobile.MobileDetails.TotalCapacityGB = model.MobileDetails.TotalCapacityGB;
                existingMobile.MobileDetails.AvailableCapacityGB = model.MobileDetails.AvailableCapacityGB;
                existingMobile.MobileDetails.ModemFirmwareVersion = model.MobileDetails.ModemFirmwareVersion;
                existingMobile.MobileDetails.UpdatedAt = DateTime.UtcNow;
            }

            // Update Operating System Info if provided
            if (model.OperatingSystemInfo != null && existingMobile.OperatingSystemInfo != null)
            {
                existingMobile.OperatingSystemInfo.Name = model.OperatingSystemInfo.Name;
                existingMobile.OperatingSystemInfo.Version = model.OperatingSystemInfo.Version;
                existingMobile.OperatingSystemInfo.BuildNumber = model.OperatingSystemInfo.BuildNumber;
                existingMobile.OperatingSystemInfo.ServicePack = model.OperatingSystemInfo.ServicePack;
                existingMobile.OperatingSystemInfo.ProductId = model.OperatingSystemInfo.ProductId;
                existingMobile.OperatingSystemInfo.UpdatedAt = DateTime.UtcNow;
            }

            // Update Network Details if provided
            if (model.NetworkDetails != null && existingMobile.NetworkDetails != null)
            {
                existingMobile.NetworkDetails.IPAddress = model.NetworkDetails.IPAddress;
                existingMobile.NetworkDetails.MACAddress = model.NetworkDetails.MACAddress;
                existingMobile.NetworkDetails.NIC = model.NetworkDetails.NIC;
                existingMobile.NetworkDetails.Network = model.NetworkDetails.Network;
                existingMobile.NetworkDetails.DefaultGateway = model.NetworkDetails.DefaultGateway;
                existingMobile.NetworkDetails.DHCPEnabled = model.NetworkDetails.DHCPEnabled;
                existingMobile.NetworkDetails.DHCPServer = model.NetworkDetails.DHCPServer;
                existingMobile.NetworkDetails.DNSHostname = model.NetworkDetails.DNSHostname;
                existingMobile.NetworkDetails.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mobile device updated successfully!";
            return RedirectToAction(nameof(Mobiles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mobile device");
            ModelState.AddModelError("", "An error occurred while updating the mobile device. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // POST: Assets/DeleteMobile/5
    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMobile(int id)
    {
        try
        {
            var mobile = await _context.MobileDevices
                .Include(m => m.NetworkDetails)
                .Include(m => m.MobileDetails)
                .Include(m => m.OperatingSystemInfo)
                .Include(m => m.AssetState)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mobile == null)
            {
                return NotFound();
            }

            // Delete related records
            if (mobile.NetworkDetails != null)
                _context.NetworkDetails.Remove(mobile.NetworkDetails);

            if (mobile.MobileDetails != null)
                _context.MobileDetails.Remove(mobile.MobileDetails);

            if (mobile.OperatingSystemInfo != null)
                _context.OperatingSystemInfos.Remove(mobile.OperatingSystemInfo);

            if (mobile.AssetState != null)
                _context.AssetStates.Remove(mobile.AssetState);

            _context.MobileDevices.Remove(mobile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mobile device deleted successfully!";
            return RedirectToAction(nameof(Mobiles));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting mobile device");
            TempData["ErrorMessage"] = "An error occurred while deleting the mobile device.";
            return RedirectToAction(nameof(Mobiles));
        }
    }

    #endregion

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

    // #region Printers

    // GET: Assets/Printers
    [Authorize(Roles = "Admin,IT")]
    public async Task<IActionResult> Printers()
    {
        try
        {
            var printers = await _context.Printers
                .Include(p => p.Product)
                .Include(p => p.Vendor)
                .Include(p => p.AssetState)
                .Include(p => p.NetworkDetails)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(printers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading printers");
            TempData["Toast"] = "حدث خطأ في تحميل الطابعات";
            return RedirectToAction("Dashboard");
        }
    }

    // GET: Assets/CreatePrinter
    [Authorize(Roles = "Admin,IT")]
    public async Task<IActionResult> CreatePrinter()
    {
        try
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(new PrinterCreateEditViewModel());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create printer page");
            TempData["Toast"] = "حدث خطأ في تحميل الصفحة";
            return RedirectToAction(nameof(Printers));
        }
    }

    // POST: Assets/CreatePrinter
    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrinter(PrinterCreateEditViewModel viewModel)
    {
        _logger.LogInformation($"CreatePrinter POST called. Name: '{viewModel?.Name}', ProductId: {viewModel?.ProductId}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState is invalid. Errors:");
            foreach (var key in ModelState.Keys)
            {
                var modelState = ModelState[key];
                foreach (var error in modelState.Errors)
                {
                    _logger.LogWarning($"  - {key}: {error.ErrorMessage}");
                }
            }

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(viewModel);
        }

        // Validate Product exists
        var productExists = await _context.Products.AnyAsync(p => p.Id == viewModel.ProductId);
        if (!productExists)
        {
            ModelState.AddModelError("ProductId", "Selected product does not exist.");
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(viewModel);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            // Create Asset State
            var assetState = new AssetState
            {
                Status = (AssetStatusEnum)viewModel.AssetStatus,
                AssociatedTo = viewModel.AssociatedTo,
                Site = viewModel.Site,
                StateComments = viewModel.StateComments,
                UserId = viewModel.UserId,
                Department = viewModel.Department,
                CreatedAt = DateTime.UtcNow
            };
            _context.AssetStates.Add(assetState);
            await _context.SaveChangesAsync();

            // Create Network Details
            NetworkDetails? networkDetails = null;
            if (!string.IsNullOrEmpty(viewModel.IPAddress) || !string.IsNullOrEmpty(viewModel.MACAddress))
            {
                networkDetails = new NetworkDetails
                {
                    IPAddress = viewModel.IPAddress,
                    MACAddress = viewModel.MACAddress,
                    NIC = viewModel.NIC,
                    Network = viewModel.Network,
                    DefaultGateway = viewModel.DefaultGateway,
                    DHCPEnabled = viewModel.DHCPEnabled ?? false,
                    DHCPServer = viewModel.DHCPServer,
                    DNSHostname = viewModel.DNSHostname,
                    CreatedAt = DateTime.UtcNow
                };
                _context.NetworkDetails.Add(networkDetails);
                await _context.SaveChangesAsync();
            }

            // Create Printer
            var printer = new Printer
            {
                Name = viewModel.Name,
                ProductId = viewModel.ProductId,
                SerialNumber = viewModel.SerialNumber,
                AssetTag = viewModel.AssetTag,
                VendorId = viewModel.VendorId,
                PurchaseCost = viewModel.PurchaseCost ?? 0,
                ExpiryDate = viewModel.ExpiryDate,
                Location = viewModel.Location,
                AcquisitionDate = viewModel.AcquisitionDate,
                WarrantyExpiryDate = viewModel.WarrantyExpiryDate,
                AssetStateId = assetState.Id,
                NetworkDetailsId = networkDetails?.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user?.Id
            };

            _context.Printers.Add(printer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Printer created successfully!";
            return RedirectToAction(nameof(Printers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating printer. Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                ex.GetType().Name, ex.Message, ex.StackTrace);

            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerExceptionType}, Message: {InnerMessage}",
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }

            ModelState.AddModelError("", $"An error occurred while creating the printer: {ex.Message}");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(viewModel);
        }
    }

    // GET: Assets/EditPrinter/5
    [Authorize(Roles = "Admin,IT")]
    public async Task<IActionResult> EditPrinter(int id)
    {
        try
        {
            var printer = await _context.Printers
                .Include(p => p.Product)
                .Include(p => p.Vendor)
                .Include(p => p.AssetState)
                .Include(p => p.NetworkDetails)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (printer == null)
            {
                return NotFound();
            }

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(printer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit printer page");
            TempData["Toast"] = "حدث خطأ في تحميل صفحة التعديل";
            return RedirectToAction(nameof(Printers));
        }
    }

    // POST: Assets/EditPrinter/5
    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPrinter(int id, Printer model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        ModelState.Remove("Product");
        ModelState.Remove("Vendor");
        ModelState.Remove("AssetState");
        ModelState.Remove("NetworkDetails");

        if (!ModelState.IsValid)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();
            return View(model);
        }

        try
        {
            var existingPrinter = await _context.Printers
                .Include(p => p.NetworkDetails)
                .Include(p => p.AssetState)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPrinter == null)
            {
                return NotFound();
            }

            // Update basic asset properties
            existingPrinter.Name = model.Name;
            existingPrinter.ProductId = model.ProductId;
            existingPrinter.SerialNumber = model.SerialNumber;
            existingPrinter.AssetTag = model.AssetTag;
            existingPrinter.VendorId = model.VendorId;
            existingPrinter.PurchaseCost = model.PurchaseCost;
            existingPrinter.ExpiryDate = model.ExpiryDate;
            existingPrinter.Location = model.Location;
            existingPrinter.AcquisitionDate = model.AcquisitionDate;
            existingPrinter.WarrantyExpiryDate = model.WarrantyExpiryDate;
            existingPrinter.UpdatedAt = DateTime.UtcNow;

            // Update Asset State if provided
            if (model.AssetState != null)
            {
                if (existingPrinter.AssetState == null)
                {
                    existingPrinter.AssetState = new AssetState
                    {
                        CreatedAt = DateTime.UtcNow
                    };
                }

                existingPrinter.AssetState.Status = model.AssetState.Status;
                existingPrinter.AssetState.AssociatedTo = model.AssetState.AssociatedTo;
                existingPrinter.AssetState.Site = model.AssetState.Site;
                existingPrinter.AssetState.StateComments = model.AssetState.StateComments;
                existingPrinter.AssetState.UserId = model.AssetState.UserId;
                existingPrinter.AssetState.Department = model.AssetState.Department;
                existingPrinter.AssetState.UpdatedAt = DateTime.UtcNow;
            }

            // Update Network Details if provided
            if (model.NetworkDetails != null)
            {
                if (existingPrinter.NetworkDetails == null)
                {
                    existingPrinter.NetworkDetails = new NetworkDetails
                    {
                        CreatedAt = DateTime.UtcNow
                    };
                }

                existingPrinter.NetworkDetails.IPAddress = model.NetworkDetails.IPAddress;
                existingPrinter.NetworkDetails.MACAddress = model.NetworkDetails.MACAddress;
                existingPrinter.NetworkDetails.NIC = model.NetworkDetails.NIC;
                existingPrinter.NetworkDetails.Network = model.NetworkDetails.Network;
                existingPrinter.NetworkDetails.DefaultGateway = model.NetworkDetails.DefaultGateway;
                existingPrinter.NetworkDetails.DHCPEnabled = model.NetworkDetails.DHCPEnabled;
                existingPrinter.NetworkDetails.DHCPServer = model.NetworkDetails.DHCPServer;
                existingPrinter.NetworkDetails.DNSHostname = model.NetworkDetails.DNSHostname;
                existingPrinter.NetworkDetails.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Printer updated successfully!";
            return RedirectToAction(nameof(Printers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating printer");
            ModelState.AddModelError("", "An error occurred while updating the printer. Please try again.");

            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.Vendors = await _context.Vendors.ToListAsync();
            ViewBag.AssetStates = Enum.GetValues(typeof(AssetStatusEnum)).Cast<AssetStatusEnum>().ToList();

            return View(model);
        }
    }

    // POST: Assets/DeletePrinter/5
    [Authorize(Roles = "Admin,IT")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePrinter(int id)
    {
        try
        {
            var printer = await _context.Printers
                .Include(p => p.NetworkDetails)
                .Include(p => p.AssetState)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (printer == null)
            {
                return NotFound();
            }

            // Delete related data
            if (printer.NetworkDetails != null)
            {
                _context.NetworkDetails.Remove(printer.NetworkDetails);
            }

            if (printer.AssetState != null)
            {
                _context.AssetStates.Remove(printer.AssetState);
            }

            _context.Printers.Remove(printer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Printer deleted successfully!";
            return RedirectToAction(nameof(Printers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting printer");
            TempData["Toast"] = "حدث خطأ في حذف الطابعة";
            return RedirectToAction(nameof(Printers));
        }
    }

    // #endregion
}
