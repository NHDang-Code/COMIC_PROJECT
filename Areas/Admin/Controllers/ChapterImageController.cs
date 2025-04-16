using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin, Translator")]
    public class ChapterImageController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public ChapterImageController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.ChapterImages.Include(c => c.Chapter).ToListAsync();
            return View(images);
        }

        [HttpGet]
        public IActionResult Create(int? chapterId)
        {
            var chapters = _context.Chapters.ToList();
            ViewBag.Chapters = chapters;

            // Nếu có chapterId, lấy thông tin chapter
            if (chapterId.HasValue)
            {
                var chapter = _context.Chapters.Find(chapterId.Value);
                if (chapter != null)
                {
                    ViewBag.ChapterId = chapter.ChapterId;
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterImageModel model, int chapterId)
        {
            if (ModelState.IsValid)
            {
                var imageUrls = new List<string>();

                if (model.UploadedImages != null && model.UploadedImages.Count > 0)
                {
                    string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var image in model.UploadedImages)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        imageUrls.Add("/media/chapter_image/" + fileName);
                    }

                    model.ImageUrls = JsonSerializer.Serialize(imageUrls);
                }

                model.ChapterId = chapterId; // Gán ID chương cho ảnh
                _context.ChapterImages.Add(model);
                await _context.SaveChangesAsync();

                TempData["success"] = "Thêm ảnh thành công!";
                return RedirectToAction("Details", "Chapter", new { id = chapterId });
            }

            return View(model);
        }

        //------------------------------------------Sửa Ảnh-------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var image = await _context.ChapterImages.FindAsync(id);
            if (image == null) return NotFound();

            // Giải mã danh sách ảnh từ JSON
            ViewBag.ImageUrls = !string.IsNullOrEmpty(image.ImageUrls)
                ? JsonSerializer.Deserialize<List<string>>(image.ImageUrls)
                : new List<string>();

            return View(image);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<IFormFile> newImages)
        {
            var image = await _context.ChapterImages.FindAsync(id);
            if (image == null) return NotFound();

            // Lấy danh sách ảnh hiện tại từ JSON
            var imageUrls = JsonSerializer.Deserialize<List<string>>(image.ImageUrls) ?? new List<string>();

            // Xử lý ảnh mới nếu có tải lên
            if (newImages != null && newImages.Count > 0)
            {
                string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");

                // Kiểm tra thư mục lưu ảnh có tồn tại không
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var file in newImages)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Lưu ảnh vào thư mục
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Thêm đường dẫn mới vào danh sách JSON
                    imageUrls.Add("/media/chapter_image/" + fileName);
                }
            }

            // Cập nhật JSON trong DB
            image.ImageUrls = JsonSerializer.Serialize(imageUrls);
            await _context.SaveChangesAsync();

            TempData["success"] = "Cập nhật ảnh thành công!";
            return RedirectToAction("Edit", new { id });
        }

        //-----------------------------------------Xóa tất cả ảnh--------------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _context.ChapterImages.FindAsync(id);
            if (image == null)
            {
                TempData["error"] = "Không tìm thấy dữ liệu!";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrEmpty(image.ImageUrls))
            {
                image.ImageUrls = "[]";
            }

            // 1️ Lấy danh sách ảnh từ ImageUrls (chuỗi JSON)
            var imageUrls = JsonSerializer.Deserialize<List<string>>(image.ImageUrls) ?? new List<string>();

            // 2️ Xác định thư mục lưu ảnh
            string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");

            // 3️ Xóa từng file có trong ImageUrls
            foreach (var imageUrl in imageUrls)
            {
                string filePath = Path.Combine(uploadPath, Path.GetFileName(imageUrl));

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Lưu ChapterId để chuyển hướng sau khi xóa
            int chapterId = image.ChapterId;

            // 4️⃣ Xóa dữ liệu trong database
            _context.ChapterImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["success"] = "Xóa tất cả ảnh và dữ liệu thành công!";

            // Chuyển hướng về trang Chapter Details theo chapterId
            return RedirectToAction("Details", "Chapter", new { id = chapterId });
        }

        /*---------------------------Xóa từng ảnh---------------------------------*/
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id, string imageUrl)
        {
            var image = await _context.ChapterImages.FindAsync(id);
            if (image == null)
            {
                return Json(new { success = false, message = "Không tìm thấy ảnh!" });
            }

            var imageUrls = JsonSerializer.Deserialize<List<string>>(image.ImageUrls) ?? new List<string>();

            if (imageUrls.Contains(imageUrl))
            {
                imageUrls.Remove(imageUrl);
                image.ImageUrls = JsonSerializer.Serialize(imageUrls);

                // Xóa file vật lý trong thư mục
                string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");
                string imagePath = Path.Combine(uploadPath, Path.GetFileName(imageUrl));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.Update(image);
                await _context.SaveChangesAsync();
                TempData["success"] = "Xóa ảnh thành công!";


                return Json(new { success = true, message = "Xóa ảnh thành công!" });
            }

            return Json(new { success = false, message = "Ảnh không tồn tại trong danh sách!" });
        }

        //----------------------------------- Cập nhật ảnh đơn----------------------
        [HttpPost]
        public async Task<IActionResult> UpdateImage(int id, string oldImageUrl, IFormFile newImage)
        {
            var image = await _context.ChapterImages.FindAsync(id);
            if (image == null) return NotFound();

            var imageUrls = JsonSerializer.Deserialize<List<string>>(image.ImageUrls) ?? new List<string>();

            if (!imageUrls.Contains(oldImageUrl))
            {
                return Json(new { success = false, message = "Ảnh cũ không tồn tại!" });
            }

            if (newImage != null && newImage.Length > 0)
            {
                // Định dạng thư mục lưu ảnh
                string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");

                // Tạo tên file mới
                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(newImage.FileName);
                string newFilePath = Path.Combine(uploadPath, newFileName);

                // Lưu file mới
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await newImage.CopyToAsync(stream);
                }

                // Xóa ảnh cũ khỏi thư mục
                string oldFilePath = Path.Combine(uploadPath, Path.GetFileName(oldImageUrl));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Cập nhật danh sách ảnh
                int index = imageUrls.IndexOf(oldImageUrl);
                if (index != -1)
                {
                    imageUrls[index] = "/media/chapter_image/" + newFileName;
                }

                image.ImageUrls = JsonSerializer.Serialize(imageUrls);
                _context.Update(image);
                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật ảnh thành công!";


                return Json(new { success = true, newUrl = "/media/chapter_image/" + newFileName, message = "Cập nhật ảnh thành công!" });
            }

            return Json(new { success = false, message = "Ảnh mới không hợp lệ!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMultipleImages([FromBody] DeleteImagesRequest request)
        {
            if (request.ImageIds == null || request.ImageUrls == null || request.ImageIds.Count == 0)
            {
                Console.WriteLine("Lỗi: Không có ảnh nào được chọn!");
                return Json(new { success = false, message = "Không có ảnh nào được chọn!" });
            }

            bool deletedAny = false;

            foreach (var id in request.ImageIds)
            {
                var image = await _context.ChapterImages.FindAsync(id);
                if (image == null) continue;

                var imageUrls = JsonSerializer.Deserialize<List<string>>(image.ImageUrls) ?? new List<string>();

                foreach (var imageUrl in request.ImageUrls)
                {
                    if (imageUrls.Contains(imageUrl))
                    {
                        imageUrls.Remove(imageUrl);

                        // Xóa file vật lý trong thư mục
                        string uploadPath = Path.Combine("D:", "ASP", "Image_Project", "Chapter_Image");
                        string imagePath = Path.Combine(uploadPath, Path.GetFileName(imageUrl));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }

                        deletedAny = true;
                    }
                }

                // Cập nhật danh sách ảnh trong database
                image.ImageUrls = JsonSerializer.Serialize(imageUrls);
                _context.Update(image);
            }

            if (deletedAny)
            {
                await _context.SaveChangesAsync();
                var response = new { success = true, message = "Xóa ảnh thành công!" };
                Console.WriteLine("Phản hồi từ API: " + JsonSerializer.Serialize(response)); // In ra log
                return Json(response);
            }

            var failResponse = new { success = false, message = "Không tìm thấy ảnh để xóa!" };
            Console.WriteLine("Phản hồi từ API: " + JsonSerializer.Serialize(failResponse)); // In ra log
            return Json(failResponse);
        }


        // DTO nhận danh sách ảnh từ client
        public class DeleteImagesRequest
        {
            public List<int> ImageIds { get; set; }
            public List<string> ImageUrls { get; set; }
        }
    }
}
