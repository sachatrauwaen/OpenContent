using System;

namespace Satrabel.OpenContent.Components.Json
{
    public class InvalidJsonFileException : Exception
    {

        public InvalidJsonFileException(string message, Exception innerException, string filename) : base(message, innerException)
        {
            Filename = filename;
        }
        public string Filename { get; set; }

    }
}