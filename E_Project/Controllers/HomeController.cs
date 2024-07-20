using E_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Hosting.Internal;

namespace E_Project.Controllers
{

    public class HomeController : Controller
    {
		private readonly FullWebsiteContext _context;

		public HomeController(FullWebsiteContext context, IWebHostEnvironment hostingEnvironment)
		{
			_context = context;
		}

        public IActionResult EditImage(int id)
        {
            var template = _context.Templates.Find(id);
            if (template == null)
            {
                return NotFound();
            }
            ViewBag.ImagePath = Url.Content("~/assets/uploadsimages/" + template.ImagePath);
            return View(new { ImagePath = ViewBag.ImagePath });
        }

        [HttpPost]
        public async Task<IActionResult> SaveEditedImage([FromBody] EditedImageModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false });
            }

            var base64Data = model.ImageData.Replace("data:image/jpeg;base64,", "");
            var imageBytes = Convert.FromBase64String(base64Data);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/uploadsimages/edited-image.jpg");
            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            return Json(new { success = true });
        }


       
        public IActionResult Index()
        {
			ViewBag.Template = _context.Templates.ToList();
			var username = HttpContext.Session.GetString("Username");
            ViewBag.SessionMessage = string.IsNullOrEmpty(username) ? null : username;
            return View();
        }
        

        [HttpPost]
        public IActionResult Contact(CombinedView model)
        {
            _context.Contacts.Add(model.C_Contact);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Feedback(CombinedView model)
        {
			model.C_Feedback.FeedbackDate = DateTime.Now;
			_context.Feedbacks.Add(model.C_Feedback);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Subscribe(CombinedView model)
        {
            model.C_Subscribe.Status = "Subscribe";
            model.C_Subscribe.SubscriptionStartDate = DateOnly.FromDateTime(DateTime.Now);
            model.C_Subscribe.SubscriptionEndDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));

            _context.SubscriptionDetails.Add(model.C_Subscribe);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Register(CombinedView model)
        {
            model.C_User.Role = "User";
            model.C_User.SubscriptionStatus = "Active";

            _context.Users.Add(model.C_User);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Addcarts()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(CombinedView model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.C_User.Email && u.Password == model.C_User.Password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Username or Password is incorrect";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

	}
    public class EditedImageModel
    {
        public string ImageData { get; set; }
    }
}
