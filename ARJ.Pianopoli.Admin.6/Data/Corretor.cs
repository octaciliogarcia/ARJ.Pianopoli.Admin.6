using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class Corretor
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Creci { get; set; }
        public int ImobiliariaId { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public string Usuario { get; set; }
        public DateTime? DataExclusao { get; set; }
        public string UsuarioExclusao { get; set; }
    }
}
