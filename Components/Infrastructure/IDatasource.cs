using Satrabel.OpenContent.Components.Manifest;

namespace Satrabel.OpenContent.Components.Infrastructure
{
    interface IDatasource
    {
        void GetDataList(TemplateInfo info, OpenContentSettings settings, bool clientSideData);
        void GetDetailData(TemplateInfo info, OpenContentSettings settings);
        void GetData(TemplateInfo info, OpenContentSettings settings);
        bool GetDemoData(TemplateInfo info, OpenContentSettings settings);
    }
}
