using Satrabel.OpenContent.Components.Manifest;

namespace Satrabel.OpenContent.Components.Infrastructure
{
    interface IDatasource
    {
        void GetDataList(RenderInfo info, OpenContentSettings settings, bool clientSideData);
        void GetDetailData(RenderInfo info, OpenContentSettings settings);
        void GetData(RenderInfo info, OpenContentSettings settings);
        bool GetDemoData(RenderInfo info, OpenContentSettings settings);
    }
}
