using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using LibraryXmlProcessor.ViewModels;

namespace LibraryXmlProcessor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set MainWindow reference in ViewModel
        this.Opened += (s, e) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.MainWindow = this;
            }
        };

        // Handle window closing (X button)
        this.Closing += OnWindowClosing;
    }

    private async void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cancel the close operation to show confirmation dialog
        e.Cancel = true;

        var dialog = new Window
        {
            Title = "Підтвердження виходу",
            Width = 450,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#FFF5F7"))
        };

        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(25)
        };

        // Header with icon
        var headerPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        headerPanel.Children.Add(new TextBlock
        {
            Text = "⚠️",
            FontSize = 32,
            Margin = new Avalonia.Thickness(0, 0, 15, 0)
        });

        headerPanel.Children.Add(new TextBlock
        {
            Text = "Підтвердження виходу",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333333")),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        mainPanel.Children.Add(headerPanel);

        // Message
        var messageBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(15),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        messageBorder.Child = new TextBlock
        {
            Text = "Чи дійсно ви хочете завершити роботу з програмою?",
            FontSize = 14,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            Foreground = new SolidColorBrush(Color.Parse("#666666"))
        };

        mainPanel.Children.Add(messageBorder);

        // Buttons
        var buttonsPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15
        };

        var yesButton = new Button
        {
            Width = 120,
            Height = 40,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            CornerRadius = new Avalonia.CornerRadius(8),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Background = Brush.Parse("#FFB6C1"),
            Foreground = Brushes.White,
            BorderThickness = new Avalonia.Thickness(0)
        };

        var yesContent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        yesContent.Children.Add(new TextBlock { Text = "✓", FontSize = 16, Margin = new Avalonia.Thickness(0, 0, 8, 0) });
        yesContent.Children.Add(new TextBlock { Text = "Так", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        yesButton.Content = yesContent;

        var noButton = new Button
        {
            Width = 120,
            Height = 40,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            CornerRadius = new Avalonia.CornerRadius(8),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Background = Brush.Parse("#A8E6CF"),
            Foreground = Brushes.White,
            BorderThickness = new Avalonia.Thickness(0)
        };

        var noContent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        noContent.Children.Add(new TextBlock { Text = "✗", FontSize = 16, Margin = new Avalonia.Thickness(0, 0, 8, 0) });
        noContent.Children.Add(new TextBlock { Text = "Ні", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        noButton.Content = noContent;

        yesButton.Click += (s, args) =>
        {
            dialog.Close(true);
        };

        noButton.Click += (s, args) =>
        {
            dialog.Close(false);
        };

        buttonsPanel.Children.Add(yesButton);
        buttonsPanel.Children.Add(noButton);

        mainPanel.Children.Add(buttonsPanel);

        dialog.Content = mainPanel;

        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            // User confirmed exit, unsubscribe from Closing event and close
            this.Closing -= OnWindowClosing;
            Close();
        }
    }

    private async void OnExitClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "Підтвердження виходу",
            Width = 450,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#FFF5F7"))
        };

        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(25)
        };

        // Header with icon
        var headerPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 0, 0, 20),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        headerPanel.Children.Add(new TextBlock
        {
            Text = "⚠️",
            FontSize = 32,
            Margin = new Avalonia.Thickness(0, 0, 15, 0)
        });

        headerPanel.Children.Add(new TextBlock
        {
            Text = "Підтвердження виходу",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333333")),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        mainPanel.Children.Add(headerPanel);

        // Message
        var messageBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(15),
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        messageBorder.Child = new TextBlock
        {
            Text = "Чи дійсно ви хочете завершити роботу з програмою?",
            FontSize = 14,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            Foreground = new SolidColorBrush(Color.Parse("#666666"))
        };

        mainPanel.Children.Add(messageBorder);

        // Buttons
        var buttonsPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15
        };

        var yesButton = new Button
        {
            Width = 120,
            Height = 40,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            CornerRadius = new Avalonia.CornerRadius(8),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Background = Brush.Parse("#FFB6C1"),
            Foreground = Brushes.White,
            BorderThickness = new Avalonia.Thickness(0)
        };

        var yesContent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        yesContent.Children.Add(new TextBlock { Text = "✓", FontSize = 16, Margin = new Avalonia.Thickness(0, 0, 8, 0) });
        yesContent.Children.Add(new TextBlock { Text = "Так", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        yesButton.Content = yesContent;

        var noButton = new Button
        {
            Width = 120,
            Height = 40,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            FontFamily = new FontFamily("Segoe UI, Arial, sans-serif"),
            CornerRadius = new Avalonia.CornerRadius(8),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Background = Brush.Parse("#A8E6CF"),
            Foreground = Brushes.White,
            BorderThickness = new Avalonia.Thickness(0)
        };

        var noContent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        noContent.Children.Add(new TextBlock { Text = "✗", FontSize = 16, Margin = new Avalonia.Thickness(0, 0, 8, 0) });
        noContent.Children.Add(new TextBlock { Text = "Ні", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        noButton.Content = noContent;

        yesButton.Click += (s, e) =>
        {
            dialog.Close(true);
        };

        noButton.Click += (s, e) =>
        {
            dialog.Close(false);
        };

        buttonsPanel.Children.Add(yesButton);
        buttonsPanel.Children.Add(noButton);

        mainPanel.Children.Add(buttonsPanel);

        dialog.Content = mainPanel;

        var result = await dialog.ShowDialog<bool>(this);

        if (result)
        {
            Close();
        }
    }
}
