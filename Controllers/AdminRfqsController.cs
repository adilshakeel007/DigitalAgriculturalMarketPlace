using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DigitalAgriculturalMarketplace.Controllers;
[SessionAuthorize(UserRoles.Admin)]
public class AdminRfqsController:Controller
{
    private readonly ApplicationDbContext _db;public AdminRfqsController(ApplicationDbContext db)=>_db=db;
    public async Task<IActionResult> Index(){ViewBag.Quotations=await _db.Quotations.Include(x=>x.Seller).Include(x=>x.Items).OrderByDescending(x=>x.CreatedAt).ToListAsync();ViewBag.Allocations=await _db.QuotationAllocations.Include(x=>x.Buyer).Include(x=>x.Seller).Include(x=>x.Order).OrderByDescending(x=>x.CreatedAt).Take(100).ToListAsync();return View(await _db.Rfqs.Include(x=>x.Buyer).Include(x=>x.Items).OrderByDescending(x=>x.CreatedAt).ToListAsync());}
}
