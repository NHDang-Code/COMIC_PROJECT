const themeToggler = document.querySelector(".theme-toggler");
const span1 = themeToggler.querySelector('span:nth-child(1)');
const span2 = themeToggler.querySelector('span:nth-child(2)');

const savedTheme = localStorage.getItem("theme");
const savedIconState = localStorage.getItem("iconState");

// Áp dụng theme nếu đã lưu trước đó
if (savedTheme === "dark") {
    document.body.classList.add("dark-theme-variables");
}

// Kiểm tra trạng thái icon nếu đã lưu trước đó
if (savedIconState === "span1") {
    span1.classList.add("active");
    span2.classList.remove("active");
} else if (savedIconState === "span2") {
    span2.classList.add("active");
    span1.classList.remove("active");
}

//===================== Sự kiện click để đổi theme và lưu trạng thái icon
themeToggler.addEventListener("click", () => {
    document.body.classList.toggle("dark-theme-variables");

    const isDark = document.body.classList.contains("dark-theme-variables");
    localStorage.setItem("theme", isDark ? "dark" : "light");

    // Đổi trạng thái icon và lưu vào localStorage
    if (isDark) {
        span1.classList.remove("active");
        span2.classList.add("active");
        localStorage.setItem("iconState", "span2");
    } else {
        span2.classList.remove("active");
        span1.classList.add("active");
        localStorage.setItem("iconState", "span1");
    }
});