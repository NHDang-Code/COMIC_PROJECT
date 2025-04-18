using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin, Translator")]
    public class LanguageController : Controller
    {
        public IActionResult LanguageSettings()
        {
            return View();
        }


        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            var supportedCultures = new[] {"vi-VN", "zh-CN", "ko-KR" };


            if (string.IsNullOrEmpty(culture) || !supportedCultures.Contains(culture))
            {
                culture = "vi-VN"; 
            }


            var cultureCookie = new RequestCulture(culture);
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(cultureCookie),
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );

            if (Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Profile");
            }
        }

    }
}
