function ocInitCodeMirror(mimeType, schema, options) {

	CodeMirror.defineMode("htmlhandlebars", function (config) {
		return CodeMirror.multiplexingMode(
		  CodeMirror.getMode(config, "text/html"),
		  {
		  	open: "{{", close: "}}",
		  	mode: CodeMirror.getMode(config, "handlebars"),
		  	parseDelimiters: true
		  });
	});


	var handlebarsHints = [
				/*{ 'text': '{{#each Items}}\n\n{{/each}}', 'displayText': '#each', 'sort': '#each' },
				{ 'text': '{{#if var}}\n\n{{/if}}', 'displayText': '#if', 'sort': '#if' }*/
				];

	var addHbsProperties = function (sch, opt, parent, prefix) {
		var hints = [];
		if (sch && sch.properties) {
			for (var k in sch.properties) {
				p = sch.properties[k];
				var vartext = (prefix ? prefix + '.' : '') + k;
				var o = opt && opt.fields ? opt.fields[k] : null;
				if (p.type == 'object') {
					addHbsProperties(p, o, '', k);
					//handlebarsHints.push({'text':'{{#with '+k+'}} {{/with}}', 'displayText': k+ (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
				} else if (p.type == 'array') {
					if (p.type == 'array' && p.items) {
						var childHints = addHbsProperties(p.items, o ? o.items : null, k, '');
						var snipet = '';
						for (i = 0; i < childHints.length; i++) {
							snipet += childHints[i].text + '\n';
						}
						snipet = '{{#each ' + vartext + '}}\n' + snipet + '{{/each}}';
						hints.push({ 'text': snipet, 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else {
						hints.push({ 'text': '{{#each ' + vartext + '}}\n\n{{/each}}', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					}
				} else {
					if (o && o.type == 'image') {
						hints.push({ 'text': '<img src="{{' + vartext + '}}" alt="" />', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else if (o && o.type == 'file') {
						hints.push({ 'text': '<a href="{{' + vartext + '}}" target="_blank" >Download</a>', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else if (o && o.type == 'url') {
						hints.push({ 'text': '<a href="{{' + vartext + '}}"  >More</a>', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else if (o && (o.type == 'ckeditor' || o.type == 'wysihtml')) {
						hints.push({ 'text': '{{{' + vartext + '}}}', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else {
						hints.push({ 'text': '{{' + vartext + '}}', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					}
				}
			}
			for (i = 0; i < hints.length; i++) {
				handlebarsHints.push(hints[i]);
			}
		}
		return hints;
	};

	var addRazorProperties = function (sch, opt, parent, prefix) {
		var hints = [];
		if (sch && sch.properties) {
			for (var k in sch.properties) {
				p = sch.properties[k];
				var varfulltext = (prefix ? prefix + '.' : 'Model.') + k;
				var vartext = (prefix ? prefix + '.' : '') + k;
				var vardisplaytext = (parent ? parent + '.' : '') + k;
				var o = opt && opt.fields ? opt.fields[k] : null;
				if (p.type == 'object') {
					addRazorProperties(p, o, k, varfulltext);
					//handlebarsHints.push({'text':'{{#with '+k+'}} {{/with}}', 'displayText': k+ (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
				} else if (p.type == 'array') {
					if (p.type == 'array' && p.items) {
						var childHints = addRazorProperties(p.items, o ? o.items : null, k, vartext + 'Item');
						var snipet = '';
						for (i = 0; i < childHints.length; i++) {
							snipet += childHints[i].text + '\n';
						}
						snipet = '@foreach(var ' + vartext + 'Item in ' + varfulltext + ') {\n' + snipet + '}';
						hints.push({ 'text': snipet, 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					} else {
						hints.push({ 'text': '@foreach(var ' + vartext + 'Item in ' + (prefix ? vartext : 'Model.' + vartext) + ') {\n}', 'displayText': (parent ? parent + '.' : '') + vartext, 'sort': (parent ? parent + '.' : '') + k });
					}
				} else {
					if (o && o.type == 'image') {
						hints.push({ 'text': '<img src="@(' + varfulltext + ')" alt="" />', 'displayText': vardisplaytext, 'sort': vardisplaytext });
					} else if (o && o.type == 'file') {
						hints.push({ 'text': '<a href="@(' + varfulltext + ')" target="_blank" >Download</a>', 'displayText': vardisplaytext, 'sort': vardisplaytext });
					} else if (o && o.type == 'url') {
						hints.push({ 'text': '<a href="@(' + varfulltext + ')"  >More</a>', 'displayText': vardisplaytext, 'sort': vardisplaytext });
					} else if (o && (o.type == 'ckeditor' || o.type == 'wysihtml')) {
						hints.push({ 'text': '@Html.Raw(' + varfulltext + ')', 'displayText': vardisplaytext, 'sort': vardisplaytext });
					} else {
						hints.push({ 'text': '<text>@' + varfulltext + '</text>', 'displayText': vardisplaytext, 'sort': vardisplaytext });
					}
				}
			}
			for (i = 0; i < hints.length; i++) {
				handlebarsHints.push(hints[i]);
			}
		}
		return hints;
	};
	if (mimeType == 'htmlhandlebars') {
		var hints = addHbsProperties(schema, options, '', '');		
	} else if (mimeType == 'text/html') { //razor
		var hints = addRazorProperties(schema, options, '', '');
	}
	console.log(handlebarsHints);
	CodeMirror.registerHelper("hint", "htmlhandlebars", function (editor) {
		var list = handlebarsHints;
		var cursor = editor.getCursor();
		var currentLine = editor.getLine(cursor.line);
		var start = cursor.ch;
		var end = start;
		while (end < currentLine.length && /[\w$]+/.test(currentLine.charAt(end)))++end;
		while (start && /[\w$]+/.test(currentLine.charAt(start - 1)))--start;
		var curWord = start != end && currentLine.slice(start, end);
		var regex = new RegExp('^' + curWord, 'i');
		var result = {
			list: (!curWord ? list : list.filter(function (item) {
				return item.displayText.match(regex);
			})).sort(function compare(a, b) {
				if (a.sort < b.sort)
					return -1;
				if (a.sort > b.sort)
					return 1;
				return 0;
			}),
			from: CodeMirror.Pos(cursor.line, start),
			to: CodeMirror.Pos(cursor.line, end)
		};
		return result;
	});
}

function ocSetupCodeMirror(mimeType, elem) {

	var handlebarsHelpers = [
		{ 'text': '{{#each var}}{{/each}}', 'displayText': 'each', 'sort': 'each' },
		{ 'text': '{{#if var}}{{/if}}', 'displayText': 'if', 'sort': 'if' },
		{ 'text': '{{else}}', 'displayText': 'else', 'sort': 'else' },
		{ 'text': '{{#ifand var1 var2 var3}}{{/ifand}}', 'displayText': 'ifand', 'sort': 'ifand' },
		{ 'text': '{{#ifor var1 var2 var3}}{{/ifor}}', 'displayText': 'ifor', 'sort': 'ifor' },
		{ 'text': '{{multiply var 2}}', 'displayText': 'multiply', 'sort': 'multiply' },
		{ 'text': '{{divide var 2}}', 'displayText': 'divide', 'sort': 'divide' },
		{ 'text': '{{add var 2}}', 'displayText': 'add', 'sort': 'add' },
		{ 'text': '{{substract var 2}}', 'displayText': 'substract', 'sort': 'substract' },
		{ 'text': '{{registerscript "javascript.js"}}', 'displayText': 'registerscript', 'sort': 'registerscript' },
		{ 'text': '{{registerstylesheet "stylesheet.css"}}', 'displayText': 'registerstylesheet', 'sort': 'registerstylesheet' },
		{ 'text': '{{formatNumber var "0.00"}}', 'displayText': 'formatNumber', 'sort': 'formatNumber' },
		{ 'text': '{{formatDateTime var "dd/MMM/yy" "nl-NL" }}', 'displayText': 'formatDateTime', 'sort': 'formatDateTime' },
		{ 'text': '{{convertHtmlToText var }}', 'displayText': 'convertHtmlToText', 'sort': 'convertHtmlToText' },
		{ 'text': '{{convertToJson var }}', 'displayText': 'convertToJson', 'sort': 'convertToJson' },
		{ 'text': '{{truncateWords var 50 "..." }}', 'displayText': 'formatDateTime', 'sort': 'formatDateTime' },
		{ 'text': '{{#equal var "value"}}{{/equal}}', 'displayText': 'equal', 'sort': 'equal' },
		{ 'text': '{{#unless var}}{{/unless}}', 'displayText': 'unless', 'sort': 'unless' },
		{ 'text': '{{#with var}}{{/with}}', 'displayText': 'with', 'sort': 'with' }
	];

	var cm = CodeMirror.fromTextArea(elem, {
		lineNumbers: true,
		matchBrackets: true,
		lineWrapping: true,
		mode: mimeType,
		extraKeys: {
			"Ctrl-Space": "autocomplete", "Shift-Space": function (editor) {
				if (editor.doc.modeOption == 'htmlhandlebars') {
					var options = {
						hint: function () {
							var list = handlebarsHelpers;
							var cursor = editor.getCursor();
							var currentLine = editor.getLine(cursor.line);
							var start = cursor.ch;
							var end = start;
							while (end < currentLine.length && /[\w$]+/.test(currentLine.charAt(end)))++end;
							while (start && /[\w$]+/.test(currentLine.charAt(start - 1)))--start;
							var curWord = start != end && currentLine.slice(start, end);
							var regex = new RegExp('^' + curWord, 'i');
							var result = {
								list: (!curWord ? list : list.filter(function (item) {
									return item.displayText.match(regex);
								})).sort(function compare(a, b) {
									if (a.sort < b.sort)
										return -1;
									if (a.sort > b.sort)
										return 1;
									return 0;
								}),
								from: CodeMirror.Pos(cursor.line, start),
								to: CodeMirror.Pos(cursor.line, end)
							};
							return result;
							/*
													  return {
														from: editor.getDoc().getCursor(),
														  to: editor.getDoc().getCursor(),
														list: handlebarsHelpers
													  }
							*/
						}
					};
					editor.showHint(options);
				}
			}
		},
		hintOptions:  { hint: CodeMirror.hint.htmlhandlebars }
	});
	/*
				cm.on("inputRead", function(editor, change) {
				  if (change.text[0] == "{"){
					var options = {
						hint: function() {
						  return {
							from: editor.getDoc().getCursor(),
							  to: editor.getDoc().getCursor(),
							list: handlebarsHints
						  }
						}
					  };
					  editor.showHint(options);
					}
				});
	*/
	return cm;
}