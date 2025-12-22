using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Der
{
    public string DersKod { get; set; } = null!;

    public string DersAd { get; set; } = null!;

    public decimal Kredi { get; set; }

    public int? BolumId { get; set; }

    public int? OgretimUyesiId { get; set; }
    public string? Tur { get; set; }


    public virtual OgretimUyesi? OgretimUyesi { get; set; }

    public virtual Bolum? Bolum { get; set; }

    public virtual ICollection<DersDetay> DersDetays { get; set; } = new List<DersDetay>();

    public virtual ICollection<Der> AnaDersKods { get; set; } = new List<Der>();

    public virtual ICollection<Der> OnkosulDersKods { get; set; } = new List<Der>();
}
