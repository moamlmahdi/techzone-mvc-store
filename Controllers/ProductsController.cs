using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.Models;

namespace project.Controllers;

// Requirement #3: Admin has Add, Edit, Delete privileges; User has Read-only privilege.
[Authorize]
public class ProductsController : Controller
{
    private readonly SqlDatabaseService _db;

    public ProductsController(SqlDatabaseService db)
    {
        _db = db;
    }

    // READ: Accessible by both Admin and User (and visitors if needed, here we allow Anonymous or logged in User/Admin so both can read)
    [AllowAnonymous]
    public IActionResult Index(string? search, string? category)
    {
        var products = _db.GetAllProducts(search, category);
        ViewBag.Categories = _db.GetCategories();
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentCategory = category;
        return View(products);
    }

    // READ Details: Accessible by User and Admin
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var product = _db.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // CREATE (Add): Admin Only
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Product
        {
            ImageUrl = "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?auto=format&fit=crop&w=800&q=80",
            StockQuantity = 10
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        product.CreatedAt = DateTime.Now;
        _db.CreateProduct(product);
        TempData["SuccessMessage"] = $"تمت إضافة المنتج «{product.Name}» إلى قاعدة بيانات SQL بنجاح!";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE (Edit): Admin Only
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _db.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        _db.UpdateProduct(product);
        TempData["SuccessMessage"] = $"تم تحديث بيانات المنتج «{product.Name}» في قاعدة بيانات SQL بنجاح!";
        return RedirectToAction(nameof(Index));
    }

    // DELETE: Admin Only
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var product = _db.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _db.GetProductById(id);
        if (product != null)
        {
            _db.DeleteProduct(id);
            TempData["SuccessMessage"] = $"تم حذف المنتج «{product.Name}» نهائياً من قاعدة بيانات SQL.";
        }
        return RedirectToAction(nameof(Index));
    }
}
