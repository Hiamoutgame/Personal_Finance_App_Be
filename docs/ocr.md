# OCR hoa don va trich xuat giao dich

Tai lieu nay ghi context bat buoc khi lam lai chuc nang OCR hoa don cho FinJar.

## Muc tieu

Flow OCR khong duoc chi noi tat ca text thanh mot chuoi dai. Can tach thanh 2 giai doan doc lap:

1. Vision OCR: nhan anh hoa don, tra ve cac khoi chu co toa do.
2. Information Extraction: dung thuat toan C# de gom dong, tim tong tien, nhan dien cua hang va goi y category.

Ket qua cuoi cung dung de tao preview giao dich chi tieu, sau do user confirm moi ghi transaction that.

## Hien trang code sau refactor

Code lien quan:

- `Personal_Finance_Management.Service/ocr/IService.cs`: interface Vision OCR.
- `Personal_Finance_Management.Service/ocr/Service.cs`: hien van goi OCR service ngoai qua HTTP, nhung da parse raw JSON thanh `OcrTextBlock` va `OcrTextLine`.
- `Personal_Finance_Management.Service/ocr/OCRResult.cs`: DTO OCR gom raw text, bounding boxes, lines va `ReceiptExtractionResult`.
- `Personal_Finance_Management.Service/ocr/IReceiptParserService.cs`: interface parsing nghiep vu.
- `Personal_Finance_Management.Service/ocr/ReceiptParserService.cs`: thuat toan gom line theo toa do Y, tim tong tien va goi y category.
- `Personal_Finance_Management.Service/import/Service.cs`: upload file, chay OCR, goi receipt parser, luu `ImportJob`/`ImportTransactionDraft`, va confirm draft thanh `Transaction`.
- `Personal_Finance_Management.Api/Program.cs`: dang ky DI cho OCR HTTP client va receipt parser.

Endpoint upload hien tai:

```http
POST /api/v1/imports
POST /api/v1/imports/image
```

Flow dung cho OCR receipt:

1. Upload/OCR: BE luu file, luu `ImportJob`, luu draft dau tien trong `ImportTransactionDraft`.
2. Review: FE lay `GET /api/v1/imports/{id}`, cho user sua draft va chon nguon tien.
3. Edit: FE goi `PATCH /api/v1/imports/{id}` hoac `PATCH /api/v1/imports/{id}/drafts/{draftId}` neu user sua amount/date/category/note/hu.
4. Confirm: FE goi `POST /api/v1/imports/{id}/confirm` de tao `Transaction` that.

Luu y nghiep vu:

- `ImportTransactionDraft` la log/preview/audit cua OCR, giu `NormalizedPayloadJson`.
- `Transaction` la ban ghi tai chinh that, co `SourceType = OCR`, `ImportJobId`, va `RawPayloadJson` copy tu draft.
- Neu confirm bang `fromJarId`, transaction tru tien tu hu va khong gan `financialAccountId`.
- Neu confirm bang `financialAccountId`, transaction tru/cong `FinancialAccount.CurrentBalance`.
- Neu FE chua co man chon vi, BE se fallback ve `ImportJob.FinancialAccountId`, nhung UI nen co buoc review/chon nguon tien truoc khi confirm.

Response co them:

- `preview`: object nhe cho FE render mac dinh.
- `ocrResult.blocks`: danh sach bounding boxes.
- `ocrResult.lines`: cac dong da gom theo toa do Y.
- `receipt.totalAmount`: tong tien trich xuat.
- `receipt.transactionDate`: ngay hoa don/giao dich neu parse duoc.
- `receipt.merchantName`: ten cua hang/thuong hieu du doan.
- `receipt.suggestedCategoryId`, `receipt.suggestedCategoryName`: category goi y neu match duoc.

Mac dinh `rawOcrJson` va `ocrResult` khong duoc tra ve de response nhe cho FE. Neu can debug OCR thi FE gui form field:

```text
includeDebug=true
```

Khi do BE moi tra them raw OCR, blocks, lines va bbox.

FE nen uu tien render:

```json
{
  "preview": {
    "id": "ocr-result-id",
    "status": "success",
    "imageUrl": "/api/v1/imports/images/file.jpg",
    "transaction": {
      "merchantName": "FamilyMart",
      "amount": 25000,
      "date": "2017-07-14T00:00:00+00:00",
      "type": "Expense",
      "suggestedCategoryId": null,
      "suggestedCategoryName": "Mua sam",
      "matchedBy": "merchant-alias",
      "note": "Hoa don FamilyMart"
    },
    "items": [],
    "summary": {
      "subtotal": 27000,
      "discount": 2000,
      "total": 25000
    },
    "warnings": []
  }
}
```

## Giai doan 1: Vision OCR

Target provider khi lam lai day du:

- Dung `Sdcb.PaddleOCR` trong C# de OCR local.
- Input: anh hoa don user upload.
- Output bat buoc: danh sach block chu, moi block co:
  - `text`
  - `x`
  - `y`
  - `width`
  - `height`
  - `confidence`
  - `pageNumber`

DTO noi bo dang dung:

```csharp
public class OcrTextBlock
{
    public required string Text { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal? Confidence { get; set; }
    public int PageNumber { get; set; } = 1;
}
```

Luu y khi thay HTTP OCR bang PaddleOCR:

- Giu interface `ocr.IService.ReadImageAsync(...)`.
- Chi thay implementation ben trong `ocr/Service.cs` hoac tach thanh `PaddleOcrVisionService`.
- Khong de controller/import service phu thuoc truc tiep vao PaddleOCR.
- Can kiem tra native runtime/model PaddleOCR tren moi truong deploy truoc khi goi la done.

## Giai doan 2: Parsing Algorithm

Parser nam o `ReceiptParserService`.

### Gom bounding box thanh dong

Thuat toan:

1. Sort block theo `pageNumber`, `CenterY`, `X`.
2. Moi block duoc dua vao line co `CenterY` gan nhat.
3. Tolerance hien tai: `max(8, block.Height * 0.65)`.
4. Trong tung line, sort block theo `X`.
5. Text cua line = noi cac block theo thu tu trai sang phai.

Muc tieu la bao toan layout ngang cua hoa don de parser co the tim gia tri nam ben phai keyword.

### Trich xuat tong tien

Khong lay so lon nhat trong toan bo hoa don vi de nham voi "khach dua", "tien thoi", VAT, phi, giam gia.

Thuat toan hien tai:

1. Normalize text: lowercase, bo dau tieng Viet.
2. Bo qua line co keyword nhieu kha nang khong phai tong can ghi:
   - `khach dua`
   - `tien khach`
   - `tien thoi`
   - `thoi lai`
   - `no lai`
   - `vat`
   - `thue`
   - `phi`
   - `chiet khau`
   - `giam gia`
   - `subtotal`
   - `tong so`
3. Uu tien line co keyword:
   - `tong tien phai tra`
   - `thanh toan`
   - `amount due`
   - `grand total`
   - `total amount`
   - `tong cong`
   - `total`
4. Trong line da match, uu tien bounding box nam ben phai keyword (`X` lon hon keyword).
5. Dung regex `[\d,\.]+` de lay so tien.
6. Parse tien bang cach bo dau `,` va `.`.

Ket qua:

- `receipt.totalAmount`
- `receipt.totalRawText`
- `receipt.totalLine`

### Trich xuat ngay giao dich

Parser tim ngay trong cac line OCR bang regex ho tro:

- `dd/MM/yyyy`
- `dd-MM-yyyy`
- `dd.MM.yyyy`
- `yyyy-MM-dd`

Neu nam co 2 chu so thi quy uoc:

- `70..99` -> `1970..1999`
- `00..69` -> `2000..2069`

Uu tien line co keyword `ngay`, `date`, `in luc`, `time`; neu khong co thi lay date candidate hop le dau tien theo thu tu uu tien cua parser.

Ket qua:

- `receipt.transactionDate`
- `receipt.transactionDateRawText`

### Trich xuat muc da tieu/category

MVP hien tai chua co bang merchant dictionary rieng. Parser dang dung 2 nguon:

1. Category dang active trong PostgreSQL (`categories`).
2. Alias hard-code nho trong `ReceiptParserService`, vi du:
   - `familymart`, `circle k`, `highlands`, `palla` -> category ten gan voi `an uong`
   - `winmart`, `coopmart`, `bach hoa xanh` -> category ten gan voi `mua sam`
   - `grab`, `be`, `taxi` -> category ten gan voi `di chuyen`

Thuat toan:

1. Lay nhom line phia tren hoa don, tam tinh `Y <= 35% chieu cao document`.
2. Bo cac line nhieu kha nang la metadata: `hoa don`, `invoice`, `receipt`, `tel`, `hotline`, `ma so thue`, `ngay`, `gio`, `thu ngan`.
3. Chon merchant candidate ngan, nam tren dau hoa don.
4. Tao searchable text tu 8 line dau.
5. Match category truc tiep theo ten category truoc.
6. Neu khong match truc tiep, match merchant alias sang category hint.

Ket qua:

- `receipt.merchantName`
- `receipt.suggestedCategoryId`
- `receipt.suggestedCategoryName`
- `receipt.categoryMatchedBy`

## Ke hoach lam lai day du

### Phase 1 - Hoan thien contract OCR noi bo

- Giu `ocr.IService` la boundary cho Vision OCR.
- Tach implementation hien tai thanh `HttpOcrVisionService` neu can giu service ngoai de fallback.
- Tao implementation moi `PaddleOcrVisionService` dung `Sdcb.PaddleOCR`.
- Config chon provider:

```json
{
  "Ocr": {
    "Provider": "Paddle",
    "Layout": "invoice",
    "TimeoutSeconds": 120
  }
}
```

### Phase 2 - Them merchant dictionary trong PostgreSQL

Nen them bang moi, khong nhhoi vao `categories`:

```sql
merchant_category_mappings
(
    id uuid primary key,
    merchant_keyword text not null,
    normalized_keyword text not null,
    category_id uuid not null references categories(id),
    priority int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null,
    updated_at timestamptz not null
)
```

Ly do:

- Admin co the quan ly mapping thuong hieu -> category.
- Mot category co nhieu merchant alias.
- Khong lam o nhiem bang `categories`.

### Phase 3 - OCR upload thanh import draft

Flow target:

1. User upload hoa don.
2. Backend luu file.
3. Vision OCR tao blocks.
4. Parser tao receipt result.
5. Backend tao `ImportJob` hoac mot OCR review job rieng.
6. Backend tao `ImportTransactionDraft` gom:
   - `TransactionDate`
   - `Amount = totalAmount`
   - `Type = Expense`
   - `RawDescription = merchantName`
   - `EditedNote = merchantName`
   - `EditedCategoryId = suggestedCategoryId`
   - `NormalizedPayloadJson = OCR + parser result`
7. FE hien preview.
8. User confirm moi tao `Transaction`.

Luu y domain:

- FE gui amount duong.
- Expense khi tao transaction thi service transaction chiu trach nhiem tru balance.
- Confirm phai atomic voi DB transaction.

### Phase 4 - Test cases bat buoc

Can co test cho parser khong phu thuoc PaddleOCR:

- Tong tien nam cung dong voi keyword va ben phai keyword.
- Tong tien nam o dong co `Thanh toan`, khong lay `Khach dua`.
- Dong `Tong cong` la subtotal, dong `Thanh toan` la final total.
- OCR co nhieu box cung Y nhung lech nhe van gom chung dong.
- Hoa don FamilyMart/Highlands/Grab goi y dung category neu DB co category phu hop.
- Khong co bounding boxes thi parser tra warning va khong crash.

## Pitfalls

- Khong parse tong tien bang cach lay so lon nhat trong raw text.
- Khong noi raw OCR text thanh mot chuoi roi regex toan cuc.
- Khong expose absolute server path cho FE.
- Khong luu secret/model path hard-code trong code.
- Khong tao transaction that ngay sau OCR; can preview/confirm.
- Khong dua PaddleOCR package vao project ma chua kiem tra native dependency tren moi truong deploy.
