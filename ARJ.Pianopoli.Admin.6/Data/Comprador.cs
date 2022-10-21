using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Data
{
    public class Comprador
    {
        public Comprador()
        {
            PropostasCompradores = new HashSet<PropostasCompradores>();
        }
        [Key]
        public int Id { get; set; }
        public string Cpf { get; set; }
        public string Rg { get; set; }
        public string RgExpPor { get; set; }
        public string Nome { get; set; }
        public string Nacionalidade { get; set; }
        public DateTime DtNasc { get; set; }
        public string EstadoCivil { get; set; }
        public DateTime? CasamentoData { get; set; }
        public string CasamentoRegime { get; set; }
        public string CasamentoLivro { get; set; }
        public string CasamentoFolhas { get; set; }
        // Escritura no pacto ante-nupicial - Tabelião de Notas
        public string CasamentoEscrTabeliao { get; set; }
        // Registro do pacto - Registro de imóveis
        public string CasamentoEscrRegistro { get; set; }
        public string ConjugeNome { get; set; }
        public string ConjugeNacionalidade { get; set; }
        public string ConjugeProfissao { get; set; }
        public string ConjugeRg { get; set; }
        public string ConjugeRgExpPor { get; set; }
        public string ConjugeCpf { get; set; }
        public string ConjugeLogradouro { get; set; }
        public string ConjugeNumero { get; set; }
        public string ConjugeBairro { get; set; }
        public string ConjugeMunicipio { get; set; }
        public string ConjugeEstado { get; set; }
        public string ConjugeEmail { get; set; }
        public string ConjugeCelular { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string Profissao { get; set; }
        public string TelefoneFixo { get; set; }
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public string Usuario { get; set; }
        public DateTime DataHora { get; set; }
        public string UsuarioExclusao { get; set; }
        public DateTime? DataExclusao { get; set; }

        public virtual ICollection<PropostasCompradores> PropostasCompradores { get; set; }

    }
}
