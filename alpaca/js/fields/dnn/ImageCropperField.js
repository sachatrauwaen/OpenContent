(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.ImageCropperField = Alpaca.Fields.TextField.extend(
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
            return "imagecropper";
        }
        ,
        setup: function () {
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            if (!this.options.cropper) {
                this.options.cropper = {};
            }
            this.options.cropper.responsive = false;
            if (!this.options.cropper.autoCropArea) {
                this.options.cropper.autoCropArea = 1;
            }
            this.base();
        },

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Image Cropper Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Image Cropper Field.";
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
                else if (Alpaca.isString(value)) {
                    el.val(value);
                }
                else {
                    el.val(value.url);
                    this.setCroppedData(value.cropdata);
                }
            }
            // be sure to call into base method
            //this.base(textvalue);

            // if applicable, update the max length indicator
            this.updateMaxLengthIndicator();
        },

        getValue: function () {
            var value = null;
            var el = this.getControlEl();
            if (el && el.length > 0) {
                //value = el.val();
                value = {
                    url: el.val()
                };
                value.cropdata = this.getCroppedData();
            }
            return value;
        },
        getCroppedData: function () {
            var el = this.getControlEl();
            var cropdata = {};
            for (var i in this.options.croppers) {
                var cropper = this.options.croppers[i];
                var id = this.id + '-' + i;
                var $cropbutton = $('#' + id);
                cropdata[i] = $cropbutton.data('cropdata');
            }
            return cropdata;
        },
        cropAllImages: function (url) {
            var self = this;
            for (var i in this.options.croppers) {

                var id = this.id + '-' + i;
                var $cropbutton = $('#' + id);

                //cropdata[i] = $cropbutton.data('cropdata');

                var cropopt = this.options.croppers[i];

                var crop = { "x": -1, "y": -1, "width": cropopt.width, "height": cropopt.height, "rotate": 0 };
                var postData = JSON.stringify({ url: url, id: i, crop: crop, resize: cropopt, cropfolder: this.options.cropfolder });

                var action = "CropImage";
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    async: false,
                    data: postData,
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (res) {
                    var cropdata = { url: res.url, cropper: {} };
                    self.setCroppedDataForId(id, cropdata);

                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });

            }
            //var data = $image.cropper('getData', { rounded: true });
            //var cropperId = cropButton.data('cropperId');

        },
        setCroppedData: function (value) {

            var el = this.getControlEl();
            var parentel = this.getFieldEl();
            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {

                }
                else {
                    var firstCropButton;
                    for (var i in this.options.croppers) {
                        var cropper = this.options.croppers[i];
                        var id = this.id + '-' + i;
                        var $cropbutton = $('#' + id);
                        cropdata = value[i];
                        if (cropdata) {
                            $cropbutton.data('cropdata', cropdata);
                        }

                        if (!firstCropButton) {
                            firstCropButton = $cropbutton;
                            $(firstCropButton).addClass('active');
                            if (cropdata) {
                                var $image = $(parentel).find('.alpaca-image-display img.image');
                                var cropper = $image.data('cropper');
                                if (cropper) {
                                    $image.cropper('setData', cropdata.cropper);
                                }
                            }
                        }

                    }
                }
            }

            /*
            var el = this.getControlEl();
            var $image = el.parent().find('.image');
            if (el && el.length > 0) {
                if (Alpaca.isEmpty(value)) {
                    $image.data('cropdata', {});
                }
                else {
                    $image.data('cropdata', value);
                }
            }
            */
        },

        setCroppedDataForId: function (id, value) {
            var el = this.getControlEl();
            if (value) {
                var $cropbutton = $('#' + id);
                $cropbutton.data('cropdata', value);
            }
        },
        getCurrentCropData: function () {
            /*
            var el = this.getControlEl();
            var curtab = $(el).parent().parent().find(".alpaca-form-tab.active");
            var cropdata = $(this).data('cropdata');
            */

            var el = this.getFieldEl(); //this.getControlEl();
            var curtab = $(el).parent().find(".alpaca-form-tab.active");
            var cropdata = $(curtab).data('cropdata');
            return cropdata;
        },
        setCurrentCropData: function (value) {
            var el = this.getFieldEl(); //this.getControlEl();
            var curtab = $(el).parent().find(".alpaca-form-tab.active");
            $(curtab).data('cropdata', value);

        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.handlePostRender(function () {
                    callback();
                });
            });
        },
        cropChange: function (e) {
            var self = e.data;
            //var parentel = this.getFieldEl();

            var currentCropdata = self.getCurrentCropData();
            if (currentCropdata) {
                var cropper = currentCropdata.cropper;
                var $image = this; //$(parentel).find('.alpaca-image-display img.image');
                var data = $(this).cropper('getData', { rounded: true });
                if (data.x != cropper.x ||
                    data.y != cropper.y ||
                    data.width != cropper.width ||
                    data.height != cropper.height ||
                    data.rotate != cropper.rotate) {

                    var cropdata = {
                        url: "",
                        cropper: data
                    };
                    self.setCurrentCropData(cropdata);
                }
            }
            //self.setCroppedDataForId(cropperButtonIdcropButton.data('cropperButtonId'), cropdata);

        },
        getCropppersData: function () {
            for (var i in self.options.croppers) {
                var cropper = self.options.croppers[i];
                var id = self.id + '-' + i;

            }
        },
        handlePostRender: function (callback) {
            var self = this;
            var el = this.getControlEl();
            var parentel = this.getFieldEl();

            var cropButton = $('<a href="#" class="alpaca-form-button">Crop</a>');//.appendTo($(el).parent());
            cropButton.click(function () {
                /*
                var data = $image.cropper('getData', { rounded: true });
                var cropperId = cropButton.data('cropperId');
                var cropopt = self.options.croppers[cropperId];
                var postData = JSON.stringify({ url: el.val(), id: cropperId, crop: data, resize: cropopt });
                */
                var data = self.getCroppedData();
                var postData = JSON.stringify({ url: el.val(), cropfolder: self.options.cropfolder, cropdata: data, croppers: self.options.croppers });


                $(cropButton).css('cursor', 'wait');

                var action = "CropImages";
                $.ajax({
                    type: "POST",
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: postData,
                    beforeSend: self.sf.setModuleHeaders
                }).done(function (res) {
                    /*
                    var cropdata = { url: res.url, cropper: data };
                    self.setCroppedDataForId(cropButton.data('cropperButtonId'), cropdata);
                    */
                    for (var i in self.options.croppers) {
                        var cropper = self.options.croppers[i];
                        var id = self.id + '-' + i;
                        var $cropbutton = $('#' + id);
                        var cropdata = { url: res.cropdata[i].url, cropper: res.cropdata[i].crop };
                        if (cropdata) {
                            $cropbutton.data('cropdata', cropdata);
                        }
                    }
                    setTimeout(function () {
                        $(cropButton).css('cursor', 'default');
                    }, 500);
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                    $(parentel).css('cursor', 'default');
                });
                return false;
            });

            var firstCropButton;
            for (var i in self.options.croppers) {
                var cropper = self.options.croppers[i];
                var id = self.id + '-' + i;
                var cropperButton = $('<a id="' + id + '" data-id="' + i + '" href="#" class="alpaca-form-tab" >' + i + '</a>').appendTo($(el).parent());
                cropperButton.data('cropopt', cropper);
                cropperButton.click(function () {
                    $image.off('change.cropper');

                    var cropdata = $(this).data('cropdata');
                    var cropopt = $(this).data('cropopt');
                    $image.cropper('setAspectRatio', cropopt.width / cropopt.height);
                    if (cropdata) {
                        $image.cropper('setData', cropdata.cropper);
                    } else {
                        $image.cropper('reset');
                    }
                    cropButton.data('cropperButtonId', this.id);
                    cropButton.data('cropperId', $(this).attr("data-id"));

                    $(this).parent().find('.alpaca-form-tab').removeClass('active');
                    $(this).addClass('active');

                    $image.on('change.cropper', self, self.cropChange);

                    return false;
                });
                if (!firstCropButton) {
                    firstCropButton = cropperButton;
                    $(firstCropButton).addClass('active');
                    cropButton.data('cropperButtonId', $(firstCropButton).attr('id'));
                    cropButton.data('cropperId', $(firstCropButton).attr("data-id"));
                }
            }

            var $image = $(parentel).find('.alpaca-image-display img.image');
            $image.cropper(self.options.cropper).on('built.cropper', function () {
                var cropopt = $(firstCropButton).data('cropopt');
                if (cropopt) {
                    $(this).cropper('setAspectRatio', cropopt.width / cropopt.height);
                }
                var cropdata = $(firstCropButton).data('cropdata');
                if (cropdata) {
                    $(this).cropper('setData', cropdata.cropper);
                }
                var $image = $(parentel).find('.alpaca-image-display img.image');
                $image.on('change.cropper', self, self.cropChange);
            });

            if (self.options.uploadhidden) {
                $(this.control.get(0)).find('input[type=file]').hide();
            } else {
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
                                //self.setValue(file.url);
                                el.val(file.url);

                                $(el).change();
                                //$(el).change();
                                //$(e.target).parent().find('input[type=text]').val(file.url);
                                //el.val(file.url);
                                //$(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                            });
                        }
                    }
                }).data('loaded', true);
            }
            $(el).change(function () {

                var value = $(this).val();

                //var newValue = $(el).typeahead('val');
                //if (newValue !== value) {
                $(parentel).find('.alpaca-image-display img.image').attr('src', value);
                $image.cropper('replace', value);
                if (value) {
                    self.cropAllImages(value);
                }

                //}

            });
            cropButton.appendTo($(el).parent());
            if (self.options.manageurl) {
                var manageButton = $('<a href="' + self.options.manageurl + '" target="_blank" class="alpaca-form-button">Manage files</a>').appendTo($(el).parent());
            }


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
                    url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/Images?q=%QUERY&d=" + tFolder,
                    ajax: {
                        beforeSend: connector.servicesFramework.setModuleHeaders,

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
                    "suggestion": "<div style='width:20%;display:inline-block;background-color:#fff;padding:2px;'><img src='{{value}}' style='height:40px' /></div> {{name}}"
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
                    //self.setValue(datum.value);
                    el.val(datum.value);
                    $(el).change();
                    //$(self.control).parent().find('input[type=text]').val(datum.value);
                    //$(self.control).parent().find('.alpaca-image-display img').attr('src', datum.value);
                });

                // listen for "selected" event and set the value of the field
                $(el).on("typeahead:selected", function (event, datum) {
                    //self.setValue(datum.value);
                    el.val(datum.value);
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

    Alpaca.registerFieldClass("imagecropper", Alpaca.Fields.ImageCropperField);

})(jQuery);