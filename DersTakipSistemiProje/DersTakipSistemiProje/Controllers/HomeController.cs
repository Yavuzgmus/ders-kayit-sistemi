using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using DersTakipSistemiProje.Models; 
namespace DersTakipSistemiProje.Controllers
{
    public class HomeController : Controller
    {
        // VERİTABANI BAĞLANTISINI TANIMLIYORUZ
        private readonly DersKayitSistemiContext _context;

        public HomeController(DersKayitSistemiContext context)
        {
            _context = context;
        }

        // ANA SAYFA METODU
        public IActionResult Index()
        {
            // Oturum kontrolü
            var rol = HttpContext.Session.GetString("UserRole");
            var adSoyad = HttpContext.Session.GetString("AdSoyad");
            var userId = HttpContext.Session.GetInt32("UserID");

            // Eğer giriş yapılmamışsa Giriş Ekranına at
            if (string.IsNullOrEmpty(rol)) return RedirectToAction("Login", "Account");

            ViewBag.Role = rol;
            ViewBag.AdSoyad = adSoyad;

            if (rol == "Ogrenci" && userId != null)
            {
                var ogrenci = _context.Ogrencis
                    .Include(x => x.Bolum)
                    .FirstOrDefault(x => x.OgrenciId == userId);

                return View(ogrenci); // Modeli View'a gönder
            }

            return View();
        }
    }
}