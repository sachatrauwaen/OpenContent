/*
function Load() {
    //var sch = JSON.parse($("#schema").val());
    //var opts = JSON.parse($("#options").val());
    var data = getBuilder(sch, opts);
    return data;
}

function getBuilder(schema, options) {
    var data = [];
    for (var prop in schema.properties) {
        var sch = schema.properties[prop];
        var opt = false;
        if (options && options.fields) {
            opt = options.fields[prop]
        }

        var d = {
            "fieldname": prop,
            "fieldtype": opt && opt.type ? opt.type : "text",
            "showtitle": sch.title ? true : false,
            "title": sch.title,
            "showhelp": opt ? (opt.helper ? true : false) : false,
            "helper": opt && opt.helper ? opt.helper : "",
            "showplaceholder": opt ? (opt.placeholder ? true : false) : false,
            "placeholder": opt && opt.placeholder ? opt.placeholder : "",
            "required": sch.required ? true : false,
            "vertical": opt ? opt.vertical : false,
            "fieldoptions": [],
            "subfields": []
        };
        if (d.fieldtype.substr(0, 2) == "ml") {
            d.fieldtype = d.fieldtype.substr(2, d.fieldtype.length - 2);
            d.multilanguage = true;
        }
        else {
            d.multilanguage = false;
        }
        if (sch.enum) {
            if (d.fieldtype == "text") {
                d.fieldtype = "select";
            }
            for (var i = 0; i < sch.enum.length; i++) {
                d.fieldoptions.push({
                    "value": sch.enum[i],
                    "text": opt && opt.optionLabels ? opt.optionLabels[i] : sch.enum[i]
                });
            }
        };
        if (sch.type == "boolean") {
            d.fieldtype = "checkbox";
        }
        if (sch.type == "array") {
            if (d.fieldtype == "checkbox") {
                d.fieldtype = "multicheckbox";
            } else if (d.fieldtype == "text") {
                d.fieldtype = "array";
            }
            var b = getBuilder(sch.items, opt && opt.items ? opt.items : null);
            d.subfields = b;
        }
        data.push(d);
    }
    return data;
}
*/
function getSchema(formdef) {
    var sch = null;
    var schema = {
        //"title": "Form preview",
        "type": "object",
        "properties": {}
    };
    var properties = schema.properties;
    if (formdef.formtype == "array") {
        schema = {
            //"title": "Form preview",
            "type": "array",
            "items": {
                "type": "object",
                //"title": "Category",
                "properties": {}
            }
        };
        properties = schema.items.properties;
    }
    var schematypes = {
        "text": "string",
        "number": "number",
        "select": "string",
        "radio": "string",
        "checkbox": "boolean",
        "email": "string",
        "textarea": "string",
        "array": "array",
        "table": "array",
        "accordion": "array",
        "multicheckbox": "array",
        "file": "string",
        "url": "string",
        "image": "string",
        "imagex": "string",
        "icon": "string",
        "guid": "string",
        "wysihtml": "string",
        "summernote": "string",
        "ckeditor": "string",
        "address": "object",
        "relation": "string",
        "related": "string",
        "gallery": "array",
        "documents": "array",
        "object": "object",
        "folder2": "string",
        "file2": "string",
        "url2": "string",
        "role2": "string",
        "image2": "string",
        "imagecrop": "string"
    };

    var baseProps = function (index, value, oldSchema) {
        var prop = {
            "type": schematypes[value.fieldtype]
        };
        if (value.fieldtype == "publishstatus") {
            prop.enum = ["draft", "published"];
            prop.required = true;
            prop.default = "draft";
        } else if (value.fieldtype == "publishstartdate") {
            prop.required = true;
            prop.default = "today";
        } else if (value.fieldtype == "publishenddate") {
            prop.required = true;
            prop.default = "2099-12-31";
        }
        if (value.title) {
            prop.title = value.title;
        }
        if (value.fieldtype == "relation" && value.relationoptions && value.relationoptions.many) {
            prop.type = "array";
        }
        if (value.fieldtype == "related" && value.relatedoptions && value.relatedoptions.many) {
            prop.type = "array";
        }
        if (value.fieldtype == "role2" && value.many) {
            prop.type = "array";
        }
        if (value.fieldoptions) {

            prop.enum = $.map(value.fieldoptions, function (v, i) {
                return v.value;
            });

            if (value.required && prop.enum && prop.enum.length > 0) {
                prop.default = prop.enum[0];
            }
        }
        if (value.required) {
            prop.required = value.required;
        }
        if (value.default) {
            prop.default = value.default;
        }
        if (value.dependencies && value.dependencies.length > 0) {
            var deps = [];
            for (var i = 0; i < value.dependencies.length; i++) {
                deps.push(value.dependencies[i].fieldname);
            }
            prop.dependencies = deps;
        }
        if (value.fieldtype == "array" || value.fieldtype == "table" || value.fieldtype == "accordion") {

            if (!value.subfields) {
                prop.items = {};
                /*
                } else if (value.fieldtype == "array" && value.subfields.length == 1) {
                    var listOldSchema = oldSchema && oldSchema.items ? oldSchema.items : null;
                    var listindex = 0;
                    var listvalue = value.subfields[0];
                    var listprop = baseProps(listindex, listvalue, listOldSchema);
                    if (!listvalue.fieldname) {
                        listvalue.fieldname = 'field_' + listindex;
                    }
                    prop.items = listprop;
                */
            } else {
                prop.items = {
                    //"title": "Field",
                    "type": "object",
                    "properties": {
                    }
                };
                $.each(value.subfields, function (listindex, listvalue) {
                    var listOldSchema = oldSchema && oldSchema.items && oldSchema.items.properties ? oldSchema.items.properties[listvalue.fieldname] : null;
                    var listprop = baseProps(listindex, listvalue, listOldSchema);
                    if (!listvalue.fieldname) {
                        listvalue.fieldname = 'field_' + listindex;
                    }
                    prop.items.properties[listvalue.fieldname] = listprop;
                });
            }
        }
        if (value.fieldtype == "object") {
            delete value.type;
            if (!value.subfields) {
                prop.properties = {};
            } else {
                prop.properties = {
                };
                $.each(value.subfields, function (objectindex, objectvalue) {
                    var objectOldSchema = oldSchema && oldSchema.properties ? oldSchema.properties[objectvalue.fieldname] : null;
                    var objectprop = baseProps(objectindex, objectvalue, objectOldSchema);
                    if (!objectvalue.fieldname) {
                        objectvalue.fieldname = 'field_' + listindex;
                    }
                    prop.properties[objectvalue.fieldname] = objectprop;
                });
            }
        }
        if (oldSchema) {
            return $.extend(false, {}, oldSchema, prop);
        }
        else {
            return prop;
        }
        //return prop;
    };
    if (formdef.formfields) {
        $.each(formdef.formfields, function (index, value) {
            var oldSchema = null; // sch.properties[value.fieldname];
            var prop = baseProps(index, value, oldSchema);
            if (!value.fieldname) {
                value.fieldname = 'field_' + index;
            }
            properties[value.fieldname] = prop;
        });
    }
    return schema;
}

var baseFields = function (index, value, oldOptions) {
    var field = {
        "type": value.fieldtype
    };


    if (value.multilanguage) {
        field.type = "ml" + field.type;
    }

    if (value.fieldtype == "multicheckbox") {
        field.type = "checkbox";
    } else if (value.fieldtype == "relation") {
        field.type = "select2";
        field.dataService = {
            "action": "LookupData",
            "data": {
                "dataKey": value.relationoptions.datakey,
                "valueField": value.relationoptions.valuefield,
                "textField": value.relationoptions.textfield
            }
        };
    } else if (value.fieldtype == "related") {
        field.type = "select2";
        field.dataService = {
            "action": "Lookup",
            "data": {
                "valueField": "Id",
                "textField": value.relatedoptions.textfield
            }
        };
    } else if (value.fieldtype == "date" && value.dateoptions) {
        field.picker = value.dateoptions;
    } else if (value.fieldtype == "file" && value.fileoptions && value.fileoptions.folder) {
        field.uploadfolder = value.fileoptions.folder;
        field.typeahead = {};
        field.typeahead.Folder = value.fileoptions.folder;
    } else if (value.fieldtype == "file2" && value.file2options) {
        field.folder = value.file2options.folder;
        field.filter = value.file2options.filter;
    } else if (value.fieldtype == "folder2" && value.folder2options) {
        field.folder = value.folder2options.folder;
        field.filter = value.folder2options.filter;
    } else if (value.fieldtype == "image" && value.imageoptions && value.imageoptions.folder) {
        field.uploadfolder = value.imageoptions.folder;
        field.typeahead = {};
        field.typeahead.Folder = value.imageoptions.folder;
    } else if (value.fieldtype == "imagex" && value.imagexoptions) {
        $.extend(field, value.imagexoptions);
    } else if (value.fieldtype == "ckeditor" && value.ckeditoroptions) {
        $.extend(field, value.ckeditoroptions);
    } else if (value.fieldtype == "image2" && value.image2options) {
        field.folder = value.image2options.folder;
        field.filter = value.image2options.filter;
    } else if (value.fieldtype == "imagecrop" && value.imagecropoptions) {
        field.cropper = {};
        field.cropper.aspectRatio = value.imagecropoptions.ratio;
    } else if (value.fieldtype == "icon" && value.iconoptions) {
        field.glyphicons = value.iconoptions.glyphicons;
        field.bootstrap = value.iconoptions.bootstrap;
        field.fontawesome = value.iconoptions.fontawesome;
    } else if (value.fieldtype == "publishstartdate") {
        field.type = "date";
        field.picker = {
            //"format": "DD/MM/YYYY",
            "minDate": "2000-01-01",
            "maxDate": "2099-12-31",
            //"locale": "nl"
        };
    } else if (value.fieldtype == "publishenddate") {
        field.type = "date";
        field.picker = {
            //"format": "DD/MM/YYYY",
            "minDate": "2000-01-01",
            "maxDate": "2099-12-31",
            //"locale": "nl"
        };
    }
    if (value.fieldoptions) {
        field.sort = false;
        field.optionLabels = $.map(value.fieldoptions, function (v, i) {
            return v.text;
        });
    }

    if (value.fieldtype == "radio") {
        field.vertical = value.vertical;
    }
    if (value.hidden) {
        field.hidden = value.hidden;
    }
    if (value.placeholder) {
        field.placeholder = value.placeholder;
    }
    if (value.helper) {
        field.helper = value.helper;
    }
    if (value.dependencies && value.dependencies.length > 0) {
        for (var i = 0; i < value.dependencies.length; i++) {
            if (value.dependencies[i].values) {
                if (!field.dependencies) {
                    field.dependencies = {};
                }
                var values = value.dependencies[i].values.split(",");
                field.dependencies[value.dependencies[i].fieldname] = values;
            }
        }
    }
    if (value.fieldtype == "array" || value.fieldtype == "table" || value.fieldtype == "accordion") {
        //field.toolbarSticky = true;
        if (!value.subfields) {
            field.items = {};
            /*
            } else if (value.fieldtype == "array" && value.subfields.length == 1) {
                var listOldOptions = oldOptions && oldOptions.items ? oldOptions.items : null;
                var listindex = 0;
                var listvalue = value.subfields[0];
                var listfield = baseFields(listindex, listvalue, listOldOptions);
                if (!listvalue.fieldname) {
                    listvalue.fieldname = 'field_' + listindex;
                }
                field.items = listfield;
            */
        } else {

            field.items = {
                //"fieldClass":"listfielddiv",
                "fields": {}
            };
            $.each(value.subfields, function (listindex, listvalue) {
                var listOldOptions = oldOptions && oldOptions.items && oldOptions.items.fields ? oldOptions.items.fields[listvalue.fieldname] : null;
                var listfield = baseFields(listindex, listvalue, listOldOptions);
                if (!listvalue.fieldname) {
                    listvalue.fieldname = 'field_' + listindex;
                }
                field.items.fields[listvalue.fieldname] = listfield;
            });
        }
    }
    if (value.fieldtype == "object") {
        //field.toolbarSticky = true;
        if (!value.subfields) {
            field.fields = {};
        } else {
            field.fields = {
            };
            $.each(value.subfields, function (objectindex, objectvalue) {
                var objectOldOptions = oldOptions && oldOptions.fields ? oldOptions.fields[objectvalue.fieldname] : null;
                var objectfield = baseFields(objectindex, objectvalue, objectOldOptions);
                if (!objectvalue.fieldname) {
                    objectvalue.fieldname = 'field_' + objectindex;
                }
                field.fields[objectvalue.fieldname] = objectfield;
            });
        }
    }
    if (oldOptions) {
        return $.extend(false, {}, oldOptions, field);
    }
    else {
        return field;
    }
    //return field;
};

function getOptions(formdef) {
    var opts = null;
    var options = {
        "fields": {}
    };
    var fields = options.fields;
    if (formdef.formtype == "array") {
        options = {
            "type": "accordion",
            "items": {
                "type": "object",
                "fields": {}
            }
        };
        fields = options.items.fields;
    }
    if (formdef.formfields) {
        $.each(formdef.formfields, function (index, value) {
            var oldOptions = opts && opts.fields ? opts.fields[value.fieldname] : null;

            var field = baseFields(index, value, oldOptions);
            fields[value.fieldname] = field;
        });
    }
    return options;
}

var baseIndexFields = function (index, value, oldOptions) {

    var indextypes = {
        "text": "text",
        "number": "float",
        "select": "key",
        "radio": "key",
        "checkbox": "boolean",
        "email": "text",
        "textarea": "text",
        "array": "array",
        "table": "array",
        "accordion": "array",
        "multicheckbox": "key",
        "file": "file",
        "url": "text",
        "image": "text",
        "imagex": "text",
        "icon": "text",
        "guid": "text",
        "wysihtml": "html",
        "summernote": "html",
        "ckeditor": "html",
        "address": "object",
        "relation": "key",
        "related": "key",
        "gallery": "array",
        "documents": "array",
        "object": "object",
        "folder2": "string",
        "file2": "file",
        "url2": "text",
        "role2": "key",
        "image2": "key",
        "imagecrop": "text",
        "date": "datetime"
    };

    var field = {
        "indexType": indextypes[value.fieldtype]
    };
    if (value.index) {
        field.index = true;
        field.sort = true;
    }
    if (value.multilanguage) {
        field.multilanguage = true;
    }
    if (value.fieldtype == "relation" && value.relationoptions && value.relationoptions.many) {
        if (value.index) {
            field.type = "array";
            field.items = {
                "indexType": "key",
                "index": true,
                "sort": true
            };
        }
    }
    else if (value.fieldtype == "related" && value.relatedoptions && value.relatedoptions.many) {
        if (value.index) {
            field.type = "array";
            field.items = {
                "indexType": "key",
                "index": true,
                "sort": true
            };
        }
    }
    else if (value.fieldtype == "role2" && value.many) {
        if (value.index) {
            field.type = "array";
            field.items = {
                "indexType": "key",
                "index": true,
                "sort": true
            };
        }
    }
    else if (value.fieldtype == "documents") {
        if (value.index) {
            field.type = "array";
            field.items = {
                fields: {
                    "Title": {
                        "index": true,
                        "sort": true,
                        "indexType": "text"
                    },
                    "File": {
                        "indexType": "file",
                        "index": true,
                        "sort": true
                    },
                }
            };
        }
    }
    else if (field.indexType == "array") {
        if (!value.subfields) {
            field.items = {};
        } else {
            field.items = {
                "fields": {}
            };
            $.each(value.subfields, function (listindex, listvalue) {
                var listOldOptions = oldOptions && oldOptions.items && oldOptions.items.fields ? oldOptions.items.fields[listvalue.fieldname] : null;
                var listfield = baseIndexFields(listindex, listvalue, listOldOptions);
                if (!listvalue.fieldname) {
                    listvalue.fieldname = 'field_' + listindex;
                }
                field.items.fields[listvalue.fieldname] = listfield;
                if (listfield.index) field.index = true;
            });
        }
    }
    else if (value.fieldtype == "object") {
        //field.toolbarSticky = true;
        if (!value.subfields) {
            field.fields = {};
        } else {
            field.fields = {
            };
            $.each(value.subfields, function (objectindex, objectvalue) {
                var objectOldOptions = oldOptions && oldOptions.fields ? oldOptions.fields[objectvalue.fieldname] : null;
                var objectfield = baseIndexFields(objectindex, objectvalue, objectOldOptions);
                if (!objectvalue.fieldname) {
                    objectvalue.fieldname = 'field_' + objectindex;
                }
                field.fields[objectvalue.fieldname] = objectfield;
                if (objectvalue.index) field.index = true;
            });
        }
    }
    if (oldOptions) {
        return $.extend(false, {}, oldOptions, field);
    }
    else {
        return field;
    }
};

function getIndex(formdef) {
    if (!Indexable) return "";

    var opts = null;
    var options = {
        "fields": {}
    };
    var fields = options.fields;

    if (formdef.formfields) {
        $.each(formdef.formfields, function (index, value) {
            var oldOptions = opts && opts.fields ? opts.fields[value.fieldname] : null;

            var field = baseIndexFields(index, value, oldOptions);
            if (field.index) options.index = true;
            fields[value.fieldname] = field;
        });
    }
    if (options.index)
        return options;
    else
        return "";
}

function getViewTemplate(row, cols) {
    var t = '<div class="row">';
    for (var i = 1; i <= cols; i++) {
        t += '<div class="col-md-' + (12 / cols) + '" id="pos_' + row + '_' + i + '"></div>';
    }
    t += '</div>';
    return t;
}

function getView(formdef) {

    var view = {
        "parent": BootstrapForm ? (BootstrapHorizontal ? "dnnbootstrap-edit-horizontal" : "dnnbootstrap-edit") : "dnn-edit",
        "layout": {
            "template": "<div><div class='row'><div class='col-md-12' id='pos_1_1'></div></div>",
            "bindings": {
            }
        }
    };

    if (formdef.formtype == "array") {
        return { parent: view.parent };
    }

    if (formdef.formfields) {
        var row = 0;
        var lastCols = 0;
        var template = "<div>";
        $.each(formdef.formfields, function (index, value) {
            var cols = value.position ? parseInt(value.position[0]) : 1;
            if (cols != lastCols) {
                row++;
                template += getViewTemplate(row, cols);
                lastCols = cols;

            }
            var col = value.position ? value.position[4] : 1;
            view.layout.bindings[value.fieldname] = "#pos_" + row + "_" + col;
        });
        template += "</div>";
        view.layout.template = template;
    }
    return view;
}

var ContactForm = false;
var Indexable = false;
var BootstrapForm = false;
var BootstrapHorizontal = false;

function showForm(value) {
    if (ContactForm) {
        fieldSchema.properties.fieldtype.enum.splice(10);
        fieldOptions.fieldtype.optionLabels.splice(10);
    }

    if (!Indexable) {
        delete fieldSchema.properties.index;
    }

    if (BootstrapForm && $.fn.select2) {
        $.fn.select2.defaults.set("theme", "bootstrap");
    }

    var ConnectorClass = Alpaca.getConnectorClass("default");
    var connector = new ConnectorClass("default");
    //connector.servicesFramework = self.sf;
    connector.culture = "en-US";
    connector.defaultCulture = "en-US";
    connector.numberDecimalSeparator = ".";
    connector.rootUrl = "/";

    var schema = getSchema(value);
    var options = getOptions(value);
    var index = getIndex(value);
    console.log(index);
    var view = getView(value);
    var config = {
        "schema": schema,
        "options": options,
        "view": view,
        "connector": connector,
        "postRender": function (control) {
            var self = control;
            $('#form2 .dnnTooltip').dnnTooltip();
        }
    };
    /*
    if (BootstrapForm) {
        config.view = "dnnbootstrap-edit-horizontal";
    }
    */
    var exists = $("#form2").alpaca("exists");
    if (exists) {
        $("#form2").alpaca("destroy");
    }
    config.options.focus = "";
    $("#form2").alpaca(config);
}

var fieldSchema =
{
    //"title": "Field",
    "type": "object",
    "properties": {
        "fieldname": {
            "type": "string",
            "title": "Field name",
            "pattern": "^(?!(?:do|if|in|for|let|new|try|var|case|else|enum|eval|false|null|this|true|void|with|break|catch|class|const|super|throw|while|yield|delete|export|import|public|return|static|switch|typeof|default|extends|finally|package|private|continue|debugger|function|arguments|interface|protected|implements|instanceof)$)[$A-Z\_a-z\xaa\xb5\xba\xc0-\xd6\xd8-\xf6\xf8-\u02c1\u02c6-\u02d1\u02e0-\u02e4\u02ec\u02ee\u0370-\u0374\u0376\u0377\u037a-\u037d\u0386\u0388-\u038a\u038c\u038e-\u03a1\u03a3-\u03f5\u03f7-\u0481\u048a-\u0527\u0531-\u0556\u0559\u0561-\u0587\u05d0-\u05ea\u05f0-\u05f2\u0620-\u064a\u066e\u066f\u0671-\u06d3\u06d5\u06e5\u06e6\u06ee\u06ef\u06fa-\u06fc\u06ff\u0710\u0712-\u072f\u074d-\u07a5\u07b1\u07ca-\u07ea\u07f4\u07f5\u07fa\u0800-\u0815\u081a\u0824\u0828\u0840-\u0858\u08a0\u08a2-\u08ac\u0904-\u0939\u093d\u0950\u0958-\u0961\u0971-\u0977\u0979-\u097f\u0985-\u098c\u098f\u0990\u0993-\u09a8\u09aa-\u09b0\u09b2\u09b6-\u09b9\u09bd\u09ce\u09dc\u09dd\u09df-\u09e1\u09f0\u09f1\u0a05-\u0a0a\u0a0f\u0a10\u0a13-\u0a28\u0a2a-\u0a30\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39\u0a59-\u0a5c\u0a5e\u0a72-\u0a74\u0a85-\u0a8d\u0a8f-\u0a91\u0a93-\u0aa8\u0aaa-\u0ab0\u0ab2\u0ab3\u0ab5-\u0ab9\u0abd\u0ad0\u0ae0\u0ae1\u0b05-\u0b0c\u0b0f\u0b10\u0b13-\u0b28\u0b2a-\u0b30\u0b32\u0b33\u0b35-\u0b39\u0b3d\u0b5c\u0b5d\u0b5f-\u0b61\u0b71\u0b83\u0b85-\u0b8a\u0b8e-\u0b90\u0b92-\u0b95\u0b99\u0b9a\u0b9c\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8-\u0baa\u0bae-\u0bb9\u0bd0\u0c05-\u0c0c\u0c0e-\u0c10\u0c12-\u0c28\u0c2a-\u0c33\u0c35-\u0c39\u0c3d\u0c58\u0c59\u0c60\u0c61\u0c85-\u0c8c\u0c8e-\u0c90\u0c92-\u0ca8\u0caa-\u0cb3\u0cb5-\u0cb9\u0cbd\u0cde\u0ce0\u0ce1\u0cf1\u0cf2\u0d05-\u0d0c\u0d0e-\u0d10\u0d12-\u0d3a\u0d3d\u0d4e\u0d60\u0d61\u0d7a-\u0d7f\u0d85-\u0d96\u0d9a-\u0db1\u0db3-\u0dbb\u0dbd\u0dc0-\u0dc6\u0e01-\u0e30\u0e32\u0e33\u0e40-\u0e46\u0e81\u0e82\u0e84\u0e87\u0e88\u0e8a\u0e8d\u0e94-\u0e97\u0e99-\u0e9f\u0ea1-\u0ea3\u0ea5\u0ea7\u0eaa\u0eab\u0ead-\u0eb0\u0eb2\u0eb3\u0ebd\u0ec0-\u0ec4\u0ec6\u0edc-\u0edf\u0f00\u0f40-\u0f47\u0f49-\u0f6c\u0f88-\u0f8c\u1000-\u102a\u103f\u1050-\u1055\u105a-\u105d\u1061\u1065\u1066\u106e-\u1070\u1075-\u1081\u108e\u10a0-\u10c5\u10c7\u10cd\u10d0-\u10fa\u10fc-\u1248\u124a-\u124d\u1250-\u1256\u1258\u125a-\u125d\u1260-\u1288\u128a-\u128d\u1290-\u12b0\u12b2-\u12b5\u12b8-\u12be\u12c0\u12c2-\u12c5\u12c8-\u12d6\u12d8-\u1310\u1312-\u1315\u1318-\u135a\u1380-\u138f\u13a0-\u13f4\u1401-\u166c\u166f-\u167f\u1681-\u169a\u16a0-\u16ea\u16ee-\u16f0\u1700-\u170c\u170e-\u1711\u1720-\u1731\u1740-\u1751\u1760-\u176c\u176e-\u1770\u1780-\u17b3\u17d7\u17dc\u1820-\u1877\u1880-\u18a8\u18aa\u18b0-\u18f5\u1900-\u191c\u1950-\u196d\u1970-\u1974\u1980-\u19ab\u19c1-\u19c7\u1a00-\u1a16\u1a20-\u1a54\u1aa7\u1b05-\u1b33\u1b45-\u1b4b\u1b83-\u1ba0\u1bae\u1baf\u1bba-\u1be5\u1c00-\u1c23\u1c4d-\u1c4f\u1c5a-\u1c7d\u1ce9-\u1cec\u1cee-\u1cf1\u1cf5\u1cf6\u1d00-\u1dbf\u1e00-\u1f15\u1f18-\u1f1d\u1f20-\u1f45\u1f48-\u1f4d\u1f50-\u1f57\u1f59\u1f5b\u1f5d\u1f5f-\u1f7d\u1f80-\u1fb4\u1fb6-\u1fbc\u1fbe\u1fc2-\u1fc4\u1fc6-\u1fcc\u1fd0-\u1fd3\u1fd6-\u1fdb\u1fe0-\u1fec\u1ff2-\u1ff4\u1ff6-\u1ffc\u2071\u207f\u2090-\u209c\u2102\u2107\u210a-\u2113\u2115\u2119-\u211d\u2124\u2126\u2128\u212a-\u212d\u212f-\u2139\u213c-\u213f\u2145-\u2149\u214e\u2160-\u2188\u2c00-\u2c2e\u2c30-\u2c5e\u2c60-\u2ce4\u2ceb-\u2cee\u2cf2\u2cf3\u2d00-\u2d25\u2d27\u2d2d\u2d30-\u2d67\u2d6f\u2d80-\u2d96\u2da0-\u2da6\u2da8-\u2dae\u2db0-\u2db6\u2db8-\u2dbe\u2dc0-\u2dc6\u2dc8-\u2dce\u2dd0-\u2dd6\u2dd8-\u2dde\u2e2f\u3005-\u3007\u3021-\u3029\u3031-\u3035\u3038-\u303c\u3041-\u3096\u309d-\u309f\u30a1-\u30fa\u30fc-\u30ff\u3105-\u312d\u3131-\u318e\u31a0-\u31ba\u31f0-\u31ff\u3400-\u4db5\u4e00-\u9fcc\ua000-\ua48c\ua4d0-\ua4fd\ua500-\ua60c\ua610-\ua61f\ua62a\ua62b\ua640-\ua66e\ua67f-\ua697\ua6a0-\ua6ef\ua717-\ua71f\ua722-\ua788\ua78b-\ua78e\ua790-\ua793\ua7a0-\ua7aa\ua7f8-\ua801\ua803-\ua805\ua807-\ua80a\ua80c-\ua822\ua840-\ua873\ua882-\ua8b3\ua8f2-\ua8f7\ua8fb\ua90a-\ua925\ua930-\ua946\ua960-\ua97c\ua984-\ua9b2\ua9cf\uaa00-\uaa28\uaa40-\uaa42\uaa44-\uaa4b\uaa60-\uaa76\uaa7a\uaa80-\uaaaf\uaab1\uaab5\uaab6\uaab9-\uaabd\uaac0\uaac2\uaadb-\uaadd\uaae0-\uaaea\uaaf2-\uaaf4\uab01-\uab06\uab09-\uab0e\uab11-\uab16\uab20-\uab26\uab28-\uab2e\uabc0-\uabe2\uac00-\ud7a3\ud7b0-\ud7c6\ud7cb-\ud7fb\uf900-\ufa6d\ufa70-\ufad9\ufb00-\ufb06\ufb13-\ufb17\ufb1d\ufb1f-\ufb28\ufb2a-\ufb36\ufb38-\ufb3c\ufb3e\ufb40\ufb41\ufb43\ufb44\ufb46-\ufbb1\ufbd3-\ufd3d\ufd50-\ufd8f\ufd92-\ufdc7\ufdf0-\ufdfb\ufe70-\ufe74\ufe76-\ufefc\uff21-\uff3a\uff41-\uff5a\uff66-\uffbe\uffc2-\uffc7\uffca-\uffcf\uffd2-\uffd7\uffda-\uffdc][$A-Z\_a-z\xaa\xb5\xba\xc0-\xd6\xd8-\xf6\xf8-\u02c1\u02c6-\u02d1\u02e0-\u02e4\u02ec\u02ee\u0370-\u0374\u0376\u0377\u037a-\u037d\u0386\u0388-\u038a\u038c\u038e-\u03a1\u03a3-\u03f5\u03f7-\u0481\u048a-\u0527\u0531-\u0556\u0559\u0561-\u0587\u05d0-\u05ea\u05f0-\u05f2\u0620-\u064a\u066e\u066f\u0671-\u06d3\u06d5\u06e5\u06e6\u06ee\u06ef\u06fa-\u06fc\u06ff\u0710\u0712-\u072f\u074d-\u07a5\u07b1\u07ca-\u07ea\u07f4\u07f5\u07fa\u0800-\u0815\u081a\u0824\u0828\u0840-\u0858\u08a0\u08a2-\u08ac\u0904-\u0939\u093d\u0950\u0958-\u0961\u0971-\u0977\u0979-\u097f\u0985-\u098c\u098f\u0990\u0993-\u09a8\u09aa-\u09b0\u09b2\u09b6-\u09b9\u09bd\u09ce\u09dc\u09dd\u09df-\u09e1\u09f0\u09f1\u0a05-\u0a0a\u0a0f\u0a10\u0a13-\u0a28\u0a2a-\u0a30\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39\u0a59-\u0a5c\u0a5e\u0a72-\u0a74\u0a85-\u0a8d\u0a8f-\u0a91\u0a93-\u0aa8\u0aaa-\u0ab0\u0ab2\u0ab3\u0ab5-\u0ab9\u0abd\u0ad0\u0ae0\u0ae1\u0b05-\u0b0c\u0b0f\u0b10\u0b13-\u0b28\u0b2a-\u0b30\u0b32\u0b33\u0b35-\u0b39\u0b3d\u0b5c\u0b5d\u0b5f-\u0b61\u0b71\u0b83\u0b85-\u0b8a\u0b8e-\u0b90\u0b92-\u0b95\u0b99\u0b9a\u0b9c\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8-\u0baa\u0bae-\u0bb9\u0bd0\u0c05-\u0c0c\u0c0e-\u0c10\u0c12-\u0c28\u0c2a-\u0c33\u0c35-\u0c39\u0c3d\u0c58\u0c59\u0c60\u0c61\u0c85-\u0c8c\u0c8e-\u0c90\u0c92-\u0ca8\u0caa-\u0cb3\u0cb5-\u0cb9\u0cbd\u0cde\u0ce0\u0ce1\u0cf1\u0cf2\u0d05-\u0d0c\u0d0e-\u0d10\u0d12-\u0d3a\u0d3d\u0d4e\u0d60\u0d61\u0d7a-\u0d7f\u0d85-\u0d96\u0d9a-\u0db1\u0db3-\u0dbb\u0dbd\u0dc0-\u0dc6\u0e01-\u0e30\u0e32\u0e33\u0e40-\u0e46\u0e81\u0e82\u0e84\u0e87\u0e88\u0e8a\u0e8d\u0e94-\u0e97\u0e99-\u0e9f\u0ea1-\u0ea3\u0ea5\u0ea7\u0eaa\u0eab\u0ead-\u0eb0\u0eb2\u0eb3\u0ebd\u0ec0-\u0ec4\u0ec6\u0edc-\u0edf\u0f00\u0f40-\u0f47\u0f49-\u0f6c\u0f88-\u0f8c\u1000-\u102a\u103f\u1050-\u1055\u105a-\u105d\u1061\u1065\u1066\u106e-\u1070\u1075-\u1081\u108e\u10a0-\u10c5\u10c7\u10cd\u10d0-\u10fa\u10fc-\u1248\u124a-\u124d\u1250-\u1256\u1258\u125a-\u125d\u1260-\u1288\u128a-\u128d\u1290-\u12b0\u12b2-\u12b5\u12b8-\u12be\u12c0\u12c2-\u12c5\u12c8-\u12d6\u12d8-\u1310\u1312-\u1315\u1318-\u135a\u1380-\u138f\u13a0-\u13f4\u1401-\u166c\u166f-\u167f\u1681-\u169a\u16a0-\u16ea\u16ee-\u16f0\u1700-\u170c\u170e-\u1711\u1720-\u1731\u1740-\u1751\u1760-\u176c\u176e-\u1770\u1780-\u17b3\u17d7\u17dc\u1820-\u1877\u1880-\u18a8\u18aa\u18b0-\u18f5\u1900-\u191c\u1950-\u196d\u1970-\u1974\u1980-\u19ab\u19c1-\u19c7\u1a00-\u1a16\u1a20-\u1a54\u1aa7\u1b05-\u1b33\u1b45-\u1b4b\u1b83-\u1ba0\u1bae\u1baf\u1bba-\u1be5\u1c00-\u1c23\u1c4d-\u1c4f\u1c5a-\u1c7d\u1ce9-\u1cec\u1cee-\u1cf1\u1cf5\u1cf6\u1d00-\u1dbf\u1e00-\u1f15\u1f18-\u1f1d\u1f20-\u1f45\u1f48-\u1f4d\u1f50-\u1f57\u1f59\u1f5b\u1f5d\u1f5f-\u1f7d\u1f80-\u1fb4\u1fb6-\u1fbc\u1fbe\u1fc2-\u1fc4\u1fc6-\u1fcc\u1fd0-\u1fd3\u1fd6-\u1fdb\u1fe0-\u1fec\u1ff2-\u1ff4\u1ff6-\u1ffc\u2071\u207f\u2090-\u209c\u2102\u2107\u210a-\u2113\u2115\u2119-\u211d\u2124\u2126\u2128\u212a-\u212d\u212f-\u2139\u213c-\u213f\u2145-\u2149\u214e\u2160-\u2188\u2c00-\u2c2e\u2c30-\u2c5e\u2c60-\u2ce4\u2ceb-\u2cee\u2cf2\u2cf3\u2d00-\u2d25\u2d27\u2d2d\u2d30-\u2d67\u2d6f\u2d80-\u2d96\u2da0-\u2da6\u2da8-\u2dae\u2db0-\u2db6\u2db8-\u2dbe\u2dc0-\u2dc6\u2dc8-\u2dce\u2dd0-\u2dd6\u2dd8-\u2dde\u2e2f\u3005-\u3007\u3021-\u3029\u3031-\u3035\u3038-\u303c\u3041-\u3096\u309d-\u309f\u30a1-\u30fa\u30fc-\u30ff\u3105-\u312d\u3131-\u318e\u31a0-\u31ba\u31f0-\u31ff\u3400-\u4db5\u4e00-\u9fcc\ua000-\ua48c\ua4d0-\ua4fd\ua500-\ua60c\ua610-\ua61f\ua62a\ua62b\ua640-\ua66e\ua67f-\ua697\ua6a0-\ua6ef\ua717-\ua71f\ua722-\ua788\ua78b-\ua78e\ua790-\ua793\ua7a0-\ua7aa\ua7f8-\ua801\ua803-\ua805\ua807-\ua80a\ua80c-\ua822\ua840-\ua873\ua882-\ua8b3\ua8f2-\ua8f7\ua8fb\ua90a-\ua925\ua930-\ua946\ua960-\ua97c\ua984-\ua9b2\ua9cf\uaa00-\uaa28\uaa40-\uaa42\uaa44-\uaa4b\uaa60-\uaa76\uaa7a\uaa80-\uaaaf\uaab1\uaab5\uaab6\uaab9-\uaabd\uaac0\uaac2\uaadb-\uaadd\uaae0-\uaaea\uaaf2-\uaaf4\uab01-\uab06\uab09-\uab0e\uab11-\uab16\uab20-\uab26\uab28-\uab2e\uabc0-\uabe2\uac00-\ud7a3\ud7b0-\ud7c6\ud7cb-\ud7fb\uf900-\ufa6d\ufa70-\ufad9\ufb00-\ufb06\ufb13-\ufb17\ufb1d\ufb1f-\ufb28\ufb2a-\ufb36\ufb38-\ufb3c\ufb3e\ufb40\ufb41\ufb43\ufb44\ufb46-\ufbb1\ufbd3-\ufd3d\ufd50-\ufd8f\ufd92-\ufdc7\ufdf0-\ufdfb\ufe70-\ufe74\ufe76-\ufefc\uff21-\uff3a\uff41-\uff5a\uff66-\uffbe\uffc2-\uffc7\uffca-\uffcf\uffd2-\uffd7\uffda-\uffdc0-9\u0300-\u036f\u0483-\u0487\u0591-\u05bd\u05bf\u05c1\u05c2\u05c4\u05c5\u05c7\u0610-\u061a\u064b-\u0669\u0670\u06d6-\u06dc\u06df-\u06e4\u06e7\u06e8\u06ea-\u06ed\u06f0-\u06f9\u0711\u0730-\u074a\u07a6-\u07b0\u07c0-\u07c9\u07eb-\u07f3\u0816-\u0819\u081b-\u0823\u0825-\u0827\u0829-\u082d\u0859-\u085b\u08e4-\u08fe\u0900-\u0903\u093a-\u093c\u093e-\u094f\u0951-\u0957\u0962\u0963\u0966-\u096f\u0981-\u0983\u09bc\u09be-\u09c4\u09c7\u09c8\u09cb-\u09cd\u09d7\u09e2\u09e3\u09e6-\u09ef\u0a01-\u0a03\u0a3c\u0a3e-\u0a42\u0a47\u0a48\u0a4b-\u0a4d\u0a51\u0a66-\u0a71\u0a75\u0a81-\u0a83\u0abc\u0abe-\u0ac5\u0ac7-\u0ac9\u0acb-\u0acd\u0ae2\u0ae3\u0ae6-\u0aef\u0b01-\u0b03\u0b3c\u0b3e-\u0b44\u0b47\u0b48\u0b4b-\u0b4d\u0b56\u0b57\u0b62\u0b63\u0b66-\u0b6f\u0b82\u0bbe-\u0bc2\u0bc6-\u0bc8\u0bca-\u0bcd\u0bd7\u0be6-\u0bef\u0c01-\u0c03\u0c3e-\u0c44\u0c46-\u0c48\u0c4a-\u0c4d\u0c55\u0c56\u0c62\u0c63\u0c66-\u0c6f\u0c82\u0c83\u0cbc\u0cbe-\u0cc4\u0cc6-\u0cc8\u0cca-\u0ccd\u0cd5\u0cd6\u0ce2\u0ce3\u0ce6-\u0cef\u0d02\u0d03\u0d3e-\u0d44\u0d46-\u0d48\u0d4a-\u0d4d\u0d57\u0d62\u0d63\u0d66-\u0d6f\u0d82\u0d83\u0dca\u0dcf-\u0dd4\u0dd6\u0dd8-\u0ddf\u0df2\u0df3\u0e31\u0e34-\u0e3a\u0e47-\u0e4e\u0e50-\u0e59\u0eb1\u0eb4-\u0eb9\u0ebb\u0ebc\u0ec8-\u0ecd\u0ed0-\u0ed9\u0f18\u0f19\u0f20-\u0f29\u0f35\u0f37\u0f39\u0f3e\u0f3f\u0f71-\u0f84\u0f86\u0f87\u0f8d-\u0f97\u0f99-\u0fbc\u0fc6\u102b-\u103e\u1040-\u1049\u1056-\u1059\u105e-\u1060\u1062-\u1064\u1067-\u106d\u1071-\u1074\u1082-\u108d\u108f-\u109d\u135d-\u135f\u1712-\u1714\u1732-\u1734\u1752\u1753\u1772\u1773\u17b4-\u17d3\u17dd\u17e0-\u17e9\u180b-\u180d\u1810-\u1819\u18a9\u1920-\u192b\u1930-\u193b\u1946-\u194f\u19b0-\u19c0\u19c8\u19c9\u19d0-\u19d9\u1a17-\u1a1b\u1a55-\u1a5e\u1a60-\u1a7c\u1a7f-\u1a89\u1a90-\u1a99\u1b00-\u1b04\u1b34-\u1b44\u1b50-\u1b59\u1b6b-\u1b73\u1b80-\u1b82\u1ba1-\u1bad\u1bb0-\u1bb9\u1be6-\u1bf3\u1c24-\u1c37\u1c40-\u1c49\u1c50-\u1c59\u1cd0-\u1cd2\u1cd4-\u1ce8\u1ced\u1cf2-\u1cf4\u1dc0-\u1de6\u1dfc-\u1dff\u200c\u200d\u203f\u2040\u2054\u20d0-\u20dc\u20e1\u20e5-\u20f0\u2cef-\u2cf1\u2d7f\u2de0-\u2dff\u302a-\u302f\u3099\u309a\ua620-\ua629\ua66f\ua674-\ua67d\ua69f\ua6f0\ua6f1\ua802\ua806\ua80b\ua823-\ua827\ua880\ua881\ua8b4-\ua8c4\ua8d0-\ua8d9\ua8e0-\ua8f1\ua900-\ua909\ua926-\ua92d\ua947-\ua953\ua980-\ua983\ua9b3-\ua9c0\ua9d0-\ua9d9\uaa29-\uaa36\uaa43\uaa4c\uaa4d\uaa50-\uaa59\uaa7b\uaab0\uaab2-\uaab4\uaab7\uaab8\uaabe\uaabf\uaac1\uaaeb-\uaaef\uaaf5\uaaf6\uabe3-\uabea\uabec\uabed\uabf0-\uabf9\ufb1e\ufe00-\ufe0f\ufe20-\ufe26\ufe33\ufe34\ufe4d-\ufe4f\uff10-\uff19\uff3f]*$"
        },
        "title": {
            "type": "string",
            "title": "Label"
        },
        "fieldtype": {
            "type": "string",
            "default": "text",
            "required": true,
            "title": "Type",
            "enum": ["text", "checkbox", "multicheckbox", "select", "radio", "textarea", "email", "date", "number",
                "file", "image", "imagex",  "url", "icon", "guid", "address",
                "array", "table", "accordion", "relation", "related",
                "folder2", "file2", "url2", "role2",
                "wysihtml", "summernote", "ckeditor", "gallery", "documents", "object",
                "image2", "imagecrop",
                    /*, "publishstatus", "publishstartdate", "publishenddate"*/]
        },
        "vertical": {
            "type": "boolean",
            "dependencies": "fieldtype"
        },
        "fieldoptions": {
            "type": "array",
            "title": "Options",
            "items": {
                "type": "object",
                "properties": {
                    "value": {
                        "title": "Value",
                        "type": "string"
                    },
                    "text": {
                        "title": "Text",
                        "type": "string"
                    }
                }
            },
            "dependencies": "fieldtype"
        },
        "subfields": {
            "type": "array",
            "title": "Fields",
            "dependencies": "fieldtype"
        },
        "relationoptions": {
            "type": "object",
            "title": "Relation Options",
            "dependencies": "fieldtype",
            "properties": {
                "many": {
                    "type": "boolean",
                    "title": "Many"
                },
                "datakey": {
                    "type": "string",
                    "title": "Additional Data Key"
                },
                "valuefield": {
                    "type": "string",
                    "title": "Value Field"
                },
                "textfield": {
                    "type": "string",
                    "title": "Text Field"
                },
            }
        },
        "relatedoptions": {
            "type": "object",
            "title": "Relation Options",
            "dependencies": "fieldtype",
            "properties": {
                "many": {
                    "type": "boolean",
                    "title": "Many"
                },
                "textfield": {
                    "type": "string",
                    "title": "Text Field"
                },
            }
        },
        "dateoptions": {
            "type": "object",
            "title": "Date Options",
            "dependencies": "fieldtype",
            "properties": {
                "format": {
                    "type": "string",
                    "title": "Format (momentjs)"
                },
                "minDate": {
                    "type": "string",
                    "title": "Min date (iso)"
                },
                "maxDate": {
                    "type": "string",
                    "title": "Max date (iso)"
                }
            }
        },
        "fileoptions": {
            "type": "object",
            "title": "File Options",
            "dependencies": "fieldtype",
            "properties": {
                "folder": {
                    "type": "string",
                    "title": "Folder"
                }
            }
        },
        "file2options": {
            "type": "object",
            "title": "File Options",
            "dependencies": "fieldtype",
            "properties": {
                "folder": {
                    "type": "string",
                    "title": "Folder"
                },
                "filter": {
                    "type": "string",
                    "title": "Filter pattern"
                }
            }   
        },
        "folder2options": {
            "type": "object",
            "title": "Folder Options",
            "dependencies": "fieldtype",
            "properties": {
                "folder": {
                    "type": "string",
                    "title": "Folder"
                },
                "filter": {
                    "type": "string",
                    "title": "Filter pattern"
                }
            }
        },
        "imageoptions": {
            "type": "object",
            "title": "Image Options",
            "dependencies": "fieldtype",
            "properties": {
                "folder": {
                    "type": "string",
                    "title": "Folder"
                }
            }
        },
        "imagexoptions": {
            "type": "object",
            "title": "Image Options",
            "dependencies": "fieldtype",
            "properties": {
                "uploadhidden": {
                    "type": "boolean",
                    "title": "Hide Upload"
                },
                "uploadfolder": {
                    "type": "string",
                    "title": "Upload Folder"
                },
                "fileExtensions": {
                    "type": "string",
                    "title": "File Extensions",
                    "default": "gif|jpg|jpeg|tiff|png"
                },
                "fileMaxSize": {
                    "type": "number",
                    "title": "File Max Size (bytes)",
                    "default": 2000000
                },
                "showOverwrite": {
                    "type": "boolean",
                    "title": "Show Overwrite",
                    "default": true
                },
                "overwrite": {
                    "type": "boolean",
                    "title": "Overwrite",
                    "dependencies": "showOverwrite"
                },
                "showCropper": {
                    "type": "boolean",
                    "title": "Show Cropper",
                    "default": true
                },
                "cropfolder": {
                    "type": "string",
                    "title": "Crop Folder",
                    "dependencies": "showCropper"
                },
                "saveCropFile": {
                    "type": "boolean",
                    "title": "Save Crop file",
                    "dependencies": "showCropper",
                    "default": true
                },
                "width": {
                    "type": "number",
                    "title": "Crop width",
                    "dependencies": "showCropper",
                    "default": 2000
                },
                "height": {
                    "type": "number",
                    "title": "Crop height",
                    "dependencies": "showCropper",
                    "default": 1500
                },
            }
        },
        "ckeditoroptions": {
            "type": "object",
            //"title": "CKEditor Options",
            "dependencies": "fieldtype",
            "properties": {
                "configset": {
                    "title": "Config set",
                    "type": "string",
                    "enum": ["basic", "standard", "full"]
                }
            }
        },
        "image2options": {
            "type": "object",
            "title": "Image Options",
            "dependencies": "fieldtype",
            "properties": {
                "folder": {
                    "type": "string",
                    "title": "Folder"
                }
            }
        },
        "imagecropoptions": {
            "type": "object",
            "title": "Crop Options",
            "dependencies": "fieldtype",
            "properties": {
                "ratio": {
                    "type": "number",
                    "title": "Ratio"
                }
            }
        },
        "iconoptions": {
            "type": "object",
            "title": "Icon Options",
            "dependencies": "fieldtype",
            "properties": {
                "glyphicons": {
                    "type": "boolean",
                    "title": "Glyphicons"
                },
                "bootstrap": {
                    "type": "boolean",
                    "title": "Bootstrap"
                },
                "fontawesome": {
                    "type": "boolean",
                    "title": "Fontawesome",
                    "default": true
                }
            }
        },
        "many": {
            "type": "boolean",
            "title": "Many",
            "dependencies": "fieldtype"
        },
        "advanced": {
            "type": "boolean",
            "title": "Advanced"
        },
        "required": {
            "type": "boolean",
            "dependencies": "advanced"
        },
        "hidden": {
            "type": "boolean",
            "dependencies": "advanced"
        },
        "default": {
            "title": "Default",
            "type": "string",
            "dependencies": "advanced"
        },
        "helper": {
            "type": "string",
            "title": "Helper",
            "dependencies": "advanced"
        },
        "placeholder": {
            "type": "string",
            "title": "Placeholder",
            "dependencies": ["fieldtype", "advanced"]
        },
        "multilanguage": {
            "type": "boolean",
            "dependencies": ["fieldtype", "advanced"]
        },
        "index": {
            "type": "boolean",
            "dependencies": ["fieldtype", "advanced"]
        },
        "position": {
            "type": "string",
            "title": "Position",
            "dependencies": ["advanced"],
            "enum": ["1col1", "2col1", "2col2", "3col1", "3col2", "3col3"]
        },
        "dependencies": {
            "type": "array",
            "title": "Dependencies",
            "items": {
                "type": "object",
                "properties": {
                    "fieldname": {
                        "title": "Field",
                        "type": "string"
                    },
                    "values": {
                        "title": "Values (value1, value2, ...)",
                        "type": "string"
                    }
                }
            },
            "dependencies": ["advanced"]
        }
    }
};

fieldSchema.properties.subfields.items = fieldSchema;

var fieldOptions =
{
    "multilanguage": {
        "label": "Multi language",
        "dependencies": {
            "advanced": [true],
            "fieldtype": ["number","address","text", "textarea", "ckeditor", "file", "image", "url", "wysihtml", "summernote", "file2", "url2", "role2", "image2", "imagex"]
        }
    },
    "index": {
        "label": "Index",
        "dependencies": {
            "advanced": [true],
            "fieldtype": ["text", "checkbox", "multicheckbox", "select", "radio", "textarea", "email", "date", "number",
                "file", "url", "icon", "guid", "address", "relation", "related", "file2", "url2", "role2",
                "wysihtml", "summernote", "ckeditor", "documents"]
        }
    },
    "placeholder": {
        "dependencies": {
            "advanced": [true],
            "fieldtype": ["email", "text", "textarea"]
        }
    },
    "fieldname": {
        "showMessages": false
        //"fieldClass":"fieldname"
    },
    "fieldtype": {
        "optionLabels": ["Text", "Checkbox", "Multi checkbox", "Dropdown list (select)", "Radio buttons", "Text area", "Email address", "Date", "Number",

            "File (upload & autocomplete)", "Image (upload & autocomplete)", "ImageX (cropper, overwrite, ...)",  "Url (autocomplete for pages)", "Font Awesome Icons", "Guid (auto id)", "Address (autocomplete & geocode)",
            "List (Panels)", "List (Table)", "List (Accordion)", "Relation (Additional Data)", "Related",
            "Folder2 (folderID)", "File2 (fileID)", "Url2 (tabID)", "Role2 (roleID)",
            "Html (Wysihtml)", "Html (Summernote)", "Html (CK Editor)", "Image Gallery", "Documents", "Group (object)",
            "_Legacy - Image2 (fileID)", "_Legacy - Image (with croppper)",
                /*,"Publish status", "Publish start date", "Publish end date"*/]
    },
    "fieldoptions": {
        "type": "table",
        "dependencies": {
            "fieldtype": ["select", "radio", "multicheckbox"]
        }
    },
    "many": {
        "dependencies": {
            "fieldtype": ["role2"]
        }
    },
    "position": {
        "optionLabels": ["1 column", "2 columns - left", "2 columns - right", "3 columns - left", "3 columns - middle", "3 columns - right"],
        "vertical": false,
        "removeDefaultNone": true
    },
    "required": {
        "label": "Required"
    },
    "hidden": {
        "label": "Hidden"
    },
    "vertical": {
        "label": "Vertical",
        "dependencies": {
            "fieldtype": "radio"
        }
    },
    "subfields": {
        "collapsible": true,
        "type": "accordion",
        "items": {
            "fieldClass": "listfielddiv",
            "titleField": "fieldname"
        },
        "dependencies": {
            "fieldtype": ["array", "table", "accordion", "object"]
        }
    },
    "relationoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["relation"]
        },
        "fields": {
            "datakey": {

            },
            "valuefield": {

            },
            "textfield": {

            }
        }
    },
    "relatedoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["related"]
        },
        "fields": {
            "textfield": {

            }
        }
    },
    "dateoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["date"]
        },
        "fields": {
            "format": {

            },
            "minDate": {

            },
            "maxDate": {

            }
        }
    },
    "fileoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["file"]
        }
    },
    "file2options": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["file2"]
        }
    },
    "folder2options": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["folder2"]
        }
    },
    "imageoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["image"]
        }
    },
    "imagexoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["imagex"]
        }
    },
    "ckeditoroptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["ckeditor"]
        },
        "fields": {
            "configset": {
                "type": "select"
               
            }
        }
    },
    "image2options": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["image2"]
        }
    },
    "imagecropoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["imagecrop"]
        }
    },
    "iconoptions": {
        "collapsible": true,
        "dependencies": {
            "fieldtype": ["icon"]
        }
    },
    "dependencies": {
        "type": "table"
    }
};

fieldOptions.subfields.items.fields = fieldOptions;

var formbuilderConfig = {
    "schema": {
        //"title": "Fields",
        "type": "object",
        "properties": {
            "formfields": {
                "type": "array",
                "items": fieldSchema
            },
            "formtype": {
                "type": "string",
                "title": "Form type",
                "enum": ["object", "array"],
                "required": true,
                "default": "object"
            }
        }
    },
    "options": {
        "fields": {
            "formfields": {
                "type": "accordion",
                "toolbarSticky": true,
                "animate": false,
                "items": {
                    //"collapsible": true,
                    "fieldClass": "fielddiv",
                    "fields": fieldOptions,
                    "titleField": "fieldname"
                }
            },
            "formtype": {
                "type": "select",
                "optionLabels": ["Default (object)", "Additional data (array)"]
            }
        }
    },
    "view": "dnn-edit",
    "postRender": function (control) {
        var self = control;
        control.childrenByPropertyId["formfields"].on("change", function () {
            var value = self.getValue();
            showForm(value);
        });
        $(".form-builder div.loading").hide();
        $(".form-builder .dnnActions").show();
    }
};