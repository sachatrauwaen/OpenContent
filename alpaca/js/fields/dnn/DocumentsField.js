(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.DocumentsField = Alpaca.Fields.MultiUploadField.extend(
    {
        setup: function () {
            this.base();
            this.schema.items = {
                "type": "object",
                "properties": {
                    "Title": {
                        "title": "Title",
                        "type": "string"
                    },
                    "File": {
                        "title": "File",
                        "type": "string"
                    },
                }
            };
            Alpaca.merge(this.options.items, {
                "fields": {
                    "File": {
                        "type": "file"
                    },
                }
            });
            this.urlfield = "File";
        },
        getTitle: function () {
            return "Gallery";
        },
        getDescription: function () {
            return "Image Gallery";
        },

        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "validateAddress": {
                        "title": "Address Validation",
                        "description": "Enable address validation if true",
                        "type": "boolean",
                        "default": true
                    },
                    "showMapOnLoad": {
                        "title": "Whether to show the map when first loaded",
                        "type": "boolean"
                    }
                }
            });
        },

        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "validateAddress": {
                        "helper": "Address validation if checked",
                        "rightLabel": "Enable Google Map for address validation?",
                        "type": "checkbox"
                    }
                }
            });
        }

    });
    Alpaca.registerFieldClass("documents", Alpaca.Fields.DocumentsField);

})(jQuery);