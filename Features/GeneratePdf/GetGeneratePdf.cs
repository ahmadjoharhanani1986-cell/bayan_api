
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font;
using SHLAPI.Database;
using Dapper;
using System.IO;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.IO.Font.Otf;
using iText.Layout.Splitting;
using System.Collections;
using System.Drawing;
using SHLAPI.Utilities;
using System.Text;
using Image = iText.Layout.Element.Image;
using SHLAPI.Models.UserInfo;



namespace SHLAPI.Features.GeneratePdf
{

    public class GetGeneratePdfF : FeatureBase
    {
        public class ArabicSplitCharacters : ISplitCharacters
        {
            public bool IsSplitCharacter(GlyphLine glyphLine, int glyphIndex)
            {
                return true; // allow splitting for all characters
            }
        }
        public class Command
        {
            public List<ItemData> items { get; set; } = new List<ItemData>(); // data for invoice
            public List<CheckData> checks { get; set; } = new List<CheckData>(); // data for checks
            public List<DataAccountSheetReport> dataRpt { get; set; } = new List<DataAccountSheetReport>(); // data for AccountSheetReport
            public List<ColumnNames> itemColumnCaptions { get; set; } = new List<ColumnNames>();
            public List<DataItemsList> dataItemsList { get; set; } = new List<DataItemsList>();
            public List<RptFilterObj> rptFilterList { get; set; } = new List<RptFilterObj>();
            public List<RptFilterObj> rptFooterList { get; set; } = new List<RptFilterObj>();
            public DateTime voucherDate { get; set; }
            public string rptName { get; set; }
            public string dateCaption { get; set; }
            public string customerNo { get; set; }
            public string customerName { get; set; }
            public float amount { get; set; }
            public float cash { get; set; }
            public float check { get; set; }
            public string customerCaption { get; set; }
            public bool isReport { get; set; } // is this report
            public bool isInvoice { get; set; } // invoice sell or pay
            public bool isQabdSarf { get; set; } // qabd or sarf
            public bool isSlip { get; set; } // slip pdf size
            public float totalBalance { get; set; } // Totalbalance
            public ChecksBalanceObj checksBalanceObj { get; set; }
            public string userName { get; set; }
            public string progName { get; set; }
            public int user_id { get; set; }
            public float debitBalance { get; set; }
            public float creditBalance { get; set; }
            public bool printCreditDebit { get; set; }
        }
        public class RptFilterObj
        {
            public string caption { get; set; }
            public string value { get; set; }
        }
        public class ItemData
        {
            public string code { get; set; }
            public string name { get; set; }
            public string unit { get; set; }
            public double quantity { get; set; }
            public double price { get; set; }
            public double amount { get; set; }
            public double itemDiscount { get; set; }
        }
        public class CheckData
        {
            public string txtAccounts { get; set; }
            public string txtCheckNo { get; set; }
            public string txtBankName { get; set; }
            public string txtBranchName { get; set; }
            public string dtpDate { get; set; }
            public double txtCheckAmount { get; set; }
        }
        public class DataAccountSheetReport
        {
            public string voucherDate { get; set; }
            public string voucherNo { get; set; }
            public string voucherType { get; set; }
            public string debit { get; set; }
            public string credit { get; set; }
            public string balance { get; set; }
        }
        public class DataItemsList
        {
            public string accountName { get; set; }
            public string finalQnty { get; set; }
            public string retailPrice { get; set; }
            public string lastPurchasePrice { get; set; }
            public string name { get; set; }
            public string no { get; set; }
        }
        public class ColumnNames
        {
            public string key { get; set; }
            public string name { get; set; }
        }
        public class ChecksBalanceObj
        {
            public float checkBalance { get; set; }
            public float unpayedChecks { get; set; }
            public float returnedChecks { get; set; }
        }
        public static int margin = 36;
        public static int marginRL = 5;
        private static readonly HttpClient httpClient = new HttpClient();
        IShamelDatabase _con;
        public async Task<byte[]> GeneratePdfAsync(Command request, int userId)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            PageSize pageSize = PageSize.A4;

            float pageWidth = pageSize.GetWidth();
            if (request.isSlip)
            {
                // Width = 80mm, height = arbitrary (e.g., 200mm)
                float width = Utilities.StringUtil.MillimetersToPoints(80);  // 80mm receipt
                float height = Utilities.StringUtil.MillimetersToPoints(200); // initial height, can grow
                pageSize = new PageSize(width, height);
                pageWidth = pageSize.GetWidth();
                pageWidth -= marginRL * 2;
            }
            else marginRL = margin;

            var document = new Document(pdf, pageSize);


            document.SetMargins(margin, margin, marginRL, marginRL);
            document.SetProperty(Property.BASE_DIRECTION, BaseDirection.RIGHT_TO_LEFT);

            // Arabic font
            string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "NotoNaskhArabic-Regular.ttf");
            PdfFont arabicFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            string companyName = "";
            string companyAddress = "";
            string headerValue = ""; string licenseNo = "";
            _con = new ShamelDatabase();
            using (var db = _con.Open())
            {
                companyName = await db.ExecuteScalarAsync<string>("SELECT company_name FROM company_info WHERE data_language_id = 1");
                companyAddress = await db.ExecuteScalarAsync<string>("SELECT company_address FROM company_info WHERE data_language_id = 1");
                var _obj = await UserInfo_M.GetData(db, null, userId);
                if (_obj != null && _obj.ToList().Count > 0)
                {
                    request.userName = _obj.ToList()[0].user_name;
                }
                dynamic showHeader;

                if (request.isInvoice || request.isQabdSarf)
                {
                    showHeader = (await db.QueryAsync<dynamic>(
                        "SELECT TOP(1) show_header, company_license_no FROM Voucher_Settings"
                    )).FirstOrDefault();
                }
                else
                {
                    showHeader = (await db.QueryAsync<dynamic>(
                        "SELECT TOP(1) show_header, company_license_no FROM Report_Settings"
                    )).FirstOrDefault();
                }

                // استخدام القيم
                headerValue = showHeader != null ? Convert.ToString(showHeader.show_header) : "";
                licenseNo = showHeader != null ? Convert.ToString(showHeader.company_license_no) : "";


            }
            request.userName = FixText(request.userName != null ? request.userName : "");
            request.progName = "الشامل لايت للمحاسبة" + "-3.3-" + companyName;
            request.progName = FixText(request.progName);


            // Register page number event
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PageNumberEventHandler(arabicFont, request.userName, request.progName));
            // Add header
            AddHeader(document, arabicFont, companyName, companyAddress, request, headerValue, licenseNo);
            // اذا كان مش قبض او صرف   او قبض وصرف ومجموع الشيكات اكبر من صفر
            if (!request.isQabdSarf || (request.isQabdSarf && request.check > 0))
                // Add items table
                AddItemsTable(document, arabicFont, pageWidth, request);

            // Add footer
            AddFooter(document, arabicFont, request);

            document.Close();
            return memoryStream.ToArray();
        }
        private void AddHeader(Document document, PdfFont font,
                                string companyName, string companyAddress, Command request, string showHeader, string licenseNo)
        {
            var header = new Paragraph(FixText(request.rptName))
                .SetFont(font).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(0);


            var companyInfo = new Paragraph(FixText($"{companyName} - {companyAddress}"))
                .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER);


            var licenseNoControl = new Paragraph(FixText($"{licenseNo}") + " : " + FixText("مشتغل مرخص"))
                .SetFont(font).SetFontSize(8).SetTextAlignment(TextAlignment.RIGHT);

            TextAlignment textAlignment = TextAlignment.RIGHT;
            if (request.isSlip) textAlignment = TextAlignment.CENTER;

            var dateHeader = new Paragraph(FixText($"{request.voucherDate:yyyy/MM/dd}") + " : " + FixText(request.dateCaption))
                .SetFont(font).SetFontSize(12).SetTextAlignment(textAlignment);

            if (request.isReport)
            {
                dateHeader = new Paragraph(FixText(request.dateCaption))
                .SetFont(font).SetFontSize(12).SetTextAlignment(textAlignment);
            }





            string customerData = FixText(request.customerName) + "/" + FixText(request.customerNo) + " : " + FixText(request.customerCaption);
            if (request.isQabdSarf) customerData = FixText(request.customerNo) + " : " + FixText(request.customerCaption);
            var dateCustomer = new Paragraph(customerData)
                .SetFont(font).SetFontSize(12).SetTextAlignment(textAlignment);
            if (licenseNo.Trim() != "")
                document.Add(licenseNoControl);
            document.Add(header);
            if (showHeader == "1" || showHeader.ToLower() == "true") // اظهار الترويسة
                document.Add(companyInfo);
            if (request.customerNo.Trim() != "")
                document.Add(dateCustomer);
            document.Add(dateHeader);


            if (request.rptFilterList != null && request.rptFilterList.Count > 0)
            {
                if (request.isSlip)
                {
                    foreach (RptFilterObj obj in request.rptFilterList)
                    {
                        var filter = new Paragraph(FixText(obj.value) + " : " + FixText(obj.caption))
                            .SetFont(font)
                            .SetFontSize(12)
                            .SetTextAlignment(textAlignment);

                        document.Add(filter);
                    }
                }
                else
                {
                    Table totalsTable = new Table(new float[] { 1, 1 })
                                 .UseAllAvailableWidth();
                    if (request.rptFilterList.Count >= 3)
                    {
                        totalsTable = new Table(new float[] { 1, 1, 1 })
                             .UseAllAvailableWidth();
                    }



                    int index = 0;
                    var reversedList = request.rptFilterList.AsEnumerable().Reverse().ToList();
                    foreach (RptFilterObj obj in reversedList)
                    {
                        TextAlignment align = TextAlignment.RIGHT;

                        Cell cell = new Cell()
                            .Add(new Paragraph(FixText(obj.value) + " : " + FixText(obj.caption))
                                .SetFont(font)
                                .SetFontSize(12)
                                .SetTextAlignment(align))
                            .SetBorder(Border.NO_BORDER);


                        totalsTable.AddCell(cell);

                        index++;
                    }

                    document.Add(totalsTable);
                }
            }


            if (request.isQabdSarf)
            {

                float totalAmount = request.amount, cashAmount = request.cash, checkAmount = request.check;

                Table totalsTable = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetTextAlignment(textAlignment).SetMarginTop(40);
                if (!request.isSlip)
                {
                    // شيكات
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(FixText("الشيكات: " + checkAmount.ToString("F2")))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );
                    // نقدا
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(FixText("نقدا: " + cashAmount.ToString("F2")))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );
                    // المجموع
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(FixText("المجموع: " + totalAmount.ToString("F2")))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );
                }
                else
                {
                    // جدول بعمود واحد فقط
                    totalsTable = new Table(1)
                       .SetWidth(UnitValue.CreatePercentValue(100))
                       .SetTextAlignment(TextAlignment.CENTER);



                    // صف 3 : المجموع
                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(totalAmount.ToString("F2") + " " + FixText("المجموع: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );

                    // صف 2 : نقدا
                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(cashAmount.ToString("F2") + " " + FixText("نقداً: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );
                    // صف 1 : الشيكات
                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(checkAmount.ToString("F2") + " " + FixText("الشيكات: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );



                }
                document.Add(totalsTable);



            }
        }

        private void AddItemsTable(Document document, PdfFont font, float pageWidth, Command request)
        { // الكميةو السعر و المبلغ و الوحدة الاسم الرقم
            float[] baseWidths = { 90, 90, 90, 70, 90, 60 };
            if (request.isInvoice) baseWidths = new float[] { 90, 45, 45, 90, 70, 90, 60 };
            float total = baseWidths.Sum();
            int colCountSummaryFil = 2;
            bool isItemListRpt = request.dataItemsList != null && request.dataItemsList.Count > 0 ? true : false;
            if (request.isSlip) // mobile
            {
                baseWidths = new float[] { 100, 100, 100, 110, 250 };
                total = baseWidths.Sum();
                float scale = pageWidth / total;
                baseWidths = baseWidths.Select(w => w * scale).ToArray();
                total = baseWidths.Sum();
            }

            List<string> headers = new List<string> { "الكمية", "السعر", "المبلغ", "الوحدة", "الاسم", "الرقم" };// يعتم تمرير اسماء الاعمدة من api
            if (request.itemColumnCaptions != null && request.itemColumnCaptions.Count > 0)
            {
                headers = new List<string>();
                foreach (ColumnNames obj in request.itemColumnCaptions)
                {
                    if (!request.isSlip)
                    {
                        headers.Add(obj.name);
                        // headers.Add("العمل على الرصيد الكلي");
                    }
                    else if (obj.key.Trim().ToLower() != "txtaccounts" && request.isSlip && request.isQabdSarf)
                    {
                        headers.Add(obj.name);
                    }
                    else if (obj.key.Trim().ToLower() != "no" && request.isSlip && isItemListRpt)
                    {
                        headers.Add(obj.name);
                    }
                    else if (obj.key.Trim().ToLower() != "voucher_type" && request.isSlip && request.isReport)
                    {
                        headers.Add(obj.name);
                    }
                    else if (obj.key.Trim().ToLower() != "item_no" && request.isSlip && request.isInvoice) // اذا كان العمود رقم الصنف يتم اخفائه
                        headers.Add(obj.name);
                }
            }

            // Find remaining width

            float remaining = pageWidth;
            if (remaining - total - 60 > 0)
            {
                remaining = pageWidth - total - 60;
            }
            else
            {
                remaining = 0;
            }

            // Expand the "الاسم" column (index 4)
            baseWidths[4] += remaining;

            // Create table
            var table = new Table(UnitValue.CreatePointArray(baseWidths))
                .SetWidth(UnitValue.CreatePointValue(baseWidths.Sum()))
                .SetFixedLayout();

            foreach (var h in headers)
            {
                // table.AddHeaderCell(new Cell()
                //     .Add(new Paragraph(FixText(h)).SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER).SetBaseDirection(BaseDirection.LEFT_TO_RIGHT))
                //     .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                //     .SetBorder(new SolidBorder(1))
                //     .SetBaseDirection(BaseDirection.LEFT_TO_RIGHT));
                table.AddHeaderCell(CreateBodyCell(h, TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font));
            }





            double totalAmount = request.items != null && request.items.Count > 0 ?
             request.items.Sum(x => x.amount) : request.checks != null && request.checks.Count > 0 ?
            request.checks.Sum(x => x.txtCheckAmount) : request.totalBalance;

            if (request.checks != null && request.checks.Count > 0)
            {
                foreach (var check in request.checks)
                {
                    AddTableCell(table, check.txtCheckAmount.ToString("F2"), TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, check.dtpDate, TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, check.txtBranchName, TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, check.txtBankName ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, check.txtCheckNo ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    if (!request.isSlip)
                        AddTableCell(table, check.txtAccounts ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                }
            }
            else if (request.items != null && request.items.Count > 0)
            {
                foreach (var item in request.items)
                {
                    AddTableCell(table, item.amount.ToString("F2"), TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    if (!request.isSlip)
                    {
                        AddTableCell(table, item.itemDiscount.ToString("F2"), TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    }
                    AddTableCell(table, item.price.ToString("F2"), TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.quantity.ToString("F2"), TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.unit ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.name ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    if (!request.isSlip)
                    {
                        AddTableCell(table, item.code ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    }
                }
            }
            else if (request.dataRpt != null && request.dataRpt.Count > 0)
            {
                foreach (var item in request.dataRpt)
                {
                    AddTableCell(table, item.balance ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.debit ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.credit ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    if (!request.isSlip)
                        AddTableCell(table, item.voucherType ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.voucherNo ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.voucherDate ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                }
            }
            else if (request.dataItemsList != null && request.dataItemsList.Count > 0)
            {
                foreach (var item in request.dataItemsList)
                {
                    AddTableCell(table, item.accountName ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.finalQnty ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.retailPrice ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);

                    AddTableCell(table, item.lastPurchasePrice ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    AddTableCell(table, item.name ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                    if (!request.isSlip)
                        AddTableCell(table, item.no ?? "", TextAlignment.CENTER, baseWidths, 0, baseWidths.Sum(), font);
                }
            }





            if (!isItemListRpt) //عدم اضافة المجموع
            {

                // عدد الأعمدة
                int colCount = headers.Count;

                if (request.isSlip) colCountSummaryFil = colCount;
                totalAmount = Math.Round(totalAmount, 2);
                // خلية المجموع في آخر عمود
                table.AddFooterCell(
                    new Cell(1, colCountSummaryFil)
                        .Add(new Paragraph(FixText("المجموع: " + totalAmount.ToString("F2")))
                            .SetFont(font)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.LEFT))
                        .SetPaddingLeft(15)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetBorder(new SolidBorder(1))
                );

                if (!request.isSlip)
                {
                    // إنشاء صف المجموع
                    table.AddFooterCell(
                                        new Cell(1, colCount - colCountSummaryFil)
                                                .SetBorder(new SolidBorder(1)));
                }
                
            }
            document.Add(table);
        }

        private void AddTableCellOld(Table table, string content, PdfFont font)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(FixText(content)).SetFont(font).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER))
                .SetBorder(new SolidBorder(1)).SetPadding(5));
        }
        private Cell AddTableCell(Table table, string text, TextAlignment alignment, float[] columnWidths, int columnIndex, float tableTotalWidth,
                                   PdfFont arabicFont)
        {
            // splitArabicIntoLines = your method for wrapping Arabic text
            List<string> rtlLines = SplitArabicIntoLines(text, arabicFont, 9, columnWidths, columnIndex, tableTotalWidth);

            Cell cell = new Cell()
                .SetFont(arabicFont)
                .SetFontSize(10)
                .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));

            foreach (string line in rtlLines)
            {
                string reshaped = ArabicTextProcessor.FixText(line); // your shaping method
                Paragraph p = new Paragraph(reshaped)
                    .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                    .SetMultipliedLeading(1.0f)
                    .SetTextAlignment(alignment);

                cell.Add(p);
            }
            table.AddCell(cell);
            return cell;
        }

        private void AddImageCell(Table table, string imageUrl, PdfFont font)
        {
            var cell = new Cell().SetBorder(new SolidBorder(1)).SetPadding(5).SetTextAlignment(TextAlignment.CENTER);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    byte[] imageBytes = DownloadImageFromUrl(imageUrl);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        var imageData = ImageDataFactory.Create(imageBytes);
                        var image = new Image(imageData).SetWidth(60).SetHeight(60).SetAutoScale(false);
                        cell.Add(image);
                    }
                    else
                        cell.Add(new Paragraph(FixText("لا توجد صورة")).SetFont(font).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER));
                }
                catch
                {
                    cell.Add(new Paragraph("").SetFont(font).SetFontSize(9));
                }
            }
            table.AddCell(cell);
        }

        private byte[] DownloadImageFromUrl(string url)
        {
            try
            {
                url = EncodeUrl(url);
                using var response = httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                    return response.Content.ReadAsByteArrayAsync().Result;
            }
            catch { }
            return null;
        }

        private string EncodeUrl(string url) => url.Replace('\\', '/').Replace(" ", "%20").Replace("%2F", "/");

        private void AddFooter(Document document, PdfFont font, Command request)
        {
            if (request.checksBalanceObj != null)
            {
                ChecksBalanceObj checksBalanceObj = request.checksBalanceObj;

                float totalAmount = request.amount, cashAmount = request.cash, checkAmount = request.check;
                TextAlignment textAlignment = TextAlignment.RIGHT;
                if (request.isSlip) textAlignment = TextAlignment.CENTER;
                Table totalsTable = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetTextAlignment(textAlignment);
                if (!request.isSlip)
                {

                    totalsTable.AddCell(
                       new Cell()
                           .Add(new Paragraph(FixText("الرصيد: " + checksBalanceObj.checkBalance.ToString("F2")))
                           .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                           .SetBorder(Border.NO_BORDER)
                   );

                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(FixText("شيكات راجعة: " + checksBalanceObj.returnedChecks.ToString("F2")))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );

                    totalsTable.AddCell(
                       new Cell()
                           .Add(new Paragraph(FixText("شيكات غير مصروفة: " + checksBalanceObj.unpayedChecks.ToString("F2")))
                           .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                           .SetBorder(Border.NO_BORDER)
                   );

                }
                else
                {
                    // جدول بعمود واحد فقط
                    totalsTable = new Table(1)
                       .SetWidth(UnitValue.CreatePercentValue(100))
                       .SetTextAlignment(TextAlignment.CENTER);




                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(checksBalanceObj.unpayedChecks.ToString("F2") + " " + FixText("شيكات غير مصروفة: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );


                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(checksBalanceObj.returnedChecks.ToString("F2") + " " + FixText("شيكات راجعة: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );

                    //////////////////////
                    totalsTable.AddCell(
                        new Cell()
                            .Add(new Paragraph(checksBalanceObj.checkBalance.ToString("F2") + " " + FixText("الرصيد: "))
                            .SetFont(font).SetFontSize(12).SetTextAlignment(TextAlignment.CENTER))
                            .SetBorder(Border.NO_BORDER)
                    );



                }
                document.Add(totalsTable);


            }
            TextAlignment textAlignmentNew = TextAlignment.RIGHT;
            if (request.isSlip) textAlignmentNew = TextAlignment.CENTER;
            if (request.rptFooterList != null && request.rptFooterList.Count > 0) // فلاتر اضافية لتقرير
            {
                foreach (RptFilterObj obj in request.rptFooterList)
                {
                    var filter = new Paragraph(FixText(obj.value) + " : " + FixText(obj.caption))
                    .SetFont(font).SetFontSize(12).SetTextAlignment(textAlignmentNew);
                    document.Add(filter);
                }
            }


            string dateAndTimeToPrint = "";
            if (request.isSlip)
            {
                dateAndTimeToPrint += FixText(" تاريخ ووقت الطباعة: ");
                dateAndTimeToPrint += " \n ";
                dateAndTimeToPrint += $"{DateTime.Now:yyyy/MM/dd HH:mm}";
            }
            else
            {
                dateAndTimeToPrint += $"{DateTime.Now:yyyy/MM/dd HH:mm}" + " ";
                dateAndTimeToPrint += FixText(" تاريخ ووقت الطباعة: ");
            }

            var footer = new Paragraph(dateAndTimeToPrint)
                .SetFont(font).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(20);

            document.Add(footer);

            if (request.isInvoice || request.isQabdSarf)
            {
                Table totalsTable = new Table(new float[] { 1, 1 })
                      .UseAllAvailableWidth().SetMarginTop(50);

                Cell cell = new Cell()
                    .Add(new Paragraph("...................." + " : " + FixText("التوقيع"))
                    .SetFont(font)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER))
                .SetBorder(Border.NO_BORDER);
                totalsTable.AddCell(cell);

                cell = new Cell()
                              .Add(new Paragraph("...................." + " : " + FixText("المصادقة"))
                                  .SetFont(font)
                                  .SetFontSize(12)
                                  .SetTextAlignment(TextAlignment.CENTER))
                              .SetBorder(Border.NO_BORDER);
                totalsTable.AddCell(cell);
                document.Add(totalsTable);
            }
        }

        private string FixText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return Utilities.ArabicTextProcessor.FixText(text);
        }

        // Event handler for page numbers
        public class PageNumberEventHandler : IEventHandler
        {
            private readonly PdfFont font;
            private readonly float fontSize; // حجم الخط
            private readonly string userName;
            private readonly string progName;
            public PageNumberEventHandler(PdfFont font, string userName, string progName, float fontSize = 8)
            {
                this.font = font;
                this.fontSize = fontSize;
                this.userName = userName;
                this.progName = progName;
            }

            public void HandleEvent(Event @event)
            {
                var docEvent = (PdfDocumentEvent)@event;
                var pdf = docEvent.GetDocument();
                var page = docEvent.GetPage();
                int pageNumber = pdf.GetPageNumber(page);
                var pageSize = page.GetPageSize();

                PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
                Canvas canvas = new Canvas(pdfCanvas, pageSize);
                try
                {
                    // رقم الصفحة
                    canvas.ShowTextAligned(
                        new Paragraph(pageNumber + "")
                            .SetFont(font)
                            .SetFontSize(fontSize),
                        pageSize.GetWidth() - 40, // X
                        20,                        // Y
                        TextAlignment.CENTER
                    );


                    // // البرنامج المحاسبي
                    canvas.ShowTextAligned(
                        new Paragraph(this.progName)
                            .SetFont(font)
                            .SetFontSize(fontSize),
                        pageSize.GetWidth() / 2, // X
                        20, // Y
                        TextAlignment.CENTER
                    );

                    // اسم المستخدم
                    canvas.ShowTextAligned(
                                new Paragraph(this.userName != null ? this.userName : "admin1")
                                    .SetFont(font)
                                    .SetFontSize(fontSize),
                                30, // X
                                20, // Y
                                TextAlignment.CENTER
                            );


                    canvas.Close();
                }
                catch
                {

                }


            }
        }

        private Cell CreateBodyCell(string text, TextAlignment alignment, PdfFont arabicFont)
        {
            string reshapedText = ArabicTextProcessor.FixText(text); // your Arabic shaping
            return new Cell()
                .Add(new Paragraph(reshapedText)
                    .SetMultipliedLeading(1.0f)
                    .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)) // Arabic direction
                .SetFont(arabicFont)
                .SetFontSize(10)
                .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                .SetTextAlignment(alignment)
                .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));
        }
        private Cell CreateBodyCell(string text, TextAlignment alignment, float[] columnWidths, int columnIndex, float tableTotalWidth, PdfFont arabicFont)
        {
            // splitArabicIntoLines = your method for wrapping Arabic text
            List<string> rtlLines = SplitArabicIntoLines(text, arabicFont, 9, columnWidths, columnIndex, tableTotalWidth);

            Cell cell = new Cell()
                .SetFont(arabicFont)
                .SetFontSize(10)
                .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 0.5f));

            foreach (string line in rtlLines)
            {
                string reshaped = ArabicTextProcessor.FixText(line); // your shaping method
                Paragraph p = new Paragraph(reshaped)
                    .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT)
                    .SetMultipliedLeading(1.0f)
                    .SetTextAlignment(alignment);

                cell.Add(p);
            }

            return cell;
        }
        public static List<string> SplitArabicIntoLines(
            string text,
            PdfFont font,
            float fontSize,
            float[] columnWidths,
            int columnIndex,
            float tableTotalWidth)
        {
            List<string> lines = new List<string>();

            // Calculate relative column width in points
            float colRelativeWidth = columnWidths[columnIndex];
            float sumWidths = 0f;
            foreach (float w in columnWidths)
            {
                sumWidths += w;
            }

            float colWidth = (colRelativeWidth / sumWidths) * tableTotalWidth;

            // Split text into words
            string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder line = new StringBuilder();
            float lineWidth = 0f;

            foreach (string word in words)
            {
                // Measure word width in points
                float wordWidth = font.GetWidth(word + " ", fontSize);

                if (lineWidth + wordWidth > colWidth)
                {
                    // Add current line to list
                    lines.Add(line.ToString().Trim());
                    // Reset
                    line.Clear();
                    lineWidth = 0f;
                }

                line.Append(word + " ");
                lineWidth += wordWidth;
            }

            // Add last line
            if (line.Length > 0)
            {
                lines.Add(line.ToString().Trim());
            }

            return lines;
        }
        //  public static List<string> splitArabicInToLine(string text,PdfFont font,float fontSize,float[] columnWidths,int columnIndex,float tableTotalWidth)
        //  {
        //      List<string> lines = new List<string>();
        //     float colRelativeWidth = columnWidths[columnIndex];

        //  }
    }
}
