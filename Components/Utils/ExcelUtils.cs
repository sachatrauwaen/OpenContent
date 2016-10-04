using System.Data;
using System.Web;
using OfficeOpenXml;

namespace Satrabel.OpenContent.Components
{
    public static class ExcelUtils
    {
        public static void OutputFile(DataTable dataTable, string filename, HttpContext ctx)
        {
            var excelBytes = ExcelUtils.CreateExcel(dataTable);
            ExcelUtils.OutputFile(excelBytes, filename, ctx);
        }

        private static byte[] CreateExcel(DataTable dataTable)
        {
            using (var pck = new ExcelPackage())  //we gebruiken using om zeker mooi alles te sluiten achteraf.
            {
                var worksheet = pck.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                return pck.GetAsByteArray();
            }
        }

        private static void OutputFile(byte[] excelBytes, string filename, HttpContext ctx)
        {
            if (excelBytes.Length > 0)
            {
                var response = ctx.Response;
                response.Clear();
                response.BufferOutput = false;
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                response.AddHeader("content-disposition", HttpUtils.CreateContentDisposition(filename, ctx.Request));
                response.BinaryWrite(excelBytes);
                response.Flush();
            }
        }
    }
}