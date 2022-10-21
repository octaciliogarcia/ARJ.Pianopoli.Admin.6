namespace ARJ.Pianopoli.Admin._6.Models
{
    public class LotesViewModel
    {
        public int Id { get; set; }
        public int LoteamentoId { get; set; }

        public string Quadra { get; set; }

        public int Lote { get; set; }

        public int CategoriaId { get; set; }

        public decimal M2 { get; set; }

        public string Situacao { get; set; }
    }
}
