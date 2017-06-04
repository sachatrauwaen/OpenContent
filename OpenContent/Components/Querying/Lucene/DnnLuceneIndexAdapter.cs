using DotNetNuke.Entities.Portals;
using Satrabel.OpenContent.Components.Indexing;

namespace Satrabel.OpenContent.Components.Lucene
{
    public class DnnLuceneIndexAdapter : BaseLuceneIndexAdapter
    {
        public DnnLuceneIndexAdapter(string luceneIndexFolder) : base(luceneIndexFolder)
        {
        }

        protected override void GetIndexData(BaseLuceneIndexAdapter lc)
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var modules = DnnUtils.GetDnnOpenContentModules(portal.PortalID);
                foreach (var module in modules)
                {
                    IndexModule(lc, module);
                }
            }
        }

        private static void IndexModule(BaseLuceneIndexAdapter lc, OpenContentModuleConfig module)
        {
            OpenContentUtils.CheckOpenContentSettings(module);

            if (module.IsListMode() && !module.Settings.IsOtherModule && module.Settings.Manifest.Index)
            {
                IndexModuleData(lc, module.ViewModule.ModuleId, module.Settings);
            }
        }

        private static void IndexModuleData(BaseLuceneIndexAdapter lc, int moduleId, OpenContentSettings settings)
        {
            bool index = false;
            if (settings.TemplateAvailable)
            {
                index = settings.Manifest.Index;
            }
            FieldConfig indexConfig = null;
            if (index)
            {
                indexConfig = OpenContentUtils.GetIndexConfig(settings.Template);
            }

            if (settings.IsOtherModule)
            {
                moduleId = settings.ModuleId;
            }
            OpenContentController occ = new OpenContentController();
            lc.DeleteAllOfType(OpenContentInfo.GetScope(moduleId, settings.Template.Collection));
            foreach (OpenContentInfo item in occ.GetContents(moduleId, settings.Template.Collection))
            {
                lc.Add(item, indexConfig);
            }
        }

    }
}