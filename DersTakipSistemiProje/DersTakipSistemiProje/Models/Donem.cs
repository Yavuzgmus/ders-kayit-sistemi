using System;
using System.Collections.Generic;

namespace DersTakipSistemiProje.Models;

public partial class Donem
{
    public int DonemId { get; set; }

    public int Yil { get; set; }

    public string Yariyil { get; set; } = null!;

    public virtual ICollection<DersDetay> DersDetays { get; set; } = new List<DersDetay>();
}
