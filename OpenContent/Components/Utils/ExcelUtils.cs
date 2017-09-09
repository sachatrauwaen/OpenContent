using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public static byte[] OutputFile(string csv)
        {
            using (var pck = new ExcelPackage())  //we gebruiken using om zeker mooi alles te sluiten achteraf.
            {
                var worksheet = pck.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromText(csv, new ExcelTextFormat()
                {
                    Delimiter = ';',
                    TextQualifier = '"',
                    //EOL = "|"
                    
                });
                return pck.GetAsByteArray();
            }
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
                response.End();
            }
        }

        public static HttpResponseMessage CreateExcelResponseMessage(string fileName, byte[] fileBytes)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            //Create a file on the fly and get file data as a byte array and send back to client
            response.Content = new ByteArrayContent(fileBytes);//Use your byte array
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;//your file Name- text.xlsx
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Content.Headers.ContentLength = fileBytes.Length;
            response.StatusCode = System.Net.HttpStatusCode.OK;
            return response;
        }
    }
}