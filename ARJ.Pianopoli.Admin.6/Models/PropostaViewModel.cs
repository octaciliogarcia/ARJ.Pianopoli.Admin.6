using ARJ.Pianopoli.Admin._6.Data;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class PropostaViewModel
    {
        public int? Id { get; set; }
        public int? PropostaId { get; set; }
        public int LoteamentoId { get; set; }
        public string Quadra { get; set; }
        public int Lote { get; set; }
        public decimal Area { get; set; }
        public decimal PrecoM2 { get; set; }
        public string CorFundo { get; set; }
        public DateTime DataProposta { get; set; }
        public string Entrada { get; set; }
        public string SaldoPagar { get; set; }
        public string TotalParcelas { get; set; }
        public string SaldoQuitacao { get; set; }
        public string PrecoVendaCorrigido { get; set; }
        public string JurosCobrados { get; set; }
        public int Status { get; set; } = 1;
        public string StatusNoSite { get; set; }
        public List<Comprador> Compradores { get; set; }
        public List<PropostaAnexo> Anexos { get; set; }
        public List<PropostaCondicaoComercial> CondicoesComerciais { get; set; }
        public decimal ValorTotal { get; set; }

        public string TipoPagamento { get; set; }
        public string Parcelamento { get; set; }

        public DateTime? DataAprovacao { get; set; }
        public string AprovadoPor { get; set; }
        public string Contrato { get; set; }
        public string Usuario { get; set; }
        public bool result { get; set; }
    }

    public class PlanosParcelasViewModel
    {
        public string Plano { get; set; }
        public int NrParcelasMensais { get; set; }
        public double VrParcelaMensal { get; set; }
        public int NrParcelasSemestrais { get; set; }
        public double VrParcelaSemestral { get; set; }
    }
   

}
