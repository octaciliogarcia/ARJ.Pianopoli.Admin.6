using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class TabelasController : Controller
    {
        private readonly DBContext db;
        static BaseFont fontebase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IAspNetUser _user;
        public TabelasController(DBContext db, IWebHostEnvironment hostingEnvironment, IAspNetUser user)
        {
            this.db = db;
            _hostingEnvironment = hostingEnvironment;
            _user = user;

        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Precos()
        {
            return View();
        }
    }
}
