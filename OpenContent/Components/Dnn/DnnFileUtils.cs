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

        internal static string RemoveCachbuster(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                int idx = url.IndexOf("?ver=");
                if (idx > 0)
                {
                    return url.Substring(0, idx);
                }
            }
            return url;
        }
    }
}