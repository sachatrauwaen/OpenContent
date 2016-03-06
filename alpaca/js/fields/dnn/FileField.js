(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.FileField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "file";
        }
        ,
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "File Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "File Field.";
        },
        getControlEl: function () {
            return $(this.control.get(0)).find('input[type=text]#' + this.id);
        },
        setValue: function (value) {

            //var el = $( this.control).filter('#'+this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getControlEl();

            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    el.val("");
                }
                else {
                    el.val(value);
                }
            }

            // be sure to call into base method
            //this.base(value);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var value = null;

            //var el = $(this.control).filter('#' + this.id);
            //var el = $(this.control.get(0)).find('input[type=text]');
            var el = this.getControlEl();
            if (el && el.length > 0) {
                value = el.val();
            }
            return value;
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

            //var el = this.control;
            var el = this.getControlEl();


            $(this.control.get(0)).find('input[type=file]').fileupload({
                dataType: 'json',
                url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                maxFileSize: 25000000,
                formData: { uploadfolder: self.options.uploadfolder },
                beforeSend: self.sf.setModuleHeaders,
                add: function (e, data) {
                    //data.context = $(opts.progressContextSelector);
                    //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                    //data.context.show('fade');
                    data.submit();
                },
                progress: function (e, data) {
                    if (data.context) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                    }
                },
                done: function (e, data) {
                    if (data.result) {
                        $.each(data.result, function (index, file) {
                            self.setValue(file.url);
                            $(el).change();
                            //$(el).change();
                            //$(e.target).parent().find('input[type=text]').val(file.url);
                            //el.val(file.url);
                            //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                        });
                    }
                }
            }).data('loaded', true);

            callback();
        },
        applyTypeAhead: function () {
            var self = this;

            if (self.control.typeahead && self.options.typeahead && !Alpaca.isEmpty(self.options.typeahead)) {

                var tConfig = self.options.typeahead.config;
                if (!tConfig) {
                    tConfig = {};
                }
                var tDatasets = tDatasets = {};
                if (!tDatasets.name) {
                    tDatasets.name = self.getId();
                }

                var tFolder = self.options.typeahead.Folder;
                if (!tFolder) {
                    tFolder = "";
                }

                var tEvents = tEvents = {};

                var bloodHoundConfig = {
                    datumTokenizer: function (d) {
                        return Bloodhound.tokenizers.whitespace(d.value);
                    },
                    queryTokenizer: Bloodhound.tokenizers.whitespace
                };

                /*
                if (tDatasets.type === "prefetch") {
                    bloodHoundConfig.prefetch = {
                        url: tDatasets.source,
                        ajax: {
                            //url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            beforeSend: connector.servicesFramework.setModuleHeaders,
        
                        }
                    };
        
                    if (tDatasets.filter) {
                        bloodHoundConfig.prefetch.filter = tDatasets.filter;
                    }
                }
                */

                bloodHoundConfig.remote = {
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Files?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: self.sf.setModuleHeaders,
                    }
                };

                if (tDatasets.filter) {
                    bloodHoundConfig.remote.filter = tDatasets.filter;
                }

                if (tDatasets.replace) {
                    bloodHoundConfig.remote.replace = tDatasets.replace;
                }


                var engine = new Bloodhound(bloodHoundConfig);
                engine.initialize();
                tDatasets.source = engine.ttAdapter();

                tDatasets.templates = {
                    "empty": "Nothing found...",
                    "suggestion": "{{name}}"
                };

                // compile templates
                if (tDatasets.templates) {
                    for (var k in tDatasets.templates) {
                        var template = tDatasets.templates[k];
                        if (typeof (template) === "string") {
                            tDatasets.templates[k] = Handlebars.compile(template);
                        }
                    }
                }

                //var el = $(this.control.get(0)).find('input[type=text]');
                var el = this.getControlEl();
                // process typeahead
                $(el).typeahead(tConfig, tDatasets);

                // listen for "autocompleted" event and set the value of the field
                $(el).on("typeahead:autocompleted", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    self.setValue(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // custom events
                if (tEvents) {
                    if (tEvents.autocompleted) {
                        $(el).on("typeahead:autocompleted", function (event, datum) {
                            tEvents.autocompleted(event, datum);
                        });
                    }
                    if (tEvents.selected) {
                        $(el).on("typeahead:selected", function (event, datum) {
                            tEvents.selected(event, datum);
                        });
                    }
                }

                // when the input value changes, change the query in typeahead
                // this is to keep the typeahead control sync'd with the actual dom value
                // only do this if the query doesn't already match
                //var fi = $(self.control);
                $(el).change(function () {

                    var value = $(this).val();

                    var newValue = $(el).typeahead('val');
                    if (newValue !== value) {
                        $(el).typeahead('val', value);
                    }

                });

                // some UI cleanup (we don't want typeahead to restyle)
                $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
                $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            }
        }

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("file", Alpaca.Fields.FileField);

})(jQuery);