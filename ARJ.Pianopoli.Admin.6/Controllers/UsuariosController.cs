using ARJ.Pianopoli.Admin._6.Areas.Identity.Pages.Account;
using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly DBContext db;
        
        private readonly UserManager<IdentityUser> _userManager;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IAspNetUser _user;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public UsuariosController(DBContext db, IWebHostEnvironment hostingEnvironment, IAspNetUser user, UserManager<IdentityUser> userManager, ILogger<RegisterModel> logger, IEmailSender emailSender)
        {
            this.db = db;
            _hostingEnvironment = hostingEnvironment;
            _user = user;
            _userManager= userManager;
            _logger = logger;
            _emailSender = emailSender;
        }


        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult ListarUsuarios()
        {
            var lista =  (from user in _userManager.Users
                                                   select new
                                                   {
                              id = user.Id,
                              username = user.UserName,
                              email = user.Email
                          }).ToList();
            
            return Json(new { data = lista });
        }

        [Authorize]
        public IActionResult NovoUsuario()
        {

            //var returnUrl = Url.Content("~/Usuarios");


            return View();
        }





    }
}
