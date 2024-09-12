/******/ (function(modules) { // webpackBootstrap
/******/ 	// install a JSONP callback for chunk loading
/******/ 	function webpackJsonpCallback(data) {
/******/ 		var chunkIds = data[0];
/******/ 		var moreModules = data[1];
/******/ 		var executeModules = data[2];
/******/
/******/ 		// add "moreModules" to the modules object,
/******/ 		// then flag all "chunkIds" as loaded and fire callback
/******/ 		var moduleId, chunkId, i = 0, resolves = [];
/******/ 		for(;i < chunkIds.length; i++) {
/******/ 			chunkId = chunkIds[i];
/******/ 			if(Object.prototype.hasOwnProperty.call(installedChunks, chunkId) && installedChunks[chunkId]) {
/******/ 				resolves.push(installedChunks[chunkId][0]);
/******/ 			}
/******/ 			installedChunks[chunkId] = 0;
/******/ 		}
/******/ 		for(moduleId in moreModules) {
/******/ 			if(Object.prototype.hasOwnProperty.call(moreModules, moduleId)) {
/******/ 				modules[moduleId] = moreModules[moduleId];
/******/ 			}
/******/ 		}
/******/ 		if(parentJsonpFunction) parentJsonpFunction(data);
/******/
/******/ 		while(resolves.length) {
/******/ 			resolves.shift()();
/******/ 		}
/******/
/******/ 		// add entry modules from loaded chunk to deferred list
/******/ 		deferredModules.push.apply(deferredModules, executeModules || []);
/******/
/******/ 		// run deferred modules when all chunks ready
/******/ 		return checkDeferredModules();
/******/ 	};
/******/ 	function checkDeferredModules() {
/******/ 		var result;
/******/ 		for(var i = 0; i < deferredModules.length; i++) {
/******/ 			var deferredModule = deferredModules[i];
/******/ 			var fulfilled = true;
/******/ 			for(var j = 1; j < deferredModule.length; j++) {
/******/ 				var depId = deferredModule[j];
/******/ 				if(installedChunks[depId] !== 0) fulfilled = false;
/******/ 			}
/******/ 			if(fulfilled) {
/******/ 				deferredModules.splice(i--, 1);
/******/ 				result = __webpack_require__(__webpack_require__.s = deferredModule[0]);
/******/ 			}
/******/ 		}
/******/
/******/ 		return result;
/******/ 	}
/******/
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// object to store loaded and loading chunks
/******/ 	// undefined = chunk not loaded, null = chunk preloaded/prefetched
/******/ 	// Promise = chunk loading, 0 = chunk loaded
/******/ 	var installedChunks = {
/******/ 		"app": 0
/******/ 	};
/******/
/******/ 	var deferredModules = [];
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "/";
/******/
/******/ 	var jsonpArray = window["webpackJsonp"] = window["webpackJsonp"] || [];
/******/ 	var oldJsonpFunction = jsonpArray.push.bind(jsonpArray);
/******/ 	jsonpArray.push = webpackJsonpCallback;
/******/ 	jsonpArray = jsonpArray.slice();
/******/ 	for(var i = 0; i < jsonpArray.length; i++) webpackJsonpCallback(jsonpArray[i]);
/******/ 	var parentJsonpFunction = oldJsonpFunction;
/******/
/******/
/******/ 	// add entry module to deferred list
/******/ 	deferredModules.push([0,"chunk-vendors"]);
/******/ 	// run deferred modules when ready
/******/ 	return checkDeferredModules();
/******/ })
/************************************************************************/
/******/ ({

/***/ "./node_modules/cache-loader/dist/cjs.js?!./node_modules/babel-loader/lib/index.js!./node_modules/cache-loader/dist/cjs.js?!./node_modules/vue-loader/lib/index.js?!./src/components/CollectionField.vue?vue&type=script&lang=js&":
/*!************************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/cache-loader/dist/cjs.js??ref--12-0!./node_modules/babel-loader/lib!./node_modules/cache-loader/dist/cjs.js??ref--0-0!./node_modules/vue-loader/lib??vue-loader-options!./src/components/CollectionField.vue?vue&type=script&lang=js& ***!
  \************************************************************************************************************************************************************************************************************************************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! lamavue */ \"./node_modules/lamavue/dist/lamavue.umd.js\");\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(lamavue__WEBPACK_IMPORTED_MODULE_0__);\n\nvar CollectionField = {\n  name: \"CollectionField\",\n  extends: lamavue__WEBPACK_IMPORTED_MODULE_0__[\"VueSelectBaseField\"],\n  props: {},\n  data: function data() {\n    return {};\n  },\n  computed: {\n    query: function query() {\n      return {\n        type: \"relation\",\n        action: this.options.dataService.action,\n        collection: this.options.dataService.data.collection,\n        textField: this.options.dataService.data.textField\n      };\n    }\n  },\n  methods: {// map(item) {\n    //   return {\n    //     value: item[this.options.dataService.data.valueField],\n    //     text: item[this.options.dataService.data.textField],\n    //   };\n    // },\n  },\n  components: {},\n  builder: {\n    props: function props() {\n      return {\n        schema: {\n          type: \"object\",\n          properties: {\n            many: {\n              type: \"boolean\"\n            },\n            collection: {\n              title: \"Collection\",\n              type: \"string\"\n            },\n            textField: {\n              title: \"TextField\",\n              type: \"string\"\n            },\n            placeholder: {\n              title: \"Placeholder\",\n              type: \"string\"\n            }\n          }\n        },\n        options: {\n          fields: {\n            many: {\n              rightLabel: \"Many\"\n            }\n          }\n        }\n      };\n    },\n    fromBuilder: function fromBuilder(field) {\n      return {\n        schema: {\n          type: field.many ? \"array\" : \"string\"\n        },\n        options: {\n          type: \"collection\",\n          placeholder: field.placeholder,\n          dataService: {\n            action: \"LookupCollection\",\n            data: {\n              collection: field.collection,\n              textField: field.textField\n            }\n          }\n        }\n      };\n    },\n    toBuilder: function toBuilder(def) {\n      var builder = {\n        fieldType: \"collection\",\n        placeholder: def.options.placeholder,\n        many: def.schema.type == \"array\"\n      };\n\n      if (def.options && def.options.dataService && def.options.dataService.data) {\n        builder.collection = def.options.dataService.data.collection;\n        builder.textField = def.options.dataService.data.textField;\n      }\n\n      return builder;\n    }\n  }\n};\n/* harmony default export */ __webpack_exports__[\"default\"] = (CollectionField);\n\n//# sourceURL=webpack:///./src/components/CollectionField.vue?./node_modules/cache-loader/dist/cjs.js??ref--12-0!./node_modules/babel-loader/lib!./node_modules/cache-loader/dist/cjs.js??ref--0-0!./node_modules/vue-loader/lib??vue-loader-options");

/***/ }),

/***/ "./node_modules/cache-loader/dist/cjs.js?!./node_modules/babel-loader/lib/index.js!./node_modules/cache-loader/dist/cjs.js?!./node_modules/vue-loader/lib/index.js?!./src/components/UserRoleField.vue?vue&type=script&lang=js&":
/*!**********************************************************************************************************************************************************************************************************************************************************!*\
  !*** ./node_modules/cache-loader/dist/cjs.js??ref--12-0!./node_modules/babel-loader/lib!./node_modules/cache-loader/dist/cjs.js??ref--0-0!./node_modules/vue-loader/lib??vue-loader-options!./src/components/UserRoleField.vue?vue&type=script&lang=js& ***!
  \**********************************************************************************************************************************************************************************************************************************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! lamavue */ \"./node_modules/lamavue/dist/lamavue.umd.js\");\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(lamavue__WEBPACK_IMPORTED_MODULE_0__);\n\nvar UserRoleField = {\n  name: \"UserRoleField\",\n  extends: lamavue__WEBPACK_IMPORTED_MODULE_0__[\"VueSelectBaseField\"],\n  props: {},\n  data: function data() {\n    return {};\n  },\n  computed: {\n    query: function query() {\n      return {\n        type: \"userrole\"\n      };\n    }\n  },\n  methods: {\n    map: function map(item) {\n      return item.text;\n    },\n    reduce: function reduce(option) {\n      return option;\n    }\n  },\n  components: {},\n  builder: {\n    props: function props() {\n      return {\n        schema: {\n          type: \"object\",\n          properties: {\n            many: {\n              type: \"boolean\"\n            },\n            placeholder: {\n              title: \"Placeholder\",\n              type: \"string\"\n            }\n          }\n        },\n        options: {\n          fields: {\n            many: {\n              rightLabel: \"Many\"\n            }\n          }\n        }\n      };\n    },\n    fromBuilder: function fromBuilder(field) {\n      return {\n        schema: {\n          type: field.many ? \"array\" : \"string\"\n        },\n        options: {\n          type: \"userrole\",\n          placeholder: field.placeholder\n        }\n      };\n    },\n    toBuilder: function toBuilder(def) {\n      var builder = {\n        fieldType: \"userrole\",\n        placeholder: def.options.placeholder,\n        many: def.schema.type == \"array\"\n      };\n      return builder;\n    }\n  }\n};\n/* harmony default export */ __webpack_exports__[\"default\"] = (UserRoleField);\n\n//# sourceURL=webpack:///./src/components/UserRoleField.vue?./node_modules/cache-loader/dist/cjs.js??ref--12-0!./node_modules/babel-loader/lib!./node_modules/cache-loader/dist/cjs.js??ref--0-0!./node_modules/vue-loader/lib??vue-loader-options");

/***/ }),

/***/ "./src/components/CollectionField.vue":
/*!********************************************!*\
  !*** ./src/components/CollectionField.vue ***!
  \********************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _CollectionField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./CollectionField.vue?vue&type=script&lang=js& */ \"./src/components/CollectionField.vue?vue&type=script&lang=js&\");\n/* empty/unused harmony star reexport *//* harmony import */ var _node_modules_vue_loader_lib_runtime_componentNormalizer_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../node_modules/vue-loader/lib/runtime/componentNormalizer.js */ \"./node_modules/vue-loader/lib/runtime/componentNormalizer.js\");\nvar render, staticRenderFns\n\n\n\n\n/* normalize component */\n\nvar component = Object(_node_modules_vue_loader_lib_runtime_componentNormalizer_js__WEBPACK_IMPORTED_MODULE_1__[\"default\"])(\n  _CollectionField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__[\"default\"],\n  render,\n  staticRenderFns,\n  false,\n  null,\n  \"3d38e0fc\",\n  null\n  \n)\n\n/* hot reload */\nif (false) { var api; }\ncomponent.options.__file = \"src/components/CollectionField.vue\"\n/* harmony default export */ __webpack_exports__[\"default\"] = (component.exports);\n\n//# sourceURL=webpack:///./src/components/CollectionField.vue?");

/***/ }),

/***/ "./src/components/CollectionField.vue?vue&type=script&lang=js&":
/*!*********************************************************************!*\
  !*** ./src/components/CollectionField.vue?vue&type=script&lang=js& ***!
  \*********************************************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _node_modules_cache_loader_dist_cjs_js_ref_12_0_node_modules_babel_loader_lib_index_js_node_modules_cache_loader_dist_cjs_js_ref_0_0_node_modules_vue_loader_lib_index_js_vue_loader_options_CollectionField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! -!../../node_modules/cache-loader/dist/cjs.js??ref--12-0!../../node_modules/babel-loader/lib!../../node_modules/cache-loader/dist/cjs.js??ref--0-0!../../node_modules/vue-loader/lib??vue-loader-options!./CollectionField.vue?vue&type=script&lang=js& */ \"./node_modules/cache-loader/dist/cjs.js?!./node_modules/babel-loader/lib/index.js!./node_modules/cache-loader/dist/cjs.js?!./node_modules/vue-loader/lib/index.js?!./src/components/CollectionField.vue?vue&type=script&lang=js&\");\n/* empty/unused harmony star reexport */ /* harmony default export */ __webpack_exports__[\"default\"] = (_node_modules_cache_loader_dist_cjs_js_ref_12_0_node_modules_babel_loader_lib_index_js_node_modules_cache_loader_dist_cjs_js_ref_0_0_node_modules_vue_loader_lib_index_js_vue_loader_options_CollectionField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__[\"default\"]); \n\n//# sourceURL=webpack:///./src/components/CollectionField.vue?");

/***/ }),

/***/ "./src/components/UserRoleField.vue":
/*!******************************************!*\
  !*** ./src/components/UserRoleField.vue ***!
  \******************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _UserRoleField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./UserRoleField.vue?vue&type=script&lang=js& */ \"./src/components/UserRoleField.vue?vue&type=script&lang=js&\");\n/* empty/unused harmony star reexport *//* harmony import */ var _node_modules_vue_loader_lib_runtime_componentNormalizer_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../node_modules/vue-loader/lib/runtime/componentNormalizer.js */ \"./node_modules/vue-loader/lib/runtime/componentNormalizer.js\");\nvar render, staticRenderFns\n\n\n\n\n/* normalize component */\n\nvar component = Object(_node_modules_vue_loader_lib_runtime_componentNormalizer_js__WEBPACK_IMPORTED_MODULE_1__[\"default\"])(\n  _UserRoleField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__[\"default\"],\n  render,\n  staticRenderFns,\n  false,\n  null,\n  \"e9d13bce\",\n  null\n  \n)\n\n/* hot reload */\nif (false) { var api; }\ncomponent.options.__file = \"src/components/UserRoleField.vue\"\n/* harmony default export */ __webpack_exports__[\"default\"] = (component.exports);\n\n//# sourceURL=webpack:///./src/components/UserRoleField.vue?");

/***/ }),

/***/ "./src/components/UserRoleField.vue?vue&type=script&lang=js&":
/*!*******************************************************************!*\
  !*** ./src/components/UserRoleField.vue?vue&type=script&lang=js& ***!
  \*******************************************************************/
/*! exports provided: default */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var _node_modules_cache_loader_dist_cjs_js_ref_12_0_node_modules_babel_loader_lib_index_js_node_modules_cache_loader_dist_cjs_js_ref_0_0_node_modules_vue_loader_lib_index_js_vue_loader_options_UserRoleField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! -!../../node_modules/cache-loader/dist/cjs.js??ref--12-0!../../node_modules/babel-loader/lib!../../node_modules/cache-loader/dist/cjs.js??ref--0-0!../../node_modules/vue-loader/lib??vue-loader-options!./UserRoleField.vue?vue&type=script&lang=js& */ \"./node_modules/cache-loader/dist/cjs.js?!./node_modules/babel-loader/lib/index.js!./node_modules/cache-loader/dist/cjs.js?!./node_modules/vue-loader/lib/index.js?!./src/components/UserRoleField.vue?vue&type=script&lang=js&\");\n/* empty/unused harmony star reexport */ /* harmony default export */ __webpack_exports__[\"default\"] = (_node_modules_cache_loader_dist_cjs_js_ref_12_0_node_modules_babel_loader_lib_index_js_node_modules_cache_loader_dist_cjs_js_ref_0_0_node_modules_vue_loader_lib_index_js_vue_loader_options_UserRoleField_vue_vue_type_script_lang_js___WEBPACK_IMPORTED_MODULE_0__[\"default\"]); \n\n//# sourceURL=webpack:///./src/components/UserRoleField.vue?");

/***/ }),

/***/ "./src/main.js":
/*!*********************!*\
  !*** ./src/main.js ***!
  \*********************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
eval("__webpack_require__.r(__webpack_exports__);\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_array_iterator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.array.iterator.js */ \"./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.array.iterator.js\");\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_array_iterator_js__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_array_iterator_js__WEBPACK_IMPORTED_MODULE_0__);\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_js__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.promise.js */ \"./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.promise.js\");\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_js__WEBPACK_IMPORTED_MODULE_1___default = /*#__PURE__*/__webpack_require__.n(C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_js__WEBPACK_IMPORTED_MODULE_1__);\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_object_assign_js__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.object.assign.js */ \"./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.object.assign.js\");\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_object_assign_js__WEBPACK_IMPORTED_MODULE_2___default = /*#__PURE__*/__webpack_require__.n(C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_object_assign_js__WEBPACK_IMPORTED_MODULE_2__);\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_finally_js__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.promise.finally.js */ \"./node_modules/@vue/babel-preset-app/node_modules/core-js/modules/es.promise.finally.js\");\n/* harmony import */ var C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_finally_js__WEBPACK_IMPORTED_MODULE_3___default = /*#__PURE__*/__webpack_require__.n(C_sacha_dnn_OpenContent_OpenContent_lama_node_modules_vue_babel_preset_app_node_modules_core_js_modules_es_promise_finally_js__WEBPACK_IMPORTED_MODULE_3__);\n/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! vue */ \"./node_modules/vue/dist/vue.esm.js\");\n/* harmony import */ var _components_UserRoleField_vue__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./components/UserRoleField.vue */ \"./src/components/UserRoleField.vue\");\n/* harmony import */ var _components_CollectionField_vue__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./components/CollectionField.vue */ \"./src/components/CollectionField.vue\");\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! lamavue */ \"./node_modules/lamavue/dist/lamavue.umd.js\");\n/* harmony import */ var lamavue__WEBPACK_IMPORTED_MODULE_7___default = /*#__PURE__*/__webpack_require__.n(lamavue__WEBPACK_IMPORTED_MODULE_7__);\n\n\n\n\n //import App from './App.vue'\n\n\n // eslint-disable-next-line no-unused-vars\n\n\nvue__WEBPACK_IMPORTED_MODULE_4__[\"default\"].config.productionTip = false;\nlamavue__WEBPACK_IMPORTED_MODULE_7__[\"Lama\"].registerFieldComponent(\"userrole\", _components_UserRoleField_vue__WEBPACK_IMPORTED_MODULE_5__[\"default\"]);\nlamavue__WEBPACK_IMPORTED_MODULE_7__[\"Lama\"].registerFieldComponent(\"collection\", _components_CollectionField_vue__WEBPACK_IMPORTED_MODULE_6__[\"default\"]);\n/*\r\nnew Vue({\r\n  render: h => h(App),\r\n}).$mount('#app')\r\n*/\n\n//# sourceURL=webpack:///./src/main.js?");

/***/ }),

/***/ 0:
/*!***************************!*\
  !*** multi ./src/main.js ***!
  \***************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

eval("module.exports = __webpack_require__(/*! C:\\sacha\\dnn\\OpenContent\\OpenContent\\lama\\src\\main.js */\"./src/main.js\");\n\n\n//# sourceURL=webpack:///multi_./src/main.js?");

/***/ })

/******/ });