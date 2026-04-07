# Technical Service Management

Windows Forms tabanli bu proje, elektronik cihaz teknik servis surecini masaustu ortaminda yonetmek icin gelistirilmis 3 katmanli bir uygulamadir. Uygulama; musteri kaydi, ariza kaydi, servis operasyonlari, yedek parca stok takibi ve is durum guncelleme adimlarini tek bir akista toplar.

Bu depo teslim odakli olacak sekilde sadelestirildi. Yalnizca guncel projeye ait kaynak kodlar, cozum dosyasi ve gerekli dokumantasyon birakildi.

## Proje Ozeti

- Musteri ekleme ve listeleme
- Cihaz ve servis talebi olusturma
- Servis talebi durumu guncelleme
- Islem kaydi ve maliyet takibi
- Yedek parca ekleme / stok guncelleme
- Servis talebine parca atama
- Dusuk stok kontrolu
- Dashboard uzerinden genel durum ozeti goruntuleme

## Kullanilan Teknolojiler

- C#
- .NET 8
- Windows Forms
- Entity Framework Core 8
- SQLite

## Mimari

Proje 3 katmanli yapida hazirlanmistir:

### 1. UI Katmani

`TechnicalServiceManagement.UI`

- `DashboardForm`: genel ozet ve son servis talepleri
- `CustomerForm`: musteri kaydi ve listeleme
- `ServiceRequestForm`: servis talebi olusturma, durum guncelleme, operasyon ve parca atama
- `SparePartForm`: stok yonetimi

### 2. Business Katmani

`TechnicalServiceManagement.Business`

- `CustomerManager`
- `ServiceRequestManager`
- `SparePartManager`
- `ApplicationBootstrapper`

Bu katmanda dogrulama ve is kurallari bulunur.

### 3. Data Katmani

`TechnicalServiceManagement.Data`

- Entity siniflari
- `TechnicalServiceDbContext`
- `TechnicalServiceDbContextFactory`

Bu katman SQLite veritabani ve Entity Framework Core yapisini yonetir.

## Proje Yapisi

```text
Gorsel Programlama/
|-- README.md
|-- TechnicalServiceManagement.sln
|-- docs/
|   |-- TechnicalServiceManagement_ProjectPlanning.md
|   |-- form-navigation-diagram.svg
|   `-- use-case-diagram.svg
|-- TechnicalServiceManagement.Business/
|-- TechnicalServiceManagement.Data/
`-- TechnicalServiceManagement.UI/
```

## Calistirma Adimlari

### Gereksinimler

- Windows 10 veya Windows 11
- .NET 8 SDK
- Visual Studio 2022 veya `dotnet` CLI

### Komut Satirindan Calistirma

```powershell
dotnet restore .\TechnicalServiceManagement.sln
dotnet build .\TechnicalServiceManagement.sln -c Release
dotnet run --project .\TechnicalServiceManagement.UI\TechnicalServiceManagement.UI.csproj
```

### Visual Studio ile Calistirma

1. `TechnicalServiceManagement.sln` dosyasini acin.
2. Baslangic projesi olarak `TechnicalServiceManagement.UI` secili olsun.
3. `F5` veya `Ctrl + F5` ile uygulamayi calistirin.

## Veritabani Davranisi

Uygulama ilk calistiginda SQLite veritabani otomatik olarak olusturulur. Veritabani dosyasi calisma dizini altinda su klasorde uretilir:

```text
TechnicalServiceManagement.UI/bin/<Configuration>/net8.0-windows/AppData/technical-service-management.db
```

Bu dosya kaynak koda dahil edilmez; uygulama calisma aninda otomatik uretilir.

## Ornek Kullanim Akisi

1. Once `Customers` ekranindan bir musteri ekleyin.
2. `Spare Parts` ekranindan stok girisi yapin.
3. `Service Requests` ekranindan cihaz ve ariza bilgileriyle servis kaydi olusturun.
4. Ayni ekrandan servis durumunu guncelleyin.
5. Servis operasyonu ve gerekiyorsa yedek parca atayin.
6. `Dashboard` ekranindan toplam is sayisi, aktif isler ve dusuk stok bilgisini takip edin.

## Dokumantasyon

- [Project planning report](docs/TechnicalServiceManagement_ProjectPlanning.md)
- [Use case diagram](docs/use-case-diagram.svg)
- [Form navigation diagram](docs/form-navigation-diagram.svg)

## Teslim Notu

Repo teslime uygun olacak sekilde temizlenmistir:

- Eski odevlere ait dosyalar kaldirildi.
- `bin`, `obj`, IDE cache ve gecici log dosyalari temizlendi.
- Gereksiz dokuman kopyalari elendi.
- Koke guncel bir `README.md` eklendi.
