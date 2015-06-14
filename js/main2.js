requirejs(['jquery', 'handlebars', 'bootstrap', 'alpaca'],
function ($, handlebars, bt) {


    var dataObject = {
        members: [
          { id: 1, name: "hoge", text: "aaaaaaaaaaaaaa" },
          { id: 9, name: "fuga", text: "bbbbbbbbbbbbbb" },
          { id: 15, name: "hoge", text: "cccccccccccccc" },
          { id: 22, name: "fuga", text: "dddddddddddddd" },
          { id: 78, name: "hoge", text: "eeeeeeeeeeeeee" },
          { id: 876, name: "fuga", text: "ffffffffffffff" },
          { id: 1033, name: "hoge", text: "gggggggggggggg" },
          { id: 7899, name: "fuga", text: "hhhhhhhhhhhhhh" }
        ]
    };

    //
    var tpl_source = $("#item_tmpl").html();
    var h = Handlebars.compile(tpl_source);
    var content = h(dataObject);

    // output
    var results = document.getElementById("content");
    results.innerHTML = content;


    //var $modal = $("#iPopUp");
    //$('#content').html(JSON.stringify(window.frameElement.dnn));


});