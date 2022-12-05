using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using ARJ.Pianopoli.Admin._6.Utils;
using DocumentFormat.OpenXml.Packaging;
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

            var retorno = (from x in lista select new ListaLotesViewModel()
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
                catch (Exception ex)
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

                    modelo.ValorCorretagem = Math.Round( precoVenda * 0.02m,2);

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
        public async Task<IActionResult> CalcularPrecos(string Quadra, string Lote, string Entrada)
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
                    obj.ValorCorretagem = Math.Round(precoVenda * 0.02m, 2);
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
        public async Task<IActionResult> BuscarCompradores(int id)
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
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://viacep.com.br/ws/" + param + "/json/");
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
        public async Task<IActionResult> SalvarComprador(CompradorViewModel model)
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
                    obj.Complemento = model.Complemento??"";
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
        public async Task<IActionResult> ListarCompradores(string propostaid)
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
                phrase.Add(new Chunk("NR. " + id.ToString().PadLeft(6,'0') + " \n\n", fonteParagrafo));
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


                cell = new PdfPCell(new Phrase("Quadra: " + obj.Quadra, fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("VALOR DO LOTE: R$ " + string.Format("{0:0,0.00}", condicoes.ValorTotal), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);



                cell = new PdfPCell(new Phrase("Lote: " + obj.Lote.ToString(), fonteParagrafo));
                cell.Colspan = 1;
                cell.BorderWidth = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Pagamento  A PRAZO - Entrada: R$ " + string.Format("{0:0,0.00}", condicoes.ValorEntrada), fonteParagrafo));
                cell.Colspan = 2;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Área: " + String.Format("{0:0,0.00}", objlote.Area) + " m2", fonteParagrafo));
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
                cell = new PdfPCell(new Phrase("VALOR REMANESCENTE: R$ " + String.Format("{0:0,0.00}", condicoes.SaldoQuitacao), fonteParagrafo));
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
                cell = new PdfPCell(new Phrase("Quantidade Prestações Semestrais: " + condicoes.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", condicoes.ValorParcelaSemestral), fonteParagrafo));
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


                    cell = new PdfPCell(new Phrase("Filiação: " + (item.NomeMae==null?"":item.NomeMae.TrimEnd()) + (item.NomePai==null?"":  " e " + item.NomePai) , fonteParagrafo));
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
                            estadocivil= "Casado(a)";
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
                        default:
                            estadocivil = "Solteiro(a)";
                            break;
                    }

                    cell = new PdfPCell(new Phrase("Estado Civil: " + (estadocivil==null?"": estadocivil), fonteParagrafo));
                    cell.Colspan = 1;
                    cell.BorderWidth = 0;
                    table.AddCell(cell);


                    if (item.EstadoCivil== "2")
                    {
                        cell = new PdfPCell(new Phrase("Data Casamento: " + (item.CasamentoData == null ? "" : item.CasamentoData.Value.ToShortDateString()), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("Regime Cas.: " + (item.CasamentoRegime??""), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);


                        // Casado
                        // -- linha
                        cell = new PdfPCell(new Phrase("\nNome Cônjuge: " + (item.ConjugeNome == null ? "": item.ConjugeNome.TrimEnd()), fonteParagrafo));
                        cell.Colspan = 3;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase("CPF Cônjuge: " + (item.ConjugeCpf==null?"": Convert.ToUInt64(item.ConjugeCpf).ToString(@"\000\.000\.000\-00")), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("RG Cônjuge: " + item.ConjugeRg??"", fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        cell = new PdfPCell(new Phrase("Dt Nasc Cônjuge: " + (item.ConjugeDtNasc == null ? "" : (item.ConjugeDtNasc == null ? "" : item.ConjugeDtNasc.Value.ToShortDateString())), fonteParagrafo));
                        cell.Colspan = 1;
                        cell.BorderWidth = 0;
                        table.AddCell(cell);
                        //---
                        cell = new PdfPCell(new Phrase("Prof. Cônjuge: " + (item.ConjugeProfissao == null ? " ": item.ConjugeProfissao), fonteParagrafo));
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

                cell = new PdfPCell(new Phrase("NÚMERO DO CRECI: " + "\n", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("\n", fonteParagrafo));
                cell.Colspan = 3;
                cell.BorderWidth = 0;
                table.AddCell(cell);


                var fonteArial7 = new iTextSharp.text.Font(FontFactory.GetFont("ARIAL", 7, iTextSharp.text.Font.NORMAL));
                cell = new PdfPCell(new Phrase("INFORMAÇÕES ESPECIAIS: IMPORTANTE - A PRESENTE PROPOSTA DE COMPRA CONSTITUI RESERVA DO BEM NELA DESCRITO, SOMENTE SENDO VÁLIDA APÓS A QUITAÇÃO DO VALOR REFERENTE A ENTRADA.A ENTRADA CORRESPONDE AO SINAL E PRINCÍPIO DE PAGAMENTO(ARRAS).NA FALTA DO PAGAMENTO DA ENTRADA ESTA PROPOSTA FICA AUTOMATICAMENTE CANCELADA E RESCINDIDA, GARANTINDO - SE AO VENDEDOR O DIREITO DE DISPOR DO LOTE NEGOCIADO.QUITADO O VALOR DA ENTRADA, O VENDEDOR ELABORARÁ O COMPROMISSO DE COMPRA E VENDA, INTIMANDO O COMPRADOR PARA ASSINATURA DO CONTRATO. \r\n ", fonteArial7));
                cell.Colspan = 3;
                cell.Border = 0;
                cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
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
                                    DataProposta = x.DataProposta,
                                    Area = a.Area
                                }).FirstOrDefault();

                var condicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == id).FirstOrDefault();
                var compradores = (from x in db.PropostasCompradores
                                   join c in db.Comprador on x.CompradorId equals c.Id
                                   where x.Id == id
                                   where c.DataExclusao == null
                                   where x.DataExclusao == null
                                   select c).OrderBy(c => c.Nome).ToList();

                foreach (var item in compradores)
                {

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

                var openhtml = System.IO.File.ReadAllBytes(path2);
                var str = System.Text.Encoding.Default.GetString(openhtml);
                str = str.Replace("[quebralinha]", "<div style='page-break-before: always'></div>");
                str = str.Replace("[contrato]", proposta.Contrato ?? "S/N");
                str = str.Replace("[preco]", proposta.ValorTotal.ToString());

                str = str.Replace("[quadra]", proposta.Quadra);
                str = str.Replace("[nomecomprador]", "");
                str = str.Replace("[lote]", proposta.Lote.ToString());
                str = str.Replace("[área]", proposta.Area.ToString());
                str = str.Replace("[data_do_contrato]", proposta.DataProposta.ToShortDateString());
                str = str.Replace("[dia_impressao]", DateTime.Now.Day.ToString());
                str = str.Replace("[mes_impressao]", fullMonthName.ToString());
                str = str.Replace("[ano_impressao]", DateTime.Now.Year.ToString());
                str = str.Replace("[testemunha1]", "Paulo Henrique");
                str = str.Replace("[testemunha2]", "Larissa Souza");
                str = str.Replace("[dados_anexos]", "");

                StringReader sr = new StringReader(str.ToString());


                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.WebPageWidth = 800;
                converter.Options.MarginLeft = 30;
                converter.Options.MarginRight = 30;
                converter.Options.MarginTop = 20;

                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;


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
                PdfTextSection text = new PdfTextSection(0, 10, "{page_number}  ", new System.Drawing.Font("Arial", 8));
                text.HorizontalAlign = PdfTextHorizontalAlign.Right;
                converter.Footer.Add(text);

                SelectPdf.PdfDocument doc2 = converter.ConvertHtmlString(str);
                doc2.Save(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");
                doc2.Close();


                MemoryStream ms = new MemoryStream();

                byte[] bytes = System.IO.File.ReadAllBytes(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");

                ms.Write(bytes, 0, bytes.Length);


                ms.Flush(); //Always catches me out
                ms.Position = 0; //Not sure if this is required
                System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".pdf");
                System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath, "doc") + "//" + guid.ToString() + ".html");


                var nome_arquivo = "Contrato-" + proposta.Quadra + proposta.Lote.ToString() + ".pdf";
                return File(ms, "application/pdf", nome_arquivo);

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
