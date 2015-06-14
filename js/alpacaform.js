define(["jquery", "alpaca", "bootstrap"], function ($) {

    return {
        dnn: {},
        sf: {},
        culture : "",
        createform: function (dnn) {
            var self = this;
            self.dnn = dnn;
            self.sf = dnn.sf;
            self.culture = "fr";
            sf = dnn.sf;
            
            var postData = {};
            var getData = "";
            var action = "Edit"; //self.getUpdateAction();

            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                data: getData,
                beforeSend: sf.setModuleHeaders
            }).done(function (config) {

                var ConnectorClass = Alpaca.getConnectorClass("default");
                connector = new ConnectorClass("default");
                connector.servicesFramework = sf;
                connector.culture = self.culture;

                $.alpaca.Fields.DnnFileField = $.alpaca.Fields.FileField.extend({
                    setup: function () {
                        this.base();
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
                        //var self = this;

                        var el = this.control;
                        self.SetupFileUpload(el);
                        callback();
                    }
                });
                Alpaca.registerFieldClass("file", Alpaca.Fields.DnnFileField);

                $("#content").alpaca({
                    "schema": config.schema,
                    "options": config.options,
                    "data": config.data,
                    "view": "dnnbootstrap-edit",
                    "connector": connector,
                    "postRender": function (control) {
                        var selfControl = control;
                        $("#cmdSave").click(function () {
                            selfControl.refreshValidationState(true);
                            if (selfControl.isValid(true)) {
                                var value = selfControl.getValue();
                                //alert(JSON.stringify(value, null, "  "));
                                var href = $(this).attr('href');
                                self.FormSubmit(value, href);
                            }
                            return false;
                        });
                    }
                });
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        },

        FormSubmit: function (data, href) {
            var self = this;
            //var postData = { form: data };
            var postData = JSON.stringify({ form: data });
            var action = "Update"; //self.getUpdateAction();
            var sf = self.sf;

            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                //alert('ok:' + data);
                //self.loadSettings();
                self.dnn.refresh();

                //var popup = window.frameElement.contentWindow.dnnModal.close();
                //$(this).closest("#iPopUp").dialog('close');
                window.parent.jQuery('#iPopUp').dialog('close');

                //window.location.href = href;
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        },

        SetupFileUpload: function (fileupload) {
            //$('#field1 input[type="file"]')
            $(fileupload).fileupload({
                dataType: 'json',
                url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                maxFileSize: 25000000,
                formData: { example: 'test' },
                beforeSend: sf.setModuleHeaders,
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
                            $(e.target).closest('.alpaca-container').find('.alpaca-field-image input').val(file.url);
                            $(e.target).closest('.alpaca-container').find('.alpaca-image-display img').attr('src', file.url);
                        });
                    }
                }
            }).data('loaded', true);
        }
    };
});