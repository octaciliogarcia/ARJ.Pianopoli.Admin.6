using ARJ.Pianopoli.Admin._6.Data;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class PrecosVM
    {
        public int Id { get; set; }
        public int? CategoriaId { get; set; }
        public string Categoria { get; set; }
        public string Preco { get; set; }
        public string Cor { get; set; }
        public string Inicio { get; set; }
        public string Usuario { get; set; }
        public string DataHora { get;set; }

    }

    public class PrecoVM
    {
        public int Id { get; set; }
        public int? CategoriaId { get; set; }
        public string Categoria { get; set; }
        public string Preco { get; set; }
        public string Cor { get; set; }
        public string Inicio { get; set; }

    }
}
