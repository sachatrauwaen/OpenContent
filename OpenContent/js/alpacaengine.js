if (typeof alpacaEngine === 'undefined' || alpacaEngine === null) {
    alpacaEngine = {};
};

alpacaEngine.engine = function (config) {
    var self = this;
    self.defaultCulture = config.defaultCulture;
    self.currentCulture = config.currentCulture;
    self.numberDecimalSeparator = config.numberDecimalSeparator;
    self.alpacaCulture = config.alpacaCulture;
    self.moduleId = config.moduleId;
    self.itemId = config.itemId;
    self.cancelButton = config.cancelButtonID;
    self.saveButton = config.saveButtonID;
    self.deleteButton = config.deleteButtonID;
    self.copyButton = config.copyButtonID;
    self.scopeWrapper = config.scopeWrapperID;
    self.ddlVersions = config.versionsID;
    self.editAction = "Edit";
    self.updateAction = "Update";
    self.deleteAction = "Delete";
    self.actionAction = "Action";
    self.data = {};
    self.rootUrl = config.appPath;
    self.bootstrap = config.bootstrap;    
    var createEdit = self.itemId ? "edit" : "create";
    self.view = "dnn-"+createEdit;
    if (config.bootstrap) {
        self.view = config.horizontal ? "dnnbootstrap-"+createEdit+"-horizontal" : "dnnbootstrap-"+createEdit;
    }
    if (config.bootstrap && $.fn.select2) {
        $.fn.select2.defaults.set("theme", "bootstrap");
    }
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

            newHeight = $window.height() - 36;
            newWidth = Math.min($window.width() - 40, 1200);

            popup.dialog("option", {
                close: function () { window.dnnModal.closePopUp(false, ""); },
                //'position': 'top',
                height: newHeight,
                width: newWidth,
                minWidth: newWidth,
                minHeight: newHeight,
                //position: 'center'
                resizable: false,
            });

            $("div.alpaca").parent().addClass('popup');


            $("#" + self.cancelButton).click(function () {
                dnnModal.closePopUp(false, "");
                return false;
            });
        }
        if (!self.itemId) {
            $("#" + self.deleteButton).hide();
            $("#" + self.copyButton).hide();
        }

        $("#" + self.deleteButton).click(function () {

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

                var href = $("#" + self.saveButton).attr('href');
                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0) {
                    windowTop.__doPostBack('dnn_ctr' + self.moduleId + '_View__UP', '');
                    //dnnModal.closePopUp(false, href);
                    dnnModal.closePopUp(false, "");
                }
                else {
                    window.location.href = href;
                }
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
            return false;
        });


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
        connector.rootUrl = self.rootUrl;
        if (config.versions) {
            $.each(config.versions, function (i, item) {
                $("#" + self.ddlVersions).append($('<option>', {
                    value: item.ticks,
                    text: item.text
                }));
                //$("#<%=ddlVersions.ClientID%>").data(item.CreatedOnDate, item.Json);
            });
        } else {
            $("#" + self.ddlVersions).hide();
        }

        $.alpaca.setDefaultLocale(self.alpacaCulture);
        self.CreateForm(connector, config, config.data);

    };

    self.CreateForm = function (connector, config, data) {

        var view = self.view;
        if (config.view) {
            view.parent = self.view;
        }
        var selfControl = null;
        if (config.options && config.options.form) {
            if (config.options.form.buttons) {
                var $actions = $('.dnnActions');
                var buttons = config.options.form.buttons;
                for (var i in buttons) {
                    $actions.append('<li><a id="action-' + i + '" data-action="' + i + '" data-after="' + buttons[i].after + '" class="dnnSecondaryAction">' + (buttons[i].title ? buttons[i].title : i) + '</a></li>');
                    $('#action-' + i, $actions).click(function () {
                        var action = $(this).attr('data-action');
                        var after = $(this).attr('data-after');
                        var button = this;
                        selfControl.refreshValidationState(true, function () {
                            if (selfControl.isValid(true)) {
                                var value = selfControl.getValue();
                                //alert(JSON.stringify(value, null, "  "));
                                var href = $(button).attr('href');
                                self.FormAction(value, action, selfControl, button, buttons[i].after);
                            }
                        });
                        return false;
                        
                    });
                }
            }
            delete config.options.form;
        }

        $("#field1").alpaca({
            "schema": config.schema,
            "options": config.options,
            "data": data,
            "view": view,
            "connector": connector,
            "postRender": function (control) {
                selfControl = control;
                $("#" + self.saveButton).click(function () {
                    var button = this;
                    selfControl.refreshValidationState(true, function () {
                        if (selfControl.isValid(true)) {
                            var value = selfControl.getValue();
                            //alert(JSON.stringify(value, null, "  "));
                            var href = $(button).attr('href');
                            self.FormSubmit(value, href);
                        }
                    });
                    return false;
                });
                $("#" + self.copyButton).click(function () {
                    var button = this;
                    selfControl.refreshValidationState(true, function () {
                        if (selfControl.isValid(true)) {
                            var value = selfControl.getValue();
                            var href = $(button).attr('href');
                            self.FormSubmit(value, href, true);
                        }
                    });
                    return false;
                });

                $("#" + self.ddlVersions).change(function () {
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

    self.FormSubmit = function (data, href, copy) {
        var postData = $.extend({ form: data }, self.data);
        //var postData = JSON.stringify({ form: data, id: self.itemId });
        //var action = "Update"; //self.getUpdateAction();
        if (copy) {
            delete postData.id;
        }

        $.ajax({
            type: "POST",
            url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + self.updateAction,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(postData),
            beforeSend: self.sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            //self.loadSettings();
            //window.location.href = href;
            if (data.isValid) {
                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0) {
                    windowTop.__doPostBack('dnn_ctr' + self.moduleId + '_View__UP', '');
                    dnnModal.closePopUp(false, href);
                }
                else {
                    window.location.href = href;
                }
                $('#field1validation').hide();
            } else {
                $('#field1validation').text(data.validMessage);
                $('#field1validation').show();
            }

        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    };

    self.FormAction = function (data, action, alpacaControl, button, after) {
        var postData = $.extend({ form: data }, self.data);
        postData.action = action;
        $.ajax({
            type: "POST",
            url: self.sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + self.actionAction,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(postData),
            beforeSend: self.sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            //self.loadSettings();
            //window.location.href = href;
            if (data.isValid) {
                if (data.result) {
                    //alpacaControl.setValue(data.result);
                }
                if (after == "disable") {
                    $(button).off();
                    $(button).addClass('alpaca-disabled');
                }
                $('#field1validation').hide();
            } else {
                $('#field1validation').text(data.validMessage);
                $('#field1validation').show();
            }
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    };

    self.Version = function (id, ticks, control) {
        if (!id) id = "";
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
