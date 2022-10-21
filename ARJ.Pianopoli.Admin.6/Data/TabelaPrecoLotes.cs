using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class TabelaPrecoLotes
    {
        [Key]
        public int Id { get; set; }
        public string Quadra { get; set; }
        public int Lote { get; set; }
        public int Plano { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoVenda { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Entrada { get; set; }
        public int NrParcelasMensais { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal VrParcelasMensais { get; set; }
        public int NrParcelasSemestrais { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal VrParcelasSemestrais { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
