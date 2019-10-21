using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Mail;
using Satrabel.OpenContent.Components.Datasource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Satrabel.OpenContent.Components.Dnn
{
    public class DnnUsersUtils
    {
        public static void CreateUserForItems(int TabId, int ModuleId, int userToChangeId, string passPrefix, string passSuffix, string roleName, string titlePath, string emailPath, string firstName)
        {
            //int ModuleId = 585; // dev 682;
            //int TabId = 160; // dev 210;
            ModuleController mc = new ModuleController();
            var activeModule = mc.GetModule(ModuleId, TabId, false);
            if (activeModule != null)
            {
                var ocModule = new OpenContentModuleInfo(activeModule);

                //IDataSource ds = DataSourceManager.GetDataSource(module.Settings.Manifest.DataSource);
                var dsContext = OpenContentUtils.CreateDataContext(ocModule);
                var ds = new OpenContentDataSource();
                var items = ds.GetAll(dsContext);
                foreach (var item in items.Items)
                {
                    if (item.CreatedByUserId == userToChangeId)
                    {
                        var title = item.Data.SelectToken(titlePath, false)?.ToString();
                        var email = item.Data.SelectToken(emailPath, false)?.ToString();
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(email))
                        {
                            //var name = title.Replace(" ", "").ToLower();
                            var password = (new Guid()).ToString().Substring(0, 10);
                            int userid =CreateUser(email, passPrefix + item.Id+ passSuffix, firstName, title, email, roleName);
                            var content = (OpenContentInfo)item.Item;
                            content.CreatedByUserId = userid;
                            ds.Update(dsContext, item, item.Data);
                        }
                    }
                    break;
                }
            }
        }

        private static int CreateUser(string username, string password, string firstName, string lastName, string email, string roleName = "")
        {
            var ps = PortalSettings.Current;
            var user = new UserInfo
            {
                AffiliateID = Null.NullInteger,
                PortalID = ps.PortalId,
                IsDeleted = false,
                IsSuperUser = false,
                Profile = new UserProfile()
            };

            //user.LastIPAddress = Request.UserHostAddress

            user.Profile.InitialiseProfile(ps.PortalId, true);
            user.Profile.PreferredLocale = ps.DefaultLanguage;
            user.Profile.PreferredTimeZone = ps.TimeZone;

            user.Username = username;
            user.Membership.Password = password;
            //user.DisplayName = DisplayName"]?.ToString() ?? "";
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            //user.Profile.PreferredLocale = data["PreferredLocale"].ToString();

            //FillUser(data, schema, user);
            //FillProfile(data, schema, user, false);
            UpdateDisplayName(ps.PortalId, user);
            if (string.IsNullOrEmpty(user.DisplayName))
            {
                user.DisplayName = user.FirstName + " " + user.LastName;
            }
            user.Membership.Approved = true;  //chkAuthorize.Checked;
            var newUser = user;
            var createStatus = UserController.CreateUser(ref newUser);
            bool notify = true;
            if (createStatus == UserCreateStatus.Success)
            {
                string strMessage = "";
                if (notify)
                {
                    //Send Notification to User
                    if (ps.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        //strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, ps);
                    }
                    else
                    {
                        //strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, ps);
                    }
                }
                if (!string.IsNullOrEmpty(strMessage))
                {
                    App.Services.Logger.Error($"Error sending notification email: [{strMessage}]");
                    //don't throw error, otherwise item does not get indexed.
                    //throw new Exception($"Error sending notification email: {strMessage}"); 
                }
                //FillProfile(data, schema, user, true);
                //UpdateRoles(context, data, schema, user);
                if (!string.IsNullOrEmpty(roleName))
                {
                    var roleInfo = RoleController.Instance.GetRoleByName(ps.PortalId, roleName);
                    RoleController.AddUserRole(user, roleInfo, ps, RoleStatus.Approved, Null.NullDate, Null.NullDate, false, false);
                }
                return user.UserID;
            }
            else
            {
                App.Services.Logger.Error($"Creation of user failed with createStatus: {createStatus}");
                throw new DataNotValidException(App.Services.Localizer.GetString(createStatus.ToString()) + " (1)");
            }

        }
        private static void UpdateDisplayName(int portalId, UserInfo user)
        {
            //Update DisplayName to conform to Format
            object setting = UserModuleBase.GetSetting(portalId, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                user.UpdateDisplayName(Convert.ToString(setting));
            }
        }
    }
}