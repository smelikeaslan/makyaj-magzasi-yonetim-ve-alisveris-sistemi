using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nesne_proje
{
    // Ürün sınıfı
    class Urun
    {
        private int stok; // private erişimci
        public string Ad { get; set; }
        public string Kategori { get; set; }
        public string Marka { get; set; }
        public decimal Fiyat { get; set; }

        public int Stok
        {
            get { return stok; }
            set
            {
                if (value < 0)
                {
                    Console.WriteLine("Stok miktarı sıfırdan küçük olamaz!");
                    stok = 0; // Negatif bir stok değeri girilirse, 0 olarak ayarlanır.
                }
                else
                {
                    stok = value; // Geçerli stok miktarı.
                }
            }
        }

        public Urun(string ad, string kategori, int stok, string marka, decimal fiyat)
        {
            Ad = ad;
            Kategori = kategori;
            Marka = marka;
            Stok = stok; // Stok değeri buradan ayarlanacak ve doğrulama yapılacak
            Fiyat = fiyat;
        }

        // Polimorfizm: ToString() metodunu override ediyoruz.
        public override string ToString()
        {
            return $"{Ad} ({Kategori}) - Marka: {Marka} - Stok: {Stok} - Fiyat: {Fiyat:C}";
        }

        // Destructor (Yıkıcı) ekliyoruz
        ~Urun()
        {
            Console.WriteLine($"{Ad} ürünü yıkıldı.");
        }
    }

    // Üye sınıfı
    class Uye
    {
        public string KullaniciAdi { get; set; }
        public string KullaniciSoyadi { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string Gmail { get; set; }
        public string TelefonNumarasi { get; set; }

        public Uye(string kullaniciAdi, string kullaniciSoyadi, DateTime dogumTarihi, string gmail, string telefonNumarasi)
        {
            KullaniciAdi = kullaniciAdi;
            KullaniciSoyadi = kullaniciSoyadi;
            DogumTarihi = dogumTarihi;
            Gmail = gmail;
            TelefonNumarasi = telefonNumarasi;
        }

        // Polimorfizm: ToString() metodunu override ediyoruz.
        public override string ToString()
        {
            return $"{KullaniciAdi} {KullaniciSoyadi} ({DogumTarihi.ToShortDateString()}) - Email: {Gmail}";
        }

        // Destructor (Yıkıcı) ekliyoruz
        ~Uye()
        {
            Console.WriteLine($"{KullaniciAdi} {KullaniciSoyadi} yıkıldı.");
        }
    }

    // Personel Üye sınıfı
    class PersonelUye : Uye
    {
        public string CalismaBolumu { get; set; }
        public string Unvan { get; set; }

        public PersonelUye(string kullaniciAdi, string kullaniciSoyadi, DateTime dogumTarihi, string gmail, string telefonNumarasi, string calismaBolumu, string unvan)
            : base(kullaniciAdi, kullaniciSoyadi, dogumTarihi, gmail, telefonNumarasi)
        {
            CalismaBolumu = calismaBolumu;
            Unvan = unvan;
        }

        // Polimorfizm: ToString() metodunu override ediyoruz.
        public override string ToString()
        {
            return base.ToString() + $" - Çalışma Bölümü: {CalismaBolumu} - Unvan: {Unvan}";
        }

        // Destructor (Yıkıcı) ekliyoruz
        ~PersonelUye()
        {
            Console.WriteLine($"{KullaniciAdi} {KullaniciSoyadi} personel üyesi olarak yıkıldı.");
        }
    }

    // Sepet sınıfı
    class Sepet
    {
        public List<(Urun Urun, int Miktar)> Urunler { get; private set; } = new List<(Urun Urun, int Miktar)>();
        public decimal ToplamFiyat => Urunler.Sum(x => x.Urun.Fiyat * x.Miktar);

        public bool UrunEkle(Urun urun, int miktar)
        {
            if (miktar > urun.Stok)
            {
                return false;
            }

            var mevcutUrun = Urunler.FirstOrDefault(x => x.Urun.Ad == urun.Ad);
            if (mevcutUrun.Urun != null)
            {
                if (mevcutUrun.Miktar + miktar > urun.Stok)
                {
                    return false;
                }
                Urunler.Remove(mevcutUrun);
                Urunler.Add((mevcutUrun.Urun, mevcutUrun.Miktar + miktar));
            }
            else
            {
                Urunler.Add((urun, miktar));
            }

            urun.Stok -= miktar;
            return true;
        }

        public void UrunCikar(string urunAdi)
        {
            var urun = Urunler.FirstOrDefault(x => x.Urun.Ad.ToLower() == urunAdi.ToLower());
            if (urun.Urun != null)
            {
                urun.Urun.Stok += urun.Miktar; // Stoku geri ekle
                Urunler.Remove(urun);
            }
        }

        public void Temizle()
        {
            foreach (var item in Urunler)
            {
                item.Urun.Stok += item.Miktar; // Tüm ürünlerin stoklarını geri ekle
            }
            Urunler.Clear();
        }
    }

    // Favori listesi sınıfı
    class FavoriListesi
    {
        public List<Urun> Urunler { get; private set; } = new List<Urun>();

        public void UrunEkle(Urun urun)
        {
            if (!Urunler.Any(x => x.Ad == urun.Ad))
            {
                Urunler.Add(urun);
            }
        }

        public void UrunCikar(string urunAdi)
        {
            var urun = Urunler.FirstOrDefault(x => x.Ad.ToLower() == urunAdi.ToLower());
            if (urun != null)
            {
                Urunler.Remove(urun);
            }
        }
    }

    // Üyelik giriş sınıfı
    class Program
    {
        private static Dictionary<string, decimal> indirimKodlari = new Dictionary<string, decimal>
        {
            { "YENI10", 0.10m },    // %10 indirim
            { "OZEL20", 0.20m },    // %20 indirim
            { "SUPER25", 0.25m }    // %25 indirim
        };

        private static Sepet aktifSepet = new Sepet();
        private static FavoriListesi favoriler = new FavoriListesi();

        static void Main(string[] args)
        {
            // Hoşgeldiniz mesajı
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));

            string welcomeMessage = "* Makyaj Mağazası Yönetim Sistemine Hoşgeldiniz *";
            int padding = (Console.WindowWidth - welcomeMessage.Length) / 2;  // Calculate padding to center the text
            string paddedMessage = new string(' ', padding) + welcomeMessage;
            Console.WriteLine(paddedMessage);

            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));
            Console.WriteLine(new string('*', Console.WindowWidth));

            Console.WriteLine("Devam etmek için herhangi bir tuşa basın...");
            Console.ReadKey(); // Kullanıcı bir tuşa basana kadar bekler

            Console.Clear(); // Ekranı temizle
            Console.WriteLine("Makyaj Mağazası Stok Yönetim Sistemi\n");

            // İndirim kodlarını dosyadan yükle
            IndirimKodlariniDosyadanOku();

            // Kullanıcı bilgileri
            Dictionary<string, string> kullanicilar = new Dictionary<string, string>
            {
                { "sudekahraman", "25122004" },
                { "melikeaslan", "13012005" }
            };

            // Kullanıcı girişi kod grubu
            bool dogrulama = false;
            Console.WriteLine("1. Yönetici Girişi");
            Console.WriteLine("2. Müşteri Girişi");
            Console.Write("Seçiminizi yapın: ");
            string girisSecimi = Console.ReadLine();

            if (girisSecimi == "1")
            {
                while (!dogrulama)
                {
                    Console.Write("Yönetici adı: ");
                    string kullaniciAdi = Console.ReadLine();

                    Console.Write("Şifre: ");
                    string sifre = SifreOku(); // Şifreyi gizleyen yöntem çağrısı

                    if (kullanicilar.ContainsKey(kullaniciAdi) && kullanicilar[kullaniciAdi] == sifre)
                    {
                        dogrulama = true;
                        Console.WriteLine("\nGiriş başarılı!\n");
                        Console.Beep(1000, 500); // Eğlenceli ses
                        Console.Beep(1200, 500);
                    }
                    else
                    {
                        Console.WriteLine("\nKullanıcı adı veya şifre yanlış.\n");
                        Console.Beep(500, 500); // Hüzünlü ses
                        Console.Beep(400, 500);
                    }
                }

                // Ürün listesi oluşturma kod grubu
                List<Urun> urunler = UrunleriDosyadanOku(); // Dosyadaki ürünleri oku
                if (urunler.Count == 0)
                {
                    urunler = new List<Urun>
                    {
                        new Urun("Far", "Göz Makyajı", 10, "Maybelline", 10.00m),
                        new Urun("Rimel", "Göz Makyajı", 5, "Flormar", 5.00m),
                        new Urun("Eyeliner", "Göz Makyajı", 7, "Beaulis", 7.00m),
                        new Urun("Göz Kalemi", "Göz Makyajı", 6, "Lykd", 6.00m),
                        new Urun("Ruj", "Dudak Makyajı", 15, "Deborah", 15.00m),
                        new Urun("Gloss", "Dudak Makyajı", 9, "Maybelline", 9.00m),
                        new Urun("Lip Balm", "Dudak Makyajı", 12, "Flormar", 12.00m),
                        new Urun("Lip Oil", "Dudak Makyajı", 8, "Beaulis", 8.00m),
                        new Urun("Dudak Kalemi", "Dudak Makyajı", 10, "Lykd", 10.00m),
                        new Urun("Fondöten", "Ten Makyajı", 8, "Deborah", 8.00m),
                        new Urun("Kapatıcı", "Ten Makyajı", 11, "Maybelline", 11.00m),
                        new Urun("Highlighter", "Ten Makyajı", 5, "Flormar", 5.00m),
                        new Urun("Pudra", "Ten Makyajı", 7, "Beaulis", 7.00m),
                        new Urun("Allık", "Ten Makyajı", 12, "Lykd", 12.00m),
                        new Urun("Makyaj Bazı", "Ten Makyajı", 10, "Maybelline", 10.00m),
                        new Urun("Oje", "Tırnak Bakımı", 20, "Golden Rose", 20.00m),
                        new Urun("Tırnak Bakım Yağı", "Tırnak Bakımı", 15, "Flormar", 15.00m),
                        new Urun("Tırnak Makası", "Tırnak Bakımı", 8, "Tweezerman", 8.00m),
                        new Urun("Nemlendirici", "Cilt Bakımı", 12, "Neutrogena", 12.00m),
                        new Urun("Yüz Temizleme Jeli", "Cilt Bakımı", 18, "Garnier", 18.00m),
                        new Urun("Tonik", "Cilt Bakımı", 10, "Simple", 10.00m),
                        new Urun("Kadın Parfüm", "Parfüm", 25, "Avon", 25.00m),
                        new Urun("Erkek Parfüm", "Parfüm", 25, "Farmasi", 25.00m),
                        new Urun("Deodorant", "Parfüm", 30, "Nivea", 30.00m)
                    };
                }

                while (true)
                {
                    Console.WriteLine("\nMakyaj Mağazası Stok Yönetimi");
                    Console.WriteLine("1. Tüm Ürünleri Listele");
                    Console.WriteLine("2. Kategoriye Göre Listele");
                    Console.WriteLine("3. Stok Durumunu Güncelle");
                    Console.WriteLine("4. İndirimli Üyelik Ekle");
                    Console.WriteLine("5. Yeni Ürün Ekle");
                    Console.WriteLine("6. Ürün Sil");
                    Console.WriteLine("7. Ürün Ara");
                    Console.WriteLine("8. Fiyata Göre Sırala");
                    Console.WriteLine("9. Sepete Ürün Ekle");
                    Console.WriteLine("10. Sepeti Göster");
                    Console.WriteLine("11. İndirim Kodu Uygula");
                    Console.WriteLine("12. Favorilere Ekle");
                    Console.WriteLine("13. Favorileri Göster");
                    Console.WriteLine("14. İndirim Kodu Oluştur");
                    Console.WriteLine("15. İndirim Kodlarını Listele");
                    Console.WriteLine("16. İndirim Kodu Sil");
                    Console.WriteLine("17. Çıkış");
                    Console.Write("Seçiminizi yapın: ");

                    string secim = Console.ReadLine();
                    switch (secim)
                    {
                        case "1":
                            TumUrunleriListele(urunler);
                            break;
                        case "2":
                            KategoriyeGoreListele(urunler);
                            break;
                        case "3":
                            StokGuncelle(urunler);
                            break;
                        case "4":
                            IndirimliUyeligiEkle();
                            break;
                        case "5":
                            UrunEkle(urunler);
                            break;
                        case "6":
                            UrunSil(urunler);
                            break;
                        case "7":
                            UrunAra(urunler);
                            break;
                        case "8":
                            FiyataGoreSirala(urunler);
                            break;
                        case "9":
                            SepeteUrunEkle(urunler);
                            break;
                        case "10":
                            SepetiGoster();
                            break;
                        case "11":
                            IndirimKoduUygula();
                            break;
                        case "12":
                            FavorilereEkle(urunler);
                            break;
                        case "13":
                            FavorileriGoster();
                            break;
                        case "14":
                            IndirimKoduOlustur();
                            break;
                        case "15":
                            IndirimKodlariniListele();
                            break;
                        case "16":
                            IndirimKoduSil();
                            break;
                        case "17":
                            Console.WriteLine("Programdan çıkılıyor...");
                            UrunleriDosyayaKaydet(urunler);
                            return;
                        default:
                            Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                            break;
                    }
                }
            }
            else if (girisSecimi == "2")
            {
                // Müşteri Girişi
                List<Urun> urunler = UrunleriDosyadanOku();
                if (urunler.Count == 0)
                {
                    urunler = new List<Urun>
                    {
                        new Urun("Far", "Göz Makyajı", 10, "Maybelline", 10.00m),
                        new Urun("Rimel", "Göz Makyajı", 5, "Flormar", 5.00m),
                        new Urun("Eyeliner", "Göz Makyajı", 7, "Beaulis", 7.00m),
                        new Urun("Göz Kalemi", "Göz Makyajı", 6, "Lykd", 6.00m),
                        new Urun("Ruj", "Dudak Makyajı", 15, "Deborah", 15.00m),
                        new Urun("Gloss", "Dudak Makyajı", 9, "Maybelline", 9.00m),
                        new Urun("Lip Balm", "Dudak Makyajı", 12, "Flormar", 12.00m),
                        new Urun("Lip Oil", "Dudak Makyajı", 8, "Beaulis", 8.00m),
                        new Urun("Dudak Kalemi", "Dudak Makyajı", 10, "Lykd", 10.00m),
                        new Urun("Fondöten", "Ten Makyajı", 8, "Deborah", 8.00m),
                        new Urun("Kapatıcı", "Ten Makyajı", 11, "Maybelline", 11.00m),
                        new Urun("Highlighter", "Ten Makyajı", 5, "Flormar", 5.00m),
                        new Urun("Pudra", "Ten Makyajı", 7, "Beaulis", 7.00m),
                        new Urun("Allık", "Ten Makyajı", 12, "Lykd", 12.00m),
                        new Urun("Makyaj Bazı", "Ten Makyajı", 10, "Maybelline", 10.00m)
                    };
                }

                while (true)
                {
                    Console.WriteLine("\nMakyaj Mağazası Müşteri Menüsü");
                    Console.WriteLine("1. Tüm Ürünleri Listele");
                    Console.WriteLine("2. Kategoriye Göre Listele");
                    Console.WriteLine("3. Ürün Ara");
                    Console.WriteLine("4. Fiyata Göre Sırala");
                    Console.WriteLine("5. Sepete Ürün Ekle");
                    Console.WriteLine("6. Sepeti Göster");
                    Console.WriteLine("7. İndirim Kodu Uygula");
                    Console.WriteLine("8. Favorilere Ekle");
                    Console.WriteLine("9. Favorileri Göster");
                    Console.WriteLine("10. Çıkış");
                    Console.Write("Seçiminizi yapın: ");

                    string secim = Console.ReadLine();
                    switch (secim)
                    {
                        case "1":
                            TumUrunleriListele(urunler);
                            break;
                        case "2":
                            KategoriyeGoreListele(urunler);
                            break;
                        case "3":
                            UrunAra(urunler);
                            break;
                        case "4":
                            FiyataGoreSirala(urunler);
                            break;
                        case "5":
                            SepeteUrunEkle(urunler);
                            break;
                        case "6":
                            SepetiGoster();
                            break;
                        case "7":
                            IndirimKoduUygula();
                            break;
                        case "8":
                            FavorilereEkle(urunler);
                            break;
                        case "9":
                            FavorileriGoster();
                            break;
                        case "10":
                            Console.WriteLine("Çıkılıyor...");
                            return;
                        default:
                            Console.WriteLine("Geçersiz seçim. Lütfen tekrar deneyin.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Geçersiz seçim. Programdan çıkılıyor.");
            }
        }

        // Şifreyi gizli bir şekilde okuma fonksiyonu
        public static string SifreOku()
        {
            string sifre = "";
            ConsoleKeyInfo tuş;
            while ((tuş = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (tuş.Key == ConsoleKey.Backspace && sifre.Length > 0)
                {
                    sifre = sifre.Substring(0, sifre.Length - 1);
                    Console.Write("\b \b");
                }
                else if (tuş.Key != ConsoleKey.Backspace)
                {
                    sifre += tuş.KeyChar;
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return sifre;
        }

        // Ürünleri dosyadan okuma
        public static List<Urun> UrunleriDosyadanOku()
        {
            List<Urun> urunler = new List<Urun>();
            if (File.Exists("urunler.txt"))
            {
                var satirlar = File.ReadAllLines("urunler.txt", Encoding.UTF8);
                foreach (var satir in satirlar)
                {
                    var bilgiler = satir.Split(';');
                    if (bilgiler.Length == 5)
                    {
                        urunler.Add(new Urun(
                            bilgiler[0],
                            bilgiler[1],
                            int.Parse(bilgiler[2]),
                            bilgiler[3],
                            decimal.Parse(bilgiler[4])
                        ));
                    }
                }
            }
            return urunler;
        }

        // Ürünleri dosyaya kaydetme
        public static void UrunleriDosyayaKaydet(List<Urun> urunler)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var urun in urunler)
            {
                sb.AppendLine($"{urun.Ad};{urun.Kategori};{urun.Stok};{urun.Marka};{urun.Fiyat}");
            }
            File.WriteAllText("urunler.txt", sb.ToString(), Encoding.UTF8);
        }

        // Tüm ürünleri listeleme
        public static void TumUrunleriListele(List<Urun> urunler)
        {
            Console.WriteLine("\nTüm Ürünler:");
            foreach (var urun in urunler)
            {
                Console.WriteLine(urun);
            }
        }

        // Kategoriye göre ürün listeleme
        public static void KategoriyeGoreListele(List<Urun> urunler)
        {
            Console.Write("Kategori girin: ");
            string kategori = Console.ReadLine();

            var kategoridekiUrunler = urunler.Where(u => u.Kategori.ToLower() == kategori.ToLower()).ToList();
            if (kategoridekiUrunler.Any())
            {
                Console.WriteLine($"\n{kategori} Kategorisindeki Ürünler:");
                foreach (var urun in kategoridekiUrunler)
                {
                    Console.WriteLine(urun);
                }
            }
            else
            {
                Console.WriteLine("\nBu kategoride ürün bulunamadı.");
            }
        }

        // Stok durumu güncelleme
        public static void StokGuncelle(List<Urun> urunler)
        {
            Console.Write("Güncellemek istediğiniz ürün adını girin: ");
            string urunAdi = Console.ReadLine();
            var urun = urunler.FirstOrDefault(u => u.Ad.ToLower() == urunAdi.ToLower());

            if (urun != null)
            {
                Console.Write("Yeni stok miktarını girin: ");
                int yeniStok = int.Parse(Console.ReadLine());
                urun.Stok = yeniStok; // Stok güncellenir
                Console.WriteLine($"Yeni stok: {urun.Stok}");
            }
            else
            {
                Console.WriteLine("Bu ürün bulunamadı.");
            }
        }

        // İndirimli üyelik ekleme
        public static void IndirimliUyeligiEkle()
        {
            Console.Write("Yeni üye adı girin: ");
            string kullaniciAdi = Console.ReadLine();
            Console.Write("Yeni üye soyadı girin: ");
            string kullaniciSoyadi = Console.ReadLine();

            DateTime dogumTarihi = DateTime.MinValue;  // Başlangıçta geçerli bir değer atandı
            bool dogumTarihiGecerli = false;

            // Doğum tarihi alırken doğru formatı kontrol et
            while (!dogumTarihiGecerli)
            {
                Console.Write("Doğum tarihi girin (gg/aa/yyyy): ");
                string tarihInput = Console.ReadLine();

                // Tarih formatını kontrol et
                dogumTarihiGecerli = DateTime.TryParseExact(tarihInput, "dd/MM/yyyy",
                                                             System.Globalization.CultureInfo.InvariantCulture,
                                                             System.Globalization.DateTimeStyles.None,
                                                             out dogumTarihi);

                if (!dogumTarihiGecerli)
                {
                    Console.WriteLine("Lütfen (gg/aa/yyyy) şeklinde tuşlama yapınız.");
                }
            }

            Console.Write("Email adresi girin: ");
            string gmail = Console.ReadLine();
            Console.Write("Telefon numarası girin: ");
            string telefonNumarasi = Console.ReadLine();

            Uye yeniUye = new Uye(kullaniciAdi, kullaniciSoyadi, dogumTarihi, gmail, telefonNumarasi);
            Console.WriteLine($"Yeni üye eklendi: {yeniUye}");
        }


        // Ürün ekleme
        public static void UrunEkle(List<Urun> urunler)
        {
            Console.Write("Ürün adı: ");
            string ad = Console.ReadLine();

            Console.Write("Ürün kategorisi: ");
            string kategori = Console.ReadLine();

            Console.Write("Ürün markası: ");
            string marka = Console.ReadLine();

            // Mevcut ürünü kontrol et (büyük/küçük harf duyarsız)
            var mevcutUrun = urunler.FirstOrDefault(u =>
                u.Ad.Equals(ad, StringComparison.OrdinalIgnoreCase) &&
                u.Kategori.Equals(kategori, StringComparison.OrdinalIgnoreCase) &&
                u.Marka.Equals(marka, StringComparison.OrdinalIgnoreCase));

            if (mevcutUrun != null)
            {
                Console.WriteLine("Bu ürün zaten mevcut.");
                return;
            }

            Console.Write("Stok miktarı: ");
            if (!int.TryParse(Console.ReadLine(), out int stok))
            {
                Console.WriteLine("Geçersiz stok miktarı!");
                return;
            }

            Console.Write("Ürün fiyatı: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal fiyat))
            {
                Console.WriteLine("Geçersiz fiyat!");
                return;
            }

            Urun yeniUrun = new Urun(ad, kategori, stok, marka, fiyat);
            urunler.Add(yeniUrun);

            UrunleriDosyayaKaydet(urunler);
            Console.WriteLine("Ürün başarıyla eklendi.");
        }

        // Ürün silme
        public static void UrunSil(List<Urun> urunler)
        {
            Console.Write("Silmek istediğiniz ürün adını girin: ");
            string urunAdi = Console.ReadLine();
            var urun = urunler.FirstOrDefault(u => u.Ad.Equals(urunAdi, StringComparison.OrdinalIgnoreCase));

            if (urun != null)
            {
                Console.Write("Bu ürünü silmek istediğinizden emin misiniz? (Evet/Hayır): ");
                string onay = Console.ReadLine();

                if (onay.Equals("Evet", StringComparison.OrdinalIgnoreCase))
                {
                    urunler.Remove(urun);

                    // Dosyaya kaydet
                    UrunleriDosyayaKaydet(urunler);
                    Console.WriteLine("Ürün başarıyla silindi.");
                }
                else
                {
                    Console.WriteLine("Ürün silinmedi.");
                }
            }
            else
            {
                Console.WriteLine("Bu ürün bulunamadı.");
            }
        }

        // Ürün ara
        public static void UrunAra(List<Urun> urunler)
        {
            Console.Write("Aramak istediğiniz ürün adını veya markayı girin: ");
            string aramaKelimesi = Console.ReadLine().ToLower();

            var bulunanUrunler = urunler.Where(u =>
                u.Ad.ToLower().Contains(aramaKelimesi) ||
                u.Marka.ToLower().Contains(aramaKelimesi)
            ).ToList();

            if (bulunanUrunler.Any())
            {
                Console.WriteLine("\nBulunan Ürünler:");
                foreach (var urun in bulunanUrunler)
                {
                    Console.WriteLine(urun);
                }
            }
            else
            {
                Console.WriteLine("\nAradığınız kriterlere uygun ürün bulunamadı.");
            }
        }

        // Fiyata göre sıralama
        public static void FiyataGoreSirala(List<Urun> urunler)
        {
            Console.WriteLine("\n1. Artan Fiyat");
            Console.WriteLine("2. Azalan Fiyat");
            Console.Write("Seçiminizi yapın: ");
            string secim = Console.ReadLine();

            if (secim == "1")
            {
                var siraliUrunler = urunler.OrderBy(u => u.Fiyat).ToList();
                Console.WriteLine("\nÜrünler (Artan Fiyat):");
                foreach (var urun in siraliUrunler)
                {
                    Console.WriteLine(urun);
                }
            }
            else if (secim == "2")
            {
                var siraliUrunler = urunler.OrderByDescending(u => u.Fiyat).ToList();
                Console.WriteLine("\nÜrünler (Azalan Fiyat):");
                foreach (var urun in siraliUrunler)
                {
                    Console.WriteLine(urun);
                }
            }
            else
            {
                Console.WriteLine("Geçersiz seçim!");
            }
        }

        // Yeni metodlar
        public static void SepeteUrunEkle(List<Urun> urunler)
        {
            Console.Write("Eklemek istediğiniz ürün adını girin: ");
            string urunAdi = Console.ReadLine();
            var urun = urunler.FirstOrDefault(u => u.Ad.ToLower() == urunAdi.ToLower());

            if (urun != null)
            {
                Console.Write("Miktar girin: ");
                if (int.TryParse(Console.ReadLine(), out int miktar) && miktar > 0)
                {
                    if (miktar <= urun.Stok)
                    {
                        aktifSepet.UrunEkle(urun, miktar);
                        Console.WriteLine("Ürün sepete eklendi.");
                        SepetiGoster();
                    }
                    else
                    {
                        Console.WriteLine("Yetersiz stok!");
                    }
                }
                else
                {
                    Console.WriteLine("Geçersiz miktar!");
                }
            }
            else
            {
                Console.WriteLine("Ürün bulunamadı.");
            }
        }

        public static void SepetiGoster()
        {
            if (aktifSepet.Urunler.Count == 0)
            {
                Console.WriteLine("\nSepetiniz boş.");
                return;
            }

            Console.WriteLine("\nSepetinizdeki Ürünler:");
            foreach (var item in aktifSepet.Urunler)
            {
                Console.WriteLine($"{item.Urun.Ad} - {item.Miktar} adet - Birim Fiyat: {item.Urun.Fiyat:C} - Toplam: {(item.Urun.Fiyat * item.Miktar):C}");
            }
            Console.WriteLine($"\nToplam Tutar: {aktifSepet.ToplamFiyat:C}");
        }

        public static void IndirimKoduUygula()
        {
            if (aktifSepet.Urunler.Count == 0)
            {
                Console.WriteLine("Sepetiniz boş!");
                return;
            }

            Console.Write("İndirim kodunu girin: ");
            string kod = Console.ReadLine().ToUpper();

            if (indirimKodlari.TryGetValue(kod, out decimal indirimOrani))
            {
                decimal indirimMiktari = aktifSepet.ToplamFiyat * indirimOrani;
                decimal indirimliToplam = aktifSepet.ToplamFiyat - indirimMiktari;
                Console.WriteLine($"İndirim uygulandı!");
                Console.WriteLine($"Toplam Tutar: {aktifSepet.ToplamFiyat:C}");
                Console.WriteLine($"İndirim Miktarı: {indirimMiktari:C}");
                Console.WriteLine($"İndirimli Toplam: {indirimliToplam:C}");
            }
            else
            {
                Console.WriteLine("Geçersiz indirim kodu!");
            }
        }

        public static void FavorilereEkle(List<Urun> urunler)
        {
            Console.Write("Favorilere eklemek istediğiniz ürün adını girin: ");
            string urunAdi = Console.ReadLine();
            var urun = urunler.FirstOrDefault(u => u.Ad.ToLower() == urunAdi.ToLower());

            if (urun != null)
            {
                favoriler.UrunEkle(urun);
                Console.WriteLine("Ürün favorilere eklendi.");
            }
            else
            {
                Console.WriteLine("Ürün bulunamadı.");
            }
        }

        public static void FavorileriGoster()
        {
            if (favoriler.Urunler.Count == 0)
            {
                Console.WriteLine("\nFavori listeniz boş.");
                return;
            }

            Console.WriteLine("\nFavori Ürünleriniz:");
            foreach (var urun in favoriler.Urunler)
            {
                Console.WriteLine(urun);
            }
        }

        public static void IndirimKoduOlustur()
        {
            Console.WriteLine("\nİndirim Kodu Oluşturma");
            Console.Write("Yeni indirim kodu girin (örn: YENI50): ");
            string yeniKod = Console.ReadLine().ToUpper();

            if (indirimKodlari.ContainsKey(yeniKod))
            {
                Console.WriteLine("Bu kod zaten mevcut!");
                return;
            }

            Console.Write("İndirim oranını girin (örn: 0.15 = %15 indirim): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal oran) && oran > 0 && oran <= 1)
            {
                indirimKodlari.Add(yeniKod, oran);
                IndirimKodlariniDosyayaKaydet(); // Yeni metod
                Console.WriteLine($"İndirim kodu başarıyla oluşturuldu: {yeniKod} - %{oran * 100:0}");
            }
            else
            {
                Console.WriteLine("Geçersiz indirim oranı! 0 ile 1 arasında bir değer girin.");
            }
        }

        public static void IndirimKodlariniListele()
        {
            Console.WriteLine("\nMevcut İndirim Kodları:");
            foreach (var kod in indirimKodlari)
            {
                Console.WriteLine($"{kod.Key} - %{kod.Value * 100:0} indirim");
            }
        }

        public static void IndirimKoduSil()
        {
            Console.Write("Silmek istediğiniz indirim kodunu girin: ");
            string kod = Console.ReadLine().ToUpper();

            if (indirimKodlari.Remove(kod))
            {
                IndirimKodlariniDosyayaKaydet(); // Yeni metod
                Console.WriteLine("İndirim kodu başarıyla silindi.");
            }
            else
            {
                Console.WriteLine("Böyle bir indirim kodu bulunamadı.");
            }
        }

        // İndirim kodlarını dosyada saklamak için yeni metodlar
        private static void IndirimKodlariniDosyayaKaydet()
        {
            string dosyaYolu = "indirimkodlari.txt";
            StringBuilder sb = new StringBuilder();
            foreach (var kod in indirimKodlari)
            {
                sb.AppendLine($"{kod.Key};{kod.Value}");
            }
            File.WriteAllText(dosyaYolu, sb.ToString(), Encoding.UTF8);
        }

        private static void IndirimKodlariniDosyadanOku()
        {
            string dosyaYolu = "indirimkodlari.txt";
            if (File.Exists(dosyaYolu))
            {
                var satirlar = File.ReadAllLines(dosyaYolu, Encoding.UTF8);
                foreach (var satir in satirlar)
                {
                    var bilgiler = satir.Split(';');
                    if (bilgiler.Length == 2 && decimal.TryParse(bilgiler[1], out decimal oran))
                    {
                        indirimKodlari[bilgiler[0]] = oran;
                    }
                }
            }
        }
    }
}
