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
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;
using System.IO;
using Satrabel.OpenContent.Components;
using Newtonsoft.Json.Linq;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Entities.Host;
using DotNetNuke.Common.Utilities;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.Web;

#endregion

namespace Satrabel.OpenContent
{

    public partial class ShareTemplate : PortalModuleBase
    {
        public string ModuleTemplateDirectory
        {
            get
            {
                return PortalSettings.HomeDirectory + "OpenContent/Templates/" + ModuleId.ToString() + "/";
            }
        }
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //cmdSave.Click += cmdSave_Click;
            //cmdCancel.Click += cmdCancel_Click;
            //ServicesFramework.Instance.RequestAjaxScriptSupport();
            //ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
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
            if (rblAction.SelectedIndex == 0) // import
            {
                phImport.Visible = true;
            }
            else if (rblAction.SelectedIndex == 1) // export
            {
                phExport.Visible = true;
                ddlTemplates.Items.Clear();
                ddlTemplates.Items.AddRange(OpenContentUtils.GetTemplates(PortalSettings, ModuleId, "").ToArray());
            }
        }
        protected void cmdImport_Click(object sender, EventArgs e)
        {
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(PortalId, "OpenContent/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(PortalId, "OpenContent/Templates");
                }
                var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
                if (Path.GetExtension(fuFile.FileName) == ".zip")
                {
                    string TemplateName = Path.GetFileNameWithoutExtension(fuFile.FileName);
                    string FolderName = "OpenContent/Templates/"+TemplateName;
                    folder = FolderManager.Instance.GetFolder(PortalId, FolderName);
                    int idx = 1;
                    while (folder != null)
                    {
                        FolderName = "OpenContent/Templates/" + TemplateName + idx;
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
            catch (PermissionsNotMetException exc)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), "OpenContent/Templates");
            }
            catch (NoSpaceAvailableException exc)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fuFile.FileName);
            }
            catch (InvalidFileExtensionException exc)
            {
                //Logger.Warn(exc);
                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fuFile.FileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception exc)
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
            var folder = FolderManager.Instance.GetFolder(PortalId, "OpenContent/Templates");
            if (folder == null)
            {
                folder = FolderManager.Instance.AddFolder(PortalId, "OpenContent/Templates");
            }
            var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
            //var file = fileManager.AddFile(folder, fuFile.FileName, fuFile.FileContent, true, fuFile.PostedFile.co);
            //var file = fileManager.AddFile(folder, fuFile.FileName, fuFile.PostedFile.InputStream, true, false, fuFile.PostedFile.ContentType);
            CreateZipFile(Server.MapPath(ddlTemplates.SelectedValue)+".zip", Server.MapPath(ddlTemplates.SelectedValue));
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, "Export Successful", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.GreenSuccess);
        }
        private void CreateZipFile(string zipFileName, string Folder)
        {
            //string basePath = Server.MapPath(OpenContentUtils.GetSiteTemplateFolder(PortalSettings));
            //string packageFilePath = Folder.Replace(basePath, "");
            //zipFileName = basePath + zipFileName + ".zip";
            int CompressionLevel = 9;
            var zipFile = new System.IO.FileInfo(zipFileName);

            string ZipFileShortName = zipFile.Name;

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
                    objResponse.AppendHeader("content-disposition", "attachment; filename=\"" +  Path.GetFileName(FileName) + "\"");
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
 
    }
}

