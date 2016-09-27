(function ($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.ImageCrop2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.ImageCrop2Field.prototype
     */
    {
        constructor: function (container, data, options, schema, view, connector) {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
            this.dataSource = {};
        },
        /**
         * @see Alpaca.Field#getFieldType
         */
        getFieldType: function () {
            return "imagecrop2";
        },
        /**
         * @see Alpaca.Fields.ImageCrop2Field#setup
         */
        setup: function () {
            var self = this;
            if (!this.options.uploadfolder) {
                this.options.uploadfolder = "";
            }
            if (!this.options.cropfolder) {
                this.options.cropfolder = this.options.uploadfolder;
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
            if (!this.options.cropButtonHidden) {
                this.options.cropButtonHidden = false;
            }
            if (!this.options.cropButtonHidden) {
                this.options.buttons = {
                    "check": {
                        "value": "Crop",
                        "click": function () {
                            this.cropImage();
                        }
                    }
                };
            }
            this.base();
        },
        getValue: function () {
            var self = this;
            if (this.control && this.control.length > 0) {
                /*
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                var url = this.base(val);
                */
                var value = null;
                $image = self.getImage();
                if (self.cropperExist())
                    value = $image.cropper('getData', { rounded: true });
                else
                    value = {};

                value.url = $(this.control).find('select').val();
                value.cropUrl = $(self.getControlEl()).attr('data-cropurl');
                return value;
            }
        },
        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function (val) {
            var self = this;
            if (val !== this.getValue()) {
                /*
                if (!Alpaca.isEmpty(val) && this.control)
                {
                    this.control.val(val);
                }
                */
                if (this.control && typeof (val) != "undefined" && val != null) {
                    //this.base(val); ???
                    if (Alpaca.isEmpty(val)) {
                        self.cropper("");
                        $(this.control).find('select').val("");
                        $(self.getControlEl()).attr('data-cropurl', '');
                    }
                    else if (Alpaca.isObject(val)) {
                        if (val.cropdata && Object.keys(val.cropdata).length > 0) { // compatibility with imagecropper
                            var firstcropdata = val.cropdata[Object.keys(val.cropdata)[0]];
                            self.cropper(val.url, firstcropdata.cropper);
                            $(this.control).find('select').val(val.url);
                            $(self.getControlEl()).attr('data-cropurl', firstcropdata.url);
                        } else {
                            self.cropper(val.url, val);
                            $(this.control).find('select').val(val.url);
                            $(self.getControlEl()).attr('data-cropurl', val.cropUrl);
                        }
                    }
                    else {
                        self.cropper(val);
                        $(this.control).find('select').val(val);
                        $(self.getControlEl()).attr('data-cropurl', '');
                    }
                    $(this.control).find('select').trigger('change.select2');
                }
            }
        },

        /**
         * @see Alpaca.ImageCrop2Field#getEnum
         */
        getEnum: function () {
            if (this.schema) {
                if (this.schema["enum"]) {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"]) {
                    return this.schema["items"]["enum"];
                }
            }
        },

        initControlEvents: function () {
            var self = this;
            self.base();
            if (self.options.multiple) {
                var button = this.control.parent().find(".select2-search__field");
                button.focus(function (e) {
                    if (!self.suspendBlurFocus) {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });
                button.blur(function (e) {
                    if (!self.suspendBlurFocus) {
                        self.onBlur.call(self, e);
                        self.trigger("blur", e);
                    }
                });
                this.control.on("change", function (e) {
                    self.onChange.call(self, e);
                    self.trigger("change", e);
                });
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
                                            "text": value.text
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
                    /*
                    if (!settings.nonSelectedText)
                    {
                        settings.nonSelectedText = "None";
                        if (self.options.noneLabel)
                        {
                            settings.nonSelectedText = self.options.noneLabel;
                        }
                    }
                    if (self.options.hideNone)
                    {
                        delete settings.nonSelectedText;
                    }
                    */

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
                                        self.refresh(function () {
                                            self.setValue(file.url);
                                        });                                       
                                    });
                                }
                            }
                        }).data('loaded', true);
                    }
                }


                callback();
            });
        },
        cropImage: function () {
            var self = this;
            var data = self.getValue();
            var postData = JSON.stringify({ url: data.url, cropfolder: self.options.cropfolder, crop: data, id: "crop" });
            $(self.getControlEl()).css('cursor', 'wait');
            $.ajax({
                type: "POST",
                url: self.sf.getServiceRoot('OpenContent') + "DnnEntitiesAPI/CropImage",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: self.sf.setModuleHeaders
            }).done(function (res) {

                $(self.getControlEl()).attr('data-cropurl', res.url);

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
            $image.attr('src', url);
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

    });

    Alpaca.registerFieldClass("imagecrop2", Alpaca.Fields.ImageCrop2Field);

})(jQuery);