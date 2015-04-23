(function ($) {

    var Alpaca = $.alpaca;

    Alpaca.Fields.ImageField = Alpaca.Fields.TextField.extend(
    /**
     * @lends Alpaca.Fields.ImageField.prototype
     */
    {
        constructor: function(container, data, options, schema, view, connector)
        {
            var self = this;
            this.base(container, data, options, schema, view, connector);
            this.sf = connector.servicesFramework;
        },

        /**
         * @see Alpaca.Fields.TextField#getFieldType
         */
        getFieldType: function () {
            return "image";
        }

        /* builder_helpers */
        ,

        /**
         * @see Alpaca.Fields.TextField#getTitle
         */
        getTitle: function () {
            return "Image Field";
        },

        /**
         * @see Alpaca.Fields.TextField#getDescription
         */
        getDescription: function () {
            return "Image Field.";
        },
        setValue: function (value) {

            //var el = $( this.control).filter('#'+this.id);
            var el = $(this.control.get(0)).find('input[type=text]');
            

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
            var el = $(this.control.get(0)).find('input[type=text]');

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

            var el = this.control;
            
            $(el.get(0)).find('input[type=file]').fileupload({
                dataType: 'json',
                url: self.sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                maxFileSize: 25000000,
                formData: { example: 'test' },
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
                            //$('<p/>').text(file.name).appendTo($(e.target).parent().parent());
                            //$('<img/>').attr('src', file.url).appendTo($(e.target).parent().parent());

                            $(e.target).parent().find('input[type=text]').val(file.url);
                            $(e.target).parent().find('.alpaca-image-display img').attr('src', file.url);
                        });
                    }
                }
            }).data('loaded', true);
            
            callback();
        },

        /* end_builder_helpers */
    });

    Alpaca.registerFieldClass("image", Alpaca.Fields.ImageField);

})(jQuery);