/* =====================================================================
   Ders Kayıt Sistemi - Veritabanı Kurulum Betiği (SQL Server)
   ---------------------------------------------------------------------
   Bu betik "DersKayitSistemi" veritabanını oluşturur, tabloları ve
   ilişkileri kurar, ardından örnek verileri ekler.

   Çalıştırma (LocalDB):
     sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i Database/DersKayitSistemi.sql

   Veya SSMS / Visual Studio SQL Server Object Explorer üzerinden açıp
   çalıştırabilirsiniz.

   UYARI: Aynı isimde bir veritabanı varsa SİLİNİP yeniden oluşturulur.
   ===================================================================== */

USE master;
GO

IF DB_ID(N'DersKayitSistemi') IS NOT NULL
BEGIN
    ALTER DATABASE [DersKayitSistemi] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DersKayitSistemi];
END
GO

CREATE DATABASE [DersKayitSistemi];
GO

USE [DersKayitSistemi];
GO

/* =========================== TABLOLAR =========================== */

CREATE TABLE [Bolum] (
    [Bolum_ID]   INT            NOT NULL CONSTRAINT [PK_Bolum] PRIMARY KEY,
    [Bolum_Ad]   NVARCHAR(100)  NOT NULL,
    [Fakulte_Ad] NVARCHAR(100)  NULL
);
GO

CREATE TABLE [Donem] (
    [Donem_ID] INT           NOT NULL CONSTRAINT [PK_Donem] PRIMARY KEY,
    [Yil]      INT           NOT NULL,
    [Yariyil]  NVARCHAR(10)  NOT NULL,
    CONSTRAINT [CK_Donem_Yariyil] CHECK ([Yariyil] IN (N'Güz', N'Bahar'))
);
GO

CREATE TABLE [Yonetici] (
    [Yonetici_ID]   INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Yonetici] PRIMARY KEY,
    [Kullanici_Adi] NVARCHAR(50)      NOT NULL CONSTRAINT [UQ_Yonetici_KullaniciAdi] UNIQUE,
    [Sifre]         NVARCHAR(50)      NOT NULL
);
GO

CREATE TABLE [Ogretim_Uyesi] (
    [Ogretim_Uyesi_ID] INT           NOT NULL CONSTRAINT [PK_Ogretim_Uyesi] PRIMARY KEY,
    [Ad]               NVARCHAR(50)  NOT NULL,
    [Soyad]            NVARCHAR(50)  NOT NULL,
    [Unvan]            NVARCHAR(50)  NULL,
    [Bolum_ID]         INT           NULL,
    [Sifre]            NVARCHAR(50)  NOT NULL CONSTRAINT [DF_OgretimUyesi_Sifre] DEFAULT ('123'),
    CONSTRAINT [FK_OgretimUyesi_Bolum] FOREIGN KEY ([Bolum_ID]) REFERENCES [Bolum]([Bolum_ID])
);
GO

CREATE TABLE [Ogrenci] (
    [Ogrenci_ID]   INT           NOT NULL CONSTRAINT [PK_Ogrenci] PRIMARY KEY,
    [Ad]           NVARCHAR(50)  NOT NULL,
    [Soyad]        NVARCHAR(50)  NOT NULL,
    [Dogum_Tarihi] DATE          NULL,
    [Bolum_ID]     INT           NULL,
    [Sifre]        NVARCHAR(50)  NOT NULL CONSTRAINT [DF_Ogrenci_Sifre] DEFAULT ('123'),
    [Telefon]      NVARCHAR(20)  NULL,
    [EPosta]       NVARCHAR(100) NULL,
    [Adres]        NVARCHAR(MAX) NULL,
    [Sinif]        INT           NULL CONSTRAINT [DF_Ogrenci_Sinif] DEFAULT ((1)),
    [KayitTarihi]  DATE          NULL CONSTRAINT [DF_Ogrenci_KayitTarihi] DEFAULT (GETDATE()),
    CONSTRAINT [FK_Ogrenci_Bolum] FOREIGN KEY ([Bolum_ID]) REFERENCES [Bolum]([Bolum_ID])
);
GO

CREATE TABLE [Ders] (
    [Ders_Kod]       NVARCHAR(10)  NOT NULL CONSTRAINT [PK_Ders] PRIMARY KEY,
    [Ders_Ad]        NVARCHAR(150) NOT NULL,
    [Kredi]          DECIMAL(3,1)  NOT NULL,
    [Bolum_ID]       INT           NULL,
    [OgretimUyesiId] INT           NULL,
    [Tur]            NVARCHAR(10)  NULL CONSTRAINT [DF_Ders_Tur] DEFAULT ('Zorunlu'),
    CONSTRAINT [FK_Ders_Bolum]        FOREIGN KEY ([Bolum_ID])       REFERENCES [Bolum]([Bolum_ID]),
    CONSTRAINT [FK_Ders_OgretimUyesi] FOREIGN KEY ([OgretimUyesiId]) REFERENCES [Ogretim_Uyesi]([Ogretim_Uyesi_ID])
);
GO

-- Bir dersin ön koşul derslerini tutan çoka-çok ilişki tablosu
CREATE TABLE [Onkosul] (
    [Ana_Ders_Kod]     NVARCHAR(10) NOT NULL,
    [Onkosul_Ders_Kod] NVARCHAR(10) NOT NULL,
    CONSTRAINT [PK_Onkosul] PRIMARY KEY ([Ana_Ders_Kod], [Onkosul_Ders_Kod]),
    CONSTRAINT [FK_Onkosul_AnaDers]     FOREIGN KEY ([Ana_Ders_Kod])     REFERENCES [Ders]([Ders_Kod]),
    CONSTRAINT [FK_Onkosul_GerekenDers] FOREIGN KEY ([Onkosul_Ders_Kod]) REFERENCES [Ders]([Ders_Kod])
);
GO

-- Bir dersin belirli bir dönemde açılmış hâli (şube)
CREATE TABLE [Ders_Detay] (
    [Ders_Detay_ID]    INT          NOT NULL CONSTRAINT [PK_Ders_Detay] PRIMARY KEY,
    [Ders_Kod]         NVARCHAR(10) NOT NULL,
    [Donem_ID]         INT          NOT NULL,
    [Ogretim_Uyesi_ID] INT          NOT NULL,
    [Kontenjan]        INT          NOT NULL,
    CONSTRAINT [CK_DersDetay_Kontenjan]     CHECK ([Kontenjan] > 0),
    CONSTRAINT [FK_DersDetay_DersKod]       FOREIGN KEY ([Ders_Kod])         REFERENCES [Ders]([Ders_Kod]),
    CONSTRAINT [FK_DersDetay_DonemID]       FOREIGN KEY ([Donem_ID])         REFERENCES [Donem]([Donem_ID]),
    CONSTRAINT [FK_DersDetay_OgretimUyesiID] FOREIGN KEY ([Ogretim_Uyesi_ID]) REFERENCES [Ogretim_Uyesi]([Ogretim_Uyesi_ID])
);
GO

CREATE TABLE [Ders_Kayit] (
    [Ogrenci_ID]    INT  NOT NULL,
    [Ders_Detay_ID] INT  NOT NULL,
    [Kayit_Tarihi]  DATE NOT NULL,
    CONSTRAINT [PK_DersKayit] PRIMARY KEY ([Ogrenci_ID], [Ders_Detay_ID]),
    CONSTRAINT [FK_DersKayit_Ogrenci]     FOREIGN KEY ([Ogrenci_ID])    REFERENCES [Ogrenci]([Ogrenci_ID]),
    CONSTRAINT [FK_DersKayit_Ders_Detay]  FOREIGN KEY ([Ders_Detay_ID]) REFERENCES [Ders_Detay]([Ders_Detay_ID])
);
GO

CREATE TABLE [Notlar] (
    [Ogrenci_ID]    INT          NOT NULL,
    [Ders_Detay_ID] INT          NOT NULL,
    [Vize]          DECIMAL(5,2) NULL,
    [Final]         DECIMAL(5,2) NULL,
    [Harf_Notu]     NVARCHAR(5)  NULL,
    CONSTRAINT [PK_Notlar] PRIMARY KEY ([Ogrenci_ID], [Ders_Detay_ID]),
    CONSTRAINT [FK_Notlar_Ogrenci]    FOREIGN KEY ([Ogrenci_ID])    REFERENCES [Ogrenci]([Ogrenci_ID]),
    CONSTRAINT [FK_Notlar_Ders_Detay] FOREIGN KEY ([Ders_Detay_ID]) REFERENCES [Ders_Detay]([Ders_Detay_ID])
);
GO

/* ========================= ÖRNEK VERİLER ========================= */

-- Yonetici (1 kayıt)
SET IDENTITY_INSERT [Yonetici] ON;
INSERT INTO [Yonetici] ([Yonetici_ID], [Kullanici_Adi], [Sifre]) VALUES
    (9, N'admin', N'1234');
SET IDENTITY_INSERT [Yonetici] OFF;
GO

-- Bolum (6 kayıt)
INSERT INTO [Bolum] ([Bolum_ID], [Bolum_Ad], [Fakulte_Ad]) VALUES
    (10, N'Bilgisayar Mühendisliği', N'Mühendislik Fak.'),
    (20, N'Elektrik-Elektronik Müh.', N'Mühendislik Fak.'),
    (30, N'Makine Mühendisliği', N'Mühendislik Fak.'),
    (40, N'İnşaat Mühendisliği', N'Mühendislik Fak.'),
    (50, N'Endüstri Mühendisliği', N'Mühendislik Fak.'),
    (60, N'Kimya Mühendisliği', N'Mühendislik Fak.');
GO

-- Donem (1 kayıt)
INSERT INTO [Donem] ([Donem_ID], [Yil], [Yariyil]) VALUES
    (1, 2025, N'Güz');
GO

-- Ogretim_Uyesi (18 kayıt)
INSERT INTO [Ogretim_Uyesi] ([Ogretim_Uyesi_ID], [Ad], [Soyad], [Unvan], [Bolum_ID], [Sifre]) VALUES
    (101, N'Ahmet', N'Yılmaz', N'Prof.', 10, N'123'),
    (102, N'Elif', N'Demir', N'Doç.', 10, N'123'),
    (103, N'Can', N'Erim', N'Dr.', 10, N'123'),
    (104, N'Selin', N'Şahin', N'Prof.', 20, N'123'),
    (105, N'Burak', N'Çelik', N'Doç.', 20, N'123'),
    (106, N'Gamze', N'Yıl', N'Dr.', 20, N'123'),
    (107, N'Kemal', N'Sunal', N'Prof.', 30, N'123'),
    (108, N'Halit', N'Akça', N'Doç.', 30, N'123'),
    (109, N'Tarık', N'Akan', N'Dr.', 30, N'123'),
    (110, N'Münir', N'Özkul', N'Prof.', 40, N'123'),
    (111, N'Adile', N'Naşit', N'Doç.', 40, N'123'),
    (112, N'Şener', N'Şen', N'Dr.', 40, N'123'),
    (113, N'Barış', N'Manço', N'Prof.', 50, N'123'),
    (114, N'Cem', N'Karaca', N'Doç.', 50, N'123'),
    (115, N'Erkin', N'Koray', N'Dr.', 50, N'123'),
    (116, N'Sezen', N'Aksu', N'Prof.', 60, N'123'),
    (117, N'Ajda', N'Pekkan', N'Doç.', 60, N'123'),
    (118, N'Nilüfer', N'Yum', N'Dr.', 60, N'123');
GO

-- Ogrenci (30 kayıt)
INSERT INTO [Ogrenci] ([Ogrenci_ID], [Ad], [Soyad], [Dogum_Tarihi], [Bolum_ID], [Sifre], [Telefon], [EPosta], [Adres], [Sinif], [KayitTarihi]) VALUES
    (2001, N'Ali', N'Yilmaz', NULL, 10, N'123', N'5551 3984934', N'ali@m.com', N'Istanbul Beylikdüzü', 1, '2025-12-20'),
    (2002, N'Veli', N'Ozturk', NULL, 10, N'123', N'5552', N'veli@m.com', N'Ankara', 2, '2025-12-20'),
    (2003, N'Ayse', N'Demir', NULL, 10, N'123', N'5553', N'ayse@m.com', N'Izm', 3, '2025-12-20'),
    (2004, N'Fatma', N'Kara', NULL, 10, N'123', N'5554', N'fatma@m.com', N'Bur', 4, '2025-12-20'),
    (2005, N'Mehmet', N'Can', NULL, 10, N'123', N'5555', N'mehmet@m.com', N'Ant', 1, '2025-12-20'),
    (2006, N'Zeynep', N'Su', NULL, 20, N'123', N'5556', N'zeynep@m.com', N'Kon', 1, '2025-12-20'),
    (2007, N'Mustafa', N'Koc', NULL, 20, N'123', N'5557', N'mustafa@m.com', N'Ada', 2, '2025-12-20'),
    (2008, N'Hasan', N'Ali', NULL, 20, N'123', N'5558', N'hasan@m.com', N'Mer', 3, '2025-12-20'),
    (2009, N'Huseyin', N'Boz', NULL, 20, N'123', N'5559', N'huseyin@m.com', N'Tra', 4, '2025-12-20'),
    (2010, N'Elif', N'Nur', NULL, 20, N'123', N'5550', N'elif@m.com', N'Sam', 1, '2025-12-20'),
    (2011, N'Burak', N'Yil', NULL, 30, N'123', N'5551', N'burak@m.com', N'Esk', 2, '2025-12-20'),
    (2012, N'Cem', N'Son', NULL, 30, N'123', N'5552', N'cem@m.com', N'Kay', 3, '2025-12-20'),
    (2013, N'Deniz', N'Gez', NULL, 30, N'123', N'5553', N'deniz@m.com', N'Mug', 4, '2025-12-20'),
    (2014, N'Ece', N'Er', NULL, 30, N'123', N'5554', N'ece@m.com', N'Ayd', 1, '2025-12-20'),
    (2015, N'Firat', N'Tan', NULL, 30, N'123', N'5555', N'firat@m.com', N'Gaz', 2, '2025-12-20'),
    (2016, N'Gokhan', N'Tor', NULL, 40, N'123', N'5556', N'gokhan@m.com', N'Hat', 3, '2025-12-20'),
    (2017, N'Hale', N'Soy', NULL, 40, N'123', N'5557', N'hale@m.com', N'Mal', 4, '2025-12-20'),
    (2018, N'Irem', N'Der', NULL, 40, N'123', N'5558', N'irem@m.com', N'Siv', 1, '2025-12-20'),
    (2019, N'Jale', N'Par', NULL, 40, N'123', N'5559', N'jale@m.com', N'Erz', 2, '2025-12-20'),
    (2020, N'Kaan', N'Yil', NULL, 40, N'123', N'5550', N'kaan@m.com', N'Van', 3, '2025-12-20'),
    (2021, N'Lale', N'Man', NULL, 50, N'123', N'5551', N'lale@m.com', N'Riz', 4, '2025-12-20'),
    (2022, N'Mert', N'Yaz', NULL, 50, N'123', N'5552', N'mert@m.com', N'Art', 1, '2025-12-20'),
    (2023, N'Nazli', N'Cel', NULL, 50, N'123', N'5553', N'nazli@m.com', N'Ord', 2, '2025-12-20'),
    (2024, N'Oguz', N'Han', NULL, 50, N'123', N'5554', N'oguz@m.com', N'Gir', 3, '2025-12-20'),
    (2025, N'Pelin', N'Kar', NULL, 50, N'123', N'5555', N'pelin@m.com', N'Sin', 4, '2025-12-20'),
    (2026, N'Riza', N'Bab', NULL, 60, N'123', N'5556', N'riza@m.com', N'Kar', 1, '2025-12-20'),
    (2027, N'Seda', N'Say', NULL, 60, N'123', N'5557', N'seda@m.com', N'Agr', 2, '2025-12-20'),
    (2028, N'Tuna', N'Kir', NULL, 60, N'123', N'5558', N'tuna@m.com', N'Igd', 3, '2025-12-20'),
    (2029, N'Ufuk', N'Bay', NULL, 60, N'123', N'5559', N'ufuk@m.com', N'Kil', 4, '2025-12-20'),
    (2030, N'Volkan', N'Kon', NULL, 60, N'123', N'5550', N'volkan@m.com', N'Can', 1, '2025-12-20');
GO

-- Ders (61 kayıt)
INSERT INTO [Ders] ([Ders_Kod], [Ders_Ad], [Kredi], [Bolum_ID], [OgretimUyesiId], [Tur]) VALUES
    (N'ABC101', N'abcabc', 3.0, 10, 111, N'Zorunlu'),
    (N'BLG101', N'Algoritma ve Prog.', 5.0, 10, 101, N'Zorunlu'),
    (N'BLG102', N'Veri Yapıları', 5.0, 10, 102, N'Zorunlu'),
    (N'BLG103', N'Bilgisayar Mimarisi', 5.0, 10, 103, N'Zorunlu'),
    (N'BLG104', N'İşletim Sistemleri', 5.0, 10, 101, N'Zorunlu'),
    (N'BLG105', N'Veritabanı Yön.', 5.0, 10, 102, N'Zorunlu'),
    (N'BLG106', N'Yazılım Müh.', 5.0, 10, 103, N'Zorunlu'),
    (N'BLG201', N'Yapay Zeka', 5.0, 10, 101, N'Secmeli'),
    (N'BLG202', N'Görüntü İşleme', 5.0, 10, 102, N'Secmeli'),
    (N'BLG203', N'Mobil Programlama', 5.0, 10, 103, N'Secmeli'),
    (N'BLG204', N'Web Tasarımı', 5.0, 10, 101, N'Secmeli'),
    (N'EEM101', N'Devre Analizi I', 5.0, 20, 104, N'Zorunlu'),
    (N'EEM102', N'Devre Analizi II', 5.0, 20, 105, N'Zorunlu'),
    (N'EEM103', N'Elektronik I', 5.0, 20, 106, N'Zorunlu'),
    (N'EEM104', N'Elektronik II', 5.0, 20, 104, N'Zorunlu'),
    (N'EEM105', N'Sinyaller', 5.0, 20, 105, N'Zorunlu'),
    (N'EEM106', N'Elektromanyetik', 5.0, 20, 106, N'Zorunlu'),
    (N'EEM201', N'Mikroişlemciler', 5.0, 20, 104, N'Secmeli'),
    (N'EEM202', N'Haberleşme', 5.0, 20, 105, N'Secmeli'),
    (N'EEM203', N'Yenilenebilir Enerji', 5.0, 20, 106, N'Secmeli'),
    (N'EEM204', N'Güç Elektroniği', 5.0, 20, 104, N'Secmeli'),
    (N'END101', N'Yöneylem I', 5.0, 50, 113, N'Zorunlu'),
    (N'END102', N'Yöneylem II', 5.0, 50, 114, N'Zorunlu'),
    (N'END103', N'Üretim Planlama', 5.0, 50, 115, N'Zorunlu'),
    (N'END104', N'İstatistik', 5.0, 50, 113, N'Zorunlu'),
    (N'END105', N'Kalite Kontrol', 5.0, 50, 114, N'Zorunlu'),
    (N'END106', N'Ergonomi', 5.0, 50, 115, N'Zorunlu'),
    (N'END201', N'Simülasyon', 5.0, 50, 113, N'Secmeli'),
    (N'END202', N'Tedarik Zinciri', 5.0, 50, 114, N'Secmeli'),
    (N'END203', N'Veri Analitiği', 5.0, 50, 115, N'Secmeli'),
    (N'END204', N'Proje Yönetimi', 5.0, 50, 113, N'Secmeli'),
    (N'INS101', N'Teknik Resim', 5.0, 40, 110, N'Zorunlu'),
    (N'INS102', N'Statik', 5.0, 40, 111, N'Zorunlu'),
    (N'INS103', N'Mukavemet', 5.0, 40, 112, N'Zorunlu'),
    (N'INS104', N'Yapı Statiği', 5.0, 40, 110, N'Zorunlu'),
    (N'INS105', N'Zemin Mekaniği', 5.0, 40, 111, N'Zorunlu'),
    (N'INS106', N'Betonarme', 5.0, 40, 112, N'Zorunlu'),
    (N'INS201', N'Çelik Yapılar', 5.0, 40, 110, N'Secmeli'),
    (N'INS202', N'Ulaştırma', 5.0, 40, 111, N'Secmeli'),
    (N'INS203', N'Hidrolik', 5.0, 40, 112, N'Secmeli'),
    (N'INS204', N'Şantiye Yönetimi', 5.0, 40, 110, N'Secmeli'),
    (N'KIM101', N'Genel Kimya', 5.0, 60, 116, N'Zorunlu'),
    (N'KIM102', N'Analitik Kimya', 5.0, 60, 117, N'Zorunlu'),
    (N'KIM103', N'Organik Kimya', 5.0, 60, 118, N'Zorunlu'),
    (N'KIM104', N'Fizikokimya', 5.0, 60, 116, N'Zorunlu'),
    (N'KIM105', N'Kütle Transferi', 5.0, 60, 117, N'Zorunlu'),
    (N'KIM106', N'Isı Transferi', 5.0, 60, 118, N'Zorunlu'),
    (N'KIM201', N'Nanoteknoloji', 5.0, 60, 116, N'Secmeli'),
    (N'KIM202', N'Biyokimya', 5.0, 60, 117, N'Secmeli'),
    (N'KIM203', N'Polimer Kimyası', 5.0, 60, 118, N'Secmeli'),
    (N'KIM204', N'Gıda Kimyası', 5.0, 60, 116, N'Secmeli'),
    (N'MAK101', N'Statik', 5.0, 30, 107, N'Zorunlu'),
    (N'MAK102', N'Dinamik', 5.0, 30, 108, N'Zorunlu'),
    (N'MAK103', N'Termodinamik I', 5.0, 30, 109, N'Zorunlu'),
    (N'MAK104', N'Termodinamik II', 5.0, 30, 107, N'Zorunlu'),
    (N'MAK105', N'Mukavemet', 5.0, 30, 108, N'Zorunlu'),
    (N'MAK106', N'Akışkanlar Mek.', 5.0, 30, 109, N'Zorunlu'),
    (N'MAK201', N'Robotik', 5.0, 30, 107, N'Secmeli'),
    (N'MAK202', N'Otomotiv', 5.0, 30, 108, N'Secmeli'),
    (N'MAK203', N'Isı Transferi', 5.0, 30, 109, N'Secmeli'),
    (N'MAK204', N'Mekatronik', 5.0, 30, 107, N'Secmeli');
GO

-- Ders_Detay (60 kayıt)
INSERT INTO [Ders_Detay] ([Ders_Detay_ID], [Ders_Kod], [Donem_ID], [Ogretim_Uyesi_ID], [Kontenjan]) VALUES
    (501, N'BLG101', 1, 101, 50),
    (502, N'BLG102', 1, 102, 50),
    (503, N'BLG103', 1, 103, 50),
    (504, N'BLG104', 1, 101, 50),
    (505, N'BLG105', 1, 102, 50),
    (506, N'BLG106', 1, 103, 50),
    (507, N'BLG201', 1, 101, 50),
    (508, N'BLG202', 1, 102, 50),
    (509, N'BLG203', 1, 103, 50),
    (510, N'BLG204', 1, 101, 50),
    (511, N'EEM101', 1, 104, 50),
    (512, N'EEM102', 1, 105, 50),
    (513, N'EEM103', 1, 106, 50),
    (514, N'EEM104', 1, 104, 50),
    (515, N'EEM105', 1, 105, 50),
    (516, N'EEM106', 1, 106, 50),
    (517, N'EEM201', 1, 104, 50),
    (518, N'EEM202', 1, 105, 50),
    (519, N'EEM203', 1, 106, 50),
    (520, N'EEM204', 1, 104, 50),
    (521, N'END101', 1, 113, 50),
    (522, N'END102', 1, 114, 50),
    (523, N'END103', 1, 115, 50),
    (524, N'END104', 1, 113, 50),
    (525, N'END105', 1, 114, 50),
    (526, N'END106', 1, 115, 50),
    (527, N'END201', 1, 113, 50),
    (528, N'END202', 1, 114, 50),
    (529, N'END203', 1, 115, 50),
    (530, N'END204', 1, 113, 50),
    (531, N'INS101', 1, 110, 50),
    (532, N'INS102', 1, 111, 50),
    (533, N'INS103', 1, 112, 50),
    (534, N'INS104', 1, 110, 50),
    (535, N'INS105', 1, 111, 50),
    (536, N'INS106', 1, 112, 50),
    (537, N'INS201', 1, 110, 50),
    (538, N'INS202', 1, 111, 50),
    (539, N'INS203', 1, 112, 50),
    (540, N'INS204', 1, 110, 50),
    (541, N'KIM101', 1, 116, 50),
    (542, N'KIM102', 1, 117, 50),
    (543, N'KIM103', 1, 118, 50),
    (544, N'KIM104', 1, 116, 50),
    (545, N'KIM105', 1, 117, 50),
    (546, N'KIM106', 1, 118, 50),
    (547, N'KIM201', 1, 116, 50),
    (548, N'KIM202', 1, 117, 50),
    (549, N'KIM203', 1, 118, 50),
    (550, N'KIM204', 1, 116, 50),
    (551, N'MAK101', 1, 107, 50),
    (552, N'MAK102', 1, 108, 50),
    (553, N'MAK103', 1, 109, 50),
    (554, N'MAK104', 1, 107, 50),
    (555, N'MAK105', 1, 108, 50),
    (556, N'MAK106', 1, 109, 50),
    (557, N'MAK201', 1, 107, 50),
    (558, N'MAK202', 1, 108, 50),
    (559, N'MAK203', 1, 109, 50),
    (560, N'MAK204', 1, 107, 50);
GO

-- Ders_Kayit (86 kayıt)
INSERT INTO [Ders_Kayit] ([Ogrenci_ID], [Ders_Detay_ID], [Kayit_Tarihi]) VALUES
    (2001, 501, '2025-12-20'),
    (2001, 502, '2025-12-20'),
    (2001, 503, '2025-12-20'),
    (2001, 504, '2025-12-20'),
    (2001, 505, '2025-12-20'),
    (2001, 506, '2025-12-20'),
    (2002, 501, '2025-12-20'),
    (2002, 502, '2025-12-20'),
    (2002, 503, '2025-12-20'),
    (2002, 504, '2025-12-20'),
    (2002, 505, '2025-12-20'),
    (2002, 506, '2025-12-20'),
    (2003, 501, '2025-12-22'),
    (2003, 502, '2025-12-22'),
    (2003, 503, '2025-12-22'),
    (2003, 504, '2025-12-22'),
    (2003, 505, '2025-12-22'),
    (2003, 506, '2025-12-22'),
    (2004, 501, '2025-12-22'),
    (2004, 502, '2025-12-22'),
    (2004, 503, '2025-12-22'),
    (2004, 504, '2025-12-22'),
    (2004, 505, '2025-12-22'),
    (2004, 506, '2025-12-22'),
    (2005, 501, '2025-12-21'),
    (2005, 502, '2025-12-21'),
    (2005, 503, '2025-12-21'),
    (2005, 504, '2025-12-21'),
    (2005, 505, '2025-12-21'),
    (2005, 506, '2025-12-21'),
    (2005, 507, '2025-12-21'),
    (2006, 511, '2025-12-22'),
    (2006, 512, '2025-12-22'),
    (2006, 513, '2025-12-22'),
    (2006, 514, '2025-12-22'),
    (2006, 515, '2025-12-22'),
    (2006, 516, '2025-12-22'),
    (2007, 511, '2025-12-22'),
    (2007, 512, '2025-12-22'),
    (2007, 513, '2025-12-22'),
    (2007, 514, '2025-12-22'),
    (2007, 515, '2025-12-22'),
    (2007, 516, '2025-12-22'),
    (2013, 551, '2025-12-22'),
    (2013, 552, '2025-12-22'),
    (2013, 553, '2025-12-22'),
    (2013, 554, '2025-12-22'),
    (2013, 555, '2025-12-22'),
    (2013, 556, '2025-12-22'),
    (2013, 557, '2025-12-22'),
    (2014, 551, '2025-12-22'),
    (2014, 552, '2025-12-22'),
    (2014, 553, '2025-12-22'),
    (2014, 554, '2025-12-22'),
    (2014, 555, '2025-12-22'),
    (2014, 557, '2025-12-22'),
    (2014, 558, '2025-12-22'),
    (2014, 559, '2025-12-22'),
    (2014, 560, '2025-12-22'),
    (2015, 551, '2025-12-22'),
    (2015, 552, '2025-12-22'),
    (2015, 553, '2025-12-22'),
    (2015, 554, '2025-12-22'),
    (2015, 555, '2025-12-22'),
    (2015, 556, '2025-12-22'),
    (2015, 557, '2025-12-22'),
    (2017, 531, '2025-12-22'),
    (2017, 532, '2025-12-22'),
    (2017, 533, '2025-12-22'),
    (2017, 534, '2025-12-22'),
    (2017, 535, '2025-12-22'),
    (2017, 536, '2025-12-22'),
    (2019, 531, '2025-12-22'),
    (2019, 532, '2025-12-22'),
    (2019, 533, '2025-12-22'),
    (2019, 534, '2025-12-22'),
    (2019, 535, '2025-12-22'),
    (2019, 536, '2025-12-22'),
    (2019, 537, '2025-12-22'),
    (2027, 541, '2025-12-22'),
    (2027, 542, '2025-12-22'),
    (2027, 543, '2025-12-22'),
    (2027, 544, '2025-12-22'),
    (2027, 545, '2025-12-22'),
    (2027, 546, '2025-12-22'),
    (2027, 547, '2025-12-22');
GO

-- Notlar (86 kayıt)
INSERT INTO [Notlar] ([Ogrenci_ID], [Ders_Detay_ID], [Vize], [Final], [Harf_Notu]) VALUES
    (2001, 501, 45.00, 35.00, N'DD'),
    (2001, 502, 50.00, NULL, N'NA'),
    (2001, 503, NULL, NULL, N'NA'),
    (2001, 504, 100.00, 80.00, N'BA'),
    (2001, 505, 40.00, NULL, N'NA'),
    (2001, 506, NULL, NULL, N'NA'),
    (2002, 501, 50.00, 30.00, N'FF'),
    (2002, 502, 80.00, 90.00, N'BA'),
    (2002, 503, NULL, NULL, N'NA'),
    (2002, 504, 60.00, 100.00, N'BA'),
    (2002, 505, 50.00, 60.00, N'CC'),
    (2002, 506, NULL, NULL, N'NA'),
    (2003, 501, 60.00, 70.00, N'BB'),
    (2003, 502, 40.00, 30.00, N'FF'),
    (2003, 503, NULL, NULL, N'NA'),
    (2003, 504, NULL, NULL, N'NA'),
    (2003, 505, 60.00, 30.00, N'FF'),
    (2003, 506, NULL, NULL, N'NA'),
    (2004, 501, NULL, NULL, N'NA'),
    (2004, 502, 60.00, 70.00, N'BB'),
    (2004, 503, NULL, NULL, N'NA'),
    (2004, 504, NULL, NULL, N'NA'),
    (2004, 505, NULL, NULL, N'NA'),
    (2004, 506, NULL, NULL, N'NA'),
    (2005, 501, 45.00, 70.00, N'CB'),
    (2005, 502, NULL, NULL, N'NA'),
    (2005, 503, NULL, NULL, N'NA'),
    (2005, 504, NULL, NULL, N'NA'),
    (2005, 505, NULL, NULL, N'NA'),
    (2005, 506, NULL, NULL, N'NA'),
    (2005, 507, 50.00, 70.00, N'DD'),
    (2006, 511, NULL, NULL, N'NA'),
    (2006, 512, NULL, NULL, N'NA'),
    (2006, 513, NULL, NULL, N'NA'),
    (2006, 514, NULL, NULL, N'NA'),
    (2006, 515, NULL, NULL, N'NA'),
    (2006, 516, NULL, NULL, N'NA'),
    (2007, 511, NULL, NULL, N'NA'),
    (2007, 512, NULL, NULL, N'NA'),
    (2007, 513, NULL, NULL, N'NA'),
    (2007, 514, NULL, NULL, N'NA'),
    (2007, 515, NULL, NULL, N'NA'),
    (2007, 516, NULL, NULL, N'NA'),
    (2013, 551, NULL, NULL, N'NA'),
    (2013, 552, NULL, NULL, N'NA'),
    (2013, 553, NULL, NULL, N'NA'),
    (2013, 554, NULL, NULL, N'NA'),
    (2013, 555, NULL, NULL, N'NA'),
    (2013, 556, NULL, NULL, N'NA'),
    (2013, 557, NULL, NULL, N'NA'),
    (2014, 551, NULL, NULL, N'NA'),
    (2014, 552, NULL, NULL, N'NA'),
    (2014, 553, NULL, NULL, N'NA'),
    (2014, 554, NULL, NULL, N'NA'),
    (2014, 555, NULL, NULL, N'NA'),
    (2014, 557, NULL, NULL, N'NA'),
    (2014, 558, NULL, NULL, N'NA'),
    (2014, 559, NULL, NULL, N'NA'),
    (2014, 560, NULL, NULL, N'NA'),
    (2015, 551, NULL, NULL, N'NA'),
    (2015, 552, NULL, NULL, N'NA'),
    (2015, 553, NULL, NULL, N'NA'),
    (2015, 554, NULL, NULL, N'NA'),
    (2015, 555, NULL, NULL, N'NA'),
    (2015, 556, NULL, NULL, N'NA'),
    (2015, 557, NULL, NULL, N'NA'),
    (2017, 531, NULL, NULL, N'NA'),
    (2017, 532, NULL, NULL, N'NA'),
    (2017, 533, NULL, NULL, N'NA'),
    (2017, 534, NULL, NULL, N'NA'),
    (2017, 535, NULL, NULL, N'NA'),
    (2017, 536, NULL, NULL, N'NA'),
    (2019, 531, NULL, NULL, N'NA'),
    (2019, 532, NULL, NULL, N'NA'),
    (2019, 533, NULL, NULL, N'NA'),
    (2019, 534, NULL, NULL, N'NA'),
    (2019, 535, NULL, NULL, N'NA'),
    (2019, 536, NULL, NULL, N'NA'),
    (2019, 537, NULL, NULL, N'NA'),
    (2027, 541, NULL, NULL, N'NA'),
    (2027, 542, NULL, NULL, N'NA'),
    (2027, 543, NULL, NULL, N'NA'),
    (2027, 544, NULL, NULL, N'NA'),
    (2027, 545, NULL, NULL, N'NA'),
    (2027, 546, NULL, NULL, N'NA'),
    (2027, 547, NULL, NULL, N'NA');
GO

