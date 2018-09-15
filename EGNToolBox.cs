using System;
using System.Collections.Generic;

namespace EGNToolBox
{
    internal class Region
    {
        public string Name { get; set; }
        public int StartRegion { get; set; }
        public int EndRegion { get; set; }
        public Region(string name, int start, int end)
        {
            Name = name;
            StartRegion = start;
            EndRegion = end;
        }
    }

    public class EGN
    {
        private readonly byte[] Egn = new byte[10];
        private readonly byte[] Weights = new byte[9] { 2, 4, 8, 5, 10, 9, 7, 3, 6 };

        public EGN(ulong egn)
        {
            if (egn < 9952319999)
            {

                for (int i = 9; i >= 0; i--)
                {
                    Egn[i] = (byte)(egn % 10);
                    egn /= 10;
                }
            }
            else
            {
                throw new ArgumentException("EGN(ulong egn) Параметърът е извън обхвата!");
            }
        }

        public EGN(string egnstr)
        {
            try
            {
                ulong egn = Convert.ToUInt64(egnstr);
                if (egn < 9952319999)
                {

                    for (int i = 9; i >= 0; i--)
                    {
                        Egn[i] = (byte)(egn % 10);
                        egn /= 10;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public int Year
        {
            get
            {
                int year = Egn[0] * 10 + Egn[1];
                int month = Egn[2] * 10 + Egn[3];
                if (month > 40)
                {
                    return year += 2000;
                }
                else if (month > 20)
                {
                    return year += 1800;
                }
                else
                {
                    return year += 1900;
                }
            }
        }

        public int Month
        {
            get
            {
                int month = Egn[2] * 10 + Egn[3];
                if (month > 40)
                {
                    return month -= 40;
                }
                else if (month > 20)
                {
                    return month -= 20;
                }
                else
                {
                    return month;
                }
            }
        }

        public int Day => Egn[4] * 10 + Egn[5];

        public int Region => Egn[6] * 100 + Egn[7] * 10 + Egn[8];

        public bool IsMale
        {
            get
            {
                if (Egn[8] % 2 != 0)
                {
                    return false;
                }
                return true;
            }
        }

        public ulong AsNumber
        {
            get
            {
                ulong result = 0;
                for (int i = 0; i < 10; i++)
                {
                    result += Egn[i];
                    result *= 10;
                }

                return result / 10;
            }
        }

        public string AsString => this.AsNumber.ToString();

        public bool IsValid()
        {
            DateTime testDate;
            try
            {
                testDate = new DateTime(this.Year, this.Month, this.Day);
            }
            catch
            {
                return false;
            }

            if (testDate > DateTime.Now)
            {
                return false;
            }

            int checksum = 0;
            for (int i = 0; i < 9; i++)
            {
                checksum += Egn[i] * Weights[i];
            }

            int remainder = checksum % 11;
            if (remainder == 10)
            {
                remainder = 0;
            }

            if (remainder != Egn[9])
            {
                return false;
            }

            return true;
        }

        public void FixChecksum()
        {
            if (this.IsValid())
            {
                return;
            }

            int checksum = 0;
            for (int i = 0; i < 9; i++)
            {
                checksum += Egn[i] * Weights[i];
            }

            int remainder = checksum % 11;
            if (remainder == 10)
            {
                remainder = 0;
            }
            Egn[9] = (byte)remainder;
        }
    }

    public class EGNTools
    {
        private static readonly Random random = new Random();
        private readonly string[] months = new string[12] {" януари ", " февруари ", " март ",
        " април ", " май ", " юни ", " юли ", " август ", " септевмри ", " октомври ", " ноември ", " декември "};
        internal readonly List<Region> regions;
        public EGNTools()
        {
            regions = new List<Region>
            {
                new Region("Благоевград", 0, 43),
                new Region("Бургас", 44, 93),
                new Region("Варна", 94, 139),
                new Region("Велико Търново", 140, 169),
                new Region("Видин", 170, 183),
                new Region("Враца", 184, 217),
                new Region("Габрово", 218, 233),
                new Region("Кърджали", 234, 281),
                new Region("Кюстендил", 282, 301),
                new Region("Ловеч", 302, 319),
                new Region("Монтана", 320, 341),
                new Region("Пазарджик", 342, 377),
                new Region("Перник", 378, 395),
                new Region("Плевен", 396, 435),
                new Region("Пловдив", 436, 501),
                new Region("Разград", 502, 527),
                new Region("Русе", 528, 555),
                new Region("Силистра", 556, 575),
                new Region("Сливен", 576, 601),
                new Region("Смолян", 602, 623),
                new Region("София - град", 624, 721),
                new Region("София - окръг", 722, 751),
                new Region("Стара Загора", 752, 789),
                new Region("Добрич", 790, 821),
                new Region("Търговище", 822, 843),
                new Region("Хасково", 844, 871),
                new Region("Шумен", 872, 903),
                new Region("Ямбол", 904, 925),
                new Region("Друг", 926, 999)
            };
        }

        public string Info(ulong egn)
        {
            try
            {
                var infoEGN = new EGN(egn);
                if (!infoEGN.IsValid())
                    return "Невалидно ЕГН";

                string result = "Лице, родено на ";

                result += infoEGN.Day.ToString() + months[infoEGN.Month - 1] + infoEGN.Year.ToString();

                bool found = false;
                int minIndex = 0;
                int maxIndex = 28;
                int queryIndex = 0;
                do
                {
                    queryIndex = (maxIndex + minIndex) / 2;
                    if (regions[queryIndex].StartRegion <= infoEGN.Region && regions[queryIndex].EndRegion >= infoEGN.Region)
                    {
                        result += " в регион " + regions[queryIndex].Name;
                        found = true;
                    }
                    else if (regions[queryIndex].StartRegion < infoEGN.Region)
                    {
                        minIndex = queryIndex;
                    }
                    else
                    {
                        maxIndex = queryIndex;
                    }
                } while (!found);

                if (infoEGN.IsMale)
                {
                    result += ", от мъжки пол,";
                }
                else
                {
                    result += ", от женски пол,";
                }

                int lastNumber = infoEGN.Region;
                if (infoEGN.IsMale)
                {
                    lastNumber--;
                }

                int burthNumber = (lastNumber - regions[queryIndex].StartRegion) / 2 + 1;

                result += $" родено {burthNumber} подред.";

                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string Info(string egn)
        {
            ulong egnUlong = 0;
            try
            {
                egnUlong = Convert.ToUInt64(egn);
            }
            catch (Exception)
            {
                return "Невалидно ЕГН!";
            }
            return Info(egnUlong);
        }

        public ulong Generate(DateTime date, bool sex, int region)
        {
            if (region < 0 && region > 28)
                return 0;

            int month = date.Month;
            if (date.Year >= 2000)
            {
                month += 40;
            }
            else if (date.Year < 1900)
            {
                month += 20;
            }
            ulong result = 0;

            result = Convert.ToUInt64(date.ToString("yy"));
            result *= 100;
            result += (ulong)month;
            result *= 100;
            result += (ulong)date.Day;
            result *= 1000;

            int regionNumber = 0;

            lock (random)
            {
                regionNumber = random.Next(regions[region].StartRegion, regions[region].EndRegion);
            }

            if (regionNumber % 2 != 0 && sex)
            {
                regionNumber--;
            }
            if (regionNumber % 2 == 0 && !sex)
            {
                regionNumber++;
            }
            result += (ulong)regionNumber;
            result *= 10;

            EGN resultEGN = new EGN(result);
            resultEGN.FixChecksum();

            if (resultEGN.IsValid())
                return resultEGN.AsNumber;

            return 0;
        }
    }
}
