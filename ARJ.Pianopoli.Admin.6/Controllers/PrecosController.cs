using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenXmlPowerTools;
using System.Drawing;
using System.Globalization;
using System.Security.Claims;

namespace ARJ.Pianopoli.Admin._6.Controllers
{
    public class PrecosController : Controller
    {
        private readonly DBContext db;
        static BaseFont fontebase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IAspNetUser _user;
        private readonly UserManager<IdentityUser> _userManager;
        public PrecosController(DBContext db, IWebHostEnvironment hostingEnvironment, IAspNetUser user, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            _hostingEnvironment = hostingEnvironment;
            _user = user;
            _userManager = userManager;

        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult ListarPrecos()
        {

            var lista = (from x in db.TabelaM2
                         join c in db.LoteCategorias on x.CategoriaId equals c.Id
                         select new PrecosVM
                         {
                             Id = x.Id,
                             CategoriaId = x.CategoriaId,
                             Categoria = c.Nome,
                             Preco = String.Format("{0:0,0.00}", x.ValorM2),
                             Cor = x.CorFundo,
                             Inicio = x.Inicio.Value.ToShortDateString(),
                             Usuario = _userManager.FindByIdAsync(x.Usuario).Result.UserName,
                             DataHora = x.DataHora.ToShortDateString()
                         }).ToList();

            return Json(new { data = lista });
        }

        public IActionResult EditarPrecos()
        {

            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult EditarPrecos(int Id)
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
                    var obj = new PrecoVM();
                    return PartialView("Imobiliaria", obj);
                }
                else
                {
                    var obj = (from f in db.TabelaM2
                               where f.Id == Id
                               select new PrecoVM()
                               {
                                   Id = f.Id,
                                   Inicio = f.Inicio.Value.ToShortDateString(),
                                   CategoriaId = f.CategoriaId,
                                   Categoria = db.LoteCategorias.Where(c => c.Id == f.CategoriaId).FirstOrDefault().Nome,
                                   Preco = String.Format("{0:0,0.00}", f.ValorM2),
                                   Cor = f.CorFundo
                               }).FirstOrDefault();

                    return PartialView("Precos", obj);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult SalvarPrecos(PrecoVM model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = new TabelaM2();
                    obj.ValorM2 = decimal.Parse(model.Preco);
                    obj.DataHora = DateTime.Now;
                    obj.Usuario = userId.ToString();
                    obj.Inicio = DateTime.Parse(model.Inicio);
                    obj.Descricao = model.Categoria.TrimEnd();
                    obj.CategoriaId = model.CategoriaId;
                    obj.CorFundo = model.Cor;
                    if (model.Id > 0)
                    {
                        obj.Id = (int)model.Id;
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.TabelaM2.Add(obj);
                        db.SaveChanges();
                    }

                    return Json(new
                    {
                        id = obj.Id,
                        result = true,
                        message = "dados cadastrados com sucesso!"
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

        }

        [Authorize(Roles = "Perfil-Admin")]
        public IActionResult PlanilhaPrecos()
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            ExcelPackage pck = new ExcelPackage();


            #region Planilha Base 

            var ws = pck.Workbook.Worksheets.Add("Base");

            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            ws.Cells["A2:C94"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
            ws.Cells["B95:C95"].Style.Font.Bold = true;
            ws.Cells["B95:C95"].Merge = true;
            ws.Cells["B95"].Value = "T O T A L";

            ws.Column(2).Width = 16;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 16;
            ws.Column(9).Width = 16;
            ws.Column(10).Width = 16;


            ws.Cells["L1:N1"].Merge = true;
            using (ExcelRange r = ws.Cells["L1:N1"])
            {
                r.Style.Font.Bold = true;
                r.Style.Font.SetFromFont("Calibri", 11, true, false, false, false);
                r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(146, 208, 80));
            }
            ws.Cells["L1"].Value = "A - VALOR DO M2=";

            ws.Cells["L2:N2"].Merge = true;
            using (ExcelRange r = ws.Cells["L2:N2"])
            {
                r.Style.Font.Bold = true;
                r.Style.Font.SetFromFont("Calibri", 11, true, false, false, false);
                r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 0));
            }
            ws.Cells["L2"].Value = "B - VALOR DO M2=";
            ws.Cells["L3:N3"].Merge = true;
            ws.Cells["L3"].Value = "C - VALOR DO M2=";
            using (ExcelRange r = ws.Cells["L3:N3"])
            {
                r.Style.Font.Bold = true;
                r.Style.Font.SetFromFont("Calibri", 11, true, false, false, false);
                r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 255));
            }
            ws.Cells["L4:N4"].Merge = true;
            ws.Cells["L4"].Value = "D - VALOR DO M2=";
            using (ExcelRange r = ws.Cells["L4:N4"])
            {
                r.Style.Font.Bold = true;
                r.Style.Font.SetFromFont("Calibri", 11, true, false, false, false);
                r.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                r.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 176, 240));
            }
            ws.Cells["O1:O4"].Style.Numberformat.Format = "#,##0.00";

            // busca os preços na base de dados
            //
            var precos = db.TabelaM2.ToList();
            var o1 = precos.Where(c => c.CategoriaId == 1).FirstOrDefault().ValorM2;
            var o2 = precos.Where(c => c.CategoriaId == 2).FirstOrDefault().ValorM2;
            var o3 = precos.Where(c => c.CategoriaId == 3).FirstOrDefault().ValorM2;
            var o4 = precos.Where(c => c.CategoriaId == 4).FirstOrDefault().ValorM2;

            ws.Cells["O1"].Value = o1;
            ws.Cells["O2"].Value = o2;
            ws.Cells["O3"].Value = o3;
            ws.Cells["O4"].Value = o4;

            // Metragens dos lotes
            ws.Cells["C3"].Value = 500.05;
            ws.Cells["C4"].Value = 500.06;
            ws.Cells["C5"].Value = 500.10;
            ws.Cells["C6"].Value = 500.12;
            ws.Cells["C7"].Value = 500.18;
            ws.Cells["C8"].Value = 500.22;
            ws.Cells["C9"].Value = 500.26;
            ws.Cells["C10"].Value = 500.30;
            ws.Cells["C11"].Value = 500.33;
            ws.Cells["C12"].Value = 500.35;
            ws.Cells["C13"].Value = 500.36;
            ws.Cells["C14"].Value = 500.37;
            ws.Cells["C15"].Value = 500.38;
            ws.Cells["C16"].Value = 500.39;
            ws.Cells["C17"].Value = 500.40;
            ws.Cells["C18"].Value = 500.44;
            ws.Cells["C19"].Value = 500.46;
            ws.Cells["C20"].Value = 500.51;
            ws.Cells["C21"].Value = 500.70;
            ws.Cells["C22"].Value = 500.87;
            ws.Cells["C23"].Value = 500.90;
            ws.Cells["C24"].Value = 500.96;
            ws.Cells["C25"].Value = 501.90;
            ws.Cells["C26"].Value = 502.11;
            ws.Cells["C27"].Value = 503.49;
            ws.Cells["C28"].Value = 504.83;
            ws.Cells["C29"].Value = 507.56;
            ws.Cells["C30"].Value = 507.62;
            ws.Cells["C31"].Value = 507.73;
            ws.Cells["C32"].Value = 508.10;
            ws.Cells["C33"].Value = 508.41;
            ws.Cells["C34"].Value = 508.52;
            ws.Cells["C35"].Value = 510.00;
            ws.Cells["C36"].Value = 510.01;
            ws.Cells["C37"].Value = 511.29;
            ws.Cells["C38"].Value = 511.34;
            ws.Cells["C39"].Value = 511.84;
            ws.Cells["C40"].Value = 512.44;
            ws.Cells["C41"].Value = 512.51;
            ws.Cells["C42"].Value = 512.55;
            ws.Cells["C43"].Value = 512.65;
            ws.Cells["C44"].Value = 514.07;
            ws.Cells["C45"].Value = 515.04;
            ws.Cells["C46"].Value = 515.53;
            ws.Cells["C47"].Value = 516.36;
            ws.Cells["C48"].Value = 523.89;
            ws.Cells["C49"].Value = 524.78;
            ws.Cells["C50"].Value = 525.00;
            ws.Cells["C51"].Value = 527.32;
            ws.Cells["C52"].Value = 527.62;
            ws.Cells["C53"].Value = 528.76;
            ws.Cells["C54"].Value = 531.27;
            ws.Cells["C55"].Value = 541.51;
            ws.Cells["C56"].Value = 558.17;
            ws.Cells["C57"].Value = 565.21;
            ws.Cells["C58"].Value = 565.44;
            ws.Cells["C59"].Value = 567.39;
            ws.Cells["C60"].Value = 568.96;
            ws.Cells["C61"].Value = 570.89;
            ws.Cells["C62"].Value = 571.83;
            ws.Cells["C63"].Value = 572.01;
            ws.Cells["C64"].Value = 574.35;
            ws.Cells["C65"].Value = 574.83;
            ws.Cells["C66"].Value = 575.52;
            ws.Cells["C67"].Value = 580.60;
            ws.Cells["C68"].Value = 581.95;
            ws.Cells["C69"].Value = 585.05;
            ws.Cells["C70"].Value = 585.55;
            ws.Cells["C71"].Value = 585.73;
            ws.Cells["C72"].Value = 587.75;
            ws.Cells["C73"].Value = 587.97;
            ws.Cells["C74"].Value = 591.49;
            ws.Cells["C75"].Value = 593.37;
            ws.Cells["C76"].Value = 604.44;
            ws.Cells["C77"].Value = 607.56;
            ws.Cells["C78"].Value = 620.61;
            ws.Cells["C79"].Value = 627.82;
            ws.Cells["C80"].Value = 630.94;
            ws.Cells["C81"].Value = 646.93;
            ws.Cells["C82"].Value = 5689.14;
            ws.Cells["C83"].Value = 500.42;
            ws.Cells["C84"].Value = 524.84;
            ws.Cells["C85"].Value = 555.26;
            ws.Cells["C86"].Value = 512.52;
            ws.Cells["C87"].Value = 527.34;
            ws.Cells["C88"].Value = 542.75;
            ws.Cells["C89"].Value = 554.24;
            ws.Cells["C90"].Value = 565.73;
            ws.Cells["C91"].Value = 525.95;
            ws.Cells["C92"].Value = 506.72;
            ws.Cells["C93"].Value = 503.58;
            ws.Cells["C94"].Value = 502.80;

            ws.Cells["B3:C94"].Style.Numberformat.Format = "#,##0.00";

            //ws.Cells["A1"].Value = "OS CÁLCULOS ABAIXO ESTÃO M2 BRANCO";
            ws.Cells["B2"].Value = "Valor";
            ws.Cells["C2"].Value = "Área";
            ws.Cells["B2:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // QTDE POR M2 X a metragem
            var itemB = 3;
            using (ExcelRange r = ws.Cells["B3:B94"])
            {
                var conteudo = "=C" + itemB.ToString().TrimEnd() + "*$O$3";
                r.Formula = conteudo;
                itemB++;
            }


            ws.Cells["E2"].Value = "QTDE";
            ws.Cells["F2"].Value = "M2 TOTAL";
            ws.Cells["G2"].Value = "VALOR TOTAL";
            ws.Cells["H2"].Value = "VALOR TOTAL";
            ws.Cells["I2"].Value = "VALOR TOTAL";
            ws.Cells["J2"].Value = "VALOR TOTAL";

            // QTDE POR M2
            using (ExcelRange r = ws.Cells["E3:E94"])
            {
                r.Value = 1;
            }

            ws.Cells["E5"].Value = 12;
            ws.Cells["E15"].Value = 2;
            ws.Cells["E16"].Value = 3;
            ws.Cells["E17"].Value = 2;
            ws.Cells["E35"].Value = 48;
            ws.Cells["E50"].Value = 48;


            // QTDE POR M2 X a metragem
            var item = 3;
            using (ExcelRange r = ws.Cells["F3:F94"])
            {
                var conteudo = "=C" + item.ToString().TrimEnd() + "*E" + item.ToString().TrimEnd();
                r.Formula = conteudo;
                item++;
            }

            // Preços totais para os lotes categoria 1  - VERDE
            //ws.Cells["G19"].Formula = "=E19*B19/O3*O1";

            // buscar os lotes por categoria (verde) e depois procura pela referência da metragem e depois monta e grava a fórmula
            var lotes = (from x in db.Lotes
                         where x.LoteamentoId == 7 && x.CategoriaId == 1 && x.SituacaoNoSite.CompareTo("0") > 0
                         select x).ToList();
            foreach (var lote in lotes)
            {
                // procura onde se encontra a metragem na planilha
                var findrange = ws.Cells.FirstOrDefault(cell => cell.Text == lote.Area.ToString());
                if (findrange != null)
                {
                    var row = findrange.Start.Row;
                    var col = findrange.Start.Column;
                    var celula = "G" + row.ToString();
                    var formula = "=E" + row.ToString() + "*B" + row.ToString() + "/O3*O1";
                    ws.Cells[celula].Formula = formula;
                }
            }
            ws.Cells["G3:G95"].Style.Numberformat.Format = "#,##0.00";

            ws.Cells["G3:G94"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["G3:G94"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(146, 208, 80));
            ws.Cells["G95"].Formula = "=Sum(G3:G94)";
            ws.Cells["G95"].Style.Font.Bold = true;
            ws.Cells["G95"].Style.Font.Italic = true;


            // Preços totais para os lotes categoria 2  - AMARELO

            // buscar os lotes por categoria (amarelo) e depois procura pela referência da metragem e depois monta e grava a fórmula
            lotes = null;
            lotes = (from x in db.Lotes
                     where x.LoteamentoId == 7 && x.CategoriaId == 2 && x.SituacaoNoSite.CompareTo("0") > 0
                     select x).ToList();
            foreach (var lote in lotes)
            {
                // procura onde se encontra a metragem na planilha
                var findrange = ws.Cells.FirstOrDefault(cell => cell.Text == lote.Area.ToString());
                if (findrange != null)
                {
                    var row = findrange.Start.Row;
                    var col = findrange.Start.Column;
                    var celula = "H" + row.ToString();
                    var formula = "=E" + row.ToString() + "*B" + row.ToString() + "/O3*O2";
                    ws.Cells[celula].Formula = formula;
                }
            }
            ws.Cells["H3:H95"].Style.Numberformat.Format = "#,##0.00";

            ws.Cells["H3:H94"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["H3:H94"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 0));
            ws.Cells["H95"].Formula = "=Sum(H3:H94)";
            ws.Cells["H95"].Style.Font.Bold = true;
            ws.Cells["H95"].Style.Font.Italic = true;


            // Preços totais para os lotes categoria 3  - branco

            // buscar os lotes por categoria (branco) e depois procura pela referência da metragem e depois monta e grava a fórmula
            lotes = null;
            lotes = (from x in db.Lotes
                     where x.LoteamentoId == 7 && x.CategoriaId == 3 && x.SituacaoNoSite.CompareTo("0") > 0
                     select x).ToList();

            foreach (var lote in lotes)
            {
                // procura onde se encontra a metragem na planilha
                var findrange = ws.Cells.FirstOrDefault(cell => cell.Text == lote.Area.ToString());
                if (findrange != null)
                {
                    var row = findrange.Start.Row;
                    var col = findrange.Start.Column;
                    var celula = "I" + row.ToString();
                    var formula = "=E" + row.ToString() + "*B" + row.ToString() + "/O3*O3";
                    ws.Cells[celula].Formula = formula;
                }
            }
            ws.Cells["I3:I95"].Style.Numberformat.Format = "#,##0.00";

            ws.Cells["I3:I94"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["I3:I94"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 255));
            ws.Cells["I95"].Formula = "=Sum(I3:I94)";
            ws.Cells["I95"].Style.Font.Bold = true;
            ws.Cells["I95"].Style.Font.Italic = true;

            // Preços totais para os lotes categoria 4  - AZUL

            // buscar os lotes por categoria (AZUL) e depois procura pela referência da metragem e depois monta e grava a fórmula
            lotes = null;
            lotes = (from x in db.Lotes where x.LoteamentoId == 7 && x.CategoriaId == 4 && x.SituacaoNoSite.CompareTo("0") > 0 select x).ToList();
            foreach (var lote in lotes)
            {
                // procura onde se encontra a metragem na planilha
                var findValue = lote.Area.ToString("#,##0.00");
                var findrange = ws.Cells.FirstOrDefault(cell => cell.Text == findValue);
                if (findrange != null)
                {
                    var row = findrange.Start.Row;
                    var col = findrange.Start.Column;
                    var celula = "J" + row.ToString();
                    var formula = "=E" + row.ToString() + "*B" + row.ToString() + "/O3*O4";
                    ws.Cells[celula].Formula = formula;
                }
            }
            ws.Cells["J3:J95"].Style.Numberformat.Format = "#,##0.00";

            ws.Cells["J3:J94"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["J3:J94"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 176, 240));
            ws.Cells["J95"].Formula = "=Sum(J3:J94)";
            ws.Cells["J95"].Style.Font.Bold = true;
            ws.Cells["J95"].Style.Font.Italic = true;

            ws.Cells["E95"].Formula = "=Sum(E3:E94)";
            ws.Cells["E95"].Style.Numberformat.Format = "#,##0.00";
            ws.Cells["E95"].Style.Font.Bold = true;
            ws.Cells["E95"].Style.Font.Italic = true;

            ws.Cells["F95"].Formula = "=Sum(F3:F94)";
            ws.Cells["F95"].Style.Numberformat.Format = "#,##0.00";
            ws.Cells["F95"].Style.Font.Bold = true;
            ws.Cells["F95"].Style.Font.Italic = true;


            ws.Cells["G96:J96"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells["G96"].Formula = "=Sum(G95:J95)";
            ws.Cells["G96"].Style.Numberformat.Format = "#,##0.00";
            ws.Cells["G96:J96"].Merge = true;
            ws.Cells["J95"].Style.Font.Bold = true;
            ws.Cells["J95"].Style.Font.Italic = true;

            ws.Cells["P1:R4"].Merge = true;
            ws.Cells["P1:R4"].Style.WrapText = true;
            ws.Cells["P1:R4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells["P1:R4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells["P1"].Value = "ESSES VALORES PODEM SER ALTERADOS";
            ws.Cells["P1:R4"].Style.Font.Bold = true;

            var celulas = ws.Cells["E2:J95"];
            celulas.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.View.ShowGridLines = true;

            ws.Column(13).Width = 24;
            ws.Cells["M7:S10"].Merge = true;
            ws.Cells["M7"].Style.Font.SetFromFont("Arial", 12, true, true, true, false);
            ws.Cells["M7"].Style.WrapText = true;
            ws.Cells["M7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            ws.Cells["M7"].Value = "OBS: NENHUM OUTRO VALOR DA PLANILA PODE SER ALTERADO. ELES SÃO CALCULADOS AUTOMATICAMENTE";

            #endregion

            /////////////////////////////////////////////
            ///
            /// Planilha Tabela de Vendas
            /// 
            /////////////////////////////////////////////


            #region Planilha Tabela de Vendas

            var tabv = pck.Workbook.Worksheets.Add("Tabela de Vendas");
            pck.Workbook.Worksheets.MoveBefore("Tabela de Vendas", "Base");


            for (var rowitem = 1; rowitem <= 30; rowitem++)
            {
                tabv.Column(rowitem).Width = 16;
            }


            tabv.Cells["A3:C3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            tabv.Cells["A3:C3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 0));
            tabv.Cells["A3:B3"].Merge = true;
            tabv.Cells["A3"].Value = "VALOR DA SEMESTRAL=";
            tabv.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            tabv.Cells["C3"].Value = 7500;
            tabv.Cells["C3"].Style.Numberformat.Format = "#,##0.00";

            tabv.Cells["B6:B7"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            tabv.Cells["B6"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 0));
            tabv.Cells["B7"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 0, 0));
            tabv.Cells["C6"].Value = "dados que pode ser alterados";
            tabv.Cells["C7"].Value = "não mexer";

            tabv.Cells["E1:G4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            tabv.Cells["E1:G2"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 0));
            tabv.Cells["E3:E4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 255));
            tabv.Cells["F3:G4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 0, 0));
            tabv.Cells["E1:F3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            tabv.Cells["E1"].Value = "IPCA =";
            tabv.Cells["E2"].Value = "CORREÇÃO (i)=";
            tabv.Cells["F1"].Value = 0;
            tabv.Cells["F2"].Value = 3;
            tabv.Cells["G1"].Value = "% a.a.";
            tabv.Cells["G2"].Value = "% a.a.";
            tabv.Cells["F3"].Value = 0.25;
            tabv.Cells["G3"].Value = "% a.a.";
            tabv.Cells["F4"].Value = "NÃO MEXER";
            tabv.Cells["F4:G4"].Merge = true;
            tabv.Cells["F4:G4"].Style.Font.Bold = true;
            tabv.Cells["F4:G4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Column(9).Width = 30.5;
            tabv.Column(10).Width = 21;
            tabv.Column(11).Width = 21;
            tabv.Column(12).Width = 21;
            tabv.Column(13).Width = 21;
            tabv.Column(14).Width = 21;
            tabv.Cells["I1:Y15"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            tabv.Cells["I1:Y15"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 0, 0));
            // 
            tabv.Cells["I2"].Value = "semestrais trazidas a VP6";
            tabv.Cells["I3"].Value = "semestrais trazidas a VP12";
            tabv.Cells["I4"].Value = "semestrais trazidas a VP18";
            tabv.Cells["I5"].Value = "semestrais trazidas a VP24";
            tabv.Cells["I6"].Value = "semestrais trazidas a VP30";
            tabv.Cells["I7"].Value = "semestrais trazidas a VP36";
            tabv.Cells["I8"].Value = "semestrais trazidas a VP42";
            tabv.Cells["I9"].Value = "semestrais trazidas a VP48";
            tabv.Cells["I10"].Value = "semestrais trazidas a VP54";
            tabv.Cells["I11"].Value = "semestrais trazidas a VP60";
            //// 1o. Semestre
            tabv.Cells["K1"].Value = "1a. SEMESTRAL";
            tabv.Cells["K1:L1"].Merge = true;
            tabv.Cells["K1:L1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Cells["K2"].Value = "FV=";
            tabv.Cells["K3"].Value = "N=";
            tabv.Cells["K4"].Value = "I=";
            tabv.Cells["K9"].Value = "FV=";
            tabv.Cells["K10"].Value = "N=";
            tabv.Cells["K11"].Value = "I=";
            tabv.Cells["K2:K4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["K9:K11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Cells["N2"].Value = "FV=";
            tabv.Cells["N3"].Value = "N=";
            tabv.Cells["N4"].Value = "I=";
            tabv.Cells["N9"].Value = "FV=";
            tabv.Cells["N10"].Value = "N=";
            tabv.Cells["N11"].Value = "I=";
            tabv.Cells["N2:N4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["N9:N11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Cells["Q2"].Value = "FV=";
            tabv.Cells["Q3"].Value = "N=";
            tabv.Cells["Q4"].Value = "I=";
            tabv.Cells["Q9"].Value = "FV=";
            tabv.Cells["Q10"].Value = "N=";
            tabv.Cells["Q11"].Value = "I=";
            tabv.Cells["Q2:Q4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["Q9:Q11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Cells["T2"].Value = "FV=";
            tabv.Cells["T3"].Value = "N=";
            tabv.Cells["T4"].Value = "I=";
            tabv.Cells["T9"].Value = "FV=";
            tabv.Cells["T10"].Value = "N=";
            tabv.Cells["T11"].Value = "I=";
            tabv.Cells["T2:T4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["T9:T11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            tabv.Cells["W2"].Value = "FV=";
            tabv.Cells["W3"].Value = "N=";
            tabv.Cells["W4"].Value = "I=";
            tabv.Cells["W9"].Value = "FV=";
            tabv.Cells["W10"].Value = "N=";
            tabv.Cells["W11"].Value = "I=";
            tabv.Cells["W2:W4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["W9:W11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;



            tabv.Cells["L2"].Formula = "=C3";
            tabv.Cells["L3"].Value = 6;
            tabv.Cells["L4"].Formula = "=F3/100";
            tabv.Cells["J2:J11"].Style.Numberformat.Format = "#,##0.00";
            tabv.Cells["J2"].Formula = "=L2/(Power((1+L4),L3))";

            //// 2o. Semestre
            tabv.Cells["N1"].Value = "2a. SEMESTRAL";
            tabv.Cells["N1:O1"].Merge = true;
            tabv.Cells["N1:O1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["O2"].Formula = "=C3";
            tabv.Cells["O3"].Value = 12;
            tabv.Cells["O4"].Formula = "=F3/100";
            tabv.Cells["J3"].Formula = "=O2/(Power((1+O4),O3))";
            //// 3o. Semestre
            tabv.Cells["Q1"].Value = "3a. SEMESTRAL";
            tabv.Cells["Q1:R1"].Merge = true;
            tabv.Cells["Q1:R1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["R2"].Formula = "=C3";
            tabv.Cells["R3"].Value = 18;
            tabv.Cells["R4"].Formula = "=F3/100";
            tabv.Cells["J4"].Formula = "=R2/(Power((1+R4),R3))";
            //// 4o. Semestre
            tabv.Cells["T1"].Value = "4a. SEMESTRAL";
            tabv.Cells["T1:U1"].Merge = true;
            tabv.Cells["T1:U1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["U2"].Formula = "=C3";
            tabv.Cells["U3"].Value = 24;
            tabv.Cells["U4"].Formula = "=F3/100";
            tabv.Cells["J5"].Formula = "=U2/(Power((1+U4),U3))";
            //// 5o. Semestre
            tabv.Cells["k7"].Value = "5a. SEMESTRAL";
            tabv.Cells["k7:l7"].Merge = true;
            tabv.Cells["K7:L7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["L9"].Formula = "=C3";
            tabv.Cells["L10"].Value = 30;
            tabv.Cells["L11"].Formula = "=F3/100";
            tabv.Cells["J6"].Formula = "=L9/(Power((1+L11),L10))";
            //// 6o. Semestre
            tabv.Cells["N7"].Value = "6a. SEMESTRAL";
            tabv.Cells["N7:O7"].Merge = true;
            tabv.Cells["N7:O7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["O9"].Formula = "=C3";
            tabv.Cells["O10"].Value = 36;
            tabv.Cells["O11"].Formula = "=F3/100";
            tabv.Cells["J7"].Formula = "=O9/(Power((1+O11),O10))";
            //// 7o. Semestre
            tabv.Cells["W7"].Value = "7a. SEMESTRAL";
            tabv.Cells["W7:X7"].Merge = true;
            tabv.Cells["W7:X7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["X9"].Formula = "=C3";
            tabv.Cells["X10"].Value = 42;
            tabv.Cells["X11"].Formula = "=F3/100";
            tabv.Cells["J8"].Formula = "=X9/(Power((1+X11),X10))";
            //// 8o. Semestre
            tabv.Cells["Q7"].Value = "8a. SEMESTRAL";
            tabv.Cells["Q7:R7"].Merge = true;
            tabv.Cells["Q7:R7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["R9"].Formula = "=C3";
            tabv.Cells["R10"].Value = 48;
            tabv.Cells["R11"].Formula = "=F3/100";
            tabv.Cells["J9"].Formula = "=R9/(Power((1+R11),R10))";
            //// 9o. Semestre
            tabv.Cells["T7"].Value = "9a. SEMESTRAL";
            tabv.Cells["T7:U7"].Merge = true;
            tabv.Cells["T7:U7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["U9"].Formula = "=C3";
            tabv.Cells["U10"].Value = 54;
            tabv.Cells["U11"].Formula = "=F3/100";
            tabv.Cells["J10"].Formula = "=U9/(Power((1+U11),U10))";

            //// 10o. Semestre
            tabv.Cells["W1"].Value = "10a. SEMESTRAL";
            tabv.Cells["W1:X1"].Merge = true;
            tabv.Cells["W1:X1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["X2"].Formula = "=C3";
            tabv.Cells["X3"].Value = 60;
            tabv.Cells["X4"].Formula = "=F3/100";
            tabv.Cells["J11"].Formula = "=X2/(Power((1+X4),X3))";

            tabv.Cells["J13"].Value = "NÃO MEXER !!!!  CALCULOS REALIZADOS AUTOMATICAMENTE";
            tabv.Cells["J13"].Style.Font.Bold = true;
            tabv.Cells["J13"].Style.Font.UnderLine = true;
            tabv.Cells["J13:P13"].Merge = true;
            tabv.Cells["J13:P13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            tabv.Cells["A10"].Value = "OBS: NÃO MEXER NOS DADOS APRESENTADOS!!! ELES SÃO CALCULADOS AUTOMATICAMENTE. SOMENTE OS DADOS EM AMARELO PODERÃO SER MODIFICADOS.";
            tabv.Cells["A10"].Style.Font.Bold = true;
            tabv.Cells["A10"].Style.Font.UnderLine = true;
            tabv.Cells["A10"].Style.Font.Size = 12;

            tabv.Cells["A10:G13"].Merge = true;
            tabv.Cells["A10:G13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            tabv.Cells["A10:G13"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            tabv.Cells["A10:G13"].Style.WrapText = true;
            tabv.Cells["A10:G13"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            tabv.Cells["A10:G13"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 217, 217));

            tabv.Column(3).Width = 27;
            tabv.Column(4).Width = 19;
            tabv.Column(5).Width = 19;
            tabv.Column(6).Width = 19;
            tabv.Column(7).Width = 19;
            tabv.Column(8).Width = 19;
            lotes = null;
            lotes = db.Lotes.Where(c => c.LoteamentoId == 7).ToList();

            var linha = 16;
            var ref_m2 = 3;
            var plano = 1;
            var nrSemestrais = 2;
            var corcategoria = "";
            var m2categoria = 0.0m;
            var idcategoria = 0;
            var celulainicial = "";
            var celulafinal = "";
            

            //for (int zi = 1; zi <= 460; zi++)

            for (int zi = 1; zi <= 460; zi++)
            {
                var celula = "";
                var refer = "C" + ref_m2.ToString();

                if (plano == 1)
                {
                    celula = "A" + linha.ToString();

                    //////// 
                    ////////  Busca a categoria do lote e depois a cor da categoria
                    /// 
                    m2categoria = decimal.Parse( ws.Cells[refer].Value.ToString());

                    idcategoria = (from x in db.Lotes
                                    where x.LoteamentoId == 7 && x.Area == m2categoria
                                    select x.CategoriaId).FirstOrDefault();

                    corcategoria = (from x in db.TabelaM2 where x.Id == idcategoria select x.CorFundo).FirstOrDefault();


                    tabv.Cells[celula].Value = "LOTE DE " + ws.Cells[refer].Value.ToString().Replace(".", ",") + " m2";
                    linha++;

                    celulainicial = "B" + (linha).ToString();
                    celulafinal = "M" + (linha+5).ToString();
                    tabv.Cells["" +celulainicial+ ":" + celulafinal + ""].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    tabv.Cells["" + celulainicial + ":" + celulafinal + ""].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml(corcategoria));

                    celula = "B" + (linha).ToString();
                    tabv.Cells[celula].Value = "Planos";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);


                    celula = "C" + linha.ToString();
                    tabv.Cells[celula].Value = "Preço de Venda R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "D" + linha.ToString();
                    tabv.Cells[celula].Value = "Entrada R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "E" + linha.ToString();
                    tabv.Cells[celula].Value = "Saldo de R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "F" + linha.ToString();
                    tabv.Cells[celula].Value = "Mensais até";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "G" + linha.ToString();
                    tabv.Cells[celula].Value = "Mensais R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "I" + linha.ToString();
                    tabv.Cells[celula].Value = "Semestrais R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "J" + linha.ToString();
                    tabv.Cells[celula].Value = "Vr total pc pagas R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "K" + linha.ToString();
                    tabv.Cells[celula].Value = "Saldo rem p/ quitar R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "L" + linha.ToString();
                    tabv.Cells[celula].Value = "Preço Venda Corrig R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    celula = "M" + linha.ToString();
                    tabv.Cells[celula].Value = "Juros Período R$";
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    linha++;

                }

                // Planos
                celula = "B" + linha.ToString();
                tabv.Cells[celula].Value = plano;
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Preços de venda
                celula = "C" + linha.ToString();
                var temp = "=base!B" + ref_m2.ToString();
                tabv.Cells[celula].Formula = temp;
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // segue os parâmetros de preços conforme a categoria de preço do lote/metragem
                var valorws = ws.Cells[refer].Value.ToString();
                decimal findValue = Decimal.Parse(valorws);
                var procura = lotes.Where(c => c.Area == findValue).FirstOrDefault();
                if (procura != null)
                {
                    var col = procura.CategoriaId.ToString();

                    celula = "C" + linha.ToString();
                    temp = "$O$" + col.ToString();
                    var formula = "=(base!" + temp + "*" + procura.Area.ToString() + ")";
                    tabv.Cells[celula].Formula = formula;
                }



                // Entrada
                celula = "D" + linha.ToString();
                temp = "=C" + linha.ToString() + "*0.15";
                tabv.Cells[celula].Formula = temp;
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Saldo
                celula = "E" + linha.ToString();
                temp = "=C" + linha.ToString() + "-D" + linha.ToString();
                tabv.Cells[celula].Formula = temp;
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Mensais até
                celula = "F" + linha.ToString();
                tabv.Cells[celula].Value = (12 * plano).ToString() + " X";
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                // Valor Mensal
                celula = "G" + linha.ToString();
                if (plano == 1)
                {
                    tabv.Cells[celula].Value = 7000;
                }
                if (plano == 2)
                {
                    temp = "=((E" + linha.ToString() + "*0.6)-($J$2+$J$3+$J$4+$J$5))*((($F$3/100)*(POWER(1+($F$3/100)," + (plano * 12).ToString() + "))/(POWER(1+($F$3/100)," + (plano * 12).ToString() + ")))-1)/-" + (plano * 12).ToString();
                    tabv.Cells[celula].Formula = temp;
                }
                if (plano == 3)
                {
                    temp = "=((E" + linha.ToString() + "*0.6)-($J$2+$J$3+$J$4+$J$5+$J$6+$J$7))*((($F$3/100)*(POWER(1+($F$3/100)," + (plano * 12).ToString() + "))/(POWER(1+($F$3/100)," + (plano * 12).ToString() + ")))-1)/-" + (plano * 12).ToString();
                    tabv.Cells[celula].Formula = temp;
                }
                if (plano == 4)
                {
                    temp = "=((E" + linha.ToString() + "*0.6)-($J$2+$J$3+$J$4+$J$5+$J$6+$J$7+$J$8+$J$9))*((($F$3/100)*(POWER(1+($F$3/100)," + (plano * 12).ToString() + "))/(POWER(1+($F$3/100)," + (plano * 12).ToString() + ")))-1)/-" + (plano * 12).ToString();
                    tabv.Cells[celula].Formula = temp;
                }
                if (plano == 5)
                {
                    temp = "=((E" + linha.ToString() + "*0.6)-($J$2+$J$3+$J$4+$J$5+$J$6+$J$7+$J$8+$J$9+$J$10+$J$11))*((($F$3/100)*(POWER(1+($F$3/100)," + (plano * 12).ToString() + "))/(POWER(1+($F$3/100)," + (plano * 12).ToString() + ")))-1)/-" + (plano * 12).ToString();
                    tabv.Cells[celula].Formula = temp;
                }

                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Semestrais até
                celula = "H" + linha.ToString();
                tabv.Cells[celula].Value = (2 * plano).ToString() + " X";
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                //// Semestrais R$
                celula = "I" + linha.ToString();
                tabv.Cells[celula].Formula = "=$C$3";
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Total parc pagas
                celula = "J" + linha.ToString();
                temp = "=(I" + linha.ToString() + "*" + (nrSemestrais * plano).ToString() + ") + (G" + linha.ToString() + "*" + (plano * 12).ToString() + ")";
                tabv.Cells[celula].Formula = temp;
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Saldo rem para quitação
                if (plano == 1)
                {
                    celula = "K" + linha.ToString();
                    tabv.Cells[celula].Formula = "=E" + linha.ToString() + "-J" + linha.ToString();
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";
                }
                else
                {
                    celula = "K" + linha.ToString();
                    temp = "=(E" + linha.ToString() + "*0.4)*(POWER(1+($F$3/100)," + (plano * 12).ToString() + "))";
                    tabv.Cells[celula].Formula = temp;
                    tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                    tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";
                }

                // preço de venda corrigido
                celula = "L" + linha.ToString();
                tabv.Cells[celula].Formula = "=K" + linha.ToString() + "+J" + linha.ToString() + "+D" + linha.ToString();
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                // Juros cobrados no período
                celula = "M" + linha.ToString();
                if (plano == 1)
                {
                    tabv.Cells[celula].Value = 0;
                }
                else
                {
                    tabv.Cells[celula].Formula = "=L" + linha.ToString() + "-C" + linha.ToString();
                }
                tabv.Cells[celula].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                tabv.Cells[celula].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                tabv.Cells[celula].Style.Numberformat.Format = "#,##0.00";

                linha++;

                plano++;
                if (plano > 5)
                {
                    plano = 1;
                    linha += 2;
                    ref_m2++;
                }
            }

            #endregion

            var file = new MemoryStream(pck.GetAsByteArray());
            byte[] arquivo = file.ToArray();

            return File(arquivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tabela de Preços.xlsx");

        }

    }
}
