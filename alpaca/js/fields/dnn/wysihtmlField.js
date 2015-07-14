(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.wysihtmlField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextAreaField#getFieldType
         */
        getFieldType: function () {
            return "wysihtml";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#setup
         */
        setup: function () {
            if (!this.data) {
                this.data = "";
            }

            this.base();

            if (typeof (this.options.wysihtml) == "undefined") {
                this.options.wysihtml = {};
            }
        },

        afterRenderControl: function (model, callback) {
            var self = this;

            this.base(model, function () {

                // see if we can render CK Editor
                if (!self.isDisplayOnly() && self.control ) {
                    //self.plugin = $(self.control).ckeditor(self.options.ckeditor); // Use CKEDITOR.replace() if element is <textarea>.
                    var el = self.control;
                    var ta = $(el).find('#'+self.id)[0];


                    self.editor = new wysihtml5.Editor(ta, {
                        toolbar: $(el).find('#' + self.id + '-toolbar')[0],                        
                        parserRules: wysihtml5ParserRules // defined in file parser rules javascript
                    });

                    wysihtml5.commands.custom_class = {
                        exec: function (composer, command, className) {
                            return wysihtml5.commands.formatBlock.exec(composer, command, "p", className, new RegExp(className, "g"));
                        },
                        state: function (composer, command, className) {
                            return wysihtml5.commands.formatBlock.state(composer, command, "p", className, new RegExp(className, "g"));
                        }
                    };

                }
                callback();
            });
        },
        getEditor: function () {
            return this.editor;
        },

        setValue: function(value)
        {
            var self = this;

            if (this.editor)
            {
                this.editor.setValue(value);
                //self.editor.clearSelection();
            }

            // be sure to call into base method
            this.base(value);
        },

        /**
         * @see Alpaca.Fields.TextField#getValue
         */
        getValue: function()
        {
            var value = null;
            if (this.editor)
            {
                if (this.editor.currentView == 'source')
                    value = this.editor.sourceView.textarea.value
                else 
                    value = this.editor.getValue();
            }
            return value;
        },


        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        getTitle: function () {
            return "wysihtml";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a wysihtml control for use in editing HTML.";
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

    Alpaca.registerFieldClass("wysihtml", Alpaca.Fields.wysihtmlField);

})(jQuery);