(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.MultiUploadField = Alpaca.Fields.ArrayField.extend(
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.itemsCount = 0;
        },
        setup: function () {

            this.base();
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            this.urlfield = "";
            if (this.options && this.options.items && (this.options.items.fields || this.options.items.type) ) {
                if (this.options.items.type == "image") {

                } else if (this.options.items.fields ) {
                    for (var i in this.options.items.fields) {
                        var f = this.options.items.fields[i];
                        if (f.type == "image" || f.type == "mlimage" || f.type == "imagecrop" || f.type == "imagecrop2" || f.type == "imagex") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.uploadfolder;
                            break;
                        }
                        else if (f.type == "file" || f.type == "mlfile") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.uploadfolder;
                            break;
                        } else if (f.type == "image2" || f.type == "mlimage2") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.folder;
                            break;
                        }
                        else if (f.type == "file2" || f.type == "mlfile2") {
                            this.urlfield = i;
                            this.options.uploadfolder = f.folder;
                            break;
                        }
                    }
                }
            }
        },
        afterRenderContainer: function (model, callback) {
            var self = this;
            this.base(model, function () {
                var container = self.getContainerEl();
                //$(container).addClass("alpaca-MultiUpload");
                if (!self.isDisplayOnly() ) {
                   
                    $('<div style="clear:both;"></div>').prependTo(container);
                    var progressBar = $('<div class="progress" ><div class="bar" style="width: 0%;"></div></div>').prependTo(container);
                    var mapButton = $('<input type="file" multiple="multiple" />').prependTo(container);

                    this.wrapper = $("<span class='dnnInputFileWrapper dnnSecondaryAction' style='margin-bottom:10px;;'></span>");
                    this.wrapper.text("Upload muliple files");
                    mapButton.wrap(this.wrapper);
                    if (self.sf){
                    mapButton.fileupload({
                        dataType: 'json',
                        url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                        maxFileSize: 25000000,
                        formData: { uploadfolder: self.options.uploadfolder },
                        beforeSend: self.sf.setModuleHeaders,
                        change: function (e, data) {
                            self.itemsCount = self.children.length;
                        },
                        add: function (e, data) {
                            //data.context = $(opts.progressContextSelector);
                            //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                            //data.context.show('fade');
                            data.submit();
                        },
                        progressall: function (e, data) {
                                var progress = parseInt(data.loaded / data.total * 100, 10);
                                $('.bar', progressBar).css('width', progress + '%').find('span').html(progress + '%');
                        },
                        done: function (e, data) {
                            if (data.result) {
                                $.each(data.result, function (index, file) {
                                    self.handleActionBarAddItemClick(self.itemsCount-1, function (item) {
                                        var val = item.getValue();
                                        if (self.urlfield == ""){
                                            val = file.url;
                                        }  else {
                                            val[self.urlfield] = file.url;
                                        }
                                        item.setValue(val);
                                        
                                    });
                                    self.itemsCount++;
                                });
                            }
                        }
                    }).data('loaded', true);
                    }
                }
                callback();
            });
        },
        getTitle: function () {
            return "Multi Upload";
        },
        getDescription: function () {
            return "Multi Upload for images and files";
        },

        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "validateAddress": {
                        "title": "Address Validation",
                        "description": "Enable address validation if true",
                        "type": "boolean",
                        "default": true
                    },
                    "showMapOnLoad": {
                        "title": "Whether to show the map when first loaded",
                        "type": "boolean"
                    }
                }
            });
        },

        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "validateAddress": {
                        "helper": "Address validation if checked",
                        "rightLabel": "Enable Google Map for address validation?",
                        "type": "checkbox"
                    }
                }
            });
        }

    });

    Alpaca.registerFieldClass("multiupload", Alpaca.Fields.MultiUploadField);

})(jQuery);