using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Satrabel.OpenContent.Components.Render
{
    public class WebFormsPageContext
    {
        private Page page;

        public WebFormsPageContext(Page page)
        {
            this.page = page;
        }
    }
}