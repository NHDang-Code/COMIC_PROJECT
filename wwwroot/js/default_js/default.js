const themeToggler = document.querySelector(".theme-toggler");
const icon = themeToggler.querySelector("span");

// Lấy theme và icon từ localStorage
const savedTheme = localStorage.getItem("theme");
const savedIcon = localStorage.getItem("icon");

// Áp dụng theme nếu đã lưu
if (savedTheme === "dark") {
    document.body.classList.add("dark-theme-variables");
    icon.textContent = "lightbulb_outline"; // icon đen (chế độ tối)
} else {
    icon.textContent = "lightbulb"; // icon sáng (chế độ sáng)
}

// Xử lý khi click nút toggle
themeToggler.addEventListener("click", () => {
    document.body.classList.toggle("dark-theme-variables");

    const isDark = document.body.classList.contains("dark-theme-variables");
    localStorage.setItem("theme", isDark ? "dark" : "light");

    // Đổi icon và lưu
    icon.textContent = isDark ? "lightbulb_outline" : "lightbulb";
    localStorage.setItem("icon", icon.textContent);
});
