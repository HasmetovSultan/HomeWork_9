using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

string token = "Введи сюда свой токен от ТелеграмБот";      //Токен секретный
TelegramBotClient client = new TelegramBotClient(token);              //Создал клиент ТелеграмБота

using CancellationTokenSource cts = new CancellationTokenSource();    // Токен отмены
ReceiverOptions receiverOptions = new ReceiverOptions
{ AllowedUpdates = { } };                                             // Настройка получения обновлений

//******************************************************************************************************************************
    string path = "download/";                                            // Путь в папку
if (!Directory.Exists(path))                         //Проверка на наличие папки
{
    DirectoryInfo di = Directory.CreateDirectory(path);   // Если папка отсутствует, то создать
}

//*******************************************************************************************************************************

          

List<string> komandsForFiles = new List<string>(); // Объявил лист для команд в блок /Files

List<string> ListFiles() // Метод для получения файлов из директории (path)
{
    List <string> locFiles = Directory.GetFiles(path).ToList(); // Объявил лист и получил строки из папки

    for (int i = 0; i < locFiles.Count; i++)
        {
            komandsForFiles.Add($"/{i + 1}"); 
        }   
    
    return locFiles;
}                       


client.StartReceiving                                                 //Функция для получений обновлений
    (                                                
    HandleUpdatesAsync,                                               //(Метод) Обработка обновлений
    HandleArorAsyns,                                                  //(Метод) Обработка ошибок
    receiverOptions,                                                  // Настройка получения обновлений
    cancellationToken: cts.Token);                                    // Токен отмены

var me = await client.GetMeAsync();                        //Переменная для того чтоб при каждом запуске выводилось имя бота в консоли
Console.WriteLine($"Активирован бот - @{me.Username}");    // Вывод на консоль имя бота

Console.ReadLine();                                        // Для задержки консоли в процессе работы с ботом
cts.Cancel();                                              // Вызов отмены токена

async Task HandleUpdatesAsync (ITelegramBotClient client, Update update, CancellationToken cancellationToken)  //Обработка событий (Получение сообщений и нажатие на инлайн кнопки)
{                                                                                                              // Асинхронная функция возврашающая (Таск)
    //**********************СООБШЕНИЯ ТЕКСТОМ****************************************
    if (update.Type == UpdateType.Message && update.Message.Text != null) 
    {            
        await HandleMessage(client, update.Message); 
        return;
    }    
//*************************ФОТО*******************************************************
    if (update.Type == UpdateType.Message && update.Message.Photo != null)
    {
        await HandleMessage(client, update.Message);
            return;
    }
//****************************ДОКУМЕНТЫ***********************************************
    if (update.Type == UpdateType.Message && update.Message.Document != null)
    {
        await HandleMessage(client, update.Message);
        return;
    }
    //***************************АУДИО************************************************
    if (update.Type == UpdateType.Message && update.Message.Audio != null)
    {        
        await HandleMessage(client, update.Message);
        return;
    }
    //*************************КНОПКИ************************************************
    if (update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQery(client, update.CallbackQuery);
        return;
    }
    
}   //Приём событий


async Task HandleMessage (ITelegramBotClient client, Message message)                                             //Обработчик сообшений
{    
    if (message.Type == MessageType.Photo)                                                   
    {                                                                                        
        var fileId = message.Photo.Last().FileId;                                            
        var fileInfo = await client.GetFileAsync(fileId);                                    
        var filePath = fileInfo.FilePath;
        var fileName = Path.GetFileName(filePath);                                                                 
                                                                                             
        string destinationFilePath = $"{path}{fileName}";                                    
        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);   
        var file = await client.GetInfoAndDownloadFileAsync(                                 
            fileId: fileId,                                                                  
            destination: fileStream);                                                        
    }

    if (message.Type == MessageType.Audio)
    {
        var fileId = message.Audio.FileId;
        var fileInfo = await client.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;
        var fileName = message.Audio.FileName;
        Path.GetFileName(fileName);

        string destinationFilePath = $"{path}{fileName}";
        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        var file = await client.GetInfoAndDownloadFileAsync(
            fileId: fileId,
            destination: fileStream);
    }

        if (message.Type == MessageType.Document)
    {
        var fileId = message.Document.FileId;
        var fileInfo = await client.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;
        var fileName = message.Document.FileName;
        Path.GetFileName(fileName);

        string destinationFilePath = $"{path}{fileName}";
        await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
        var file = await client.GetInfoAndDownloadFileAsync(
            fileId: fileId,
            destination: fileStream);
    }

    ///*********************************ВЫВОД СПИСКА СОДЕРЖИМОГО ПАПКИ В ЧАТ**********************************************
    
    if (message.Text == "/Files")        
    {
        if (ListFiles().Count == 0)        // Если в Директории пусто то вывод сообшения
        {
            await client.SendTextMessageAsync(message.Chat.Id, $"Директория пуста!");
            return;
        }
        
        string strMessage = "";  // Вношу в переменную список всех файлов из директории и вывожу одним сообшением в чат
        for (int i = 0; i < ListFiles().Count; i++)
        {
            strMessage += $"\n/{i + 1} {Path.GetFileName(ListFiles()[i])}";            
        }

        await client.SendTextMessageAsync(message.Chat.Id, $"Выберите файл");
        await client.SendTextMessageAsync(message.Chat.Id, $"{strMessage}\n");
                
    }
    //********************************************************************************************************************

    //******************************ВЫГРУЗКА СОДЕРЖИМОГО ИЗ ДИРЕКТОРИИ В ЧАТ**********************************************
    if (komandsForFiles.Contains(message.Text)) // Проверяю есть ли введённая в чат команда косаемо файлов. (Для выгрузки в чат)
    {
        string locStr = message.Text;
        int count = 0;
        for (int i = 0; i < komandsForFiles.Count; i++)
        {
            if (komandsForFiles[i] == locStr)
            {
                break;
            }
            count++;
        }
        string locResolt = ListFiles()[count];
        await using Stream stream = System.IO.File.OpenRead(locResolt);
        Path.GetFileName(locResolt);
        message = await client.SendDocumentAsync(
            message.Chat.Id,
            document: new InputOnlineFile(content: stream, fileName: locResolt),
            caption: "Нет описания");                
    }
    //*******************************************************************************************************************
        
    if (message.Text == "/start")            // если сообшение == /start
    {
        await client.SendTextMessageAsync(message.Chat.Id, "Для вывода списка фалов : /Files");
    }

    #region Незадействованный код
    //**************************************************КНОПКИ********************************************************************************************
    //if (message.Text == "/keyboard")                             //Обработка команды клавиатура
    //{
    //    ReplyKeyboardMarkup keyboard = new(new[]                //Создали массив keyboard и создали две кнопки 
    //    {
    //        new KeyboardButton[] { "Helo", "Goobay" },          //Массив кнопок
    //        new KeyboardButton[] { "Привет", "Пока" }           //Массив кнопок
    //    })
    //    {
    //        ResizeKeyboard = true    //Размер клавиатуры
    //    };

    //    await client.SendTextMessageAsync(message.Chat.Id, "Выбери что то", replyMarkup: keyboard); //Вызов функции из клиента бота(id в которое нужно отправить)
    //    return;                                                                                                                     //сообшение(взято от юзера)
    //}                                                                                                                               //(Текст от меня)
    //-------------------------------ИНЛАЙН КЛАВИАТУРА-------------------------------------------------------                                                                                                                                   //(чтоб юзер увидел клаву)
    //if (message.Text == "/inline") //Обработка команды инлайн
    //{
    //    InlineKeyboardMarkup keyboard = new(new[]
    //    {
    //        new []
    //        {
    //            InlineKeyboardButton.WithCallbackData("Buy 50c", "buy_50c"),
    //            InlineKeyboardButton.WithCallbackData("Buy 100c", "buy_100c"),
    //        },
    //        new []
    //        {
    //            InlineKeyboardButton.WithCallbackData("Sell 50c", "sell_50c"),
    //            InlineKeyboardButton.WithCallbackData("Sell 100c", "sell_100c"),
    //        }

    //    });
    //    await client.SendTextMessageAsync(message.Chat.Id, text: "Выбери нужный вариант", replyMarkup: keyboard);
    //    return;
    //}
    //await client.SendTextMessageAsync(message.Chat.Id, $"Ты написал: \n{message.Text}");      //Эхо - Если юзер не выбрал ни одну команду, то бот ответит тем же сообшением
    //***************************************************************************************************************************************************
}
//*********************************************ОБРАБОТКА ИНЛАЙН КЛАВИАТУРЫ***************************************************************************
async Task HandleCallbackQery(ITelegramBotClient client, CallbackQuery callbackQuery)
{
    if (callbackQuery.Data.StartsWith("buy"))
    {
        await client.SendTextMessageAsync(
            callbackQuery.Message.Chat.Id,
            $"Вы хотите купить?"
        );
        return;
    }
    if (callbackQuery.Data.StartsWith("sell"))
    {
        await client.SendTextMessageAsync(
            callbackQuery.Message.Chat.Id,
            $"Вы хотите продать?"
        );
        return;
    }
    await client.SendTextMessageAsync(
        callbackQuery.Message.Chat.Id,
        $"You choose with data: {callbackQuery.Data}"
        );
    return;
}
//******************************************************************************************************************************************************
#endregion

Task HandleArorAsyns(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)  // Обработка ошибок
{
    var ErrorMassag = exception switch     //Переменная Эрор содержимое которого определим через свитч
    {
        ApiRequestException apiRequestException                                                       //Если ошибка с API телеграмом то
        => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",   //выведим эту ошибку пользователю
        _ => exception.ToString()           //Вернём сообшение об ошибке от нашей библиотеки и
    };
    Console.WriteLine(ErrorMassag);         //выведим это на консоль
    return Task.CompletedTask;
}







