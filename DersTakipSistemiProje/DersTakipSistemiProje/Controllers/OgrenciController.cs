using DersTakipSistemiProje.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DersTakipSistemiProje.Controllers
{
    public class OgrenciController : Controller
    {
        private readonly DersKayitSistemiContext _context;

        public OgrenciController(DersKayitSistemiContext context)
        {
            _context = context;
        }

        // KİŞİSEL BİLGİLER
        [HttpGet]
        public IActionResult KisiselBilgiler()
        {
            if (HttpContext.Session.GetString("UserRole") != "Ogrenci") return RedirectToAction("Login", "Account");
            int? ogrenciId = HttpContext.Session.GetInt32("UserID");

            var ogrenci = _context.Ogrencis
                .Include(x => x.Bolum)
                .FirstOrDefault(x => x.OgrenciId == ogrenciId);

            if (ogrenci == null) return RedirectToAction("Login", "Account");

            return View(ogrenci);
        }

        [HttpPost]
        public IActionResult KisiselBilgiler(Ogrenci gelenVeri)
        {
            if (HttpContext.Session.GetString("UserRole") != "Ogrenci") return RedirectToAction("Login", "Account");
            var mevcutOgrenci = _context.Ogrencis.Find(gelenVeri.OgrenciId);
            if (mevcutOgrenci != null)
            {
                mevcutOgrenci.Telefon = gelenVeri.Telefon;
                mevcutOgrenci.EPosta = gelenVeri.EPosta;
                mevcutOgrenci.Adres = gelenVeri.Adres;
                _context.SaveChanges();
                TempData["BasariMesaji"] = "Bilgileriniz güncellendi.";
            }
            return RedirectToAction("Index", "Home");
        }

        // DERS SEÇİM EKRANI
        [HttpGet]
        public IActionResult DersSecimi()
        {
            if (HttpContext.Session.GetString("UserRole") != "Ogrenci") return RedirectToAction("Login", "Account");
            int? ogrenciId = HttpContext.Session.GetInt32("UserID");
            var ogrenci = _context.Ogrencis.Find(ogrenciId);
            if (ogrenci == null) return RedirectToAction("Login", "Account");

            // Bölüm derslerini getir
            var acilanDersler = _context.DersDetays
                .Include(d => d.DersKodNavigation)
                .Include(d => d.OgretimUyesi)
                .Where(d => d.DersKodNavigation.BolumId == ogrenci.BolumId)
                .ToList();

            // Zaten kayıtlı olanları bul
            var kayitliDerslerID = _context.DersKayits
                .Where(k => k.OgrenciId == ogrenciId)
                .Select(k => k.DersDetayId)
                .ToList();

            ViewBag.KayitliDersler = kayitliDerslerID;
            return View(acilanDersler);
        }

        // 3. DERS KAYDI VE NOT ÇİZELGESİ OLUŞTURMA
        [HttpPost]
        public IActionResult DersKayitTamamla(List<int> secilenDersler)
        {
            if (HttpContext.Session.GetString("UserRole") != "Ogrenci") return RedirectToAction("Login", "Account");

            int? ogrenciId = HttpContext.Session.GetInt32("UserID");
            if (ogrenciId == null) return RedirectToAction("Login", "Account");

            if (secilenDersler != null && secilenDersler.Count > 0)
            {
                foreach (var dersDetayId in secilenDersler)
                {
                    // Ders Kaydı Kontrolü ve Ekleme
                    bool kayitVarMi = _context.DersKayits.Any(k => k.OgrenciId == ogrenciId && k.DersDetayId == dersDetayId);
                    if (!kayitVarMi)
                    {
                        var yeniKayit = new DersKayit
                        {
                            OgrenciId = (int)ogrenciId,
                            DersDetayId = dersDetayId,
                            KayitTarihi = DateOnly.FromDateTime(DateTime.Now)
                        };
                        _context.DersKayits.Add(yeniKayit);
                    }

                    // Not Satırı Kontrolü ve Ekleme (BAĞIMSIZ KONTROL)
                    // Ders kaydı önceden olsa bile not satırı yoksa eklemeliyiz.
                    bool notVarMi = _context.Notlars.Any(n => n.OgrenciId == ogrenciId && n.DersDetayId == dersDetayId);
                    if (!notVarMi)
                    {
                        var yeniNot = new Notlar
                        {
                            OgrenciId = (int)ogrenciId,
                            DersDetayId = dersDetayId,
                            Vize = null,
                            Final = null,
                            HarfNotu = "NA"
                        };
                        _context.Notlars.Add(yeniNot);
                    }
                }

                _context.SaveChanges();
                TempData["BasariMesaji"] = "Dersleriniz başarıyla güncellendi.";
                return RedirectToAction("DonemNotlari");
            }
            else
            {
                TempData["Hata"] = "Lütfen en az bir ders seçiniz.";
                return RedirectToAction("DersSecimi");
            }
        }


        //DÖNEM NOTLARI
        public IActionResult DonemNotlari()
        {
            if (HttpContext.Session.GetString("UserRole") != "Ogrenci") return RedirectToAction("Login", "Account");
            int? ogrenciId = HttpContext.Session.GetInt32("UserID");

            var notlar = _context.Notlars
                .Include(n => n.DersDetay)
                .Include(n => n.DersDetay.DersKodNavigation) // Ders Adı buradan gelir
                .Include(n => n.DersDetay.OgretimUyesi)      // Hoca Adı buradan gelir
                .Where(n => n.OgrenciId == ogrenciId)
                .ToList();

            // Ortalama Hesabı
            double toplamPuan = 0;
            double toplamKredi = 0;

            foreach (var n in notlar)
            {
                // Sadece notu girilmişleri (NA olmayanları) ortalamaya kat
                if (n.HarfNotu != "NA" && n.HarfNotu != null)
                {
                    double katsayi = HarfNotuCevir(n.HarfNotu);
                    double kredi = (double)n.DersDetay.DersKodNavigation.Kredi;

                    toplamPuan += (katsayi * kredi);
                    toplamKredi += kredi;
                }
            }

            ViewBag.Yano = toplamKredi > 0 ? (toplamPuan / toplamKredi).ToString("0.00") : "0.00";

            return View(notlar);
        }

        private double HarfNotuCevir(string harf)
        {
            switch (harf)
            {
                case "AA": return 4.0;
                case "BA": return 3.5;
                case "BB": return 3.0;
                case "CB": return 2.5;
                case "CC": return 2.0;
                case "DC": return 1.5;
                case "DD": return 1.0;
                case "FF": return 0.0;
                default: return 0.0;
            }
        }

        public IActionResult Transkript()
        {
            return RedirectToAction("DonemNotlari");
        }
    }
}