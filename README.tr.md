# ATS Resume Builder

ATS Resume Builder, CV hazırlama sürecini sadeleştirmek için geliştirdiğim bir özgeçmiş oluşturma uygulamasıdır. Kullanıcıların karmaşık bir tasarım aracını öğrenmek yerine deneyimlerine, yeteneklerine ve anlatmak istediklerine odaklanmasını amaçlar.

[English documentation](README.md)

Temel kullanım akışı oldukça basittir:

1. Özgeçmiş bilgilerinizi girin.
2. Değişiklikleri canlı önizlemede takip edin.
3. İhtiyacınıza uygun şablonu seçin.
4. Özgeçmişinizi PDF olarak dışa aktarın veya kaydedin.

## Neden Geliştirdim?

CV hazırlama sürecinde doğru formatı bulmak, ATS uyumluluğunu korumak ve gereksiz tasarım karmaşasından kaçınmak çoğu zaman düşündüğümüzden daha zor olabiliyor.

Bu ihtiyacı ilk olarak bir arkadaşım CV hazırlarken fark ettim. Süreç ilerledikçe bunun yalnızca tek bir kişiye özel olmadığını, birçok kişinin işine yarayabilecek ve daha sade hâle getirilebilecek bir problem olduğunu da gördüm.

Birçok CV hazırlama aracında kullanıcılar fazla sayıda görsel seçenek, karışık kullanım akışları veya temel PDF çıktısı için ücretli adımlarla karşılaşabiliyor. Bu projede amacım, bu süreci daha sade, erişilebilir ve içeriği ön planda tutan bir akışa indirmekti.

Bu fikirden yola çıkarak **ATS Resume Builder** adında, kullanıcıların bilgilerini kolayca girip canlı önizleme üzerinden CV’lerini düzenleyebildiği, isteğe bağlı olarak fotoğraf ekleyebildiği ve ATS uyumlu bir formatta PDF olarak kaydedebildiği bir web uygulaması geliştirdim.

## Özellikler

- Düzenleme sırasında anlık canlı önizleme
- Önizlemede A4 sayfa sınırlarını görebilme
- Sade ve tek sütunlu ATS uyumlu şablon
- İsteğe bağlı profil fotoğrafı destekleyen modern şablon
- İngilizce ve Türkçe arayüz etiketleri
- Açık/koyu uygulama teması
- Dinamik iş deneyimi ve gönüllü deneyim bölümleri
- Eğitim, projeler ve farklı mesleklere uyarlanabilen yetenek kategorileri
- Diller ve detay/konu eklenebilen sertifikalar
- İsteğe bağlı referanslar ve kişisel ek alanlar
- Opsiyonel bağlantılar ve kişisel detaylar için tek merkez olarak Ek Alanlar
- LinkedIn, GitHub, Portföy, web sitesi, Medium veya LeetCode gibi bağlantıları tanıyan özel alanlar
- Bağlantılar için Etiket, Kısa URL veya Tam URL olarak görünüm seçebilme
- Tarayıcı üzerinden **PDF olarak Kaydet** desteği
- Yerel geliştirme ortamında opsiyonel backend üzerinden doğrudan PDF indirme
- Responsive tasarım

En iyi ATS uyumluluğu için fotoğrafsız ve metin tabanlı ATS uyumlu şablon kullanılabilir.

### Ek Alanlar ve Bağlantılar

Opsiyonel kişisel bağlantılar **Ek Alanlar** üzerinden yönetilir. Böylece ana Kişisel Bilgiler formu sade kalır. Her ek alan basit bir **Etiket** ve **Değer** çiftinden oluşur; bağlantı veya normal bir kişisel detay için kullanılabilir.

`meltemmeydan.dev` veya `github.com/example` gibi domain içeren değerler, başında `https://` olmasa bile bağlantı olarak algılanır. Bir değer URL olarak algılandığında kullanıcı bağlantının özgeçmişte nasıl görüneceğini seçebilir:

- **Etiket**, ek alanın etiketini gösterir.
- **Kısa URL**, protokolü, `www.` bölümünü ve sondaki eğik çizgiyi kaldırır.
- **Tam URL**, girilen değeri olduğu gibi gösterir.

Protokolsüz bağlantılara uygulama içinde otomatik olarak `https://` eklenir ve bağlantılar tıklanabilir kalır. Yeni eklenen alanlarda da değer URL olarak algılandığında görünüm seçenekleri otomatik olarak açılır. URL olmayan normal metin değerlerinde bu seçenekler gösterilmez. Boş alanlar önizleme veya PDF çıktısında yer almaz.

### A4 Önizleme

Özgeçmiş önizlemesi A4 sayfa yapısını daha anlaşılır gösterecek şekilde düzenlenmiştir. İçerik uzadıkça yeni sayfa başlangıçları önizleme alanında görsel olarak ayrılır. Böylece kullanıcı CV’nin PDF çıktısında kaç sayfaya yayılacağını ve sayfa geçişlerinin nerede oluşacağını daha kolay takip edebilir.

Bu görsel sayfa ayrımları yalnızca önizleme deneyimini iyileştirmek için kullanılır; PDF çıktısında gereksiz çizgi, etiket veya önizleme yardımcısı gösterilmez.

## PDF Dışa Aktarma

Proje iki farklı PDF dışa aktarma biçimini destekler.

### Yalnızca Frontend Modu

`VITE_API_BASE_URL` tanımlı değilse uygulamada **PDF olarak Kaydet** butonu görünür. Bu buton tarayıcının yazdırma ekranını açar ve özgeçmişin PDF olarak kaydedilmesini sağlar.

Fotoğraflı Modern şablon seçildiğinde yüklenen profil fotoğrafı tarayıcı tarafından oluşturulan PDF'de korunur. ATS uyumlu şablonda fotoğraf gösterilmez ve dışa aktarılmaz.

### Opsiyonel Backend Modu

Repoda QuestPDF kullanan bir .NET 8 backend de bulunur. Frontend aşağıdaki değişkenle yapılandırıldığında ve backend çalıştığında doğrudan **PDF İndir** özelliği etkinleşir:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Backend zorunlu değildir. Yerel geliştirme ve referans uygulama olarak repoda tutulur; canlı frontend çalışmak için backend'e ihtiyaç duymaz.

## Canlı Yayın ve Cloudflare Pages

Frontend, Cloudflare Pages üzerinde yayınlanır. Cloudflare Pages yalnızca React/Vite frontend uygulamasını barındırır; .NET backend production ortamında ayrı olarak host edilmez.

Bu tercih canlı demoyu basit ve ücretsiz tutmak, ayrıca ayrı bir backend hosting servisi gerektirmemek için yapılmıştır. Production ortamında `VITE_API_BASE_URL` tanımlanmaz ve uygulama tarayıcı üzerinden **PDF olarak Kaydet** akışını kullanır.

Cloudflare Pages ayarları:

```text
Root directory: frontend
Build command: npm run build
Output directory: dist
VITE_API_BASE_URL: boş bırakılmalı
```

## Ekran Görüntüleri

Ekran görüntüleri buraya eklenecek.

Önerilen klasör: `docs/screenshots/`

## Proje Yapısı

```text
ats-resume-builder/
|-- frontend/                     React + Vite uygulaması
|-- backend/
|   `-- AtsResumeBuilder.Api/     Opsiyonel .NET 8 QuestPDF API
|-- .gitignore
|-- README.md
`-- README.tr.md
```

## Yerel Ortamda Çalıştırma

### Frontend

Gereksinimler:

- Node.js 20.19+ veya 22.12+
- npm

```bash
cd frontend
npm install
npm run dev
```

Vite tarafından terminalde gösterilen yerel adresi tarayıcıda açın.

Herhangi bir environment değişkeni tanımlanmadığında frontend tek başına çalışır ve tarayıcı üzerinden **PDF olarak Kaydet** özelliğini kullanır.

### Opsiyonel Backend

Gereksinim:

- .NET 8 SDK

```bash
cd backend/AtsResumeBuilder.Api
dotnet restore
dotnet run
```

Yerel ortamda doğrudan PDF indirmeyi etkinleştirmek için `frontend/.env.local` dosyasını oluşturun:

```env
VITE_API_BASE_URL=http://localhost:5000
```

Environment değişkenini değiştirdikten sonra Vite geliştirme sunucusunu yeniden başlatın.

Backend API adresi:

```text
http://localhost:5000
```

Swagger adresi:

```text
http://localhost:5000/swagger
```

## Doğrulama

Frontend:

```bash
cd frontend
npm run lint
npm run build
```

Backend:

```bash
cd backend/AtsResumeBuilder.Api
dotnet build
```

## Kullanılan Teknolojiler

Frontend:

- React
- Vite
- JavaScript
- CSS

Opsiyonel backend:

- ASP.NET Core Web API
- .NET 8
- QuestPDF
- Swagger

## Bilinen Sınırlamalar

- Özgeçmiş verileri sayfa yenilendiğinde saklanmaz.
- Tarayıcı tarafından oluşturulan PDF çıktısı tarayıcıya göre küçük farklılıklar gösterebilir.
- Doğrudan backend PDF indirme özelliği için opsiyonel API'nin çalışıyor olması gerekir.
- Henüz otomatik testler eklenmemiştir.

