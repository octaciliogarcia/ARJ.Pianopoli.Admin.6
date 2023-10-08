using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class CorretorVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Campo obrigatório")]
        public string Nome { get; set; }
        [Display(Name = "CPF")]
        [Required(ErrorMessage = "Campo obrigatório")]
        public string Cpf { get; set; }
        [Display(Name ="CRECI")]
        public string Creci { get; set; }
        public int ImobiliariaId { get; set; }
        [Display(Name ="Imobiliária")]
        public string ImobiliariaNome { get; set; }
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Campo obrigatório")] 
        public string Email { get; set; }

        [Display(Name = "Senha")]
        
        public string Senha { get; set; }
        [Display(Name = "Confirmar Senha")]
        public string ConfirmarSenha { get; set; }
        public string UserId { get; set; }
    }
}
