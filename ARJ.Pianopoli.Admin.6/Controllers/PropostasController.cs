using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using ARJ.Pianopoli.Admin._6.Utils;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using SelectPdf;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Document = iTextSharp.text.Document;
using Font = iTextSharp.text.Font;
using PageSize = iTextSharp.text.PageSize;
using Paragraph = iTextSharp.text.Paragraph;


namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class PropostasController : Controller
    {
        private readonly DBContext db;
        static BaseFont fontebase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IAspNetUser _user;



        public PropostasController(DBContext db, IWebHostEnvironment hostingEnvironment, IAspNetUser user)
        {
            this.db = db;
            _hostingEnvironment = hostingEnvironment;
            _user = user;
        }
        [Authorize]
        public IActionResult Index()
        {

            ViewBag.Loteamento = (from f in db.Loteamentos
                                  select new SelectListItem
                                  {
                                      Text = f.Nome,
                                      Value = f.Id.ToString()
                                  });
            ViewBag.Corretor = (from f in db.Corretores
                                select new SelectListItem
                                {
                                    Text = f.Nome,
                                    Value = f.Id.ToString()
                                });

            ViewBag.EstadoCivil = (from f in db.EstadoCivil
                                   select new SelectListItem
                                   {
                                       Text = f.Descricao,
                                       Value = f.Id.ToString()
                                   });
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToPage("/");
            }
        }

        [Authorize]
        public async Task<IActionResult> ListarPropostas()
        {
            var lista = await db.Procedures.SP_LISTAR_LOTESAsync(7);

            var retorno = (from x in lista
                           select new ListaLotesViewModel()
                           {
                               Id = x.Id,
                               LoteamentoId = (int)x.LoteamentoId,
                               Quadra = x.Quadra,
                               Lote = (int)x.Lote,
                               Area = String.Format("{0:0,0.00}", x.Area),
                               SituacaoNoSite = ""
                           }).ToList();

            foreach (var item in retorno)
            {
                try
                {
                    var obj = db.Propostas.Where(c => c.LoteamentoId == item.LoteamentoId && c.Quadra == item.Quadra && c.Lote == item.Lote).FirstOrDefault();
                    if (obj != null)
                        item.SituacaoNoSite = obj.Contrato == null ? "Reservado" : "Vendido";
                    else
                        item.SituacaoNoSite = "Disponível";

                }
                catch (Exception)
                {

                    throw;
                }
            }

            return Json(new { data = retorno });
        }
        [HttpPost]
        [Authorize]
        public IActionResult EditarProposta(int Loteamento, string Quadra, int Lote)
        {
            if (ModelState.IsValid)
            {
                var usuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var modelo = new PropostaViewModel();
                var lote = db.Lotes.Where(c => c.LoteamentoId == Loteamento && c.Quadra == Quadra && c.Lote == Lote).FirstOrDefault();
                // monta dados do lote 
                modelo.Lote = Lote;
                modelo.Quadra = Quadra;
                modelo.Area = lote.Area;
                modelo.LoteamentoId = Loteamento;
                modelo.PrecoM2 = db.TabelaM2.Where(c => c.CategoriaId == lote.CategoriaId).FirstOrDefault().ValorM2;
                modelo.CorFundo = db.TabelaM2.Where(c => c.CategoriaId == lote.CategoriaId).FirstOrDefault().CorFundo ?? "#ffffff";

                if (lote.SituacaoNoSite == "1")
                {
                    modelo.StatusNoSite = "Disponível";
                    // busca o valor total do lote quando este ainda está disponível e sem proposta enviada.
                    var precoVenda = Math.Round(modelo.PrecoM2 * lote.Area, 2); //db.TabelaPrecoLotes.Where(c => c.Quadra == Quadra && c.Lote == Lote).FirstOrDefault().PrecoVenda;

                    modelo.ValorCorretagem = Math.Round(precoVenda * 0.04m, 2);

                    modelo.ValorTotal = (precoVenda);

                    double vlrParcSemestral = 7500.00;   // deverár ser parametrizado
                    double jurosAM = 0.0025;
                    double constante = 1;
                    var entradaPadrao = Math.Round(precoVenda * 0.15m, 2);
                    var saldoPadrao = precoVenda - entradaPadrao;


                    // var indice = Math.Pow((constante + jurosAM), 6) ;

                    var VP6 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 6)), 2);
                    var VP12 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 12)), 2);
                    var VP18 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 18)), 2);
                    var VP24 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 24)), 2);
                    var VP30 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 30)), 2);
                    var VP36 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 36)), 2);
                    var VP42 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 42)), 2);
                    var VP48 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 48)), 2);
                    var VP54 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 54)), 2);
                    var VP60 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 60)), 2);

                    var parc12 = 7000.00m;
                    var parc24 = Math.Round(((((double)(saldoPadrao * 0.60m)) - (VP6 + VP12 + VP18 + VP24)) * (((jurosAM * (Math.Pow(1 + jurosAM, 24))) / Math.Pow(1 + jurosAM, 24)) - 1)) / -24, 2);
                    var parc36 = Math.Round(((((double)(saldoPadrao * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36)) * (((jurosAM * (Math.Pow(1 + jurosAM, 36))) / Math.Pow(1 + jurosAM, 36)) - 1)) / -36, 2);
                    var parc48 = Math.Round(((((double)(saldoPadrao * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48)) * (((jurosAM * (Math.Pow(1 + jurosAM, 48))) / Math.Pow(1 + jurosAM, 48)) - 1)) / -48, 2);
                    var parc60 = Math.Round(((((double)(saldoPadrao * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48 + VP54 + VP60)) * (((jurosAM * (Math.Pow(1 + jurosAM, 60))) / Math.Pow(1 + jurosAM, 60)) - 1)) / -60, 2);

                    var listaTabela = new List<SelectListItem>();
                    listaTabela.Add(new SelectListItem
                    {
                        Text = " 12 x R$ " + String.Format("{0:0,0.00}", parc12) + " +  2 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                        Value = "1"
                    });
                    listaTabela.Add(new SelectListItem
                    {
                        Text = " 24 x R$ " + String.Format("{0:0,0.00}", parc24) + " +  4 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                        Value = "2"
                    });
                    listaTabela.Add(new SelectListItem
                    {
                        Text = " 36 x R$ " + String.Format("{0:0,0.00}", parc36) + " +  6 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                        Value = "3"
                    });
                    listaTabela.Add(new SelectListItem
                    {
                        Text = " 48 x R$ " + String.Format("{0:0,0.00}", parc48) + " +  8 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                        Value = "4"
                    });
                    listaTabela.Add(new SelectListItem
                    {
                        Text = " 60 x R$ " + String.Format("{0:0,0.00}", parc60) + " + 10 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                        Value = "5"
                    });

                    ViewBag.Mensais = listaTabela;


                    //ViewBag.Mensais = (from f in db.TabelaPrecoLotes
                    //                   where f.Quadra == Quadra && f.Lote == Lote
                    //                   select new SelectListItem
                    //                   {
                    //                       Text = " " + f.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasMensais) + " +   " + f.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasSemestrais) + "   ",
                    //                       Value = f.Id.ToString()
                    //                   });
                    return PartialView("EditarProposta", modelo);

                }
                else
                {

                    var proposta = db.Propostas.Where(c => c.LoteamentoId == Loteamento && c.Quadra == Quadra && c.Lote == Lote).FirstOrDefault();
                    if (proposta != null)
                    {


                        modelo.ValorTotal = proposta.ValorTotal;
                        modelo.DataProposta = proposta.DataProposta;
                        var propostaCondicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == proposta.Id).FirstOrDefault();
                        var compradores = (from x in db.PropostasCompradores
                                           join c in db.Comprador on x.CompradorId equals c.Id
                                           where x.PropostaId == proposta.Id
                                           select c).ToList();

                        foreach (var item in compradores)
                        {
                            item.Celular = Convert.ToUInt64(item.Celular).ToString(@"(00) 00000-0000");
                            item.Cpf = Convert.ToUInt64(item.Cpf).ToString(@"000\.000\.000\-00");
                        }


                        var statusnosite = "";
                        if (proposta.Status == 2)
                            statusnosite = "Reservado";
                        else
                            statusnosite = "Vendido";

                        //var valorConverter = 2421457943.54m;

                        //var extenso = ValorExtenso.ExtensoReal(valorConverter);


                        var retorno = new PropostaViewModel()
                        {
                            StatusNoSite = statusnosite,
                            Id = proposta.Id,
                            PropostaId = proposta.Id,
                            LoteamentoId = Loteamento,
                            Lote = Lote,
                            Quadra = Quadra,
                            Area = lote.Area,
                            PrecoM2 = modelo.PrecoM2,
                            CorFundo = modelo.CorFundo,
                            DataProposta = proposta.DataProposta,
                            DataAprovacao = proposta.DataAprovacao,
                            Entrada = String.Format("{0:0,0.00}", propostaCondicoes.ValorEntrada),
                            ValorTotal = propostaCondicoes.ValorTotal,
                            ValorCorretagem = proposta.ValorCorretagem,
                            SaldoPagar = String.Format("{0:0,0.00}", (proposta.ValorTotal - propostaCondicoes.ValorEntrada)),
                            TipoPagamento = proposta.ValorTotal == propostaCondicoes.ValorEntrada ? "a vista" : "a prazo",
                            Parcelamento = propostaCondicoes.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaMensal) + " + " + propostaCondicoes.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaSemestral),
                            ParcelamentoMensal = propostaCondicoes.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaMensal),
                            ParcelamentoSemestral = propostaCondicoes.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaSemestral),
                            Compradores = compradores,
                            PrimeiroVencMensal = (DateTime)proposta.PrimeiroVencMensal,
                            PrimeiroVencSemestral = (DateTime)proposta.PrimeiroVencSemestral,
                            TotalParcelas = String.Format("{0:0,0.00}", propostaCondicoes.TotalParcelas),
                            SaldoQuitacao = String.Format("{0:0,0.00}", propostaCondicoes.SaldoQuitacao),
                            PrecoVendaCorrigido = String.Format("{0:0,0.00}", propostaCondicoes.PrecoVendaCorrigido),
                            JurosCobrados = String.Format("{0:0,0.00}", propostaCondicoes.JurosPeriodo),
                            BancoCliente = proposta.BancoCliente,
                            AgenciaCliente = proposta.AgenciaCliente,
                            ContaCliente = proposta.ContaCliente,
                            TestemunhaNome1 = proposta.TestemunhaNome1,
                            TestemunhaNome2 = proposta.TestemunhaNome2,
                            TestemunhaEnd1 = proposta.TestemunhaEnd1,
                            TestemunhaEnd2 = proposta.TestemunhaEnd2,
                            TestemunhaRg1 = proposta.TestemunhaRg1,
                            TestemunhaRg2 = proposta.TestemunhaRg2
                        };
                        return PartialView("EditarPropostaPreenchida", retorno);
                    }
                    return PartialView("EditarPropostaPreenchida", new PropostaViewModel());
                }

            }
            else
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult CalcularPrecos(string Quadra, string Lote, string Entrada)
        {
            if (ModelState.IsValid)
            {
                var stringEntrada = Entrada; //  Entrada.Replace(".", "").Replace(",", ".");
                var lote = Int32.Parse(Lote);
                var entrada = decimal.Parse(stringEntrada);
                var dadosLote = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                //verifica se o lote foi vendido
                var disponivel = dadosLote.SituacaoNoSite; // db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault().SituacaoNoSite;
                if (disponivel != "1")
                {
                    return Json(new
                    {
                        result = false,
                        message = "Esse lote não está mais disponível!"
                    });
                }

                //verifica se valor da entrada é compatível e permitida
                // var valoresLote = db.TabelaPrecoLotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                var retorno = new PrecoRetonar();
                retorno.PrecoM2 = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().ValorM2;
                var precoVenda = Math.Round(retorno.PrecoM2 * dadosLote.Area, 2);


                if (entrada < (precoVenda * 0.15m))
                {
                    return Json(new
                    {
                        result = false,
                        message = "Valor da entrada não permitido!"
                    });

                }
                retorno.Entrada = String.Format("{0:0,0.00}", entrada);


                var saldo = precoVenda - entrada;

                if (saldo > 0)
                    retorno.TipoPgtoPermitido = "0";
                else
                    retorno.TipoPgtoPermitido = "1";

                retorno.SaldoPagar = String.Format("{0:0,0.00}", saldo);
                retorno.CorFundo = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().CorFundo ?? "#ffffff";

                retorno.PrecoVenda = precoVenda;

                double vlrParcSemestral = 7500.00;   // deverár ser parametrizado
                double jurosAM = 0.0025;
                double constante = 1;
                var entradaPadrao = Math.Round(precoVenda * 0.15m, 2);

                var VP6 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 6)), 2);
                var VP12 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 12)), 2);
                var VP18 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 18)), 2);
                var VP24 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 24)), 2);
                var VP30 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 30)), 2);
                var VP36 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 36)), 2);
                var VP42 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 42)), 2);
                var VP48 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 48)), 2);
                var VP54 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 54)), 2);
                var VP60 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 60)), 2);

                var parc12 = 7000.00m;
                var parc24 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24)) * (((jurosAM * (Math.Pow(1 + jurosAM, 24))) / Math.Pow(1 + jurosAM, 24)) - 1)) / -24, 2);
                var parc36 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36)) * (((jurosAM * (Math.Pow(1 + jurosAM, 36))) / Math.Pow(1 + jurosAM, 36)) - 1)) / -36, 2);
                var parc48 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48)) * (((jurosAM * (Math.Pow(1 + jurosAM, 48))) / Math.Pow(1 + jurosAM, 48)) - 1)) / -48, 2);
                var parc60 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48 + VP54 + VP60)) * (((jurosAM * (Math.Pow(1 + jurosAM, 60))) / Math.Pow(1 + jurosAM, 60)) - 1)) / -60, 2);

                var listaTabela = new List<SelectListItem>();
                listaTabela.Add(new SelectListItem
                {
                    Text = " Selecione um plano de pagamento... ",
                    Value = "0"
                });
                listaTabela.Add(new SelectListItem
                {
                    Text = " 12 x R$ " + String.Format("{0:0,0.00}", parc12) + " +  2 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                    Value = "1"
                });
                listaTabela.Add(new SelectListItem
                {
                    Text = " 24 x R$ " + String.Format("{0:0,0.00}", parc24) + " +  4 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                    Value = "2"
                });
                listaTabela.Add(new SelectListItem
                {
                    Text = " 36 x R$ " + String.Format("{0:0,0.00}", parc36) + " +  6 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                    Value = "3"
                });
                listaTabela.Add(new SelectListItem
                {
                    Text = " 48 x R$ " + String.Format("{0:0,0.00}", parc48) + " +  8 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                    Value = "4"
                });
                listaTabela.Add(new SelectListItem
                {
                    Text = " 60 x R$ " + String.Format("{0:0,0.00}", parc60) + " + 10 x R$ " + String.Format("{0:0,0.00}", vlrParcSemestral) + "   ",
                    Value = "5"
                });


                // Calcular a condição de parcelamento baseado no valor de entrada



                if (saldo > 0)
                {

                    //var condicoespagto = (from f in db.TabelaPrecoLotes
                    //                      where f.Quadra == Quadra && f.Lote == lote
                    //                      select new SelectListItem
                    //                      {
                    //                          Text = " " + f.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasMensais) + " +   " + f.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasSemestrais) + "   ",
                    //                          Value = f.Id.ToString()
                    //                      });

                    //ViewBag.Mensais = condicoespagto;

                    ViewBag.Mensais = listaTabela;



                }
                else
                {
                    var condicoespagto =
                                          new SelectListItem()
                                          {
                                              Text = " 1 parcela de R$ " + String.Format("{0:0,0.00}", entrada) + "   ",
                                              Value = "0"
                                          };

                    ViewBag.Mensais = condicoespagto;
                }

                // gera um html para substituir no combobox

                var conteudo = "";
                foreach (var item in listaTabela)
                {
                    conteudo = conteudo + "<option value = '" + item.Value + "' > " + item.Text + " </option>";
                }

                retorno.Parcelas = conteudo + "</select>";

                retorno.result = true;

                return Json(new
                {
                    retorno = retorno
                });
            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Erro no processamento! Tente novamente."
                });

            }

        }


        [HttpPost]
        [Authorize]
        public IActionResult ValidarRestante(int Loteamento, string Quadra, string Lote, string Entrada, string TipoPagamento, string Parcelamento)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var lote = int.Parse(Lote);


                    //verifica se o lote foi vendido ou ainda está disponível
                    var disponivel = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault().SituacaoNoSite;
                    if (disponivel != "1")
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Esse lote não está mais disponível!"
                        });
                    }

                    // verifica todos os valores novamente, para evitar "injeções" no jQuery

                    var dadosLote = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                    var stringEntrada = Entrada.Replace(".", "").Replace(",", ".");
                    var entrada = decimal.Parse(stringEntrada) / 100;

                    var precoM2 = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().ValorM2;
                    var precoVenda = Math.Round(precoM2 * dadosLote.Area, 2);
                    double vlrParcSemestral = 7500.00;   // deverár ser parametrizado
                    double jurosAM = 0.0025;
                    double constante = 1;
                    var entradaPadrao = Math.Round(precoVenda * 0.15m, 2);
                    var saldo = precoVenda - entrada;


                    //verifica se valor da entrada é compatível e permitida
                    if (entrada < entradaPadrao)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada não permitido!"
                        });
                    }

                    if (entrada > precoVenda)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada está maior que o valor do Lote!"
                        });
                    }

                    var VP6 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 6)), 2);
                    var VP12 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 12)), 2);
                    var VP18 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 18)), 2);
                    var VP24 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 24)), 2);
                    var VP30 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 30)), 2);
                    var VP36 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 36)), 2);
                    var VP42 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 42)), 2);
                    var VP48 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 48)), 2);
                    var VP54 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 54)), 2);
                    var VP60 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 60)), 2);

                    var parc12 = (double)7000.00m;
                    var parc24 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24)) * (((jurosAM * (Math.Pow(1 + jurosAM, 24))) / Math.Pow(1 + jurosAM, 24)) - 1)) / -24, 2);
                    var parc36 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36)) * (((jurosAM * (Math.Pow(1 + jurosAM, 36))) / Math.Pow(1 + jurosAM, 36)) - 1)) / -36, 2);
                    var parc48 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48)) * (((jurosAM * (Math.Pow(1 + jurosAM, 48))) / Math.Pow(1 + jurosAM, 48)) - 1)) / -48, 2);
                    var parc60 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48 + VP54 + VP60)) * (((jurosAM * (Math.Pow(1 + jurosAM, 60))) / Math.Pow(1 + jurosAM, 60)) - 1)) / -60, 2);

                    var listaTabela = new List<PlanosParcelasViewModel>();
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "1",
                        NrParcelasMensais = 12,
                        VrParcelaMensal = parc12,
                        NrParcelasSemestrais = 2,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "2",
                        NrParcelasMensais = 24,
                        VrParcelaMensal = parc24,
                        NrParcelasSemestrais = 4,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "3",
                        NrParcelasMensais = 36,
                        VrParcelaMensal = parc36,
                        NrParcelasSemestrais = 6,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "4",
                        NrParcelasMensais = 48,
                        VrParcelaMensal = parc48,
                        NrParcelasSemestrais = 8,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "5",
                        NrParcelasMensais = 60,
                        VrParcelaMensal = parc60,
                        NrParcelasSemestrais = 10,
                        VrParcelaSemestral = vlrParcSemestral
                    });

                    var planoescolhidosemestral = "";
                    var planomensalescolhido = "";

                    var planoEscolhido = listaTabela.Where(c => c.Plano == Parcelamento).FirstOrDefault();
                    if (planoEscolhido != null)
                    {
                        planomensalescolhido = planoEscolhido.NrParcelasMensais.ToString() + " X R$ " + String.Format("{0:0,0.00}", planoEscolhido.VrParcelaMensal);
                        planoescolhidosemestral = planoEscolhido.NrParcelasSemestrais.ToString() + " X R$ " + String.Format("{0:0,0.00}", planoEscolhido.VrParcelaSemestral);

                    }
                    else
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Erro na escolha do plano!"
                        });
                    }
                    double totalParcelas;
                    double saldoQuitacao;// = Math.Round(((double)(saldo * 0.4m)) * Math.Pow(constante + jurosAM, planoEscolhido.NrParcelasMensais), 2);
                    double precoVendaCorrigido;// = Math.Round(totalParcelas + saldoQuitacao + (double)entrada, 2);
                    double jurosPeriodo;// = precoVendaCorrigido - (double)precoVenda;

                    if (planoEscolhido.Plano == "1")
                    {
                        totalParcelas = (planoEscolhido.NrParcelasMensais * planoEscolhido.VrParcelaMensal) + (planoEscolhido.NrParcelasSemestrais * planoEscolhido.VrParcelaSemestral);
                        saldoQuitacao = (double)saldo - totalParcelas;
                        precoVendaCorrigido = Math.Round(totalParcelas + saldoQuitacao + (double)entrada, 2);
                        jurosPeriodo = precoVendaCorrigido - (double)precoVenda;
                    }
                    else
                    {
                        totalParcelas = (planoEscolhido.NrParcelasMensais * planoEscolhido.VrParcelaMensal) + (planoEscolhido.NrParcelasSemestrais * planoEscolhido.VrParcelaSemestral);
                        saldoQuitacao = Math.Round(((double)(saldo * 0.4m)) * Math.Pow(constante + jurosAM, planoEscolhido.NrParcelasMensais), 2);
                        precoVendaCorrigido = Math.Round(totalParcelas + saldoQuitacao + (double)entrada, 2);
                        jurosPeriodo = precoVendaCorrigido - (double)precoVenda;
                    }

                    return Json(new
                    {
                        result = true,
                        juroscobrados = String.Format("{0:0,0.00}", jurosPeriodo),
                        precovendacorrigido = String.Format("{0:0,0.00}", precoVendaCorrigido),
                        saldoquitacao = String.Format("{0:0,0.00}", saldoQuitacao),
                        totalparcelas = String.Format("{0:0,0.00}", totalParcelas),
                        planomensalescolhido,
                        planoescolhidosemestral
                    });
                }
                catch (Exception)
                {

                    return Json(new
                    {
                        result = false,
                        message = "Erro no processamento!"
                    });
                }
            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Erro no processamento!"
                });

            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult Compradores(int Loteamento, string Quadra, int Lote, string Entrada, string TipoPagamento, string Parcelamento, string PrimeiroVctMensal, string PrimeiroVctSemestral, string Banco, string Agencia, string Conta, string TestNome1, string TestNome2, string TestEnd1, string TestEnd2, string TestRg1, string TestRg2)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var lote = Lote;

                    //verifica se o lote foi vendido ou ainda está disponível
                    var disponivel = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault().SituacaoNoSite;
                    if (disponivel != "1")
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Esse lote não está mais disponível!"
                        });
                    }

                    // testa as datas de primeiro vencimento

                    DateTime PrimeiroVencimentoMensal;
                    var convertido = DateTime
                        .TryParseExact(PrimeiroVctMensal,
                                        "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out PrimeiroVencimentoMensal);

                    if (!convertido)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Primeiro vencimento mensal inválido!"
                        });
                    }


                    DateTime PrimeiroVencimentoSemestral;
                    convertido = DateTime
                        .TryParseExact(PrimeiroVctSemestral,
                                        "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out PrimeiroVencimentoSemestral);

                    if (!convertido)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Primeiro vencimento semestral inválido!"
                        });
                    }




                    // verifica todos os valores novamente, para evitar "injeções" no jQuery

                    var dadosLote = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                    var stringEntrada = Entrada.Replace(".", "").Replace(",", ".");
                    var entrada = decimal.Parse(stringEntrada) / 100;

                    //verifica se valor da entrada é compatível e permitida
                    var precoM2 = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().ValorM2;
                    var precoVenda = Math.Round(precoM2 * dadosLote.Area, 2);
                    double vlrParcSemestral = 7500.00;   // deverár ser parametrizado
                    double jurosAM = 0.0025;
                    double constante = 1;
                    var entradaPadrao = Math.Round(precoVenda * 0.15m, 2);
                    var saldo = precoVenda - entrada;


                    if (entrada < entradaPadrao)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada não permitido!"
                        });
                    }

                    if (entrada > precoVenda)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada está maior que o valor do Lote!"
                        });
                    }

                    //return Json(new
                    //{
                    //    result = false,
                    //    message = "Esta etapa está sendo concluída, baseado nas mudanças de cálculos!"
                    //});

                    var VP6 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 6)), 2);
                    var VP12 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 12)), 2);
                    var VP18 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 18)), 2);
                    var VP24 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 24)), 2);
                    var VP30 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 30)), 2);
                    var VP36 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 36)), 2);
                    var VP42 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 42)), 2);
                    var VP48 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 48)), 2);
                    var VP54 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 54)), 2);
                    var VP60 = Math.Round((vlrParcSemestral / Math.Pow((constante + jurosAM), 60)), 2);

                    var parc12 = (double)7000.00m;
                    var parc24 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24)) * (((jurosAM * (Math.Pow(1 + jurosAM, 24))) / Math.Pow(1 + jurosAM, 24)) - 1)) / -24, 2);
                    var parc36 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36)) * (((jurosAM * (Math.Pow(1 + jurosAM, 36))) / Math.Pow(1 + jurosAM, 36)) - 1)) / -36, 2);
                    var parc48 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48)) * (((jurosAM * (Math.Pow(1 + jurosAM, 48))) / Math.Pow(1 + jurosAM, 48)) - 1)) / -48, 2);
                    var parc60 = Math.Round(((((double)(saldo * 0.60m)) - (VP6 + VP12 + VP18 + VP24 + VP30 + VP36 + VP42 + VP48 + VP54 + VP60)) * (((jurosAM * (Math.Pow(1 + jurosAM, 60))) / Math.Pow(1 + jurosAM, 60)) - 1)) / -60, 2);

                    var listaTabela = new List<PlanosParcelasViewModel>();
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "1",
                        NrParcelasMensais = 12,
                        VrParcelaMensal = parc12,
                        NrParcelasSemestrais = 2,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "2",
                        NrParcelasMensais = 24,
                        VrParcelaMensal = parc24,
                        NrParcelasSemestrais = 4,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "3",
                        NrParcelasMensais = 36,
                        VrParcelaMensal = parc36,
                        NrParcelasSemestrais = 6,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "4",
                        NrParcelasMensais = 48,
                        VrParcelaMensal = parc48,
                        NrParcelasSemestrais = 8,
                        VrParcelaSemestral = vlrParcSemestral
                    });
                    listaTabela.Add(new PlanosParcelasViewModel
                    {
                        Plano = "5",
                        NrParcelasMensais = 60,
                        VrParcelaMensal = parc60,
                        NrParcelasSemestrais = 10,
                        VrParcelaSemestral = vlrParcSemestral
                    });

                    var planoEscolhido = listaTabela.Where(c => c.Plano == Parcelamento).FirstOrDefault();
                    if (planoEscolhido != null)
                    {

                    }
                    else
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Erro na escolha do plano!"
                        });
                    }
                    double totalParcelas;
                    double saldoQuitacao;
                    double precoVendaCorrigido;
                    double jurosPeriodo;

                    if (planoEscolhido.Plano == "1")
                    {
                        totalParcelas = (planoEscolhido.NrParcelasMensais * planoEscolhido.VrParcelaMensal) + (planoEscolhido.NrParcelasSemestrais * planoEscolhido.VrParcelaSemestral);
                        saldoQuitacao = (double)saldo - totalParcelas;
                        precoVendaCorrigido = Math.Round(totalParcelas + saldoQuitacao + (double)entrada, 2);
                        jurosPeriodo = precoVendaCorrigido - (double)precoVenda;
                    }
                    else
                    {
                        totalParcelas = (planoEscolhido.NrParcelasMensais * planoEscolhido.VrParcelaMensal) + (planoEscolhido.NrParcelasSemestrais * planoEscolhido.VrParcelaSemestral);
                        saldoQuitacao = Math.Round(((double)(saldo * 0.4m)) * Math.Pow(constante + jurosAM, planoEscolhido.NrParcelasMensais), 2);
                        precoVendaCorrigido = Math.Round(totalParcelas + saldoQuitacao + (double)entrada, 2);
                        jurosPeriodo = precoVendaCorrigido - (double)precoVenda;
                    }



                    Guid UserId = _user.GetUserId();

                    //var parcelaId = int.Parse(Parcelamento);

                    //var parcelas = (from f in db.TabelaPrecoLotes
                    //                where f.Quadra == Quadra && f.Lote == Lote
                    //                where f.Id == parcelaId
                    //                select f).FirstOrDefault();


                    // se tudo estiver correto - grava a proposta com as condições comerciais e mostra grid para preencher com compradores

                    var obj = new Proposta();
                    obj.DataHora = DateTime.Now;
                    obj.DataProposta = DateTime.Today;
                    obj.Status = 2;
                    obj.Quadra = Quadra;
                    obj.Lote = Lote;
                    obj.LoteamentoId = Loteamento;
                    obj.Usuario = UserId.ToString();
                    obj.ValorTotal = precoVenda;
                    obj.ValorCorretagem = Math.Round(precoVenda * 0.04m, 2);
                    obj.PrimeiroVencMensal = PrimeiroVencimentoMensal;
                    obj.PrimeiroVencSemestral = PrimeiroVencimentoSemestral;
                    obj.NumeroBoletoEntrada = "";
                    obj.BancoCliente = Banco;
                    obj.AgenciaCliente = Agencia;
                    obj.ContaCliente = Conta;
                    obj.TestemunhaNome1 = TestNome1;
                    obj.TestemunhaEnd1 = TestEnd1;
                    obj.TestemunhaRg1 = TestRg1;
                    obj.TestemunhaNome2 = TestNome2;
                    obj.TestemunhaEnd2 = TestEnd2;
                    obj.TestemunhaRg2 = TestRg2;
                    db.Add(obj);
                    db.SaveChanges();

                    var objcm = new PropostaCondicaoComercial();
                    // var listacm = new List<PropostaCondicaoComercial>();
                    objcm.PropostaId = obj.Id;
                    objcm.DataHora = DateTime.Now;
                    objcm.Usuario = UserId.ToString();
                    objcm.ValorTotal = precoVenda;
                    objcm.ValorEntrada = entrada;
                    objcm.NrParcelasMensais = entrada == precoVenda ? 0 : planoEscolhido.NrParcelasMensais;
                    objcm.NrParcelasSemestrais = entrada == precoVenda ? 0 : planoEscolhido.NrParcelasSemestrais;
                    objcm.ValorParcelaMensal = entrada == precoVenda ? 0 : (decimal)planoEscolhido.VrParcelaMensal; ;
                    objcm.ValorParcelaSemestral = entrada == precoVenda ? 0 : (decimal)planoEscolhido.VrParcelaSemestral;
                    objcm.TotalParcelas = (decimal)totalParcelas;
                    objcm.SaldoQuitacao = (decimal)saldoQuitacao;
                    objcm.PrecoVendaCorrigido = (decimal)precoVendaCorrigido;
                    objcm.JurosPeriodo = (decimal)jurosPeriodo;
                    db.Add(objcm);
                    db.SaveChanges();

                    // altera a condição do lote para "reservado"
                    var objlote = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();
                    objlote.SituacaoNoSite = "2";
                    db.Entry(objlote).State = EntityState.Modified;
                    db.SaveChanges();

                    var retorno = new PropostaViewModel();

                    //var saldo = precoVenda - entrada;

                    retorno.Id = obj.Id;
                    retorno.SaldoPagar = String.Format("{0:0,0.00}", saldo);
                    retorno.Entrada = String.Format("{0:0,0.00}", entrada);
                    retorno.Area = objlote.Area;
                    retorno.Lote = objlote.Lote;
                    retorno.Quadra = objlote.Quadra;
                    retorno.LoteamentoId = objlote.LoteamentoId;
                    retorno.Status = int.Parse(disponivel);
                    retorno.ValorTotal = precoVenda;
                    retorno.Parcelamento = Parcelamento;
                    retorno.TipoPagamento = TipoPagamento;
                    retorno.result = true;
                    switch (disponivel)
                    {
                        case "1":
                            retorno.StatusNoSite = "Disponível";
                            break;
                        case "2":
                            retorno.StatusNoSite = "Reservado";
                            break;
                        case "3":
                            retorno.StatusNoSite = "Vendido";
                            break;
                    }

                    ViewBag.Corretor = (from f in db.Corretores
                                        select new SelectListItem
                                        {
                                            Text = f.Nome,
                                            Value = f.Id.ToString()
                                        });

                    var auxiliar = listaTabela.Where(c => c.Plano == Parcelamento).FirstOrDefault();
                    retorno.TipoPagamento = auxiliar.NrParcelasMensais.ToString() + " X R$ " + String.Format("{0:0,0.00}", auxiliar.VrParcelaMensal) + " + " + auxiliar.NrParcelasSemestrais.ToString() + " X R$ " + String.Format("{0:0,0.00}", auxiliar.VrParcelaSemestral);

                    retorno.result = true;
                    return PartialView("Compradores", retorno);

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        result = false,
                        message = "Erro no processamento! " + ex.Message
                    });
                }
            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Erro no processamento! Tente novamente."
                });
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult BuscarCompradores(int id)
        {
            var compradores = (from f in db.PropostasCompradores
                               join c in db.Comprador on f.CompradorId equals c.Id
                               where (f.DataExclusao == null)
                               where f.PropostaId == id
                               select new
                               {
                                   id = f.Id,
                                   nome = c.Nome
                               }).ToList();

            return Json(new { data = compradores });

        }
        [HttpPost]
        [Authorize]
        public IActionResult EditarComprador(string PropostaId, int Id)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    Result = false,
                    Message = "Dados inconsistentes."
                });
            }
            try
            {
                if (Id == 0)
                {
                    ViewBag.EstadoCivil = (from f in db.EstadoCivil
                                           select new SelectListItem
                                           {
                                               Text = f.Descricao,
                                               Value = f.Id.ToString()
                                           });
                    var obj = new CompradorViewModel();
                    var propostaid = String.IsNullOrEmpty(PropostaId) ? 0 : int.Parse(PropostaId);
                    obj.PropostaId = propostaid;
                    return PartialView("Comprador", obj);
                }
                else
                {
                    using (DBContext context = new DBContext())
                    {
                        ViewBag.EstadoCivil = (from f in db.EstadoCivil
                                               select new SelectListItem
                                               {
                                                   Text = f.Descricao,
                                                   Value = f.Id.ToString()
                                               });
                        var obj = (from f in db.Comprador
                                   join p in db.PropostasCompradores on f.Id equals p.CompradorId
                                   where p.Id == Id
                                   select f).FirstOrDefault();

                        return PartialView("Comprador", obj);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult BuscarCep(string cep)
        {
            try
            {
                if (!string.IsNullOrEmpty(cep))
                {
                    var param = cep.Replace(".", "").Replace("-", "");
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                    var buscacep = new Cep();
#pragma warning disable SYSLIB0014 // Type or member is obsolete
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://viacep.com.br/ws/" + param + "/json/");
#pragma warning restore SYSLIB0014 // Type or member is obsolete
                    request.ContentType = "application/json; charset=utf-8";
                    request.PreAuthenticate = true;
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        buscacep = JsonConvert.DeserializeObject<Cep>(reader.ReadToEnd());

                    }

                    return Json(new { endereco = (!string.IsNullOrEmpty(buscacep.logradouro) ? buscacep.logradouro : ""), cidade = (!string.IsNullOrEmpty(buscacep.localidade) ? buscacep.localidade + "" : ""), bairro = (!string.IsNullOrEmpty(buscacep.bairro) ? buscacep.bairro : ""), uf = (!string.IsNullOrEmpty(buscacep.uf) ? buscacep.uf : "") });
                }
                else
                {
                    return Json(new { Logradouro = "" });
                }

            }
            catch (Exception)
            {
                return Json(new { Logradouro = "" });
            }

        }
        [HttpPost]
        [Authorize]
        public IActionResult SalvarComprador(CompradorViewModel model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = new Comprador();
                    obj.Nome = model.Nome;
                    obj.Cpf = getnumber(model.Cpf);
                    obj.Rg = model.Rg;
                    obj.RgExpPor = model.RgExpPor;
                    obj.DtNasc = model.DtNasc;
                    obj.Email = model.Email;
                    obj.Profissao = model.Profissao;
                    obj.Nacionalidade = model.Nacionalidade;
                    obj.EstadoCivil = model.EstadoCivil;
                    obj.CasamentoData = model.CasamentoData;
                    obj.CasamentoLivro = model.CasamentoLivro;
                    obj.CasamentoFolhas = model.CasamentoFolhas;
                    obj.CasamentoEscrRegistro = model.CasamentoEscrRegistro;
                    obj.CasamentoEscrTabeliao = model.CasamentoEscrTabeliao;
                    obj.CasamentoRegime = model.CasamentoRegime;
                    obj.ConjugeCpf = getnumber(model.ConjugeCpf);
                    obj.ConjugeNome = model.ConjugeNome;
                    obj.ConjugeRg = model.ConjugeRg;
                    obj.ConjugeRgExpPor = model.ConjugeRgExpPor;
                    obj.ConjugeProfissao = model.ConjugeProfissao;
                    obj.ConjugeNacionalidade = model.ConjugeNacionalidade;
                    obj.ConjugeCelular = model.ConjugeCelular;
                    obj.Cep = getnumber(model.Cep);
                    obj.Logradouro = model.Logradouro;
                    obj.Numero = model.Numero;
                    obj.Complemento = model.Complemento ?? "";
                    obj.Bairro = model.Bairro;
                    obj.Municipio = model.Municipio;
                    obj.Estado = model.Estado;
                    obj.TelefoneFixo = getnumber(model.TelefoneFixo);
                    obj.Celular = getnumber(model.Celular);
                    obj.Usuario = userId;
                    obj.DataHora = DateTime.Now;
                    if (model.Id > 0)
                    {
                        obj.Id = (int)model.Id;
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Comprador.Add(obj);
                        db.SaveChanges();

                        var objPc = new PropostasCompradores();
                        objPc.DataHora = DateTime.Now;
                        objPc.Usuario = userId;
                        objPc.PropostaId = model.PropostaId;
                        objPc.CompradorId = obj.Id;
                        db.PropostasCompradores.Add(objPc);
                        db.SaveChanges();

                    }





                    return Json(new
                    {
                        id = obj.Id,
                        result = true,
                        message = "proposta cadastrada com sucesso!"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        result = false,
                        message = "Não foi possível efetuar o cadastro!"
                    });
                }

            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Não foi possível efetuar o cadastro!"
                });

            }

            //if (model.Id <= 0)
            //{
            //    var obj2 = new PropostasCompradores();
            //    obj2.PropostaId = model.PropostaId;
            //    obj2.CompradorId = obj.Id;
            //    obj2.Usuario = userId;
            //    obj2.DataHora = DateTime.Now;
            //    db.PropostasCompradores.Add(obj2);
            //    db.SaveChanges();
            //}

        }

        [HttpPost]
        [Authorize]
        public IActionResult ListarCompradores(string propostaid)
        {
            var propostaId = int.Parse(propostaid);

            var lista = (from x in db.PropostasCompradores
                         join y in db.Comprador on (int)x.CompradorId equals y.Id
                         where x.PropostaId == propostaId
                         select new ListaCompradoresViewModel
                         {
                             CompradorId = y.Id,
                             PropostaId = (int)x.PropostaId,
                             NomeComprador = y.Nome,
                             CpfComprador = Convert.ToUInt64(y.Cpf).ToString(@"000\.000\.000\-00"),
                             CelularComprador = Convert.ToUInt64(y.Celular).ToString(@"(00) 00000-0000")
                         }
                         ).ToList();

            return PartialView("ListaCompradores", lista);
        }
        //
        // 20/10/2022 - Impressão da proposta para diversas finalidades
        //

        public IActionResult ImprimirProposta(string id)
        {
            var PropostaId = int.Parse(id);

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var obj = db.Propostas.Where(c => c.Id == PropostaId).FirstOrDefault();
                var objlote = db.Lotes.Where(c => c.Quadra == obj.Quadra && c.Lote == obj.Lote).FirstOrDefault();

                var compradores = db.PropostasCompradores.Where(c => c.PropostaId == PropostaId).ToList();
                var condicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == PropostaId).FirstOrDefault();
                var pxPormm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4, 15 * pxPormm, 15 * pxPormm, 2 * pxPormm, 5 * pxPormm);
                var writer = PdfWriter.GetInstance(pdf, memoryStream);
                pdf.Open();

                var fonteParagrafo = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.NORMAL);
                var fonteBold = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.BOLD);
                var fonteTitulo = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.BOLD);
                var fonteReduzida = new iTextSharp.text.Font(fontebase, 8, iTextSharp.text.Font.NORMAL);


                var titulo1 = new Paragraph("PROPOSTA DE COMPRA E VENDA - LOTEAMENTO RESIDENCIAL PIANOPOLI\n\n", fonteTitulo);
                var titulo2 = new Paragraph("1a. Via Comprador\n\n", fonteReduzida);

                //var titulo3 = new Paragraph("PROPOSTA PARA PAGAMENTO A PRAZO - NR. " + id.ToString() + "\n\n", fonteParagrafo);
                //var titulo4 = new Paragraph("E COM CONDIÇÕES SUSPENSIVAS\n\n", fonteParagrafo);

                PdfPCell cell = new PdfPCell();
                Phrase phrase = new Phrase();
                phrase.Add(new Chunk("PROPOSTA PARA PAGAMENTO ", fonteParagrafo));
                phrase.Add(new Chunk("A PRAZO ", fonteBold));
                phrase.Add(new Chunk("NR. " + id.ToString().PadLeft(6, '0') + " \n\n", fonteParagrafo));
                //cell = new PdfPCell(phrase);

                var titulo3 = new Paragraph(phrase);

                titulo1.Alignment = Element.ALIGN_CENTER;
                titulo2.Alignment = Element.ALIGN_RIGHT;
                titulo3.Alignment = Element.ALIGN_CENTER;

                //titulo3.Alignment = Element.ALIGN_CENTER;
                //titulo4.Alignment = Element.ALIGN_CENTER;
                pdf.Add(titulo1);
                pdf.Add(titulo2);
                pdf.Add(titulo3);


                PdfPTable mtable = new PdfPTable(1);
                mtable.WidthPercentage = 100;
                mtable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                PdfPTable table = new PdfPTable(3);
                table.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                table.WidthPercentage = 100;


                var txtquadra = new Phrase();
                txtquadra.Add(new Chunk("Quadra: ", fonteParagrafo));
                txtquadra.Add(new Chunk(obj.Quadra, fonteBold));

                cell = new PdfPCell(txtquadra);
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("VALOR DO LOTE: R$ " + string.Format("{0:0,0.00}", condicoes.ValorTotal), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);


                var txtlote = new Phrase();
                txtlote.Add(new Chunk("Lote: ", fonteParagrafo));
                txtlote.Add(new Chunk(obj.Lote.ToString(), fonteBold));

                cell = new PdfPCell(txtlote);
                //cell = new PdfPCell(new Phrase("Lote: " + obj.Lote.ToString(), fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Pagamento - Entrada: R$ " + string.Format("{0:0,0.00}", condicoes.ValorEntrada), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                var txtarea = new Phrase();
                txtarea.Add(new Chunk("Área: ", fonteParagrafo));
                txtarea.Add(new Chunk(String.Format("{0:0,0.00}", objlote.Area) + " m2", fonteBold));

                cell = new PdfPCell(txtarea);
                //cell = new PdfPCell(new Phrase("Área: " + String.Format("{0:0,0.00}", objlote.Area) + " m2", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("DATA VENCIMENTO PARCELA ENTRADA: " + obj.DataProposta.AddDays(5).ToShortDateString(), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Quantidade Prestações Mensais: " + condicoes.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", condicoes.ValorParcelaMensal), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Primeiro vencimento mensal: " + obj.PrimeiroVencMensal.Value.ToShortDateString(), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Quantidade Prestações Semestrais: " + condicoes.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", condicoes.ValorParcelaSemestral), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Primeiro vencimento semestral: " + obj.PrimeiroVencSemestral.Value.ToShortDateString(), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("SALDO REMANESCENTE: R$ " + String.Format("{0:0,0.00}", condicoes.SaldoQuitacao), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("REAJUSTE ANUAL: IPCA + 3% a.a. (conforme contrato de venda)\n\n", fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                mtable.AddCell(table);

                table = new PdfPTable(3);

                var idcomprador = 1;

                var sql = (from f in db.PropostasCompradores
                           join c in db.PropostasCompradores on f.CompradorId equals c.Id
                           join d in db.Comprador on c.CompradorId equals d.Id
                           where f.PropostaId == PropostaId
                           where (f.DataExclusao == null)
                           select d).ToList();

                foreach (var item in sql)
                {
                    //cell = new PdfPCell(new Phrase("Comprador " + idcomprador + ": " + item.Nome.ToUpper(), fonteParagrafo));
                    phrase = new Phrase();
                    phrase.Add(new Chunk("Comprador " + idcomprador + ": ", fonteParagrafo));
                    phrase.Add(new Chunk(item.Nome.ToUpper(), fonteBold));
                    cell = new PdfPCell(phrase);

                    cell.Colspan = 3;
                    cell.BorderWidthLeft = 0;
                    cell.BorderWidthTop = 0;
                    cell.BorderWidthRight = 0;
                    cell.BorderWidthBottom = 1;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("CPF: " + Convert.ToUInt64(item.Cpf).ToString(@"000\.000\.000\-00"), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("RG: " + item.Rg, fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("EXP: " + item.RgExpPor, fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Data Nascimento: " + item.DtNasc.ToShortDateString(), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    cell = new PdfPCell(new Phrase("Filiação: " + (item.NomeMae == null ? "" : item.NomeMae.TrimEnd()) + (item.NomePai == null ? "" : " e " + item.NomePai), fonteParagrafo));
                    cell.Colspan = 2;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Profissão: " + item.Profissao.TrimEnd(), fonteParagrafo));
                    cell.Colspan = 2;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Nacionalidade: " + item.Nacionalidade.TrimEnd(), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // -- linha
                    cell = new PdfPCell(new Phrase("E-mail: " + item.Email.ToLower().TrimEnd(), fonteParagrafo));
                    cell.Colspan = 2;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Fone: " + Convert.ToUInt64(item.Celular).ToString(@"\(00\) 00000\-0000"), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                    //---

                    // -- linha
                    cell = new PdfPCell(new Phrase("Endereço: " + item.Logradouro.TrimEnd() + " " + item.Numero.TrimEnd() + " " + (item.Complemento.TrimEnd() ?? ""), fonteParagrafo));
                    cell.Colspan = 3;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                    //---

                    // -- linha
                    cell = new PdfPCell(new Phrase("Cidade: " + item.Municipio.TrimEnd(), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Bairro: " + item.Bairro.TrimEnd(), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Cep: " + Convert.ToUInt64(item.Cep).ToString(@"00000\-000"), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // -- linha
                    var estadocivil = "";
                    switch (item.EstadoCivil)
                    {
                        case "2":
                            estadocivil = "Casado(a)";
                            break;
                        case "3":
                            estadocivil = "Separado(a)";
                            break;
                        case "4":
                            estadocivil = "Divorciado(a)";
                            break;
                        case "5":
                            estadocivil = "Viúvo(a)";
                            break;
                        case "6":
                            estadocivil = "Amasiado(a)";
                            break;
                        default:
                            estadocivil = "Solteiro(a)";
                            break;
                    }

                    cell = new PdfPCell(new Phrase("Estado Civil: " + (estadocivil == null ? "" : estadocivil), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    if (item.EstadoCivil == "2")
                    {
                        cell = new PdfPCell(new Phrase("Data Casamento: " + (item.CasamentoData == null ? "" : item.CasamentoData.Value.ToShortDateString()), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Regime Cas.: " + (item.CasamentoRegime ?? ""), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Cartório:" + item.CasamentoEscrRegistro.TrimEnd() ?? "", fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Tabelião:" + (item.CasamentoEscrTabeliao ?? ""), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Livro:" + (item.CasamentoLivro ?? "") + (" - Fls: " + item.CasamentoFolhas.TrimEnd() ?? ""), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);


                        // Casado
                        // -- linha
                        cell = new PdfPCell(new Phrase("\nNome Cônjuge: " + (item.ConjugeNome == null ? "" : item.ConjugeNome.TrimEnd()), fonteParagrafo));
                        cell.Colspan = 3;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("CPF Cônjuge: " + (item.ConjugeCpf == null ? "" : Convert.ToUInt64(item.ConjugeCpf).ToString(@"\000\.000\.000\-00")), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("RG Cônjuge: " + item.ConjugeRg ?? "", fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Dt Nasc Cônjuge: " + (item.ConjugeDtNasc == null ? "" : (item.ConjugeDtNasc == null ? "" : item.ConjugeDtNasc.Value.ToShortDateString())), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        //---
                        cell = new PdfPCell(new Phrase("Prof. Cônjuge: " + (item.ConjugeProfissao == null ? " " : item.ConjugeProfissao), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Nac. Cônjuge: " + (item.ConjugeNacionalidade == null ? "" : item.ConjugeNacionalidade.TrimEnd()), fonteParagrafo));
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                    }
                    else
                    {
                        cell = new PdfPCell(new Phrase(" ", fonteParagrafo));
                        cell.Colspan = 2;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                    }
                    cell = new PdfPCell(new Phrase("", fonteParagrafo));
                    cell.Colspan = 3;
                    cell.BorderWidthBottom = 1;
                    cell.BorderWidthTop = 0;
                    cell.BorderWidthLeft = 0;
                    cell.BorderWidthRight = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase("\n", fonteParagrafo));
                    cell.Colspan = 3;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    idcomprador++;
                }
                mtable.AddCell(table);

                var hoje = DateTime.Now;
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;

                cell = new PdfPCell(new Phrase("NOME DO CORRETOR / IMOBILIÁRIA: TESTE ", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("NÚMERO DO CRECI: SP 99.999 2a. Região" + "\n", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("\n", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);


                var fonteArial7 = new iTextSharp.text.Font(FontFactory.GetFont("ARIAL", 7, iTextSharp.text.Font.NORMAL));
                var texto1 = "INFORMAÇÕES ESPECIAIS: IMPORTANTE - A PRESENTE PROPOSTA DE COMPRA CONSTITUI RESERVA DO BEM NELA DESCRITO, SOMENTE SENDO VÁLIDA APÓS A QUITAÇÃO DO VALOR REFERENTE A ENTRADA.A ENTRADA CORRESPONDE AO SINAL E PRINCÍPIO DE PAGAMENTO(ARRAS).NA FALTA DO PAGAMENTO DA ENTRADA ESTA PROPOSTA FICA AUTOMATICAMENTE CANCELADA E RESCINDIDA, GARANTINDO - SE AO VENDEDOR O DIREITO DE DISPOR DO LOTE NEGOCIADO.QUITADO O VALOR DA ENTRADA, O VENDEDOR ELABORARÁ O COMPROMISSO DE COMPRA E VENDA, INTIMANDO O COMPRADOR PARA ASSINATURA DO CONTRATO. \r\n ";
                //cell = new PdfPCell(new Phrase("INFORMAÇÕES ESPECIAIS: IMPORTANTE - A PRESENTE PROPOSTA DE COMPRA CONSTITUI RESERVA DO BEM NELA DESCRITO, SOMENTE SENDO VÁLIDA APÓS A QUITAÇÃO DO VALOR REFERENTE A ENTRADA.A ENTRADA CORRESPONDE AO SINAL E PRINCÍPIO DE PAGAMENTO(ARRAS).NA FALTA DO PAGAMENTO DA ENTRADA ESTA PROPOSTA FICA AUTOMATICAMENTE CANCELADA E RESCINDIDA, GARANTINDO - SE AO VENDEDOR O DIREITO DE DISPOR DO LOTE NEGOCIADO.QUITADO O VALOR DA ENTRADA, O VENDEDOR ELABORARÁ O COMPROMISSO DE COMPRA E VENDA, INTIMANDO O COMPRADOR PARA ASSINATURA DO CONTRATO. \r\n ", fonteArial7));
                cell = new PdfPCell(new Paragraph(texto1, fonteArial7));
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                cell.Colspan = 3;
                cell.Border = 0;
                //cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                table.AddCell(cell);

                var texto2 = "No caso de União Estável, há necessidade de a respectiva escritura estar registrada no Livro E do Oficial do Registro Civil das Pessoas Naturais da Sede, ou, onde houver, no 1º Subdistrito da Comarca em que os companheiros têm ou tiveram seu último domicílio. Ainda, no Estado de São Paulo, há a necessidade de se registrar a escritura no Livro 3 do Registro de Imóveis do domicílio dos conviventes, nos termos do item 85.1 do Capítulo XX das Normas de Serviço da Corregedoria Geral de Justiça do Estado de São Paulo. Se não houver essa escritura registrada, o estado civil será de solteiro, casado, separado, viúvo ou divorciado, ainda que viva efetivamente em regime de união estável. \r\n ";
                cell = new PdfPCell(new Paragraph(texto2, fonteArial7));
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                //                cell = new PdfPCell(new Phrase("No caso de União Estável, há necessidade de a respectiva escritura estar registrada no Livro E do Oficial do Registro Civil das Pessoas Naturais da Sede, ou, onde houver, no 1º Subdistrito da Comarca em que os companheiros têm ou tiveram seu último domicílio. Ainda, no Estado de São Paulo, há a necessidade de se registrar a escritura no Livro 3 do Registro de Imóveis do domicílio dos conviventes, nos termos do item 85.1 do Capítulo XX das Normas de Serviço da Corregedoria Geral de Justiça do Estado de São Paulo. Se não houver essa escritura registrada, o estado civil será de solteiro, casado, separado, viúvo ou divorciado, ainda que viva efetivamente em regime de união estável. \r\n ", fonteArial7));

                cell.Colspan = 3;
                cell.Border = 0;
                //cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("Araraquara, SP, " + hoje.Day + " de " + DateTime.Now.ToString("MMMM", new CultureInfo("pt-BR")) + " de " + hoje.Year + "\r\n \r\n", fonteParagrafo));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.HorizontalAlignment = PdfCell.ALIGN_RIGHT;
                table.AddCell(cell);

                //cell = new PdfPCell(new Phrase("Documentos Necessários: \r\n - CPF  \r\n  - RG \r\n  - Comprovante Residência(água ou luz) \r\n - Certidão estado civil(solteiro, casado ou divorciado) \r\n \r\n \r\n", fonteParagrafo));
                //cell.Colspan = 3;
                //cell.BorderWidth = 0;
                //table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Documentos Necessários:", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" -CPF ", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" -RG ", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" -Comprovante Residência (água ou luz) ", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" -Certidão estado civil (solteiro, casado ou divorciado) \n\n\n", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);



                //var id2 = 1;
                foreach (var item in sql)
                {
                    cell = new PdfPCell(new Phrase("___________________________________  \r\n " + item.Nome.TrimEnd() + " \r\n\n"));
                    cell.Colspan = 1;
                    cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_CENTER;
                    table.AddCell(cell);
                    //id2++;
                    if (item.EstadoCivil == "2")
                    {
                        cell = new PdfPCell(new Phrase("___________________________________  \r\n " + item.ConjugeNome.TrimEnd() + " \r\n\n"));
                        cell.Colspan = 1;
                        cell.Border = 0;
                        cell.HorizontalAlignment = PdfCell.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                cell = new PdfPCell(new Phrase("___________________________________  \r\n" + " Corretor"));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.HorizontalAlignment = PdfCell.ALIGN_CENTER;
                table.AddCell(cell);

                mtable.AddCell(table);

                pdf.Add(mtable);

                pdf.Close();



                byte[] file = memoryStream.ToArray();
                MemoryStream ms = new MemoryStream();
                ms.Write(file, 0, file.Length);
                ms.Flush();
                ms.Position = 0;

                return new FileStreamResult(ms, "application/pdf");
                // return File(fileStream: ms, contentType: "application/pdf", fileDownloadName: "test_file_name" + ".pdf");

            }
        }


        public string getnumber(string param)
        {
            var retorno = "";
            if (!string.IsNullOrEmpty(param))
            {
                var numeros = (from t in param
                               where char.IsDigit(t)
                               select t).ToArray();
                retorno = string.Join("", numeros);
            }
            return retorno;
        }

        private static void CriarCelulaTexto(PdfPTable tabela, string nome, int alinhamentohoriz = PdfCell.ALIGN_LEFT, bool negrito = false, bool italico = false, int tamanhoFonte = 12, int alturaCelula = 15, int colspan = 1)
        {


            int estilo = iTextSharp.text.Font.NORMAL;
            if (negrito && italico)
            {
                estilo = iTextSharp.text.Font.BOLDITALIC;
            }
            if (negrito)
            {
                estilo = iTextSharp.text.Font.BOLD;
            }
            if (italico)
            {
                estilo = iTextSharp.text.Font.ITALIC;
            }


            var fonte = new iTextSharp.text.Font(fontebase, tamanhoFonte, estilo);
            var celula = new PdfPCell(new Phrase(nome, fonte));
            celula.Colspan = colspan;
            celula.HorizontalAlignment = alinhamentohoriz;
            celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celula.Border = 0;
            celula.BorderWidthBottom = 0;
            celula.FixedHeight = alturaCelula;
            tabela.AddCell(celula);

        }
        public ActionResult ImpressaoContrato(int? id)
        {
            // Variáveis do contrato

            // [quadra] e [lote]
            // [descritivo]   --> memorial descritivo do lote
            // [valorTotal]   --> valor total em decimal formatado + valor por extenso
            // [valorTotalCorrigido]  --> valor total corrigido em decimal + valor por extenso
            // [totalMeses]  --> total em meses escolhido no plano de pagamento
            // [valorCorretagem]   --> valor da corretagem em decimal + por extenso.
            // [Corretor] - nome do corretor
            // [Cresci] - número do registro do Cresci
            // [CpfCorretor] - cpf do corretor
            // [valorCorretagemDec] - valor da Corretagem apenas em decimal
            // [DataPgCorretagem] - data para pagemento da corretagem

            // [ValorEntrada] - Valor de entrada em decimal + extenso
            // [saldoParcelar] - valor total - entrada
            // [numeroBoleto] - número do boleto emitido para pagamento da entrada
            // [numeroProposta] - número ID da proposta emitida para a quadra/lote
            // [valorParcelaMensal] - valor da parcela mensal escolhida em decimal + extenso
            // [planoPagamento] - número de parcelas das prestações mensais
            // [primeiroVencimento] - primeiro data de vencimento da parcela mensal
            // [numeroPrestacoesSemestral] - numero de parcelas semestrais
            // [valorParcelaSemestral] - valor decimal + extenso da parcela semestral
            // [primeiroVencSemestral] - data do primeiro vencimento das parcelas semestrais
            // [saldoRemanescente] - valor decimal + extenso do saldo remanescente
            // [totalMeses] - total de meses das parcelas mensais 

            // [bancoCli] - banco do cliente para arrependimento
            // [agenciaCli] - agencia cliente  "      "
            // [contaCli] - conta do cliente   "      "

            // [nomeTestemunha1]
            // [endTestemunha1]
            // [rgTestemunha1]

            // [nomeTestemunha2]
            // [endTestemunha2]
            // [rgTestemunha2]



            try
            {
                var proposta = (from x in db.Propostas
                                join a in db.Lotes on new { Quadra = x.Quadra, Lote = x.Lote } equals new { Quadra = a.Quadra, Lote = a.Lote }
                                where x.Id == id
                                select new
                                {
                                    Id = id,
                                    Quadra = a.Quadra,
                                    Lote = a.Lote,
                                    ValorTotal = x.ValorTotal,
                                    Contrato = x.Contrato ?? "",
                                    BancoCliente = x.BancoCliente,
                                    AgenciaCliente = x.AgenciaCliente,
                                    ContaCliente = x.ContaCliente,
                                    DataProposta = x.DataProposta,
                                    PrimeiroVencMensal = x.PrimeiroVencMensal,
                                    PrimeiroVencSemestral = x.PrimeiroVencSemestral,
                                    Area = a.Area,
                                    ValorCorretagem = x.ValorCorretagem,
                                    CorretorNome = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Nome ?? "",
                                    CorretorCpf = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Cpf ?? "",
                                    CorretorCresci = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Cresci ?? ""
                                }).FirstOrDefault();

                var condicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == id).FirstOrDefault();
                var compradores = (from x in db.PropostasCompradores
                                   join c in db.Comprador on x.CompradorId equals c.Id
                                   where x.Id == id
                                   where c.DataExclusao == null
                                   where x.DataExclusao == null
                                   select c).OrderBy(c => c.Nome).ToList();
                var descritivo = db.LotesDescritivos.Where(c => c.Quadra == proposta.Quadra && c.Lote == proposta.Lote).FirstOrDefault();

                var valorTotalExtenso = Utils.ValorExtenso.ExtensoReal(proposta.ValorTotal);
                var valorTotalCorrigidoExtenso = Utils.ValorExtenso.ExtensoReal(condicoes.PrecoVendaCorrigido);
                var valorCorretagem = proposta.ValorCorretagem; 
                var saldoRemascenteExtenso = Utils.ValorExtenso.ExtensoReal(condicoes.SaldoQuitacao);
                var saldoPagarExtenso = Utils.ValorExtenso.ExtensoReal((proposta.ValorTotal - condicoes.ValorEntrada));
                var boletonumero = proposta.Quadra.TrimEnd() + proposta.Lote.ToString().PadLeft(5, '0');


                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    var pxPormm = 72 / 25.2F;
                    var pdf = new Document(PageSize.A4, 15 * pxPormm, 15 * pxPormm, 35 * pxPormm, 5 * pxPormm);
                    var writer = PdfWriter.GetInstance(pdf, memoryStream);
                    var fonte = new Font(fontebase, 10, iTextSharp.text.Font.NORMAL);
                    Eventos ev = new Eventos(fonte);
                    writer.PageEvent = new HeaderFooter();
                    
                    pdf.Open();

                    var fonteParagrafo = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.NORMAL);
                    var fonteBold = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.BOLD);
                    var fonteTitulo = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.BOLD);
                    var fonteReduzida = new iTextSharp.text.Font(fontebase, 8, iTextSharp.text.Font.NORMAL);
                    var fontesublinhada = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.UNDERLINE);



                    var titulo1 = new Paragraph("LOTEAMENTO RESIDENCIAL PIANOPOLI\n\n", fonteTitulo);
                    var titulo2 = new Paragraph("CONTRATO DE VENDA E COMPRA\n", fonteTitulo);
                    var titulo3 = new Paragraph("COM ALIENAÇÃO FIDUCIÁRIA EM GARANTIA\n", fonteTitulo);
                    var titulo4 = new Paragraph("E COM CONDIÇÕES SUSPENSIVAS\n\n\n", fonteTitulo);
                    var titulo5 = new Paragraph("Pelo presente instrumento particular, com força de escritura pública.\n\n", fontesublinhada);
                    var titulo6 = new Paragraph("LOTE " + proposta.Lote.ToString() + " - QUADRA " + proposta.Quadra.TrimEnd() + " - " + String.Format("{0:0,0.00}", proposta.Area) + " m2\n\n\n", fonteBold);
                    var titulo7 = new Paragraph("QUADRO RESUMO\n\n\n", fonteTitulo);
                    var titulo8 = new Paragraph("CAPÍTULO I - DAS PARTES\n\n", fonteTitulo);


                    titulo1.Alignment = Element.ALIGN_CENTER;
                    titulo2.Alignment = Element.ALIGN_CENTER;
                    titulo3.Alignment = Element.ALIGN_CENTER;
                    titulo4.Alignment = Element.ALIGN_CENTER;
                    titulo5.Alignment = Element.ALIGN_CENTER;
                    titulo6.Alignment = Element.ALIGN_CENTER;
                    titulo7.Alignment = Element.ALIGN_CENTER;
                    titulo8.Alignment = Element.ALIGN_LEFT;

                    pdf.Add(titulo1);
                    pdf.Add(titulo2);
                    pdf.Add(titulo3);
                    pdf.Add(titulo4);
                    pdf.Add(titulo5);
                    pdf.Add(titulo6);
                    pdf.Add(titulo7);
                    pdf.Add(titulo8);

                    PdfPCell cell = new PdfPCell();


                    //PdfPTable mtable = new PdfPTable(1);
                    //mtable.WidthPercentage = 100;
                    //mtable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


                    PdfPTable table = new PdfPTable(8);
                    table.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    table.WidthPercentage = 100;

                    // Paragrafo 1

                    var par1 = new Paragraph();
                    par1.Alignment = Element.ALIGN_JUSTIFIED;
                    
                    
                    // item 1.1
                    var txtPar_1_1 = new Phrase();
                    txtPar_1_1.Add(new Chunk("1.1", fonteParagrafo));

                    cell = new PdfPCell(txtPar_1_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // Col 2
                    cell = new PdfPCell(new Phrase("De um lado, como outorgante vendedora e credora fiduciária, ARJ EMPREENDIMENTOS IMOBILIÁRIOS LTDA., com sede em Ribeirão Preto – SP, na Rua Américo Brasiliense, 1856, Vila Seixas, CEP 14015-050, inscrita no CNPJ/MF sob n.º 18.739.252/0001-00, com seu contrato social registrado na Junta Comercial do Estado de São Paulo sob o NIRE  nº 35.227.734.539 em 23.08.2013 e última alteração contratual sob nº 451.536/19-8 em 04.09.2019, neste ato representada, na forma de seu contrato social, pelos seus representantes ao final assinados, daqui em diante chamada, simplesmente, por “ARJ”;\n\n", fonteParagrafo));
                    //cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.2f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    table.AddCell(cell);


                    // item 1.2
                    var txtPar_1_2 = new Phrase();
                    txtPar_1_2.Add(new Chunk("1.2", fonteParagrafo));

                    cell = new PdfPCell(txtPar_1_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    var frase_1_2 = new Phrase();
                    frase_1_2.Add(new Chunk("De outro lado, como outorgado comprador e devedor fiduciante, designado, doravante e abreviadamente, por ", fonteParagrafo));
                    frase_1_2.Add(new Chunk("COMPRADOR ", fonteBold));
                    frase_1_2.Add(new Chunk(", independentemente de gênero e número dos adquirentes:\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_1_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    var dadosCompradores = "";
                    foreach (var item in compradores)
                    {

                        dadosCompradores = "Nome Completo: " + item.Nome.TrimEnd() + "\n";
                        dadosCompradores += "Nacionalidade: " + item.Nacionalidade.TrimEnd() + "\n";
                        dadosCompradores += "Profissão: " + item.Profissao.TrimEnd() + "\n";
                        dadosCompradores += "RG: " + item.Rg.TrimEnd() + "\n";
                        dadosCompradores += "CPF: " + Convert.ToUInt64(item.Cpf.TrimEnd()).ToString(@"\000\.000\.000\-00") + "\n";
                        dadosCompradores += "Endereço: " + item.Logradouro.TrimEnd() + " " + item.Numero.TrimEnd() + " " + item.Complemento.TrimEnd() + " " + item.Bairro.TrimEnd() + " " + item.Municipio.TrimEnd() + " " + item.Estado + " " + item.Cep + "\n";
                        dadosCompradores += "E-mail: " + item.Email.TrimEnd() + "\n";
                        switch (item.EstadoCivil)
                        {
                            case "1":
                                dadosCompradores += "Estado Civil: Solteiro\n";
                                break;
                            case "2":
                                dadosCompradores += "Estado Civil: Casado(a)\n";
                                break;
                            case "3":
                                dadosCompradores += "Estado Civil: Solteiro\n";
                                break;
                            case "4":
                                dadosCompradores += "Estado Civil: Divorciado(a)";
                                break;
                            case "5":
                                dadosCompradores += "Estado Civil: Viúvo(a)";
                                break;
                            default:
                                dadosCompradores += "Estado Civil: Solteiro(a)";
                                break;
                        }

                        // Dados do comprador
                        if (item.EstadoCivil == "2")
                        {
                            // dados do casamento - se for casado
                            dadosCompradores += "Regime Casamento: " + item.CasamentoRegime.TrimEnd() + "\n";
                            dadosCompradores += "Data Casamento: " + item.CasamentoData.Value.ToShortDateString() + "\n";
                            if (item.CasamentoEscrRegistro != null)
                            {
                                dadosCompradores += "Escritura do Pacto Antenupicial - " + "Tabelião: " + item.CasamentoEscrTabeliao ?? "" + "Livro: " + item.CasamentoLivro.TrimEnd() ?? "" + " Fls. " + item.CasamentoFolhas.TrimEnd() ?? "" + "\n";
                                dadosCompradores += "Registro de Imóveis: " + item.CasamentoEscrRegistro.TrimEnd() + "\n";

                            }

                            dadosCompradores += "\nDados do Cônjuge \n\n";
                            // dados do cônjuge - se for casado
                            dadosCompradores += "Nome: " + item.ConjugeNome.TrimEnd() + "\n";
                            dadosCompradores += "Celular: " + item.ConjugeCelular + "\n"; ;
                            dadosCompradores += "Nacionalidade: " + item.ConjugeNacionalidade.TrimEnd() + "\n";
                            dadosCompradores += "Profissão: " + item.ConjugeProfissao.TrimEnd() + "\n";
                            dadosCompradores += "RG: " + item.ConjugeRg.TrimEnd() + "\n";
                            dadosCompradores += "CPF: " + Convert.ToUInt64(item.ConjugeCpf.TrimEnd()).ToString(@"\000\.000\.000\-00") + "\n";
                            //  dadosCompradores += "Endereço: " + item.ConjugeLogradouro.TrimEnd()??"" + " " + item.ConjugeNumero.TrimEnd()??"" + " " + item.ConjugeBairro.TrimEnd()??"" + " " + " " + item.ConjugeMunicipio.TrimEnd() + " " + item.ConjugeEstado + " " + "\n";
                            if (item.ConjugeEmail != null)
                            {
                                dadosCompradores += "E-mail: " + item.ConjugeEmail.TrimEnd() + "\n";
                            }

                        }
                        dadosCompradores += "\n\n";

                    }

                    var txtfrase_dados_comprador = new Phrase();
                    txtfrase_dados_comprador.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txtfrase_dados_comprador);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    var frase_dados_comprador = new Phrase();
                    frase_dados_comprador.Add(new Chunk(dadosCompradores, fonteParagrafo));
                    cell = new PdfPCell(frase_dados_comprador);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 1.2.1
                    var txtPar_1_2_1 = new Phrase();
                    txtPar_1_2_1.Add(new Chunk("1.2.1", fonteParagrafo));
                    cell = new PdfPCell(txtPar_1_2_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    var frase_1_2_1 = new Phrase();
                    frase_1_2_1.Add(new Chunk("Na hipótese de ser mais de um COMPRADOR, estes são solidários entre si em todas as obrigações ajustadas no presente contrato (“Contrato”), especialmente quanto ao pagamento do preço (“Preço”).\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_1_2_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    var txtTitulo2 = new Phrase();
                    txtTitulo2.Add(new Chunk("CAPÍTULO II - DO IMÓVEL\n\n\n", fonteBold));

                    cell = new PdfPCell(txtTitulo2);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);
                                    
                    // item 2.1
                    var txt_2_1 = new Phrase();
                    txt_2_1.Add(new Chunk("2.1", fonteParagrafo));

                    cell = new PdfPCell(txt_2_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var frase_2_1 = new Phrase();
                    frase_2_1.Add(new Chunk("Lote n.º " + proposta.Lote.ToString() + ", da Quadra n.º " + proposta.Quadra + ", integrante do Loteamento Residencial Pianopoli (“Loteamento”), situado no Município de Araraquara-SP, registrado sob n.º ---, em --- de --- de ---, na Matrícula n.º ---, do ---º Oficial de Registro de Imóveis de Araraquara (“---º RI”).\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_2_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 2.2
                    var txt_2_2 = new Phrase();
                    txt_2_2.Add(new Chunk("2.2", fonteParagrafo));

                    cell = new PdfPCell(txt_2_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var frase_2_2 = new Phrase();
                    frase_2_2.Add(new Chunk("Referido lote (“Imóvel”), objetivado pela M. ---, do ---º RI, tem a seguinte descrição e confrontação: " + descritivo.Descritivo + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_2_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // Capítulo III
                    var txtCapitulo3 = new Phrase();
                    txtCapitulo3.Add(new Chunk("CAPÍTULO III - DO PREÇO\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo3);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 3.1
                    var txt_3_1 = new Phrase();
                    txt_3_1.Add(new Chunk("3.1", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1 = "O Preço, certo e ajustado, para pagamento é de R$ " + String.Format("{0:0,0.00}", proposta.ValorTotal) + " (" + valorTotalExtenso + ") acrescidos anualmente por juros de 3% e corrigido pelo IPCA – ÍNDICE NACIONAL DE PREÇOS AO CONSUMIDOR, conforme ajustado abaixo. Logo, o preço com os referidos juros pactuados, calculados pela Tabela Price, totaliza o montante de R$ " + String.Format("{0:0,0.00}", condicoes.PrecoVendaCorrigido) + " (" + valorTotalCorrigidoExtenso + ") em: " + condicoes.NrParcelasMensais.ToString() + " meses.";

                    var frase_3_1 = new Phrase();
                    frase_3_1.Add(new Chunk(texto3_1 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 3.1.1
                    var txt_3_1_1 = new Phrase();
                    txt_3_1_1.Add(new Chunk("3.1.1", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_1 = "A COMISSÃO DE CORRETAGEM PELA INTERMEDIAÇÃO DA PRESENTE VENDA E COMPRA NÃO INTEGRA O PREÇO.";

                    var frase_3_1_1 = new Phrase();
                    frase_3_1_1.Add(new Chunk(texto3_1_1 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 3.1.2
                    var txt_3_1_2 = new Phrase();
                    txt_3_1_2.Add(new Chunk("3.1.2", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_2 = "O COMPRADOR é o único e exclusivo responsável pelo pagamento da despesa com os serviços de corretagem diretamente ao credor respectivo (a empresa imobiliária e/ou o corretor associado, a seguir identificados), não podendo, sob qualquer hipótese, ser a ARJ responsabilizada pelo pagamento da referida despesa.";

                    var frase_3_1_2 = new Phrase();
                    frase_3_1_2.Add(new Chunk(texto3_1_2 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 3.1.3
                    var txt_3_1_3 = new Phrase();
                    txt_3_1_3.Add(new Chunk("3.1.3", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_3 = "O valor da despesa com os serviços de corretagem a ser pago diretamente pelo COMPRADOR à imobiliária e/ou aos corretores associados identificados no quadro abaixo totaliza R$ " + String.Format("{0:0,0.00}", valorCorretagem) +", com o que está plenamente de acordo:";

                    var frase_3_1_3 = new Phrase();
                    frase_3_1_3.Add(new Chunk(texto3_1_3 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    //
                    // Tabela da Corretagem
                    // 
                    // 
                    var txt_tab_3_1_3 = new Phrase();
                    txt_tab_3_1_3.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txt_tab_3_1_3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    PdfPTable tab_3_1_3 = new PdfPTable(5);

                    PdfPCell coluna1 = new PdfPCell(new Phrase( new Chunk("NOME",fonteParagrafo)));
                    coluna1.Colspan = 1;
                    coluna1.HorizontalAlignment= Element.ALIGN_CENTER;
                    coluna1.SetLeading(0.0f, 1.3f);
                    tab_3_1_3.AddCell(coluna1);

                    PdfPCell coluna2 = new PdfPCell(new Phrase(new Chunk("CRECI", fonteParagrafo)));
                    coluna2.Colspan = 1;
                    coluna2.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna2.SetLeading(0.0f, 1.3f);
                    tab_3_1_3.AddCell(coluna2);

                    PdfPCell coluna3 = new PdfPCell(new Phrase(new Chunk("CPF", fonteParagrafo)));
                    coluna3.Colspan = 1;
                    coluna3.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna3.SetLeading(0.0f, 1.3f);
                    tab_3_1_3.AddCell(coluna3);

                    PdfPCell coluna4 = new PdfPCell(new Phrase(new Chunk("VALOR", fonteParagrafo)));
                    coluna4.Colspan = 1;
                    coluna4.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna4.SetLeading(0.0f, 1.3f);
                    tab_3_1_3.AddCell(coluna4);

                    PdfPCell coluna5 = new PdfPCell(new Phrase(new Chunk("DATA PAGTO", fonteParagrafo)));
                    coluna5.Colspan = 1;
                    coluna5.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna5.SetLeading(0.0f, 1.3f);
                    tab_3_1_3.AddCell(coluna5);


                    cell = new PdfPCell(tab_3_1_3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);




                    // item 3.1.4
                    var txt_3_1_4 = new Phrase();
                    txt_3_1_4.Add(new Chunk("3.1.4", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_4);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_4 = "O COMPRADOR tem conhecimento de que, pelas normas vigentes, o valor acima poderá ser incluído em seu imposto de renda como custo de aquisição";

                    var frase_3_1_4 = new Phrase();
                    frase_3_1_4.Add(new Chunk(texto3_1_4 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_4);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 3.1.5
                    var txt_3_1_5 = new Phrase();
                    txt_3_1_5.Add(new Chunk("3.1.5", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_5);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_5 = "O valor da comissão sempre deverá ser pago de forma pré-datada com prazo nunca inferior a 8 (oito) dias da data do vencimento do boleto de pagamento da data da entrada do terreno.";

                    var frase_3_1_5 = new Phrase();
                    frase_3_1_5.Add(new Chunk(texto3_1_5 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_5);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 3.1.6
                    var txt_3_1_6 = new Phrase();
                    txt_3_1_6.Add(new Chunk("3.1.6", fonteParagrafo));

                    cell = new PdfPCell(txt_3_1_6);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto3_1_6 = "É imprescindível que o Corretor/ imobiliária sempre consulte a ARJ sobre a quitação do boleto de entrada, antes de depositar em sua conta o valor recebido pela comissão, evitando ter de restituir o valor recebido em casos de desistência do COMPRADOR.";

                    var frase_3_1_6 = new Phrase();
                    frase_3_1_6.Add(new Chunk(texto3_1_6 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_3_1_6);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // CAPÍTULO IV
                    var txtCapitulo4 = new Phrase();
                    txtCapitulo4.Add(new Chunk("CAPÍTULO IV - DA FORMA DE PAGAMENTO\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo4);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 4.1
                    var txt_4_1 = new Phrase();
                    txt_4_1.Add(new Chunk("4.1", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1 = "O Preço de aquisição do Imóvel será pago pelo COMPRADOR da seguinte forma:";

                    var frase_4_1 = new Phrase();
                    frase_4_1.Add(new Chunk(texto4_1 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.1 - a
                    var txt_4_1a = new Phrase();
                    txt_4_1a.Add(new Chunk("a)", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1a);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1a = "a título de sinal ou de entrada, a parcela de R$ " + String.Format("{0:0,0.00}", condicoes.ValorEntrada) + " (a “Parcela de Sinal”), por meio de boleto bancário emitido neste ato, e da qual a ARJ dá a devida quitação, condicionada à efetiva compensação de boleto bancário nº " + boletonumero + " emitido pelo banco ITAÚ S/A ou usando pagamento eletrônico como PIX, TED ou TEF, Termo de Proposta nº " + proposta.Id.ToString().PadLeft(6, '0') +". ";

                    var frase_4_1a = new Phrase();
                    frase_4_1a.Add(new Chunk(texto4_1a + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1a);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.1 - b
                    var txt_4_1b = new Phrase();
                    txt_4_1b.Add(new Chunk("b)", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1b);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1b = "a parcela de R$ " + String.Format("{0:0,0.00}", (proposta.ValorTotal - condicoes.ValorEntrada)) + " (" + saldoPagarExtenso + ") (o “Saldo do Preço”), a ser acrescida anualmente por juros de 3% e corrigida pelo IPCA – Índice Nacional de Preços ao Consumidor, conforme item 3.2 das Normas Gerais, por meio de: ";

                    var frase_4_1b = new Phrase();
                    frase_4_1b.Add(new Chunk(texto4_1b + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1b);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 4.1 - b.1 
                    var txt_4_1b1 = new Phrase();
                    txt_4_1b1.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1b1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1b1 = "b.1)  – " + condicoes.NrParcelasMensais.ToString() + " prestações mensais, iguais e sucessivas, corrigidas monetariamente na forma do item 3.2 das Normas Gerais, no valor unitário de R$ " + String.Format("{0:0,0.00}", condicoes.ValorParcelaMensal) + ", vencendo-se a primeira (1.ª) no dia " + proposta.PrimeiroVencMensal.Value.ToShortDateString() + ", e as demais nos mesmos dias dos meses subsequentes, até final liquidação;";

                    var frase_4_1b1 = new Phrase();
                    frase_4_1b1.Add(new Chunk(texto4_1b1 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1b1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.1 - b.2 
                    var txt_4_1b2 = new Phrase();
                    txt_4_1b2.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1b2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1b2 = "b.2)	– " + condicoes.NrParcelasSemestrais.ToString() + " prestações semestrais, iguais e sucessivas, corrigidas monetariamente na forma do item 3.2 das Normas Gerais, no valor unitário de R$ " + String.Format("{0:0,0.00}", condicoes.ValorParcelaSemestral) + ", vencendo-se a primeira em " + proposta.PrimeiroVencSemestral.Value.ToShortDateString() + " e as demais em igual dia dos semestres subsequentes, até final liquidação.";

                    var frase_4_1b2 = new Phrase();
                    frase_4_1b2.Add(new Chunk(texto4_1b2 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1b2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.1 - b.3 
                    var txt_4_1b3 = new Phrase();
                    txt_4_1b3.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txt_4_1b3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_1b3 = "b.3) \t– R$ " + String.Format("{0:0,0.00}", condicoes.SaldoQuitacao) + " (" + saldoRemascenteExtenso + ") saldo remanescente ao final dos " + condicoes.NrParcelasMensais.ToString() + " meses, corrigido monetariamente na forma do item 3.2 das normas gerais.";

                    var frase_4_1b3 = new Phrase();
                    frase_4_1b3.Add(new Chunk(texto4_1b3 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_1b3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.2
                    var txt_4_2 = new Phrase();
                    txt_4_2.Add(new Chunk("4.2", fonteParagrafo));

                    cell = new PdfPCell(txt_4_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_2 = "Correção Monetária: As prestações e o Saldo do Preço serão corrigidos anualmente de acordo com a variação percentual mensal acumulada do IPCA conforme previsto no item 3.2 das Normas Gerais.";

                    var frase_4_2 = new Phrase();
                    frase_4_2.Add(new Chunk(texto4_2 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.3
                    var txt_4_3 = new Phrase();
                    txt_4_3.Add(new Chunk("4.3", fonteParagrafo));

                    cell = new PdfPCell(txt_4_3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_3 = "Juros compensatórios:  as prestações e o Saldo do Preço serão acrescidos de juros efetivos e nominais  de 3% (três por cento) ao ano, calculados pela Tabela Price.";

                    var frase_4_3 = new Phrase();
                    frase_4_3.Add(new Chunk(texto4_3 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 4.4
                    var txt_4_4 = new Phrase();
                    txt_4_4.Add(new Chunk("4.4", fonteParagrafo));

                    cell = new PdfPCell(txt_4_4);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto4_4 = "Caso o COMPRADOR não pague o boleto de entrada no prazo de até 05 (cinco) dias úteis da data do seu vencimento operar-se-á o Distrato Automático do presente contrato, voltando o referido lote do presente contrato ao estoque da ARJ que poderá imediatamente comercializá-lo a outro interessado.";

                    var frase_4_4 = new Phrase();
                    frase_4_4.Add(new Chunk(texto4_4 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_4_4);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // CAPÍTULO V
                    var txtCapitulo5 = new Phrase();
                    txtCapitulo5.Add(new Chunk("CAPÍTULO V - DA CONCLUSÃO DAS OBRAS DE INFRAESTRUTURA\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo5);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 5.1
                    var txt_5_1 = new Phrase();
                    txt_5_1.Add(new Chunk("5.1", fonteParagrafo));

                    cell = new PdfPCell(txt_5_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto5_1 = "Prazo para conclusão das obras de infraestrutura: --- de --- de ---, ou seja, até 48 (quarenta e oito) meses, contados a partir do registro do Loteamento.";

                    var frase_5_1 = new Phrase();
                    frase_5_1.Add(new Chunk(texto5_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_5_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 5.1.1
                    var txt_5_1_1 = new Phrase();
                    txt_5_1_1.Add(new Chunk("5.1.1", fonteParagrafo));

                    cell = new PdfPCell(txt_5_1_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto5_1_1 = "Prazo de Tolerância: O prazo do item 5.1 observará uma tolerância de 180 (cento e oitenta) dias corridos, ou seja, até --- de --- de --- (“Prazo de Tolerância”), consoante regramento previsto no Capítulo IX das Normas Gerais, abaixo.";

                    var frase_5_1_1 = new Phrase();
                    frase_5_1_1.Add(new Chunk(texto5_1_1 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_5_1_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 5.2
                    var txt_5_2 = new Phrase();
                    txt_5_2.Add(new Chunk("5.2", fonteParagrafo));

                    cell = new PdfPCell(txt_5_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto5_2 = "Prazo para protocolo do pedido de emissão do Termo de Verificação de Execução de Obras (“TVEO”): --- de --- de ---, observada a mesma tolerância de 180 (cento e oitenta) dias corridos, caso utilizado o Prazo de Tolerância.";

                    var frase_5_2 = new Phrase();
                    frase_5_2.Add(new Chunk(texto5_2 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_5_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 5.3
                    var txt_5_3 = new Phrase();
                    txt_5_3.Add(new Chunk("5.3", fonteParagrafo));

                    cell = new PdfPCell(txt_5_3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto5_3 = "As partes estabelecem que o Loteamento será tido como entregue na data de postagem de carta registrada a ser enviada pela ARJ ao COMPRADOR, comunicando (“Comunicação”) a conclusão das Obras de infraestrutura do Loteamento, caracterizada pelo protocolo, junto à Municipalidade, do pedido de emissão do TVEO, ou documento equivalente.";

                    var frase_5_3 = new Phrase();
                    frase_5_3.Add(new Chunk(texto5_3 + "\n\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_5_3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // CAPÍTULO VI
                    var txtCapitulo6 = new Phrase();
                    txtCapitulo6.Add(new Chunk("CAPÍTULO VI - DA POSSE\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo6);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 6.1
                    var txt_6_1 = new Phrase();
                    txt_6_1.Add(new Chunk("6.1", fonteParagrafo));

                    cell = new PdfPCell(txt_6_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto6_1 = "Superadas as Condições Suspensivas, abaixo previstas, o COMPRADOR ficará automaticamente imitido na posse direta do Imóvel, ficando a ARJ com a posse indireta, na qualidade de credora fiduciária.\n\n";

                    var frase_6_1 = new Phrase();
                    frase_6_1.Add(new Chunk(texto6_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_6_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 6.2
                    var txt_6_2 = new Phrase();
                    txt_6_2.Add(new Chunk("6.2", fonteParagrafo));

                    cell = new PdfPCell(txt_6_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto6_2 = "Entretanto, o COMPRADOR apenas poderá executar benfeitorias, acessões e melhoramentos no Imóvel, a partir do deferimento da Comunicação aludida no item 5.3 pelos órgãos competentes e desde que observadas as regras constantes neste contrato.\n\n";

                    var frase_6_2 = new Phrase();
                    frase_6_2.Add(new Chunk(texto6_2 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_6_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // CAPÍTULO VII
                    var txtCapitulo7 = new Phrase();
                    txtCapitulo7.Add(new Chunk("CAPÍTULO VII - DA POSSIBILIDADE DO EXERCÍCIO DO DIREITO DE ARREPENDIMENTO\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo7);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 7.1
                    var txt_7_1 = new Phrase();
                    txt_7_1.Add(new Chunk("7.1", fonteParagrafo));

                    cell = new PdfPCell(txt_7_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto7_1 = "Direito de Arrependimento: Como o Contrato foi celebrado em estande de venda ou fora da sede da ARJ, o COMPRADOR tem assegurado o direito de arrependimento, durante o prazo improrrogável de 7 (sete) dias, contados desta data, conforme regrado no Capítulo XV das Normas Gerais, abaixo.\n\n";

                    var frase_7_1 = new Phrase();
                    frase_7_1.Add(new Chunk(texto7_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_7_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 7.2
                    var txt_7_2 = new Phrase();
                    txt_7_2.Add(new Chunk("7.2", fonteParagrafo));

                    cell = new PdfPCell(txt_7_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto7_2 = "Para fins de restituição do Preço, na hipótese de exercício do direito de arrependimento, o COMPRADOR indica a seguinte conta corrente de sua titularidade:\n\n";

                    var frase_7_2 = new Phrase();
                    frase_7_2.Add(new Chunk(texto7_2 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_7_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    //
                    // Tabela dos Dados Bancários
                    // 
                    // 
                    var txt_tab_7_2 = new Phrase();
                    txt_tab_7_2.Add(new Chunk(" ", fonteParagrafo));

                    cell = new PdfPCell(txt_tab_7_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    PdfPTable tab_7_2 = new PdfPTable(3);

                    PdfPCell coluna7_1 = new PdfPCell(new Phrase(new Chunk("Banco", fonteParagrafo)));
                    coluna7_1.Colspan = 1;
                    coluna7_1.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna7_1.SetLeading(0.0f, 1.3f);
                    tab_7_2.AddCell(coluna7_1);

                    PdfPCell coluna7_2 = new PdfPCell(new Phrase(new Chunk("Agência", fonteParagrafo)));
                    coluna7_2.Colspan = 1;
                    coluna7_2.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna7_2.SetLeading(0.0f, 1.3f);
                    tab_7_2.AddCell(coluna7_2);

                    PdfPCell coluna7_3 = new PdfPCell(new Phrase(new Chunk("Conta", fonteParagrafo)));
                    coluna7_3.Colspan = 1;
                    coluna7_3.HorizontalAlignment = Element.ALIGN_CENTER;
                    coluna7_3.SetLeading(0.0f, 1.3f);
                    tab_7_2.AddCell(coluna7_3);

                    cell = new PdfPCell(tab_7_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    
                    // CAPÍTULO VIII
                    var txtCapitulo8 = new Phrase();
                    txtCapitulo8.Add(new Chunk("\n\nCAPÍTULO VIII - DOS ÔNUS\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo8);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 8.1
                    var txt_8_1 = new Phrase();
                    txt_8_1.Add(new Chunk("8.1", fonteParagrafo));

                    cell = new PdfPCell(txt_8_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto8_1 = "Ônus: O Imóvel se acha inteiramente livre e desembaraçado de toda e qualquer restrição, real ou pessoal, judicial ou extrajudicial.\n\n";

                    var frase_8_1 = new Phrase();
                    frase_8_1.Add(new Chunk(texto8_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_8_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // CAPÍTULO IX
                    var txtCapitulo9 = new Phrase();
                    txtCapitulo9.Add(new Chunk("CAPÍTULO IX - DA COMISSÃO DE CORRETAGEM PELA INTERMEDIAÇÃO E DAS DISPOSIÇÕES GERAIS\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo9);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 9.1
                    var txt_9_1 = new Phrase();
                    txt_9_1.Add(new Chunk("9.1", fonteParagrafo));

                    cell = new PdfPCell(txt_9_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto9_1 = "O COMPRADOR FOI INFORMADO E CONCORDA QUE O PAGAMENTO DA COMISSÃO DE CORRETAGEM NÃO INTEGRA O PREÇO DO IMÓVEL E QUE FARÁ O PAGAMENTO DIRETAMENTE AO CORRETOR ASSOCIADO E/OU À IMOBILIÁRIA.\n";

                    var frase_9_1 = new Phrase();
                    frase_9_1.Add(new Chunk(texto9_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_9_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 9.1.1
                    var txt_9_1_1 = new Phrase();
                    txt_9_1_1.Add(new Chunk("9.1.1", fonteParagrafo));

                    cell = new PdfPCell(txt_9_1_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto9_1_1 = "O COMPRADOR ESTÁ CIENTE QUE O VALOR DA COMISSÃO DE CORRETAGEM,  MENCIONADO NO CAPÍTULO III, ACIMA, NÃO SERÁ DEVOLVIDO EM HIPÓTESE ALGUMA, UMA VEZ QUE ESSA VENDA E COMPRA SE CARACTERIZA COMO OPERAÇÃO DEFINITIVA DE VENDA E COMPRA, OBSERVADAS AS CONDIÇÕES SUSPENSIVAS ABAIXO PREVISTAS.\n\n\n";

                    var frase_9_1_1 = new Phrase();
                    frase_9_1_1.Add(new Chunk(texto9_1_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_9_1_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 9.1.1 (assinaturas)
                    var txt_9_1_1a = new Phrase();
                    txt_9_1_1a.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_9_1_1a);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto9_1_1a = "                ____________________________              __________________________";

                    var frase_9_1_1a = new Phrase();
                    frase_9_1_1a.Add(new Chunk(texto9_1_1a + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_9_1_1a);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 9.1.1 (assinaturas b)
                    var txt_9_1_1b = new Phrase();
                    txt_9_1_1b.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_9_1_1b);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto9_1_1b = "                      COMPRADOR                                        COMPRADOR\n";

                    var frase_9_1_1b = new Phrase();
                    frase_9_1_1b.Add(new Chunk(texto9_1_1b + "\n\n", fonteParagrafo));
                    cell = new PdfPCell(frase_9_1_1b);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 9.2
                    var txt_9_2 = new Phrase();
                    txt_9_2.Add(new Chunk("9.2", fonteParagrafo));

                    cell = new PdfPCell(txt_9_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto9_2 = "Manifestação do COMPRADOR: Foi concedida ao COMPRADOR a oportunidade para previamente examinar este Contrato, pelo que declara estar bem esclarecido quanto às condições contratuais, não tendo ele qualquer alteração a solicitar e aceitando, na íntegra, as cláusulas deste Contrato, bem como declara ter conferido todo o Quadro Resumo.\n\n\n";

                    var frase_9_2 = new Phrase();
                    frase_9_2.Add(new Chunk(texto9_2 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_9_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // CAPÍTULO X
                    var txtCapitulo10 = new Phrase();
                    txtCapitulo10.Add(new Chunk("CAPÍTULO X - DAS RESTRIÇÕES DE USO E OCUPAÇÃO DO LOTE\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo10);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 10.1
                    var txt_10_1 = new Phrase();
                    txt_10_1.Add(new Chunk("10.1", fonteParagrafo));

                    cell = new PdfPCell(txt_10_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto10_1 = "O COMPRADOR tem ciência e está de acordo com as restrições que a ARJ estabeleceu para o Loteamento, constantes no Regulamento Construtivo (ANEXO V), integrante do presente instrumento.\n";

                    var frase_10_1 = new Phrase();
                    frase_10_1.Add(new Chunk(texto10_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_10_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // CAPÍTULO XI
                    var txtCapitulo11 = new Phrase();
                    txtCapitulo11.Add(new Chunk("CAPÍTULO XI - ASSOCIAÇÃO\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo11);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 11.1
                    var txt_11_1 = new Phrase();
                    txt_11_1.Add(new Chunk("11.1", fonteParagrafo));

                    cell = new PdfPCell(txt_11_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto11_1 = "O COMPRADOR tem conhecimento e concorda, como condição essencial do presente negócio, com sua filiação à Associação dos Moradores do Residencial Pianopoli (a “ASSOCIAÇÃO”), neste ato, independentemente de qualquer outra formalidade.\n";

                    var frase_11_1 = new Phrase();
                    frase_11_1.Add(new Chunk(texto11_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_11_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 11.2
                    var txt_11_2 = new Phrase();
                    txt_11_2.Add(new Chunk("11.2", fonteParagrafo));

                    cell = new PdfPCell(txt_11_2);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto11_2 = "A ASSOCIAÇÃO é uma entidade, sem fins lucrativos, que visa, dentre outras finalidades, à defesa e à preservação de direitos e interesses coletivos ou difusos, de qualquer natureza, dos moradores do Loteamento, promovendo, por si ou por terceiros que contratar e nomear, a preservação das características do Loteamento, zelando por sua adequada utilização e a prestação de serviços em prol de seus Associados, tanto os serviços necessários, como os de comodidade.\n";

                    var frase_11_2 = new Phrase();
                    frase_11_2.Add(new Chunk(texto11_2 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_11_2);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 11.3
                    var txt_11_3 = new Phrase();
                    txt_11_3.Add(new Chunk("11.3", fonteParagrafo));

                    cell = new PdfPCell(txt_11_3);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto11_3 = "Além das disposições constantes no Capítulo XIV das Normas Gerais, o COMPRADOR declara estar ciente das obrigações e direitos decorrentes da sua qualidade de Associado Contribuinte, nos termos do Estatuto Social da ASSOCIAÇÃO, cuja cópia integra o presente como ANEXO IV.\n";

                    var frase_11_3 = new Phrase();
                    frase_11_3.Add(new Chunk(texto11_3 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_11_3);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // CAPÍTULO XII
                    var txtCapitulo12 = new Phrase();
                    txtCapitulo12.Add(new Chunk("CAPÍTULO XII - DAS CONDIÇÕES SUSPENSIVAS\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo12);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 12.1
                    var txt_12_1 = new Phrase();
                    txt_12_1.Add(new Chunk("12.1", fonteParagrafo));

                    cell = new PdfPCell(txt_12_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto12_1 = "Conforme o Capítulo XVI das Normas Gerais , como exceção à irrevogabilidade e irretratabilidade, a eficácia do presente Contrato acha-se subordinada às seguintes condições suspensivas (“Condições Suspensivas”):\n";

                    var frase_12_1 = new Phrase();
                    frase_12_1.Add(new Chunk(texto12_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_12_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 12.1a
                    var txt_12_1a = new Phrase();
                    txt_12_1a.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_12_1a);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto12_1a = "a) -\tcompensação, em até 05 (cinco) dias úteis, contados da data de vencimento do(s) boleto(s) representativos da Parcela de Sinal na conta da ARJ;\n";

                    var frase_12_1a = new Phrase();
                    frase_12_1a.Add(new Chunk(texto12_1a + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_12_1a);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 12.1b
                    var txt_12_1b = new Phrase();
                    txt_12_1b.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_12_1b);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto12_1b = "b) -\tnão exercício, em até 07 (sete) dias, contados desta data, do Direito de Arrependimento pelo COMPRADOR;\n";

                    var frase_12_1b = new Phrase();
                    frase_12_1b.Add(new Chunk(texto12_1b + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_12_1b);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 12.1b1
                    var txt_12_1b1 = new Phrase();
                    txt_12_1b1.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_12_1b1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto12_1b1 = "     b.1) essa condição suspensiva não é aplicável para contratos assinados na sede da ARJ;\n";

                    var frase_12_1b1 = new Phrase();
                    frase_12_1b1.Add(new Chunk(texto12_1b1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_12_1b1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // CAPÍTULO XIII
                    var txtCapitulo13 = new Phrase();
                    txtCapitulo13.Add(new Chunk("CAPÍTULO XIII - ANEXOS\n\n\n", fonteBold));

                    cell = new PdfPCell(txtCapitulo13);
                    cell.Colspan = 8;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 13.1
                    var txt_13_1 = new Phrase();
                    txt_13_1.Add(new Chunk("13.1", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1 = "Consiste parte integrante do presente os anexos abaixo relacionados, cujo teor é de conhecimento do COMPRADOR, que declara estar de acordo:\n";

                    var frase_13_1 = new Phrase();
                    frase_13_1.Add(new Chunk(texto13_1 + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 13.1-a
                    var txt_13_1a = new Phrase();
                    txt_13_1a.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1a);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1a = "a) – \tPlanta Geral, com identificação do Lote (ANEXO I);\n";

                    var frase_13_1a = new Phrase();
                    frase_13_1a.Add(new Chunk(texto13_1a + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1a);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 13.1-b
                    var txt_13_1b = new Phrase();
                    txt_13_1b.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1b);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1b = "b) – \tRegulamento Interno da Associação dos Moradores (ANEXO II);\n";

                    var frase_13_1b = new Phrase();
                    frase_13_1b.Add(new Chunk(texto13_1b + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1b);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 13.1-c
                    var txt_13_1c = new Phrase();
                    txt_13_1c.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1c);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1c = "c) – \tMemorial de Obras (ANEXO III);\n";

                    var frase_13_1c = new Phrase();
                    frase_13_1c.Add(new Chunk(texto13_1c + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1c);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    // item 13.1-d
                    var txt_13_1d = new Phrase();
                    txt_13_1d.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1d);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1d = "d) – \tCópia do Estatuto Social da ASSOCIAÇÃO (ANEXO IV);\n";

                    var frase_13_1d = new Phrase();
                    frase_13_1d.Add(new Chunk(texto13_1d + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1d);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item 13.1-e
                    var txt_13_1e = new Phrase();
                    txt_13_1e.Add(new Chunk("", fonteParagrafo));

                    cell = new PdfPCell(txt_13_1e);
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    cell.SetLeading(0.0f, 1.3f);
                    table.AddCell(cell);

                    var texto13_1e = "e) –   Regulamento Construtivo (ANEXO V).\n";

                    var frase_13_1e = new Phrase();
                    frase_13_1e.Add(new Chunk(texto13_1e + "\n", fonteParagrafo));
                    cell = new PdfPCell(frase_13_1e);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 7;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    // item final

                    var textofinal = "E, por estarem, assim, justos e contratados, assinam o presente em três (03) vias, de um só teor, na presença das duas (02) testemunhas abaixo.\n";

                    var frasefinal = new Phrase();
                    frasefinal.Add(new Chunk(textofinal + "\n", fonteParagrafo));
                    cell = new PdfPCell(frasefinal);
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                    cell.SetLeading(0.0f, 1.3f);
                    cell.Colspan = 8;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);

                    par1.Add(table);
                    
                    pdf.Add(par1);
                    
                    pdf.Close();

                    byte[] file = memoryStream.ToArray();
                    MemoryStream ms = new MemoryStream();
                    ms.Write(file, 0, file.Length);
                    ms.Flush();
                    ms.Position = 0;

                    return new FileStreamResult(ms, "application/pdf");


                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        public class HeaderFooter : PdfPageEventHelper
        {
            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                base.OnOpenDocument(writer, document);  
            }
            public override void OnStartPage(PdfWriter writer, Document document)
            {
                var fonteReduzida = new iTextSharp.text.Font(fontebase, 8, iTextSharp.text.Font.NORMAL);
                //base.OnEndPage(writer, document);
                PdfPTable tbHeader = new PdfPTable(3);
                tbHeader.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                tbHeader.DefaultCell.Border = 0;

                tbHeader.AddCell(new Paragraph());
                PdfPCell _cell = new PdfPCell(new Paragraph("ARJ Empreendimentos"));
                _cell.HorizontalAlignment = Element.ALIGN_CENTER;
                _cell.Border = 0;
                tbHeader.AddCell(_cell);
                tbHeader.AddCell(new Paragraph());

                tbHeader.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetTop(document.TopMargin) + 60, writer.DirectContent);

            }
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var fonteReduzida = new iTextSharp.text.Font(fontebase, 8, iTextSharp.text.Font.NORMAL);
                ////base.OnEndPage(writer, document);
                //PdfPTable tbHeader = new PdfPTable(3);
                //tbHeader.TotalWidth= document.PageSize.Width -document.LeftMargin - document.RightMargin;
                //tbHeader.DefaultCell.Border = 0;

                //tbHeader.AddCell(new Paragraph());
                //PdfPCell _cell = new PdfPCell(new Paragraph("ARJ Empreendimentos"));
                //_cell.HorizontalAlignment = Element.ALIGN_CENTER;
                //_cell.Border = 0;
                //tbHeader.AddCell(_cell);
                //tbHeader.AddCell(new Paragraph());

                //tbHeader.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetTop(document.TopMargin)-30, writer.DirectContent);

                PdfPTable tbFooter = new PdfPTable(3);
                tbFooter.TotalWidth = document.PageSize.Width -document.LeftMargin - document.RightMargin;
                tbFooter.DefaultCell.Border = 0;

                tbFooter.AddCell(new Paragraph());
                PdfPCell _cell = new PdfPCell(new Paragraph("Ribeirão Preto",fonteReduzida));
                _cell.HorizontalAlignment = Element.ALIGN_CENTER;
                _cell.Border = 0;
                tbFooter.AddCell(_cell);

                _cell = new PdfPCell(new Paragraph("Pág." + writer.PageNumber,fonteReduzida));
                _cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                _cell.Border = 0;
                tbFooter.AddCell(_cell);
                tbFooter.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetBottom(document.BottomMargin) + 10F, writer.DirectContent);
            }


        }


        //public class PDFFooter : PdfPageEventHelper
        //{
        //    // write on top of document
        //    public override void OnOpenDocument(PdfWriter writer, Document document)
        //    {
        //        //base.OnOpenDocument(writer, document);
        //        PdfPTable tbHeader = new PdfPTable(3);
        //        tbHeader.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //        tbHeader.DefaultCell.Border = 0;
        //        tbHeader.AddCell(new Paragraph());
                
        //        PdfPCell _cell = new PdfPCell(new Paragraph("ARJ Empreendimentos"));
        //        _cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        tbHeader.AddCell(_cell);


        //        PdfPTable tabFot = new PdfPTable(new float[] { 1F });
        //        tabFot.SpacingAfter = 10F;
        //        PdfPCell cell;
        //        tabFot.TotalWidth = 300F;
        //        cell = new PdfPCell(new Phrase(""));
        //        cell.Border = Rectangle.NO_BORDER;
        //        tabFot.AddCell(cell);
        //        tabFot.WriteSelectedRows(0, -1, 150, document.Top, writer.DirectContent);
        //    }

        //    // write on start of each page
        //    public override void OnStartPage(PdfWriter writer, Document document)
        //    {
        //        base.OnStartPage(writer, document);

        //        var fonteNormal = new iTextSharp.text.Font(fontebase, 10, iTextSharp.text.Font.NORMAL);
        //        Paragraph ph = new Paragraph(new Chunk("- ARJ - \n", fonteNormal));

        //        //// adiciono a linha e posteriormente mais linhas que podem ser necessárias em um cabeçalho de relatório

        //        document.Add(ph);

        //        ph = new Paragraph("\n\n", fonteNormal);

        //        document.Add(ph);

        //        // cria um novo paragrafo para imprimir um traço e uma linha em branco

        //        //ph = new Paragraph();

        //        // cria um objeto sepatador (um traço)

        //        ////iTextSharp.text.pdf.draw.VerticalPositionMark seperator = new iTextSharp.text.pdf.draw.LineSeparator();

        //        //// adiciona o separador ao paragravo

        //        ////ph.Add(seperator);
        //        //// adiciona a linha em branco(enter) ao paragrafo
        //        //ph.Add(new Chunk("\n"));
        //        //// imprime o pagagrafo no documento
        //       // document.Add(ph);


        //        //base.OnStartPage(writer, document);
        //    }

        //    // write on end of each page
        //    public override void OnEndPage(PdfWriter writer, Document document)
        //    {
        //        var fonteReduzida = new iTextSharp.text.Font(fontebase, 8, iTextSharp.text.Font.NORMAL);

        //        DateTime horario = DateTime.Now;
        //        base.OnEndPage(writer, document);
        //        PdfPTable tabFot = new PdfPTable(new float[] { 0.1F });
        //        tabFot.TotalWidth = 300F;

        //        PdfPCell cell = new PdfPCell(new Phrase("Rua Américo Brasiliense, 1856 - Vila Seixas Pág. " + writer.PageNumber.ToString(), fonteReduzida));
        //        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //        cell.Border = Rectangle.NO_BORDER;
        //        tabFot.AddCell(cell);
        //        tabFot.WriteSelectedRows(0, -1, 150, document.Bottom + 3.5F, writer.DirectContent);
                

        //    }

        //    //write on close of document
        //    public override void OnCloseDocument(PdfWriter writer, Document document)
        //    {
        //        base.OnCloseDocument(writer, document);
        //    }
        //}
    


    public class Eventos:PdfPageEventHelper
        {
            // propriedade da fonte que será usada no cabeçalho

            public  Font fonte { get; set; }



            // a classe recebe a fonte no seu construtor a classe não possui construtor padrão, para obrigar

            // a passagem da fonte e evitar erros

            public Eventos(Font fonte_)

            {

                fonte = fonte_;

            }



            // Este método cria um cabeçalho para o documento

            public override void OnStartPage(PdfWriter writer, Document document)

            {

                // Cria um novo paragrafo com o texto do cabeçalho

                Paragraph ph = new Paragraph( new Chunk("\n", fonte));



                // adiciono a linha e posteriormente mais linhas que podem ser necessárias em um cabeçalho de relatório

                document.Add(ph);

                ph = new Paragraph("\n\n", fonte);

                document.Add(ph);

                // cria um novo paragrafo para imprimir um traço e uma linha em branco

                //ph = new Paragraph();

                // cria um objeto sepatador (um traço)

                //iTextSharp.text.pdf.draw.VerticalPositionMark seperator = new iTextSharp.text.pdf.draw.LineSeparator();



                // adiciona o separador ao paragravo

                //ph.Add(seperator);



                // adiciona a linha em branco(enter) ao paragrafo

                ph.Add(new Chunk("\n"));



                // imprime o pagagrafo no documento

                document.Add(ph);

            }

            public override void OnEndPage(PdfWriter writer, Document document)

            {
                // para o rodapé é um pouco diferente precisamos criar um PdfContentByte e uma BaseFont e

                // setar as propriedades dos mesmos para então poder imprimir alinhado a direita

                // cria uma instancia da classe PdfContentByte

                PdfContentByte cb = writer.DirectContent;


                // cria uma instancia da classe font

                BaseFont font;


                // seta as propriedades da fonte

                font = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);


                // seta a fonte do objeto PdfContentByte
                
                cb.SetFontAndSize(font, 9);

                // escreve a linha para imprimir o numero da página

                string texto = "Página: " + writer.PageNumber.ToString();



                // imprime a linha no rodapé

                cb.ShowTextAligned(Element.ALIGN_RIGHT, texto, document.Right, document.Bottom - 20, 0);

            }
        }

       
        //
        // 20-10-2022 - impressão do contrato 
        //
        public ActionResult ImprimirContrato(int? id)
        {

            // Variáveis do contrato

            // [quadra] e [lote]
            // [descritivo]   --> memorial descritivo do lote
            // [valorTotal]   --> valor total em decimal formatado + valor por extenso
            // [valorTotalCorrigido]  --> valor total corrigido em decimal + valor por extenso
            // [totalMeses]  --> total em meses escolhido no plano de pagamento
            // [valorCorretagem]   --> valor da corretagem em decimal + por extenso.
            // [Corretor] - nome do corretor
            // [Cresci] - número do registro do Cresci
            // [CpfCorretor] - cpf do corretor
            // [valorCorretagemDec] - valor da Corretagem apenas em decimal
            // [DataPgCorretagem] - data para pagemento da corretagem

            // [ValorEntrada] - Valor de entrada em decimal + extenso
            // [saldoParcelar] - valor total - entrada
            // [numeroBoleto] - número do boleto emitido para pagamento da entrada
            // [numeroProposta] - número ID da proposta emitida para a quadra/lote
            // [valorParcelaMensal] - valor da parcela mensal escolhida em decimal + extenso
            // [planoPagamento] - número de parcelas das prestações mensais
            // [primeiroVencimento] - primeiro data de vencimento da parcela mensal
            // [numeroPrestacoesSemestral] - numero de parcelas semestrais
            // [valorParcelaSemestral] - valor decimal + extenso da parcela semestral
            // [primeiroVencSemestral] - data do primeiro vencimento das parcelas semestrais
            // [saldoRemanescente] - valor decimal + extenso do saldo remanescente
            // [totalMeses] - total de meses das parcelas mensais 

            // [bancoCli] - banco do cliente para arrependimento
            // [agenciaCli] - agencia cliente  "      "
            // [contaCli] - conta do cliente   "      "

            // [nomeTestemunha1]
            // [endTestemunha1]
            // [rgTestemunha1]

            // [nomeTestemunha2]
            // [endTestemunha2]
            // [rgTestemunha2]



            try
            {
                var proposta = (from x in db.Propostas
                                join a in db.Lotes on new { Quadra = x.Quadra, Lote = x.Lote } equals new { Quadra = a.Quadra, Lote = a.Lote }
                                where x.Id == id
                                select new
                                {
                                    Id = id,
                                    Quadra = a.Quadra,
                                    Lote = a.Lote,
                                    ValorTotal = x.ValorTotal,
                                    Contrato = x.Contrato ?? "",
                                    BancoCliente = x.BancoCliente,
                                    AgenciaCliente = x.AgenciaCliente,
                                    ContaCliente = x.ContaCliente,
                                    DataProposta = x.DataProposta,
                                    PrimeiroVencMensal = x.PrimeiroVencMensal,
                                    PrimeiroVencSemestral = x.PrimeiroVencSemestral,
                                    Area = a.Area,
                                    ValorCorretagem = x.ValorCorretagem,
                                    CorretorNome = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Nome ?? "",
                                    CorretorCpf = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Cpf ?? "",
                                    CorretorCresci = db.Corretores.Where(c => c.Id == x.CorretorId).FirstOrDefault().Cresci ?? ""
                                }).FirstOrDefault();

                var condicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == id).FirstOrDefault();
                var compradores = (from x in db.PropostasCompradores
                                   join c in db.Comprador on x.CompradorId equals c.Id
                                   where x.Id == id
                                   where c.DataExclusao == null
                                   where x.DataExclusao == null
                                   select c).OrderBy(c => c.Nome).ToList();
                var descritivo = db.LotesDescritivos.Where(c => c.Quadra == proposta.Quadra && c.Lote == proposta.Lote).FirstOrDefault();

                var dadosCompradores = "";
                foreach (var item in compradores)
                {

                    dadosCompradores = "Nome Completo: " + item.Nome.TrimEnd() + "\n";
                    dadosCompradores += "Nacionalidade: " + item.Nacionalidade.TrimEnd() + "\n";
                    dadosCompradores += "Profissão: " + item.Profissao.TrimEnd() + "\n";
                    dadosCompradores += "RG: " + item.Rg.TrimEnd() + "\n";
                    dadosCompradores += "CPF: " + Convert.ToUInt64(item.Cpf.TrimEnd()).ToString(@"\000\.000\.000\-00") + "\n";
                    dadosCompradores += "Endereço: " + item.Logradouro.TrimEnd() + " " + item.Numero.TrimEnd() + " " + item.Complemento.TrimEnd() + " " + item.Bairro.TrimEnd() + " " + item.Municipio.TrimEnd() + " " + item.Estado + " " + item.Cep + "\n";
                    dadosCompradores += "E-mail: " + item.Email.TrimEnd() + "\n";
                    switch (item.EstadoCivil)
                    {
                        case "1":
                            dadosCompradores += "Estado Civil: Solteiro\n";
                            break;
                        case "2":
                            dadosCompradores += "Estado Civil: Casado(a)\n";
                            break;
                        case "3":
                            dadosCompradores += "Estado Civil: Solteiro\n";
                            break;
                        case "4":
                            dadosCompradores += "Estado Civil: Divorciado(a)";
                            break;
                        case "5":
                            dadosCompradores += "Estado Civil: Viúvo(a)";
                            break;
                        default:
                            dadosCompradores += "Estado Civil: Solteiro(a)";
                            break;
                    }

                    // Dados do comprador
                    if (item.EstadoCivil == "2")
                    {
                        // dados do casamento - se for casado
                        dadosCompradores += "Regime Casamento: " + item.CasamentoRegime.TrimEnd() + "\n";
                        dadosCompradores += "Data Casamento: " + item.CasamentoData.Value.ToShortDateString() + "\n";
                        if (item.CasamentoEscrRegistro != null)
                        {
                            dadosCompradores += "Escritura do Pacto Antenupicial - " + "Tabelião: " + item.CasamentoEscrTabeliao ?? "" + "Livro: " + item.CasamentoLivro.TrimEnd() ?? "" + " Fls. " + item.CasamentoFolhas.TrimEnd() ?? "" + "\n";
                            dadosCompradores += "Registro de Imóveis: " + item.CasamentoEscrRegistro.TrimEnd() + "\n";

                        }

                        dadosCompradores += "\nDados do Cônjuge \n\n";
                        // dados do cônjuge - se for casado
                        dadosCompradores += "Nome: " + item.ConjugeNome.TrimEnd() + "\n";
                        dadosCompradores += "Celular: " + item.ConjugeCelular + "\n"; ;
                        dadosCompradores += "Nacionalidade: " + item.ConjugeNacionalidade.TrimEnd() + "\n";
                        dadosCompradores += "Profissão: " + item.ConjugeProfissao.TrimEnd() + "\n";
                        dadosCompradores += "RG: " + item.ConjugeRg.TrimEnd() + "\n";
                        dadosCompradores += "CPF: " + Convert.ToUInt64(item.ConjugeCpf.TrimEnd()).ToString(@"\000\.000\.000\-00") + "\n";
                        //  dadosCompradores += "Endereço: " + item.ConjugeLogradouro.TrimEnd()??"" + " " + item.ConjugeNumero.TrimEnd()??"" + " " + item.ConjugeBairro.TrimEnd()??"" + " " + " " + item.ConjugeMunicipio.TrimEnd() + " " + item.ConjugeEstado + " " + "\n";
                        if (item.ConjugeEmail != null)
                        {
                            dadosCompradores += "E-mail: " + item.ConjugeEmail.TrimEnd() + "\n";
                        }

                    }
                    dadosCompradores += "\n\n\n";

                }

                var guid = Guid.NewGuid();
                var path = Path.Combine(_hostingEnvironment.WebRootPath, "Documentos") + "\\Modelo1.docx";
                var path2 = Path.Combine(_hostingEnvironment.WebRootPath, "Documentos") + "\\" + guid.ToString() + ".html";
                var footer = Path.Combine(_hostingEnvironment.WebRootPath, "Documentos") + "\\footer.html";


                byte[] byteArray = System.IO.File.ReadAllBytes(path);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument doc =
                        WordprocessingDocument.Open(memoryStream, true))
                    {
                        HtmlConverterSettings settings = new HtmlConverterSettings()
                        {
                            PageTitle = "",
                            AdditionalCss = "span {font-size: 15px!important;line-height: 1.1;}"

                        };
                        XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                        System.IO.File.WriteAllText(path2, html.ToStringNewLineOnAttributes());
                    }
                    memoryStream.Flush(); //Always catches me out
                    memoryStream.Position = 0; //Not sure if this is required
                    memoryStream.Close();
                }

                string fullMonthName = DateTime.Now.ToString("MMMM", CultureInfo.CreateSpecificCulture("pt-BR"));

                var valorTotalExtenso = Utils.ValorExtenso.ExtensoReal(proposta.ValorTotal);
                var valorTotalCorrigidoExtenso = Utils.ValorExtenso.ExtensoReal(condicoes.PrecoVendaCorrigido);
                var valorCorretagem = proposta.ValorCorretagem; // Math.Round(proposta.ValorTotal * 0.04m, 2);
                var saldoRemascenteExtenso = Utils.ValorExtenso.ExtensoReal(condicoes.SaldoQuitacao);
                var saldoPagarExtenso = Utils.ValorExtenso.ExtensoReal((proposta.ValorTotal - condicoes.ValorEntrada));

                var openhtml = System.IO.File.ReadAllBytes(path2);
                var str = System.Text.Encoding.Default.GetString(openhtml);

                //var teste = "<p align=\"justify\"><br>Welcome to Geeks for Geeks. It is <br>a computer science portal for geeks.<br>It contains well written, well <br>thought articles. We are learning<br>how to justify content on<br>a web page.</p>";

                var quebrapagina = "<div class=\"pagebreak\"> </div>";

                var boletonumero = proposta.Quadra.TrimEnd() + proposta.Lote.ToString().PadLeft(5, '0');

                str = str.Replace("<style>", "<style> @media print { .pagebreak {clear: both; page-break-after: always;}}  ");
                str = str.Replace("[quebralinha]", "<div style='page-break-before: always'></div>");
                str = str.Replace("[pagebreak]", quebrapagina);
                str = str.Replace("[contrato]", proposta.Contrato ?? "S/N");
                str = str.Replace("[preco]", String.Format("{0:0,0.00}", proposta.ValorTotal));
                str = str.Replace("[preco_extenso]", valorTotalExtenso);
                str = str.Replace("[valorCorretagem]", String.Format("{0:0,0.00}", valorCorretagem));
                str = str.Replace("[valorCorretagemDec]", String.Format("{0:0,0.00}", valorCorretagem));
                str = str.Replace("[Corretor]", proposta.CorretorNome);
                str = str.Replace("[Cresci]", proposta.CorretorCresci);
                str = str.Replace("[CpfCorretor]", proposta.CorretorCpf);
                str = str.Replace("[DataPgCorretagem]", proposta.DataProposta.AddDays(13).ToShortDateString());
                str = str.Replace("[dataContrato]", proposta.DataProposta.ToShortDateString());
                str = str.Replace("[quadra]", proposta.Quadra);
                str = str.Replace("[dadoscompradores]", dadosCompradores);
                str = str.Replace("[descritivo]", descritivo.Descritivo.TrimEnd());
                str = str.Replace("[lote]", proposta.Lote.ToString());
                str = str.Replace("[área]", proposta.Area.ToString());
                str = str.Replace("[data_do_contrato]", proposta.DataProposta.ToShortDateString());
                str = str.Replace("[dia_impressao]", DateTime.Now.Day.ToString());
                str = str.Replace("[mes_impressao]", fullMonthName.ToString());
                str = str.Replace("[ano_impressao]", DateTime.Now.Year.ToString());
                str = str.Replace("[testemunha1]", "Paulo Henrique");
                str = str.Replace("[testemunha2]", "Larissa Souza");
                str = str.Replace("[endTestemunha1]", "Araraquara, SP");
                str = str.Replace("[endTestemunha2]", "Araraquara, SP");
                str = str.Replace("[rgTestemunha1]", "041222 SSP/SP");
                str = str.Replace("[rgTestemunha2]", "749909 SSP-BA");
                str = str.Replace("[dados_anexos]", "");
                str = str.Replace("[ValorEntrada]", String.Format("{0:0,0.00}", condicoes.ValorEntrada));
                str = str.Replace("[saldoParcelar]", String.Format("{0:0,0.00}", (proposta.ValorTotal - condicoes.ValorEntrada)));
                str = str.Replace("[saldoParcelarExtenso]", String.Format("{0:0,0.00}", saldoPagarExtenso));

                str = str.Replace("[valorTotalCorrigido]", String.Format("{0:0,0.00}", condicoes.PrecoVendaCorrigido));
                str = str.Replace("[valorTotalCorrigidoExtenso]", valorTotalCorrigidoExtenso);
                str = str.Replace("[totalMeses]", condicoes.NrParcelasMensais.ToString());
                str = str.Replace("[numeroProposta]", proposta.Id.ToString().PadLeft(6, '0'));
                str = str.Replace("[valorParcelaMensal]", String.Format("{0:0,0.00}", condicoes.ValorParcelaMensal));
                str = str.Replace("[planoPagamento]", condicoes.NrParcelasMensais.ToString());
                str = str.Replace("[primeiroVencMensal]", proposta.PrimeiroVencMensal.Value.ToShortDateString());
                str = str.Replace("[primeiroVencSemestral]", proposta.PrimeiroVencSemestral.Value.ToShortDateString());
                str = str.Replace("[numeroPrestacoesSemestral]", condicoes.NrParcelasSemestrais.ToString());
                str = str.Replace("[valorParcelaSemestral]", String.Format("{0:0,0.00}", condicoes.ValorParcelaSemestral));
                str = str.Replace("[saldoRemanescente]", String.Format("{0:0,0.00}", condicoes.SaldoQuitacao));
                str = str.Replace("[saldoRemanescenteExtenso]", saldoRemascenteExtenso);
                str = str.Replace("[bancoCli]", proposta.BancoCliente);
                str = str.Replace("[agenciaCli]", proposta.AgenciaCliente);
                str = str.Replace("[contaCli]", proposta.ContaCliente);
                str = str.Replace("[numeroBoleto]", boletonumero);

                StringReader sr = new StringReader(str.ToString());


                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.WebPageWidth = 800;
                converter.Options.MarginLeft = 45;   //40
                converter.Options.MarginRight = 30;
                converter.Options.MarginTop = 20;
                converter.Options.CssMediaType = HtmlToPdfCssMediaType.Print;

                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;


                converter.Options.DisplayHeader = true;
                converter.Header.Height = 70;
                converter.Options.DisplayFooter = true;
                converter.Footer.DisplayOnFirstPage = true;
                converter.Footer.DisplayOnOddPages = true;
                converter.Footer.DisplayOnEvenPages = true;
                converter.Footer.Height = 70;

                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                converter.Options.DisplayFooter = true;
                converter.Footer.DisplayOnFirstPage = true;
                converter.Footer.DisplayOnOddPages = true;
                converter.Footer.DisplayOnEvenPages = true;
                converter.Footer.Height = 70;

                PdfHtmlSection footerHtml = new PdfHtmlSection(footer);
                footerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Footer.Add(footerHtml);

                // add page numbering element to the footer

                // page numbers can be added using a PdfTextSection object
                //PdfTextSection text = new PdfTextSection(0, 10, "{page_number}  ", new System.Drawing.Font("Arial", 8));
                ////text.HorizontalAlign = PdfTextHorizontalAlign.Right;
                //text.HorizontalAlign = PdfTextHorizontalAlign.Justify;
                //converter.Footer.Add(text);

                SelectPdf.PdfDocument doc2 = converter.ConvertHtmlString(str);
                doc2.Save(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");
                doc2.Close();


                MemoryStream ms = new MemoryStream();

                byte[] bytes = System.IO.File.ReadAllBytes(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");

                ms.Write(bytes, 0, bytes.Length);


                ms.Flush(); //Always catches me out
                ms.Position = 0; //Not sure if this is required
                                 //System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");
                                 //System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".html");

                System.IO.File.Delete(path2);



                // este trecho comentado é para o caso de querer baixar o pdf
                //var nome_arquivo = "Contrato - Q " + proposta.Quadra + " L " + proposta.Lote.ToString() + ".pdf";
                //return File(ms, "application/pdf", nome_arquivo);
                return new FileStreamResult(ms, "application/pdf");
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
    //
    //  Classe de retorno para a 
    //
    public class PrecoRetonar
    {
        public string SaldoPagar { get; set; }
        public bool result { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal PrecoM2 { get; set; }
        public string CorFundo { get; set; }
        public string Entrada { get; set; }
        public string TipoPgtoPermitido { get; set; }
        public string TipoPagamento { get; set; }
        public string TipoParcelamento { get; set; }
        public string Parcelas { get; set; }

    }

}
