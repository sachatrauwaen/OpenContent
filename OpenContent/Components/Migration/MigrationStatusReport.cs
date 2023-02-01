using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Satrabel.OpenContent.Components.Migration
{
    public class MigrationStatusReport
    {
        private readonly MigrationConfig _migrationConfig;
        private int _migrateToCounter;
        private int _moduleCounter;
        private int _moduleDataCounter;
        private int _alreadyMigratedDataCounter;
        private int _donotOverwrite;
        private readonly Dictionary<string, int> _skipped = new Dictionary<string, int>();
        private int _migrated;
        private int _ignoreExistingFile;
        private readonly List<string> _errors = new List<string>();

        public MigrationStatusReport(MigrationConfig migrationConfig)
        {
            _migrationConfig = migrationConfig;
        }

        public HtmlString Print()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<h2>Migration status Report</h2>");
            if (_errors.Count == 0)
                html.Append("<p>Migration ran without errors.</p>");
            else
            {
                foreach (var error in _errors)
                {
                    html.Append("<ul>");
                    html.Append($"<li><strong>ERROR: {error}</strong></li>");
                    html.Append("</ul>");
                }
            }

            html.Append("<ul>");
            html.Append($"<li>Number of 'MigrateTo' tags found in options.json: <strong>{_migrateToCounter}</strong>.</li>");
            html.Append($"<li>Number of Modules found with template {_migrationConfig.TemplateFolder}: <strong>{_moduleCounter}</strong>.</li>");
            html.Append($"<li>Number of data items found in those modules: <strong>{_moduleDataCounter}</strong>.</li>");
            if (_migrationConfig.DryRun)
            {
                html.Append($"<li><strong>DRY RUN - No modification have been made.</strong>.</li>");
            }
            html.Append($"<li>Number of Data items ready for migration: <strong>{_moduleDataCounter - _alreadyMigratedDataCounter}</strong>.</li>");
            html.Append($"<li>Number of Data items skipped because already migrated: <strong>{_alreadyMigratedDataCounter} items</strong> with '{_migrationConfig.MigrationVersion}' tag.</li>");
            html.Append($"<li>Number of Data items skipped because OverWrite==false: <strong>{_donotOverwrite} items</strong>.</li>");
            if (_ignoreExistingFile > 0)
                html.Append($"<li>Number of Files skipped because already Existed: <strong>{_ignoreExistingFile} items</strong>.</li>");

            foreach (var skipped in _skipped)
            {
                html.Append($"<li>{skipped.Key}: <strong>{skipped.Value} items</strong>.</li>");
            }
            html.Append($"<li>Number of Data items actually migrated: <strong>{_migrated} items</strong>.</li>");
            html.Append("</ul>");

            return new HtmlString(html.ToString());
        }

        public void FoundMigrateTo()
        {
            _migrateToCounter += 1;
        }

        public void FoundModule()
        {
            _moduleCounter += 1;
        }

        public void FoundModuleData()
        {
            _moduleDataCounter += 1;
        }

        public void FoundAlreadyMigratedData()
        {
            _alreadyMigratedDataCounter += 1;
        }

        public void FoundDoNotOverwrite()
        {
            _donotOverwrite += 1;
        }

        public void Skipped(string reason)
        {
            if (_skipped.ContainsKey(reason))
                _skipped[reason] += 1;
            else
                _skipped.Add(reason, 1);
        }

        public void Migrated()
        {
            _migrated += 1;
        }

        public void FoundExistingFile()
        {
            _ignoreExistingFile += 1;

        }

        public void LogError(string errMsg)
        {
            _errors.Add(errMsg);
        }
    }
}