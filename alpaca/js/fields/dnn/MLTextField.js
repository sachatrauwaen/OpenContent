(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLTextField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.TagField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
        },
        /**
         * @see Alpaca.Fields.TextField#getFieldType
        */
        /*
        getFieldType: function () {
            return "text";
        },
        */

        /**
         * @see Alpaca.Fields.TextField#setup
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
            /*
            Alpaca.mergeObject(this.options, {
                "fieldClass": "flag-"+this.culture
            });
            */
        },
        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function () {
            var val = this.base();
            var self = this;
            /*
            if (val === "") {
                return [];
            }
            */

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
            //o["_type"] = "languages";
            return o;
        },

        /**
         * @see Alpaca.Fields.TextField#setValue
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
            //$(this.control.get(0)).after('<div style="background:#eee;margin-bottom: 18px;display:inline-block;padding-bottom:8px;"><span>' + this.culture + '</span></div>');
            callback();
        },
        
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Multi Language Text Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Multi Language Text field .";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
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
         * @see Alpaca.Fields.TextField#getOptionsForOptions
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

    Alpaca.registerFieldClass("mltext", Alpaca.Fields.MLTextField);

})(jQuery);