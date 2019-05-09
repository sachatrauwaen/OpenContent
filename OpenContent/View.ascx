<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.View" CodeBehind="View.ascx.cs" %>
<%@ Register Src="~/DesktopModules/OpenContent/TemplateInit.ascx" TagPrefix="uc1" TagName="TemplateInit" %>
<uc1:TemplateInit runat="server" ID="TemplateInitControl" />

<asp:Panel ID="pInit" runat="server" Visible="false">
<asp:Panel ID="pVue" runat="server" >
    <div v-if="step==1">
        <div v-if ="!advanced ">
            <p style="text-align:center">Choose a template</p>
            <a v-for="(val, index) in templates" v-if="index < 11" :value="val.Value" @click.prevent="selectTemplate(val.Value)" href="#" class="template">{{val.Text}}</a>
            <a href="#" @click.prevent="advanced=true" class="template advanced" >More...</a>
                <div style="clear:both"></div>
        </div>
        <div v-else class="dnnForm">
            <fieldset>
                <div v-if="advanced" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <table class="dnnFormRadioButtons">
                        <tr>
                            <td>
                                <input type="radio" v-model="UseContent" @input="thisModuleChange" value="0" class="dnnRadiobutton" /><label><%= Resource("liThisModule") %></label></td>
                            <td >
                                <input type="radio" v-model="UseContent" @input="otherModuleChange" value="1" class="dnnRadiobutton" /><label><%=Resource("liOtherModule")%></label></td>
                        </tr>
                    </table>
                </div>
                <div v-if="UseContent=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lDataSource") %></label>
                    <select v-model="tabModuleId" @change="moduleChange">
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
                <div v-if="advanced" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseTemplate") %></label>
                    <table class="dnnFormRadioButtons">
                        <tr>
                            <td>
                                <input type="radio" v-model="UseTemplate" value="0" @input="existingTemplateChange" /><label><%=Resource("liUseExistingTemplate")%></label></td>
                            <td :class="{dnnDisabled:otherModule}">
                                <input type="radio" v-model="UseTemplate" value="1" @input="newTemplateChange" :disabled="otherModule"/><%=Resource("liCreateNewTemplate")%></td>
                        </tr>
                    </table>
                </div>
                <div v-if="UseTemplate=='1'" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lFrom") %></label>
                    <table class="dnnFormRadioButtons">
                        <tr>
                            <td>
                                <input type="radio" v-model="from" value="0"  @input="fromSiteChange" /><%= Resource("liFromSite")%></td>
                            <td>
                                <input type="radio" v-model="from" value="1" @input="fromWebChange" /><%= Resource("liFromWeb")%></td>
                        </tr>
                    </table>
                </div>
                <div class="dnnFormItem" style="padding-left: 32%; margin-left: 38px; width: auto;">
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
                    <input type="text" runat="server" />
                </div>
                <div v-if="advanced && existingTemplate && Template" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lDetailPage") %></label>
                    <select v-model="detailPage">
                        <option v-for="page in detailPages" :value="page.TabId">{{page.Text}}</option>
                    </select>
                </div>
            </fieldset>
            <ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
                <li>
                    <a href="" @click.prevent="save" class="dnnPrimaryAction"><%= Resource("Save") %></a>
                </li>
                <li>
                    <a href="#" @click.prevent="advanced=!advanced" class="dnnSecondaryAction" >{{advanced ? 'Basic' : 'Advanced'}}</a>
                </li>
            </ul>
        </div>
    </div>
    <div v-else-if="step==2">
        <fieldset>
             <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
            <div class="dnnFormItem">
                <label class="dnnLabel"><%= Resource("lTemplate") %></label>
                <select v-model="Template" disabled>
                    <option v-for="template in templates" :value="template.Value">{{template.Text}}</option>
                </select>
                <a @click.prevent="templateDefined=false" href="#" class="dnnSecondaryAction">Change</a>
            </div>
            <div class="dnnFormItem">
                <label class="dnnLabel">Settings</label>
                <a :href="settingsUrl" class="dnnPrimaryAction">Template Settings</a> <span></span>
            </div>
        </fieldset>
    </div>
    <div v-else-if="step==3">
        <fieldset>
            <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel"><%= Resource("lUseContent") %></label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
            <div class="dnnFormItem" @click="templateDefined=false">
                <label class="dnnLabel"><%= Resource("lTemplate") %></label>
                <select v-model="Template" disabled>
                    <option v-for="template in templates" :value="template.Value">{{template.Text}}</option>
                </select>
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
</asp:Panel>
<script src="https://unpkg.com/vue"></script>
<script>
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
                modules: [],
                detailPages: [],
                detailPage: -1,
                settingsUrl:"<%= ModuleContext.EditUrl("EditSettings") %>",
                editUrl:"<%= ModuleContext.EditUrl("Edit") %>"
            },
            computed: {
                existingTemplate: function () {
                    return this.UseTemplate == '0';
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
                }
            },
            mounted: function () {
                var self = this;
                this.apiGet('GetTemplateState', {}, function (data) {
                    self.templateDefined = data.Template;
                    self.Template = data.Template;
                    self.settingsNeeded = data.SettingsNeeded;
                    self.settingsDefined = data.SettingsDefined;
                    self.dataNeeded = data.DataNeeded;
                    self.dataDefined = data.DataDefined;
                });
                this.apiGet('GetTemplates', {}, function (data) {
                    self.templates = data;
                });
            },
            methods: {
                thisModuleChange: function () {
                    var self = this;
                    this.apiGet('GetTemplates', {}, function (data) {
                        self.templates = data;
                    });
                },
                otherModuleChange: function () {
                    this.UseTemplate = "0";
                    this.tabModuleId = 0;
                    var self = this;
                    this.apiGet('GetModules', {}, function (data) {
                        self.modules = data;
                    });
                },
                moduleChange: function () {
                    var self = this;
                    this.apiGet('GetTemplates', { tabModuleId: this.tabModuleId }, function (data) {
                        self.templates = data;
                    });
                },
                existingTemplateChange: function () {
                    this.thisModuleChange();
                },
                newTemplateChange: function () {
                    this.from = "0";
                    this.fromSiteChange();
                },
                fromWebChange: function () {
                    var self = this;
                    this.apiGet('GetTemplates', { web: true }, function (data) {
                        self.templates = data;
                    });
                },
                fromSiteChange: function () {
                    var self = this;
                    this.apiGet('GetTemplates', { web: false }, function (data) {
                        self.templates = data;
                    });
                },
                templateChange: function () {
                    var self = this;
                    this.apiGet('GetDetailPages', { template: this.Template, tabModuleId: this.tabModuleId }, function (data) {
                        self.detailPages = data;
                    });
                },
                selectTemplate: function (template) {
                    this.Template = template;
                    this.templateChange();
                    this.save();
                },
                save: function () {
                    var self = this;
                    this.apiPost('SaveTemplate', {
                        template: self.Template,
                        otherModule: self.otherModule,
                        tabModuleId: self.tabModuleId,
                        newTemplate: false,
                        fromWeb: false,
                        templateName: ''
                    }, function (data) {
                        self.templateDefined = true;
                        self.settingsNeeded = data.SettingsNeeded;
                        self.settingsDefined = data.SettingsDefined;
                        self.dataNeeded = data.DataNeeded;
                        self.dataDefined = data.DataDefined;
                    });
                },
                settings: function () {
                    this.settingsDefined = true;
                },
                apiGet: function (action, data, success) {
                    $.ajax({
                        type: "GET",
                        url: sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: data,
                        beforeSend: sf.setModuleHeaders
                    }).done(function (data) {
                        if (success) success(data);
                    }).fail(function (xhr, result, status) {
                        if (fail) fail(xhr, result, status);
                        else console.error("Uh-oh, something broke: " + status);
                    });
                },
                apiPost: function (action, data, success) {
                    $.ajax({
                        type: "POST",
                        url: sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify(data),
                        beforeSend: sf.setModuleHeaders
                    }).done(function (data) {
                        if (success) success(data);
                    }).fail(function (xhr, result, status) {
                        if (fail) fail(xhr, result, status);
                        else console.error("Uh-oh, something broke: " + status);
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
</script>

</asp:Panel>