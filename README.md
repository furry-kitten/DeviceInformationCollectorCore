ОПИСАНИЕ
===

Данный репазиторий содержит 2 проекта позволяющих определять минимальное количество информации о подключённых по usb устройств. Работает только под виндой, так как использует виндовые библиотеки.

На данный момент приложение позволяет получить от устройств Id, модель, номер модели, мануфактурера. Всё это получается с помощью стандартных инструментов Windows. Всю предоставляемую информацию можно найти руками в диспетчере устройств. Подробная информация в пропертях. 

Для запуска понадобиться один проект на .Net 5.0, любой другой может быть любой платформы. Для работы необходимо в проекте с целевой платформой .Net 5.0  создать экземпляр класса ```CollectorUsbDiFacade```, представленный в проекте [UsbDeviceInformationCollectorCore][collector]. Другие проекты могут только плучить ссылку получаемемую через интерфейс представленный в проекте [DeviceSearcherGate][gate]. Чтобы всё работало хорошо достаточно создать класс конфигурации и передать его в конструктор, в таком случае данные могут быть получены при запуске полной перезаписи (пидальный привод) и запросом всех устройств, но не будет возможности получить информацию по [хабам][term].

Для получаения возможности реагировать на подключения утройств, надо передать хук в предоставляемый делегат, пример создания хука ищи в [примерах][примеры]. Также нужно будет передать дискриптор окна, как его получить описано в документации Microsoft ["Получение дескриптора окна (HWND)"][HwndMsDoc]. 

Чтобы ипользовать весь предоставляемый функционал, нужно дополнительно прокинуть подписки, на интересующую подписку: изменение коллекции хабов, изменение коллекции [нужных устройств][term] и изменение коллекции остальных устройств. В таком случае информаци о подключеных устройства будет выводиться через секнду после первых изменений состояний устройства. Для пример возьмём телефон и iPad. Допустим, мы подключим сначала samsung, подключение является изменением состояния устройства для компьютера (он подключён), в этот момент компьютер начинает создавать все нужные нам свойства (другимим словами классы). Как правило, 1 секунды достаточно, чтобы комьютер выполнил все необходимые действия, после чего начинает отрабатывать наш collector. Через секунду после того, как телефон был подключен к компьтеру, коллектор кинет сообщение об изменении одной из коллекций. Куда телефон опредлиться, мы можем узнать исходя из того, каким образом тут был подключен. Если телефон подключен с найтройками _передачи файлов (MTP)_, он придёт в коллекию нужных устройств, если телефон будет подключен с настройками _передачи фото PTP_, телефон отправится в коллекцию остальных устройст, при этом у него будет определён Id, будет иметься информация, но у него будет свойство MEDIA, поэтому он там и окажется. В ту же коллекцию телефон при настройках _передачи и прослушивания музыки (MIDI)_. В режиме _зарятки этого устройства_ некоторые устройства могут попать в одну из этих коллекций, это правило касается только андроидов, так как у яблочных таких настроек нет. Каждое переключение режима для компьютера выглядит как отключение и подключение одного и того же телефона. Если вслючить настройку USB-Debug, но телефон не будет прыгать из одной коллекции в другую, к нему просто добавиться ещё одно свойство. Пример подключенного [Samsung][samsung] с включенным usb- есть в [примерах][примеры]

Теперь рассмотрим подключение iPad. Так как у яблочных нет никаких настроек подключения по usb, девайс всегда будет попадать в одну коллекцию (в коллекцию нужных), это касается всех iPhone и iPad. В нашем случае iPad выдал 3 свойства, 2 из которых носят одинаковое название, но фактически носят разную информацию и разный смылс. Дело в том, что класс _USBDevice_ может считаться идентичным классу _USB_ и классу _AndroidUsbDeviceClass_. По факту можно заметить, что они похожи но только 50/50, где половиня взята от одного класса и половина от другого. Также можно увидеть, что iPad не любит отдавать свою модель и номер модели.

Больше всего информации о себе выдают андроиды. Чтобы убедиться, что телефон подключен правильно, нужно заглянуть в класс wpd, этот класс есть у всех адроидов и должен быть у всех яболочных иделий, но порой его нет, значит, телефон подключен с не птеми настройками, если это так и вы предполагаете работать с устройством, например, выполнять какие-то команды на нём, то без wpd класса у Вас этого не получится. Так же этот класс позволяет нам понять с какими настройками подключено устройство. Что бы понять, какие настройки в данный момент включены на устройстве, нужно знать сокращения типа **MTP**, **PTP**, **MIDI**. Если с яблочками нам это не важно, то с анроидами это очень важно и для андроидов дожно быть написано **MTP**, иначе вы несможите на нём ничего выполнять. Также надо помнить про Usb-Debug, если он включен, у андроидов как правило будет появляться класс _AndroidUsbDeviceClass_, но иногда (некоторые) устройства вместо этого класса получают _USBDevice_, пока не известно почему так, но в целом это не влияет получаемую информацию и возможность работать с устройством так что можно просто забыть об этом. Главное держать в голове, что от андроида к андроиду картина устройства будет маеняться, вероятней всего на это влияет версия андроида, но это пока не точно.

---
# Оглавление
- [Описание][characters]
- [Термины][term]
- [Примеры][примеры]
  - [Свойства][свойства]
  - [Девайс][device]
  - [Создание хука][hook]
- [Для разработчика][fordeveloper]
- [Полезные ссылки][links]

---

## Термины

Устройство/девайс
: *Любое подключаемое по usb устройство, которое может передать компьютеру какую-либо информацию о себе (и не только). Устройством можно считать некоторые выключенные телефоны, если они могут передать о себе хотя бы что-нибудь (хотя бы один виндовый класс).*

Нужные устройства
: *Это телефоны или планшеты. Подобные устройства записываются в список, если у них есть id и не имеют при себе свойства типа HIDClass, MEDIA или неопределённый (такое может быть, если мы получим незнакомый нам тип свойства)*

Хаб/Hub
: *Это логический хаб, являющийся целым физическим хабом или частью физического хаба. Как правило в одном логическом хабе 4 порта, в один из которых может быть подключён другой логический хаб. В роле хаба также представляется usb шина компьютера, она определяется путём как в [примере](#hub)*

Проперти/свойства устройства
: *Экземпляр класса ```DeviceProperties``` и представляет собой один из виндовых классов __(WPD, AndroidUsbDeviceClass, USB, Modem, USBDevice, HIDClass, MEDIA)__, которые представляют некоторую информацию об устройстве. Информация от класса к классу может меняться.*

## Примеры
### Свойства
##### WDP

```json
{
    "PnpClassesTypes": 2,
    "Address": "\u0004",
    "ComPort": null,
    "Description": "SM-N920P",
    "FriendlyName": "Galaxy Note5",
    "Location": null,
    "Manufacturer": "Samsung Electronics Co., Ltd.",
    "ShortPath": "UsbHub(18)#USB(4)",
    "Type": null,
    "BusReportedDeviceDesc": "MTP",
    "Class": "WPD",
    "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400&MS_COMP_MTP&SAMSUNG_Android",
    "Id": "USB\\VID_04E8&PID_6860&MS_COMP_MTP&SAMSUNG_ANDROID\\8&2B829510&0&0000",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)#USB(4)",
    "PhysicalObjectName": "\\Device\\0000093e",
    "IsRemoved": false
}
```

##### Modem

```json
{
    "PnpClassesTypes": 16,
    "Address": "\u0004",
    "ComPort": "COM9",
    "Description": "SAMSUNG Mobile USB Modem",
    "FriendlyName": "SAMSUNG Mobile USB Modem #6",
    "Location": null,
    "Manufacturer": "SAMSUNG Electronics Co., Ltd. ",
    "ShortPath": "UsbHub(18)#USB(4)",
    "Type": null,
    "BusReportedDeviceDesc": "CDC Abstract Control Model (ACM)",
    "Class": "Modem",
    "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400&Modem",
    "Id": "USB\\VID_04E8&PID_6860&MODEM\\8&2B829510&0&0001",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)#USB(4)",
    "PhysicalObjectName": "\\Device\\0000093f",
    "IsRemoved": false
}
```

##### USB

```json
{
    "PnpClassesTypes": 8,
    "Address": "\u0004",
    "ComPort": null,
    "Description": "SAMSUNG Mobile USB Composite Device ",
    "FriendlyName": null,
    "Location": "Port_#0004.Hub_#0018",
    "Manufacturer": "SAMSUNG Electronics Co., Ltd. ",
    "ShortPath": "UsbHub(18)#USB(4)",
    "Type": null,
    "BusReportedDeviceDesc": "SAMSUNG_Android",
    "Class": "USB",
    "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400",
    "Id": "USB\\VID_04E8&PID_6860\\06157DF62983CD14",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)",
    "PhysicalObjectName": "\\Device\\USBPDO-22",
    "IsRemoved": false
}
```

##### USBDevice

```json
{
    "PnpClassesTypes": 32,
    "Address": "\u0004",
    "ComPort": null,
    "Description": "Устройство ADB",
    "FriendlyName": "ADB Interface",
    "Location": "0000.0014.0000.002.003.003.003.000.000",
    "Manufacturer": "Устройство WinUSB",
    "ShortPath": "UsbHub(7)#USB(3)",
    "Type": null,
    "BusReportedDeviceDesc": "ADB Interface",
    "Class": "USBDevice",
    "HardwareId": "USB\\VID_1004&PID_631D&REV_0318&MI_01",
    "Id": "USB\\VID_1004&PID_631D&MI_01\\6&CDC5A24&0&0001",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(2)#USB(3)#USB(3)#USB(3)#USBMI(1)",
    "PhysicalObjectName": "\\Device\\0000005c",
    "IsRemoved": false
}
```

##### HIDClass

```json
{
    "PnpClassesTypes": 64,
    "Address": "\u0006",
    "ComPort": null,
    "Description": "USB-устройство ввода",
    "FriendlyName": null,
    "Location": "0000.0014.0000.005.000.000.000.000.000",
    "Manufacturer": "(Стандартные системные устройства)",
    "ShortPath": "UsbHub(1)#USB(5)",
    "Type": null,
    "BusReportedDeviceDesc": "USB Keyboard",
    "Class": "HIDClass",
    "HardwareId": "USB\\VID_046D&PID_C31C&REV_4900&MI_01",
    "Id": "USB\\VID_046D&PID_C31C&MI_01\\6&15C398B7&0&0001",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(5)#USBMI(1)",
    "PhysicalObjectName": "\\Device\\0000004c",
    "IsRemoved": false
}
```

##### Hub

```json
{
    "PnpClassesTypes": 8,
    "Address": null,
    "ComPort": null,
    "Description": null,
    "FriendlyName": null,
    "Location": null,
    "Manufacturer": null,
    "ShortPath": null,
    "Type": null,
    "BusReportedDeviceDesc": null,
    "Class": "USB",
    "HardwareId": "USB\\ROOT_HUB30\\4&CDBAEC3&0&0",
    "Id": "1",
    "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)",
    "PhysicalObjectName": "\\Device\\USBPDO-0",
    "IsRemoved": false
}
```
---

### Девайс
##### Samsung

```json
[
    {
        "Properties": [
            {
                "PnpClassesTypes": 4,
                "Address": "\u0004",
                "ComPort": null,
                "Description": "SAMSUNG Android ADB Interface",
                "FriendlyName": null,
                "Location": "Port_#0004.Hub_#0018",
                "Manufacturer": "SAMSUNG Electronics Co., Ltd. ",
                "ShortPath": "UsbHub(18)#USB(4)",
                "Type": null,
                "BusReportedDeviceDesc": "SAMSUNG_Android",
                "Class": "AndroidUsbDeviceClass",
                "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400&ADB",
                "Id": "USB\\VID_04E8&PID_6860&ADB\\8&2B829510&0&0003",
                "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)#USB(4)",
                "PhysicalObjectName": "\\Device\\00000940",
                "IsRemoved": false
            },
            {
                "PnpClassesTypes": 2,
                "Address": "\u0004",
                "ComPort": null,
                "Description": "SM-N920P",
                "FriendlyName": "Galaxy Note5",
                "Location": null,
                "Manufacturer": "Samsung Electronics Co., Ltd.",
                "ShortPath": "UsbHub(18)#USB(4)",
                "Type": null,
                "BusReportedDeviceDesc": "MTP",
                "Class": "WPD",
                "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400&MS_COMP_MTP&SAMSUNG_Android",
                "Id": "USB\\VID_04E8&PID_6860&MS_COMP_MTP&SAMSUNG_ANDROID\\8&2B829510&0&0000",
                "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)#USB(4)",
                "PhysicalObjectName": "\\Device\\0000093e",
                "IsRemoved": false
            },
            {
                "PnpClassesTypes": 16,
                "Address": "\u0004",
                "ComPort": "COM9",
                "Description": "SAMSUNG Mobile USB Modem",
                "FriendlyName": "SAMSUNG Mobile USB Modem #6",
                "Location": null,
                "Manufacturer": "SAMSUNG Electronics Co., Ltd. ",
                "ShortPath": "UsbHub(18)#USB(4)",
                "Type": null,
                "BusReportedDeviceDesc": "CDC Abstract Control Model (ACM)",
                "Class": "Modem",
                "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400&Modem",
                "Id": "USB\\VID_04E8&PID_6860&MODEM\\8&2B829510&0&0001",
                "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)#USB(4)",
                "PhysicalObjectName": "\\Device\\0000093f",
                "IsRemoved": false
            },
            {
                "PnpClassesTypes": 8,
                "Address": "\u0004",
                "ComPort": null,
                "Description": "SAMSUNG Mobile USB Composite Device ",
                "FriendlyName": null,
                "Location": "Port_#0004.Hub_#0018",
                "Manufacturer": "SAMSUNG Electronics Co., Ltd. ",
                "ShortPath": "UsbHub(18)#USB(4)",
                "Type": null,
                "BusReportedDeviceDesc": "SAMSUNG_Android",
                "Class": "USB",
                "HardwareId": "USB\\VID_04E8&PID_6860&REV_0400",
                "Id": "USB\\VID_04E8&PID_6860\\06157DF62983CD14",
                "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(3)#USB(4)#USB(4)",
                "PhysicalObjectName": "\\Device\\USBPDO-22",
                "IsRemoved": false
            }
        ],
        "ComPort": "COM9",
        "Id": "06157DF62983CD14",
        "Manufacture": "Samsung Electronics Co., Ltd.",
        "ModelName": "Galaxy Note5",
        "ModelNumber": "SM-N920P",
        "ShortPath": "UsbHub(18)#USB(4)",
        "IsRemoved": false
    }
]
```

##### Apple Ipad

```json
{
    "Properties": [
        {
            "PnpClassesTypes": 32,
            "Address": "\u000f",
            "ComPort": null,
            "Description": "Apple Mobile Device USB Device",
            "FriendlyName": "Apple Mobile Device USB Device",
            "Location": "0000.0014.0000.014.000.000.000.000.000",
            "Manufacturer": "Apple, Inc.",
            "ShortPath": "UsbHub(1)#USB(14)",
            "Type": null,
            "BusReportedDeviceDesc": "Apple USB Multiplexor",
            "Class": "USBDevice",
            "HardwareId": "USB\\VID_05AC&PID_12AB&REV_0404&MI_01",
            "Id": "USB\\VID_05AC&PID_12AB&MI_01\\9&1872417B&0&0001",
            "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(14)#USBMI(1)",
            "PhysicalObjectName": "\\Device\\00000300",
            "IsRemoved": false
        },
        {
            "PnpClassesTypes": 32,
            "Address": "\u000e",
            "ComPort": null,
            "Description": "Apple Mobile Device USB Composite Device",
            "FriendlyName": "Apple Mobile Device USB Composite Device",
            "Location": "Port_#0014.Hub_#0001",
            "Manufacturer": "Apple, Inc.",
            "ShortPath": "UsbHub(1)#USB(14)",
            "Type": null,
            "BusReportedDeviceDesc": "iPad",
            "Class": "USBDevice",
            "HardwareId": "USB\\VID_05AC&PID_12AB&REV_0404",
            "Id": "USB\\VID_05AC&PID_12AB\\77B9BB897FC4CBA023FD9A4D393013C03644D436",
            "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(14)",
            "PhysicalObjectName": "\\Device\\USBPDO-1",
            "IsRemoved": false
        },
        {
            "PnpClassesTypes": 2,
            "Address": "\u000e",
            "ComPort": null,
            "Description": "Apple iPad",
            "FriendlyName": "Apple iPad",
            "Location": "0000.0014.0000.014.000.000.000.000.000",
            "Manufacturer": "Apple Inc.",
            "ShortPath": "UsbHub(1)#USB(14)",
            "Type": null,
            "BusReportedDeviceDesc": "PTP",
            "Class": "WPD",
            "HardwareId": "USB\\VID_05AC&PID_12AB&REV_0404&MI_00",
            "Id": "USB\\VID_05AC&PID_12AB&MI_00\\9&1872417B&0&0000",
            "Path": "PCIROOT(0)#PCI(1400)#USBROOT(0)#USB(14)#USBMI(0)",
            "PhysicalObjectName": "\\Device\\000002ff",
            "IsRemoved": false
        }
    ],
    "ComPort": null,
    "Id": "77B9BB897FC4CBA023FD9A4D393013C03644D436",
    "Manufacture": "Apple Inc.",
    "ModelName": "Apple iPad",
    "ModelNumber": "iPad",
    "ShortPath": "UsbHub(1)#USB(14)",
    "IsRemoved": false
}
```

### Создание хука

```cs
//  Пример выполнен в WPF
private static Action<WndProcDelegate> AddHook(Window mainWindow)
{
    _addHook = wndProc =>
    {
        _source = (HwndSource)PresentationSource.FromVisual(mainWindow);
        _hook = new HwndSourceHook(wndProc);
        _source?.AddHook(_hook);
    };
    return _addHook;
}
```

---

# Для разработчиков

Информация обовсех класса описана отдельно в каждом проекте ([DeviceSearcherGate][gate], [UsbDeviceInformationCollectorCore][collector]). 

## Задачи

- [ ] Определять точно, что подключили телефон или другое устройство
- [ ] Сделать поле у устройства, что будет содержать в себе все pnp-классы. Они реализованы флагами, это может ещё усткорить работу по проверке, есть ли такое свойство у устройства.
- [ ] Использовать дополнительные источники данных для получения более точной и правильной информации. Речь идёт о путях, что начинаются на SWD. Некоторые из них относятся к usb устройствам, могут содержать больше полезной информации.
- [ ] Отладить работу метода _FullResetUsbDevices_ класса ```DeviceManager```. Некорректно отрабатывает, если вызвать во время работы решения.

### Пожелания

:bulb: Научиться работать с полем DeviceLoaction. Это поле содержить точную информацию о том, куда подключено устройство, тогда можно будет избавляться от ShortPath.

:bulb: Собрать хабы в едный физический хаб, это поволит быстрее работать с удалением устройств, при удалении хаба.

:bulb: Изменить сбор информации по устройству. Изначально надо получать путь к устройству, а потом отбирать все устройства с таким путём, предварительно преобразовав его в ShortPath, это может позволить лишний раз не читать свойства устройств, что уже были добавлены в пул или уже добавляются. Для коротки путей надо будет создать hashset.

:bulb: Добавить в обработку свойств асинхронность, получаение всех свойств должно быть асинхронным и обработка свойств тоже, синхронизация нужна будет только на стадии добавления. В асинхронном чтении свойств может быть спрятан мелкий баг.

## Полезные ссылки

[Pinvoke][pinvoke] - сайт содержит минимальную информацию по большинству методов всех библиотек, что на нём описаны.

[UsbTreeView][usbtreeview] - приложение позволяющее увидеть всю информацию о подключённых usb устройствах к компьютеру, увидет хабы, но не единым устройством, а отдельными хабами по 4 порта каждый. Также есть много интересного на сайте [NirSoft][nirsoft]

[Получение дескриптора окна (HWND)][HwndMsDoc] - как получить дискриптор окна, для работы приложения

[SafeHandleZeroOrMinusOneIsInvalid][SafeHandleZeroOrMinusOneIsInvalid] - класс родитель для корректного отлавливания событий.

[properties]: #Пропертисвойства-устройства
[gate]: DeviceSearcherGate/ReadMe.md
[collector]: UsbDeviceInformationCollectorCore/README.md
[device]: #Устройстводевайс
[hook]: #Создание-хука
[pinvoke]: https://www.pinvoke.net/default.aspx
[usbtreeview]: https://www.net-usb.com/post-download.html
[nirsoft]: https://www.nirsoft.net/utils/usb_devices_view.html
[links]: #Полезные-ссылки
[term]: #Термины
[примеры]: #Примеры
[свойства]: #Свойства
[device]: #Девайс
[HwndMsDoc]: https://learn.microsoft.com/ru-ru/windows/apps/develop/ui-input/retrieve-hwnd
[SafeHandleZeroOrMinusOneIsInvalid]: https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.win32.safehandles.safehandlezeroorminusoneisinvalid?view=net-6.0
[samsung]: #Samsung
[fordeveloper]: #Для-разработчиков
[characters]: #ОПИСАНИЕ