using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using project.Data;
using project.Models;

namespace project.Controllers;

public class AccountController : Controller
{
    private readonly SqlDatabaseService _db;

    public AccountController(SqlDatabaseService db)
    {
        _db = db;
    }

    // Requirement #5 (6): Login Page
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Products");
        }
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _db.AuthenticateUser(model.UsernameOrEmail, model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة. يرجى التأكد من اسم المستخدم أو كلمة المرور.");
            return View(model);
        }

        await SignInUserAsync(user, model.RememberMe);
        TempData["SuccessMessage"] = $"مرحباً بك مجدداً يا {user.FullName}! تم تسجيل دخولك بصلاحية ({user.Role}).";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Products");
    }

    // Requirement #7: Create Account Page for User
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Products");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_db.UsernameOrEmailExists(model.Username, model.Email))
        {
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو البريد الإلكتروني مسجل مسبقاً. يرجى اختيار بيانات أخرى.");
            return View(model);
        }

        var newUser = _db.RegisterUser(model);
        await SignInUserAsync(newUser, isPersistent: false);

        TempData["SuccessMessage"] = $"تم إنشاء حسابك بنجاح يا {newUser.FullName}! صلاحية حسابك هي (User - قراءة وتصفح فقط).";
        return RedirectToAction("Index", "Products");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "تم تسجيل الخروج بنجاح.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
