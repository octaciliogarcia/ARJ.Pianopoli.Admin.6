using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class ListaImobiliariasVM
    {
        public int Id { get; set; }
        [Display(Name="Descição")]
        public string Descricao { get; set; }
        public string SituacaoNoSite { get; set; }
    }
}
