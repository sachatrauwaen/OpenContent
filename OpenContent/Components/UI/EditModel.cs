#region Copyright

// 
// Copyright (c) 2015-2016
// by Satrabel
// 

#endregion

#region Using Statements

using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Render;
using Satrabel.OpenContent.Components.Settings;
using System;

#endregion

namespace Satrabel.OpenContent.Components.UI
{
    /// <summary>
    /// Model class containing all data and configuration for the Edit view
    /// </summary>
    public class EditModel : BaseModel
    {
        #region Configuration Properties

        /// <summary>
        /// Indicates if Bootstrap layout is being used
        /// </summary>
        public bool Bootstrap { get; set; }

        /// <summary>
        /// Indicates if Bootstrap CSS should be loaded
        /// </summary>
        public bool LoadBootstrap { get; set; }

        /// <summary>
        /// Indicates if Glyphicons should be loaded
        /// </summary>
        public bool LoadGlyphicons { get; set; }

        /// <summary>
        /// Indicates if Builder V2 is being used
        /// </summary>
        public bool BuilderV2 { get; set; }

        /// <summary>
        /// Google API key for maps and other services
        /// </summary>
        public string GoogleApiKey { get; set; }

        /// <summary>
        /// Indicates if the layout is horizontal bootstrap
        /// </summary>
        public bool IsHorizontalLayout { get; set; }

        #endregion

        #region Navigation URLs

        /// <summary>
        /// URL for the cancel action
        /// </summary>
        public string CancelUrl { get; set; }

        /// <summary>
        /// URL for the save action
        /// </summary>
        public string SaveUrl { get; set; }

        /// <summary>
        /// URL for the copy action
        /// </summary>
        public string CopyUrl { get; set; }

        #endregion

        #region Module Context

        /// <summary>
        /// Portal ID
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Module ID
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Item ID being edited (from query string)
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// OpenContent settings for the module
        /// </summary>
        public OpenContentSettings Settings { get; set; }

        #endregion

        #region Alpaca Context

        /// <summary>
        /// Alpaca context containing form configuration
        /// </summary>
        public AlpacaContext AlpacaContext { get; set; }

        /// <summary>
        /// Template collection prefix
        /// </summary>
        public string TemplatePrefix { get; set; }

        #endregion

        #region UI Control Properties

        /// <summary>
        /// Scope wrapper client ID
        /// </summary>
        public string ScopeWrapperClientId { get; set; }

        /// <summary>
        /// Cancel hyperlink client ID
        /// </summary>
        public string CancelClientId { get; set; }

        /// <summary>
        /// Save command client ID
        /// </summary>
        public string SaveClientId { get; set; }

        /// <summary>
        /// Copy command client ID
        /// </summary>
        public string CopyClientId { get; set; }

        /// <summary>
        /// Delete hyperlink client ID
        /// </summary>
        public string DeleteClientId { get; set; }

        /// <summary>
        /// Versions dropdown client ID
        /// </summary>
        public string VersionsClientId { get; set; }

        #endregion

        #region Visibility Properties

        /// <summary>
        /// Indicates if the copy button should be visible
        /// </summary>
        public bool IsCopyVisible { get; set; }

        /// <summary>
        /// Indicates if the delete button should be visible
        /// </summary>
        public bool IsDeleteVisible { get; set; }

        #endregion

        #region Localization

        /// <summary>
        /// Delete confirmation message
        /// </summary>
        public string DeleteConfirmMessage { get; set; }

        /// <summary>
        /// Indicates if the portal is multi-lingual
        /// </summary>
        public bool IsMultiLingual { get; set; }

        #endregion

        #region State Properties

        /// <summary>
        /// Indicates if this is a new item being created
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Page context for rendering
        /// </summary>
        public IPageContext PageContext { get; set; }
        

        #endregion


    }

    /// <summary>
    /// Container for client IDs of edit controls
    /// </summary>
    public class EditControlClientIds
    {
        public string ScopeWrapper { get; set; }
        public string Cancel { get; set; }
        public string Save { get; set; }
        public string Copy { get; set; }
        public string Delete { get; set; }
        public string Versions { get; set; }
    }
}
