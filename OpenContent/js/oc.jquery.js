(function ($) {

    $.openContentRestApiV1 = function (sf) {
        var self = this;
        this.sf = sf;
        this.get = function (entity, id, success, fail) {
            $.ajax({
                type: "GET",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v1/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        this.getAll = function (entity, pageIndex, pageSize, filter, sort, success, fail) {
            $.ajax({
                type: "GET",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v1/" + entity,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: { pageIndex: pageIndex, pageSize: pageSize, filter: JSON.stringify(filter), sort: JSON.stringify(sort) },
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.add = function (entity, item, success, fail) {
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v1/" + entity,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ item: item }),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.update = function (entity, id, item, success, fail) {
            $.ajax({
                type: "PUT",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v1/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ item: item }),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.delete = function (entity, id, callback) {
            $.ajax({
                type: "DELETE",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v1/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: {},
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

    }

    $.openContentRestApiV2 = function (sf) {
        var self = this;
        this.sf = sf;
        this.get = function (entity, id, success, fail) {
            $.ajax({
                type: "GET",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v2/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        this.getAll = function (entity, pageIndex, pageSize, filter, sort, success, fail) {
            $.ajax({
                type: "GET",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v2/" + entity,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: { pageIndex: pageIndex, pageSize: pageSize, filter: JSON.stringify(filter), sort: JSON.stringify(sort) },
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.add = function (entity, item, success, fail) {
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v2/" + entity,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ item: item }),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.update = function (entity, id, item, success, fail) {
            $.ajax({
                type: "PUT",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v2/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ item: item }),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

        self.delete = function (entity, id, callback) {
            $.ajax({
                type: "DELETE",
                url: self.sf.getServiceRoot('OpenContent') + "Rest/v2/" + entity + "/" + id,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: {},
                beforeSend: self.sf.setModuleHeaders
            }).done(function (data) {
                if (success) success(data);
            }).fail(function (xhr, result, status) {
                if (fail) fail(xhr, result, status);
                else console.error("Uh-oh, something broke: " + status);
            });
        }

    }

    var OpenContentForm = function (element, options) {
        var elem = $(element);
        var obj = this;
        var settings = $.extend({
            servicesFramework: null,
            form: "form",
            action: "FormSubmit",
            onRendered: null,
            onSubmit: null,
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
                if (field) {
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
                data: JSON.stringify({ id: id, form: data, action: settings.action }),
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

    var OpenContentEditForm = function (element, options) {
        var elem = $(element);
        var obj = this;
        var settings = $.extend({
            servicesFramework: null,
            form: "",
            id: "",
            onSubmited: null
        }, options || {});
        // Public method - can be called from client code
        this.submit = function () {
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
                        //value.id = settings.id;
                        formSubmit(settings.id, value);
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
                if (field) {
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
            elem.removeData('opencontenteditform');
        };

        this.show = function (id) {
            settings.id = id;
            if (elem.alpaca("exists")) {
                elem.alpaca("destroy");
            }
            showForm();
        };
        // Private method - can only be called from within this object
        var showForm = function () {
            var sf = settings.servicesFramework;
            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/Edit",
                data: settings.id ? "id=" + settings.id : "",
                beforeSend: sf.setModuleHeaders
            }).done(function (config) {
                $.alpaca.setDefaultLocale(config.context.alpacaCulture);
                if (config.context.bootstrap && $.fn.select2) {
                    $.fn.select2.defaults.set("theme", "bootstrap");
                }
                var ConnectorClass = Alpaca.getConnectorClass("default");
                connector = new ConnectorClass("default");
                connector.servicesFramework = sf;
                connector.culture = config.context.currentCulture;
                connector.defaultCulture = config.context.defaultCulture;
                connector.numberDecimalSeparator = config.context.numberDecimalSeparator;
                connector.rootUrl = config.context.rootUrl;
                var view = config.view;
                if (view) {
                    view.parent = config.context.horizontal ? "dnnbootstrap-edit-horizontal" : "dnnbootstrap-edit";
                } else {
                    view = config.context.horizontal ? "dnnbootstrap-edit-horizontal" : "dnnbootstrap-edit";
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
                            settings.onRendered(selfControl, config.data);
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
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/Update",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ id: id, form: data }),
                beforeSend: sf.setModuleHeaders
            }).done(function () {
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
                if (settings.onSubmited) {
                    settings.onSubmited(data);
                }
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };
        if (settings.id) {
            showForm();
        }
    };
    $.fn.openContentEditForm = function (options) {
        var element = $(this);
        // Return early if this element already has a plugin instance
        if (element.data('opencontenteditform')) return element.data('opencontenteditform');
        // pass options to plugin constructor
        var opencontenteditform = new OpenContentEditForm(this, options);
        // Store plugin object in this element's data
        element.data('opencontenteditform', opencontenteditform);
        return opencontenteditform;
    };
}(jQuery));