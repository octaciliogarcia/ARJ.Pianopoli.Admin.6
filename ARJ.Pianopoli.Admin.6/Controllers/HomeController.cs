using ARJ.Pianopoli.Admin._6.Models;
using Microsoft.AspNetCore.Mvc;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class HomeController : MainController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("error/{id:length(3,3)}")]
        public IActionResult Error(int id)
        {
            var modelError = new ErrorViewModel();

            if (id == 500)
            {
                modelError.Message = "Infelizmente, um erro aconteceu! Tente novamente daqui alguns minutos ou contate nosso suporte.";
                modelError.Title = "Um erro aconteceu!";
                modelError.ErroCode = id;
            }
            else if (id == 404)
            {
                modelError.Message =
                    "A página que você está procurando não existe! <br />Se você acredita que deveria existir, contate o suporte por favor.";
                modelError.Title = "Ops! Página não encontrada.";
                modelError.ErroCode = id;
            }
            else if (id == 403)
            {
                modelError.Message = "Você não pode acessar.";
                modelError.Title = "Acesso negado!";
                modelError.ErroCode = id;
            }
            else
            {
                return StatusCode(404);
            }

            return View("Error", modelError);
        }

    }

}
