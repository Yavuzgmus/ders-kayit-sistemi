using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Yonetici
{
    public int YoneticiId { get; set; }

    public string KullaniciAdi { get; set; } = null!;

    public string Sifre { get; set; } = null!;
}
