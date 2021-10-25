const slider_content = document.querySelector('.slider_container');
const slider_content1 = document.querySelector('.newsslider_container');
const nextbutton = document.querySelector(".nxtbtn");
const previousbutton = document.querySelector(".prevbtn");
const nextbutton1 = document.querySelector(".nxtbtn1");
const previousbutton1 = document.querySelector(".prevbtn1");
const imagearray = Array.from(document.querySelectorAll(".slider_container div"));
const imagearray1 = Array.from(document.querySelectorAll(".newsslider_container div"));


let i = 0;
let j = 0;



nextbutton.addEventListener('click', () => {



    if (i < imagearray.length - 1) {
        i = i + 1;
    } else {
        i = 0;
    }
    slider_content.innerHTML = imagearray[i].innerHTML;

});



previousbutton.addEventListener('click', () => {
    if (i > 0) {



        i = i - 1;
    } else {
        i = imagearray.length - 1;
    }
    slider_content.innerHTML = imagearray[i].innerHTML;



});


nextbutton1.addEventListener('click', () => {



    if (j < imagearray1.length - 1) {
        j = j + 1;
    } else {
        j = 0;
    }
    slider_content1.innerHTML = imagearray1[j].innerHTML;



});



previousbutton1.addEventListener('click', () => {
    if (j > 0) {



        j = j - 1;
    } else {
        j = imagearray1.length - 1;
    }
    slider_content1.innerHTML = imagearray1[j].innerHTML;



})