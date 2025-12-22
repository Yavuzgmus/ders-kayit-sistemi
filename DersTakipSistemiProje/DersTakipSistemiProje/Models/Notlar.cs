using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Notlar
{
    public int OgrenciId { get; set; }

    public int DersDetayId { get; set; }

    public decimal? Vize { get; set; }

    public decimal? Final { get; set; }

    public string? HarfNotu { get; set; }

    public virtual DersDetay DersDetay { get; set; } = null!;

    public virtual Ogrenci Ogrenci { get; set; } = null!;
}
