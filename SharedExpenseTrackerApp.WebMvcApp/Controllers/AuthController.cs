using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.Domain.Features.Auth;
using SharedExpenseTrackerApp.Domain.Models.Auth;
using SharedExpenseTrackerApp.WebMvcApp.Filters;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;

namespace SharedExpenseTrackerApp.WebMvcApp.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ─── GET /Auth/Login ─────────────────────────────────────────────────────
    public IActionResult Login()
    {
        if (HttpContext.Session.IsAuthenticated())
            return RedirectToAction("Index", "Groups");

        return View();
    }

    // ─── POST /Auth/Login ────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string phoneNumber, string password)
    {
        var result = await _authService.LoginAsync(new LoginReqModel
        {
            PhoneNumber = phoneNumber,
            Password = password
        });

        if (!result.IsSuccess)
        {
            ViewData["Error"] = result.Message;
            ViewData["PhoneNumber"] = phoneNumber;
            return View();
        }

        // Store session
        HttpContext.Session.SetUserId(result.Data!.Id);
        HttpContext.Session.SetFullName(result.Data.FullName);
        HttpContext.Session.SetPhoneNumber(result.Data.PhoneNumber);

        return RedirectToAction("Index", "Groups");
    }

    // ─── GET /Auth/Register ──────────────────────────────────────────────────
    public IActionResult Register()
    {
        if (HttpContext.Session.IsAuthenticated())
            return RedirectToAction("Index", "Groups");

        return View();
    }

    // ─── POST /Auth/Register ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string phoneNumber, string password)
    {
        var result = await _authService.RegisterAsync(new RegisterReqModel
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Password = password
        });

        if (!result.IsSuccess)
        {
            ViewData["Error"] = result.Message;
            ViewData["FullName"] = fullName;
            ViewData["PhoneNumber"] = phoneNumber;
            return View();
        }

        ViewData["Success"] = "Registration successful! Please log in.";
        return RedirectToAction("Login");
    }

    // ─── POST /Auth/Logout ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.ClearUser();
        return RedirectToAction("Login");
    }
}
