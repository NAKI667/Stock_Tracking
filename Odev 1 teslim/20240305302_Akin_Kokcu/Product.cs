using System;

namespace _20240305302_Akin_Kokcu
{
    public class Product
    {
        public int SequenceNumber { get; set; }
        public DateTime CountDate { get; set; }
        public string? ProductName { get; set; }
        public string? Brand { get; set; }
        public string? PackageType { get; set; }
        public string? WarehouseName { get; set; }
        public string? Location { get; set; }
        public int UnitNumber { get; set; }
    }
}