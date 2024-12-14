using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components.Manifest;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Services.Localization;
using System.IO;
using System.Web.UI;

namespace Satrabel.OpenContent.Components
{
    public class ViewControl : IModuleControl, IActionable
    {
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        public ViewControl()
        {
            //this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.ControlPath = "DesktopModules/OpenContent";
            this.ID = "View.ascx";
        }
        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {

                var _engine = new RenderEngine(OpenContentModuleConfig.Create(ModuleContext.Configuration, PortalSettings.Current), new DnnRenderContext(ModuleContext), LocalResourceFile);
                //_engine.QueryString = Page.Request.QueryString;
                //if (Page.Request.QueryString["id"] != null)
                //{
                //    _engine.ItemId = Page.Request.QueryString["id"];
                //}


                var actions = new ModuleActionCollection();
                var actionDefinitions = _engine.GetMenuActions();

                foreach (var item in actionDefinitions)
                {
                    actions.Add(ModuleContext.GetNextActionID(),
                        item.Title,
                        item.ActionType.ToDnnActionType(),
                        "",
                        item.Image,
                        item.Url,
                        false,
                        item.AccessLevel.ToDnnSecurityAccessLevel(),
                        true,
                        item.NewWindow);
                }

                return actions;
            }
        }
        public string ID { get; set; }

        /// <summary>Gets or Sets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public string ControlPath { get; set; }

        /// <summary>Gets the Name for this control.</summary>
        /// <returns>A String.</returns>
        public string ControlName
        {
            get
            {
                return this.GetType().Name.Replace("_", ".");
            }
        }

        /// <summary>Gets the Module Context for this control.</summary>
        /// <returns>A ModuleInstanceContext.</returns>
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this.moduleContext == null)
                {
                    this.moduleContext = new ModuleInstanceContext(this);
                }

                return this.moduleContext;
            }
        }

        public Control Control => throw new NotImplementedException();



        #endregion

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = Path.Combine(this.ControlPath, DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + this.ID);
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }
    }
}
