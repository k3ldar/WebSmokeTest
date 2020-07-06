var testConfig = (function () {
var _controls = {
responseId: '',
methodId: '',
formIdList: '',
postTypeId: '',
responseUrl: '',
submitResponse: '',
form: '',
formInput: '',
formId: '',
postType: '',
bodyData: '',
postData: '',
route: '',
routeId: '',
parametersId: '',
formInputDataId: '',
};
var _settings = {
defaultResponseId: '',
defaultMethod: '',
defaultResponsSelect: '',
defaultFormId: '',
defaultPostType: '',
other: '',
formValueUrl: '',
postType: '',
isNew: '',
};
var root = {
init: function (controls, settings) {
_controls = controls;
_settings = settings;
$(document).ready(function () {
var dd = document.getElementById(_controls.responseId);
dd.value = _settings.defaultResponseId;
root.UpdateResponseType(dd);
dd = document.getElementById(_controls.methodId);
dd.value = _settings.defaultMethod;
root.UpdateMethodType(dd);
dd = document.getElementById(_controls.formIdList);
dd.value = _settings.defaultResponsSelect;
root.UpdateFormId(dd);
root.selectElement(_controls.formIdList, _settings.defaultFormId);
dd = document.getElementById(_controls.postTypeId);
dd.value = _settings.defaultPostType;
root.UpdatePostType(dd);
});
},
UpdateResponseType: function (dropdown) {
var selected = dropdown.value;
if (selected === undefined || selected === '') {
selected = "200";
}
if (selected === _settings.other) {
document.getElementById(_controls.responseId).style.display = 'inline-block';
document.getElementById(_controls.responseUrl).style.display = 'flex';
}
else {
if (selected.substr(0, 1) === "3") {
document.getElementById(_controls.responseUrl).style.display = 'flex';
}
else {
document.getElementById(_controls.responseUrl).style.display = 'none';
}
document.getElementById(_controls.responseId).style.display = 'none';
document.getElementById(_controls.responseId).value = selected;
}
},
UpdateMethodType: function (dropdown) {
var selected = dropdown.value;
if (selected === "POST") {
document.getElementById(_controls.submitResponse).style.display = 'flex';
document.getElementById(_controls.form).style.display = 'flex';
document.getElementById(_controls.formInput).style.display = 'flex';
document.getElementById(_controls.postType).style.display = 'flex';
var pt = document.getElementById(_controls.postTypeId);
root.UpdatePostType(pt);
}
else {
document.getElementById(_controls.bodyData).style.display = 'none';
document.getElementById(_controls.submitResponse).style.display = 'none';
document.getElementById(_controls.form).style.display = 'none';
document.getElementById(_controls.formInput).style.display = 'none';
document.getElementById(_controls.postType).style.display = 'none';
document.getElementById(_controls.route).style.display = 'flex"';
}
var fi = document.getElementById(_controls.formIdList);
if (fi != null && fi != undefined) {
root.UpdateFormId(fi);
}
var pt = document.getElementById(_controls.postTypeId);
root.UpdatePostType(pt);
},
UpdateFormId: function (dropdown) {
var selected = dropdown.value;
if (selected === _settings.other) {
document.getElementById(_controls.bodyData).style.display = 'none';
document.getElementById(_controls.formId).style.display = 'inline-block';
}
else {
document.getElementById(_controls.bodyData).style.display = 'none';
document.getElementById(_controls.formId).style.display = 'none';
}
if (_settings.postType === "Form") {
if (selected == _settings.other) {
//document.getElementById(_controls.routeId).readOnly = false;
}
else {
if (_settings.isNew === "True") {
$.ajax({
type: 'POST',
url: _settings.formValueUrl + selected + '/',
cache: false,
success: function (response) {
document.getElementById(_controls.parametersId).value = response.parameters;
document.getElementById(_controls.routeId).value = response.route;
document.getElementById(_controls.formInputDataId).value = response.inputData;
},
})
}
}
}
},
UpdatePostType: function (dropdown) {
var selected = dropdown.value;
_settings.postType = selected;
if (selected === "Form") {
document.getElementById(_controls.form).style.display = 'flex';
document.getElementById(_controls.bodyData).style.display = 'none';
document.getElementById(_controls.formInput).style.display = 'flex';
var fi = document.getElementById(_controls.formIdList);
if (fi != null && fi != undefined) {
root.UpdateFormId(fi);
}
}
else if (selected === "Xml") {
document.getElementById(_controls.formInput).style.display = 'none';
document.getElementById(_controls.form).style.display = 'none';
document.getElementById(_controls.bodyData).style.display = 'flex';
document.getElementById(_controls.postData).innerText = 'Xml Data';
}
else if (selected === "Json") {
document.getElementById(_controls.formInput).style.display = 'none';
document.getElementById(_controls.form).style.display = 'none';
document.getElementById(_controls.bodyData).style.display = 'flex';
document.getElementById(_controls.postData).innerText = 'Json Data';
}
else if (selected === _settings.other) {
document.getElementById(_controls.formInput).style.display = 'none';
document.getElementById(_controls.form).style.display = 'none';
document.getElementById(_controls.bodyData).style.display = 'flex';
document.getElementById(_controls.postData).innerText = 'Body Data';
}
},
selectElement: function (id, valueToSelect) {
let element = document.getElementById(id);
element.value = valueToSelect;
},
};
return root;
})();