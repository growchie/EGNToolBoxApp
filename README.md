# Проверка, генериране и извличане на информация от ЕГН, ЕИК и български IBAN

Този проект е набор от няколко класа които се занимават с верифициране, извличане на основна информация и генериране на ЕГН, ЕИК и IBAN номера. Проектът е разработен на база публично достъпна документация и описание на алгоритмите. *Верификацията се извършва __само__ алгоритмично и не е свързана с проверка към съответните бази на държавните учреждения!*

Библиотеката може да се ползва като нормална референция, така и като [NuGet](https://www.nuget.org/packages/VerificationToolBox/) пакет.

## ЕГН

Като база е използвана имплементацията на [Георги Чорбаджийски](https://georgi.unixsol.org/programs/egn.php?a=gen&s=0&d=0&m=0&y=0&n=5&r=0), както и информацията налична в българската версия на [Wikipedia](https://bg.wikipedia.org/wiki/Единен_граждански_номер). Кодовете за регионите са от Георги Чорбаджийски.

###  Структура на модула

Неймспейсът на модула е `EGNToolBox`. Потебителските класове са два `EGN` и `EGNTools`. `EGN` основно съдържа смият граждански номер, функция за верификация пропъртита за отделните елементи. `EGNTools` предоставя функции за извличане на информация и генериране на граждански номера.

## ЕИК

Модулът предоставя възможност за извършване на проверка на валидността на ЕИК на фирми и учреждения с 9 и 13 цифри.

### Структура на модула

Немйспейсът на модула е `EIKToolBox`. Към момента не е имплементирана възмоност за генериране на ЕИК поради липса на данни за регионите за разпределяне на номерата. Поради тази причина използваме е само клас `EIK` който предоставя възможност за верификация на номера.

## Български IBAN

Модулът предоставя възможност за верификация, извличане на данни за банката или финансовата институция към която принадлежи IBAN номерът, както и за генерирането на произволен такъв. Информацията за актуалните финансови институции е от регистъра на БАЕ кодовете от сайта на [БНБ](http://www.bnb.bg/RegistersAndServices/RSBAEAndBIC/index.htm). Информацията е конвертирана до _json_ формат. Съхранява се в `banks.json` от хранилището и е препоръчително да се актуализира до последната достъпна версия.

Алгоритъмът за генериране и верификация е от [Wikipedia](https://en.wikipedia.org/wiki/International_Bank_Account_Number).

### Структура на модула

Нейспейсът на модула е `IBANToolBox`. Публичните класове са `IBAN`, където се съхранява самият номер и се предоставя функция за верифициране и `IBANTools` с функции за извличане на информация за банковата принадлежност и генериране на тестов номер.

___

Генерирането на IBAN номерата е отговорност на всяка финансова институция, поради което не е гарантирано, че проверката би гарантирала успешно верифициране от банката, издала номера. IBAN стандартът предвижда 10 числа за сметка, но по наблюдения в България се използват реално 8 от тях. Случайните числа, генерирани от модула са в диапазона:
`````
string accountNumber = "00" + random.Next(1111, 99999999).ToString("00000000");
``````
При желания може да се варира с обхватът на `random` функцията.