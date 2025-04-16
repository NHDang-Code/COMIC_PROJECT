const themeToggler = document.querySelector(".theme-toggler");
const span1 = themeToggler.querySelector('span:nth-child(1)');
const span2 = themeToggler.querySelector('span:nth-child(2)');

const sideMenu = document.querySelector("#sidebar");
const menuBtn = document.querySelector("#menu-btn");
const closeBtn = document.querySelector("#close-btn");

menuBtn.addEventListener('click', () => {
    sideMenu.style.display = 'block';
});

//đóng siderbar
closeBtn.addEventListener('click', () => {
    sideMenu.style.display = 'none';
});

// Kiểm tra kích thước màn hình và reset sidebar khi lớn hơn 768px
window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
        sideMenu.style.display = ''; // Reset về trạng thái mặc định của CSS
    }
});


//==================== Lấy trạng thái theme và icon từ localStorage
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

//============================================= lưu hiệu ứng active trên sidebar
document.addEventListener("DOMContentLoaded", function () {
    const sidebarLinks = document.querySelectorAll(".sidebar-menu a");

    // Nếu không có session flag -> đây là lần mở tab mới => xóa active cũ
    if (!sessionStorage.getItem("tabSessionStarted")) {
        localStorage.removeItem("activeSidebarLink");
        sessionStorage.setItem("tabSessionStarted", "true");
    }

    // Lấy đường dẫn active từ localStorage
    const activeLink = localStorage.getItem("activeSidebarLink");

    if (activeLink) {
        const link = document.querySelector(`.sidebar-menu a[href="${activeLink}"]`);
        if (link) {
            document.querySelector(".sidebar-menu a.active")?.classList.remove("active");
            link.classList.add("active");
        }
    }

    // Xử lý sự kiện click
    sidebarLinks.forEach(link => {
        link.addEventListener("click", function (event) {
            event.preventDefault(); // Ngăn chặn nhảy trang đột ngột

            document.querySelector(".sidebar-menu a.active")?.classList.remove("active");

            this.classList.add("active");

            // Lưu href vào localStorage
            localStorage.setItem("activeSidebarLink", this.getAttribute("href"));

            // Chuyển hướng sau một khoảng thời gian nhỏ để hiệu ứng không bị giật
            setTimeout(() => {
                window.location.href = this.getAttribute("href");
            }, 150);
        });
    });
});




//============================================ demo biểu đồ
document.addEventListener("DOMContentLoaded", function () {
    var options = {
        series: [
            {
                name: "Doanh thu",
                data: [2500000, 3000000, 2700000, 2900000, 3100000, 3200000, 3300000],
            },
            {
                name: "Chi phí",
                data: [2800000, 2500000, 2600000, 2400000, 2700000, 2800000, 2900000],
            },
            {
                name: "Thu nhập",
                data: [5000000, 5200000, 5100000, 5300000, 5500000, 5600000, 5700000],
            },
        ],
        chart: {
            type: "bar",
            height: 350,

        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: "55%",
            },
        },
        dataLabels: {
            enabled: false,
        },
        stroke: {
            show: true,
            width: 2,
        },
        xaxis: {
            categories: ["Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN"],
        },
        yaxis: {
            labels: {
                formatter: function (val) {
                    return val.toLocaleString("vi-VN") + " vnd";
                },
            },
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val.toLocaleString("vi-VN") + " vnd";
                },
            },
        },
    };

    var chart = new ApexCharts(document.querySelector("#chart"), options);
    chart.render();
});


// ============================ Xem trước ảnh
function previewImage(event, previewId) {
    var file = event.target.files[0];
    var reader = new FileReader();

    reader.onload = function (e) {
        var img = document.getElementById(previewId);
        img.src = e.target.result;  // Set the img src to the selected file data
    }

    reader.readAsDataURL(file);
}



//======================================== Edit ChapterImage===========================
document.querySelectorAll(".update-image-btn").forEach(button => {
    button.addEventListener("click", function () {
        let imageId = this.getAttribute("data-id");
        let oldImageUrl = this.getAttribute("data-old");

        let inputFile = this.previousElementSibling;
        inputFile.click();

        inputFile.addEventListener("change", function () {
            let file = inputFile.files[0];
            if (!file) return;

            let formData = new FormData();
            formData.append("id", imageId);
            formData.append("oldImageUrl", oldImageUrl);
            formData.append("newImage", file);

            fetch('/Admin/ChapterImage/UpdateImage', {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert(data.message);
                        location.reload(); // 🔄 Reload trang để cập nhật ảnh
                    } else {
                        alert("Cập nhật ảnh thất bại!");
                    }
                })
                .catch(error => console.error("Lỗi khi cập nhật ảnh:", error));
        });
    });
});

document.querySelectorAll(".delete-image-btn").forEach(button => {
    button.addEventListener("click", function () {
        let imageId = this.getAttribute("data-id");
        let oldImageUrl = this.getAttribute("data-old");

        if (!confirm("Bạn có chắc chắn muốn xóa ảnh này?")) return;

        let formData = new FormData();
        formData.append("id", imageId);
        formData.append("imageUrl", oldImageUrl);

        fetch('/Admin/ChapterImage/DeleteImage', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    location.reload(); // 🔄 Reload trang để cập nhật danh sách ảnh
                } else {
                    alert("Xóa ảnh thất bại!");
                }
            })
            .catch(error => console.error("Lỗi khi xóa ảnh:", error));
    });
});

document.addEventListener("DOMContentLoaded", function () {
    let backToTop = document.getElementById("backToTop");

    // Hiển thị nút khi cuộn xuống
    window.addEventListener("scroll", function () {
        if (window.scrollY > 200) {
            backToTop.style.display = "block";
        } else {
            backToTop.style.display = "none";
        }
    });

    // Cuộn lên đầu trang khi bấm
    backToTop.addEventListener("click", function () {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
});
document.addEventListener("DOMContentLoaded", function () {
    let backToTop = document.getElementById("backToTop");
    let uploadImage = document.getElementById("uploadImage");
    let imagePreview = document.getElementById("imagePreview");

    // Hiển thị nút khi cuộn xuống
    window.addEventListener("scroll", function () {
        backToTop.style.display = window.scrollY > 200 ? "block" : "none";
    });

    // Cuộn lên đầu trang khi bấm
    backToTop.addEventListener("click", function () {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });

    // Hiển thị ảnh khi chọn file
    uploadImage.addEventListener("change", function () {
        if (this.files && this.files[0]) {
            let reader = new FileReader();
            reader.onload = function (e) {
                imagePreview.innerHTML = `<img src="${e.target.result}" alt="Ảnh xem trước" style="width: 100%; height: 100%; object-fit: cover; border-radius: 6px;">`;
            };
            reader.readAsDataURL(this.files[0]);
        }
    });
});
document.addEventListener("DOMContentLoaded", function () {
    const deleteButton = document.querySelector("#deleteSelectedImages");

    if (deleteButton) {
        deleteButton.addEventListener("click", function () {
            let selectedImages = [];
            let selectedIds = [];

            document.querySelectorAll(".delete-checkbox:checked").forEach((checkbox) => {
                selectedImages.push(checkbox.getAttribute("data-url"));
                selectedIds.push(parseInt(checkbox.getAttribute("data-id")));
            });

            if (selectedImages.length === 0) {
                alert("Vui lòng chọn ít nhất một ảnh để xóa.");
                return;
            }

            if (!confirm("Bạn có chắc muốn xóa các ảnh đã chọn?")) return;

            fetch("/Admin/ChapterImage/DeleteMultipleImages", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    ImageIds: selectedIds,
                    ImageUrls: selectedImages,
                }),
            })
                .then(response => {
                    console.log("Raw response:", response);
                    return response.text();
                })
                .then(text => {
                    console.log("Response text:", text);
                    if (!text) {
                        throw new Error("Phản hồi rỗng từ server.");
                    }
                    return JSON.parse(text);
                })
                .then(data => {
                    if (data.success) {
                        alert("Xóa ảnh thành công!");
                        location.reload();
                    } else {
                        alert("Lỗi: " + data.message);
                    }
                })
                .catch(error => console.error("Lỗi:", error));

        });
    } else {
        console.error("Không tìm thấy nút xóa nhiều ảnh!");
    }
});

