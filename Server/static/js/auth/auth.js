function subscribeSubmit(id, onSubmit){
    let form = document.getElementById(id);
    form.onsubmit = function() {return onSubmit()};
}


function validateLogin(){
    let isEmailValid = validate('login-email', /^[a-zA-Z][0-9a-zA-Z]*(\.[0-9a-zA-Z]+)*@[a-zA-Z]+(\.[a-zA-Z]+)+/);
    let isPassword = validate('login-password', /^[a-zA-Z0-9]{5,50}$/);
    let isCorrect = isEmailValid && isPassword;

    if(isCorrect){
            document.getElementById('secretValue').innerHTML = `<input type="hidden" name="rememberMe" value="${document.getElementById('rememberMe').checked}">`;
    }


    return isCorrect;
}

function validateRegister(){
    let isNameValid = validate('register-name', /^[a-zA-Zа-яА-ЯёЁ]+$/);
    let isTelephoneValid = validate('register-phone', /^\+7[0-9]{10}$/);
    let isEmailValid = validate('register-email', /^[a-zA-Z][0-9a-zA-Z]*(\.[0-9a-zA-Z]+)*@[a-zA-Z]+(\.[a-zA-Z]+)+/);
    let isPassword = validate('register-password', /^[a-zA-Z0-9]{5,50}$/);
    let isPasswordRepeat = !isPassword ? false : validate('register-password-repeat', new RegExp(document.getElementById('register-password').value));
    let age = new Date(document.getElementById('register-birthDate').value);
    age.setFullYear(age.getFullYear() + 18);
    let isAge = age <= new Date();
    let field = document.getElementById('register-birthDate');
    let nextSibling = field.nextSibling;
    while(nextSibling && nextSibling.nodeType != 1) {
        nextSibling = nextSibling.nextSibling}
    if(!isAge){

        nextSibling.style.display = 'block';
    }
    else{
        nextSibling.style.display = 'none';
    }
    let isAgree = document.getElementById('register-agreement').checked;
    if(!isAgree) { alert('Примите согласие на обработку данных!');}

    return isNameValid && isPassword && isTelephoneValid && isEmailValid && isPasswordRepeat && isAge && isAgree;
}