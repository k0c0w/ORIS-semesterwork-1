window.onload = function(){
    var df = document.getElementById('df');
    var df_container = document.getElementById('container');
    df.addEventListener('click', function (e) {
        let target = e.target;
        let its_df = target == df_container || df_container.contains(target);
        let df_is_active = df.classList.contains('df-active');
        if (!its_df && df_is_active) {
            toggle();
        }
        e.stopPropagation();
      });

    document.getElementById('order').addEventListener('click', (e) => {tryOrderCar(); e.stopPropagation();});
}

function toggle(){
    df.classList.toggle('df-active');
}

function openOrderingBLock(id){
    let img = document.getElementById(`${id}-image`).attributes.src.value;
    let name = document.getElementById(`${id}-name`).innerText;
    let carImg = document.getElementById(`car-image`).attributes;
    carImg.src.value = img;
    carImg.alt.value = name;
    document.getElementById(`brand-model`).innerText = name;
    document.getElementById(`order`).value=id;
    printError("");
    
    toggle();
}


function printError(error){
    document.getElementById('error-field').innerText=error;
}

function getValueById(id){
    return document.getElementById(id).value;
}

function tryOrderCar() {
    let tariff = getValueById('tariff-name');
    let car = getValueById('order');
    let start = getValueById('start');
    let end = getValueById('end');
    let city = getValueById('city');
    if(start == '' || end == '' || new Date(end) - new Date(start) < 0){
        printError("Некорректные даты!");
        return;
    }
    else{
        printError("");
    }
    if(city == ''){
        printError("Укажите город!");
        return;
    }
    else{
        printError("");
    }

    let body = `car=${encodeURIComponent(car)}&tariff=${encodeURIComponent(tariff)}&start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}&city=${encodeURIComponent(city)}`;

    post(body, parseJSON);
}

function parseJSON(text){
    text = JSON.parse(text);
    if(text.Success == true){
        alert(`Машина забронирована. Номерной знак ${text.Sign}`);
    }
    else{
        let errors = text.Errors;
        for(i in errors)
        printError(errors[i].ErrorMessage);
    }

}

function post(body, onSuccess) {
    let xmlhttp = new XMLHttpRequest();
    
    xmlhttp.onreadystatechange = function() {
        if (xmlhttp.readyState == XMLHttpRequest.DONE) {
           if (xmlhttp.status == 200) {
               onSuccess(xmlhttp.responseText);
           }
           else if (xmlhttp.status == 303) {
                window.location.replace("/login");
           }
           else if (xmlhttp.status == 500){
              alert('Server error occured!');
           }
        }
    };

    xmlhttp.open('POST', '/order/car', false);
    xmlhttp.send(body);
}

function isAuthorized() {
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.open('GET', '/profile/getUserName', false);
    xmlhttp.send();
    

    return xmlhttp.status == 200;
}