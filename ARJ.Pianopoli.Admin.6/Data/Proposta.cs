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
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorCorretagem { get; set; }
        public string NumeroBoletoEntrada { get; set; }
        public DateTime? PrimeiroVencMensal { get; set; }
        public DateTime? PrimeiroVencSemestral { get; set; }
        public string BancoCliente { get; set; }
        public string AgenciaCliente { get; set; }
        public string ContaCliente { get; set; }
        public string TestemunhaNome1 { get; set; }
        public string TestemunhaEnd1 { get; set; }
        public string TestemunhaRg1 { get; set; }
        public string TestemunhaCpf1 { get; set; }
        public string TestemunhaNome2 { get; set; }
        public string TestemunhaEnd2 { get; set; }
        public string TestemunhaRg2 { get; set; }
        public string TestemunhaCpf2 { get; set; }
        public DateTime? DataAprovacao { get; set; }
        public string AprovadoPor { get; set; }
        public string Contrato { get; set; }
        public int? CorretorId { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
        public virtual ICollection<PropostasCompradores> PropostasCompradores { get; set; }

    }
}
