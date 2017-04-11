using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DnnUsersDataSource : DefaultDataSource, IDataActions
    {
        const string Registered_Users = "Registered Users";
        public override string Name
        {
            get
            {
                return "Satrabel.DnnUsers";
            }
        }
        public override IDataItem Get(DataSourceContext context, string id)
        {
            //return GetAll(context, null).Items.SingleOrDefault(i => i.Id == id);
            var user = UserController.GetUserById(context.PortalId, int.Parse(id));
            if (user.IsInRole("Administrators"))
            {
                throw new Exception("Not autorized to get Administrator users");
            }
            return ToData(user);
        }
        private IDataItem ToData(UserInfo user)
        {
            var item = new DefaultDataItem()
            {
                Id = user.UserID.ToString(),
                Title = user.DisplayName,
                Data = JObject.FromObject(new
                {
                    user.UserID,
                    user.DisplayName,
                    user.Username,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Membership.Approved,
                    user.Membership.LockedOut,
                    user.IsDeleted
                }),
                CreatedByUserId = user.CreatedByUserID,
                LastModifiedByUserId = user.LastModifiedByUserID,
                LastModifiedOnDate = user.LastModifiedOnDate,
                CreatedOnDate = user.CreatedOnDate,
                Item = user
            };
            item.Data["Profile"] = new JObject();
            foreach (ProfilePropertyDefinition def in user.Profile.ProfileProperties)
            {
                //ProfilePropertyAccess.GetRichValue(def, )
                if (def.PropertyName.ToLower() == "photo")
                {
                    item.Data["Profile"]["PhotoURL"] = user.Profile.PhotoURL;
                    item.Data["Profile"][def.PropertyName] = def.PropertyValue;
                }
                else
                {
                    item.Data["Profile"][def.PropertyName] = def.PropertyValue;
                }
            }
            var roles = new JArray();
            item.Data["Roles"] = roles;
            foreach (var role in user.Roles)
            {
                if (role != Registered_Users)
                {
                    roles.Add(role);
                }
            }
            return item;
        }

        public override IDataItems GetAll(DataSourceContext context, Search.Select selectQuery)
        {
            int pageIndex = 0;
            int pageSize = 1000;
            int total = 0;
            IEnumerable<UserInfo> users;
            if (selectQuery != null)
            {
                pageIndex = selectQuery.PageIndex;
                pageSize = selectQuery.PageSize;
                var ruleDisplayName = selectQuery.Query.FilterRules.FirstOrDefault(f => f.Field == "DisplayName");
                var ruleRoles = selectQuery.Query.FilterRules.FirstOrDefault(f => f.Field == "Roles");
                if (ruleDisplayName != null)
                {
                    string displayName = ruleDisplayName.Value.AsString + "%";
                    users = UserController.GetUsersByDisplayName(context.PortalId, displayName, pageIndex, pageSize, ref total, true, false).Cast<UserInfo>();
                }
                else
                {
                    users = UserController.GetUsers(context.PortalId, pageIndex, pageSize, ref total, true, false).Cast<UserInfo>();
                    total = users.Count();
                }
                if (ruleRoles != null)
                {
                    var roleNames = ruleRoles.MultiValue.Select(r => r.AsString).ToList();
                    users = users.Where(u => u.Roles.Intersect(roleNames).Any());
                }
            }
            else
            {
                users = UserController.GetUsers(context.PortalId, pageIndex, pageSize, ref total, true, false).Cast<UserInfo>();
            }
            users = users.Where(u => !u.IsInRole("Administrators"));
            //users = users.Skip(pageIndex * pageSize).Take(pageSize);
            var dataList = new List<IDataItem>();
            foreach (var user in users)
            {
                dataList.Add(ToData(user));
            }
            return new DefaultDataItems()
            {
                Items = dataList,
                Total = total,
                //DebugInfo = 
            };
        }
        public override JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            var alpaca = fb.BuildForm("", context.CurrentCultureCode);
            return alpaca;
        }
        public override void Add(DataSourceContext context, Newtonsoft.Json.Linq.JToken data)
        {
            var schema = GetAlpaca(context, true, false, false)["schema"] as JObject;
            var user = new UserInfo();
            user.AffiliateID = Null.NullInteger;
            user.PortalID = context.PortalId;

            user.IsDeleted = false;
            user.IsSuperUser = false;
            //user.LastIPAddress = Request.UserHostAddress
            user.Profile = new UserProfile();
            var ps = PortalSettings.Current;
            user.Profile.InitialiseProfile(ps.PortalId, true);
            user.Profile.PreferredLocale = ps.DefaultLanguage;
            user.Profile.PreferredTimeZone = ps.TimeZone;
            if (HasProperty(schema, "", "Username"))
            {
                user.Username = data["Username"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "Password"))
            {
                user.Membership.Password = data["Password"]?.ToString() ?? "";
            }
            FillUser(data, schema, user);
            UpdateDisplayName(context, user);
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
                if (notify)
                {
                    string strMessage = "";
                    //Send Notification to User
                    if (ps.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, ps);
                    }
                    else
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, ps);
                    }
                }
                FillProfile(data, schema, user);
                UpdateRoles(context, data, schema, user);
            }
            else
            {
                throw new DataNotValidException(Localization.GetString(createStatus.ToString()));
            }
        }
        public override void Update(DataSourceContext context, IDataItem item, Newtonsoft.Json.Linq.JToken data)
        {
            var schema = GetAlpaca(context, true, false, false)["schema"] as JObject;
            var user = (UserInfo)item.Item;
            FillUser(data, schema, user);
            UserController.UpdateUser(context.PortalId, user);
            if (HasProperty(schema, "", "Approved"))
            {
                bool approved = (bool)(data["Approved"] as JValue).Value;
                if (approved != user.Membership.Approved)
                {
                    if (approved)
                    {
                        user.Membership.Approved = true;
                        UserController.UpdateUser(context.PortalId, user);
                        //Update User Roles if needed
                        if (!user.IsSuperUser && user.IsInRole("Unverified Users") && PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                        {
                            UserController.ApproveUser(user);
                        }
                    }
                    else
                    {
                        user.Membership.Approved = false;
                    }
                }
            }
            UserController.UpdateUser(context.PortalId, user);
            if (HasProperty(schema, "", "Password") && data["Password"] != null)
            {
                string password = data["Password"].ToString();
                ChangePassword(user, password);
            }
            if (HasProperty(schema, "", "Username") && data["Username"] != null && !string.IsNullOrEmpty(data["Username"].ToString()))
            {
                var userName = data["Username"].ToString();
                if (userName != user.Username)
                {
                    UpdateDisplayName(context, user);
                    try
                    {
                        //Update DisplayName to conform to Format
                        UserController.ChangeUsername(user.UserID, userName);
                    }
                    catch (Exception exc)
                    {
                        throw new DataNotValidException(Localization.GetString("Username not valid"));
                        //var args = new UserUpdateErrorArgs(User.UserID, User.Username, "EmailError");
                    }
                }
            }
            FillProfile(data, schema, user);
            UpdateRoles(context, data, schema, user);
        }

        private void UpdateRoles(DataSourceContext context, JToken data, JObject schema, UserInfo user)
        {
            if (HasProperty(schema, "", "Roles"))
            {
                List<string> rolesToRemove = new List<string>(user.Roles);
                var roles = data["Roles"] as JArray;
                foreach (var role in roles)
                {
                    string roleName = role.ToString();
                    rolesToRemove.Remove(roleName);
                    if (!user.Roles.Contains(roleName))
                    {
                        var roleInfo = RoleController.Instance.GetRoleByName(context.PortalId, roleName);
                        RoleController.AddUserRole(user, roleInfo, PortalSettings.Current, RoleStatus.Approved, Null.NullDate, Null.NullDate, false, false);
                    }
                }
                foreach (var roleName in rolesToRemove)
                {
                    if (roleName != Registered_Users)
                    {
                        var roleInfo = RoleController.Instance.GetRoleByName(context.PortalId, roleName);
                        RoleController.DeleteUserRole(user, roleInfo, PortalSettings.Current, false);
                    }
                }
            }
        }
        private void ChangePassword(UserInfo user, string password)
        {
            // Check New Password is Valid
            if (!UserController.ValidatePassword(password))
            {
                throw new DataNotValidException(Localization.GetString("PasswordInvalid"));
            }
            // Check New Password is not same as username or banned
            var settings = new MembershipPasswordSettings(user.PortalID);
            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController();
                if (m.FoundBannedPassword(password) || user.Username == password)
                {
                    throw new DataNotValidException(Localization.GetString("BannedPasswordUsed"));
                }
            }
            UserController.ResetAndChangePassword(user, password);
        }
        private void FillProfile(JToken data, JObject schema, UserInfo user)
        {
            if (HasProperty(schema, "", "Profile"))
            {
                ProfileController pc = new ProfileController();
                var profile = data["Profile"] as JObject;
                var profileSchema = schema["properties"]["Profile"]["properties"] as JObject;
                foreach (var prop in profileSchema.Properties())
                {
                    if (user.Profile.GetProperty(prop.Name) != null)
                    {
                        if (profile[prop.Name] != null)
                        {
                            if (prop.Name.ToLower() == "photo")
                            {
                                user.Profile.SetProfileProperty(prop.Name, profile[prop.Name].ToString());
                            }
                            else
                            {
                                user.Profile.SetProfileProperty(prop.Name, profile[prop.Name].ToString());
                            }
                        }
                        else
                        {
                            user.Profile.SetProfileProperty(prop.Name, "");
                        }
                    }
                }
                ProfileController.UpdateUserProfile(user);
            }
        }

        private void FillUser(JToken data, JObject schema, UserInfo user)
        {
            if (HasProperty(schema, "", "DisplayName"))
            {
                user.DisplayName = data["DisplayName"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "FirstName"))
            {
                user.FirstName = data["FirstName"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "LastName"))
            {
                user.LastName = data["LastName"]?.ToString() ?? "";
            }
            if (HasProperty(schema, "", "Email"))
            {
                user.Email = data["Email"]?.ToString() ?? "";
            }
        }

        private void UpdateDisplayName(DataSourceContext context, UserInfo user)
        {
            //Update DisplayName to conform to Format
            object setting = UserModuleBase.GetSetting(context.PortalId, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                user.UpdateDisplayName(Convert.ToString(setting));
            }
        }
        public override void Delete(DataSourceContext context, IDataItem item)
        {
            var user = (UserInfo)item.Item;
            UserController.DeleteUser(ref user, true, false);
        }
        private bool HasProperty(JObject schema, string subobject, string property)
        {
            if (!string.IsNullOrEmpty(subobject))
            {
                schema = schema[subobject] as JObject;
            }
            if (schema == null || !(schema["properties"] is JObject)) return false;

            return ((JObject)schema["properties"]).Properties().Any(p => p.Name == property);
        }
        public override JToken Action(DataSourceContext context, string action, IDataItem item, JToken data)
        {
            if (action == "unlock")
            {
                var user = (UserInfo)item.Item;
                UserController.UnLockUser(user);
            }
            return null;
        }
        public List<IDataAction> GetActions(DataSourceContext context, IDataItem item)
        {
            var actions = new List<IDataAction>();
            if (item != null)
            {
                var user = (UserInfo)item.Item;
                if (user.Membership.LockedOut)
                {
                    actions.Add(new DefaultDataAction()
                    {
                        Name = "unlock",
                        AfterExecute = "disable"
                    });
                }
            }
            return actions;
        }
    }
}