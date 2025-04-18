using K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.ViewModels;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using K21CNT2_NguyenHaiDang_2110900067_DATN.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class NationController : Controller
    {

        private readonly K21CNT2_NguyenHaiDang_2110900067_DATNContext _context;

        public NationController(K21CNT2_NguyenHaiDang_2110900067_DATNContext context)
        {
            _context = context;

        }
        public IActionResult Index(string search, int page = 1)
        {
            int pageSize = 3;

            var query = _context.Nations.AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.NationName.Contains(search));
            }

            int totalNations = query.Count();

            var nations = query
                .OrderByDescending(r => r.NationId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginate = new Paginate(totalNations, page, pageSize);

            var viewmodel = new NationViewModel
            {
                Nations = nations,
                Paginate = paginate,
                SearchTerm = search
            };
            return View(viewmodel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(NationModel nation)
        {
            if (ModelState.IsValid)
            {
                _context.Nations.Add(nation);
                _context.SaveChanges();
                TempData["success"] = "Thêm quốc gia thành công";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var nation = _context.Nations.Find(id);
            if (nation == null) return NotFound();
            return View(nation);
        }

        [HttpPost]
        public IActionResult Edit(int id, NationModel nation)
        {
            if (id != nation.NationId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Nations.Update(nation);
                _context.SaveChanges();
                TempData["success"] = "Sửa quốc gia thành công";
                return RedirectToAction("Index");
            }

            return View(nation);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var nation = _context.Nations.Find(id);
            if (nation == null) return NotFound();
            return View(nation);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var nation = _context.Nations.Find(id);
            if (nation == null) return NotFound();
            _context.Nations.Remove(nation);
            _context.SaveChanges();
            TempData["success"] = "Xóa quốc gia thành công";
            return RedirectToAction("Index");
        }
    }
}
