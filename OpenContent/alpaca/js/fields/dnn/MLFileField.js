﻿(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLFileField = Alpaca.Fields.FileField.extend(
    /**
     * @lends Alpaca.Fields.MLFileField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
            this.rootUrl = connector.rootUrl;
        },

        /**
         * @see Alpaca.Fields.MLFileField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            if (this.culture != this.defaultCulture && this.olddata && this.olddata[this.defaultCulture]) {
                this.options.placeholder = this.olddata[this.defaultCulture];
            } else if (this.olddata && Object.keys(this.olddata).length && this.olddata[Object.keys(this.olddata)[0]]) {
                this.options.placeholder = this.olddata[Object.keys(this.olddata)[0]];
            } else {
                this.options.placeholder = "";
            }

            this.base();
        },
        /**
         * @see Alpaca.Fields.MLFileField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
            var o = {};
            if (this.olddata && Alpaca.isObject(this.olddata)) {
                $.each(this.olddata, function (key, value) {
                    var v = Alpaca.copyOf(value);
                    if (key != self.culture) {
                        o[key] = v;
                    }
                });
            }
            if (val != "") {
                o[self.culture] = val;
            }
            if ($.isEmptyObject(o)) {
                return "";
            }
            return o;
        },

        /**
         * @see Alpaca.Fields.MLFileField#setValue
         */
        setValue: function (val) {
            if (val === "") {
                return;
            }
            if (!val) {
                this.base("");
                return;
            }
            if (Alpaca.isObject(val)) {
                var v = val[this.culture];
                if (!v) {
                    this.base("");
                    return;
                }
                this.base(v);
            }
            else
            {
                this.base(val);
            }
        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender2(function () {
                    callback();
                });
            });
        },
        handlePostRender2: function (callback) {
            var self = this;
            var el = this.getTextControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLFileField#getTitle
         */
        getTitle: function () {
            return "Multi Language Url Field";
        },

        /**
         * @see Alpaca.Fields.MLFileField#getDescription
         */
        getDescription: function () {
            return "Multi Language Url field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLFileField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "separator": {
                        "title": "Separator",
                        "description": "Separator used to split tags.",
                        "type": "string",
                        "default": ","
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.MLFileField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "separator": {
                        "type": "text"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlfile", Alpaca.Fields.MLFileField);

})(jQuery);