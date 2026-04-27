using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace _20240305302_Akin_Kokcu
{
    public class StockManager
    {
        private List<Product> stockList;
        private readonly string filePath = "stockData.json";

        public StockManager()
        {
            stockList = new List<Product>();
            LoadData();
        }

        public bool IsSequenceNumberTaken(int sequenceNumber)
        {
            return stockList.Any(p => p.SequenceNumber == sequenceNumber);
        }

        public void AddProduct(Product newProduct)
        {
            var existingProduct = stockList.FirstOrDefault(p =>
                p.ProductName == newProduct.ProductName &&
                p.WarehouseName == newProduct.WarehouseName);

            if (existingProduct != null)
            {
                existingProduct.UnitNumber += newProduct.UnitNumber;
                existingProduct.CountDate = DateTime.Now;

                Console.WriteLine($"\nBİLGİ: '{existingProduct.ProductName}' adlı ürün '{existingProduct.WarehouseName}' deposunda zaten mevcut.");
                Console.WriteLine($"Stok adedi üzerine eklendi. Yeni Toplam Adet: {existingProduct.UnitNumber}\n");
                SaveData();
                return;
            }

            stockList.Add(newProduct);
            Console.WriteLine($"\nBAŞARILI: {newProduct.ProductName} başarıyla stoka eklendi.\n");
            SaveData();
        }

        public void ListProducts()
        {
            if (stockList.Count == 0)
            {
                Console.WriteLine("Şu anda stokta hiç ürün bulunmuyor.");
                return;
            }

            Console.WriteLine("\n--- Güncel Stok Listesi ---");
            foreach (var product in stockList)
            {
                Console.WriteLine($"Sıra: {product.SequenceNumber} | Ürün: {product.ProductName} | Adet: {product.UnitNumber} | Depo: {product.WarehouseName}");
            }
            Console.WriteLine("---------------------------\n");
        }

        public void SearchProduct(string keyword)
        {
            var results = stockList.Where(p => p.ProductName != null && p.ProductName.Contains(keyword)).ToList();

            if (results.Count > 0)
            {
                Console.WriteLine($"\n--- '{keyword}' İçin Arama Sonuçları ---");
                foreach (var product in results)
                {
                    Console.WriteLine($"Sıra: {product.SequenceNumber} | Ürün: {product.ProductName} | Adet: {product.UnitNumber} | Depo: {product.WarehouseName}");
                }
                Console.WriteLine("--------------------------------------\n");
            }
            else
            {
                Console.WriteLine("Aradığınız kritere uygun ürün bulunamadı.");
            }
        }

        public void DeleteProduct(int sequenceNumber)
        {
            var productToRemove = stockList.FirstOrDefault(p => p.SequenceNumber == sequenceNumber);

            if (productToRemove != null)
            {
                stockList.Remove(productToRemove);
                Console.WriteLine($"{productToRemove.ProductName} stoktan başarıyla silindi.");
                SaveData();
            }
            else
            {
                Console.WriteLine("Bu sıra numarasına sahip bir ürün bulunamadı.");
            }
        }

        public void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(stockList, options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri kaydedilirken bir hata oluştu: {ex.Message}");
            }
        }

        public void LoadData()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    stockList = JsonSerializer.Deserialize<List<Product>>(jsonString) ?? new List<Product>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kayıtlı veriler yüklenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
}