using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Satrabel.OpenContent.Components.JPList
{
    public class RequestDTO
    {
        public string statuses { get; set; }
        public string options { get; set; }
        
        public List<StatusDTO> StatusLst
        {
            get
            {
                var lst = new List<StatusDTO>();
                if (!String.IsNullOrEmpty(statuses))
                {
                    lst  = JsonConvert.DeserializeObject<List<StatusDTO>>(HttpUtility.UrlDecode(statuses));
                    if (lst != null)
                    {

                    }
                }
                return lst;
            }
        }

    }
}
