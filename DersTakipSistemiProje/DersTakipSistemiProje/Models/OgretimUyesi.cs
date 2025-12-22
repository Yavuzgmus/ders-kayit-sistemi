using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class OgretimUyesi
{
    public int OgretimUyesiId { get; set; }

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string? Unvan { get; set; }

    public int? BolumId { get; set; }

    public string Sifre { get; set; } = null!;

    public virtual Bolum? Bolum { get; set; }

    public virtual ICollection<DersDetay> DersDetays { get; set; } = new List<DersDetay>();
}
