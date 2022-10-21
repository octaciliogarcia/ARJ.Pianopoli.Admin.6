using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class LotesDescritivos
    {
        [Key]
        public int Id { get; set; }
        public int LoteamentoId { get; set; }
        public string Quadra { get; set; }
        public int Lote { get; set; }
        public string Descritivo { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Area { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
