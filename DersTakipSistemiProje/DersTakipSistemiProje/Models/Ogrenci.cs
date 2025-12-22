using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Ogrenci
{
    public int OgrenciId { get; set; } 
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public DateTime? DogumTarihi { get; set; } 
    public int? BolumId { get; set; }
    public string Sifre { get; set; } = null!;


    //Öğrenci bilgileri için gerekli olan getter setterlar 
    public string? Telefon { get; set; }
    public string? EPosta { get; set; }
    public string? Adres { get; set; }
    public int? Sinif { get; set; }
    public DateTime? KayitTarihi { get; set; } 





    public virtual Bolum? Bolum { get; set; }
    public virtual ICollection<DersKayit> DersKayits { get; set; } = new List<DersKayit>();
    public virtual ICollection<Notlar> Notlars { get; set; } = new List<Notlar>();
}
