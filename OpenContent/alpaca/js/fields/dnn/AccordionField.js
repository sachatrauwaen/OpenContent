(function($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.Accordion = Alpaca.Fields.ArrayField.extend(
    /**
     * @lends Alpaca.Fields.TitleArray.prototype
     */
    {

        /**
        * @see Alpaca.ControlField#getFieldType
        */
        getFieldType: function () {
            return "accordion";
        },
            
        setup: function()
        {
            var self = this;
            this.base();

            if (!self.options.titleField) {                
                if (self.schema.items && self.schema.items.properties 
                    && Object.keys(self.schema.items.properties).length) {
                    self.options.titleField = Object.keys(self.schema.items.properties)[0];                    
                }
            }

            /*
            if (typeof (this.options.items.postRender) == "undefined")
            {
                var label = "[no title]";
                this.options.items.postRender = function (callback) {
                    var field = null;
                        field = this.childrenByPropertyId[this.options.titleField];
                    if (field) {
                        var val = field.getValue();
                        val = val ? val : label;
                        this.getContainerEl().closest('.panel').find('.panel-title a').text(val);
                        field.on("keyup", function () {
                            var val = this.getValue();
                            val = val ? val : label;
                            $(this.getControlEl()).closest('.panel').find('.panel-title a').text(val);
                        });
                    }                    
                    if (Alpaca.isFunction(callback)) {
                        callback();
                    }
                };
            }
            */
        },
            
        createItem: function (index, itemSchema, itemOptions, itemData, postRenderCallback) {
            var self = this;
            this.base(index, itemSchema, itemOptions, itemData, function (control) {
                var label = "[no title]";
                var field = control.childrenByPropertyId[self.options.titleField];                
                if (field) {
                    var val = field.getValue();
                    val = val ? val : label;
                    control.getContainerEl().closest('.panel').find('.panel-title a').first().text(val);
                    field.on("keyup", function () {
                        var val = this.getValue();
                        val = val ? val : label;
                        $(this.getControlEl()).closest('.panel').find('.panel-title a').first().text(val);
                    });
                }

                if (postRenderCallback) {
                    postRenderCallback(control);
                }

            });
        },

        /**
         * @see Alpaca.ControlField#getType
         */
        getType: function() {
            return "array";
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.ControlField#getTitle
         */
        getTitle: function() {
            return "accordion Field";
        },

        /**
         * @see Alpaca.ControlField#getDescription
         */
        getDescription: function() {
            return "Renders array with title";
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("accordion", Alpaca.Fields.Accordion);

})(jQuery);
