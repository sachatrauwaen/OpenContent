using System.Web.UI.WebControls;

namespace Satrabel.OpenContent.Components
{
    public class TemplateItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
        public string Image { get; internal set; }
        public string Description { get; internal set; }

        public TemplateItem(string text, string value)
        {
            Text = text;
            Value = value;
        }
    }
}