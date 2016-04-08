(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLCKEditorField = Alpaca.Fields.CKEditorField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
        },

        /**
         * @see Alpaca.Fields.CKEditorField#setup
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

            if (!this.options.ckeditor) {
                this.options.ckeditor = {};
            }
            if (!this.options.ckeditor.extraPlugins) {
                this.options.ckeditor.extraPlugins = 'confighelper';
            }
        },

        /**
         * @see Alpaca.Fields.CKEditorField#getValue
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
         * @see Alpaca.Fields.CKEditorField#setValue
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
            $(this.control.get(0)).after('<img src="/images/Flags/' + this.culture + '.gif" class="flag" />');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.CKEditorField#getTitle
         */
        getTitle: function () {
            return "Multi Language CKEditor Field";
        },

        /**
         * @see Alpaca.Fields.CKEditorField#getDescription
         */
        getDescription: function () {
            return "Multi Language CKEditor field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.CKEditorField#getSchemaOfOptions
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
         * @see Alpaca.Fields.CKEditorField#getOptionsForOptions
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

    Alpaca.registerFieldClass("mlckeditor", Alpaca.Fields.MLCKEditorField);

})(jQuery);