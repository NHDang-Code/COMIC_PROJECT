function toggleLike(comicId) {
    $.ajax({
        url: '/Home/Like',
        type: 'POST',
        data: { comicId: comicId },
        success: function () {
            let btn = $("#like-btn");
            if (btn.hasClass("liked")) {
                btn.removeClass("liked").text("Yêu Thích");
                toastr.info("Đã bỏ yêu thích truyện.");
            } else {
                btn.addClass("liked").text("Đã Thích");
                toastr.success("Bạn đã yêu thích truyện.");
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
