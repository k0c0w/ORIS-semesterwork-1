window.onload = function() {
  var header = document.getElementById('header');
  if(header.classList.contains('fixed'))
        header.classList.remove('fixed');
  trySetUserName();
  subscribeValidationOnQuestionForm('question-form');
}

window.addEventListener('scroll', function () {
  let header = document.getElementById('header');
   if (window.scrollY > 20) {
      header.classList.add("fixed");
    } else {
        header.classList.remove("fixed");
    }
});
