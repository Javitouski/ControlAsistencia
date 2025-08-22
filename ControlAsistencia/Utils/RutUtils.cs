using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Utils
{
    public static class RutUtils
    {
        // Normaliza: quita puntos/espacios, pone DV en mayúscula, asegura el guión.
        public static string Normalize(string rutInput)
        {
            if (string.IsNullOrWhiteSpace(rutInput)) throw new ArgumentException("RUT vacío.");
            var raw = new string(rutInput.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpperInvariant();

            // Debe tener al menos 2 caracteres (num + DV)
            if (raw.Length < 2) throw new ArgumentException("RUT inválido.");

            var body = raw[..^1];
            var dv = raw[^1];

            // Reinsertar guión
            return $"{body}-{dv}";
        }

        // Valida el dígito verificador (módulo 11)
        public static bool IsValid(string rutNormalized)
        {
            if (string.IsNullOrWhiteSpace(rutNormalized)) return false;
            rutNormalized = rutNormalized.Replace(".", "").ToUpperInvariant();
            var parts = rutNormalized.Split('-');
            if (parts.Length != 2) return false;

            var bodyStr = parts[0];
            var dvStr = parts[1];
            if (!long.TryParse(bodyStr, out var body) || body <= 0) return false;

            char dvCalc = CalcDv(body);
            return dvStr == dvCalc.ToString();
        }

        private static char CalcDv(long body)
        {
            int[] seq = { 2, 3, 4, 5, 6, 7 };
            int sum = 0; int i = 0;
            foreach (var ch in body.ToString().Reverse())
            {
                sum += (ch - '0') * seq[i++ % seq.Length];
            }
            int r = 11 - (sum % 11);
            if (r == 11) return '0';
            if (r == 10) return 'K';
            return r.ToString()[0];
        }

        // Intenta normalizar y valida; si es correcto, entrega la versión normalizada.
        public static bool TryNormalizeValid(string input, out string normalized)
        {
            normalized = string.Empty;
            try
            {
                var n = Normalize(input);
                if (!IsValid(n)) return false;
                normalized = n;
                return true;
            }
            catch { return false; }
        }
    }
}

