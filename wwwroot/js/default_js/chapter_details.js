function toggleChapterList() {
    let list = document.getElementById("chapter-list");
    list.classList.toggle("show");
}


document.addEventListener("click", function (event) {
    let dropdown = document.getElementById("chapter-list");
    let button = document.querySelector(".btn-chapter-list");

    if (!button.contains(event.target) && !dropdown.contains(event.target)) {
        dropdown.classList.remove("show");
    }
});


function toggleSettingsMenu() {
    let menu = document.getElementById("settings-dropdown");

    if (!menu.classList.contains("active")) {
        menu.style.opacity = "1";
        menu.style.pointerEvents = "all";
        menu.classList.add("active");
    } else {
        menu.classList.remove("active");
        setTimeout(() => {
            menu.style.opacity = "0";
            menu.style.pointerEvents = "none";
        }, 300);
    }
}

function filterChapters() {
    let input = document.getElementById("searchChapter").value.toLowerCase();
    let ul = document.getElementById("chapterUl");
    let li = ul.getElementsByTagName("li");

    for (let i = 0; i < li.length; i++) {
        let a = li[i].getElementsByTagName("a")[0];
        if (a) {
            let txtValue = a.textContent || a.innerText;
            if (txtValue.toLowerCase().indexOf(input) > -1) {
                li[i].style.display = "";
            } else {
                li[i].style.display = "none";
            }
        }
    }
}


$(document).ready(function () {
    let chapterId = $("#chapter-container").data("chapter-id");
    let interval = 30000;
    let pointsPerInterval = 50;

    function increasePoints() {
        $.ajax({
            url: '/Chapter/IncreasePoints',
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    console.log("Điểm đã được tăng:", response.message);
                } else {
                    console.log("Không tăng điểm:", response.message);
                }
            },
            error: function (xhr) {
                console.error("Lỗi khi tăng điểm:", xhr.responseText);
            }
        });
    }

    setInterval(increasePoints, interval);
});

