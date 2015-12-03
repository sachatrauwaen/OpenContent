using System;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.FileSystem;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Json;

namespace Satrabel.OpenContent.Components
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
            : base(path.FolderPath, filename)
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
    }
}