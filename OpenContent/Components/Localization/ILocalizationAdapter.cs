namespace Satrabel.OpenContent.Components.Localization
{
    public interface ILocalizationAdapter
    {
        string GetString(string value);
        string GetString(string value, string resourceFile);
    }
}