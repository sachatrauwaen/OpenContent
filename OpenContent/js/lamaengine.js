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
    var createEdit = config.isNew ? "create" : "edit";
    self.view = "dnn-" + createEdit;
    if (config.bootstrap) {
        self.view = config.horizontal ? "dnnbootstrap-" + createEdit + "-horizontal" : "dnnbootstrap-" + createEdit;
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

        $("#" + self.deleteButton).dnnConfirm({
            callbackTrue: function () {
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
                    if (popup.length > 0 && windowTop.WebForm_GetElementById('dnn_ctr' + self.moduleId + '_View__UP')) {
                        setTimeout(function () { windowTop.__doPostBack('dnn_ctr' + self.moduleId + '_View__UP', ''); }, 1);
                        dnnModal.closePopUp(false, "");
                    }
                    else {
                        window.location.href = href;
                    }
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });
                return false;
            }
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
    };

    self.FormEdit = function (config) {
        //var ConnectorClass = Alpaca.getConnectorClass("default");
        //var connector = new ConnectorClass("default");
        //connector.servicesFramework = self.sf;
        //connector.culture = self.currentCulture;
        //connector.defaultCulture = self.defaultCulture;
        //connector.numberDecimalSeparator = self.numberDecimalSeparator;
        //connector.rootUrl = self.rootUrl;
        if (config.versions) {
            $.each(config.versions, function (i, item) {
                $("#" + self.ddlVersions).append($('<option>', {
                    value: item.ticks,
                    text: item.text
                }));

            });
        } else {
            $("#" + self.ddlVersions).hide();
        }

        //$.alpaca.setDefaultLocale(self.alpacaCulture);

        //var connector = Lama.getConnectorClass("default");
        self.CreateForm(self.connector, config, config.data);


    };

    self.CreateForm = function (connector, config, data) {

        var view = self.view;
        if (config.view) {
            view = config.view;
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

        var app = Lama.mount("#field1", {
            "schema": config.schema,
            "options": config.options,
            "data": data,
            "view": view,
            "connector": connector,
            "init": Lama.isEmpty(data)
        });
        $("#" + self.saveButton).click(function () {
            var button = this;
            app.validate(function () {

                var value = app.getValue();
                //alert(JSON.stringify(value, null, "  "));
                var href = $(button).attr('href');
                $(document).trigger("beforeSubmit.opencontent", [value, self.moduleId, self.data.id, self.sf, self.editAction]);
                self.FormSubmit(value, href);
                $(document).trigger("afterSubmit.opencontent", [value, self.moduleId, self.data.id, self.sf, self.editAction]);

            });
            return false;
        });

        $("#" + self.ddlVersions).change(function () {
            self.Version(self.itemId, $(this).val(), app);
            return false;
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
            //self.loadSettings();
            //window.location.href = href;
            if (data.isValid) {
                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0 && windowTop.WebForm_GetElementById('dnn_ctr' + self.moduleId + '_View__UP')) {
                    setTimeout(function () { windowTop.__doPostBack('dnn_ctr' + self.moduleId + '_View__UP', ''); }, 1);
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

    self.connector = {
        currentCulture: self.currentCulture,
        connect() {

        },
        loadAll: function (resources, onSuccess) {
            onSuccess();
        },
        getMessage() {

        },
        /*
        * Loads data source (value/text) pairs from a remote source.
        * This default implementation allows for config to be a string identifying a URL.
        */
        loadDataSource: function (config, successCallback, errorCallback) {

            console.log("loadDataSource");
            console.log(config);

            if (config && config.query && config.query) {
                if (config.query.type == "page") {
                    var postData = {
                        q: config.query.search || '*',
                        l: self.currentCulture
                    };
                    $.ajax({
                        
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "TabsLookup",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (data) {
                            if (data) {
                                var pages = [];
                                $.each(data, function (index, value) {
                                    pages.push({
                                        value: value.value,
                                        text: value.text,
                                        url: value.url
                                    });
                                });
                                successCallback(pages);
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            errorCallback({
                                "message": "Unable to load data from uri : ",
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });

                } else if (config.query.type == "relation") {
                    var postData = {
                        "dataKey": config.query.dataKey,
                        "valueField": config.query.valueField,
                        "textField": config.query.textField
                    };
                    $.ajax({
                        //url: self.sf.getServiceRoot(self.options.dataService.module) + self.options.dataService.controller + "/" + self.options.dataService.action,
                        url: self.sf.getServiceRoot("OpenContent") + "OpenContentAPI" + "/" + "LookupData",
                        beforeSend: self.sf.setModuleHeaders,

                        type: "post",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,

                        success: function (data) {
                            if (data) {
                                successCallback(data);
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            errorCallback({
                                "message": "Unable to load data from uri : ",
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else if (config.query.type == "folders") {
                    successCallback([{ id: "1", name: "Files", url: "/Files" }]);
                } else if (config.query.type == "images") {
                    //var files = [{ id: "1", url: "https://agontuk.github.io/assets/images/berserk.jpg", name: "berserk.jpg", folderId: "1" }];

                    var files = [];

                    //var completionFunction = function () {
                    //    self.schema.enum = [];
                    //    self.options.optionLabels = [];
                    //    for (var i = 0; i < self.selectOptions.length; i++) {
                    //        self.schema.enum.push(self.selectOptions[i].value);
                    //        self.options.optionLabels.push(self.selectOptions[i].text);
                    //    }
                    //    // push back to model
                    //    model.selectOptions = self.selectOptions;
                    //    callback();
                    //};

                    var postData = {
                        q: "*",
                        folder: config.query.folder || ""/*self.options.uploadfolder*/,
                        secure: config.query.secure,
                        itemKey: ""/*self.itemKey*/
                    };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookupSecure",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;
                            if (ds) {
                                    $.each(ds, function (index, value) {
                                        files.push({
                                            id: value.id,
                                            url: value.url,
                                            filename: value.filename,
                                            folderId: "1",
                                            thumbUrl: value.thumbUrl,
                                            //"text": value.text,
                                            //"width": value.width,
                                            //"height": value.height,
                                        });
                                    });
                                    //completionFunction();
                                successCallback(files);
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            errorCallback({
                                "message": "Unable to load data from uri : " ,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });

                    //successCallback(files.filter((f) => {
                    //    if (config.query.folder)
                    //        return f.folderId == config.query.folder;
                    //    else
                    //        return false;
                    //}).map(f => {
                    //    return {
                    //        id: f.id,
                    //        filename: f.name,
                    //        url: f.url
                    //    };
                    //}));
                } else if (config.query.type == "files") {
                    //var files = [{ id: "1", url: "https://agontuk.github.io/assets/images/berserk.jpg", name: "berserk.jpg", folderId: "1" }];

                    var files = [];

                    //var completionFunction = function () {
                    //    self.schema.enum = [];
                    //    self.options.optionLabels = [];
                    //    for (var i = 0; i < self.selectOptions.length; i++) {
                    //        self.schema.enum.push(self.selectOptions[i].value);
                    //        self.options.optionLabels.push(self.selectOptions[i].text);
                    //    }
                    //    // push back to model
                    //    model.selectOptions = self.selectOptions;
                    //    callback();
                    //};

                    var postData = {
                        q: "*",
                        folder: config.query.folder ||"" /*self.options.uploadfolder*/,
                        secure: config.query.secure,
                        itemKey: ""/*self.itemKey*/
                    };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FilesLookupSecure",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;
                            if (ds) {
                                $.each(ds, function (index, value) {
                                    files.push({
                                        id: value.id,
                                        url: value.url,
                                        filename: value.filename,
                                        folderId: "1",
                                        thumbUrl: value.thumbUrl,
                                        //"text": value.text,
                                        //"width": value.width,
                                        //"height": value.height,
                                    });
                                });
                                //completionFunction();
                                successCallback(files);
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            errorCallback({
                                "message": "Unable to load data from uri : ",
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });

                    //successCallback(files.filter((f) => {
                    //    if (config.query.folder)
                    //        return f.folderId == config.query.folder;
                    //    else
                    //        return false;
                    //}).map(f => {
                    //    return {
                    //        id: f.id,
                    //        filename: f.name,
                    //        url: f.url
                    //    };
                    //}));
                }
            }
            else {
                errorCallback();
            }
        },
        // eslint-disable-next-line no-unused-vars
        upload(config, successCallback, errorCallback) {
            //debugger;
            var uploadImage = function (config, callbackFn) {
                var formData = new FormData();
                formData.append('file', config.file);
                formData.append('name', config.file.name || config.name);
                //formData.append('width', config.width);
                //formData.append('height', config.height);
                if (typeof config.overwrite !== 'undefined')
                    formData.append('overwrite', config.overwrite);
                if (typeof config.secure !== 'undefined')
                    formData.append('secure', config.secure);

                if (typeof config.folder !== 'undefined')
                    formData.append('uploadfolder', config.folder);

                if (config.hidden) {
                    formData.append('hidden', true);
                    if (typeof config.folder !== 'undefined')
                        formData.append('cropfolder', config.folder);
                }

                var url = self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile";

                $.ajax({
                    type: 'POST',
                    url: url,
                    data: formData,
                    contentType: false,
                    processData: false,
                    beforeSend: self.sf.setModuleHeaders,
                    success: function (response) {
                        callbackFn(response);
                    }
                });
            };
            uploadImage(config, function (statuses) {
                var status = JSON.parse(statuses)[0];
                if (status.success) {
                    successCallback({ id: status.id, url: status.url, filename: status.name });
                } else {
                    errorCallback(status.message);
                    console.log(status.name +" : "+status.message);
                }
            });
        }
    };

};
