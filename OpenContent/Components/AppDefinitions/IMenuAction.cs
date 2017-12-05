namespace Satrabel.OpenContent.Components.AppDefinitions
{

    public interface IMenuAction
    {
        string Title { get; }
        ActionType ActionType { get; }
        string Image { get; }
        string Url { get; }
    }

    public enum ActionType
    {
        Add = 1,
        Edit = 2,
        Misc = 9
    }
}