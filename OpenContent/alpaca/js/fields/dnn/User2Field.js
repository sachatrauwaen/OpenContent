(function($) {

    var Alpaca = $.alpaca;
    
    Alpaca.Fields.User2Field = Alpaca.Fields.ListField.extend(
    /**
     * @lends Alpaca.Fields.User2Field.prototype
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
        getFieldType: function()
        {
            return "select";
        },

        /**
         * @see Alpaca.Fields.User2Field#setup
         */
        setup: function()
        {
            var self = this;
            if (self.schema["type"] && self.schema["type"] === "array") {
                self.options.multiple = true;
                self.options.removeDefaultNone = true;
                //self.options.hideNone = true;
            }
            if (!this.options.folder) {
                this.options.folder = "";
            }
       
            if (!this.options.role) {
                this.options.role = "";
            }
            var self = this;
            if (this.options.lazyLoading) {
                var pageSize = 10;
                this.options.select2 = {
                    ajax: {
                        url: this.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "UsersLookup",
                        beforeSend: this.sf.setModuleHeaders,
                        type: "get",
                        dataType: 'json',
                        delay: 250,
                        data: function (params) {
                            return {
                                q: params.term ? params.term : "", // search term
                                d: self.options.folder, 
                                role: self.options.role,
                                pageIndex: params.page ? params.page : 1,
                                pageSize: pageSize
                            };
                        },
                        processResults: function (data, params) {
                            params.page = params.page || 1;
                            if (params.page == 1) {
                                data.items.unshift({
                                    id: "",
                                    text: self.options.noneLabel
                                })
                            }
                            return {
                                results: data.items,
                                pagination: {
                                    more: (params.page * pageSize) < data.total
                                }
                            };
                        },
                        cache: true
                    },
                    escapeMarkup: function (markup) { return markup; },
                    minimumInputLength: 0
                }
            };
            this.base();
        },

        getValue: function () {
            if (this.control && this.control.length > 0) {
                var val = this._getControlVal(true);
                if (typeof (val) === "undefined") {
                    val = this.data;
                }
                else if (Alpaca.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        val[i] = this.ensureProperType(val[i]);
                    }
                }
                return this.base(val);
            }
        },

        /**
         * @see Alpaca.Field#setValue
         */
        setValue: function (val) {
            if (Alpaca.isArray(val)) {
                if (!Alpaca.compareArrayContent(val, this.getValue())) {
                    if (!Alpaca.isEmpty(val) && this.control) {
                        this.control.val(val);
                    }
                    this.base(val);
                }
            }
            else {
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
                }
            }
        },

        /**
         * @see Alpaca.User2Field#getEnum
         */
        getEnum: function()
        {
            if (this.schema)
            {
                if (this.schema["enum"])
                {
                    return this.schema["enum"];
                }
                else if (this.schema["type"] && this.schema["type"] === "array" && this.schema["items"] && this.schema["items"]["enum"])
                {
                    return this.schema["items"]["enum"];
                }
            }
        },
        /*
        initControlEvents: function()
        {
            var self = this;

            self.base();

            if (self.options.multiple)
            {
                var button = this.control.parent().find(".select2-search__field");

                button.focus(function(e) {
                    if (!self.suspendBlurFocus)
                    {
                        self.onFocus.call(self, e);
                        self.trigger("focus", e);
                    }
                });

                button.blur(function(e) {
                    if (!self.suspendBlurFocus)
                    {
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
        */
        beforeRenderControl: function(model, callback)
        {
            var self = this;

            this.base(model, function() {
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
                    if (self.options.lazyLoading) {
                        if (self.data) {
                            self.getUserName(self.data, function (data) {
                                self.selectOptions.push({
                                    "value": self.data,
                                    "text": data.text
                                });
                                self.dataSource[self.data] = data.text;
                                completionFunction();
                            });
                        } else {
                            completionFunction();
                        }
                    }
                    else {
                        var postData = { q: "", role: self.options.role };
                        $.ajax({
                            url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "UsersLookup",
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
                                                "value": value.value,
                                                "text": value.text
                                            });
                                            self.dataSource[value.value] = value;
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
                    }
                }
                else {
                    callback();
                }
            });
        },

        prepareControlModel: function(callback)
        {
            var self = this;
            this.base(function(model) {
                model.selectOptions = self.selectOptions;
                callback(model);
            });
        },

        afterRenderControl: function(model, callback)
        {
            var self = this;
            this.base(model, function() {
                // if emptySelectFirst and nothing currently checked, then pick first item in the value list
                // set data and visually select it
                if (Alpaca.isUndefined(self.data) && self.options.emptySelectFirst && self.selectOptions && self.selectOptions.length > 0)
                {
                    self.data = self.selectOptions[0].value;
                }

                // do this little trick so that if we have a default value, it gets set during first render
                // this causes the state of the control
                if (self.data)
                {
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

                        if (state.loading) return state.text;

                        //if (!state.id) { return state.text; }

                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };

                    settings.templateSelection = function (state) {
                        if (!state.id) { return state.text; }

                        var $state = $(
                            '<span>' + state.text + '</span>'
                        );
                        return $state;
                    };
                    
                    $(self.getControlEl()).select2(settings);
                }

                
                callback();
            });
        },

        getUserName: function (userid, callback) {
            var self = this;
            if (self.sf){
                var postData = { userid: userid };
                $.ajax({
                    url: self.sf.getServiceRoot("OpenContent") + "DnnEntitiesAPI" + "/" + "GetUserInfo",
                    beforeSend: self.sf.setModuleHeaders,
                    type: "get",
                    asych : false,
                    dataType: "json",
                    //contentType: "application/json; charset=utf-8",
                    data: postData,
                    success: function (data) {
                        if (callback) callback(data);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("Error GetUserInfo " + userid);
                    }
                });
            }
        },

        /**
         * Validate against enum property.
         *
         * @returns {Boolean} True if the element value is part of the enum list, false otherwise.
         */
        _validateEnum: function()
        {
            var _this = this;

            if (this.schema["enum"])
            {
                var val = this.data;

                if (!this.isRequired() && Alpaca.isValEmpty(val))
                {
                    return true;
                }

                if (this.options.multiple)
                {
                    var isValid = true;

                    if (!val)
                    {
                        val = [];
                    }

                    if (!Alpaca.isArray(val) && !Alpaca.isObject(val))
                    {
                        val = [val];
                    }

                    $.each(val, function(i,v) {
                        /*
                        if ($.inArray(v, _this.schema["enum"]) <= -1)
                        {
                            isValid = false;
                            return false;
                        }
                        */
                    });

                    return isValid;
                }
                else
                {
                    //return ($.inArray(val, this.schema["enum"]) > -1);
                    return true;
                }
            }
            else
            {
                return true;
            }
        },

        /**
         * @see Alpaca.Field#onChange
         */
        onChange: function(e)
        {
            this.base(e);

            var _this = this;

            Alpaca.later(25, this, function() {
                var v = _this.getValue();
                _this.setValue(v);
                _this.refreshValidationState();
            });
        },

       
        /**
         * @see Alpaca.Field#focus
         */
        focus: function(onFocusCallback)
        {
            if (this.control && this.control.length > 0)
            {
                // set focus onto the select
                var el = $(this.control).get(0);

                el.focus();

                if (onFocusCallback)
                {
                    onFocusCallback(this);
                }
            }
        },
        getTextControlEl: function () {
            var self = this;
            return $(self.getControlEl()).find('input[type=text]');
        },
        
        /**
         * @see Alpaca.Field#getTitle
         */
        getTitle: function() {
            return "Select User Field";
        },

        /**
         * @see Alpaca.Field#getDescription
         */
        getDescription: function() {
            return "Select User Field";
        },

        /**
         * @private
         * @see Alpaca.Fields.User2Field#getSchemaOfOptions
         */
        getSchemaOfOptions: function() {
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
         * @see Alpaca.Fields.User2Field#getOptionsForOptions
         */
        getOptionsForOptions: function() {
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

    Alpaca.registerFieldClass("user2", Alpaca.Fields.User2Field);

})(jQuery);