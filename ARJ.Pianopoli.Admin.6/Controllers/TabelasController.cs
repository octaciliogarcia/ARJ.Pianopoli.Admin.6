using ARJ.Pianopoli.Admin._6.Core;
using ARJ.Pianopoli.Admin._6.Data;
using ARJ.Pianopoli.Admin._6.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;
        public TabelasController(DBContext db, IWebHostEnvironment hostingEnvironment, IAspNetUser user, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            _hostingEnvironment = hostingEnvironment;
            _user = user;
            _userManager = userManager;

        }
        //public IActionResult Index()
        //{
        //    return RedirectToAction("Precos");
        //}

        //[Authorize]
        //public IActionResult Precos()
        //{
        //    return View();
        //}
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

        [Authorize]
        [HttpPost]
        public IActionResult UsuariosImobiliaria(int Id)
        {

            var corretores = (from x in db.Corretores
                              where x.ImobiliariaId == Id
                              where x.DataExclusao == null
                              select new ListaCorretoresVM()
                              {
                                  Id = x.Id,
                                  ImobiliariaId = x.ImobiliariaId,
                                  Nome = x.Nome,
                                  ImobiliariaNome = "",
                                  Cpf = x.Cpf,
                                  Creci = x.Creci,
                                  SituacaoNoSite = x.DataExclusao != null ? "Inativo" : "Ativo"
                              }).ToList();

            var lista = (from x in db.Imobiliarias
                         where x.Id == Id
                         select new ListaCorretoresFiltroVM()
                         {
                             Nome = x.Nome,
                             ImobiliariaId = x.Id
                         }).FirstOrDefault();

            if (lista != null)
            {
                lista.Corretores = corretores;
            }

            return PartialView("ImobiliariasCorretores", lista);
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
        public IActionResult EditarCorretor(int Id, string _ImobiliariaId)
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
                if (_ImobiliariaId == null)
                {
                    ViewBag.Imobiliarias = (from f in db.Imobiliarias
                                            where f.DataExclusao == null
                                            select new SelectListItem
                                            {
                                                Text = f.Nome,
                                                Value = f.Id.ToString()
                                            });
                }
                else
                {
                    var imobiliariaId = int.Parse( _ImobiliariaId);
                    ViewBag.Imobiliarias = (from f in db.Imobiliarias
                                            where f.Id== imobiliariaId
                                            select new SelectListItem
                                            {
                                                Text = f.Nome,
                                                Value = f.Id.ToString()
                                            });
                }

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
                                   Email = f.Email,
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
        public async Task<IActionResult> SalvarCorretorAsync(CorretorVM model)
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
                    if(model.Email=="")
                    {
                        return Json(new
                        {
                            result = false,
                            message = "O Email é de preenchimento obrigatório."
                        });
                    }
                    // pega  o id do usuário logado
                    var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);


                    // Valida o e-mail informado. Se ele já existir, retorna erro.
                    if(model.Id>0)
                    {
                        // busca o id do corretor que sendo alterado
                        var corretorUserId = db.Corretores.Where(x => x.Id == model.Id).First().UserId;

                        var emailExiste = await _userManager.FindByEmailAsync(model.Email);
                        if (emailExiste != null && corretorUserId != emailExiste.Id)
                        {
                            return Json(new
                            {
                                result = false,
                                message = "O e-mail informado já está cadastrado."
                            });
                        }
                    }
                    else
                    {
                        // novo corretor
                        var emailExiste = await _userManager.FindByEmailAsync(model.Email);
                        if (emailExiste != null)
                        {
                            return Json(new
                            {
                                result = false,
                                message = "O e-mail informado já está cadastrado."
                            });
                        }
                    }

                    CorretorVM objid = model;

                    if (model.Id > 0)
                    {
                        var cpf = model.Cpf.Replace(".", "").Replace("-", "");
                        var obj = db.Corretores.Where(c => c.Id == model.Id).FirstOrDefault();
                        obj.Nome = model.Nome;
                        obj.Cpf = cpf;
                        obj.DataHora = DateTime.Now;
                        obj.Creci = model.Creci;
                        //obj.ImobiliariaId = model.ImobiliariaId;
                        obj.Email = model.Email;
                        obj.Usuario = userId.ToString();
                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();
                        
                        var usuario = await _userManager.FindByIdAsync(obj.UserId);
                        if(usuario != null)
                        {
                            if(model.Senha != "" && model.Senha != null)
                            {
                                usuario.PasswordHash = _userManager.PasswordHasher.HashPassword(usuario, model.Senha);
                                usuario.Email = model.Email;
                                var result = await _userManager.UpdateAsync(usuario);
                            }
                            else
                            {
                                usuario.Email = model.Email;
                                var result = await _userManager.UpdateAsync(usuario);
                            }

                            return Json(new
                            {
                                id = obj.Id,
                                result = true,
                                message = "dados cadastrados com sucesso!"
                            });

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
                    else
                    {
                        var cpf = model.Cpf.Replace(".", "").Replace("-", "");
                        var obj = new Corretor();
                        obj.Nome = model.Nome;
                        obj.Cpf = cpf;
                        obj.DataHora = DateTime.Now;
                        obj.Creci = model.Creci;
                        obj.ImobiliariaId = model.ImobiliariaId;
                        obj.Email = model.Email;
                        obj.Usuario = userId.ToString();



                        var email = model.Email;
                        var user = new IdentityUser { UserName = model.Email, Email = email };
                        var result = await _userManager.CreateAsync(user, model.Senha);
                        if (result.Succeeded)
                        {
                            // grava o id do usuário no corretor
                            obj.UserId = user.Id;
                            db.Corretores.Add(obj);
                            db.SaveChanges();
                        }
                        objid.Id = obj.Id;

                    }

                        return Json(new
                    {
                        id = objid.Id,
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
