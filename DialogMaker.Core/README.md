# DialogMaker.Core

Библиотека для создания, компиляции и выполнения диалогов. Позволяет делать диалоги, которые симулируют осмысленность. Визуальный редактор [прилагается](../DialogMaker).

## Начало работы

Установите библиотеку через [NuGet](https://www.nuget.org/packages/DialogMaker.Core) или закиньте целый проект в своё решение.

```
Install-Package DialogMaker.Core 
```

или

```
dotnet package add DialogMaker.Core
```

> [!CAUTION]
> Этой библиотеки пока нет на NuGet. Это было написано на вырост.

## Использование для создания диалогов
Это будет написано когда-нибудь потом в отдельном файле

## Использование для запуска диалогов

1. Открытие экспортированного проекта

```
using DialogMaker.Core.Common;

// Открываем пакет диалогов
DialogPackage package = DialogPackage.Open("path/to/file.dpack");

// Получаем папку с диалогами
DialogFolder folder = package["ultraFolder"];

// Берём свой диалог
Dialog dialog = folder["myFirstDialog"];

```

2. Создание обработчика диалога

```
using DialogMaker.Core.Executioning;

public class ConsoleDialogHandler : IDialogExecutingHandler
{
    // Необязательный диспетчер (для консоли можно null, нам не нужен UI)
    public IDispatcher? Dispatcher => null;

    // Показываем реплику
    public Task ShowReplica(ICharacter? character, ICharacter? listener, 
                           IResourceString text, CancellationToken cancellationToken)
    {
        string speaker = character?.Name ?? "Голоса в голове";
        string audience = listener != null ? $" -> {listener.Name}" : string.Empty;
        
        Console.WriteLine($"[{speaker}{audience}]: {text.Text}");
        
        // Притворяемся, что текст печатается или озвучивается
        return Task.Delay(200, cancellationToken);
    }

    // Показываем выбор вариантов ответа
    public async Task<int> ShowChoice(ICharacter? character, ICharacter? listener,
                                     IStringCollection variants, CancellationToken cancellationToken)
    {
        Console.WriteLine("\n=== ВАРИАНТЫ ОТВЕТА ===");
        
        int index = 0;
        foreach (var variant in variants.Strings)
        {
            Console.WriteLine($"[{index}] {variant.Text}");
            index++;
        }
        
        Console.Write("Ваш выбор (0-{0}): ", variants.Strings.Count - 1);
        
        while (true)
        {
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && 
                choice >= 0 && choice < variants.Strings.Count)
            {
                return choice;
            }

            Console.WriteLine("... попробуйте ещё раз, я верю в вас");
        }
    }

    // В консоли эмоции показать не получиться, но и полотно текста выводить лень
    public Task ShowEmotion(ICharacter? character, IEmotion? emotion, 
                           CancellationToken cancellationToken)
    {
        string name = character?.Name ?? "Кто-то";
        Console.WriteLine($"* {name} корчит рожу * " +
            $"(левая бровь: {emotion?.LeftEye.Eyebrow.YPosition:F2}, " +
            $"рот открыт на: {emotion?.Mouth.OpenPercent:P0})");

        return Task.CompletedTask;
    }

    // Обработка триггера
    public Task HandleTrigger(Trigger trigger, CancellationToken cancellationToken)
    {
        Console.WriteLine($"\n ТРИГГЕР: {trigger.Id}");

        foreach (var param in trigger.Parameters)
        {
            Console.WriteLine($"  {param.Key} = {param.Value}");
        }
        
        // Заглушка: возвращаем какие-то данные обратно
        foreach (var key in trigger.OutputKeys)
        {
            trigger.SetOutput(key, new OperandValue("консоль_знает_ответ"));
        }
        
        return Task.CompletedTask;
    }

    // События жизни диалога
    public void OnDialogExecutingStarted(object? sender, EventArgs e)
    {
        Console.WriteLine("\n╔══════════════════════╗");
        Console.WriteLine("║   ДИАЛОГ НАЧАТ      ║");
        Console.WriteLine("╚══════════════════════╝\n");
    }

    public void OnDialogExecutingEnded(object? sender, EventArgs e)
    {
        Console.WriteLine("\n╔══════════════════════╗");
        Console.WriteLine("║   ДИАЛОГ ОКОНЧЕН    ║");
        Console.WriteLine("║  всем спасибо, все   ║");
        Console.WriteLine("║     свободны         ║");
        Console.WriteLine("╚══════════════════════╝");
    }
}
```

3. Запуск шарманки

```
// Создаём обработчик и исполнитель
ConsoleDialogHandler dialogHandler = new();
DialogExecutor executor = dialog.CreateExecutor();
executor.Handler = dialogHandler;

// Запускаем диалог. 
// Изолированный режим для того чтобы значения переменных не сохранялись после завершения диалога
executor.Start(true);
```

![](https://img.shields.io/badge/6-7-blue)
![](https://img.shields.io/badge/say-wallahi-black)
![](https://img.shields.io/badge/Samsung_Galaxy-S5-green)
![](https://img.shields.io/badge/Americano,_Cappuccino,_Latte-Nescoffe-brown)