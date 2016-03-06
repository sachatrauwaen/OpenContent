(function($) {

    var Alpaca = $.alpaca;

    $.alpaca.Fields.DnnUrlField = $.alpaca.Fields.TextField.extend({

        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.culture = connector.culture;
            this.sf = connector.servicesFramework;
        },


        setup: function () {
            this.base();
        },
        applyTypeAhead: function () {
            var self = this;
            var tConfig = tConfig = {};
            var tDatasets = tDatasets = {};
            if (!tDatasets.name) {
                tDatasets.name = self.getId();
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
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Tabs?q=%QUERY&l="+self.culture,
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

            // process typeahead
            $(self.control).typeahead(tConfig, tDatasets);

            // listen for "autocompleted" event and set the value of the field
            $(self.control).on("typeahead:autocompleted", function (event, datum) {
                self.setValue(datum.value);
                $(self.control).change();
            });

            // listen for "selected" event and set the value of the field
            $(self.control).on("typeahead:selected", function (event, datum) {
                self.setValue(datum.value);
                $(self.control).change();
            });

            // custom events
            if (tEvents) {
                if (tEvents.autocompleted) {
                    $(self.control).on("typeahead:autocompleted", function (event, datum) {
                        tEvents.autocompleted(event, datum);
                    });
                }
                if (tEvents.selected) {
                    $(self.control).on("typeahead:selected", function (event, datum) {
                        tEvents.selected(event, datum);
                    });
                }
            }

            // when the input value changes, change the query in typeahead
            // this is to keep the typeahead control sync'd with the actual dom value
            // only do this if the query doesn't already match
            var fi = $(self.control);
            $(self.control).change(function () {

                var value = $(this).val();

                var newValue = $(fi).typeahead('val');
                if (newValue !== value) {
                    $(fi).typeahead('val', newValue);
                }

            });

            // some UI cleanup (we don't want typeahead to restyle)
            $(self.field).find("span.twitter-typeahead").first().css("display", "block"); // SPAN to behave more like DIV, next line
            $(self.field).find("span.twitter-typeahead input.tt-input").first().css("background-color", "");
            
        }
    });
    Alpaca.registerFieldClass("url", Alpaca.Fields.DnnUrlField);

})(jQuery);