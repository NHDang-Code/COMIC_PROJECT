function toggleFollow(comicId) {
    $.ajax({
        url: '/Home/Follow',
        type: 'POST',
        data: { comicId: comicId },
        success: function () {
            let btn = $("#follow-btn");
            if (btn.hasClass("followed")) {
                btn.removeClass("followed").text("Theo Dõi");
                toastr.info("Đã bỏ theo dõi truyện.");
            } else {
                btn.addClass("followed").text("Đã Theo Dõi");
                toastr.success("Bạn đã theo dõi truyện.");
            }
            setTimeout(() => location.reload(), 800);
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                toastr.warning("Bạn hãy đăng nhập để dùng chức năng.");
                
            } else {
                toastr.error("Đã xảy ra lỗi. Vui lòng thử lại sau.");
            }
        }
    });
}
