using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Cursor _customCursor;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Загружаем .cur файл как ресурс
        var uri = new Uri("pack://application:,,,/Resources/pointer.cur");
        using var stream = Application.GetResourceStream(uri)?.Stream;
        if (stream != null)
        {
            _customCursor = new Cursor(stream);
            Mouse.OverrideCursor = _customCursor;
        }

        // При входе в любое окно / элемент — снова задаём курсор
        EventManager.RegisterClassHandler(typeof(Window),
            UIElement.MouseEnterEvent,
            new MouseEventHandler((s, args) =>
            {
                if (_customCursor != null)
                    Mouse.OverrideCursor = _customCursor;
            }));
    }

}

