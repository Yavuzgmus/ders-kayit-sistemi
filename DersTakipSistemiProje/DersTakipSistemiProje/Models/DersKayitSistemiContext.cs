using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DersTakipSistemiProje.Models;

public partial class DersKayitSistemiContext : DbContext
{
    public DersKayitSistemiContext()
    {
    }

    public DersKayitSistemiContext(DbContextOptions<DersKayitSistemiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bolum> Bolums { get; set; }
    public virtual DbSet<Der> Ders { get; set; }
    public virtual DbSet<DersDetay> DersDetays { get; set; }
    public virtual DbSet<DersKayit> DersKayits { get; set; }
    public virtual DbSet<Donem> Donems { get; set; }
    public virtual DbSet<Notlar> Notlars { get; set; }
    public virtual DbSet<Ogrenci> Ogrencis { get; set; }
    public virtual DbSet<OgretimUyesi> OgretimUyesis { get; set; }
    public virtual DbSet<Yonetici> Yoneticis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DersKayitSistemi;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bolum>(entity =>
        {
            entity.ToTable("Bolum");
            entity.Property(e => e.BolumId).HasColumnName("Bolum_ID").ValueGeneratedNever();
            entity.Property(e => e.BolumAd).HasMaxLength(100).HasColumnName("Bolum_Ad");
            entity.Property(e => e.FakulteAd).HasMaxLength(100).HasColumnName("Fakulte_Ad");
        });

        modelBuilder.Entity<Der>(entity =>
        {
            entity.HasKey(e => e.DersKod);
            entity.ToTable("Ders"); 

            entity.Property(e => e.DersKod).HasMaxLength(10).HasColumnName("Ders_Kod");
            entity.Property(e => e.DersAd).HasMaxLength(150).HasColumnName("Ders_Ad");
            entity.Property(e => e.Kredi).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.BolumId).HasColumnName("Bolum_ID"); 
            entity.Property(e => e.Tur).HasMaxLength(10).HasDefaultValueSql("('Zorunlu')");


            entity.HasOne(d => d.Bolum).WithMany(p => p.Ders)
                .HasForeignKey(d => d.BolumId)
                .HasConstraintName("FK_Ders_Bolum");

            entity.HasMany(d => d.AnaDersKods).WithMany(p => p.OnkosulDersKods)
                .UsingEntity<Dictionary<string, object>>(
                    "Onkosul",
                    r => r.HasOne<Der>().WithMany().HasForeignKey("AnaDersKod").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Onkosul_AnaDers"),
                    l => l.HasOne<Der>().WithMany().HasForeignKey("OnkosulDersKod").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Onkosul_GerekenDers"),
                    j =>
                    {
                        j.HasKey("AnaDersKod", "OnkosulDersKod");
                        j.ToTable("Onkosul");
                        j.IndexerProperty<string>("AnaDersKod").HasMaxLength(10).HasColumnName("Ana_Ders_Kod");
                        j.IndexerProperty<string>("OnkosulDersKod").HasMaxLength(10).HasColumnName("Onkosul_Ders_Kod");
                    });
        });

        modelBuilder.Entity<DersDetay>(entity =>
        {
            entity.ToTable("Ders_Detay");
            entity.Property(e => e.DersDetayId).HasColumnName("Ders_Detay_ID").ValueGeneratedNever();
            entity.Property(e => e.DersKod).HasMaxLength(10).HasColumnName("Ders_Kod");
            entity.Property(e => e.DonemId).HasColumnName("Donem_ID");
            entity.Property(e => e.OgretimUyesiId).HasColumnName("Ogretim_Uyesi_ID");
            entity.Property(e => e.Kontenjan);

            entity.HasOne(d => d.DersKodNavigation).WithMany(p => p.DersDetays)
                .HasForeignKey(d => d.DersKod).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DersDetay_DersKod");
            entity.HasOne(d => d.Donem).WithMany(p => p.DersDetays)
                .HasForeignKey(d => d.DonemId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DersDetay_DonemID");
            entity.HasOne(d => d.OgretimUyesi).WithMany(p => p.DersDetays)
                .HasForeignKey(d => d.OgretimUyesiId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DersDetay_OgretimUyesiID");
        });

        modelBuilder.Entity<DersKayit>(entity =>
        {
            entity.HasKey(e => new { e.OgrenciId, e.DersDetayId });
            entity.ToTable("Ders_Kayit");
            entity.Property(e => e.OgrenciId).HasColumnName("Ogrenci_ID");
            entity.Property(e => e.DersDetayId).HasColumnName("Ders_Detay_ID");
            entity.Property(e => e.KayitTarihi).HasColumnType("date").HasColumnName("Kayit_Tarihi");

            entity.HasOne(d => d.DersDetay).WithMany(p => p.DersKayits)
                .HasForeignKey(d => d.DersDetayId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DersKayit_Ders_Detay");
            entity.HasOne(d => d.Ogrenci).WithMany(p => p.DersKayits)
                .HasForeignKey(d => d.OgrenciId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DersKayit_Ogrenci");
        });

        modelBuilder.Entity<Donem>(entity =>
        {
            entity.ToTable("Donem");
            entity.Property(e => e.DonemId).HasColumnName("Donem_ID").ValueGeneratedNever();
            entity.Property(e => e.Yariyil).HasMaxLength(10);
            entity.Property(e => e.Yil);
        });

        modelBuilder.Entity<Notlar>(entity =>
        {
            entity.HasKey(e => new { e.OgrenciId, e.DersDetayId });
            entity.ToTable("Notlar");
            entity.Property(e => e.OgrenciId).HasColumnName("Ogrenci_ID");
            entity.Property(e => e.DersDetayId).HasColumnName("Ders_Detay_ID");
            entity.Property(e => e.Vize).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Final).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.HarfNotu).HasMaxLength(5).HasColumnName("Harf_Notu");

            entity.HasOne(d => d.DersDetay).WithMany(p => p.Notlars)
                .HasForeignKey(d => d.DersDetayId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Notlar_Ders_Detay");
            entity.HasOne(d => d.Ogrenci).WithMany(p => p.Notlars)
                .HasForeignKey(d => d.OgrenciId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Notlar_Ogrenci");
        });

        modelBuilder.Entity<Ogrenci>(entity =>
        {
            entity.ToTable("Ogrenci");

            entity.Property(e => e.OgrenciId).HasColumnName("Ogrenci_ID").ValueGeneratedNever();
            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.Soyad).HasMaxLength(50);
            entity.Property(e => e.Sifre).HasMaxLength(50).HasDefaultValueSql("('123')");
            entity.Property(e => e.BolumId).HasColumnName("Bolum_ID");

            entity.Property(e => e.DogumTarihi).HasColumnType("date").HasColumnName("Dogum_Tarihi");

            entity.Property(e => e.KayitTarihi).HasColumnType("date");
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.EPosta).HasMaxLength(100);
            entity.Property(e => e.Adres);
            entity.Property(e => e.Sinif).HasDefaultValue(1);

            entity.HasOne(d => d.Bolum).WithMany(p => p.Ogrencis)
                .HasForeignKey(d => d.BolumId).HasConstraintName("FK_Ogrenci_Bolum");
        });

        modelBuilder.Entity<OgretimUyesi>(entity =>
        {
            entity.ToTable("Ogretim_Uyesi");
            entity.Property(e => e.OgretimUyesiId).HasColumnName("Ogretim_Uyesi_ID").ValueGeneratedNever();
            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.Soyad).HasMaxLength(50);
            entity.Property(e => e.Unvan).HasMaxLength(50);
            entity.Property(e => e.Sifre).HasMaxLength(50).HasDefaultValue("123");
            entity.Property(e => e.BolumId).HasColumnName("Bolum_ID");

            entity.HasOne(d => d.Bolum).WithMany(p => p.OgretimUyesis)
                .HasForeignKey(d => d.BolumId).HasConstraintName("FK_OgretimUyesi_Bolum");
        });

        modelBuilder.Entity<Yonetici>(entity =>
        {
            entity.ToTable("Yonetici");
            entity.HasIndex(e => e.KullaniciAdi, "UQ_Yonetici_KullaniciAdi").IsUnique();
            entity.Property(e => e.YoneticiId).HasColumnName("Yonetici_ID");
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50).HasColumnName("Kullanici_Adi");
            entity.Property(e => e.Sifre).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}