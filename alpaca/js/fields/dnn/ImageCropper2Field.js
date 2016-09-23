(function ($) {
    var Alpaca = $.alpaca;
    Alpaca.Fields.ImageCropper2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.ImageCropper2Field.prototype
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
            return "imagecropper2";
        },
        /**
         * @see Alpaca.Fields.ImageCropper2Field#setup
         */
        setup: function () {
            var self = this;
            if (!this.options.folder) {
                this.options.folder = "";
            }
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
                    this.control.val(val);
                }
                this.base(val);
                if (Alpaca.isEmpty(val)) {
                    self.cropper("");
                }
                else if (Alpaca.isObject(val)) {
                    self.cropper(val.url, val);
                }
                else {
                    self.cropper(val);
                }
            }
        },

        /**
         * @see Alpaca.ImageCropper2Field#getEnum
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
                    var postData = { q: "*", d: self.options.folder };
                    $.ajax({
                        url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "ImagesLookup",
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
                                if (Alpaca.isObject(ds)) {
                                    // for objects, we walk through one key at a time
                                    // the insertion order is the order of the keys from the map
                                    // to preserve order, consider using an array as below
                                    $.each(ds, function (key, value) {
                                        self.selectOptions.push({
                                            "value": key,
                                            "text": value
                                        });
                                    });
                                    completionFunction();
                                }
                                else if (Alpaca.isArray(ds)) {
                                    // for arrays, we walk through one index at a time
                                    // the insertion order is dictated by the order of the indices into the array
                                    // this preserves order
                                    $.each(ds, function (index, value) {
                                        self.selectOptions.push({
                                            "value": value.url,
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

                // if we are in multiple mode and the bootstrap multiselect plugin is available, bind it in
                //if (self.options.multiple && $.fn.multiselect)
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
                          '<span><img src="' + self.dataSource[state.id].url + '" style="height: 45px;width: 54px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                          '<span><img src="' + self.dataSource[state.id].url + '" style="height: 15px;width: 18px;"  /> ' + state.text + '</span>'
                        );
                        return $state;
                    };

                    $(self.getControlEl().find('select')).select2(settings);

                }

                callback();

            });
        },
        getFileUrl: function (fileid) {
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
                    if (url != cropperExist.originalUrl) {
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
         * Validates if number of items has been less than minItems.
         * @returns {Boolean} true if number of items has been less than minItems
         */
        _validateMinItems: function () {
            if (this.schema.items && this.schema.items.minItems) {
                if ($(":selected", this.control).length < this.schema.items.minItems) {
                    return false;
                }
            }

            return true;
        },

        /**
         * Validates if number of items has been over maxItems.
         * @returns {Boolean} true if number of items has been over maxItems
         */
        _validateMaxItems: function () {
            if (this.schema.items && this.schema.items.maxItems) {
                if ($(":selected", this.control).length > this.schema.items.maxItems) {
                    return false;
                }
            }

            return true;
        },

        /**
         * @see Alpaca.ContainerField#handleValidate
         */
        handleValidate: function () {
            var baseStatus = this.base();

            var valInfo = this.validation;

            var status = this._validateMaxItems();
            valInfo["tooManyItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("tooManyItems"), [this.schema.items.maxItems]),
                "status": status
            };

            status = this._validateMinItems();
            valInfo["notEnoughItems"] = {
                "message": status ? "" : Alpaca.substituteTokens(this.getMessage("notEnoughItems"), [this.schema.items.minItems]),
                "status": status
            };

            return baseStatus && valInfo["tooManyItems"]["status"] && valInfo["notEnoughItems"]["status"];
        },

        /**
         * @see Alpaca.Field#focus
         */
        focus: function (onFocusCallback) {
            if (this.control && this.control.length > 0) {
                // set focus onto the select
                var el = $(this.control).get(0);

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
            return "Select Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function () {
            return "Select Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.ImageCropper2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function () {
            return Alpaca.merge(this.base(), {
                "properties": {
                    "multiple": {
                        "title": "Mulitple Selection",
                        "description": "Allow multiple selection if true.",
                        "type": "boolean",
                        "default": false
                    },
                    "size": {
                        "title": "Displayed Options",
                        "description": "Number of options to be shown.",
                        "type": "number"
                    },
                    "emptySelectFirst": {
                        "title": "Empty Select First",
                        "description": "If the data is empty, then automatically select the first item in the list.",
                        "type": "boolean",
                        "default": false
                    },
                    "multiselect": {
                        "title": "Multiselect Plugin Settings",
                        "description": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect",
                        "type": "any"
                    }
                }
            });
        },

        /**
         * @private
         * @see Alpaca.Fields.ImageCropper2Field#getOptionsForOptions
         */
        getOptionsForOptions: function () {
            return Alpaca.merge(this.base(), {
                "fields": {
                    "multiple": {
                        "rightLabel": "Allow multiple selection ?",
                        "helper": "Allow multiple selection if checked",
                        "type": "checkbox"
                    },
                    "size": {
                        "type": "integer"
                    },
                    "emptySelectFirst": {
                        "type": "checkbox",
                        "rightLabel": "Empty Select First"
                    },
                    "multiselect": {
                        "type": "object",
                        "rightLabel": "Multiselect plugin properties - http://davidstutz.github.io/bootstrap-multiselect"
                    }
                }
            });
        }

        /* end_builder_helpers */

    });

    Alpaca.registerFieldClass("imagecropper2", Alpaca.Fields.ImageCropper2Field);

})(jQuery);