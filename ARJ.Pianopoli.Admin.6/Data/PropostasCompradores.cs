namespace ARJ.Pianopoli.Admin._6.Data
{
    public partial class PropostasCompradores
    {

        public int Id { get; set; }
        public int? PropostaId { get; set; }
        public int? CompradorId { get; set; }
        public DateTime? DataHora { get; set; }
        public string Usuario { get; set; }
        public DateTime? DataExclusao { get; set; }
        public string UsuarioExclusao { get; set; }
        public virtual Comprador Comprador { get; set; }
        public virtual Proposta Proposta { get; set; }
    }
}
