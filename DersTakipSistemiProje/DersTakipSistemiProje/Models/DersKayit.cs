using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class DersKayit
{
    public int OgrenciId { get; set; }

    public int DersDetayId { get; set; }

    public DateOnly KayitTarihi { get; set; }

    public virtual DersDetay DersDetay { get; set; } = null!;

    public virtual Ogrenci Ogrenci { get; set; } = null!;
}
