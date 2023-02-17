using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class MapaController : Controller
    {
        private readonly ILogger<MapaController> _logger;
        private readonly DBContext db;

        public MapaController(ILogger<MapaController> logger, DBContext db)
        {
            _logger = logger;
            this.db = db;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult BuscarLoteamento(int loteamento)
        {
            var lista = new List<MapaPianopoliViewModel>();
            var sql = (from f in db.Lotes
                       where f.LoteamentoId == loteamento
                       select new MapaPianopoliViewModel
                       {
                           situacao = f.SituacaoNoSite,
                           lote = f.Lote,
                           quadra = f.Quadra,
                           area = f.Area
                       }).ToList();

            var propostas = (from f in db.Propostas
                             where f.LoteamentoId == 7
                             select f
                             ).ToList();

            foreach (var item in sql)
            {
                item.mensagem = "quadra: " + item.quadra + " - lote: " + item.lote + " - área: " + item.area + " ";
                var proposta = propostas.Where(c => c.Lote == item.lote).FirstOrDefault();
                if (proposta != null)
                {
                    if (item.situacao == "2" )
                    {
                        item.mensagem = item.mensagem + " Reservado";
                    }
                    else if (item.situacao == "3")
                    {
                        item.mensagem = item.mensagem + " Vendido";
                    }


                    // não é para usar a reserva com fim automático após 5 dias
                    //DateTime datadaproposta = proposta.DataProposta.Date;
                    //if (item.situacao == "2" && DateTime.Now.Date <= datadaproposta.AddDays(5).Date  )
                    //{
                    //    item.mensagem = item.mensagem + " Rsv até " + datadaproposta.AddDays(5).ToShortDateString();
                    //}
                    //else if (item.situacao == "2" && DateTime.Now.Date > datadaproposta.AddDays(5).Date )
                    //{
                    //    item.situacao = "1";
                    //}
                    //else if (item.situacao == "3")
                    //{
                    //    item.mensagem = item.mensagem + " Vendido";
                    //}
                }

                lista.Add(item);

            }
            return Json(lista);

        }
    }
}
