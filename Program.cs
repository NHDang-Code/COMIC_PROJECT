using K21CNT2_NguyenHaiDang_2110900067_DATN.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace K21CNT2_NguyenHaiDang_2110900067_DATN
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;  // cho phép truy cap qua HTTP
                options.Cookie.IsEssential = true;  // ??m b?o cookie c?n thi?t cho session
                options.Cookie.Name = "HDDev";  // ??t tên cookie cho session
                options.IdleTimeout = TimeSpan.FromHours(1);  // Th?i gian h?t h?n c?a session
                options.Cookie.SameSite = SameSiteMode.Lax; ;  // Không c?n SSL cho localhost
            });

            builder.Services.AddDbContext<K21CNT2_NguyenHaiDang_2110900067_DATNContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
            });

            builder.Services.AddAuthentication()
                .AddCookie("AdminScheme", options =>
                {
                    options.LoginPath = "/Admin/Account/Login";
                    options.LogoutPath = "/Admin/Account/Logout";
                    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
                    options.Cookie.Name = "AdminAuthCookie";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.SlidingExpiration = true;
                })
                .AddCookie("UserScheme", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "UserAuthCookie";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.SlidingExpiration = true;
                });


            builder.Services.AddAuthorization();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(@"D:\ASP\Image_Project\Chapter_Image"),
                RequestPath = "/media/chapter_image"
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(@"D:\ASP\Image_Project\Frame"),
                RequestPath = "/media/frame_image"
            });

            //route cho default
            app.MapControllerRoute(
                name: "custom_home_index",
                pattern: "trang-chu",
                defaults: new { controller = "Home", action = "Index" }
            );

            //route cho areas/admin
            app.MapControllerRoute(
                name: "custom_dashboard_index",
                pattern: "admin/dashboard",
                defaults: new { area = "Admin", controller = "Dashboard", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_profile_index",
               pattern: "admin/trang-ca-nhan",
               defaults: new { area = "Admin", controller = "Profile", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_profile_create_comic",
               pattern: "admin/trang-ca-nhan/them-moi-truyen/{teamId?}",
               defaults: new { area = "Admin", controller = "Profile", action = "AddComic" }
            );

            app.MapControllerRoute(
               name: "custom_profile_edit_comic",
               pattern: "admin/trang-ca-nhan/chinh-sua-truyen/{id?}",
               defaults: new { area = "Admin", controller = "Profile", action = "EditComic" }
            );

            app.MapControllerRoute(
               name: "custom_profile_delete_comic",
               pattern: "admin/trang-ca-nhan/xoa-truyen/{id?}",
               defaults: new { area = "Admin", controller = "Profile", action = "Delete" }
            );

            app.MapControllerRoute(
               name: "custom_profile_details",
               pattern: "admin/trang-ca-nhan/chi-tiet/{id?}",
               defaults: new { area = "Admin", controller = "Profile", action = "Details" }
            );

            app.MapControllerRoute(
                name: "custom_chapter_create",
                pattern: "admin/chuong/them-moi-chuong/{comicId?}",
                defaults: new { area = "Admin", controller = "Chapter", action = "Create" }
            );

            app.MapControllerRoute(
                name: "custom_chapter_edit",
                pattern: "admin/chuong/chinh-sua-chuong/{id?}",
                defaults: new { area = "Admin", controller = "Chapter", action = "Edit" }
            );

            app.MapControllerRoute(
                name: "custom_chapter_details",
                pattern: "admin/chuong/chi-tiet/{id?}",
                defaults: new { area = "Admin", controller = "Chapter", action = "Details" }
            );

            app.MapControllerRoute(
                name: "custom_chapterimage_create",
                pattern: "admin/anh-chuong/them-moi-anh/{chapterId?}",
                defaults: new { area = "Admin", controller = "ChapterImage", action = "Create" }
            );

            app.MapControllerRoute(
                name: "custom_chapterimage_edit",
                pattern: "admin/anh-chuong/chinh-sua-anh/{id?}",
                defaults: new { area = "Admin", controller = "ChapterImage", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_comic_index",
               pattern: "admin/truyen-tranh",
               defaults: new { area = "Admin", controller = "Comic", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_comic_create",
               pattern: "admin/truyen-tranh/them-moi-truyen-tranh",
               defaults: new { area = "Admin", controller = "Comic", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_comic_edit",
               pattern: "admin/truyen-tranh/chinh-sua-truyen-tranh/{id?}",
               defaults: new { area = "Admin", controller = "Comic", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_comic_delete",
               pattern: "admin/truyen-tranh/xoa-truyen-tranh/{id?}",
               defaults: new { area = "Admin", controller = "Comic", action = "Delete" }
            );

            app.MapControllerRoute(
               name: "custom_genre_index",
               pattern: "admin/the-loai",
               defaults: new { area = "Admin", controller = "Genre", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_genre_create",
               pattern: "admin/the-loai/them-moi-the-loai",
               defaults: new { area = "Admin", controller = "Genre", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_genre_edit",
               pattern: "admin/the-loai/chinh-sua-the-loai/{id?}",
               defaults: new { area = "Admin", controller = "Genre", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_genre_delete",
               pattern: "admin/the-loai/xoa-the-loai/{id?}",
               defaults: new { area = "Admin", controller = "Genre", action = "Delete" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_index",
                pattern: "admin/nhom-dich",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "Index" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_add",
                pattern: "admin/nhom-dich/them-moi-nhom-dich",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "Create" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_edit",
                pattern: "admin/nhom-dich/chinh-sua-nhom-dich/{id?}",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "Edit" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_delete",
                pattern: "admin/nhom-dich/xoa-nhom-dich/{id?}",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "Delete" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_details",
                pattern: "admin/nhom-dich/chi-tiet-nhom-dich/{id?}",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "Details" }
            );

            app.MapControllerRoute(
                name: "custom_translation_team_add_member",
                pattern: "admin/nhom-dich/them-moi-thanh-vien/{teamId?}",
                defaults: new { area = "Admin", controller = "TranslationTeam", action = "AddMember" }
            );

            app.MapControllerRoute(
               name: "custom_role_index",
               pattern: "admin/quyen",
               defaults: new { area = "Admin", controller = "Role", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_role_create",
               pattern: "admin/quyen/them-moi-quyen",
               defaults: new { area = "Admin", controller = "Role", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_role_edit",
               pattern: "admin/quyen/chinh-sua-quyen/{id?}",
               defaults: new { area = "Admin", controller = "Role", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_role_delete",
               pattern: "admin/quyen/xoa-quyen/{id?}",
               defaults: new { area = "Admin", controller = "Role", action = "Delete" }
            );

            app.MapControllerRoute(
               name: "custom_user_index",
               pattern: "admin/nguoi-dung",
               defaults: new { area = "Admin", controller = "User", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_user_create",
               pattern: "admin/nguoi-dung/them-moi-nguoi-dung",
               defaults: new { area = "Admin", controller = "User", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_user_edit",
               pattern: "admin/nguoi-dung/chinh-sua-nguoi-dung/{id?}",
               defaults: new { area = "Admin", controller = "User", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_user_delete",
               pattern: "admin/nguoi-dung/xoa-nguoi-dung/{id?}",
               defaults: new { area = "Admin", controller = "User", action = "Delete" }
            );

            app.MapControllerRoute(
               name: "custom_frame_index",
               pattern: "admin/khung",
               defaults: new { area = "Admin", controller = "Frame", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_frame_create",
               pattern: "admin/khung/them-moi-khung",
               defaults: new { area = "Admin", controller = "Frame", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_frame_edit",
               pattern: "admin/khung/chinh-sua-khung/{id?}",
               defaults: new { area = "Admin", controller = "Frame", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_frame_delete",
               pattern: "admin/khung/xoa-khung/{id?}",
               defaults: new { area = "Admin", controller = "Frame", action = "Delete" }
            );


            app.MapControllerRoute(
               name: "custom_leveltype_index",
               pattern: "admin/kieu-cap-do",
               defaults: new { area = "Admin", controller = "LevelType", action = "Index" }
            );

            app.MapControllerRoute(
               name: "custom_leveltype_create",
               pattern: "admin/kieu-cap-do/them-moi-kieu-cap-do",
               defaults: new { area = "Admin", controller = "LevelType", action = "Create" }
            );

            app.MapControllerRoute(
               name: "custom_leveltype_details",
               pattern: "admin/kieu-cap-do/chi-tiet-kieu-cap-do/{id?}",
               defaults: new { area = "Admin", controller = "LevelType", action = "Details" }
            );

            app.MapControllerRoute(
               name: "custom_leveltype_edit",
               pattern: "admin/kieu-cap-do/chinh-sua-kieu-cap-do/{id?}",
               defaults: new { area = "Admin", controller = "LevelType", action = "Edit" }
            );

            app.MapControllerRoute(
               name: "custom_leveltype_delete",
               pattern: "admin/kieu-cap-do/xoa-kieu-cap-do/{id?}",
               defaults: new { area = "Admin", controller = "LevelType", action = "Delete" }
            );

            app.MapControllerRoute(
                name: "custom_levelmapping_create",
                pattern: "admin/loai-cap-do/them-moi-loai-cap-do/{levelTypeId?}",
                defaults: new { area = "Admin", controller = "LevelMapping", action = "Create" }
            );
            app.MapControllerRoute(
                name: "custom_levelmapping_edit",
                pattern: "admin/loai-cap-do/chinh-sua-loai-cap-do/{id?}",
                defaults: new { area = "Admin", controller = "LevelMapping", action = "Edit" }
            );

            app.MapControllerRoute(
                name: "custom_levelmapping_delete",
                pattern: "admin/loai-cap-do/xoa-loai-cap-do/{id?}",
                defaults: new { area = "Admin", controller = "LevelMapping", action = "Delete" }
            );

            app.MapControllerRoute(
                name: "custom_account_login",
                pattern: "admin/dang-nhap",
                defaults: new { area = "Admin", controller = "Account", action = "Login" }
            );

           

            app.MapControllerRoute(
              name: "Areas",
              pattern: "{area:exists}/{controller=Profile}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
