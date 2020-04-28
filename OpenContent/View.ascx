<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.View" CodeBehind="View.ascx.cs" %>
<%@ Register Src="~/DesktopModules/OpenContent/TemplateInit.ascx" TagPrefix="uc1" TagName="TemplateInit" %>
<uc1:TemplateInit runat="server" ID="TemplateInitControl" />

<asp:Panel ID="pInit" runat="server" Visible="false">

<asp:Panel ID="pVue" runat="server"  >
    <div class="oc-view" v-if="step==1" v-cloak>
        <div v-if ="!advanced ">
            <p style="text-align:center">Choose a template</p>
            <div class="octemplate" v-for="(val, index) in templates" v-if="index < 23">
                <a  :value="val.Value" @click.prevent="selectTemplate(val.Value)" href="#" :title="val.Text" >{{val.Text}}</a>
            </div>
            <div class="octemplate">
                <a href="#" @click.prevent="goAdvanced" class="advanced" ><%=Resource("Advanced")%></a>
            </div>
            <div style="clear:both"></div>
        </div>
        <div v-else class="dnnForm">
            <fieldset>
                <div v-if="advanced && existingTemplate" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <table class="dnnFormRadioButtons">
                        <tr>
                            <td>
                                <input type="radio" v-model="UseContent" @change="thisModuleChange" value="0" class="dnnRadiobutton" /><label><%= Resource("liThisModule") %></label></td>
                            <td >
                                <input type="radio" v-model="UseContent" @change="otherModuleChange" value="1" class="dnnRadiobutton" :disabled="noTemplates" /><label><%=Resource("liOtherModule")%></label></td>
                        </tr>
                    </table>
                </div>
               <%--  <div v-if="UseContent=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lDataSource") %></label>
                    <select v-model="portalId" @change="portalChange">
                        <option v-for="p in portals" :value="p.PortalId">{{p.Text}}</option>
                    </select>
                </div>--%>
                <div v-if="UseContent=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lDataSource") %></label>
                    <select v-model="tabModuleId" @change="moduleChange">
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
                <div v-if="UseTemplate=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lFrom") %></label>
                    <table class="dnnFormRadioButtons">
                        <tr>
                            <td>
                                <input type="radio" v-model="from" value="0"  @change="fromSiteChange" :disabled="noTemplates" /><label><%= Resource("liFromSite")%></label></td>
                            <td>
                                <input type="radio" v-model="from" value="1" @change="fromWebChange" /><label><%= Resource("liFromWeb")%></label></td>
                        </tr>
                    </table>
                </div>
                <div v-if="false" class="dnnFormItem" style="padding-left: 32%; margin-left: 38px; width: auto;">
                    <label runat="server" />
                </div>
                <div class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lTemplate") %></label>
                    <select v-model="Template" @change="templateChange">
                        <option v-for="template in templates" :value="template.Value">{{template.Text}}</option>
                    </select>
                </div>
                <div v-if="UseTemplate=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lTemplateName") %></label>
                    <input v-model="templateName" type="text" runat="server" />
                </div>
                <div v-if="advanced && existingTemplate && Template && detailPages.length" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lDetailPage") %></label>
                    <select v-model="detailPage">
                        <option v-for="page in detailPages" :value="page.TabId">{{page.Text}}</option>
                    </select>
                </div>
            </fieldset>
            <ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
                <li>
                    <a href="" @click.prevent="save" class="dnnPrimaryAction" :disabled="loading">{{newTemplate ? '<%= Resource("Create") %>' : '<%= Resource("Save") %>'}}</a>
                </li>
                <li>
                    <a href="#" v-if="existingTemplate && thisModule" @click.prevent="basic" class="dnnSecondaryAction" :disabled="loading" >Basic</a>
                </li>
                <li>
                    <a href="#" v-if="isPageAdmin && !otherModule && existingTemplate"  @click.prevent="newTemplateChange" class="dnnSecondaryAction" :disabled="loading" ><%=Resource("liCreateNewTemplate")%></a>
                </li>
                <li>
                    <a href="#" v-if="!otherModule && !existingTemplate"  @click.prevent="existingTemplateChange" class="dnnSecondaryAction" :disabled="loading" ><%=Resource("liUseExistingTemplate")%></a>
                </li>

            </ul>
        </div>
    </div>
    <div v-else-if="step==2" v-cloak>
        <fieldset>
             <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
            </div>
            <div class="dnnFormItem">
                <label class="dnnLabel"><%= Resource("lTemplate") %></label>
               <span style="height:30px;white-space: normal;">
                {{templateTitle}}
                </span>
                <a @click.prevent="templateDefined=false" href="#" class="dnnSecondaryAction" :disabled="loading">Change</a>
            </div>
            <div class="dnnFormItem">
                <label class="dnnLabel">Settings</label>
                <a :href="settingsUrl" class="dnnPrimaryAction">Template Settings</a> <span></span>
            </div>
        </fieldset>
    </div>
    <div v-else-if="step==3" v-cloak>
        <fieldset>
            <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
            <div class="dnnFormItem" @click="templateDefined=false">
                <label class="dnnLabel"><%= Resource("lTemplate") %></label>
               <span style="height:30px;white-space: normal;">
                {{templateTitle}}
                </span>
                <a @click.prevent="templateDefined=false" href="#" class="dnnSecondaryAction">Change</a>
            </div>
            <div class="dnnFormItem" v-if="settingsNeeded">
                <label class="dnnLabel">Settings</label>
                <a :href="settingsUrl" class="dnnSecondaryAction">Template Settings</a>
            </div>
            <div class="dnnFormItem">
                <label class="dnnLabel">Content</label>
                <a :href="editUrl" class="dnnPrimaryAction">Edit Content</a>
            </div>
        </fieldset>
        
    </div>
    <p style="text-align:center" v-if="noTemplates"><%= Resource("CreateHelp") %></p>
    <p v-if="message" style="color:#ff0000" v-cloak>{{message}}</p>
    <div v-if="loading" style="background-color:rgba(255, 255, 255, 0.70);color:#0094ff;text-align:center;position:absolute;width:100%;height:100%;top:0;left:0;padding-top:200px;text-align:center;font-size:20px;">Loading...</div>
</asp:Panel>

<script>
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        $(document).ready(function () {
            var sf = $.ServicesFramework(<%= ModuleContext.ModuleId %>);
            var app = new Vue({
                el: '#<%= pVue.ClientID %>',
                data: {
                    templateDefined: false,
                    settingsNeeded: false,
                    settingsDefined: false,
                    dataNeeded: false,
                    dataDefined: false,
                    UseContent: '0',
                    UseTemplate: '0',
                    Template: "",
                    from: "0",
                    advanced: false,
                    templates: [],
                    tabModuleId: 0,
                    portalId: 0,
                    portals: [],
                    modules: [],
                    detailPages: [],
                    detailPage: -1,
                    templateName: '',
                    settingsUrl:"<%= ModuleContext.EditUrl("EditSettings") %>",
                    editUrl: "<%= ModuleContext.EditUrl("Edit") %>",
                    message: '',
                    loading: false,
                    noTemplates: false,
                    isPageAdmin: <%=IsPageAdmin() ? "true" : "false"%> 
                },
                computed: {
                    existingTemplate: function () {
                        return this.UseTemplate == '0';
                    },
                    newTemplate: function () {
                        return this.UseTemplate == '1';
                    },
                    thisModule: function () {
                        return this.UseContent == '0';
                    },
                    otherModule: function () {
                        return this.UseContent == '1';
                    },
                    fromWeb: function () {
                        return this.from == '1';
                    },
                    step: function () {
                        if (!this.templateDefined)
                            return 1;
                        else if (this.settingsNeeded && !this.settingsDefined)
                            return 2;
                        else
                            return 3
                    },
                    templateTitle: function () {
                        for (var i = 0; i < this.templates.length; i++) {
                            if (this.templates[i].Value == this.Template) return this.templates[i].Text;
                        }
                        return this.Template;
                    }
                },
                mounted: function () {
                    var self = this;
                    self.loading = true;
                    this.apiGet('GetTemplateState', {}, function (data) {
                        self.templateDefined = data.Template;
                        self.Template = data.Template;
                        self.settingsNeeded = data.SettingsNeeded;
                        self.settingsDefined = data.SettingsDefined;
                        self.dataNeeded = data.DataNeeded;
                        self.dataDefined = data.DataDefined;
                    });
                    this.apiGet('GetTemplates', { advanced: this.advanced }, function (data) {
                        self.templates = data;
                        self.loading = false;
                        if (self.templates.length == 0) {
                            self.noTemplates = true;
                            self.advanced = true;
                            self.UseTemplate = '1';
                            self.from = '1';
                            self.loading = true;
                            self.apiGet('GetNewTemplates', { web: true }, function (data) {
                                self.templates = data;
                                self.loading = false;
                            });
                        }
                    });
                },
                watch: {
                    templates: function (val, oldval) {
                        if (val.length) {
                            if (!this.Template) {
                                this.Template = val[0].Value;
                                this.templateChange();
                            }
                        }
                    },
                    portals: function (val, oldval) {
                        if (val.length) {
                            this.PortalId = val[0].PortalId;
                            this.portalChange();
                        }
                    },
                    modules: function (val, oldval) {
                        if (val.length) {
                            this.tabModuleId = val[0].TabModuleId;
                            this.moduleChange();
                        }
                    },
                    detailPages: function (val, oldval) {
                        if (val.length) {
                            this.detailPage = val[0].TabId;
                        }
                    }
                },
                methods: {
                    basic: function () {
                        this.advanced = false;
                        this.existingTemplateChange();
                    },
                    goAdvanced: function () {
                        this.advanced = true;
                        this.existingTemplateChange();
                    },
                    thisModuleChange: function () {
                        this.UseTemplate = "0";
                        this.tabModuleId = 0;
                        this.Template = '';
                        var self = this;
                        this.apiGet('GetTemplates', { advanced: this.advanced }, function (data) {
                            self.templates = data;
                        });
                    },
                    otherModuleChange: function () {
                        this.UseTemplate = "0";
                        this.tabModuleId = 0;
                        this.Template = '';
                        var self = this;
                        //this.apiGet('GetPortals', {}, function (data) {
                        //    self.portals = data;
                        //});
                        this.apiGet('GetModules', {}, function (data) {
                            self.modules = data;
                        });
                    },
                    moduleChange: function () {
                        var self = this;
                        this.Template = '';
                        this.apiGet('GetTemplates', { tabModuleId: this.tabModuleId }, function (data) {
                            self.templates = data;
                        });
                    },
                    existingTemplateChange: function () {
                        this.UseTemplate = "0";
                        this.thisModuleChange();
                    },
                    newTemplateChange: function () {
                        this.UseTemplate = "1";
                        this.from = "0";
                        this.Template = '';
                        this.fromSiteChange();
                    },
                    fromWebChange: function () {
                        var self = this;
                        this.Template = '';
                        this.apiGet('GetNewTemplates', { web: true }, function (data) {
                            self.templates = data;
                        });
                    },
                    fromSiteChange: function () {
                        var self = this;
                        this.Template = '';
                        this.apiGet('GetNewTemplates', { web: false }, function (data) {
                            self.templates = data;
                        });
                    },
                    templateChange: function () {
                        var self = this;
                        if (this.existingTemplate) {
                            this.apiGet('GetDetailPages', { template: this.Template, tabModuleId: this.tabModuleId }, function (data) {
                                self.detailPages = data;
                            });
                        } else {
                            self.detailPages = [];
                        }
                    },
                    selectTemplate: function (template) {
                        this.Template = template;
                        this.templateChange();
                        this.save();
                    },
                    save: function () {
                        var self = this;
                        self.message = "";
                        this.apiPost('SaveTemplate', {
                            template: self.Template,
                            otherModule: self.otherModule,
                            tabModuleId: self.tabModuleId,
                            newTemplate: this.newTemplate,
                            fromWeb: this.fromWeb,
                            templateName: this.templateName
                        }, function (data) {
                            if (data.Error) {
                                self.message = data.Error;
                                return;
                            }
                            if (!data.DataNeeded) {

                                self.loading = false;
                                <%--
                                var paneId= 'dnn_<%=ModuleContext.Configuration.PaneName%>';
                                    var pane = $('#' + paneId);
                                    var parentPane = pane.data('parentpane');
                                    if (parentPane) {
                                        //this.refreshPane(parentPane, args, callback);
                                        return;
                                    }
                                    //set module manager to current refresh pane.
                                    this._moduleManager = pane.data('dnnModuleManager');
                                    var ajaxPanel = $('#' + paneId + "_SyncPanel");
                                    if (ajaxPanel.length) {
                                        //remove action menus from DOM bbefore fresh pane.
                                        var handler = this;
                                        //pane.find('div.DnnModule').each(function () {
                                        //    var moduleId = handler._moduleManager._findModuleId($(this));
                                        //    $('#moduleActions-' + moduleId).remove();
                                        //});

                                        //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._refreshCompleteHandler);
                                        //this._refreshPaneId = paneId;
                                        //this._refreshCallback = callback;
                                        var args = {};
                                        window.setTimeout(function () { __doPostBack(ajaxPanel.attr('id'), args); }, 100);
                                    } else {
                                        location.reload(true);
                                    }
                                     --%>
                                    location.reload(true);
                                    return;
                                }
                                self.templateDefined = true;
                                self.settingsNeeded = data.SettingsNeeded;
                                self.settingsDefined = data.SettingsDefined;
                                self.dataNeeded = data.DataNeeded;
                                self.dataDefined = data.DataDefined;
                                self.Template = data.Template;
                                if (self.newTemplate) {
                                    self.apiGet('GetTemplates', { advanced: this.advanced }, function (dataTemplates) {
                                        self.templates = dataTemplates;
                                        self.Template = data.Template;
                                    });
                                    self.UseTemplate = "0";
                                }
                            });
                        self.noTemplates = false;
                    },
                    settings: function () {
                        this.settingsDefined = true;
                    },
                    apiGet: function (action, data, success) {
                        var self = this;
                        self.loading = true;
                        $.ajax({
                            type: "GET",
                            url: sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            data: data,
                            beforeSend: sf.setModuleHeaders
                        }).done(function (data) {
                            if (success) success(data);
                            self.loading = false;
                        }).fail(function (xhr, result, status) {
                            if (fail) fail(xhr, result, status);
                            else console.error("Uh-oh, something broke: " + status);
                            self.loading = false;
                        });
                    },
                    apiPost: function (action, data, success) {
                        var self = this;
                        self.loading = true;
                        $.ajax({
                            type: "POST",
                            url: sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            data: JSON.stringify(data),
                            beforeSend: sf.setModuleHeaders
                        }).done(function (data) {
                            if (success) success(data);
                            self.loading = false;
                        }).fail(function (xhr, result, status) {
                            if (fail) fail(xhr, result, status);
                            else console.error("Uh-oh, something broke: " + status);
                            self.loading = false;
                        });
                    }
                },
                components: {
                    modal: {
                        template: '<div v-show="showModal" class="oc-modal"><div class="oc-modal-content"><span class="close">&times;</span><slot></slot></div></div>',
                        data: function () {
                            return {
                                showModal: true
                            };
                        },
                        methods: {
                            show: function () {
                                this.showModal = true;
                            }
                        }
                    },
                }
            })
        });
    }(jQuery, window.Sys));
</script>

</asp:Panel>
