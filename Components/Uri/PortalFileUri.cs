using System;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.FileSystem;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components.Uri
{
    public class PortalFileUri : FileUri
    {
        #region Constructors

        public PortalFileUri(string pathToFile)
            : base(System.IO.Path.GetDirectoryName(pathToFile))
        {

        }

        public PortalFileUri(string path, string filename)
            : base(path, filename)
        {

        }
        public PortalFileUri(FolderUri path, string filename)
            : base(path.Path, filename)
        {

        }

        protected PortalFileUri(int fileId)
            : base(GetFilePath(fileId))
        {
            var fileInfo = FileManager.Instance.GetFile(fileId);
            if (fileInfo == null)
                throw new ArgumentNullException(string.Format("iFileInfo not found for id [{0}]", fileId));

            FileInfo = fileInfo;
        }

        private static string GetFilePath(int fileId)
        {
            var fileInfo = FileManager.Instance.GetFile(fileId);
            if (fileInfo == null)
                throw new ArgumentNullException(string.Format("iFileInfo not found for id [{0}]", fileId));
            var filePath = FileManager.Instance.GetUrl(fileInfo);
            return NormalizePath(filePath);
        }
        #endregion

        /// <summary>
        /// Gets or sets the Dnn file information object.
        /// </summary>
        /// <value>
        /// The Dnn file information object.
        /// </value>
        /// <remarks>This is only available for files under the Dnn Portal Directory</remarks>
        protected IFileInfo FileInfo { get; set; }

        public dynamic MetaData
        {
            get
            {
                dynamic retval = new object();
                if (ModuleDefinitionController.GetModuleDefinitionByFriendlyName("OpenDocument") == null) return retval;
                if (FileInfo.ContentItemID <= 0) return retval;

                var item = Util.GetContentController().GetContentItem(FileInfo.ContentItemID);
                if (item != null && item.Content.IsJson())
                {
                    retval = JsonUtils.JsonToDynamic(item.Content);
                }
                return retval;
            }
        }

    }
}