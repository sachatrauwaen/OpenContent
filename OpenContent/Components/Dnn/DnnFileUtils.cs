using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.FileSystem;
using Satrabel.OpenContent.Components.TemplateHelpers;

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

        /// <summary>
        /// Returns the Url of the file. In case OpenImageProcessor is installed, Linkclick urls are transformed into ImgClick urls.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <returns></returns>
        internal static string ToLinkClickSafeUrl(this IFileInfo fileInfo)
        {
            if (fileInfo == null) return "";

            var url = FileManager.Instance.GetUrl(fileInfo);
            if (url.ToLower().Contains("linkclick"))
            {
                var hash = url.SubstringBetween("fileticket=", "&portalid");
                url = $"/imgclick/{fileInfo.PortalId}/{hash}.axd";
            }
            return url;
        }

        internal static string RemoveCachebuster(this string url)
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