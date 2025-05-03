$(document).ready(function () {
    //Gửi bình luận gốc
    $('#submit-comment').click(function () {
        var content = $('#comment-content').val().trim();
        var comicId = $('#comment-content').data('comic-id');

        if (!comicId) {
            toastr.error("Không xác định được truyện.");
            return;
        }

        if (content === '') {
            toastr.warning("Nội dung bình luận không được để trống.");
            return;
        }

        
        if (content.length > 5000) {
            toastr.warning("Nội dung bình luận không được vượt quá 5000 ký tự.");
            return;
        }

        $.ajax({
            url: '/Comment/AddComment',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ ComicId: comicId, Content: content, ParentId: null }),
            success: function (response) {
                if (response.success) {
                    $('#comment-content').val('');
                    if (response.html) {
                        
                        $('#notification-container').html(response.html).fadeIn(500);
                        setTimeout(function () {
                            location.reload();
                        }, 100);
                    }
                    toastr.success(response.message);
                } else {
                    toastr.error(response.message || "Đã xảy ra lỗi.");
                }
            },
            error: function (xhr) {
                toastr.error("Lỗi: " + xhr.responseText);
            }
        });
    });

    //Gửi bình luận khi nhấn Enter (tránh Shift + Enter)
    $('#comment-content').keypress(function (e) {
        if (e.which === 13 && !e.shiftKey) {
            e.preventDefault();
            $('#submit-comment').click();
        }
    });

    

    //Gửi bình luận con
    $(document).on('click', '.submit-reply', function () {
        var commentIdRaw = $(this).attr('data-parent-id');
        if (!commentIdRaw || commentIdRaw.includes("??")) {
            toastr.error("Lỗi dữ liệu commentId.");
            return;
        }

        var commentId = parseInt(commentIdRaw.split(" ")[0]);
        if (isNaN(commentId)) {
            toastr.error("commentId không hợp lệ.");
            return;
        }

        var repliedToUserId = $(this).data('replied-user-id');
        var comicId = $('#comment-content').data('comic-id');
        var replyForm = $('#reply-form-' + commentId);

        if (replyForm.length === 0) {
            toastr.error("Không tìm thấy form trả lời.");
            return;
        }

        replyForm.removeClass('d-none').show();
        var replyTextarea = replyForm.find('.reply-content');
        var content = replyTextarea.val().trim();

        if (content.length > 5000) {
            toastr.warning("Bình luận không được vượt quá 5000 ký tự.");
            return;
        }
        
        content = content.replace(/^@\S+(\s+)?/, "").trim();

        if (!comicId) {
            toastr.error("Không xác định được truyện.");
            return;
        }

        if (!content || content.length < 1) {
            toastr.warning('Nội dung trả lời không được để trống.');
            replyTextarea.focus();
            return;
        }

        var cleanedContent = content;

        $.ajax({
            url: '/Comment/AddComment',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                ComicId: comicId,
                Content: cleanedContent,
                ParentId: commentId,
                RepliedToUserId: repliedToUserId
            }),
            success: function (response) {
                if (response.success) {  
                    $('#comment-content').val('');

                    if (response.html) {
                        $('#notification-container').html(response.html).fadeIn(500);

                        setTimeout(function () {
                            location.reload();
                        }, 100);
                    }

                    
                    toastr.success(response.message);

                } else {
                    toastr.error(response.message || "Đã xảy ra lỗi.");
                }
            },
            error: function (xhr) {
                toastr.error("Lỗi: " + xhr.responseText);
            }
        });
    });




    // Gửi bình luận con khi nhấn Enter (tránh Shift + Enter)
    $(document).on('keypress', '.reply-content', function (e) {
        if (e.which === 13 && !e.shiftKey) {
            e.preventDefault();
            $(this).closest('.reply-form').find('.submit-reply').click();
        }
    });



    // Ẩn form trả lời khi click ra ngoài form trả lời hoặc nút "Trả Lời"
    $(document).click(function (event) {
        var target = $(event.target);

        if (!target.closest('.reply-form').length && !target.closest('.reply-btn').length) {
            $('.reply-form').addClass('d-none');
        }
    });


    // Ngừng sự kiện lan truyền khi click vào trong .reply-form
    $(document).on('click', '.reply-form', function (event) {
        event.stopPropagation();
    });



    //Mở form trả lời bình luận và tag tên người được trả lời
    $(document).on('click', '.reply-btn', function () {
        var commentId = $(this).data('comment-id');
        var fullUserName = $(this).closest('.comment').find('.comment-username').first().text().trim();
        var replyForm = $('#reply-form-' + commentId);

        if (replyForm.hasClass('d-none')) {
            replyForm.removeClass('d-none');
        }

        var textarea = replyForm.find('.reply-content');

       
        var cleanUserName = fullUserName.replace(/\s*\(.*?\)/g, "").trim();

        
        var parts = cleanUserName.split(" ");
        var lastName = parts.pop();

        
        var prefix = "@" + lastName + " ";

        
        if (!textarea.val().startsWith(prefix)) {
            textarea.val(prefix).focus();
        }

        textarea.off('input.protect').on('input.protect', function () {
            if (!$(this).val().startsWith(prefix)) {
                var current = $(this).val().replace(prefix, "").trimStart();
                $(this).val(prefix + current);
            }
        });

        textarea.off('keydown.protect').on('keydown.protect', function (e) {
            var caretPos = this.selectionStart;
            if ((e.key === "Backspace" || e.key === "ArrowLeft") && caretPos <= prefix.length) {
                e.preventDefault();
                this.setSelectionRange(prefix.length, prefix.length);
            }
        });
    });




    //Ẩn hiện bình luận con
    $(document).ready(function () {
        $('.toggle-replies-btn').each(function () {
            var commentId = $(this).data('comment-id');
            var repliesContainer = $('#replies-' + commentId);
            var button = $(this);

            var storedState = localStorage.getItem('replies-' + commentId);
            if (storedState === 'visible') {
                repliesContainer.removeClass('d-none');
                button.text('Ẩn bình luận');
            } else {
                repliesContainer.addClass('d-none');
                button.text('Hiện bình luận');
            }

            if (repliesContainer.children().length === 0) {
                button.hide();
            }
        });

        $(document).on('click', '.toggle-replies-btn', function () {
            var commentId = $(this).data('comment-id');
            var repliesContainer = $('#replies-' + commentId);
            var button = $(this);

            if (repliesContainer.hasClass('d-none')) {
                repliesContainer.removeClass('d-none');
                button.text('Ẩn bình luận');
                localStorage.setItem('replies-' + commentId, 'visible');
            } else {
                repliesContainer.addClass('d-none');
                button.text('Hiện bình luận');
                localStorage.setItem('replies-' + commentId, 'hidden');
            }
        });
    });


 
});



