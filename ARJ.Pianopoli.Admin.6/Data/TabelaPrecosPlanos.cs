using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class TabelaPrecosPlanos
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PercEntrada { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }

    }
}
