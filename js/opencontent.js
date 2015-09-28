/*globals jQuery, window, Sys */
(function ($, Sys) {
    /*
    var myString = $(this).closest("div[class*='DnnModule-']").attr('class');
    var myRegexp = /DnnModule-(\d+)/g;
    var match = myRegexp.exec(myString);
    alert(match[1]);  // abc
    */

    $(document).ready(function () {
        $(document).trigger("opencontent.ready", document);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) {
            var sName = args.get_response().get_webRequest().get_userContext();
            var div = $("#" + sName);
            $(document).trigger("opencontent.change", div);
        });

        Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function (sender, args) {
            var sName = args.get_postBackElement().id;
            args.get_request().set_userContext(args.get_postBackElement().id);
        });
    });

}(jQuery, window.Sys));

function openContent($, settings) {
        
    var $rootElement;
    var dialog;
    //var config;    
    var sf;
    var self = this;
    self.saveCallback = function(){
    
    };
    self.itemId = -1;

    var init = function (element) {
        $rootElement = $(element);

        var config = {
            settings: settings,
            $rootElement: $rootElement
        };

        dialog = $rootElement.dialog({
            title:"Open Content",
            autoOpen: false,
            //height: 600,
            //width: 960,
            dialogClass: "dnnFormPopup ocFormPopup",
            //position: { my: "center", at: "center" },
            //minWidth: width,
            //minHeight: height,
            maxWidth: 1920,
            maxHeight: 1080,
            resizable: false,
            closeOnEscape: true,
            modal: true,
            width: Math.min(window.innerWidth-100, 1170),
            height : window.innerHeight-50,
            //$(popup).css('height', windowTop.innerHeight + 100).dialog('option', );
            position: 'center',
            /*
            buttons: {
                Save: function () {
                    self.saveCallback();
                    dialog.dialog("close");
                },
                Cancel: function () {

                    dialog.dialog("close");
                }
            },
            */
            close: function () {
                $(".oc-form", $rootElement).alpaca("destroy");

            }
        });
        $(window).resize(function () {
            dialog.dialog("option", "width", Math.min(window.innerWidth - 100, 1170));
            dialog.dialog("option", "height", window.innerHeight - 50);
            dialog.dialog("option", "position", { my: "center", at: "center", of: window });
        });

        sf = $.ServicesFramework(settings.moduleId);
        $(".oc-btn-cancel", $rootElement).off().click(function () {
            dialog.dialog("close");
        });
        $(".oc-btn-delete", $rootElement).off().click(function () {

            var postData = JSON.stringify({ id: self.itemId });
            var action = "Delete";
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                dnnModal.closePopUp(false, "");
                var href = $(".oc-btn-save", $rootElement).attr('href');
                __doPostBack('dnn_ctr'+settings.moduleId+'_View__UP', '');
                dialog.dialog("close");
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });


            return false;
        });
           
    }
    var open = function (itemId) {
        self.itemId = itemId
        if (!itemId) {
            $(".oc-btn-delete", $rootElement).hide();
        } else {
            $(".oc-btn-delete", $rootElement).show();
        }
        var postData = {};
        var getData = "";
        var action = "Edit";
        if (itemId) getData = "id=" + itemId;
        $.ajax({
            type: "GET",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            data: getData,
            beforeSend: sf.setModuleHeaders
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
        dialog.dialog("open");
    }

   
    self.FormEdit = function (config) {
        var ConnectorClass = Alpaca.getConnectorClass("default");
        connector = new ConnectorClass("default");
        connector.servicesFramework = sf;
        connector.culture = settings.culture;
        connector.numberDecimalSeparator = settings.numberDecimalSeparator;
        if (config.versions) {
            $(".oc-ddl-versions option", $rootElement).remove();
            $.each(config.versions, function (i, item) {
                $(".oc-ddl-versions", $rootElement).append($('<option>', {
                    value: item.ticks,
                    text: item.text
                }));
            });
        } else {
            $(".oc-ddl-versions", $rootElement).hide();
        }

        $.alpaca.setDefaultLocale(connector.culture.replace('-', '_'));
        self.CreateForm(connector, config, config.data);

    };

    self.CreateForm = function (connector, config, data) {

        $(".oc-form", $rootElement).alpaca({
            "schema": config.schema,
            "options": config.options,
            "data": data,
            "view": "dnn-edit",
            "connector": connector,
            "postRender": function (control) {
                var selfControl = control;
                $(".oc-btn-save", $rootElement).off().click(function () {
                    selfControl.refreshValidationState(true);
                    if (selfControl.isValid(true)) {
                        var value = selfControl.getValue();
                        //alert(JSON.stringify(value, null, "  "));
                        var href = $(this).attr('href');
                        self.FormSubmit(value, href);
                    }
                    return false;
                });
                $(".oc-ddl-versions", $rootElement).off().change(function () {

                    self.Version(self.itemId, $(this).val(), control);
                    return false;
                });
            }
        });

    };

    self.FormSubmit = function (data, href) {
        //var postData = { form: data };
        var postData = JSON.stringify({ form: data, id: self.itemId });
        var action = "Update"; 

        $.ajax({
            type: "POST",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: postData,
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            __doPostBack('dnn_ctr' + settings.moduleId + '_View__UP', '');
            dialog.dialog("close");
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
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            //contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: postData,
            beforeSend: sf.setModuleHeaders
        }).done(function (data) {
            //alert('ok:' + data);
            control.setValue(data);
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
    };

    return {
        init: init,
        open: open
    }

};