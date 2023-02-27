<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Swagger" CodeBehind="Swagger.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/object-assign-pollyfill.js"  Priority="101" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/jquery.slideto.min.js"  Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/jquery.wiggle.min.js"  Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/jquery.ba-bbq.min.js"  Priority="104" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/handlebars-2.0.0.js"  Priority="105" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/js-yaml.min.js"  Priority="106" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/lodash.min.js"  Priority="107" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/backbone-min.js"  Priority="108" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/swagger-ui.min.js"  Priority="109" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/highlight.9.1.0.pack.js"  Priority="110" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/highlight.9.1.0.pack_extended.js"  Priority="111" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/jsoneditor.min.js"  Priority="112" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/marked.js"  Priority="113" />
<dnn:DnnJsInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/lib/swagger-oauth.js"  Priority="114" />


<dnn:DnnCssInclude runat="server" FilePath="~/Desktopmodules/OpenContent/js/swagger/css/screen.css" />

<asp:Panel ID="ScopeWrapper" runat="server">
</asp:Panel>

<script type="text/javascript">
    $(document).ready(function () {
        var url = "/Desktopmodules/OpenContent/api/Swagger/Json?moduleid=<%=ModuleId%>&tabid=<%=TabId%>";

        hljs.configure({
            highlightSizeThreshold: 5000
        });

        // Pre load translate...
        if (window.SwaggerTranslator) {
            window.SwaggerTranslator.translate();
        }
        window.swaggerUi = new SwaggerUi({
            url: url,
            dom_id: "swagger-ui-container",
            supportedSubmitMethods: ['get', 'post', 'put', 'delete', 'patch'],
            onComplete: function (swaggerApi, swaggerUi) {

                //if (typeof initOAuth == "function") {
                //    initOAuth({
                //        clientId: "your-client-id",
                //        clientSecret: "your-client-secret-if-required",
                //        realm: "your-realms",
                //        appName: "your-app-name",
                //        scopeSeparator: ",",
                //        additionalQueryStringParams: {}
                //    });
                //}

                if (window.SwaggerTranslator) {
                    window.SwaggerTranslator.translate();
                }
            },
            onFailure: function (data) {
                log("Unable to Load SwaggerUI");
            },
            docExpansion: "none",
            jsonEditor: false,
            defaultModelRendering: 'schema',
            showRequestHeaders: true,
            authorizations: {
                TabId: new SwaggerClient.ApiKeyAuthorization("TabId", "<%=TabId%>", "header"),
                ModuleID: new SwaggerClient.ApiKeyAuthorization("ModuleID", "<%=ModuleId%>", "header")
            }
        });
        window.swaggerUi.load();

        function log() {
            if ('console' in window) {
                console.log.apply(console, arguments);
            }
        }
    });
</script>


<div class="swagger-section">
    <div id="message-bar" class="swagger-ui-wrap" data-sw-translate>&nbsp;</div>
    <div id="swagger-ui-container" class="swagger-ui-wrap"></div>
    <div class="swagger-ui-wrap">
        <a href="/Desktopmodules/OpenContent/api/Swagger/Json?moduleid=<%=ModuleId%>&tabid=<%=TabId%>" target="_blank">Api definition in Swagger Json</a>
    </div>
</div>


