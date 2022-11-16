using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class PropostaCondicaoComercial
    {
        [Key]
        public int Id { get; set; }
        public int PropostaId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorEntrada { get; set; }
        public int NrParcelasMensais { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorParcelaMensal { get; set; }
        public int NrParcelasSemestrais { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorParcelaSemestral { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalParcelas { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoQuitacao { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoVendaCorrigido { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal JurosPeriodo { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;
        public string Usuario { get; set; }
        public DateTime? DataExclusao { get; set; }
        public string UsuarioExclusao { get; set; }

    }
}
