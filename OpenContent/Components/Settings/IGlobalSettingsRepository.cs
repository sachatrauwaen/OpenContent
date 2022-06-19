using Satrabel.OpenContent.Components.Alpaca;

namespace Satrabel.OpenContent.Components.Dnn
{
    public interface IGlobalSettingsRepository
    {
        int GetMaxVersions();
        void SetMaxVersions(int maxVersions);

        AlpacaLayoutEnum GetEditLayout();
        void SetEditLayout(AlpacaLayoutEnum editLayout);

        bool GetLoadBootstrap();
        void SetLoadBootstrap(bool @checked);

        void SetLoadGlyphicons(bool @checked);
        bool GetLoadGlyphicons();

        string GetGoogleApiKey();
        void SetGoogleApiKey(string text);

        bool GetLegacyHandlebars();
        void SetLegacyHandlebars(bool @checked);

        bool GetCompositeCss();
        void SetCompositeCss(bool @checked);
        

        bool GetAutoAttach();
        void SetAutoAttach(string value);

        string GetLoggingScope();
        void SetLoggingScope(string value);

        string GetEditorRoleId();
        void SetEditorRoleId(string value);

        bool IsSaveXml();
        void SetSaveXml(bool saveXml);
        
        string GetGithubRepository();
        void SetGithubRepository(string value);

        bool IsBuilderV2();
        void SetBuilderV2(bool builderV2);
        string GetRestApiKey();
        void SetRestApiKey(string text);
    }
}