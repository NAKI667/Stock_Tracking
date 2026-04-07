using System;

namespace _20240305302_Akin_Kokcu
{
    class Program
    {
        static void Main(string[] args)
        {
            StockManager manager = new StockManager();

            while (true)
            {
                Console.WriteLine("\n=== STOK TAKİP SİSTEMİ ===");
                Console.WriteLine("1. Ürün Ekle");
                Console.WriteLine("2. Ürünleri Listele");
                Console.WriteLine("3. Ürün Ara");
                Console.WriteLine("4. Ürün Sil");
                Console.WriteLine("5. Çıkış");
                Console.Write("Seçiminiz: ");

                string? secim = Console.ReadLine();

                if (secim == "1")
                {
                    Product newProduct = new Product();

                    int sequenceNumber;
                    Console.Write("Sıra No: ");
                    while (true)
                    {
                        if (!int.TryParse(Console.ReadLine(), out sequenceNumber))
                        {
                            Console.WriteLine("Hatalı giriş! Lütfen harf değil, sadece rakam giriniz.");
                            Console.Write("Sıra No: ");
                            continue;
                        }

                        if (manager.IsSequenceNumberTaken(sequenceNumber))
                        {
                            Console.WriteLine("Hata! Bu sıra numarası başka bir ürün tarafından kullanılıyor. Lütfen yeni bir numara girin.");
                            Console.Write("Sıra No: ");
                            continue;
                        }

                        break;
                    }
                    newProduct.SequenceNumber = sequenceNumber;

                    Console.Write("Ürün Adı (Zorunlu): ");
                    string? pName = Console.ReadLine();
                    while (string.IsNullOrWhiteSpace(pName) || char.IsDigit(pName[0]))
                    {
                        if (string.IsNullOrWhiteSpace(pName))
                        {
                            Console.WriteLine("Hata! Ürün adı boş bırakılamaz.");
                        }
                        else
                        {
                            Console.WriteLine("Hata! Ürün adı sayıyla başlayamaz.");
                        }
                        Console.Write("Ürün Adı (Zorunlu): ");
                        pName = Console.ReadLine();
                    }
                    newProduct.ProductName = pName;

                    int unitNumber;
                    Console.Write("Adet (Zorunlu): ");
                    while (!int.TryParse(Console.ReadLine(), out unitNumber))
                    {
                        Console.WriteLine("Hatalı giriş! Lütfen harf değil, sadece rakam giriniz.");
                        Console.Write("Adet (Zorunlu): ");
                    }
                    newProduct.UnitNumber = unitNumber;

                    Console.Write("Depo Adı (Zorunlu): ");
                    string? wName = Console.ReadLine();
                    while (string.IsNullOrWhiteSpace(wName) || char.IsDigit(wName[0]))
                    {
                        if (string.IsNullOrWhiteSpace(wName))
                        {
                            Console.WriteLine("Hata! Depo adı boş bırakılamaz.");
                        }
                        else
                        {
                            Console.WriteLine("Hata! Depo adı sayıyla başlayamaz.");
                        }
                        Console.Write("Depo Adı (Zorunlu): ");
                        wName = Console.ReadLine();
                    }
                    newProduct.WarehouseName = wName;

                    newProduct.CountDate = DateTime.Now;

                    manager.AddProduct(newProduct);
                }
                else if (secim == "2")
                {
                    manager.ListProducts();
                }
                else if (secim == "3")
                {
                    Console.Write("Aramak istediğiniz ürünün adını girin: ");
                    string? keyword = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        manager.SearchProduct(keyword);
                    }
                    else
                    {
                        Console.WriteLine("Arama yapmak için geçerli bir kelime girmelisiniz.");
                    }
                }
                else if (secim == "4")
                {
                    Console.Write("Silmek istediğiniz ürünün Sıra No'sunu girin: ");
                    int seqNo;
                    if (int.TryParse(Console.ReadLine(), out seqNo))
                    {
                        manager.DeleteProduct(seqNo);
                    }
                    else
                    {
                        Console.WriteLine("Hatalı giriş yaptınız. Lütfen geçerli bir rakam girin.");
                    }
                }
                else if (secim == "5")
                {
                    Console.WriteLine("Program kapatılıyor. İyi günler!");
                    break;
                }
                else
                {
                    Console.WriteLine("Hatalı seçim yaptınız, lütfen tekrar deneyin.");
                }
            }
        }
    }
}