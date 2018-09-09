using System;
using System.Collections.Generic;
using System.Text;

namespace VerificationToolBox
{
    public class EIK
    {
        private readonly byte[] Eik13 = new byte[13];
        private readonly byte[] Eik09 = new byte[9];
        public bool Is13 { get; }

        public EIK(ulong eik)
        {
            if (eik > 9999999999999)
            {
                throw new ArgumentException("Параметърът е извън обхвата!");
            }
            else if (eik < 999999999)
            {
                Is13 = false;
                for (int i = 8; i >= 0; i--)
                {
                    Eik09[i] = (byte)(eik % 10);
                    eik /= 10;
                }
            }
            else
            {
                Is13 = true;
                for (int i = 12; i >= 0; i--)
                {
                    Eik13[i] = (byte)(eik % 10);
                    eik /= 10;
                }
            }
        }

        public EIK(string eik)
        {
            ulong eikUlong = 0;
            try
            {
                eikUlong = Convert.ToUInt64(eik);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            if (eik.Length != 9 && eik.Length != 13)
            {
                throw new ArgumentException("Параметърът е извън обхвата!");
            }
            else if (eik.Length == 9)
            {
                Is13 = false;
                for (int i = 8; i >= 0; i--)
                {
                    Eik09[i] = (byte)(eikUlong % 10);
                    eikUlong /= 10;
                }
            }
            else
            {
                Is13 = true;
                for (int i = 12; i >= 0; i--)
                {
                    Eik13[i] = (byte)(eikUlong % 10);
                    eikUlong /= 10;
                }
            }
        }

        public bool IsValid()
        {
            int result = 0;
            int checksum = 0;
            if (!this.Is13)
            {
                for (int i = 0; i < 8; i++)
                {
                    result += (i + 1) * Eik09[i];
                }

                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        result += (i + 3) * Eik09[i];
                    }

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }
                if (Eik09[8] != checksum)
                {
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    result += (i + 1) * Eik13[i];
                }

                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        result += (i + 3) * Eik13[i];
                    }

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }
                if (Eik13[8] != checksum)
                {
                    return false;
                }

                result = 2 * Eik13[8];
                result += 7 * Eik13[9];
                result += 3 * Eik13[10];
                result += 5 * Eik13[11];
                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 4 * Eik13[8];
                    result += 9 * Eik13[9];
                    result += 5 * Eik13[10];
                    result += 7 * Eik13[11];
                    checksum = result % 11;

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }

                if (Eik13[12] != checksum)
                {
                    return false;
                }
            }

            return true;
        }

        public void FixChecksum()
        {
            if (this.IsValid())
            {
                return;
            }

            int result = 0;
            int checksum = 0;

            if (!this.Is13)
            {
                for (int i = 0; i < 8; i++)
                {
                    result += (i + 1) * Eik09[i];
                }

                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        result += (i + 3) * Eik09[i];
                    }

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }
                Eik09[8] = (byte)checksum;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    result += (i + 1) * Eik13[i];
                }

                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        result += (i + 3) * Eik13[i];
                    }

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }

                Eik13[8] = (byte)checksum;
                
                result = 2 * Eik13[8];
                result += 7 * Eik13[9];
                result += 3 * Eik13[10];
                result += 5 * Eik13[11];
                checksum = result % 11;

                if (checksum >= 10)
                {
                    result = 4 * Eik13[8];
                    result += 9 * Eik13[9];
                    result += 5 * Eik13[10];
                    result += 7 * Eik13[11];
                    checksum = result % 11;

                    if (checksum >= 10)
                    {
                        checksum = 0;
                    }
                }

                Eik13[12] = (byte)checksum;
            }
        }
        
    }

    class EIKToolBox
    {
    }

    public class IBAN
    {
        private readonly string _IBAN;

        public IBAN(string iban)
        {
            if (iban.Length != 22)
            {
                throw new ArgumentException("Невалиден български IBAN!");
            }
            _IBAN = iban;
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
    }
}
