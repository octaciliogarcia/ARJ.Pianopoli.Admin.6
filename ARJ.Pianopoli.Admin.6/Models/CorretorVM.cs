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
    }
}
