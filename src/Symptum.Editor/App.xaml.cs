using System.Diagnostics;
using Symptum.Core.Management.Resources;
using Symptum.Core.Subjects;
using Symptum.Core.Subjects.Books;
using Symptum.Core.Subjects.QuestionBanks;
using Uno.Resizetizer;
using Windows.System;

#if __WASM__
using Symptum.Common.Helpers;
using static Uno.Storage.Pickers.FileSystemAccessApiInformation;
#endif

namespace Symptum.Editor;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    public Window? MainWindow { get; private set; }

    private Uri lastUri;

    private string lastId;

    private void LoadSubjects()
    {
        for (int i = 0; i < 100; i++)
        {
            Subject sub = new()
            {
                Title = "Subject " + i,
                Id = "Subjects." + i,
                Uri = ResourceManager.GetAbsoluteUri("subjects/" + i),
            };
            QuestionBank qb = new()
            {
                Title = "QBank " + i,
                Id = sub?.Id + ".QBank" + i,
                Uri = new(sub?.Uri?.ToString() + "/qbank" + i),
                Papers = []
            };
            sub.QuestionBank = qb;
            for (int j = 0; j < 100; j++)
            {
                QuestionBankPaper paper = new()
                {
                    Title = $"Paper {i}-{j}",
                    Id = qb?.Id + $".Paper{i}-{j}",
                    Uri = new(qb?.Uri?.ToString() + $"/paper{i}-{j}"),
                    Topics = []
                };
                qb.Papers.Add(paper);

                for (int k = 0; k < 100; k++)
                {
                    QuestionBankTopic topic = new()
                    {
                        Title = $"Topic {i}-{j}-{k}",
                        Id = paper?.Id + $".Topic{i}-{j}-{k}",
                        Uri = new(paper?.Uri?.ToString() + $"/topic{i}-{j}-{k}")
                    };
                    paper.Topics.Add(topic);

                    if (i == 99 && j == 99 && k == 99)
                    {
                        lastUri = topic.Uri;
                        lastId = topic.Id;
                        Debug.WriteLine(lastUri);
                        Debug.WriteLine(lastId);
                    }
                }
            }

            ResourceManager.Resources.Add(sub);
            ((IResource)sub).InitializeResource(null);
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        //DateTime now = DateTime.Now;
        //LoadSubjects();
        //TimeSpan diff = DateTime.Now - now;
        //Debug.WriteLine("LoadSubjects(): " + diff.TotalMilliseconds + " ms");

        //now = DateTime.Now;
        //IResource? res = ResourceManager.TryGetResourceFromUri(lastUri);
        //diff = DateTime.Now - now;
        //Debug.WriteLine("TryGetResourceFromUri(): " + diff.TotalMilliseconds + " ms");
        //Debug.WriteLine("Found " + res?.Uri);

        //now = DateTime.Now;
        //res = ResourceManager.TryGetResourceFromId(lastId);
        //diff = DateTime.Now - now;
        //Debug.WriteLine("TryGetResourceFromId(): " + diff.TotalMilliseconds + " ms");
        //Debug.WriteLine("Found " + res?.Id);

#if __WASM__
        Uno.WinRTFeatureConfiguration.Storage.Pickers.WasmConfiguration = Uno.WasmPickerConfiguration.FileSystemAccessApiWithFallback;
        StorageHelper.SetPickerSupport(IsOpenPickerSupported, IsSavePickerSupported, IsFolderPickerSupported);
#endif
        LoadAllBookListsAsync();

        MainWindow = new();

#if NET6_0_OR_GREATER && WINDOWS && !HAS_UNO
        MainWindow.ExtendsContentIntoTitleBar = true;
        MainWindow.SystemBackdrop = new MicaBackdrop();
        MainWindow.Title = "Symptum Editor";
#endif

#if DEBUG
        MainWindow.EnableHotReload();
#endif

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (MainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            MainWindow.Content = rootFrame;

            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        MainWindow.SetWindowIcon();
        // Ensure the current window is active
        MainWindow.Activate();
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    private async Task LoadAllBookListsAsync()
    {
        try
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Books/First Year Books.csv"));
            string content = await FileIO.ReadTextAsync(file);
            BookStore.LoadBooks(content);
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Books/Second Year Books.csv"));
            content = await FileIO.ReadTextAsync(file);
            BookStore.LoadBooks(content);
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Books/Third Year Books.csv"));
            content = await FileIO.ReadTextAsync(file);
            BookStore.LoadBooks(content);
        }
        catch { }
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
#if DEBUG
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on the web or
        // desktop targets, you can use URL or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__ || __MACCATALYST__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#else
            builder.AddConsole();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Information);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);

            // Generic Xaml events
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace );

            // Layouter specific messages
            // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

            // builder.AddFilter("Windows.Storage", LogLevel.Debug );

            // Binding related messages
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );
            // builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug );

            // Binder memory references tracking
            // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

            // DevServer and HotReload related
            // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

            // Debug JS interop
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
#endif
    }
}
