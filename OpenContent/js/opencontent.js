/*globals jQuery, window, Sys */
(function ($, Sys) {
    var OpenContent = function () {
        return {
            version: { major: 2, minor: 1, patch: 1 }
        };
    }
    $.fn.openContent = function (options) {
        return OpenContent();
    };
    $.fn.openContent.printLogs = function (title, logs) {
        if (window.console) {
            console.group(title);
            for (var i in logs) {
                console.group(i);
                for (var j = 0; j < logs[i].length; j++) {
                    console.log(logs[i][j].label, logs[i][j].message);
                }
                console.groupEnd();
            }
            console.groupEnd();
        }
    }
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

(function ($) {
    var OpenContentForm = function (element, options) {
        var elem = $(element);
        var obj = this;
        var settings = $.extend({
            servicesFramework: null,
            form: "form",
            onSubmited: null
        }, options || {});
        // Public method - can be called from client code
        this.submit = function (id) {
            if (elem.alpaca("exists")) {
                var control = elem.alpaca("get");
                control.refreshValidationState(true, function () {
                    var recaptcha = typeof (grecaptcha) != "undefined";
                    if (recaptcha) {
                        var recap = grecaptcha.getResponse();
                    }
                    if (control.isValid(true) && (!recaptcha || recap.length > 0)) {
                        var value = control.getValue();
                        if (recaptcha) {
                            value.recaptcha = recap;
                        }
                        //$(this).prop('disabled', true);
                        formSubmit(id, value);
                        //$(document).trigger("postSubmitForm.openform", [value, moduleId, sf]);
                    }
                });
            }
        };
        this.setData = function (data) {
            if (elem.alpaca("exists")) {
                var control = elem.alpaca("get");
                control.setValue(data);
            }
        };
        this.setField = function (fieldPath, data) {
            if (elem.alpaca("exists")) {
                var control = elem.alpaca("get");
                var field = control.getControlByPath(fieldPath);
                if (field){
                    field.setValue(data);
                }
            }
        };
        this.getField = function (fieldPath) {
            if (elem.alpaca("exists")) {
                var control = elem.alpaca("get");
                return control.getControlByPath(fieldPath);
            }
        };
        this.destroy = function () {
            if (elem.alpaca("exists")) {
                elem.alpaca("destroy");
            }
        };
        this.show = function () {
            elem.show();
            elem.parent().find('.ResultMessage').remove();
        };
        // Private method - can only be called from within this object
        var showForm = function () {
            //$.alpaca.setDefaultLocale("AlpacaCulture");
            var sf = settings.servicesFramework;
            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('OpenContent') + "FormAPI/Form",
                data: "key=" + settings.form,
                beforeSend: sf.setModuleHeaders
            }).done(function (config) {
                var ConnectorClass = Alpaca.getConnectorClass("default");
                connector = new ConnectorClass("default");
                connector.servicesFramework = sf;
                var view = config.view;
                if (view) {
                    view.parent = "bootstrap-create";
                } else {
                    view = "bootstrap-create";
                }
                elem.alpaca({
                    "schema": config.schema,
                    "options": config.options,
                    "data": config.data,
                    "view": view,
                    "connector": connector,
                    "postRender": function (control) {
                        var selfControl = control;
                        if (settings.onRendered) {
                            settings.onRendered(selfControl);
                        }
                        //$(document).trigger("postRenderForm.opencontent", [control, moduleId, sf]);
                    }
                });
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };
        var formSubmit = function (id, data) {
            if (settings.onSubmit) {
                settings.onSubmit(data);
            }
            var sf = settings.servicesFramework;
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "FormAPI/Submit",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ id: id, form : data }),
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                if (data.Errors && data.Errors.length > 0) {
                    console.log(data.Errors);
                }
                if (data.Tracking) {
                    
                }
                if (typeof ga === 'function') {
                    ga('send', 'event', {
                        eventCategory: 'Form',
                        eventAction: 'Submit',
                        eventLabel: window.location.href
                    });
                }
                elem.hide();
                elem.parent().append("<div class='ResultMessage'>" + data.Message + "</div>");
                if (settings.onSubmited) {
                    settings.onSubmited(data);
                }
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };
        showForm();
    };
    $.fn.openContentForm = function (options) {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('opencontentform')) return element.data('opencontentform');
        // pass options to plugin constructor
        var opencontentform = new OpenContentForm(this, options);
        // Store plugin object in this element's data
        element.data('opencontentform', opencontentform);
        return opencontentform;
    };
})(jQuery);

