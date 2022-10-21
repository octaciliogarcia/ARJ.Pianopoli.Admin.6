using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class LoteCategoria
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
