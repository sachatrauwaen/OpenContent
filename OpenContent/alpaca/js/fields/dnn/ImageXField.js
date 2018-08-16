(function ($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.ImageXField = Alpaca.Fields.ListField.extend(
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        getFieldType: function () {
            return "imagex";
        },
        setup: function () {
            var self = this;
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.uploadhidden) {
                this.options.uploadhidden = false;
            }
            if (!this.options.overwrite) {
                this.options.overwrite = false;
            }
            if (!this.options.showOverwrite) {
                this.options.showOverwrite = false;
            }
            if (this.options.showCropper === undefined) {
                this.options.showCropper = true;
            }
            if (this.options.showCropper) {
                this.options.showImage = true;
            }
            if (this.options.showImage === undefined) {
                this.options.showImage = true;
            }
            if (this.options.showCropper) {
                if (!this.options.cropfolder) {
                    this.options.cropfolder = this.options.uploadfolder;
                }
                if (!this.options.cropper) {
                    this.options.cropper = {};
                }
                if (this.options.width && this.options.height) {
                    this.options.cropper.aspectRatio = this.options.width / this.options.height;
                }
                this.options.cropper.responsive = false;
                if (!this.options.cropper.autoCropArea) {
                    this.options.cropper.autoCropArea = 1;
                }
                 if (!this.options.cropper.viewMode) {
                    this.options.cropper.viewMode = 1;
                }
                if (!this.options.cropper.zoomOnWheel) {
                    this.options.cropper.zoomOnWheel = false;
                }
                if (!this.options.cropButtonHidden) {
                    this.options.cropButtonHidden = false;
                }
                if (!this.options.cropButtonHidden) {
                    this.options.buttons = {
                        "check": {
                            "value": "Crop Image",
                            "click": function () {
                                this.cropImage();
                            }
                        }
                    };
                }
            }
            this.base();
        },
        getValue: function () {
            var self = this;
            if (this.control && this.control.length > 0) {
                var value = null;
                $image = self.getImage();
                value = {};
                if (this.options.showCropper) {
                    if (self.cropperExist())
                        value = $image.cropper('getData', { rounded: true });
                }
                value.url = $(this.control).find('select').val();
                if (value.url) {
                    if (this.dataSource && this.dataSource[value.url]) {
                        value.id = this.dataSource[value.url].id;
                        value.filename = this.dataSource[value.url].filename;
                    }
                    if (this.options.showCropper) {
                        value.cropUrl = $(self.getControlEl()).attr('data-cropurl');
                    }
                }
                return value;
            }
        },
        setValue: function (val) {
            var self = this;
            if (val !== this.getValue()) {
                if (this.control && typeof (val) != "undefined" && val != null) {
                    //this.base(val); ???
                    if (Alpaca.isEmpty(val)) {
                        $image.attr('src', url);
                        if (this.options.showCropper) {
                            self.cropper("");
                            $(self.getControlEl()).attr('data-cropurl', '');
                        }
                        $(this.control).find('select').val("");
                    }
                    else if (Alpaca.isObject(val)) {
                        // Fix for OC data that still has the Cachebuster SQ parameter
                        if (val.url) val.url = val.url.split("?")[0];
                        $image.attr('src', val.url);
                        if (this.options.showCropper) {
                            if (val.cropUrl) val.cropUrl = val.cropUrl.split("?")[0];
                            if (val.cropdata && Object.keys(val.cropdata).length > 0) { // compatibility with imagecropper
                                var firstcropdata = val.cropdata[Object.keys(val.cropdata)[0]];
                                self.cropper(val.url, firstcropdata.cropper);
                                $(self.getControlEl()).attr('data-cropurl', firstcropdata.url);
                            } else {
                                self.cropper(val.url, val);
                                $(self.getControlEl()).attr('data-cropurl', val.cropUrl);
                            }
                        }
                         $(this.control).find('select').val(val.url);
                    }
                    else {
                        $image.attr('src', val);
                        if (this.options.showCropper) {
                            self.cropper(val);
                            $(self.getControlEl()).attr('data-cropurl', '');
                        }
                        $(this.control).find('select').val(val);
                    }
                    $(this.control).find('select').trigger('change.select2');
                }
            }
        },
        beforeRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                self.selectOptions = [];
                if (self.sf) {
                    var completionFunction = function () {
                        self.schema.enum = [];
                        self.options.optionLabels = [];
                        for (var i = 0; i < self.selectOptions.length; i++) {
                            self.schema.enum.push(self.selectOptions[i].value);
                            self.options.optionLabels.push(self.selectOptions[i].text);
                        }
                        // push back to model
                        model.selectOptions = self.selectOptions;
                        callback();
                    };
                    var postData = { q: "*", folder: self.options.uploadfolder };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookupExt",
                        beforeSend: self.sf.setModuleHeaders,
                        type: "get",
                        dataType: "json",
                        //contentType: "application/json; charset=utf-8",
                        data: postData,
                        success: function (jsonDocument) {
                            var ds = jsonDocument;
                            if (self.options.dsTransformer && Alpaca.isFunction(self.options.dsTransformer)) {
                                ds = self.options.dsTransformer(ds);
                            }
                            if (ds) {
                                if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.url,
                                            "thumbUrl": value.thumbUrl,
                                            "id": value.id,
                                            "text": value.text,
                                            "filename": value.filename,
                                        });
                                        self.dataSource[value.url] = value;
                                    });
                                    completionFunction();
                                }
                            }
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            self.errorCallback({
                                "message": "Unable to load data from uri : " + self.options.dataSource,
                                "stage": "DATASOURCE_LOADING_ERROR",
                                "details": {
                                    "jqXHR": jqXHR,
                                    "textStatus": textStatus,
                                    "errorThrown": errorThrown
                                }
                            });
                        }
                    });
                } else {
                    callback();
                }
            });
        },

        prepareControlModel: function (callback) {
            var self = this;
            this.base(function (model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },
        afterRenderControl: function (model, callback) {
            var self = this;
            this.base(model, function () {
                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0) {
                    self.data = self.selectOptions[0].value;
                }
                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data) {
                    self.setValue(self.data);
                }

                if ($.fn.select2) {
                    var settings = null;
                    if (self.options.select2) {
                        settings = self.options.select2;
                    }
                    else {
                        settings = {};
                    }
                    settings.templateResult = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 45px;width: 54px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].thumbUrl + '" style="height: 15px;width: 18px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl().find('select')).select2(settings);
                }
                if (self.options.uploadhidden) {
                    $(self.getControlEl()).find('input[type=file]').hide();
                } else {
                    if (self.sf) {
                        
                        $(self.getControlEl()).find('input[type=file]').fileupload({
                            dataType: 'json',
                            url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                            maxFileSize: 25000000,
                            formData: function () {
                                return [
                                    { name: 'uploadfolder', value: self.options.uploadfolder },
                                    { name: 'overwrite', value: self.isOverwrite() },
                                ]
                                //{ uploadfolder: self.options.uploadfolder, overwrite: self.isOverwrite() }
                            },
                            beforeSend: self.sf.setModuleHeaders,
                            add: function (e, data) {
                                self.showAlert('File uploading...');
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
                                        if (file.success) {
                                            self.refresh(function () {
                                                self.setValue(file.url);
                                                self.showAlert('File uploaded', true);
                                            }); 
                                        } else {
                                            self.showAlert(file.message, true);
                                        }
                                    });
                                }
                                
                            }
                        }).data('loaded', true);
                    }
                }

                if (!self.options.showOverwrite) {
                    $(self.control).parent().find('#' + self.id + '-overwrite').hide();
                }
                callback();
            });
        },
        cropImage: function () {
            var self = this;
            var data = self.getValue();
            var postData = { url: data.url, cropfolder: self.options.cropfolder, crop: data, id: "crop" };
            if (self.options.width && self.options.height) {
                postData.resize = { width: self.options.width, height: self.options.height };
            }
            $(self.getControlEl()).css('cursor', 'wait');
            self.showAlert('Image cropping...');
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/CropImage",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(postData),
                beforeSend: self.sf.setModuleHeaders
            }).done(function (res) {

                $(self.getControlEl()).attr('data-cropurl', res.url);
                self.showAlert('Image cropped', true);
                setTimeout(function () {
                    $(self.getControlEl()).css('cursor', 'initial');
                }, 500);
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
                $(self.getControlEl()).css('cursor', 'initial');
            });
        },
        getFileUrl: function (fileid) {
            var self = this;
            if (self.sf) {
                var postData = { fileid: fileid };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "FileUrl",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych: false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        return data;
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        return "";
                    }
                });
            }
        },
        cropper: function (url, data) {
            var self = this;
            $image = self.getImage();
            
            var cropperExist = $image.data('cropper');
            if (url) {
                $image.show();
                if (!cropperExist) {
                    var config = $.extend({}, {
                        aspectRatio: 16 / 9,
                        checkOrientation: false,
                        autoCropArea: 0.90,
                        minContainerHeight: 200,
                        minContainerWidth: 400,
                        toggleDragModeOnDblclick: false
                    }, self.options.cropper);
                    if (data) {
                        config.data = data;
                    }
                    $image.cropper(config);
                } else {
                    if (url != cropperExist.originalUrl || (cropperExist.url && url != cropperExist.url)) {
                        $image.cropper('replace', url);
                    }
                    //$image.cropper('reset');
                    if (data) {
                        $image.cropper('setData', data);
                    }
                }
            } else {
                $image.hide();
                if (!cropperExist) {

                } else {
                    $image.cropper('destroy');
                }
            }
        },
        cropperExist: function () {
            var self = this;
            $image = self.getImage();
            var cropperData = $image.data('cropper');

            return cropperData;
        },
        getImage: function () {
            var self = this;
            return $(self.control).parent().find('#' + self.id + '-image'); //.find('.alpaca-image-display > img');

        },
        isOverwrite: function () {
            var self = this;
            if (this.options.showOverwrite) {
                var checkbox = $(self.control).parent().find('#' + self.id + '-overwrite');
                return Alpaca.checked(checkbox);
            } else {
                return this.options.overwrite;
            }
        },
        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function () {
            var _this = this;

            if (this.schema["enum"]) {
                var val = this.data ? this.data.url : "";

                if (!this.isRequired() && Alpaca.isValEmpty(val)) {
                    return true;
                }

                if (this.options.multiple) {
                    var isValid = true;

                    if (!val) {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val)) {
                        val = [val];
                    }

                    $.each(val, function (i, v) {

                        if ($.inArray(v, _this.schema["enum"]) <= -1) {
                            isValid = false;
                            return false;
                        }

                    });

                    return isValid;
                }
                else {
                    return ($.inArray(val, this.schema["enum"]) > -1);
                }
            }
            else {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function (e) {
            this.base(e);
            var _this = this;
            Alpaca.later(25, this, function () {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function (onFocusCallback) {
            if (this.control && this.control.length > 0) {
                // set focus onto the select
                var el = $(this.control).find('select');

                el.focus();

                if (onFocusCallback) {
                    onFocusCallback(this);
                }
            }
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function () {
            return "Image Crop 2 Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function () {
            return "Image Crop 2 Field";
        },
        showAlert: function (text, time) {
            var self =this;
            $('#' + self.id + '-alert').text(text);
            $('#' + self.id + '-alert').show();
            if(time){
                setTimeout( function (text) {
                    $('#' + self.id + '-alert').hide();
                }, 2000);
            }
        },
    });

    Alpaca.registerFieldClass("imagex", Alpaca.Fields.ImageXField);

})(jQuery);