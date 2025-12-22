using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using DersTakipSistemiProje.Models; 

namespace DersTakipSistemiProje.Controllers
{
    public class AccountController : Controller
    {
        private readonly DersKayitSistemiContext _context;

        public AccountController(DersKayitSistemiContext context)
        {
            _context = context;
        }


        // GİRİŞ EKRANI (GET)
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yapmışsa, direkt anasayfaya yönlendir
            if (HttpContext.Session.GetString("UserRole") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        // GİRİŞ İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Login(string kullaniciAdi, string sifre)
        {
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                ViewBag.Hata = "Lütfen kullanıcı adı ve şifre giriniz.";
                return View();
            }

            // 1. ADIM: YÖNETİCİ (ADMIN) KONTROLÜ
            // Yönetici girişi 'admin' gibi metin tabanlıdır.
            var yonetici = _context.Yoneticis.FirstOrDefault(x => x.KullaniciAdi == kullaniciAdi && x.Sifre == sifre);
            if (yonetici != null)
            {
                HttpContext.Session.SetString("UserRole", "Yonetici");
                HttpContext.Session.SetInt32("UserID", yonetici.YoneticiId);
                HttpContext.Session.SetString("AdSoyad", yonetici.KullaniciAdi);
                return RedirectToAction("Index", "Home");
            }

            if (int.TryParse(kullaniciAdi, out int id))
            {
                var hoca = _context.OgretimUyesis.FirstOrDefault(x => x.OgretimUyesiId == id && x.Sifre == sifre);
                if (hoca != null)
                {
                    HttpContext.Session.SetString("UserRole", "OgretimUyesi");
                    HttpContext.Session.SetInt32("UserID", hoca.OgretimUyesiId);
                    HttpContext.Session.SetString("AdSoyad", hoca.Unvan + " " + hoca.Ad + " " + hoca.Soyad);
                    return RedirectToAction("Index", "Home");
                }

                var ogrenci = _context.Ogrencis.FirstOrDefault(x => x.OgrenciId == id && x.Sifre == sifre);
                if (ogrenci != null)
                {
                    HttpContext.Session.SetString("UserRole", "Ogrenci");
                    HttpContext.Session.SetInt32("UserID", ogrenci.OgrenciId);
                    HttpContext.Session.SetString("AdSoyad", ogrenci.Ad + " " + ogrenci.Soyad);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Hata = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }


        // ÇIKIŞ YAPMA (Logout)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        // ŞİFRE ÖĞRENME (AJAX - POPUP İÇİN)
        [HttpPost]
        public JsonResult SifreOgrenAjax(int id)
        {
            // Öğrenci Ara
            var ogrenci = _context.Ogrencis.Find(id);
            if (ogrenci != null)
            {
                return Json(new { success = true, mesaj = $"{ogrenci.Ad} {ogrenci.Soyad}, Şifreniz: {ogrenci.Sifre}" });
            }

            // Hoca Ara
            var hoca = _context.OgretimUyesis.Find(id);
            if (hoca != null)
            {
                return Json(new { success = true, mesaj = $"Sayın {hoca.Unvan} {hoca.Ad} {hoca.Soyad}, Şifreniz: {hoca.Sifre}" });
            }

            // Yönetici Ara 
            var admin = _context.Yoneticis.Find(id);
            if(admin != null)
            {
                 return Json(new { success = true, mesaj = $"Yönetici, Şifreniz: {admin.Sifre}" });
            }

            return Json(new { success = false, mesaj = "Bu numaraya ait bir kayıt bulunamadı." });
        }


        // ŞİFREMİ UNUTTUM EKRANI (GET)
        [HttpGet]
        public IActionResult SifremiUnuttum()
        {
            return View();
        }

        // ŞİFRE SORGULAMA (POST)
        [HttpPost]
        public IActionResult SifremiUnuttum(int? id)
        {
            if (id == null)
            {
                ViewBag.Hata = "Lütfen numaranızı giriniz.";
                return View();
            }

            var ogrenci = _context.Ogrencis.Find(id);
            if (ogrenci != null)
            {
                ViewBag.Mesaj = $"{ogrenci.Ad} {ogrenci.Soyad}, Şifreniz: {ogrenci.Sifre}";
                return View();
            }

            var hoca = _context.OgretimUyesis.Find(id);
            if (hoca != null)
            {
                ViewBag.Mesaj = $"Sayın {hoca.Unvan} {hoca.Ad} {hoca.Soyad}, Şifreniz: {hoca.Sifre}";
                return View();
            }

            ViewBag.Hata = "Bu numaraya ait kayıt bulunamadı.";
            return View();
        }
    }
}