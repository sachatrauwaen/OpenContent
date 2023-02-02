namespace Satrabel.OpenContent.Components.Migration
{
    public class MigrationConfig
    {
        public MigrationConfig(string templateFolder, int portalId, bool overwriteTargetData, string migrationVersion, bool dryRun)
        {
            TemplateFolder = templateFolder;
            PortalId = portalId;
            OverwriteTargetData = overwriteTargetData;
            MigrationVersion = migrationVersion;
            DryRun = dryRun;
        }

        public string TemplateFolder { get; }
        public int PortalId { get; }
        public bool OverwriteTargetData { get; }
        public string MigrationVersion { get; }
        public bool DryRun { get; }
    }
}