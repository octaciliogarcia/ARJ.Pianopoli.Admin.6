using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class PropostaAnexo
    {
        [Key]
        public int Id { get; set; }

        public string Descricao { get; set; }
        public string TipoAnexo { get; set; }
        public string UrlAnexo { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }
        public virtual Proposta Proposta { get; set; }
    }
}
