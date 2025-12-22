using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Bolum
{
    public int BolumId { get; set; }

    public string BolumAd { get; set; } = null!;

    public string? FakulteAd { get; set; }

    public virtual ICollection<Der> Ders { get; set; } = new List<Der>();

    public virtual ICollection<Ogrenci> Ogrencis { get; set; } = new List<Ogrenci>();

    public virtual ICollection<OgretimUyesi> OgretimUyesis { get; set; } = new List<OgretimUyesi>();
}
