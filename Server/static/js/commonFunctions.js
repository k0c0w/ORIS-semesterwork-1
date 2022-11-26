function trySetUserName(){
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function() {
        if (xmlhttp.readyState == XMLHttpRequest.DONE) {
           if (xmlhttp.status == 200) {
                let nameElement = document.getElementById('login');
                nameElement.innerText = JSON.parse(xmlhttp.responseText).name;
                nameElement.setAttribute('href', 'profile/edit');
           }
        }
    };

    xmlhttp.open('GET', '../../profile/getUserName', true);
    xmlhttp.send();
}

function subscribeValidationOnQuestionForm(id){
    let form = document.getElementById(id);
    form.onsubmit = function (){
        let isNameValid = validate('question-name', /^[a-zA-Zа-яА-ЯёЁ]+$/);
        let isTelephoneValid = validate('question-phone', /^\+7[0-9]{10}$/);
        let isEmailValid = validate('question-email', /^[a-zA-Z][0-9a-zA-Z]*(\.[0-9a-zA-Z]+)*@[a-zA-Z]+(\.[a-zA-Z]+)+/);
        let isMessage = validate('question-text', /[a-zA-Zа-яА-ЯёЁ]+/);
        let isAgree = document.getElementById('question-agreement').checked;
        if(!isAgree) { alert('Примите согласие на обработку данных!');}

        return isNameValid && isTelephoneValid && isEmailValid && isMessage && isAgree;
    }
}

function validate(id, template){
    let field = document.getElementById(id);
    let nextSibling = field.nextSibling;
    while(nextSibling && nextSibling.nodeType != 1) {
        nextSibling = nextSibling.nextSibling
    }
    if(!template.test(field.value)){
        nextSibling.style.display = 'block';
        return false;
    }
    nextSibling.style.display = 'none';
    return true;
}