using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class RoleController : Controller
    {

        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public RoleController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, int page = 1)
        {
            int pageSize = 10;

            // Start with all roles
            var query = _context.Roles.AsQueryable();

            // Filter the roles based on the search term
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.RoleName.Contains(search));
            }

            int totalRoles = query.Count();

            var roles = query
                .OrderBy(r => r.RoleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginate = new Paginate(totalRoles, page, pageSize);

            var viewmodel = new RoleViewModel
            {
                Roles = roles,
                Paginate = paginate,
                SearchTerm = search // Include the search term in the view model
            };
            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RoleModel role)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(role);
                _context.SaveChanges();
                TempData["success"] = "Thêm thành công";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost]
        public IActionResult Edit(int id, RoleModel role)
        {
            if (id != role.RoleId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Roles.Update(role);
                _context.SaveChanges();
                TempData["success"] = "Sửa thành công";
                return RedirectToAction("Index");
            }

            return View(role);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = _context.Roles.Find(id);
            if (role == null) return NotFound();
            _context.Roles.Remove(role);
            _context.SaveChanges();
            TempData["success"] = "Xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
