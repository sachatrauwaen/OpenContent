(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLSummernote = Alpaca.Fields.SummernoteField.extend(
    /**
     * @lends Alpaca.Fields.MLSummernote.prototype
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
         * @see Alpaca.Fields.MLSummernote#setup
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
            } else {
                this.options.placeholder = "";
            }

            this.base();
        },

        /**
         * @see Alpaca.Fields.MLSummernote#getValue
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
         * @see Alpaca.Fields.MLSummernote#setValue
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
                self.handlePostRender(function () {
                    callback();
                });
            });
        },
        handlePostRender: function (callback) {
            var self = this;
            var el = this.getControlEl();
            $(this.control.get(0)).after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.MLSummernote#getTitle
         */
        getTitle: function () {
            return "Multi Language CKEditor Field";
        },

        /**
         * @see Alpaca.Fields.MLSummernote#getDescription
         */
        getDescription: function () {
            return "Multi Language CKEditor field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.MLSummernote#getSchemaOfOptions
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
         * @see Alpaca.Fields.MLSummernote#getOptionsForOptions
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

    Alpaca.registerFieldClass("mlsummernote", Alpaca.Fields.MLSummernote);

})(jQuery);