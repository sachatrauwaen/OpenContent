using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace Satrabel.OpenContent
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Collections;

   
        /// <summary>
        /// Summary description for GroupDropDownList.
        /// </summary>
        [ToolboxData("<{0}:GroupDropDownList runat=server></{0}:GroupDropDownList>")]
        public class GroupDropDownList : DropDownList
        {
            /// <summary>
            /// The field in the datasource which provides values for groups
            /// </summary>
            [DefaultValue(""), Category("Data")]
            public virtual string DataGroupField
            {
                get
                {
                    object obj = ViewState["DataGroupField"];
                    if (obj != null)
                    {
                        return (string)obj;
                    }
                    return string.Empty;
                }
                set
                {
                    ViewState["DataGroupField"] = value;
                }
            }
            /// <summary>
            /// if a group doesn't has any enabled items,there is no need
            /// to render the group too
            /// </summary>
            /// <param name="groupName"></param>
            /// <returns></returns>
            private bool IsGroupHasEnabledItems(string groupName)
            {
                ListItemCollection items = Items;
                for (int i = 0; i < items.Count; i++)
                {
                    ListItem item = items[i];
                    if ( (item.Attributes["DataGroupField"] != null && item.Attributes["DataGroupField"].Equals(groupName)) && item.Enabled)
                    {
                        return true;
                    }
                }
                return false;
            }
            /// <summary>
            /// Render this control to the output parameter specified.
            /// Based on the source code of the original DropDownList method
            /// </summary>
            /// <param name="writer"> The HTML writer to write out to </param>
            protected override void RenderContents(HtmlTextWriter writer)
            {
                ListItemCollection items = Items;
                int itemCount = Items.Count;
                string curGroup = String.Empty;
                bool bSelected = false;

                if (itemCount <= 0)
                {
                    return;
                }

                for (int i = 0; i < itemCount; i++)
                {
                    ListItem item = items[i];
                    string itemGroup = item.Attributes["DataGroupField"];
                    if (itemGroup != null && itemGroup != curGroup && IsGroupHasEnabledItems(itemGroup))
                    {
                        if (curGroup != String.Empty)
                        {
                            writer.WriteEndTag("optgroup");
                            writer.WriteLine();
                        }

                        curGroup = itemGroup;
                        writer.WriteBeginTag("optgroup");
                        writer.WriteAttribute("label", curGroup, true);
                        writer.Write('>');
                        writer.WriteLine();
                    }
                    // we don't want to render disabled items
                    if (item.Enabled)
                    {
                        writer.WriteBeginTag("option");
                        if (item.Selected)
                        {
                            if (bSelected)
                            {
                                throw new HttpException("Cant_Multiselect_In_DropDownList");
                            }
                            bSelected = true;
                            writer.WriteAttribute("selected", "selected", false);
                        }

                        writer.WriteAttribute("value", item.Value, true);
                        writer.Write('>');
                        HttpUtility.HtmlEncode(item.Text, writer);
                        writer.WriteEndTag("option");
                        writer.WriteLine();
                    }
                }
                if (curGroup != String.Empty)
                {
                    writer.WriteEndTag("optgroup");
                    writer.WriteLine();
                }
            }

            /// <summary>
            /// Perform data binding logic that is associated with the control
            /// </summary>
            /// <param name="e">An EventArgs object that contains the event data</param>
            protected override void OnDataBinding(EventArgs e)
            {
                // Call base method to bind data
                base.OnDataBinding(e);

                if (DataGroupField == String.Empty)
                {
                    return;
                }
                // For each Item add the attribute "DataGroupField" with value from the datasource
                IEnumerable dataSource = GetResolvedDataSource(DataSource, DataMember);
                if (dataSource != null)
                {
                    ListItemCollection items = Items;
                    int i = 0;

                    string groupField = DataGroupField;
                    foreach (object obj in dataSource)
                    {
                        string groupFieldValue = DataBinder.GetPropertyValue(obj, groupField, null);
                        ListItem item = items[i];
                        item.Attributes.Add("DataGroupField", groupFieldValue);
                        i++;
                    }
                }

            }

            /// <summary>
            /// This is copy of the internal ListControl method
            /// </summary>
            /// <param name="dataSource"></param>
            /// <param name="dataMember"></param>
            /// <returns></returns>
            private IEnumerable GetResolvedDataSource(object dataSource, string dataMember)
            {
                if (dataSource != null)
                {
                    var source1 = dataSource as IListSource;
                    if (source1 != null)
                    {
                        IList list1 = source1.GetList();
                        if (!source1.ContainsListCollection)
                        {
                            return list1;
                        }
                        var list = list1 as ITypedList;
                        if (list != null)
                        {
                            var list2 = list;
                            PropertyDescriptorCollection collection1 = list2.GetItemProperties(new PropertyDescriptor[0]);
                            if ((collection1 == null) || (collection1.Count == 0))
                            {
                                throw new HttpException("ListSource_Without_DataMembers");
                            }

                            PropertyDescriptor descriptor1 = collection1[0];

                            if (!string.IsNullOrWhiteSpace(dataMember))
                            {
                                descriptor1 = collection1.Find(dataMember, true);
                            }

                            if (descriptor1 != null)
                            {
                                object obj1 = list1[0];
                                object obj2 = descriptor1.GetValue(obj1);
                                var enumerable = obj2 as IEnumerable;
                                if (enumerable != null)
                                {
                                    return enumerable;
                                }
                            }
                            throw new HttpException("ListSource_Missing_DataMember");
                        }
                    }
                    var source = dataSource as IEnumerable;
                    if (source != null)
                    {
                        return source;
                    }
                }
                return null;
            }
            #region Internal behaviour
            /// <summary>
            /// Saves the state of the view.
            /// </summary>
            protected override object SaveViewState()
            {
                // Create an object array with one element for the CheckBoxList's
                // ViewState contents, and one element for each ListItem in skmCheckBoxList
                var state = new object[Items.Count + 1];

                object baseState = base.SaveViewState();
                state[0] = baseState;

                // Now, see if we even need to save the view state
                bool itemHasAttributes = false;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Attributes.Count == 0) continue;

                    itemHasAttributes = true;

                    // Create an array of the item's Attribute's keys and values
                    var attribKv = new object[Items[i].Attributes.Count * 2];
                    int k = 0;
                    foreach (string key in Items[i].Attributes.Keys)
                    {
                        attribKv[k++] = key;
                        attribKv[k++] = Items[i].Attributes[key];
                    }

                    state[i + 1] = attribKv;
                }

                // return either baseState or state, depending on if any ListItems had attributes
                return itemHasAttributes ? state : baseState;
            }

            /// <summary>
            /// Loads the state of the view.
            /// </summary>
            /// <param name="savedState">State of the saved.</param>
            protected override void LoadViewState(object savedState)
            {
                if (savedState == null) return;

                // see if savedState is an object or object array
                var objects = savedState as object[];
                if (objects != null)
                {
                    // we have an array of items with attributes
                    object[] state = objects;
                    base.LoadViewState(state[0]); // load the base state

                    for (int i = 1; i < state.Length; i++)
                    {
                        if (state[i] != null)
                        {
                            // Load back in the attributes
                            var attribKv = (object[])state[i];
                            for (int k = 0; k < attribKv.Length; k += 2)
                                Items[i - 1].Attributes.Add(attribKv[k].ToString(),
                                    attribKv[k + 1].ToString());
                        }
                    }
                }
                else
                {
                    // we have just the base state
                    base.LoadViewState(savedState);
                }
            }
            #endregion
        }
    }
