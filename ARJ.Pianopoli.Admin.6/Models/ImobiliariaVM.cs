using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class ImobiliariaVM
    {
        [Display(Name ="ID")]
        public int ID { get; set; }
        [Display(Name ="Descrição")]
        public string Nome { get; set; }
    }
}
