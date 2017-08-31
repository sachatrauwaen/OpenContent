using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using OfficeOpenXml;

namespace Satrabel.OpenContent.Components
{
    public static class ExcelUtils
    {
        public static byte[] CreateExcel(string csv)
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

        public static byte[] CreateExcel(DataTable dataTable)
        {
            using (var pck = new ExcelPackage())  //we gebruiken using om zeker mooi alles te sluiten achteraf.
            {
                var worksheet = pck.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                return pck.GetAsByteArray();
            }
        }

        public static void PushDataAsExcelOntoHttpResponse(byte[] excelBytes, string filename, HttpContext ctx)
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

        public static HttpResponseMessage CreateExcelResponseMessage(string filename, byte[] filebytes)
        {
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            //Create a file on the fly and get file data as a byte array and send back to client
            responseMessage.Content = new ByteArrayContent(filebytes);//Use your byte array
            responseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            responseMessage.Content.Headers.ContentDisposition.FileName = filename; //your file Name- text.xlsx
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            responseMessage.Content.Headers.ContentLength = filebytes.Length;
            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;
            return responseMessage;
        }

        [Obsolete("This method is obsolete since aug 2017; use PushDataAsExcelOntoHttpResponse() instead")]
        public static void OutputFile(DataTable datatable, string filename, HttpContext currentContext)
        {
            var excelBytes = ExcelUtils.CreateExcel(datatable);
            ExcelUtils.PushDataAsExcelOntoHttpResponse(excelBytes, filename, currentContext);
        }
    }
}