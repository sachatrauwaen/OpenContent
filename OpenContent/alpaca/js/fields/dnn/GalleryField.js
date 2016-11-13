(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.GalleryField = Alpaca.Fields.MultiUploadField.extend(
    {
        setup: function () {
            this.base();
            this.schema.items = {
                "type": "object",
                "properties": {
                
                    "Image": {
                        "title": "Image",
                        "type": "string"
                    }
                }
            };
            Alpaca.merge(this.options.items, {
                "fields": {
                    "Image": {
                        "type": "image"
                    }
                }
            });
            this.urlfield = "Image";
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

    Alpaca.registerFieldClass("gallery", Alpaca.Fields.GalleryField);

})(jQuery);