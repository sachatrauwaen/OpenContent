using System;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Modules;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components
{
    public class PortalFileUri : FileUri
    {
        #region Constructors

        public PortalFileUri(int portalid, string pathToFile) : base(pathToFile)
        {
            FileInfo = GetFileInfo(portalid);
        }
        public PortalFileUri(int portalid, string path, string filename) : base(path, filename)
        {
            FileInfo = GetFileInfo(portalid);
        }
        public PortalFileUri(PortalFolderUri portalFolderUri, string filename) : base(portalFolderUri.FolderPath, filename)
        {
            FileInfo = GetFileInfo(portalFolderUri.FolderInfo.PortalID);
        }
        public PortalFileUri(IFileInfo fileInfo) : base(GetFilePath(fileInfo))
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            FileInfo = fileInfo;
        }
        public PortalFileUri(int fileId) : base(GetFilePath(fileId))
        {
            var fileInfo = FileManager.Instance.GetFile(fileId);
            if (fileInfo == null)
                throw new FileNotFoundException($"iFileInfo not found for id [{fileId}]");

            FileInfo = fileInfo;
        }

        private IFileInfo GetFileInfo(int portalid)
        {
            //var portalid = PortalSettings.Current.PortalId;
            var pf = (new PortalController()).GetPortal(portalid).HomeDirectory;
            var pos = FilePath.IndexOf(pf, StringComparison.InvariantCultureIgnoreCase);
            if (pos < 0)
            {
                //maybe we are looking for _default file?
                pf = "portals/_default";
                pos = FilePath.IndexOf(pf, StringComparison.InvariantCultureIgnoreCase);
                if (pos < 0)
                    throw new FileNotFoundException($"iFileInfo not found for path [{FilePath}]. Incorrect Homedirectory.");
                else
                    portalid = -1;
            }

            var fileRequested = FileManager.Instance.GetFile(portalid, FilePath.Substring(pos + pf.Length + 1));
            if (fileRequested == null)
                throw new FileNotFoundException($"iFileInfo not found for path [{FilePath}]");

            return fileRequested;
        }
        private static string GetFilePath(int fileId)
        {
            IFileInfo fileInfo = FileManager.Instance.GetFile(fileId);
            if (fileInfo == null)
                throw new FileNotFoundException($"iFileInfo not found for id [{fileId}]");

            return NormalizePath(fileInfo.ToUrlWithoutLinkClick());
        }
        private static string GetFilePath(IFileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return NormalizePath(fileInfo.ToUrlWithoutLinkClick());
        }
        #endregion

        /// <summary>
        /// Gets or sets the Dnn file information object.
        /// </summary>
        /// <value>
        /// The Dnn file information object.
        /// </value>
        /// <remarks>This is only available for files under the Dnn Portal Directory</remarks>
        protected IFileInfo FileInfo { get; }

        private JObject _fileMetaData;

        private JToken GetMetaData(string fieldname)
        {
            if (_fileMetaData == null)
            {
                if (ModuleDefinitionController.GetModuleDefinitionByFriendlyName("OpenFiles") == null)
                    _fileMetaData = JObject.Parse("{}");
                else if (FileInfo.ContentItemID <= 0)
                    _fileMetaData = JObject.Parse("{}");
                else
                {
                    var item = Util.GetContentController().GetContentItem(FileInfo.ContentItemID);
                    if (item != null && item.Content.IsJson())
                    {
                        _fileMetaData = JObject.Parse(item.Content);
                    }
                    else
                    {
                        _fileMetaData = JObject.Parse("{}");
                    }
                }
            }

            return _fileMetaData == null ? null : _fileMetaData[fieldname];
        }

        public int DnnFileId => FileInfo == null ? 0 : FileInfo.FileId;

        /// <summary>
        /// Get a value from the OpenFiles Metadata attached to a PortalFile.
        /// </summary>
        /// <param name="fieldname">The fieldname.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string MetaData(string fieldname, string defaultValue)
        {
            string retval = defaultValue;
            var value = GetMetaData(fieldname);
            if (value != null)
                retval = value.ToString();
            return retval;
        }

        /// <summary>
        /// Get a value from the OpenFiles Metadata attached to a PortalFile.
        /// </summary>
        /// <param name="fieldname">The fieldname.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public int MetaData(string fieldname, int defaultValue)
        {
            int retval;
            var value = GetMetaData(fieldname);
            if (value == null || !int.TryParse(value.ToString(), out retval))
                retval = defaultValue;
            return retval;
        }

        public string EditUrl(ModuleInfo module)
        {
            if (module == null) return string.Empty;
            var mc = new ModuleInstanceContext { Configuration = module };
            if (!mc.IsEditable) return string.Empty;
            return EditUrl();
        }

        public string EditUrl()
        {
            //var url = Globals.NavigateURL(tabFileManager);
            //var dnnFileManagerModule = DnnUtils.GetDnnModulesByFriendlyName("filemanager", tabFileManager).OrderByDescending(m=> m.ModuleID).FirstOrDefault();
            var dnnFileManagerModule = DnnUtils.GetLastModuleByFriendlyName("Digital Asset Management");
            var modId = dnnFileManagerModule.ModuleID;
            //var modId = 1420; 
            var url = Globals.NavigateURL(dnnFileManagerModule.TabID, "FileProperties", "mid=" + modId, "popUp=true", "fileId=" + FileInfo.FileId);
            return $"javascript:dnnModal.show('{url}',/*showReturn*/false,550,950,true,'')";
            //javascript:dnnModal.show('http://localhost:54068/en-us/OpenFiles/ctl/Module/ModuleId/487/view/gridview/pageSize/10?ReturnURL=/en-us/OpenFiles?folderId=42&popUp=true',/*showReturn*/false,550,950,true,'')
            //return string.Format("javascript:dnnModal.show('{0}/ctl/FileProperties/mid/{2}?popUp=true&fileId={1}')", url, FileInfo.FileId, modId);
        }
    }
}