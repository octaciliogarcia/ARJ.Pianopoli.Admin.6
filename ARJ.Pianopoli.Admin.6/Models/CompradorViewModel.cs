using System.ComponentModel.DataAnnotations;

namespace ARJ.Pianopoli.Admin._6.Models
{
    public class CompradorViewModel
    {
        public int? Id { get; set; }
        public int PropostaId { get; set; }
        [Display(Name = "CPF")]
        public string Cpf { get; set; }
        [Display(Name = "RG")]
        public string Rg { get; set; }
        [Display(Name = "RG Exp. por")]
        public string RgExpPor { get; set; }
        [Display(Name = "Comprador")]
        public string Nome { get; set; }
        public string Nacionalidade { get; set; }
        [Display(Name = "Dt. Nasc.")]
        public DateTime DtNasc { get; set; }
        [Display(Name = "Est. Civil")]
        public string EstadoCivil { get; set; }
        [Display(Name = "Dt Casamento")]
        public DateTime? CasamentoData { get; set; }
        [Display(Name = "Regime")]
        public string CasamentoRegime { get; set; }
        [Display(Name = "Livro")]
        public string CasamentoLivro { get; set; }
        [Display(Name = "Fls.")]
        public string CasamentoFolhas { get; set; }
        // Escritura no pacto ante-nupicial - Tabelião de Notas
        [Display(Name = "Tabelião")]
        public string CasamentoEscrTabeliao { get; set; }
        // Registro do pacto - Registro de imóveis
        [Display(Name = "Cart. Registro")]
        public string CasamentoEscrRegistro { get; set; }
        [Display(Name = "Nome")]
        public string ConjugeNome { get; set; }
        [Display(Name = "Nacionalidade")]
        public string ConjugeNacionalidade { get; set; }
        [Display(Name = "Profissão")]
        public string ConjugeProfissao { get; set; }
        [Display(Name = "RG")]
        public string ConjugeRg { get; set; }
        [Display(Name = "Exp. Por")]
        public string ConjugeRgExpPor { get; set; }
        [Display(Name = "CPF")]
        public string ConjugeCpf { get; set; }
        [Display(Name = "Cep")]
        public string ConjugeCep { get; set; }
        [Display(Name = "Logradouro")]
        public string ConjugeLogradouro { get; set; }
        [Display(Name = "Número")]
        public string ConjugeNumero { get; set; }
        [Display(Name = "Complemento")]
        public string ConjugeComplemento { get; set; }
        [Display(Name = "Bairro")]
        public string ConjugeBairro { get; set; }
        [Display(Name = "Município")]
        public string ConjugeMunicipio { get; set; }
        [Display(Name = "UF")]
        public string ConjugeEstado { get; set; }
        [Display(Name = "E-mail")]
        public string ConjugeEmail { get; set; }

        [Display(Name = "Celular")]
        public string ConjugeCelular { get; set; }

        [Display(Name = "Celular")]
        public string Celular { get; set; }
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [Display(Name = "Profissão")]
        public string Profissao { get; set; }
        [Display(Name = "Tel. Fixo")]
        public string TelefoneFixo { get; set; }
        [Display(Name = "CEP")]
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        [Display(Name = "Número")]
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        [Display(Name = "Município")]
        public string Municipio { get; set; }
        [Display(Name = "UF")]
        public string Estado { get; set; }

    }
}
