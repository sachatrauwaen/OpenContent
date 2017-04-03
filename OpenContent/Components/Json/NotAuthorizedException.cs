using System.Web;

namespace Satrabel.OpenContent.Components.Json
{
    public class NotAuthorizedException : HttpException
    {
        public NotAuthorizedException(int httpCode, string message) : base(httpCode, message)
        {
        }
    }
}