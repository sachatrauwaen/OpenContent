using DotNetNuke.Application;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Data;
using Newtonsoft.Json.Linq;
using Satrabel.OpenContent.Components.Alpaca;
using Satrabel.OpenContent.Components.Datasource.Search;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DnnPortalSettingsDataSource : DefaultDataSource //, IDataActions
    {
        public override string Name => "Satrabel.DnnPortalSettings";

        public override IDataItem Get(DataSourceContext context, string id)
        {
            IDataItem data = null;
            PortalSettingInfoBase item;
            if (DotNetNukeContext.Current.Application.Version.Major <= 7)
            {
                var ps = new PortalSettingInfo7(id);
                item = GetByPrimaryKey(ps.PortalID, ps.SettingName, ps.CultureCode);
            }
            else
            {
                item = GetById<PortalSettingInfo>(Int32.Parse(id));
            }
            data = ToData(item);
            return data;
        }
        private static IDataItem ToData(PortalSettingInfoBase setting)
        {
            var item = new DefaultDataItem()
            {
                Id = setting.Id(),
                Title = $"{setting.SettingName}",
                Data = JObject.FromObject(new
                {
                    Id = setting.Id(),
                    setting.PortalID,
                    setting.SettingName,
                    setting.SettingValue,
                    setting.CultureCode,
                    //setting.CreatedByUserID,
                    //setting.CreatedOnDate,
                    //setting.LastModifiedByUserID,
                    //setting.LastModifiedOnDate
                }),
                CreatedByUserId = setting.CreatedByUserID.GetValueOrDefault(),
                LastModifiedByUserId = setting.LastModifiedByUserID.GetValueOrDefault(),
                LastModifiedOnDate = setting.LastModifiedOnDate.GetValueOrDefault(),
                CreatedOnDate = setting.CreatedOnDate.GetValueOrDefault(),
                Item = setting
            };
            return item;
        }

        public override IDataItems GetAll(DataSourceContext context, Select selectQuery)
        {
            IEnumerable<PortalSettingInfoBase> all;
            if (DotNetNukeContext.Current.Application.Version.Major <= 7)
            {
                all = GetAll<PortalSettingInfo7>(context.PortalId);
            }
            else
            {
                all = GetAll<PortalSettingInfo>(context.PortalId);
            }
            int total = all.Count();
            if (selectQuery != null)
            {
                var settingName = selectQuery.Query.FilterRules.FirstOrDefault(f => f.Field == "SettingName");
                if (settingName != null)
                {
                    all = all.Where(t => t.SettingName.ToLower().Contains(settingName.Value.AsString.ToLower()));
                }
                settingName = selectQuery.Query.FilterRules.FirstOrDefault(f => f.Field == "CultureCode");
                if (settingName != null)
                {
                    all = all.Where(t => t.CultureCode.ToLower().Contains(settingName.Value.AsString.ToLower()));
                }
                all = all.Skip(selectQuery.PageIndex * selectQuery.PageSize).Take(selectQuery.PageSize).ToList();
            }
            var dataList = new List<IDataItem>();
            foreach (var setting in all)
            {
                dataList.Add(ToData(setting));
            }
            return new DefaultDataItems()
            {
                Items = dataList,
                Total = total
            };
        }

        public override JObject GetAlpaca(DataSourceContext context, bool schema, bool options, bool view)
        {
            var fb = new FormBuilder(new FolderUri(context.TemplateFolder));
            var alpaca = fb.BuildForm("", context.CurrentCultureCode);
            return alpaca;
        }
        public override void Add(DataSourceContext context, JToken data)
        {
            var schema = GetAlpaca(context, true, false, false)["schema"] as JObject;

            if (DotNetNukeContext.Current.Application.Version.Major <= 7)
            {
                var sett = new PortalSettingInfo7();
                sett.PortalID = context.PortalId;
                sett.CultureCode = context.CurrentCultureCode;
                sett.CreatedByUserID = context.UserId;
                sett.CreatedOnDate = DateTime.Now;
                sett.LastModifiedByUserID = context.UserId;
                sett.LastModifiedOnDate = DateTime.Now;
                if (HasProperty(schema, "", "SettingName"))
                {
                    sett.SettingName = data["SettingName"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "SettingValue"))
                {
                    sett.SettingValue = data["SettingValue"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "CultureCode"))
                {
                    sett.CultureCode = data["CultureCode"]?.ToString() ?? "";
                }

                Add(sett);
            }
            else
            {
                var sett = new PortalSettingInfo();
                sett.PortalID = context.PortalId;
                sett.CultureCode = context.CurrentCultureCode;
                sett.CreatedByUserID = context.UserId;
                sett.CreatedOnDate = DateTime.Now;
                sett.LastModifiedByUserID = context.UserId;
                sett.LastModifiedOnDate = DateTime.Now;
                if (HasProperty(schema, "", "SettingName"))
                {
                    sett.SettingName = data["SettingName"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "SettingValue"))
                {
                    sett.SettingValue = data["SettingValue"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "CultureCode"))
                {
                    sett.CultureCode = data["CultureCode"]?.ToString() ?? "";
                    if (sett.CultureCode == "") sett.CultureCode = null;
                }

                Add(sett);
            }

        }

        public override void Update(DataSourceContext context, IDataItem item, JToken data)
        {
            var schema = GetAlpaca(context, true, false, false)["schema"] as JObject;

            if (DotNetNukeContext.Current.Application.Version.Major <= 7)
            {
                var sett = (PortalSettingInfo7)item.Item;
                if (HasProperty(schema, "", "SettingName"))
                {
                    sett.SettingName = data["SettingName"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "SettingValue"))
                {
                    sett.SettingValue = data["SettingValue"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "CultureCode"))
                {
                    sett.CultureCode = data["CultureCode"]?.ToString() ?? "";
                }
                Update(sett);
            }
            else
            {
                var sett = (PortalSettingInfo)item.Item;
                if (HasProperty(schema, "", "SettingName"))
                {
                    sett.SettingName = data["SettingName"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "SettingValue"))
                {
                    sett.SettingValue = data["SettingValue"]?.ToString() ?? "";
                }
                if (HasProperty(schema, "", "CultureCode"))
                {
                    sett.CultureCode = data["CultureCode"]?.ToString() ?? "";
                    if (sett.CultureCode == "") sett.CultureCode = null;
                }
                Update(sett);
            }

        }

        public override void Delete(DataSourceContext context, IDataItem item)
        {
            throw new NotImplementedException();
        }

        //public List<IDataAction> GetActions(DataSourceContext context, IDataItem item)
        //{
        //    return null;
        //}

        //public override JToken Action(DataSourceContext context, string action, IDataItem item, JToken data)
        //{
        //    throw new NotImplementedException();
        //}

        #region private methods

        private static bool HasProperty(JObject schema, string subobject, string property)
        {
            if (!string.IsNullOrEmpty(subobject))
            {
                schema = schema[subobject] as JObject;
            }
            if (!(schema?["properties"] is JObject)) return false;

            return ((JObject)schema["properties"]).Properties().Any(p => p.Name == property);
        }

        #endregion

        public T GetById<T>(int id) where T : class
        {
            T t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<T>();
                t = rep.GetById(id);
            }
            return t;
        }

        public T Update<T>(T setting) where T : class
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<T>();
                rep.Update(setting);
            }
            return setting;
        }

        public T Add<T>(T setting) where T : class
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<T>();
                rep.Insert(setting);
            }
            return setting;
        }

        #region DNN 7 stuff

        public PortalSettingInfo7 Update(PortalSettingInfo7 setting)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(CommandType.Text,
                    "UPDATE {databaseOwner}[{objectQualifier}PortalSettings] SET SettingName = @1, SettingValue = @3, CultureCode = @2 WHERE PortalID = @0 AND SettingName = @1 AND CultureCode = @2 ",
                    setting.PortalID, setting.SettingName, setting.CultureCode, setting.SettingValue);
            }
            return setting;
        }

        public PortalSettingInfo7 Add(PortalSettingInfo7 setting)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(CommandType.Text,
                    "INSERT INTO {databaseOwner}[{objectQualifier}PortalSettings] "
                    + "(PortalID, SettingName, SettingValue, CreatedByUserID, CreatedOnDate, LastModifiedByUserID, LastModifiedOnDate, CultureCode) "
                    + "VALUES (@0, @1, @2, @3, @4, @5, @6, @7)",
                    setting.PortalID, setting.SettingName, setting.SettingValue, setting.CreatedByUserID, setting.CreatedOnDate, setting.LastModifiedByUserID, setting.LastModifiedOnDate, setting.CultureCode);
            }
            return setting;
        }

        public IEnumerable<T> GetAll<T>(int portalId) where T : PortalSettingInfoBase
        {
            IEnumerable<T> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<T>();
                t = rep.Get(portalId);
            }
            return t;
        }

        public PortalSettingInfo7 GetByPrimaryKey(int portalId, string settingName, string cultureCode)
        {
            PortalSettingInfo7 t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<PortalSettingInfo7>();
                t = rep.Find("WHERE PortalID = @0 AND SettingName = @1 AND CultureCode = @2", portalId, settingName, cultureCode).FirstOrDefault();
            }
            return t;
        }

        #endregion
    }

    [TableName("PortalSettings")]
    [Scope("PortalID")]
    public class PortalSettingInfo7 : PortalSettingInfoBase
    {
        public PortalSettingInfo7() : base() { }
        public PortalSettingInfo7(string id) : base()
        {
            var values = id.Split(IdSep);
            PortalID = int.Parse(values[0]);
            SettingName = values[1];
            CultureCode = values[2];
        }
        public const char IdSep = '~';
        public const char IdSepReplace = '*';
        public override string Id()
        {
            return $"{PortalID}{IdSep}{SettingName.Replace(IdSep, IdSepReplace)}{IdSep}{CultureCode}";
        }
    }

    [TableName("PortalSettings")]
    [PrimaryKey("PortalSettingID", AutoIncrement = true)]
    [Scope("PortalID")]
    public class PortalSettingInfo : PortalSettingInfoBase
    {
        public override string Id()
        {
            return PortalSettingID.ToString();
        }

        public int PortalSettingID { get; set; }
    }
    public abstract class PortalSettingInfoBase
    {
        protected PortalSettingInfoBase()
        {
        }

        public abstract string Id();
        public int PortalID { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        public string CultureCode { get; set; }
    }
}