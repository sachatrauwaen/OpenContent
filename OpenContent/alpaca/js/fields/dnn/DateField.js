﻿(function ($) {

    // NOTE: this requires bootstrap-datetimepicker.js
    // NOTE: this requires moment.js

    var Alpaca = $.alpaca;

    Alpaca.Fields.DateField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.DateField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "date";
        },

        getDefaultFormat: function () {
            return "MM/DD/YYYY";
        },

        getDefaultExtraFormats: function () {
            return [];
        },

        /**
         * @see Alpaca.Fields.TextField#setup
         */
        setup: function () {
            var self = this;

            // default html5 input type = "date";
            //this.inputType = "date";

            this.base();

            if (!self.options.picker) {
                self.options.picker = {};
            }

            if (typeof (self.options.picker.useCurrent) === "undefined") {
                self.options.picker.useCurrent = false;
            }

            // date format

            /*
            if (self.options.picker.format) {
                self.options.dateFormat = self.options.picker.format;
            }
            */
            if (!self.options.dateFormat) {
                //self.options.dateFormat = self.getDefaultFormat();
            }
            if (!self.options.picker.format) {
                self.options.picker.format = self.options.dateFormat;
            }

            // extra formats
            if (!self.options.picker.extraFormats) {
                var extraFormats = self.getDefaultExtraFormats();
                if (extraFormats) {
                    self.options.picker.extraFormats = extraFormats;
                }
            }

            if (typeof (self.options.manualEntry) === "undefined") {
                self.options.manualEntry = false;
            }
            if (typeof (self.options.icon) === "undefined") {
                self.options.icon = false;
            }
        },

        onKeyPress: function (e) {
            if (this.options.manualEntry) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
            else {
                this.base(e);
                return;
            }
        },

        onKeyDown: function (e) {
            if (this.options.manualEntry) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
            else {
                this.base(e);
                return;
            }
        },

        beforeRenderControl: function (model, callback) {
            this.field.css("position", "relative");

            callback();
        },

        /**
         * @see Alpaca.Fields.TextField#afterRenderControl
         */
        afterRenderControl: function (model, callback) {

            var self = this;

            this.base(model, function () {

                if (self.view.type !== "display") {
                    $component = self.getControlEl();
                    if (self.options.icon) {
                        self.getControlEl().wrap('<div class="input-group date"></div>');
                        self.getControlEl().after('<span class="input-group-addon"><span class="glyphicon glyphicon-calendar"></span></span>');
                        var $component = self.getControlEl().parent();
                    }

                    if ($.fn.datetimepicker) {
                        
                        $component.datetimepicker(self.options.picker);
                        self.picker = $component.data("DateTimePicker");

                        $component.on("dp.change", function (e) {

                            // we use a timeout here because we want this to run AFTER control click handlers
                            setTimeout(function () {
                                self.onChange.call(self, e);
                                self.triggerWithPropagation("change", e);
                            }, 250);

                        });

                        // set value if provided
                        if (self.data) {
                            self.picker.date(self.data);
                        }
                    }
                }
                callback();
            });
        },

        /**
         * Returns field value as a JavaScript Date.
         *
         * @returns {Date} Field value.
         */
        getDate: function () {
            var self = this;

            var date = null;
            try {
                if (self.picker) {
                    date = (self.picker.date() ? self.picker.date()._d : null);
                }
                else {
                    date = new Date(this.getValue());
                }
            }
            catch (e) {
                console.error(e);
            }

            return date;
        },

        /**
         * Returns field value as a JavaScript Date.
         *
         * @returns {Date} Field value.
         */
        date: function () {
            return this.getDate();
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function (e) {
            this.base();

            this.refreshValidationState();
        },

        isAutoFocusable: function () {
            return false;
        },

        /**
         * @see Alpaca.Fields.TextField#handleValidate
         */
        handleValidate: function () {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateDateFormat();
            valInfo["invalidDate"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("invalidDate"), [this.options.dateFormat]),
                "status": status
            };

            return baseStatus && valInfo["invalidDate"]["status"];
        },

        /**
         * Validates date format.
         *
         * @returns {Boolean} True if it is a valid date, false otherwise.
         */
        _validateDateFormat: function () {
            var self = this;

            var isValid = true;

            if (self.options.dateFormat) {
                var value = self.getValue();
                if (value || self.isRequired()) {
                    // collect all formats
                    var dateFormats = [];
                    dateFormats.push(self.options.dateFormat);
                    if (self.options.picker && self.options.picker.extraFormats) {
                        for (var i = 0; i < self.options.picker.extraFormats.length; i++) {
                            dateFormats.push(self.options.picker.extraFormats[i]);
                        }
                    }

                    for (var i = 0; i < dateFormats.length; i++) {
                        isValid = isValid || moment(value, self.options.dateFormat, true).isValid();
                    }
                }
            }

            return isValid;
        },

        /**
         * @see Alpaca.Fields.TextField#setValue
         */
        setValue: function (value) {
            var self = this;
            if (value == "now") {
                value = moment().format();
            } else if (value == "today") {
                value = moment().startOf('day').format();
            }
            this.base(value);
            if (this.picker) {
                if (self.options.dateFormat) {
                    if (moment(value, self.options.dateFormat, true).isValid()) {
                        this.picker.date(value);
                    }
                }
                else {
                    if (moment(value).isValid()) {
                        this.picker.date(moment(value));
                    }
                }
            }
        },

        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function () {
            
            var self = this;

            var date = null;
            try {
                if (self.picker) {
                    date = (self.picker.date() ? self.picker.date().format("YYYY-MM-DDTHH:mm:ss") : null);
                }
                else {
                    date = this.base();
                }
            }
            catch (e) {
                console.error(e);
            }

            return date;

        },

        destroy: function () {
            this.base();

            this.picker = null;
        }


        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Date Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Date Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfSchema
         */
        getSchemaOfSchema: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "format": {
                        "title": "Format",
                        "description": "Property data format",
                        "type": "string",
                        "default": "date",
                        "enum": ["date"],
                        "readonly": true
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getOptionsForSchema
         */
        getOptionsForSchema: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "format": {
                        "type": "text"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.TextField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "dateFormat": {
                        "title": "Date Format",
                        "description": "Date format (using moment.js format)",
                        "type": "string"
                    },
                    "picker": {
                        "title": "DatetimePicker options",
                        "description": "Options that are supported by the <a href='http://eonasdan.github.io/bootstrap-datetimepicker/'>Bootstrap DateTime Picker</a>.",
                        "type": "any"
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
                    "dateFormat": {
                        "type": "text"
                    },
                    "picker": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerMessages({
        "invalidDate": "Invalid date for format {0}"
    });
    Alpaca.registerFieldClass("date", Alpaca.Fields.DateField);
    Alpaca.registerDefaultFormatFieldMapping("date", "date");

})(jQuery);