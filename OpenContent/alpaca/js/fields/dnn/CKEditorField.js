﻿(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.CKEditorField = Alpaca.Fields.TextAreaField.extend(
    /**
     * @lends Alpaca.Fields.CKEditorField.prototype
     */
    {
        /**
         * @see Alpaca.Fields.TextAreaField#getFieldType
         */
        getFieldType: function () {
            return "ckeditor";
        },

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextAreaField#setup
         */
        setup: function () {
            if (!this.data) {
                this.data = "";
            }

            this.base();

            if (typeof (this.options.ckeditor) == "undefined") {
                this.options.ckeditor = {};
            }
            if (typeof (this.options.configset) == "undefined") {
                this.options.configset = "";
            }
        },

        afterRenderControl: function (model, callback) {
            var self = this;

            this.base(model, function () {

                // see if we can render CK Editor
                if (!self.isDisplayOnly() && self.control && typeof (CKEDITOR) !== "undefined") {

                    var defaultConfig = {
                        toolbar: [
                             { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                             { name: 'styles', items: ['Styles', 'Format'] },
                             { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                             { name: 'links', items: ['Link', 'Unlink'] },

                             { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source'] },
                        ],
                        // Set the most common block elements.
                        format_tags: 'p;h1;h2;h3;pre',

                        // Simplify the dialog windows.
                        removeDialogTabs: 'image:advanced;link:advanced',

                        // Remove one plugin.
                        removePlugins: 'elementspath',

                        extraPlugins: 'dnnpages',

                        //autoGrow_onStartup : true,
                        //autoGrow_minHeight : 100,
                        //autoGrow_maxHeight : 300,
                        height: 150,
                        //skin : 'flat',

                        customConfig: '',
                        stylesSet: []
                    };
                    if (self.options.configset == "basic") {
                        defaultConfig = {
                            toolbar: [
                                 { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                 { name: 'styles', items: ['Styles', 'Format'] },
                                 { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                                 { name: 'links', items: ['Link', 'Unlink'] },

                                 { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Maximize', 'Source'] },
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre',
                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: []
                        };
                    } else if (self.options.configset == "standard") {
                        defaultConfig = {
                            toolbar: [
                                 { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                 { name: 'styles', items: ['Styles', 'Format'] },
                                 { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', ] },
                                 { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                                 { name: 'insert', items: ['Table', 'Smiley', 'SpecialChar', 'Iframe'] },
                                 { name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Maximize', 'ShowBlocks', 'Source'] }
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre;div',

                            //http://docs.ckeditor.com/#!/guide/dev_allowed_content_rules
                            extraAllowedContent:
                            'table tr th td caption[*](*);' +
                            'div span(*);'
                            //'a[!href](*);' 
                            //'img[!src,alt,width,height](*);' +
                            //'h1 h2 h3 p blockquote strong em(*);' +
                            ,

                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: []
                        };
                    } else if (self.options.configset == "full") {
                        defaultConfig = {
                            toolbar: [                                
                                { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
                                { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll', '-', 'SpellChecker', 'Scayt'] },
                                { name: 'insert', items: ['EasyImageUpload', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
                                '/',
                                { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                                {
                                    name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv',
                                    '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl']
                                },
                                { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
                                
                                '/',
                                { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
                                { name: 'colors', items: ['TextColor', 'BGColor'] },
                                { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'About', '-', 'Source'] }
                            ],
                            // Set the most common block elements.
                            format_tags: 'p;h1;h2;h3;pre;div',
                            //http://docs.ckeditor.com/#!/api/CKEDITOR.config-cfg-allowedContent
                            allowedContentRules: true, 
                            // Simplify the dialog windows.
                            removeDialogTabs: 'image:advanced;link:advanced',
                            // Remove one plugin.
                            removePlugins: 'elementspath,link,image',
                            extraPlugins: 'dnnpages',
                            //autoGrow_onStartup : true,
                            //autoGrow_minHeight : 100,
                            //autoGrow_maxHeight : 300,
                            height: 150,
                            //skin : 'flat',
                            customConfig: '',
                            stylesSet: [],
                            //easyimage_toolbar :['EasyImageAlignLeft', 'EasyImageAlignCenter', 'EasyImageAlignRight']
                        };
                    }
                    var config = $.extend({}, defaultConfig, self.options.ckeditor);

                    // wait for Alpaca to declare the DOM swapped and ready before we attempt to do anything with CKEditor
                    self.on("ready", function () {
                        if (!self.editor) {
                            if (self.sf) {
                                config.cloudServices_uploadUrl = self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadEasyImage";
                                config.cloudServices_tokenUrl = self.sf.getServiceRoot('OpenContent') + "FileUpload/EasyImageToken";
                            }
                            self.editor = CKEDITOR.replace($(self.control)[0], config);
                            self.initCKEditorEvents();
                        }
                    });
                }

                // if the ckeditor's dom element gets destroyed, make sure we clean up the editor instance
                $(self.control).bind('destroyed', function () {

                    if (self.editor) {
                        self.editor.removeAllListeners();
                        // catch here because CKEditor has an issue if DOM element deletes before CKEditor cleans up
                        // see: https://github.com/lemonde/angular-ckeditor/issues/7
                        try { self.editor.destroy(false); } catch (e) { }
                        self.editor = null;
                    }

                });

                callback();
            });
        },

        initCKEditorEvents: function () {
            var self = this;

            if (self.editor) {
                // click event
                self.editor.on("click", function (e) {
                    self.onClick.call(self, e);
                    self.trigger("click", e);
                });

                // change event
                self.editor.on("change", function (e) {
                    self.onChange();
                    self.triggerWithPropagation("change", e);
                });

                // blur event
                self.editor.on('blur', function (e) {
                    self.onBlur();
                    self.trigger("blur", e);
                });

                // focus event
                self.editor.on("focus", function (e) {
                    self.onFocus.call(self, e);
                    self.trigger("focus", e);
                });

                // keypress event
                self.editor.on("key", function (e) {
                    self.onKeyPress.call(self, e);
                    self.trigger("keypress", e);
                });

                // NOTE: these do not seem to work with CKEditor?
                /*
                 // keyup event
                 self.editor.on("keyup", function(e) {
                 self.onKeyUp.call(self, e);
                 self.trigger("keyup", e);
                 });

                 // keydown event
                 self.editor.on("keydown", function(e) {
                 self.onKeyDown.call(self, e);
                 self.trigger("keydown", e);
                 });
                 */

                self.editor.on('fileUploadRequest', function (evt) {
                    self.sf.setModuleHeaders(evt.data.fileLoader.xhr);
                });
            }
        },

        setValue: function (value) {
            var self = this;

            // be sure to call into base method
            this.base(value);

            if (self.editor) {
                self.editor.setData(value);
            }
        },

        /**
         * @see Alpaca.Fields.ControlField#getControlValue
         */
        getControlValue: function () {
            var self = this;

            var value = null;

            if (self.editor) {
                value = self.editor.getData();
            }

            return value;
        },

        /**
         * @see Alpaca.Field#destroy
         */
        destroy: function () {
            var self = this;

            // destroy the plugin instance
            if (self.editor) {
                self.editor.destroy();
                self.editor = null;
            }

            // call up to base method
            this.base();
        }

        /* builder_helpers */

        /**
         * @see Alpaca.Fields.TextAreaField#getTitle
         */
        ,
        getTitle: function () {
            return "CK Editor";
        },

        /**
         * @see Alpaca.Fields.TextAreaField#getDescription
         */
        getDescription: function () {
            return "Provides an instance of a CK Editor control for use in editing HTML.";
        },

        /**
         * @private
         * @see Alpaca.ControlField#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "ckeditor": {
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
                    "ckeditor": {
                        "type": "any"
                    }
                }
            });
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("ckeditor", Alpaca.Fields.CKEditorField);

})(jQuery);