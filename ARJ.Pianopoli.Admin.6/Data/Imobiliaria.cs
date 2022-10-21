using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class Imobiliaria
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = String.Empty;
        public string Usuario { get; set; } = String.Empty;
        public DateTime DataHora { get; set; } = DateTime.Now;
        public string UsuarioExclusao { get; set; } = String.Empty;
        public DateTime? DataExclusao { get; set; }

    }
}
