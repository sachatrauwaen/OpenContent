namespace Satrabel.OpenContent.Components.Migration
{
    public class MigrationConfig
    {
        public MigrationConfig(string templateFolder, int portalId, bool overwriteTargetData, bool dryRun)
        {
            TemplateFolder = templateFolder;
            PortalId = portalId;
            OverwriteTargetData = overwriteTargetData;
            DryRun = dryRun;
        }

        public string TemplateFolder { get; }
        public int PortalId { get; }
        public bool OverwriteTargetData { get; }
        public bool DryRun { get; }
    }
}