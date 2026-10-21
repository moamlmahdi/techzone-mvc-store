using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.Models;

namespace project.Controllers;

public class HomeController : Controller
{
    private readonly SqlDatabaseService _db;

    public HomeController(SqlDatabaseService db)
    {
        _db = db;
    }

    // Requirement #5: Home page answering the 4 questions: Who, What, Why, Where
    public IActionResult Index()
    {
        var products = _db.GetAllProducts();
        ViewBag.TotalProducts = products.Count;
        ViewBag.FeaturedProducts = products.Take(3).ToList();
        return View();
    }

    // Requirement #6: Project Plan showing Target Audience, Goals & Objectives, and Structure Used
    public IActionResult ProjectPlan()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
