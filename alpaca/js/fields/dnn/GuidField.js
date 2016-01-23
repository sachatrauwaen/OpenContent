(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.GuidField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.TagField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            
            this.base(container, data, options, schema, view, connector);
           
        },
        setup: function () {
            var self = this;
            this.base();
            
        },
        setValue: function (value) {
     
            if (Alpaca.isEmpty(value)) {
                value = this.createGuid();
            }

            // be sure to call into base method
            this.base(value);

        },
        createGuid: function ()
        {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
                var r = Math.random()*16|0, v = c === 'x' ? r : (r&0x3|0x8);
                return v.toString(16);
            });
        },
        
        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Guid Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Guid field .";
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

    Alpaca.registerFieldClass("guid", Alpaca.Fields.GuidField);

})(jQuery);