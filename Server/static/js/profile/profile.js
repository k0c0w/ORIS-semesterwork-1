function postToEdit(postFunc){
  return function(a,b){
    return postFunc('../../profile/edit', a, b, onUnauthorized);
  }
}

function post(path, body, onSuccess, onUnauthorized) {
    let xmlhttp = new XMLHttpRequest();
    
    xmlhttp.onreadystatechange = function() {
        if (xmlhttp.readyState == XMLHttpRequest.DONE) {
           if (xmlhttp.status == 200) {
               onSuccess(xmlhttp.responseText);
           }
           else if (xmlhttp.status == 401) {
              onUnauthorized();
           }
           else if (xmlhttp.status == 500){
              alert('Server error occured!');
           }
        }
    };

    xmlhttp.open('POST', path, true);
    xmlhttp.send(body);
}

function onResponse(responseText){
  let responseJson = JSON.parse(responseText);
  try{
    if(responseJson.Success == true){
      let name = document.getElementById('firstName').value;
      document.getElementById('login').innerHTML = name;
      alert("Сохранено!");
      return;
    }

    let errors = responseJson.Errors;
    for(let key in errors){
      if(errors.hasOwnProperty(key)){
        let error = errors[key];
        let id = error.Value;
        let input = document.getElementById(id);
        let block = input.parentElement;
        block.innerHTML += `<p class="questions-form-error" id="${id}-error">${error.ErrorMessage}</p>`;
        input.style.color = 'red';
      }
    }
  }
  catch(error){
    console.log(error);
  }
}

function onUnauthorized(){
  alert("не авторизован");
}

function removeOldMessages(dict){
  for(let key in dict){
    let element = document.getElementById(`${key}-error`);
    if(element != undefined){
    element.parentNode.removeChild(element);
    }
  }
}

function sendChangedFields() {
  let sendPost = postToEdit(post);
  try {
    if(FieldsChanged(UserInfo)){
      let user = getEncodedAndUpdateDict(UserInfo);
      removeOldMessages(UserInfo);
      sendPost(user, onResponse);
    }
    if(FieldsChanged(PersonalInfo)){
      let profile = getEncodedAndUpdateDict(PersonalInfo);
      removeOldMessages(PersonalInfo);
      sendPost(profile, onResponse);
    }
  }
  catch(error){
    alert('Saving error occured! Refresh page!');
    console.log(error);
  }
}

function FieldsChanged(object) {
  for(let key in object){
    if(object[key] != getById(key)){
      return true;
    }
  }
  return false;
}

function getEncodedAndUpdateDict(dict){
  let result = "";
  for(let key in dict){
    let value = getById(key);
    dict[key] = value;
    result += `${key}=${encodeURIComponent(value)}&`;
  }
  if(result == ""){
    return result;
  }
  return result.slice(0, -1);
}

function getById(id){
  return document.getElementById(id).value;
}

function scanUserInfo() {
  return {
    'email' : getById('email'),
    'birthDate' : getById('birthDate'),
    'firstName' : getById('firstName')
  }
}

function scnanPersonalInfo() {
  return {
    'firstName' : getById('firstName'),
    'middleName' :  getById('middleName'),
    'lastName' :  getById('lastName'),
    'telephone' :  getById('telephone'),
    'passport' : getById('passport'),
    'license' : getById('license'),
    'card' : getById('card'),
    'cardOwner' : getById('cardOwner'),
    'cvc' : getById('cvc')
  }
}

window.onload = function() {

  document.getElementById('openAddition').onclick = function() {
      document.getElementById('personalinfo').classList.toggle('show');
      document.getElementById('triangle').classList.toggle('triangle-rotate');
    }

  UserInfo = scanUserInfo();
  PersonalInfo = scnanPersonalInfo();

  document.getElementById('main-form').onsubmit = function() {
    sendChangedFields();  
    return false;
  }
}
