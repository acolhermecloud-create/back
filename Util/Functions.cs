using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Util
{
    public static class Functions
    {
        public static string ReplaceVariable(string htmlDoc, string variableName, string replacement)
        {
            // Expressão regular para encontrar a variável no HTML
            string pattern = "%" + Regex.Escape(variableName) + "%";

            // Substitui a variável pelo valor de substituição
            string substitutedHtmlDoc = Regex.Replace(htmlDoc, pattern, replacement);

            return substitutedHtmlDoc;
        }

        public static bool ValidateEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateTimeStampStrUnique()
        {
            DateTime now = DateTime.UtcNow; // obtém a data e hora atual em UTC
            DateTime unixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = now - unixEpoch;

            string timeStamp = timeSpan.TotalSeconds.ToString();
            string numericOnly = new string(timeStamp.Where(char.IsDigit).ToArray());

            return numericOnly;
        }

        public static bool ValidateDocument(string document)
        {
            string cleanDocument = Regex.Replace(document, @"[^\d]+", "");

            if (cleanDocument.Length == 11)
            {
                // Validate CPF
                string cpf = cleanDocument;
                if (cpf.Length != 11 ||
                    cpf == "00000000000" ||
                    cpf == "11111111111" ||
                    cpf == "22222222222" ||
                    cpf == "33333333333" ||
                    cpf == "44444444444" ||
                    cpf == "55555555555" ||
                    cpf == "66666666666" ||
                    cpf == "77777777777" ||
                    cpf == "88888888888" ||
                    cpf == "99999999999")
                    return false;

                int add = 0;
                for (int i = 0; i < 9; i++)
                    add += int.Parse(cpf[i].ToString()) * (10 - i);

                int rev = 11 - (add % 11);
                if (rev == 10 || rev == 11) rev = 0;
                if (rev != int.Parse(cpf[9].ToString())) return false;

                add = 0;
                for (int i = 0; i < 10; i++)
                    add += int.Parse(cpf[i].ToString()) * (11 - i);

                rev = 11 - (add % 11);
                if (rev == 10 || rev == 11) rev = 0;
                if (rev != int.Parse(cpf[10].ToString())) return false;

                return true;
            }
            else if (cleanDocument.Length == 14)
            {
                // Validate CNPJ
                string cnpj = cleanDocument;
                if (cnpj.Length != 14) return false;

                int tamanho = cnpj.Length - 2;
                string numeros = cnpj.Substring(0, tamanho);
                string digitos = cnpj.Substring(tamanho);
                int soma = 0;
                int pos = tamanho - 7;
                for (int i = tamanho; i >= 1; i--)
                {
                    soma += int.Parse(numeros[tamanho - i].ToString()) * pos--;
                    if (pos < 2) pos = 9;
                }
                int resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
                if (resultado != int.Parse(digitos[0].ToString())) return false;

                tamanho += 1;
                numeros = cnpj.Substring(0, tamanho);
                soma = 0;
                pos = tamanho - 7;
                for (int i = tamanho; i >= 1; i--)
                {
                    soma += int.Parse(numeros[tamanho - i].ToString()) * pos--;
                    if (pos < 2) pos = 9;
                }
                resultado = soma % 11 < 2 ? 0 : 11 - soma % 11;
                if (resultado != int.Parse(digitos[1].ToString())) return false;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
