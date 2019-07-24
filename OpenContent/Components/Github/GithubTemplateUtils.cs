using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Satrabel.OpenContent.Components.Github;
using DotNetNuke.Services.FileSystem;
using System.IO;
using DotNetNuke.Entities.Host;

namespace Satrabel.OpenContent.Components
{
    public static class GithubTemplateUtils
    {
        // 1. api call to template list: https://api.github.com/repos/sachatrauwaen/OpenContent-Templates/contents (type='dir')

        // 2. per template ophalen manifest: 
        // 3. uit manifest (master branch) de naam, onmschrijving en evt. afbeelding lezen 
        // inhoud schema file(is json):
        // https://raw.githubusercontent.com/sachatrauwaen/OpenContent-Templates/master/Bootstrap3Accordion/manifest.json

        // 4. lijst templates opbouwen voor tonen

        // vervolgens:
        // 5. template laten kiezen
        // 6. om eigen/locale template naam vragen plus locatie (radio button)? host map /skin map / portal map

        // 7. alle files uit het gekozen template downloaden naar locale template map
        // JSON lijst van files in OC template, voor download in locale map
        // https://api.github.com/repos/sachatrauwaen/OpenContent-Templates/contents/Bootstrap3Accordion


        private static string GetGitRepository(int portalId)
        {
            var globalSettingsRepository = App.Services.CreateGlobalSettingsRepository(portalId);
            return globalSettingsRepository.GetGithubRepository();
        }

        // Git templates
        public static List<Contents> GetTemplateList(int portalId)
        {
            // we need to force the protocol to TLS 1.2
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            List<Contents> contents = new List<Contents>();

            var gitRepos = GetGitRepository(portalId);
            foreach (var repo in gitRepos.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string url = "https://api.github.com/repos/" + repo + "/contents";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                var response = client.GetStringAsync(new Uri(url)).Result;
                if (response != null)
                {
                    //content = JArray.Parse(response);
                    contents .AddRange(Contents.FromJson(response));
                }
            }
            return contents;
        }

        public static List<Contents> GetFileList(int portalId, string path)
        {
            // we need to force the protocol to TLS 1.2
            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            List<Contents> contents = null;
            string url = "https://api.github.com/repos/" + GetGitRepository(portalId) + "/contents/" + path;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            var response = client.GetStringAsync(new Uri(url)).Result;
            if (response != null)
            {
                contents = Contents.FromJson(response);
            }
            return contents;
        }

        // all registed github templates (datasource for the repeater)
        public static List<Contents> ProcessGithubTemplatesNames(int portalId)
        {
            return GetTemplateList(portalId)
                .Where(t => t.Type == Components.Github.TypeEnum.Dir)
                .OrderBy(t => t.Name).ToList();
        }

        /*
        public static JObject GetManifestFile(string templatename)
        {
            JObject manifest = null;
            string manfesturl = "https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/" + templatename + "/manifest.json";

            //  "https://raw.githubusercontent.com/sachatrauwaen/OpenContent-Templates/master/" + tempatename + "/manifest.json";
            // https://raw.githubusercontent.com/schotman/OpenContent-Templates/gitTemplates/Bootstrap3Columns/manifest.json

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

            Uri uri = new Uri(manfesturl);
            Task<HttpResponseMessage> getManifest = client.GetAsync(uri);

            var response = getManifest.GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                Task<string> content = response.Content.ReadAsStringAsync();
                content.GetAwaiter().GetResult();
                var c1 = content.Result;
                manifest = JObject.Parse(c1);
            }
            return manifest;
        }
        */
        public static void SaveFileContent(Contents file, IFolderInfo folder)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
            Task<HttpResponseMessage> getManifest = client.GetAsync(file.DownloadUrl);
            var response = getManifest.GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync();
                var res = content.GetAwaiter().GetResult();
                File.WriteAllText(folder.PhysicalPath + file.Name, res);
            }
        }

        public static string ImportFromGithub(int portalId, string name, string path, string newTemplateName)
        {
            string strMessage = "";
            try
            {
                var folder = FolderManager.Instance.GetFolder(portalId, "OpenContent/Templates");
                if (folder == null)
                {
                    folder = FolderManager.Instance.AddFolder(portalId, "OpenContent/Templates");
                }
                if (String.IsNullOrEmpty(newTemplateName))
                {
                    newTemplateName = name;
                }
                string folderName = "OpenContent/Templates/" + newTemplateName;
                folder = FolderManager.Instance.GetFolder(portalId, folderName);
                if (folder != null)
                {
                    throw new Exception("Template already exist " + folder.FolderName);
                }
                folder = FolderManager.Instance.AddFolder(portalId, folderName);
                var fileList = GetFileList(portalId, path).Where(f => f.Type == TypeEnum.File);
                foreach (var file in fileList)
                {
                    SaveFileContent(file, folder);
                }
                return OpenContentUtils.GetDefaultTemplate(folder.PhysicalPath);
            }
            catch (PermissionsNotMetException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("InsufficientFolderPermission"), "OpenContent/Templates");
            }
            catch (NoSpaceAvailableException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("DiskSpaceExceeded"), name);
            }
            catch (InvalidFileExtensionException)
            {
                //Logger.Warn(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("RestrictedFileType"), name, Host.AllowedExtensionWhitelist.ToDisplayString());
            }
            catch (Exception exc)
            {
                //Logger.Error(exc);
                strMessage = String.Format(App.Services.Localizer.GetString("SaveFileError") + " - " + exc.Message, name);
            }
            if (!String.IsNullOrEmpty(strMessage))
            {
                throw new Exception(strMessage);
            }
            return "";
        }
    }
}