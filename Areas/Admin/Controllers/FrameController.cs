using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class FrameController : Controller
    {
        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public FrameController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 6)
        {
            // Build the query based on search term
            var query = _context.Frames.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.FrameName.Contains(search));
            }

            int totalItems = await query.CountAsync(); // Count the total items after filtering by search

            // Calculate pagination details
            var paginate = new Paginate(totalItems, page, pageSize);

            // Get the list of frames for the current page
            var frames = await query
                .OrderBy(f => f.FrameId)
                .Skip((paginate.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create the ViewModel
            var viewModel = new FrameViewModel
            {
                Frame = frames,
                Pagination = paginate,
                SearchTerm = search
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(FrameModel frame, IFormFile frameImage)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu có ảnh được tải lên
                if (frameImage != null && frameImage.Length > 0)
                {
                    // Đặt đường dẫn lưu ảnh
                    var uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Frame");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Tạo tên ảnh mới
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(frameImage.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Lưu ảnh vào thư mục chỉ định
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await frameImage.CopyToAsync(stream);
                    }

                    // Cập nhật đường dẫn ảnh trong model
                    frame.FrameImage = "/media/frame_image/" + fileName;
                }
                else
                {
                    // Nếu không có ảnh tải lên, bạn có thể gán giá trị mặc định cho FrameImage
                    frame.FrameImage = "/media/frame_image/default.jpg";  // Ví dụ: đặt ảnh mặc định
                }

                // Lưu model vào cơ sở dữ liệu
                _context.Add(frame);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm khung thành công!";

                return RedirectToAction(nameof(Index));
            }

            TempData["error"] = "Dữ liệu không hợp lệ!";
            return View(frame);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var frame = await _context.Frames.FindAsync(id);
            if (frame == null)
            {
                return NotFound();
            }
            return View(frame);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FrameId, FrameName, FrameImage, IsFree, Coin")] FrameModel frame, IFormFile frameImage)
        {
            if (id != frame.FrameId)
            {
                return NotFound();
            }
            if (frameImage == null || frameImage.Length == 0)
            {
                // Loại bỏ FrameImage khỏi ModelState nếu không có ảnh mới
                ModelState.Remove("FrameImage");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin frame hiện tại từ cơ sở dữ liệu
                    var existingFrame = await _context.Frames.FindAsync(id);
                    if (existingFrame == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra nếu có ảnh mới tải lên
                    if (frameImage != null && frameImage.Length > 0)
                    {
                        var uploadPath = Path.Combine(@"D:\ASP\Image_Project\Frame");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingFrame.FrameImage))
                        {
                            var oldImagePath = Path.Combine(@"D:\ASP\Image_Project\Frame", Path.GetFileName(existingFrame.FrameImage));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu ảnh mới vào thư mục
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(frameImage.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await frameImage.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh vào trong cơ sở dữ liệu
                        existingFrame.FrameImage = "/media/frame_image/" + fileName;
                    }
                    // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                    else
                    {
                        existingFrame.FrameImage = existingFrame.FrameImage;
                    }

                    // Cập nhật các trường khác (bất kể có ảnh mới hay không)
                    existingFrame.FrameName = frame.FrameName;
                    existingFrame.IsFree = frame.IsFree;
                    existingFrame.Coin = frame.Coin;

                    // Lưu các thay đổi vào cơ sở dữ liệu
                    _context.Update(existingFrame);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FrameExists(frame.FrameId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["success"] = "Cập nhật khung thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(frame);
        }

        private bool FrameExists(int id)
        {
            return _context.Frames.Any(e => e.FrameId == id);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var frame = await _context.Frames
                .FirstOrDefaultAsync(m => m.FrameId == id);
            if (frame == null)
            {
                return NotFound();
            }

            return View(frame);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frame = await _context.Frames.FindAsync(id);
            if (frame == null)
            {
                return NotFound();
            }

            // Xóa tệp ảnh liên quan
            if (!string.IsNullOrEmpty(frame.FrameImage))
            {
                var imagePath = Path.Combine(@"D:\ASP\Image_Project\Frame", Path.GetFileName(frame.FrameImage));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);  // Xóa ảnh khỏi thư mục
                }
            }

            // Xóa khung trong cơ sở dữ liệu
            _context.Frames.Remove(frame);
            await _context.SaveChangesAsync();

            TempData["success"] = "Khung và ảnh đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
