

namespace ARJ.Pianopoli.Admin._6.Utils
{
    public class MesPorExtenso
    {
        public static string MesExtenso(DateTime DataRef)
        {
            var mes = DataRef.Month;
            var retorno = "";
            switch (mes)
            {
                    case 1:
                    retorno = "janeiro";
                    break;
                    case 2:
                    retorno = "fevereiro";
                    break;
                    case 3:
                    retorno = "março";
                    break;
                    case 4:
                    retorno = "abril";
                    break;
                case 5:
                    retorno = "maio";
                    break;
                case 6:
                    retorno = "junho";
                    break;
                case 7:
                    retorno = "julho";
                    break;
                case 8:
                    retorno = "agosto";
                    break;
                case 9:
                    retorno = "setembro";
                    break;
                case 10:
                    retorno = "outubro";
                    break;
                case 11:
                    retorno = "novembro";
                    break;
                case 12:
                    retorno = "dezembro";
                    break;

            }
            return retorno;

        }
    }
}
