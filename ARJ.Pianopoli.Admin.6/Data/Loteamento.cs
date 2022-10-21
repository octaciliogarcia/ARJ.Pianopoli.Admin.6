using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class Loteamento
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Municipio { get; set; }
        public int QtdeLotes { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal M2Total { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
