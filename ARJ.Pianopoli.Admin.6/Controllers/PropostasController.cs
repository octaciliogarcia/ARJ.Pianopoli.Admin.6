using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using DocumentFormat.OpenXml.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using SelectPdf;
using System.Globalization;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

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


            foreach (var item in lista)
            {
                var obj = db.Propostas.Where(c => c.LoteamentoId == item.LoteamentoId && c.Quadra == item.Quadra && c.Lote == item.Lote).FirstOrDefault();
                if (obj != null)
                    item.SituacaoNoSite = obj.Contrato == null ? "Reservado" : "Vendido";
                else
                    item.SituacaoNoSite = "Disponível";
            }

            return Json(new { data = lista });
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
                    var precoVenda = db.TabelaPrecoLotes.Where(c => c.Quadra == Quadra && c.Lote == Lote).FirstOrDefault().PrecoVenda;

                    modelo.ValorTotal = (precoVenda);

                    ViewBag.Mensais = (from f in db.TabelaPrecoLotes
                                       where f.Quadra == Quadra && f.Lote == Lote
                                       select new SelectListItem
                                       {
                                           Text = " " + f.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasMensais) + " +   " + f.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasSemestrais) + "   ",
                                           Value = f.Id.ToString()
                                       });
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
                                           select c).ToList();


                        var statusnosite = "";
                        if (proposta.Status == 2)
                            statusnosite = "Reservado";
                        else
                            statusnosite = "Vendido";


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
                            SaldoPagar = String.Format("{0:0,0.00}", (propostaCondicoes.ValorTotal - propostaCondicoes.ValorEntrada)),
                            TipoPagamento = propostaCondicoes.ValorTotal == propostaCondicoes.ValorEntrada ? "a vista" : "a prazo",
                            Parcelamento = propostaCondicoes.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaMensal) + " + " + propostaCondicoes.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", propostaCondicoes.ValorParcelaSemestral),
                            Compradores = compradores
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
                var valoresLote = db.TabelaPrecoLotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                if (entrada < valoresLote.Entrada)
                {
                    return Json(new
                    {
                        result = false,
                        message = "Valor da entrada não permitido!"
                    });

                }

                var retorno = new PrecoRetonar();
                var saldo = valoresLote.PrecoVenda - entrada;

                if (saldo > 0)
                    retorno.TipoPgtoPermitido = "0";
                else
                    retorno.TipoPgtoPermitido = "1";

                retorno.SaldoPagar = String.Format("{0:0,0.00}", saldo);
                retorno.Entrada = String.Format("{0:0,0.00}", entrada);
                retorno.PrecoM2 = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().ValorM2;
                retorno.CorFundo = db.TabelaM2.Where(c => c.CategoriaId == dadosLote.CategoriaId).FirstOrDefault().CorFundo ?? "#ffffff";


                // Calcular a condição de parcelamento baseado no valor de entrada



                if (saldo > 0)
                {

                    var condicoespagto = (from f in db.TabelaPrecoLotes
                                          where f.Quadra == Quadra && f.Lote == lote
                                          select new SelectListItem
                                          {
                                              Text = " " + f.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasMensais) + " +   " + f.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasSemestrais) + "   ",
                                              Value = f.Id.ToString()
                                          });

                    ViewBag.Mensais = condicoespagto;

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
        public IActionResult Compradores(int Loteamento, string Quadra, int Lote, string Entrada, string TipoPagamento, string Parcelamento)
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

                    // verifica todos os valores novamente, para evitar "injeções" no jQuery

                    var stringEntrada = Entrada.Replace(".", "").Replace(",", ".");
                    var entrada = decimal.Parse(stringEntrada) / 100;

                    //verifica se valor da entrada é compatível e permitida
                    var valoresLote = db.TabelaPrecoLotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();

                    if (entrada < valoresLote.Entrada)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada não permitido!"
                        });
                    }

                    if (entrada > valoresLote.PrecoVenda)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Valor da entrada está maior que o valor do Lote!"
                        });
                    }

                    var UserId = _user.GetUserId();

                    var parcelaId = int.Parse(Parcelamento);

                    var parcelas = (from f in db.TabelaPrecoLotes
                                    where f.Quadra == Quadra && f.Lote == Lote
                                    where f.Id == parcelaId
                                    select f).FirstOrDefault();


                    // se tudo estiver correto - grava a proposta com as condições comerciais e mostra grid para preencher com compradores

                    var obj = new Proposta();
                    obj.DataHora = DateTime.Now;
                    obj.DataProposta = DateTime.Today;
                    obj.Status = 2;
                    obj.Quadra = Quadra;
                    obj.Lote = Lote;
                    obj.LoteamentoId = Loteamento;
                    obj.Usuario = UserId.ToString();
                    obj.ValorTotal = valoresLote.PrecoVenda;
                    db.Add(obj);
                    db.SaveChanges();

                    var objcm = new PropostaCondicaoComercial();
                    // var listacm = new List<PropostaCondicaoComercial>();
                    objcm.PropostaId = obj.Id;
                    objcm.DataHora = DateTime.Now;
                    objcm.Usuario = UserId.ToString();
                    objcm.ValorTotal = valoresLote.PrecoVenda;
                    objcm.ValorEntrada = entrada;
                    objcm.NrParcelasMensais = entrada == valoresLote.PrecoVenda ? 0 : parcelas.NrParcelasMensais;
                    objcm.NrParcelasSemestrais = entrada == valoresLote.PrecoVenda ? 0 : parcelas.NrParcelasSemestrais;
                    objcm.ValorParcelaMensal = entrada == valoresLote.PrecoVenda ? 0 : parcelas.VrParcelasMensais; ;
                    objcm.ValorParcelaSemestral = entrada == valoresLote.PrecoVenda ? 0 : parcelas.VrParcelasSemestrais;
                    db.Add(objcm);
                    db.SaveChanges();

                    // altera a condição do lote para "reservado"
                    var objlote = db.Lotes.Where(c => c.Quadra == Quadra && c.Lote == lote).FirstOrDefault();
                    objlote.SituacaoNoSite = "2";
                    db.Entry(objlote).State = EntityState.Modified;
                    db.SaveChanges();

                    var retorno = new PropostaViewModel();

                    var saldo = valoresLote.PrecoVenda - entrada;

                    retorno.Id = obj.Id;
                    retorno.SaldoPagar = String.Format("{0:0,0.00}", saldo);
                    retorno.Entrada = String.Format("{0:0,0.00}", entrada);
                    retorno.Area = objlote.Area;
                    retorno.Lote = objlote.Lote;
                    retorno.Quadra = objlote.Quadra;
                    retorno.LoteamentoId = objlote.LoteamentoId;
                    retorno.Status = int.Parse(disponivel);
                    retorno.ValorTotal = valoresLote.PrecoVenda;
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

                    ViewBag.Mensais = (from f in db.TabelaPrecoLotes
                                       where f.Quadra == Quadra && f.Lote == Lote
                                       select new SelectListItem
                                       {
                                           Text = " " + f.NrParcelasMensais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasMensais) + " +   " + f.NrParcelasSemestrais.ToString() + " x R$ " + String.Format("{0:0,0.00}", f.VrParcelasSemestrais) + "   ",
                                           Value = f.Id.ToString()
                                       });

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

                    return Json(new { endereco = (!string.IsNullOrEmpty(buscacep.logradouro) ? buscacep.logradouro : ""), cidade = (!string.IsNullOrEmpty(buscacep.localidade) ? buscacep.localidade + "," : ""), bairro = (!string.IsNullOrEmpty(buscacep.bairro) ? buscacep.bairro : ""), uf = (!string.IsNullOrEmpty(buscacep.uf) ? buscacep.uf : "") });
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
                    obj.Complemento = model.Complemento;
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
                             CpfComprador = y.Cpf,
                             CelularComprador = y.Celular
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
                var compradores = db.PropostasCompradores.Where(c => c.PropostaId == PropostaId).ToList();
                var condicoes = db.PropostasCondicoesComerciais.Where(c => c.PropostaId == PropostaId).FirstOrDefault();
                var pxPormm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4, 15 * pxPormm, 15 * pxPormm, 2 * pxPormm, 0 * pxPormm);
                var writer = PdfWriter.GetInstance(pdf, memoryStream);
                pdf.Open();

                var fonteParagrafo = new iTextSharp.text.Font(fontebase, 12, iTextSharp.text.Font.NORMAL);

                var titulo1 = new Paragraph("LOTEAMENTO RESIDENCIAL PIANOPOLI\n\n", fonteParagrafo);
                var titulo2 = new Paragraph("CONTRATO DE COMPRA E VENDA\n\n", fonteParagrafo);
                var titulo3 = new Paragraph("COM ALIENAÇÃO FIDUCIÁRIA EM GARANTIA\n\n", fonteParagrafo);
                var titulo4 = new Paragraph("E COM CONDIÇÕES SUSPENSIVAS\n\n", fonteParagrafo);

                titulo1.Alignment = Element.ALIGN_CENTER;
                titulo2.Alignment = Element.ALIGN_CENTER;
                titulo3.Alignment = Element.ALIGN_CENTER;
                titulo4.Alignment = Element.ALIGN_CENTER;
                pdf.Add(titulo1);


                PdfPTable mtable = new PdfPTable(1);
                mtable.WidthPercentage = 100;
                mtable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                PdfPTable table = new PdfPTable(2);
                PdfPCell cell = new PdfPCell(new Phrase(""));
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;

                table.AddCell("Quadra:  " + obj.Quadra);
                table.AddCell("Pagamento  A VISTA: R$ " + string.Format("{0:N2}", obj.ValorTotal));

                table.AddCell("Lote:  " + obj.Lote);
                table.AddCell("Data Vencimento:  "); // + condicoes.VencimentoInicial.Value.ToShortDateString());

                table.AddCell("Área:  XXX m2"); // + obj.Area + "m2");
                table.AddCell(" ");

                table.AddCell(" ");

                mtable.AddCell(table);

                table = new PdfPTable(3);

                //table.DefaultCell.Border = 0;
                var idcomprador = 1;

                var sql = (from f in db.PropostasCompradores
                           join c in db.PropostasCompradores on f.CompradorId equals c.Id
                           join d in db.Comprador on c.CompradorId equals d.Id
                           where f.PropostaId == PropostaId
                           where (f.DataExclusao == null)
                           select d).ToList();

                foreach (var item in sql)
                {
                    cell = new PdfPCell(new Phrase("Comprador " + idcomprador + ": " + item.Nome.ToUpper()));
                    cell.Colspan = 3;
                    //cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);
                    table.AddCell("CPF: " + item.Cpf);
                    table.AddCell("RG: " + item.Rg);
                    //table.AddCell("Exp:" + item.ExpRg.ToUpper());

                    table.AddCell("Data Nascimento: " + item.DtNasc.ToShortDateString());
                    cell.Colspan = 2;
                    //cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);
                    table.AddCell("Profissão: " + item.Profissao);
                    cell = new PdfPCell(new Phrase("Nacionalidade: " + item.Nacionalidade.ToUpper()));
                    cell.Colspan = 2;
                    //cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase("Email: " + item.Email.ToUpper()));
                    cell.Colspan = 2;
                    //cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);
                    table.AddCell("Telefone: ");

                    cell = new PdfPCell(new Phrase("Endereço Resid: " + item.Logradouro));
                    cell.Colspan = 3;
                    //cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);


                    table.AddCell("Cidade:" + item.Municipio);
                    table.AddCell("Bairro:" + item.Bairro);
                    table.AddCell("Cep:" + item.Cep);



                    //table.AddCell("Estado Civil:" + (item.EstadoCivil != null ? db.EstadoCivil.Where(c => c.Id == item.EstadoCivil).FirstOrDefault().Descricao : " "));
                    //table.AddCell("Data Casamento:" + (item .CasamentoData != null ? item.DtCasamento.Value.ToShortDateString() : ""));
                    //table.AddCell("Regime Casamento:" + (item.RegimeCasamento > 0 ? item.RegimeCasamento == 1 ? "Comunhão parcial de bens" : item.RegimeCasamento == 2 ? "Comunhão total de bens" : item.RegimeCasamento == 3 ? "Separação total de bens" : "" : ""));
                    //cell = new PdfPCell(new Phrase("Nome Cônjuge: " + item.NomeConjuge));
                    //cell.Colspan = 3;
                    ////cell.Border = 0;
                    //cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    //table.AddCell(cell);


                    //table.AddCell("CPF Cônjuge:" + item.CpfConjuge);
                    //table.AddCell("RG Cônjuge:" + item.RgConjuge);
                    //table.AddCell("Dt Nasc:" + (item.DtNascConjuge != null ? item.DtNascConjuge.Value.ToShortDateString() : ""));

                    //table.AddCell("Profissão Cônjuge:" + item.ProfissaoConjuge);
                    //cell = new PdfPCell(new Phrase("Nacionalidade: " + item.NacionalidadeConjuge));
                    //cell.Colspan = 2;
                    ////cell.Border = 0;
                    //cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    //table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(""));
                    cell.Colspan = 3;
                    cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(""));
                    cell.Colspan = 3;
                    cell.Border = 0;
                    //cell.FixedHeight = 20f;
                    cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                    table.AddCell(cell);

                    idcomprador++;
                }
                mtable.AddCell(table);

                var hoje = DateTime.Now;
                table = new PdfPTable(1);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = 0;
                table.AddCell("NOME DO CORRETOR / IMOBILIÁRIA: TESTE "); // + db.Corretores.Where(c => c.Id == obj. .Corretor).FirstOrDefault().Nome);
                table.AddCell("NUMERO DO CRECI: 00000 "); //+ db.Corretores.Where(c => c.Id == obj.Corretor).FirstOrDefault().Creci + "\r\n \r\n");
                //table.AddCell("INFORMAÇÕES ESPECIAIS: IMPORTANTE - A PRESENTE PROPOSTA DE COMPRA CONSTITUI RESERVA DO BEM NELA DESCRITO, SOMENTE SENDO VÁLIDA APÓS A QUITAÇÃO DO VALOR REFERENTE A ENTRADA.A ENTRADA CORRESPONDE AO SINAL E PRINCÍPIO DE PAGAMENTO(ARRAS).NA FALTA DO PAGAMENTO DA ENTRADA ESTA PROPOSTA FICA AUTOMATICAMENTE CANCELADA E RESCINDIDA, GARANTINDO - SE AO VENDEDOR O DIREITO DE DISPOR DO LOTE NEGOCIADO.QUITADO O VALOR DA ENTRADA, O VENDEDOR ELABORARÁ O COMPROMISSO DE COMPRA E VENDA, INTIMANDO O COMPRADOR PARA ASSINATURA DO CONTRATO.");
                var fonte = new iTextSharp.text.Font(fontebase, 7);
                cell = new PdfPCell(new Phrase("INFORMAÇÕES ESPECIAIS: IMPORTANTE - A PRESENTE PROPOSTA DE COMPRA CONSTITUI RESERVA DO BEM NELA DESCRITO, SOMENTE SENDO VÁLIDA APÓS A QUITAÇÃO DO VALOR REFERENTE A ENTRADA.A ENTRADA CORRESPONDE AO SINAL E PRINCÍPIO DE PAGAMENTO(ARRAS).NA FALTA DO PAGAMENTO DA ENTRADA ESTA PROPOSTA FICA AUTOMATICAMENTE CANCELADA E RESCINDIDA, GARANTINDO - SE AO VENDEDOR O DIREITO DE DISPOR DO LOTE NEGOCIADO.QUITADO O VALOR DA ENTRADA, O VENDEDOR ELABORARÁ O COMPROMISSO DE COMPRA E VENDA, INTIMANDO O COMPRADOR PARA ASSINATURA DO CONTRATO. \r\n ", fonte));
                cell.Colspan = 3;
                cell.Border = 0;
                cell.HorizontalAlignment = PdfCell.ALIGN_LEFT;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("Araraquara-SP, " + hoje.Day + " de " + DateTime.Now.ToString("MMMM", new CultureInfo("pt-BR")) + " de " + hoje.Year + "\r\n \r\n"));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.HorizontalAlignment = PdfCell.ALIGN_RIGHT;
                table.AddCell(cell);
                table.AddCell("Documentos Necessários: \r\n - CPF  \r\n  - RG \r\n  - Comprovante Residência(água ou luz) \r\n - Certidão estado civil(solteiro, casado ou divorciado) \r\n \r\n \r\n");


                var id2 = 1;
                foreach (var item in sql)
                {
                    cell = new PdfPCell(new Phrase("___________________________________  \r\n Comprador" + id2 + " \r\n"));
                    cell.Colspan = 1;
                    cell.Border = 0;
                    cell.HorizontalAlignment = PdfCell.ALIGN_CENTER;
                    table.AddCell(cell);
                    id2++;
                }
                cell = new PdfPCell(new Phrase("___________________________________  \r\n Corretor"));
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
                ms.Position = 0;

                return File(fileStream: ms, contentType: "application/pdf", fileDownloadName: "test_file_name" + ".pdf");

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
            catch (Exception ex)
            {

                throw ex;
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

    }

}
