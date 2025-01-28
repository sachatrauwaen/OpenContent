<template>
    <div id="app">
        <div class="oc-view oc-select-template" v-if="step==1" v-cloak>
            <div class="oc-tabs">
                <div class="oc-tab-wrap oc-tab-basic">
                    <a class="oc-tab-link" href="#" @click.prevent="goBasic" :class="{advanced:true, active: tab==1 }">{{Resource("lAddNewContent")}}</a>
                </div>
                <div class="oc-tab-wrap oc-tab-shared">
                    <a class="oc-tab-link" href="#" @click.prevent="goShared" :class="{advanced:true, active: tab==2 }">{{Resource("lUseExistingContent")}}</a>
                </div>
                <div class="oc-tab-wrap oc-tab-new-template">
                    <a class="oc-tab-link" href="#" @click.prevent="goCreate" :class="{advanced:true, active: tab==3 }">{{Resource("CreateNewTemplate")}}</a>
                </div>
            </div>

            <div class="oc-tab-content oc-tab-basic-content" v-if="tab==1">
                <div class="oc-templates" v-if="!basicAll">
                    <!--<p style="text-align:left">Choose a template</p>-->
                    <div class="oc-template" v-for="(val) in templates.slice(0, 23)" :key="val.Value">
                        <a class="oc-template-link" :value="val.Value" @click.prevent="selectTemplate(val.Value)" href="#" :title="val.Text" @mouseover="tooltip = val.Value" @mouseleave="tooltip = null">
                            {{val.Text}} <span class="oc-template-info-icon" v-if="val.ImageUrl || val.Description">?</span>
                        </a>
                        <div class="oc-template-info" v-if="tooltip == val.Value && (val.ImageUrl || val.Description)">
                            <div class="oc-template-info-img" v-if="val.ImageUrl"><img style="max-width: 100%; height: auto;" :src="val.ImageUrl" /></div>
                            <div class="oc-template-info-text" v-if="val.Description">{{val.Description}}</div>
                        </div>
                    </div>
                    <div class="oc-template" v-if="templates.length > 2">
                        <a class="oc-template-link advanced" @click.prevent="goBasicAll" href="#">
                            <div>{{Resource("More")}} </div>
                        </a>
                    </div>
                </div>
                <div v-else class="dnnForm" style="max-width:600px;">
                    <fieldset>
                        <div class="dnnFormItem">
                            <label class="dnnLabel">{{ Resource("lTemplate") }}</label>
                            <select v-model="Template" @change="templateChange">
                                <option v-for="template in templates" :key="template.Value" :value="template.Value">{{template.Text}}</option>
                            </select>
                            <span v-if="currentTemplate && (currentTemplate.ImageUrl || currentTemplate.Description)" style="margin-left:5px;background-color:#555555;padding:10px;line-height:16px;font-size:14px;color:#fff;border-radius:3px;" @mouseover="tooltip = Template" @mouseleave="tooltip = null">?</span>
                            <div v-if="currentTemplate && tooltip == currentTemplate.Value && (currentTemplate.ImageUrl || currentTemplate.Description)" style="position: absolute; right: 10px; top: 40px; z-index: 999; width: 200px;  background-color: #3D3C3C; padding: 0.3em 0.5em;border:1px solid #eee;">
                                <div v-if="currentTemplate.ImageUrl" style="background-color:#fff;"><img style="max-width: 100%; height: auto;" :src="currentTemplate.ImageUrl" /></div>
                                <div v-if="currentTemplate.Description">{{currentTemplate.Description}}</div>
                            </div>
                        </div>
                    </fieldset>
                    <ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
                        <li>
                            <a href="" @click.prevent="save" class="dnnPrimaryAction" :disabled="loading">{{Resource("Save")}}</a>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="oc-tab-content oc-tab-shared-content" v-if="tab==2">
                <div class="dnnForm" style="max-width:600px;">
                    <fieldset v-if="tabs && tabs.length">
                        <div class="dnnFormItem">
                            <label class="dnnLabel">{{ Resource("lTab") }}</label>
                            <select v-model="currentTab" @change="tabChange">
                                <option v-for="tab in tabs" :key="tab.TabId" :value="tab">{{tab.Text}}</option>
                                <option v-if="!moreTabs" :value="{TabId:0,Text:Resource('More')}">{{Resource("More")}}</option>
                            </select>
                        </div>
                        <div v-if="currentTab" class="dnnFormItem">
                            <label class="dnnLabel">{{ Resource("lModule") }}</label>
                            <select v-model="tabModuleId" @change="moduleChange">
                                <option v-for="mod in modules" :key="mod.TabModuleId" :value="mod.TabModuleId">{{mod.Text}}</option>
                            </select>
                        </div>
                        <div v-if="false" class="dnnFormItem" style="padding-left: 32%; margin-left: 38px; width: auto;">
                            <label runat="server" />
                        </div>
                        <!--<div v-if="currentTab && tabModuleId" class="dnnFormItem">
                            <label class="dnnLabel">{{ Resource("lTemplate") }}</label>
                            <select v-model="Template" @change="templateChange">
                                <option v-for="template in templates" :key="template.Value" :value="template.Value">{{template.Text}}</option>
                            </select>
                        </div>-->
                        <div v-if="Template && detailPages.length" class="dnnFormItem">
                            <label class="dnnLabel">{{ Resource("lDetailPage") }}</label>
                            <select v-model="detailPage">
                                <option v-for="page in detailPages" :key="page.TabId" :value="page.TabId">{{page.Text}}</option>
                            </select>
                        </div>

                        <div v-if="currentTab && tabModuleId" class="oc-templates">
                            <div class="oc-template" v-for="(val) in templates" :key="val.Value">

                                <a class="oc-template-link" :value="val.Value" @click.prevent="selectTemplate(val.Value)" href="#" :title="val.Text" @mouseover="tooltip = val.Value" @mouseleave="tooltip = null">
                                    {{val.Text}} <span class="oc-template-info-icon" v-if="val.ImageUrl || val.Description">?</span>
                                </a>
                                <div class="oc-template-info" v-if="tooltip == val.Value && (val.ImageUrl || val.Description)">
                                    <div class="oc-template-info-img" v-if="val.ImageUrl"><img style="max-width: 100%; height: auto;" :src="val.ImageUrl" /></div>
                                    <div class="oc-template-info-text" v-if="val.Description">{{val.Description}}</div>
                                </div>
                            </div>
                        </div>

                    </fieldset>
                    <div v-else>
                        No modules exists
                    </div>
                    <!--<ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
                        <li>
                            <a href="" @click.prevent="save" class="dnnPrimaryAction" :disabled="loading">{{newTemplate ? Resource("Create") : Resource("Save")}}</a>
                        </li>
                    </ul>-->
                </div>
            </div>
            <!--<div class="oc-template">
                <a href="#" @click.prevent="goAdvanced" class="advanced">{{Resource("Advanced")}}</a>
            </div>-->
            <div style="clear:both"></div>

            <div class="oc-tab-content oc-tab-new-template-content dnnForm" v-if="tab==3">
                <fieldset>
                    <div class="dnnFormItem">
                        <label class="dnnLabel">{{ Resource("lFrom") }}</label>
                        <table class="dnnFormRadioButtons">
                            <tr>
                                <td>
                                    <input type="radio" v-model="from" value="0" @change="fromSiteChange" :disabled="noTemplates" /><label>{{ Resource("liFromSite")}}</label>
                                </td>
                                <td>
                                    <input type="radio" v-model="from" value="1" @change="fromWebChange" /><label>{{ Resource("liFromWeb")}}</label>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="dnnFormItem">
                        <label class="dnnLabel">{{ Resource("lTemplate") }}</label>
                        <select v-model="Template" @change="templateChange">
                            <option v-for="template in templates" :key="template.Value" :value="template.Value">{{template.Text}}</option>
                        </select>
                        <span v-if="currentTemplate && (currentTemplate.ImageUrl || currentTemplate.Description)" style="margin-left:5px;background-color:#555555;padding:10px;line-height:16px;font-size:14px;color:#fff;border-radius:3px;" @mouseover="tooltip = Template" @mouseleave="tooltip = null">?</span>
                        <div v-if="currentTemplate && tooltip == currentTemplate.Value && (currentTemplate.ImageUrl || currentTemplate.Description)" style="position: absolute; right: 10px; top: 40px; z-index: 999; width: 200px;  background-color: #3D3C3C; padding: 0.3em 0.5em;border:1px solid #eee;">
                            <div v-if="currentTemplate.ImageUrl" style="background-color:#fff;"><img style="max-width: 100%; height: auto;" :src="currentTemplate.ImageUrl" /></div>
                            <div v-if="currentTemplate.Description" style="color:#fff">{{currentTemplate.Description}}</div>
                        </div>
                    </div>
                    <div class="dnnFormItem">
                        <label class="dnnLabel">{{ Resource("lTemplateName") }}</label>
                        <input v-model="templateName" type="text" runat="server" />
                    </div>
                </fieldset>
                <ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
                    <li>
                        <a href="" @click.prevent="save" class="dnnPrimaryAction" :disabled="loading">{{Resource("Create")}}</a>
                    </li>
                </ul>
            </div>
        </div>
        <div v-else-if="step==2" v-cloak>
            <fieldset>
                <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel">{{ Resource("lUseContent") }}</label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :key="mod.TabModuleId" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
                <div class="dnnFormItem">
                    <label class="dnnLabel">{{ Resource("lTemplate") }}</label>
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
                <!--
                <div v-if="otherModule" class="dnnFormItem">
                    <label class="dnnLabel">{{ Resource("lUseContent") }}</label>
                    <select v-model="tabModuleId" disabled>
                        <option v-for="mod in modules" :key="mod.TabModuleId" :value="mod.TabModuleId">{{mod.Text}}</option>
                    </select>
                </div>
                    -->
                <div class="dnnFormItem" @click="templateDefined=false">
                    <label class="dnnLabel">{{ Resource("lTemplate") }}</label>
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
        <p style="text-align:center" v-if="noTemplates">{{ Resource("CreateHelp") }}</p>
        <p v-if="message" style="color:#ff0000" v-cloak>{{message}}</p>
        <div v-if="loading" style="background-color:rgba(255, 255, 255, 0.70);color:#0094ff;text-align:center;position:absolute;width:100%;height:100%;top:0;left:0;padding-top:50%;text-align:center;font-size:20px;">Loading...</div>
    </div>
</template>

<script>
    //import HelloWorld from './components/HelloWorld.vue'


    //export default {
    //  name: "App",
    //  data() {
    //    return {
    //    };
    //  },
    //  components: {
    //  },
    //    };

    /*global $ */

    export default {

        data() {
            return {
                templateDefined: false,
                settingsNeeded: false,
                settingsDefined: false,
                dataNeeded: false,
                dataDefined: false,
                //UseContent: '0',
                UseOtherContent: false,

                Template: "",
                from: "0",
                createTemplate: false,
                newTemplate: false,
                shared: false,
                moreTabs: false,
                basicAll: false,
                templates: [],
                currentTab: null,
                tabModuleId: 0,
                portalId: 0,
                portals: [],

                tabs: [],
                modules: [],
                detailPages: [],
                detailPage: -1,
                templateName: '',

                message: '',
                loading: false,
                noTemplates: false,
                tooltip: null
            };
        },
        computed: {
            currentTemplate() {
                if (this.Template)
                    return this.templates.filter((t) => { return t.Value == this.Template })[0];
                else
                    return null;
            },
            config() {
                return this.$root.$data.config;
            },
            sf() {
                return this.config.sf;
            },
            settingsUrl() {
                return this.config.settingsUrl;
            },
            editUrl() {
                return this.config.editUrl;
            },
            isPageAdmin() {
                return this.config.IsPageAdmin;
            },
            thisModule: function () {
                return this.UseContent == '0';
            },
            otherModule: function () {
                return this.UseContent == '1';
            },
            UseContent() {
                return this.UseOtherContent ? "1" : "0";
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
            tab: function () {
                if (this.createTemplate)
                    return 3;
                else if (this.shared)
                    return 2;
                else
                    return 1
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
            this.apiGet('GetTemplates', { advanced: false }, function (data) {
                self.templates = data;
                self.loading = false;
                if (self.templates.length == 0) {
                    self.noTemplates = true;
                    self.newTemplate = true;
                    self.createTemplate = true;
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
            templates: function (val/*, oldval*/) {
                if (val.length) {
                    if (!this.Template) {
                        this.Template = val[0].Value;
                        this.templateChange();
                    }
                }
            },
            portals: function (val/*, oldval*/) {
                if (val.length) {
                    this.PortalId = val[0].PortalId;
                    this.portalChange();
                }
            },
            modules: function (val/*, oldval*/) {
                if (val.length) {
                    this.tabModuleId = val[0].TabModuleId;
                    this.moduleChange();
                }
            },
            detailPages: function (val/*, oldval*/) {
                if (val.length) {
                    this.detailPage = val[0].TabId;
                }
            }
        },
        methods: {
            Resource(key) {
                return this.config.resources[key];
            },
            goBasic: function () {
                this.basicAll = false;
                this.createTemplate = false;
                this.shared = false;
                this.UseOtherContent = false;
                this.newTemplate = false;
                this.existingTemplateChange();
            },
            goBasicAll: function () {
                this.basicAll = true;
                this.existingTemplateChange();
            },
            goCreate: function () {
                this.createTemplate = true;
                this.shared = false;
                this.UseOtherContent = false;
                this.currentTab = null;
                this.tabs = [];
                this.newTemplate = true;
                //this.existingTemplateChange();
                this.newTemplateChange();
            },
            goShared() {
                this.shared = true;
                this.createTemplate = false;
                this.UseOtherContent = true;
                this.currentTab = null;
                this.tabs = [];
                this.moreTabs = false;
                this.newTemplate = false;
                this.sharedTemplateChange();

            },
            thisModuleChange: function () {
                this.tabModuleId = 0;
                this.Template = '';
                var self = this;
                this.apiGet('GetTemplates', { advanced: this.basicAll }, function (data) {
                    self.templates = data;
                });
            },
            /*
            otherModuleChange: function () {
                this.tabModuleId = 0;
                this.Template = '';
                var self = this;
                //this.apiGet('GetPortals', {}, function (data) {
                //    self.portals = data;
                //});
                //this.apiGet('GetModules', {shared: this.shared}, function (data) {
                //    self.modules = data;
                //});
                if (self.tabs.length == 0) {
                    this.apiGet('GetTabs', { shared: this.shared }, function (data) {
                        self.tabs = data;
                    });
                }
            },
            */
            /*
            useModuleChange() {
                if (this.UseOtherContent)
                    this.otherModuleChange();
                else
                    this.thisModuleChange();

            },
            */
            tabChange: function () {
                var self = this;
                if (this.currentTab.TabId == 0) {
                    this.apiGet('GetTabs', { shared: false }, function (data) {
                        self.tabs = data;
                        self.moreTabs = true;
                        self.currentTab = null;
                    });
                }
                else {
                    this.modules = this.currentTab.Modules;
                }
                this.Template = '';
            },
            moduleChange: function () {
                var self = this;
                this.Template = '';
                this.apiGet('GetTemplates', { tabModuleId: this.tabModuleId }, function (data) {
                    self.templates = data;
                });
            },
            existingTemplateChange: function () {

                this.thisModuleChange();
            },
            sharedTemplateChange: function () {
                this.UseOtherContent = true;
                this.tabModuleId = 0;
                this.Template = '';
                var self = this;
                //this.apiGet('GetPortals', {}, function (data) {
                //    self.portals = data;
                //});
                this.apiGet('GetTabs', { shared: true }, function (data) {
                    self.tabs = data;
                    if (self.tabs.length == 0) {
                        self.apiGet('GetTabs', { shared: false }, function (data) {
                            self.tabs = data;
                            self.moreTabs = true;
                        });
                    }
                });
                //this.apiGet('GetModules', { shared: this.shared }, function (data) {
                //    self.modules = data;
                //});
            },
            newTemplateChange: function () {
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
                        self.apiGet('GetTemplates', { advanced: true }, function (dataTemplates) {
                            self.templates = dataTemplates;
                            self.Template = data.Template;
                        });
                    }
                });
                self.noTemplates = false;
            },
            settings: function () {
                this.settingsDefined = true;
            },
            apiGet: function (action, data, success, fail) {
                var self = this;
                self.loading = true;
                console.log(data);
                $.ajax({
                    type: "GET",
                    url: this.sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: data,
                    beforeSend: this.sf.setModuleHeaders
                }).done(function (data) {
                    self.loading = false;
                    if (success) success(data);                    
                }).fail(function (xhr, result, status) {
                    self.loading = false;
                    if (fail) fail(xhr, result, status);
                    else console.error("Uh-oh, something broke: " + status);
                });
            },
            apiPost: function (action, data, success, fail) {
                var self = this;
                self.loading = true;
                $.ajax({
                    type: "POST",
                    url: this.sf.getServiceRoot('OpenContent') + "InitAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify(data),
                    beforeSend: this.sf.setModuleHeaders
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

        }
    }

</script>

<style>
</style>
