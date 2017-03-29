using DotNetNuke.Services.FileSystem;

namespace Satrabel.OpenContent.Components.Dnn
{
    public static class DnnFileUtils
    {
        internal static string ToUrl(this IFileInfo fileInfo)
        {
            if (fileInfo == null) return "";
            var url = FileManager.Instance.GetUrl(fileInfo);
            return url;
        }

        internal static string ToUrlWithoutLinkClick(this IFileInfo fileInfo)
        {
            if (fileInfo == null) return "";

            var url = FileManager.Instance.GetUrl(fileInfo);
            if (url.ToLower().Contains("linkclick"))
            {
                //this method works also for linkclick
                url = "/" + fileInfo.PhysicalPath.Replace(new FolderUri("/").PhysicalFullDirectory, "").Replace("\\", "/");
            }
            return url;
        }
    }
}