using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class TabelaPreco
    {
        [Key]
        public int Id { get; set; }
        public int? LoteamentoId { get; set; }
        [Column(TypeName = "smalldatetime")]
        public DateTime Validade { get; set; }
        public int CategoriaId { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
