/*globals jQuery, window, Sys */
(function ($, Sys) {

    function setup() {
        $(document).trigger("opencontent.ready");
    }

    $(document).ready(function () {
        setup();
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            setup();
        });
    });

}(jQuery, window.Sys));
