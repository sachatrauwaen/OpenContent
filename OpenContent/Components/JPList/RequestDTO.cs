using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;

namespace Satrabel.OpenContent.Components.JPList
{
    public class RequestDTO
    {
        public string statuses { get; set; }
        public string options { get; set; }
        public bool onlyItems { get; set; }

        public List<StatusDTO> StatusLst
        {
            get
            {
                var lst = new List<StatusDTO>();
                if (!string.IsNullOrEmpty(statuses))
                {
                    lst = JsonConvert.DeserializeObject<List<StatusDTO>>(HttpUtility.UrlDecode(statuses));
                    if (lst != null)
                    {

                    }
                }
                return lst;
            }
        }

    }
}
