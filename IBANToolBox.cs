using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace VerificationToolBox
{
    internal class BgBank
    {
        public string name { get; set; }
        public string BAE { get; set; }
        public string BIC { get; set; }
    }

    public class IBAN
    {
        private readonly string _IBAN;
        public string AsString => _IBAN;

        public IBAN(string iban)
        {
            if (iban.Length < 16 || iban.Length > 34)
            {
                throw new ArgumentException("Невалиден IBAN!");
            }
            _IBAN = iban.ToUpper();
        }

        public bool IsBulgarian()
        {
            if (_IBAN.Length != 22 && _IBAN.Substring(0, 2) != "BG")
            {
                return false;
            }
            return true;
        }

        public bool IsValid()
        {
            string inverted = _IBAN.Substring(4, 18) + _IBAN.Substring(0, 4);
            string converted = "";
            for (int i = 0; i < inverted.Length; i++)
            {
                if (inverted[i] >= 'A' && inverted[i] <= 'Z')
                {
                    uint substr = Convert.ToUInt32(inverted[i]) - 55;
                    converted += substr.ToString();
                }
                else
                {
                    converted += inverted[i];
                }
            }

            int segstart = 0;
            int step = 9;
            string prepended = "";
            ulong number = 0;
            while (segstart < converted.Length - step)
            {
                number = Convert.ToUInt64(prepended + converted.Substring(segstart, step));
                ulong remainder = number % 97;
                prepended = remainder.ToString();
                if (remainder < 10)
                {
                    prepended = "0" + prepended;
                }
                segstart = segstart + step;
                step = 7;
            }

            number = Convert.ToUInt64(prepended + converted.Substring(segstart));

            ulong result = number % 97;
            if (result != 1)
            {
                return false;
            }

            return true;
        }

        public string BAE => _IBAN.Substring(4, 8);
        public string CCODE => _IBAN.Substring(0, 2);
    }

    public class IBANToolBox
    {
        private static readonly Random random = new Random();
        private readonly List<BgBank> bgBanks;
        private readonly string BankValidDate;
        private readonly List<BgBank> bgOther;
        private readonly string OtherValidDate;

        public IBANToolBox()
        {
            try
                {
                using (StreamReader r = new StreamReader("banks.json"))
                {
                    string json = r.ReadToEnd();
                    var jsonParse = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Object>>>(json);
                    BankValidDate = jsonParse["banks"]["date"].ToString();
                    bgBanks = JsonConvert.DeserializeObject<List<BgBank>>(jsonParse["banks"]["data"].ToString());
                    OtherValidDate = jsonParse["others"]["date"].ToString();
                    bgOther = JsonConvert.DeserializeObject<List<BgBank>>(jsonParse["others"]["data"].ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("IBANToolBox() проблем с обраработването на banks.json!");
                throw e;
            }
        }

        public string Info(string _iban)
        {
            return this.Info(new IBAN(_iban));
        }

        public string Info(IBAN _iban)
        {
            
            if (!_iban.IsBulgarian())
            {
                return $"Не се поддържа информация за IBAN с държавен код {_iban.CCODE}";
            }
            else if (!_iban.IsValid())
            {
                return "Невалиден български IBAN!";
            }

            var query =
                from _name in bgBanks
                where _name.BAE == _iban.BAE
                select _name;

            bool bank = true;

            if (!query.Any())
            {
                bank = false;
                query =
                    from _name in bgOther
                    where _name.BAE == _iban.BAE
                    select _name;

                if (!query.Any())
                    return "Липсва информация за такава банка!";
            }
            if (bank)
            {
                return $"Име на банка: {query.FirstOrDefault().name} с BAE: {query.FirstOrDefault().BAE} и BIC: {query.FirstOrDefault().BIC}." +
                    $" Последна актуализация: {BankValidDate}";
            }
            return $"Име на друга финансова институция: {query.FirstOrDefault().name} с BAE: {query.FirstOrDefault().BAE}" +
                $"и BIC: {query.FirstOrDefault().BIC}. Последна актуализация: {OtherValidDate}";
        }

        public Dictionary<int, string> AvailableBanks()
        {
            var result = new Dictionary<int, string>();
            int index = 0;
            foreach (var item in bgBanks)
            {
                result.Add(index, item.name);
                index++;
            }
            return result;
        }

        public string GenerateAsString(int bankIndex = -1)
        {
            return this.Genearate(bankIndex).AsString;
        }

        public IBAN Genearate(int bankIndex = -1)
        {
            if (bankIndex == -1)
            {
                bankIndex = random.Next(0, bgBanks.Count - 1);
            }

            string accountNumber = "00" + random.Next(1111, 99999999).ToString("00000000");
            string result = bgBanks[bankIndex].BAE + accountNumber +"BG00";
            string numbered = "";

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] >= 'A' && result[i] <= 'Z')
                {
                    uint substr = Convert.ToUInt32(result[i]) - 55;
                    numbered += substr.ToString();
                }
                else
                {
                    numbered += result[i];
                }
            }

            int segstart = 0;
            int step = 9;
            string prepended = "";
            ulong number = 0;
            while (segstart < numbered.Length - step)
            {
                number = Convert.ToUInt64(prepended + numbered.Substring(segstart, step));
                ulong remainder = number % 97;
                prepended = remainder.ToString();
                if (remainder < 10)
                {
                    prepended = "0" + prepended;
                }
                segstart = segstart + step;
                step = 7;
            }

            number = Convert.ToUInt64(prepended + numbered.Substring(segstart));

            ulong checkDigit = 98 - (number % 97);

            result = "BG" + checkDigit.ToString("00") + bgBanks[bankIndex].BAE + accountNumber;

            return new IBAN(result);
        }
    }
}
