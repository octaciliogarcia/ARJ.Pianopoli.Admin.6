using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
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

        [Authorize]
        public IActionResult PlanilhaPrecos()
        {
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

            ws.Cells["O1"].Value = 800;
            ws.Cells["O2"].Value = 780;
            ws.Cells["O3"].Value = 750;
            ws.Cells["O4"].Value = 400;

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

            ws.Cells["A1"].Value = "OS CÁLCULOS ABAIXO ESTÃO M2 BRANCO";
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
            var lotes = db.Lotes.Where(c=>c.LoteamentoId==7 && c.CategoriaId==1 && int.Parse(c.SituacaoNoSite) > 0).ToList();
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
            lotes = db.Lotes.Where(c => c.LoteamentoId == 7 && c.CategoriaId == 2 && int.Parse( c.SituacaoNoSite ) > 0).ToList();
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
            lotes = db.Lotes.Where(c => c.LoteamentoId == 7 && c.CategoriaId == 3 && int.Parse(c.SituacaoNoSite) > 0 ).ToList();
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
            lotes = db.Lotes.Where(c => c.LoteamentoId == 7 && c.CategoriaId == 4 && int.Parse(c.SituacaoNoSite) > 0 ).ToList();
            foreach (var lote in lotes)
            {
                // procura onde se encontra a metragem na planilha
                var findValue = lote.Area.ToString().Replace(".",",");
                var findrange = ws.Cells.FirstOrDefault(cell => cell.Text.Replace(".","") == findValue);
                //var findrange = ws.Cells.FirstOrDefault(cell => cell.Formula == $"\"{findValue}\"");
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
            ws.Cells["P1:R4"].Style.WrapText= true;
            ws.Cells["P1:R4"].Style.VerticalAlignment =ExcelVerticalAlignment.Center;
            ws.Cells["P1:R4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells["P1"].Value = "ESSES VALORES PODEM SER ALTERADOS";
            ws.Cells["P1:R4"].Style.Font.Bold = true;

            var celulas = ws.Cells["E2:J95"];
            celulas.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            celulas.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.View.ShowGridLines= true;

            #endregion

            #region Planilha Tabela de Vendas

            var tabv = pck.Workbook.Worksheets.Add("Tabela de Vendas");
            pck.Workbook.Worksheets.MoveBefore("Tabela de Vendas", "Base");

            
            for (var rowitem =1;rowitem<=30;rowitem++)
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

            #endregion

            var file = new MemoryStream(pck.GetAsByteArray());
            byte[] arquivo = file.ToArray();

            return File(arquivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tabela de Preços.xlsx");

        }

    }
}
