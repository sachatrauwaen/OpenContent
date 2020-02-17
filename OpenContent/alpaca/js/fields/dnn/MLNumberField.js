(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MLNumberField = Alpaca.Fields.NumberField.extend(
        {
            constructor: function (container, data, options, schema, view, connector) {
                var self = this;
                this.base(container, data, options, schema, view, connector);
                this.culture = connector.culture;
                this.defaultCulture = connector.defaultCulture;
                this.rootUrl = connector.rootUrl;
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
                } else if (this.olddata && Object.keys(this.olddata).length && this.olddata[Object.keys(this.olddata)[0]]) {
                    this.options.placeholder = this.olddata[Object.keys(this.olddata)[0]];
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
            getFloatValue: function () {
                var val = this.base();
                var self = this;
                
                return val;
            },
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

            getControlValue: function () {
                var val = this._getControlVal(true);

                if (typeof (val) == "undefined" || "" == val) {
                    return val;
                }

                return parseFloat(val);
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
             * Validates if it is a float number.
             * @returns {Boolean} true if it is a float number
             */
            _validateNumber: function () {

                // get value as text
                var textValue = this._getControlVal();
                if (typeof (textValue) === "number") {
                    textValue = "" + textValue;
                }

                // allow empty
                if (Alpaca.isValEmpty(textValue)) {
                    return true;
                }

                // check if valid number format
                var validNumber = Alpaca.testRegex(Alpaca.regexps.number, textValue);
                if (!validNumber) {
                    return false;
                }

                // quick check to see if what they entered was a number
                var floatValue = this.getFloatValue();
                if (isNaN(floatValue)) {
                    return false;
                }

                return true;
            },

            /**
             * Validates divisibleBy constraint.
             * @returns {Boolean} true if it passes the divisibleBy constraint.
             */
            _validateDivisibleBy: function () {
                var floatValue = this.getFloatValue();
                if (!Alpaca.isEmpty(this.schema.divisibleBy)) {

                    // mod
                    if (floatValue % this.schema.divisibleBy !== 0) {
                        return false;
                    }
                }
                return true;
            },

            /**
             * Validates maximum constraint.
             * @returns {Boolean} true if it passes the maximum constraint.
             */
            _validateMaximum: function () {
                var floatValue = this.getFloatValue();

                if (!Alpaca.isEmpty(this.schema.maximum)) {
                    if (floatValue > this.schema.maximum) {
                        return false;
                    }

                    if (!Alpaca.isEmpty(this.schema.exclusiveMaximum)) {
                        if (floatValue == this.schema.maximum && this.schema.exclusiveMaximum) { // jshint ignore:line
                            return false;
                        }
                    }
                }

                return true;
            },

            /**
             * Validates maximum constraint.
             * @returns {Boolean} true if it passes the minimum constraint.
             */
            _validateMinimum: function () {
                var floatValue = this.getFloatValue();

                if (!Alpaca.isEmpty(this.schema.minimum)) {
                    if (floatValue < this.schema.minimum) {
                        return false;
                    }

                    if (!Alpaca.isEmpty(this.schema.exclusiveMinimum)) {
                        if (floatValue == this.schema.minimum && this.schema.exclusiveMinimum) { // jshint ignore:line
                            return false;
                        }
                    }
                }

                return true;
            },

            /**
             * Validates multipleOf constraint.
             * @returns {Boolean} true if it passes the multipleOf constraint.
             */
            _validateMultipleOf: function () {
                var floatValue = this.getFloatValue();

                if (!Alpaca.isEmpty(this.schema.multipleOf)) {
                    if (floatValue && this.schema.multipleOf !== 0) {
                        return false;
                    }
                }

                return true;
            },

            getTitle: function () {
                return "Multi Language Number Field";
            },


            getDescription: function () {
                return "Multi Language Number field .";
            },


            /* end_builder_helpers */
        });

    Alpaca.registerFieldClass("mlnumber", Alpaca.Fields.MLNumberField);

})(jQuery);