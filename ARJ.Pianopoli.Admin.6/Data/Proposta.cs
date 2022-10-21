using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class Proposta
    {
        public Proposta()
        {
            PropostasCompradores = new HashSet<PropostasCompradores>();
        }
        [Key]
        public int Id { get; set; }
        public int LoteamentoId { get; set; }
        public string Quadra { get; set; }
        public int Lote { get; set; }
        public DateTime DataProposta { get; set; }
        public int Status { get; set; } = 1;
        public List<Comprador> Compradores { get; set; }
        public ICollection<PropostaCondicaoComercial> CondicoesComerciais { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }
        public DateTime? DataAprovacao { get; set; }
        public string AprovadoPor { get; set; }
        public string Contrato { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
        public virtual ICollection<PropostasCompradores> PropostasCompradores { get; set; }

    }
}
