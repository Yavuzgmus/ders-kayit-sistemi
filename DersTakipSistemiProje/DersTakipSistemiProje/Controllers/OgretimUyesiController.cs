using DersTakipSistemiProje.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DersTakipSistemiProje.Controllers
{
    public class OgretimUyesiController : Controller
    {
        private readonly DersKayitSistemiContext _context;

        public OgretimUyesiController(DersKayitSistemiContext context)
        {
            _context = context;
        }


        // VERİLEN DERSLER LİSTESİ
        public IActionResult VerilenDersler()
        {
            // 1. Yetki Kontrolü
            if (HttpContext.Session.GetString("UserRole") != "OgretimUyesi") return RedirectToAction("Login", "Account");

            int? hocaId = HttpContext.Session.GetInt32("UserID");

            // 2. Veriyi Çek
            var dersler = _context.DersDetays
                .Include(d => d.DersKodNavigation)
                .Where(d => d.OgretimUyesiId == hocaId)
                .ToList();

            // 3. View'a GÖNDER 
            return View(dersler);
        }
  
        // 2. NOT GİRİŞ EKRANI (LİSTELEME)
        [HttpGet]
        public IActionResult NotGiris(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "OgretimUyesi") return RedirectToAction("Login", "Account");

            // İlgili dersin not listesini ve öğrenci bilgilerini getir
            var notListesi = _context.Notlars
                .Include(n => n.Ogrenci)
                .Include(n => n.DersDetay)
                .ThenInclude(d => d.DersKodNavigation)
                .Where(n => n.DersDetayId == id)
                .ToList();

            // Ders adını başlıkta göstermek için ViewBag'e atılır
            var dersBilgisi = _context.DersDetays
                .Include(d => d.DersKodNavigation)
                .FirstOrDefault(d => d.DersDetayId == id);

            ViewBag.DersAdi = dersBilgisi?.DersKodNavigation?.DersAd ?? "Ders";
            ViewBag.DersDetayId = id;

            return View(notListesi);
        }

        // 3. NOT KAYDETME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult NotKaydet(int id, int OgrenciId, int? Vize, int? Final)
        {
            // Yetki kontrolü
            if (HttpContext.Session.GetString("UserRole") != "OgretimUyesi") return RedirectToAction("Login", "Account");

            // İlgili satırı bul
            var notKaydi = _context.Notlars.FirstOrDefault(x => x.DersDetayId == id && x.OgrenciId == OgrenciId);

            if (notKaydi != null)
            {
                // 2. Gelen verileri işle (Boş gelirse null yap)
                notKaydi.Vize = Vize;
                notKaydi.Final = Final;

                // 3. Harf Notu Hesaplama
                // Final girilmediyse harf notu hesaplanmaz
                if (Final == null)
                {
                    notKaydi.HarfNotu = "NA";
                }
                else
                {
                    // Vize girilmemişse 0 kabul et
                    double vizeNotu = (double)(Vize ?? 0);
                    double finalNotu = (double)Final;

                    double ortalama = (vizeNotu * 0.4) + (finalNotu * 0.6);

                    if (finalNotu < 35) notKaydi.HarfNotu = "FF"; 
                    else if (ortalama >= 90) notKaydi.HarfNotu = "AA";
                    else if (ortalama >= 85) notKaydi.HarfNotu = "BA";
                    else if (ortalama >= 80) notKaydi.HarfNotu = "BB";
                    else if (ortalama >= 75) notKaydi.HarfNotu = "CB";
                    else if (ortalama >= 70) notKaydi.HarfNotu = "CC";
                    else if (ortalama >= 65) notKaydi.HarfNotu = "DC";
                    else if (ortalama >= 60) notKaydi.HarfNotu = "DD";
                    else if (ortalama >= 50) notKaydi.HarfNotu = "FD";
                    else notKaydi.HarfNotu = "FF";
                }

                _context.SaveChanges();
                TempData["Mesaj"] = "Not başarıyla kaydedildi.";
            }
            else
            {
                TempData["Hata"] = "Hata: Öğrenci kaydı bulunamadı.";
            }

            // Listeye geri dön
            return RedirectToAction("NotGiris", new { id = id });
        }
    }
}