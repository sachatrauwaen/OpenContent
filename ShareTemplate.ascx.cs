#region Copyright

// 
// Copyright (c) 2015
// by Satrabel
// 

#endregion

#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Common;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Entities.Host;
using DotNetNuke.Common.Utilities;
using ICSharpCode.SharpZipLib.Zip;
using System.Web;
using Satrabel.OpenContent.Components.Rss;
using Satrabel.OpenContent.Components.Manifest;

#endregion

namespace Satrabel.OpenContent
{
    public partial class ShareTemplate : PortalModuleBase
    {
        protected virtual string GetModuleSubDir()
        {
            string dir = Path.GetDirectoryName(ModuleContext.Configuration.ModuleControl.ControlSrc);
            dir = dir.Substring(dir.IndexOf("DesktopModules") + 15);
            return dir;
        }
        public string ModuleTemplateDirectory
        {
            get
            {
                return PortalSettings.HomeDirectory + GetModuleSubDir() + "/Templates/" + ModuleId.ToString() + "/";
            }
        }
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
            }
        }
        protected void cmdSave_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }
        #endregion
        protected void rblAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            phImport.Visible = false;
            phExport.Visible = false;
            phImportWeb.Visible = false;
            phCopy.Visible = false;
            if (rblAction.SelectedIndex == 0) return;

            if (rblAction.SelectedValue == "importfile") // import
            {
                phImport.Visible = true;
            }
            else if (rblAction.SelectedValue == "exportfile") // export
            {
                phExport.Visible = true;
                ddlTemplates.Items.Clear();
                ddlTemplates.Items.AddRange(OpenContentUtils.GetTemplates(PortalSettings, ModuleId, (TemplateManifest)null, GetModuleSubDir()).ToArray());
            }
            else if (rblAction.SelectedValue == "importweb") // Import from web
            {
                phImportWeb.Visible = true;
                ddlWebTemplates.Items.Clear();
                FeedParser parser = new FeedParser();
                var items = parser.Parse("http://www.openextensions.net/templates?agentType=rss&PropertyTypeID=9", FeedType.RSS);
                foreach (var item in items)
                {
                    ddlWebTemplates.Items.Add(new ListItem(item.Title, item.ZipEnclosure));
                }
            }
            else if (rblAction.SelectedValue == "copy") // copy
            {
                phCopy.Visible = true;
                ddlCopyTemplate.Items.Clear();
                ddlCopyTemplate.Items.AddRange(OpenContentUtils.GetTemplates(PortalSettings, ModuleId, (TemplateManifest)null, GetModuleSubDir()).ToArray());
            }
        }
        protected void cmdImport_Click(object sender, EventArgs e)
        {
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(PortalId, GetModuleSubDir() + "/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(PortalId, GetModuleSubDir() + "/Templates");
                }
                var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
                if (Path.GetExtension(fuFile.FileName) == ".zip")
                {
                    string TemplateName = Path.GetFileNameWithoutExtension(fuFile.FileName);
                    if (!string.IsNullOrEmpty(tbImportName.Text))
                        TemplateName = tbImportName.Text;

                    string FolderName = GetModuleSubDir() + "/Templates/" + TemplateName;
                    folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                    int idx = 1;
                    while (folder != null)
                    {
                        FolderName = GetModuleSubDir() + "/Templates/" + TemplateName + idx;
                        folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                        idx++;
                    }
                    if (folder == null)
                    {
                        folder = FolderManager.Instance.AddFolder(PortalId, FolderName);
                    }
                    FileSystemUtils.UnzipResources(new ZipInputStream(fuFile.FileContent), folder.PhysicalPath);
                }
            }
            catch (PermissionsNotMetException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), GetModuleSubDir() + "/Templates");
            }
            catch (NoSpaceAvailableException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fuFile.FileName);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fuFile.FileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception)
            {
                //Logger.Error(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("SaveFileError"), fuFile.FileName);
            }
            if (string.IsNullOrEmpty(strMessage))
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Import Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, strMessage, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }
        protected void cmdExport_Click(object sender, EventArgs e)
        {
            var folder = FolderManager.Instance.GetFolder(PortalId, GetModuleSubDir() + "/Templates");
            if (folder == null)
            {
                folder = FolderManager.Instance.AddFolder(PortalId, GetModuleSubDir() + "/Templates");
            }
            var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
            //var file = fileManager.AddFile(folder, fuFile.FileName, fuFile.FileContent, true, fuFile.PostedFile.co);
            //var file = fileManager.AddFile(folder, fuFile.FileName, fuFile.PostedFile.InputStream, true, false, fuFile.PostedFile.ContentType);
            string zipfilename = Server.MapPath(ddlTemplates.SelectedValue) + ".zip";
            CreateZipFile(zipfilename, Server.MapPath(ddlTemplates.SelectedValue));
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Export Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }
        protected void cmdImportWeb_Click(object sender, EventArgs e)
        {
            string FileName = ddlWebTemplates.SelectedValue;
            string strMessage = OpenContentUtils.ImportFromWeb(PortalId, FileName, "");
            /*
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(PortalId, GetModuleSubDir() + "/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(PortalId, GetModuleSubDir() + "/Templates");
                }
                var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
                if (Path.GetExtension(FileName) == ".zip")
                {
                    string TemplateName = Path.GetFileNameWithoutExtension(FileName);
                    string FolderName = GetModuleSubDir() + "/Templates/" + TemplateName;
                    folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                    int idx = 1;
                    while (folder != null)
                    {
                        FolderName = GetModuleSubDir() + "/Templates/" + TemplateName + idx;
                        folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                        idx++;
                    }
                    if (folder == null)
                    {
                        folder = FolderManager.Instance.AddFolder(PortalId, FolderName);
                    }
                    var req = (HttpWebRequest)WebRequest.Create(FileName);
                    Stream stream = req.GetResponse().GetResponseStream();
                    FileSystemUtils.UnzipResources(new ZipInputStream(stream), folder.PhysicalPath);
                }
            }
            catch (PermissionsNotMetException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), GetModuleSubDir() + "/Templates");
            }
            catch (NoSpaceAvailableException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fuFile.FileName);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fuFile.FileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception)
            {
                //Logger.Error(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("SaveFileError"), fuFile.FileName);
            }
             */
            if (string.IsNullOrEmpty(strMessage))
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Import Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
            else
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, strMessage, DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
        }
        private void CreateZipFile(string zipFileName, string Folder)
        {
            int CompressionLevel = 9;
            var zipFile = new System.IO.FileInfo(zipFileName);
            FileStream strmZipFile = null;
            //Log.StartJob(Util.WRITER_CreatingPackage);
            try
            {
                //Log.AddInfo(string.Format(Util.WRITER_CreateArchive, ZipFileShortName));
                strmZipFile = File.Create(zipFileName);
                ZipOutputStream strmZipStream = null;
                try
                {
                    strmZipStream = new ZipOutputStream(strmZipFile);
                    strmZipStream.SetLevel(CompressionLevel);
                    foreach (var item in Directory.GetFiles(Folder))
                    {
                        FileSystemUtils.AddToZip(ref strmZipStream, Path.GetFullPath(item), Path.GetFileName(item), "");
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    //Log.AddFailure(string.Format(Util.WRITER_SaveFileError, ex));
                }
                finally
                {
                    if (strmZipStream != null)
                    {
                        strmZipStream.Finish();
                        strmZipStream.Close();
                    }
                }
                //Log.EndJob(Util.WRITER_CreatedPackage);
                WriteFileToHttpContext(zipFileName, ContentDisposition.Attachment);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                //Log.AddFailure(string.Format(Util.WRITER_SaveFileError, ex));
            }
            finally
            {
                if (strmZipFile != null)
                {
                    strmZipFile.Close();
                }
            }
        }
        private void WriteFileToHttpContext(string FileName, ContentDisposition contentDisposition)
        {
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;
            var objResponse = HttpContext.Current.Response;
            objResponse.ClearContent();
            objResponse.ClearHeaders();
            switch (contentDisposition)
            {
                case ContentDisposition.Attachment:
                    objResponse.AppendHeader("content-disposition", "attachment; filename=\"" + Path.GetFileName(FileName) + "\"");
                    break;
                case ContentDisposition.Inline:
                    objResponse.AppendHeader("content-disposition", "inline; filename=\"" + Path.GetFileName(FileName) + "\"");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("contentDisposition");
            }
            //objResponse.AppendHeader("Content-Length", File.get.ToString());
            objResponse.ContentType = FileManager.Instance.GetContentType(Path.GetExtension(FileName).Replace(".", ""));
            try
            {
                Response.WriteFile(FileName);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
                objResponse.Write("Error : " + ex.Message);
            }
            objResponse.Flush();
            objResponse.End();
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }
        /*
        private void CopyTemplate(string Folder, string TemplateName)
        {
            try
            {
                string FolderName = GetModuleSubDir() + "/Templates/" + TemplateName;
                var folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                int idx = 1;
                while (folder != null)
                {
                    FolderName = GetModuleSubDir() + "/Templates/" + TemplateName + idx;
                    folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                    idx++;
                }
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(PortalId, FolderName);
                }
                foreach (var item in Directory.GetFiles(Folder))
                {
                    File.Copy(item, folder.PhysicalPath + Path.GetFileName(item));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        */
        protected void lbCopy_Click(object sender, EventArgs e)
        {
            string oldFolder = Server.MapPath(ddlCopyTemplate.SelectedValue);
            try
            {
                //CopyTemplate(oldFolder, tbCopyName.Text);
                OpenContentUtils.CopyTemplate(PortalId, oldFolder, tbCopyName.Text);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Copy Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }
    }
}

