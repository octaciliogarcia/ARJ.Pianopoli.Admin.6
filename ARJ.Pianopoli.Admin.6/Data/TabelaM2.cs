using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class TabelaM2
    {
        [Key]
        public int Id { get; set; }
        public int? CategoriaId { get; set; }
        public string Descricao { get; set; }
        public string CorFundo { get; set; }
        public DateTime? Inicio { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorM2 { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public string Usuario { get; set; }
        public DateTime? DataExclusao { get; set; }
        public string UsuarioExclusao { get; set; }

    }
}
