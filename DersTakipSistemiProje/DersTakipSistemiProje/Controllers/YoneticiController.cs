using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using DersTakipSistemiProje.Models;

namespace DersTakipSistemiProje.Controllers
{
    public class YoneticiController : Controller
    {
        private readonly DersKayitSistemiContext _context;

        public YoneticiController(DersKayitSistemiContext context)
        {
            _context = context;
        }

        // 1. ÖĞRENCİ YÖNETİMİ (LİSTELE, EKLE, DÜZENLE, SİL)
        public IActionResult OgrenciListesi()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            var ogrenciler = _context.Ogrencis.Include(o => o.Bolum).ToList();
            return View(ogrenciler);
        }

        // ÖĞRENCİ EKLEME (GET)
        [HttpGet]
        public IActionResult OgrenciEkle()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            // Dropdown için bölümleri gönder
            ViewBag.Bolumler = _context.Bolums.ToList();
            return View();
        }

        // ÖĞRENCİ EKLEME (POST) 
        [HttpPost]
        public IActionResult OgrenciEkle(Ogrenci p)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            try
            {
                // Şifre boşsa varsayılan 123 ata
                if (string.IsNullOrEmpty(p.Sifre)) p.Sifre = "123";

                _context.Ogrencis.Add(p);
                _context.SaveChanges();

                TempData["Mesaj"] = "Yeni öğrenci başarıyla oluşturuldu.";
                return RedirectToAction("OgrenciListesi");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Hata oluştu: " + ex.Message;
                ViewBag.Bolumler = _context.Bolums.ToList();
                return View(p);
            }
        }

        // ÖĞRENCİ DÜZENLEME (GET)
        [HttpGet]
        public IActionResult OgrenciDuzenle(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            var ogrenci = _context.Ogrencis.Find(id);
            if (ogrenci == null) return RedirectToAction("OgrenciListesi");

            // Bölüm değiştirme ihtimaline karşı bölümleri de gönderelim
            ViewBag.Bolumler = _context.Bolums.ToList();

            return View(ogrenci);
        }

        // ÖĞRENCİ DÜZENLEME (POST)
        [HttpPost]
        public IActionResult OgrenciDuzenle(Ogrenci p)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            var mevcut = _context.Ogrencis.Find(p.OgrenciId);
            if (mevcut != null)
            {
                mevcut.Ad = p.Ad;
                mevcut.Soyad = p.Soyad;
                mevcut.Sinif = p.Sinif;
                mevcut.BolumId = p.BolumId; // Bölüm değişikliği yapılacaksa diye bunu ekledim

                mevcut.EPosta = p.EPosta;
                mevcut.Telefon = p.Telefon;
                mevcut.Adres = p.Adres;

                _context.SaveChanges();
                TempData["Mesaj"] = "Öğrenci bilgileri güncellendi.";
            }
            return RedirectToAction("OgrenciListesi");
        }

        // ÖĞRENCİ SİLME
        public IActionResult OgrenciSil(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            var ogrenci = _context.Ogrencis.Find(id);
            if (ogrenci != null)
            {
                // Önce ilişkili verileri (Notlar ve Ders Kayıtları) temizle
                var notlar = _context.Notlars.Where(x => x.OgrenciId == id).ToList();
                _context.Notlars.RemoveRange(notlar);

                var kayitlar = _context.DersKayits.Where(x => x.OgrenciId == id).ToList();
                _context.DersKayits.RemoveRange(kayitlar);

                // Sonra öğrenciyi sil
                _context.Ogrencis.Remove(ogrenci);
                _context.SaveChanges();
                TempData["Mesaj"] = "Öğrenci ve tüm kayıtları silindi.";
            }
            return RedirectToAction("OgrenciListesi");
        }


        // 2. ÖĞRETİM ÜYESİ YÖNETİMİ

        public IActionResult OgretimUyesiListesi()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            var hocalar = _context.OgretimUyesis.Include(x => x.Bolum).ToList();
            return View(hocalar);
        }

        [HttpGet]
        public IActionResult OgretimUyesiEkle()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            ViewBag.Bolumler = _context.Bolums.ToList();
            return View();
        }



        [HttpPost]
        public IActionResult OgretimUyesiEkle(OgretimUyesi hoca)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            try
            {
                var varMi = _context.OgretimUyesis.Find(hoca.OgretimUyesiId);
                if (varMi != null)
                {
                    ViewBag.Hata = "Bu ID numarası zaten kullanılıyor.";
                    ViewBag.Bolumler = _context.Bolums.ToList();
                    return View(hoca);
                }

                _context.OgretimUyesis.Add(hoca);
                _context.SaveChanges();
                return RedirectToAction("OgretimUyesiListesi");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = ex.Message;
                ViewBag.Bolumler = _context.Bolums.ToList();
                return View(hoca);
            }
        }

        public IActionResult OgretimUyesiSil(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            var hoca = _context.OgretimUyesis.Find(id);
            if (hoca != null)
            {
                try
                {
                    _context.OgretimUyesis.Remove(hoca);
                    _context.SaveChanges();
                }
                catch
                {
                    TempData["Hata"] = "Bu hocanın üzerinde dersler var, önce dersleri silin.";
                }
            }
            return RedirectToAction("OgretimUyesiListesi");
        }



        // DERS KATALOĞU (TÜM DERSLER)
        public IActionResult DersListesi()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            var dersler = _context.Ders.Include(d => d.Bolum).ToList();
            return View(dersler);
        }

        [HttpGet]
        public IActionResult DersEkle()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            ViewBag.Bolumler = _context.Bolums.ToList();
            ViewBag.Hocalar = _context.OgretimUyesis.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult DersEkle(Der ders)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            try
            {
                _context.Ders.Add(ders);
                _context.SaveChanges();
                TempData["Mesaj"] = "Ders kataloğa eklendi. Şimdi 'Ders Aç' menüsünden bu dersi döneme atayabilirsiniz.";
                return RedirectToAction("DersAc");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = ex.Message;
                ViewBag.Bolumler = _context.Bolums.ToList();
                ViewBag.Hocalar = _context.OgretimUyesis.ToList();
                return View();
            }
        }

        public IActionResult DersSil(string id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            var ders = _context.Ders.Find(id);
            if (ders != null)
            {
                try
                {
                    _context.Ders.Remove(ders);
                    _context.SaveChanges();
                }
                catch
                {
                    TempData["Hata"] = "Bu ders açılmış veya öğrencisi var, silinemez.";
                }
            }
            return RedirectToAction("DersListesi");
        }


        // 4. DERS AÇMA (DÖNEMLİK AKTİF DERSLER)
        public IActionResult AcilanDersler()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            var acilanlar = _context.DersDetays
                .Include(x => x.DersKodNavigation)
                .Include(x => x.OgretimUyesi)
                .Include(x => x.Donem)
                .ToList();

            return View(acilanlar);
        }

        [HttpGet]
        public IActionResult DersAc()
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");
            ViewBag.Dersler = _context.Ders.ToList();
            ViewBag.Donemler = _context.Donems.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult DersAc(DersDetay detay)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login", "Account");

            try
            {
                // Seçilen dersin kataloğundaki hocayı bulup atayalım
                var anaDers = _context.Ders.Find(detay.DersKod);
                if (anaDers != null && anaDers.OgretimUyesiId != null)
                {
                    detay.OgretimUyesiId = (int)anaDers.OgretimUyesiId;
                }
                else
                {
                    ViewBag.Hata = "Seçilen dersin bir hocası yok. Önce Ders Listesinden hocayı atayın.";
                    ViewBag.Dersler = _context.Ders.ToList();
                    ViewBag.Donemler = _context.Donems.ToList();
                    return View(detay);
                }

                // ID çakışmaması için rastgele ID veya Identity 
                if (detay.DersDetayId == 0) detay.DersDetayId = new Random().Next(1000, 99999);

                _context.DersDetays.Add(detay);
                _context.SaveChanges();
                return RedirectToAction("AcilanDersler");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = ex.Message;
                ViewBag.Dersler = _context.Ders.ToList();
                ViewBag.Donemler = _context.Donems.ToList();
                return View();
            }
        }
    }
}