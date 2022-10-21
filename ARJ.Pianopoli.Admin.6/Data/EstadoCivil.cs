using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class EstadoCivil
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string Descricao { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
