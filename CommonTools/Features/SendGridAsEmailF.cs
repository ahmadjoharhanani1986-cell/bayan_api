using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Mapster;
using MediatR;
using SHLAPI.Database;
using SHLAPI.Models;
using ClosedXML.Excel;

namespace SHLAPI.Features
{
    public class SendGridAsEmailF
    {
        public class Command : FeatureBase, IRequest<Result>
        {
            public string xlsxFile { get; set; }

            public int to_user_id { get; set; }

            public string note { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
            }
        }

        public class CommandHandler : FeatureHandlerBase, IRequestHandler<Command, Result>
        {
            public CommandHandler(IShamelDatabase con) : base(con)
            {
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                using (var db = _con.Open())
                {
                    var result = new Result(true);

                    var user = await UserM.Get(db, null, request.to_user_id);

                    byte[] excelBytes = Convert.FromBase64String(request.xlsxFile);

                    // var stream = new MemoryStream(excelBytes);
                    MemoryStream memStream;
                    using (memStream = new MemoryStream())
                    {
                        memStream.Write(excelBytes, 0, excelBytes.Length);

                        // Create a workbook from the loaded Excel data.
                        using (var workbook = new XLWorkbook(memStream))
                        {
                            // Get the worksheet you want to work with (e.g., by index or name).
                            var worksheet = workbook.Worksheet(1); // Adjust the index as needed.

                            // Adjust row heights based on content.
                            worksheet.Rows().AdjustToContents();

                            foreach (var cell in worksheet.CellsUsed())
                            {
                                // Center the text vertically in each cell.
                                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            }

                            int desiredRowHeight = 40;
                            foreach (var row in worksheet.Rows())
                            {
                                if (row.Height < desiredRowHeight)
                                    row.Height = desiredRowHeight;
                            }


                            // Clear the memorymemStream and save the updated workbook back to it.
                            memStream.Seek(0, SeekOrigin.Begin);
                            workbook.SaveAs(memStream);
                        }
                        string fileName = "temp.xlsx";
                        var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                        memStream.WriteTo(fileStream);
                        fileStream.Close();

                        string _note = request.note + "<br/><br/>" + "ملاحظة : مرفق ملف التقرير";
                        List<string> cc = new List<string>();
                        cc.Add("ahmad.dweikat@iscosoft.com");
                        cc.Add("amnah.droobi@iscosoft.com");
                        Notifications.SendNotification(user.email, "تقرير المهام اليومي", _note, fileName,cc);
                        // Notifications.SendNotification("ahmad@iscosoft.com", "تقرير المهام اليومي", _note, fileName,"ahmad.dweikat@iscosoft.com");
                    }

                    return result;
                }
            }
        }
    }
}