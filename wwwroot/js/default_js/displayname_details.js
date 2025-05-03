document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".comment-username .display_name").forEach(function (element) {
        var points = parseInt(element.getAttribute("data-points")) || 0;

        if (points >= 5000000) {
            element.style.backgroundImage = "url('/media/icons/dien.gif')";
        } else if (points >= 3000000) {
            element.style.backgroundImage = "url('/media/icons/8.gif')";
        } else if (points >= 100000) {
            element.style.backgroundImage = "url('/media/icons/5.gif')";
        } else if (points >= 80000) {
            element.style.backgroundImage = "url('/media/icons/6.gif')";
        } else if (points >= 40000) {
            element.style.backgroundImage = "url('/media/icons/7.gif')";
        } else if (points >= 20000) {
            element.style.backgroundImage = "url('/media/icons/4.gif')";
        } else if (points >= 1000) {
            element.style.backgroundImage = "url('/media/icons/3.gif')";
        } else {
            element.style.backgroundImage = "none";
        }
    });
});