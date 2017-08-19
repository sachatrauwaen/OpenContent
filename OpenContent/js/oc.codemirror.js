function ocInitCodeMirror(mimeType, model) {

	var schema = model.schema;
	var options = model.options;

	var addressSchema = {
		"title": "Address",
		"type": "object",
		"properties": {
			"street": {
				"title": "Street",
				"type": "string"
			},
			"number": {
				"title": "House Number",
				"type": "string"
			},
			"city": {
				"title": "City",
				"type": "string"
			},
			"state": {
				"title": "State",
				"type": "string",
				"enum": ["AL", "AK", "AS", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK", "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI", "VA", "WA", "WV", "WI", "WY"]
			},
			"postalcode": {
				"title": "Postal Code",
				"type": "string"
			},
			"country": {
				"title": "Country",
				"type": "string"
			},
			"latitude": {
				"title": "Latitude",
				"type": "number"
			},
			"longitude": {
				"title": "Longitude",
				"type": "number"
			}
		}
	};

	var contextVars = ['ModuleId', 'ModuleTitle', 'PortalId', 'GoogleApiKey', 'IsEditable', 'IsEditMode', 'MainUrl', 'HomeDirectory', 'HTTPAlias', 'DetailUrl', 'Id', 'EditUrl'];

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
				var vardisplaytext = (parent ? parent + '.' : '') + k;
				var o = opt && opt.fields ? opt.fields[k] : null;
				if (p.type == 'object') {
					if (o.type == 'address') {
						var childHints = addHbsProperties(addressSchema, null, vardisplaytext, k);
						hints = hints.concat(childHints);
					} else if (!p.properties) {
						hints.push({ 'text': '{{' + vartext + '.var}}', 'displayText': vardisplaytext });
					} else {
						var childHints = addHbsProperties(p, o, vardisplaytext, k);
						hints = hints.concat(childHints);
					}
					//handlebarsHints.push({'text':'{{#with '+k+'}} {{/with}}', 'displayText': k+ (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
				} else if (p.type == 'array') {
					if (p.items) {
						var childHints = addHbsProperties(p.items, o ? o.items : null, k, '');
						var snipet = '';
						for (i = 0; i < childHints.length; i++) {
							snipet += childHints[i].text + '\n';
						}
						snipet = '{{#each ' + vartext + '}}\n' + snipet + '{{/each}}';
						hints.push({ 'text': snipet, 'displayText': vardisplaytext });
						hints = hints.concat(childHints);
					} else {
						hints.push({ 'text': '{{#each ' + vartext + '}}\n\n{{/each}}', 'displayText': vardisplaytext });
					}
				} else {
					if (o && o.type == 'image') {
						hints.push({ 'text': '<img src="{{' + vartext + '}}" alt="" />', 'displayText': vardisplaytext });
					} else if (o && o.type == 'file') {
						hints.push({ 'text': '<a href="{{' + vartext + '}}" target="_blank" >Download</a>', 'displayText': vardisplaytext });
					} else if (o && o.type == 'url') {
						hints.push({ 'text': '<a href="{{' + vartext + '}}"  >More</a>', 'displayText': vardisplaytext });
					} else if (o && (o.type == 'ckeditor' || o.type == 'wysihtml')) {
						hints.push({ 'text': '{{{' + vartext + '}}}', 'displayText': vardisplaytext });
					} else {
						hints.push({ 'text': '{{' + vartext + '}}', 'displayText': vardisplaytext });
					}
				}
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
					if (o.type == 'address') {
						var childHints = addRazorProperties(addressSchema, null, vardisplaytext, varfulltext);
						hints = hints.concat(childHints);
					} else if (!p.properties) {
						hints.push({ 'text': '@' + varfulltext + '.var', 'displayText': vardisplaytext });
					} else {
						var childHints = addRazorProperties(p, o, vardisplaytext, varfulltext);
						hints = hints.concat(childHints);
					}
				} else if (p.type == 'array') {
					if (p.type == 'array' && p.items) {
						var childHints = addRazorProperties(p.items, o ? o.items : null, k, vartext + 'Item');
						var snipet = '';
						for (i = 0; i < childHints.length; i++) {
							snipet += '' + childHints[i].text + '\n';
						}
						snipet = '@foreach(var ' + vartext + 'Item in ' + varfulltext + ') {\n' + snipet + '}';
						hints.push({ 'text': snipet, 'displayText': vardisplaytext });
						hints = hints.concat(childHints);
					} else {
						hints.push({ 'text': '@foreach(var ' + vartext + 'Item in ' + (prefix ? vartext : 'Model.' + vartext) + ') {\n}', 'displayText': vardisplaytext });
					}
				} else {
					if (o && o.type == 'image') {
						hints.push({ 'text': '<img src="@(' + varfulltext + ')" alt="" />', 'displayText': vardisplaytext });
					} else if (o && o.type == 'file') {
						hints.push({ 'text': '<a href="@(' + varfulltext + ')" target="_blank" >Download</a>', 'displayText': vardisplaytext });
					} else if (o && o.type == 'url') {
						hints.push({ 'text': '<a href="@(' + varfulltext + ')"  >More</a>', 'displayText': vardisplaytext });
					} else if (o && (o.type == 'ckeditor' || o.type == 'wysihtml')) {
						hints.push({ 'text': '@Html.Raw(' + varfulltext + ')', 'displayText': vardisplaytext });
					} else {
						hints.push({ 'text': '@' + varfulltext + '', 'displayText': vardisplaytext });
					}
				}
			}
		}
		return hints;
	};
	if (mimeType == 'htmlhandlebars') {

		var hints = addHbsProperties(schema, options, '', '');
		if (model.listTemplate) {
			var snipet = '';
			for (i = 0; i < hints.length; i++) {
				snipet += hints[i].text + '\n';
			}
			snipet = '{{#each Items}}\n' + snipet + '{{/each}}';
			handlebarsHints.push({ 'text': snipet, 'displayText': 'Items' });
		}
		handlebarsHints = handlebarsHints.concat(hints);
		var settingsHints = addHbsProperties(model.settingsSchema, model.settingsOptions, 'Settings', 'Settings');
		handlebarsHints = handlebarsHints.concat(settingsHints);
		for (var i = 0; i < contextVars.length; i++) {
			handlebarsHints.push({ 'text': '{{Context.' + contextVars[i]+'}}', 'displayText': 'Context.' + contextVars[i] });
		}
		if (model.localization) {
			for (var i in model.localization) {
				handlebarsHints.push({ 'text': '{{Localization.' + i + '}}', 'displayText': 'Localization.' + i });
			}
		}
		if (model.additionalData) {
			for (var i in model.additionalData) {
				var aHints = addHbsProperties(model.additionalData[i].schema, model.additionalData[i].options, 'AdditionalData.' + i, 'AdditionalData.' + i);
				handlebarsHints = handlebarsHints.concat(aHints);
			}
		}
	} else if (mimeType == 'text/html') { //razor

		var hints = addRazorProperties(schema, options, '', '');
		if (model.listTemplate) {
			var snipet = '';
			for (i = 0; i < hints.length; i++) {
				snipet += hints[i].text + '\n';
			}
			snipet = '@foreach(var item in Model.Items) {\n' + snipet + '}';
			handlebarsHints.push({ 'text': snipet, 'displayText': 'Items' });
		}
		handlebarsHints = handlebarsHints.concat(hints);
		var settingsHints = addRazorProperties(model.settingsSchema, model.settingsOptions, 'Settings', 'Model.Settings');
		handlebarsHints = handlebarsHints.concat(settingsHints);
		for (var i = 0; i < contextVars.length; i++) {
			handlebarsHints.push({ 'text': '@Model_or_item.Context.' + contextVars[i], 'displayText': 'Context.' + contextVars[i] });
		}
		if (model.localization) {
			for (var i in model.localization) {
				handlebarsHints.push({ 'text': '@Model.Localization.' + i + '}}', 'displayText': 'Localization.' + i });
			}
		}
		if (model.additionalData) {
			for (var i in model.additionalData) {
				handlebarsHints.push({ 'text': '@Model.Localization.' + i + '}}', 'displayText': 'Localization.' + i });
			}
		}
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
				if (a.displayText < b.displayText)
					return -1;
				if (a.displayText > b.displayText)
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
		hintOptions: { hint: CodeMirror.hint.htmlhandlebars }
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