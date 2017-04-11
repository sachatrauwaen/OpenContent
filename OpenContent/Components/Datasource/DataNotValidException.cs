using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataNotValidException : Exception
    {
        public DataNotValidException(string message) : base(message)
        {
        }
    }
}