namespace ARJ.Pianopoli.Admin._6.Models
{
    public class CondicoesViewModel
    {
        public int Lote { get; set; }
        public string Quadra { get; set; }
        public decimal Area { get; set; }
        public int LoteamentoId { get; set; }
        public decimal PrecoM2 { get; set; }
        public decimal ValorTotal { get; set; }
        // a propriedade Tipo será utilizada para habilitar o recurso de mostrar o botão
        // de chamada do formulário da proposta de dentro do form de condições Comerciais
        // tipo = 0 -> indica que não pode ser mostrado este botão
        // tipo = 1 -> mostrar o botão
        public int Tipo { get; set; }
        // número do id do registro
        public int? Registro { get; set; }
        public List<CondicoesPlanosViewModel> Planos { get; set; }
    }

    public class CondicoesPlanosViewModel
    {
        public int Plano { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal Entrada { get; set; }
        public decimal Saldo { get; set; }
        public int NrParcMen { get; set; }
        public decimal Mensais { get; set; }
        public int NrParcSem { get; set; }
        public decimal Semestrais { get; set; }
        public decimal TotalPagas { get; set; }
        public decimal SaldoRem { get; set; }
        public decimal PrecoComJuros { get; set; }
        public decimal JurosCobrados { get; set; }
    }


}
