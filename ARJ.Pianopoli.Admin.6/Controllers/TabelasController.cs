using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            return RedirectToAction("Precos");
        }

        [Authorize]
        public IActionResult Precos()
        {
            return View();
        }
        [Authorize]
        public IActionResult Imobiliarias()
        {
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
        public IActionResult Corretores()
        {
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
        public IActionResult ListarImobiliarias()
        {

            var retorno = (from x in db.Imobiliarias
                           select new ListaImobiliariasVM()
                           {
                               Id = x.Id,
                               Descricao = x.Nome,
                               SituacaoNoSite = x.DataExclusao != null ? "Inativo" : "Ativo"
                           }).ToList();

            return Json(new { data = retorno });
        }

        [Authorize]
        public IActionResult ListarCorretores()
        {

            var retorno = (from x in db.Corretores
                           join i in db.Imobiliarias on x.ImobiliariaId equals i.Id
                           select new ListaCorretoresVM()
                           {
                               Id = x.Id,
                               ImobiliariaId = x.ImobiliariaId,
                               Nome = x.Nome,
                               ImobiliariaNome = i.Nome,
                               SituacaoNoSite = x.DataExclusao != null ? "Inativo" : "Ativo"
                           }).ToList();

            return Json(new { data = retorno });
        }


        [HttpPost]
        [Authorize]
        public IActionResult EditarImobiliaria(int Id)
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
                    var obj = new ImobiliariaVM();
                    return PartialView("Imobiliaria", obj);
                }
                else
                {
                    var obj = (from f in db.Imobiliarias
                               where f.Id == Id
                               select new ImobiliariaVM()
                               {
                                   ID = f.Id,
                                   Nome = f.Nome,
                               }).FirstOrDefault();

                    return PartialView("Imobiliaria", obj);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult EditarCorretor(int Id)
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
                ViewBag.Imobiliarias = (from f in db.Imobiliarias
                                        where f.DataExclusao == null
                                        select new SelectListItem
                                        {
                                            Text = f.Nome,
                                            Value = f.Id.ToString()
                                        });

                if (Id == 0)
                {
                    var obj = new CorretorVM();
                    return PartialView("Corretor", obj);
                }
                else
                {
                    var obj = (from f in db.Corretores
                               join i in db.Imobiliarias on f.ImobiliariaId equals i.Id
                               where f.Id == Id
                               select new CorretorVM()
                               {
                                   Id = f.Id,
                                   Nome = f.Nome,
                                   ImobiliariaId = f.ImobiliariaId,
                                   ImobiliariaNome = f.Nome,
                                   Cpf = f.Cpf,
                                   Creci = f.Creci,
                               }).FirstOrDefault();

                    return PartialView("Corretor", obj);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        [Authorize]
        public IActionResult SalvarImobiliaria(ImobiliariaVM model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = new Imobiliaria();
                    obj.Nome = model.Nome;
                    obj.DataHora = DateTime.Now;
                    obj.Usuario = userId.ToString();
                    if (model.ID > 0)
                    {
                        obj.Id = (int)model.ID;
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Imobiliarias.Add(obj);
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
                        message = "Não foi possível efetuar o cadastro! "
                    });
                }

            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Não foi possível efetuar o cadastro! "
                });

            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult ExcluirImobiliaria(int Id)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (Id == 0)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Não foi possível efetuar o cadastro! "
                        });
                    }

                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = db.Imobiliarias.Where(c => c.Id == Id).FirstOrDefault();
                    obj.DataExclusao = DateTime.Now;
                    obj.UsuarioExclusao = userId.ToString();
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new
                    {
                        id = obj.Id,
                        result = true,
                        message = "dados excluídos com sucesso!"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        result = false,
                        message = "Não foi possível efetuar o cadastro! "
                    });
                }

            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Não foi possível efetuar o cadastro! "
                });

            }

        }


        [HttpPost]
        [Authorize]
        public IActionResult SalvarCorretor(CorretorVM model)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (model.Cpf == "" || model.Cpf == null)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "O CPF é de preenchimento obrigatório."
                        });

                    }
                    if (model.Nome == "")
                    {
                        return Json(new
                        {
                            result = false,
                            message = "O Nome é de preenchimento obrigatório."
                        });
                    }
                    if (model.Creci == "")
                    {
                        return Json(new
                        {
                            result = false,
                            message = "O CRECI é de preenchimento obrigatório."
                        });
                    }

                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = new Corretor();
                    obj.Nome = model.Nome;
                    obj.Cpf = model.Cpf;
                    obj.DataHora = DateTime.Now;
                    obj.Creci = model.Creci;
                    obj.ImobiliariaId = model.ImobiliariaId;
                    obj.Usuario = userId.ToString();

                    if (model.Id > 0)
                    {
                        obj.Id = (int)model.Id;
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Corretores.Add(obj);
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
                        message = "Não foi possível efetuar o cadastro! "
                    });
                }

            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Não foi possível efetuar o cadastro! "
                });

            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult ExcluirCorretor(int Id)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    if (Id == 0)
                    {
                        return Json(new
                        {
                            result = false,
                            message = "Não foi possível excluir o cadastro!"
                        });
                    }

                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var obj = db.Corretores.Where(c => c.Id == Id).FirstOrDefault();
                    obj.DataExclusao = DateTime.Now;
                    obj.UsuarioExclusao = userId.ToString();
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new
                    {
                        id = obj.Id,
                        result = true,
                        message = "dados excluídos com sucesso!"
                    });
                }
                catch (Exception)
                {
                    return Json(new
                    {
                        result = false,
                        message = "Não foi possível excluir o cadastro!"
                    });
                }

            }
            else
            {
                return Json(new
                {
                    result = false,
                    message = "Não foi possível excluir o cadastro!"
                });

            }
        }
    }
}
