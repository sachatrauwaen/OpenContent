if (typeof alpacaEngine === 'undefined' || alpacaEngine === null) {
    alpacaEngine = {};
};

alpacaEngine.engine = function(config) {
    var self = this;
    self.defaultCulture = config.defaultCulture;
    self.currentCulture = config.currentCulture;
    self.NumberDecimalSeparator = config.NumberDecimalSeparator;
    self.AlpacaCulture = config.AlpacaCulture;
    self.moduleId = config.moduleId;
    self.itemId = config.itemId;
    self.cancelButton = config.cancelButtonID;
    self.saveButton = config.saveButtonID;
    self.deleteButton = config.deleteButtonID;
    self.scopeWrapper = config.scopeWrapperID;
    self.ddlVersions = config.versionsID;
    self.editAction = "Edit";
    self.updateAction = "Update";
    self.deleteAction = "Delete";
    self.data = {};

    
    if (config.editAction) {
        self.editAction = config.editAction;
    }
    if (config.updateAction) {
        self.updateAction = config.updateAction;
    }
    if (config.deleteAction) {
        self.deleteAction = config.deleteAction;
    }
    if (config.data) {
        self.data = config.data;
    }
    if (config.itemId) {
        self.data.id = config.itemId;
    }
    self.init = function () {
        var windowTop = parent;
        var popup = windowTop.jQuery("#iPopUp");
        if (popup.length) {

            var $window = $(windowTop),
                            newHeight,
                            newWidth;

            newHeight = $window.height() - 46;
            newWidth = Math.min($window.width() - 40, 1100);

            popup.dialog("option", {
                close: function () { window.dnnModal.closePopUp(false, ""); },
                //'position': 'top',
                height: newHeight,
                width: newWidth,
                //position: 'center'
            });

            $("#"+self.cancelButton).click(function () {
                dnnModal.closePopUp(false, "");
                return false;
            });

            if (!self.itemId) {
                $("#"+self.deleteButton).hide();
            }

            $("#"+self.deleteButton).click(function () {

                var postData = JSON.stringify({ id: self.itemId });
                //var action = "Delete";
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + self.deleteAction,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: postData,
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (data) {
                    dnnModal.closePopUp(false, "");
                    var href = $("#"+self.saveButton).attr('href');
                    var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                    var popup = windowTop.jQuery("#iPopUp");
                    if (popup.length > 0) {
                        windowTop.__doPostBack('dnn_ctr'+self.moduleId+'_View__UP', '');
                        dnnModal.closePopUp(false, href);
                    }
                    else {
                        window.location.href = href;
                    }
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });
                return false;
            });
        }

        //var moduleScope = $('#'+self.scopeWrapper),
            //self = moduleScope,
        //sf = $.ServicesFramework(self.moduleId);

        self.sf = $.ServicesFramework(self.moduleId);

        var postData = {};
        var getData = "";
        //var action = "Edit";
        if (self.itemId) getData = "id=" + self.itemId;
        
        $.ajax({
            type: "GET",
            url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + self.editAction,
            data: self.data,
            beforeSend: self.sf.setModuleHeaders
        }).done(function (config) {

            /*
            oc_loadmodules(config.options, function () {
                self.FormEdit(config);
            });
            */
            self.FormEdit(config);

        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    }
   
    self.FormEdit = function (config) {
        var ConnectorClass = Alpaca.getConnectorClass("default");
        var connector = new ConnectorClass("default");
        connector.servicesFramework = self.sf;
        connector.culture = self.currentCulture;
        connector.defaultCulture = self.defaultCulture;
        connector.numberDecimalSeparator = self.numberDecimalSeparator;
        if (config.versions) {
            $.each(config.versions, function (i, item) {
                $("#"+self.ddlVersions).append($('<option>', {
                    value: item.ticks,
                    text: item.text
                }));
                //$("#<%=ddlVersions.ClientID%>").data(item.CreatedOnDate, item.Json);
            });
        } else {
            $("#"+self.ddlVersions).hide();
        }

        $.alpaca.setDefaultLocale(self.AlpacaCulture);
        self.CreateForm(connector, config, config.data);

    };

    self.CreateForm = function (connector, config, data) {

        $("#field1").alpaca({
            "schema": config.schema,
            "options": config.options,
            "data": data,
            "view": "dnn-edit",
            "connector": connector,
            "postRender": function (control) {
                var selfControl = control;
                $("#"+self.saveButton).click(function () {
                    selfControl.refreshValidationState(true);
                    if (selfControl.isValid(true)) {
                        var value = selfControl.getValue();
                        //alert(JSON.stringify(value, null, "  "));
                        var href = $(this).attr('href');
                        self.FormSubmit(value, href);
                    }
                    return false;
                });

                $("#"+self.ddlVersions).change(function () {
                    //var versions = config.versions;
                    //var ver = $("#<%=ddlVersions.ClientID%>").data($(this).val());
                    //$("#field1").empty();
                    //$("#<%=cmdSave.ClientID%>").off("click");
                    //self.CreateForm(connector, config, ver);
                    //selfControl.setValue(ver);
                    self.Version(self.itemId, $(this).val(), control);
                    return false;
                });

            }
        });

    };

    self.FormSubmit = function (data, href) {
        var postData = JSON.stringify($.extend({ form: data }, self.data));
        //var postData = JSON.stringify({ form: data, id: self.itemId });
        //var action = "Update"; //self.getUpdateAction();

        $.ajax({
            type: "POST",
            url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + self.updateAction,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: postData,
            beforeSend: self.sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            //self.loadSettings();
            //window.location.href = href;

            var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
            var popup = windowTop.jQuery("#iPopUp");
            if (popup.length > 0) {
                windowTop.__doPostBack('dnn_ctr'+self.moduleId+'_View__UP', '');
                dnnModal.closePopUp(false, href);
            }
            else {
                window.location.href = href;
            }
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    };

    self.Version = function (id, ticks, control) {
        if (!id) id = 0;
        var postData = { id: id, ticks: ticks };
        var action = "Version";

        $.ajax({
            type: "GET",
            url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: postData,
            beforeSend: self.sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            control.setValue(data);
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    };

};
