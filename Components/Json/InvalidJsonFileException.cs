using Satrabel.OpenContent.Components.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Json
{
    public class InvalidJsonFileException : OpenContentException
    {
       
        public InvalidJsonFileException(string message, Exception innerException, string filename)
            : base(message, innerException)
        {
            Filename = filename;
        }
        public string Filename { get; set; }

    }
}