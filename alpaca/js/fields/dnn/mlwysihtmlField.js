(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLwysihtmlField = Alpaca.Fields.wysihtmlField.extend(
    /**
     * @lends Alpaca.Fields.MLwysihtmlField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.defaultCulture = connector.defaultCulture;
        },
        /**
         * @see Alpaca.Fields.MLwysihtmlField#setup
         */
        setup: function () {
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.MLwysihtmlField#getValue
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
         * @see Alpaca.Fields.MLwysihtmlField#setValue
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
            else {
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
            var el = this.getControlEl();
            $(this.control.get(0)).after('<img src="/images/Flags/' + this.culture + '.gif" />');
            callback();
        },


        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        getTitle: function () {
            return "ML wysihtml Field";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a wysihtml control for use in editing MLHTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "wysihtml": {
                        "title": "CK Editor options",
                        "description": "Use this entry to provide configuration options to the underlying CKEditor plugin.",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.ControlField#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "wysiwyg": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("mlwysihtml", Alpaca.Fields.MLwysihtmlField);


})(jQuery);