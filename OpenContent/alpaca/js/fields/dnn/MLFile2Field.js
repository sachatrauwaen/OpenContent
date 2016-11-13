(function($) {

    var Alpaca = $.alpaca;
        
    Alpaca.Fields.MLFile2Field = Alpaca.Fields.File2Field.extend(
    /**
     * @lends Alpaca.Fields.File2Field.prototype
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
         * @see Alpaca.Fields.File2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (this.data && Alpaca.isObject(this.data)) {
                this.olddata = this.data;
            } else if (this.data) {
                this.olddata = {};
                this.olddata[this.defaultCulture] = this.data;
            }
            this.base();
        },

        getValue: function () {
            
                var val = this.base(val);
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
         * @see Alpaca.Field#setValue
         */
        setValue: function(val)
        {
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
            callback();
            $(this.control).parent().find('.select2').after('<img src="' + self.rootUrl + 'images/Flags/' + this.culture + '.gif" class="flag" />');
        },
    });

    Alpaca.registerFieldClass("mlfile2", Alpaca.Fields.MLFile2Field);

})(jQuery);