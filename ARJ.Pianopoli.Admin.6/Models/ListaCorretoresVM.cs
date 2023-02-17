using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class ListaCorretoresVM
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string ImobiliariaNome { get; set; }
        public int ImobiliariaId { get; set; }
        public string SituacaoNoSite { get; set; }

    }
}
