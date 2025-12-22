using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class DersDetay
{
    public int DersDetayId { get; set; }

    public string DersKod { get; set; } = null!;

    public int DonemId { get; set; }

    public int OgretimUyesiId { get; set; }

    public int Kontenjan { get; set; }

    public virtual ICollection<DersKayit> DersKayits { get; set; } = new List<DersKayit>();

    public virtual Der DersKodNavigation { get; set; } = null!;

    public virtual Donem Donem { get; set; } = null!;

    public virtual ICollection<Notlar> Notlars { get; set; } = new List<Notlar>();

    public virtual OgretimUyesi OgretimUyesi { get; set; } = null!;
}
