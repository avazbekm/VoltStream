﻿namespace VoltStream.WPF.Commons.UserControls;

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Логика взаимодействия для UserCalendar.xaml
/// </summary>
public partial class UserCalendar : UserControl
{
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(UserCalendar), new PropertyMetadata(DateTime.Now, OnSelectedDateChanged));

    public UserCalendar()
    {
        InitializeComponent();
        dateTextBox.PreviewTextInput += DateTextBox_PreviewTextInput;
        dateTextBox.TextChanged += DateTextBox_TextChanged;
        Loaded += UserCalendar_Loaded; // yangi hodisa
        SetDefaultDate();
    }

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private void UserCalendar_Loaded(object sender, RoutedEventArgs e)
    {
        // UserControl yuklanganda, agar Bindingdan kelgan qiymat bo‘lsa — shuni ko‘rsatadi
        if (SelectedDate is DateTime date)
            dateTextBox.Text = date.ToString("dd.MM.yyyy");
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserCalendar userCalendar && e.NewValue is DateTime newDate)
        {
            userCalendar.dateTextBox.Text = newDate.ToString("dd.MM.yyyy");
        }
        UserCalendar userCal = (d as UserCalendar)!;
        userCal!.dateTextBox.Focus();
        userCal.dateTextBox.SelectAll();
    }

    private void SetDefaultDate()
    {
        if (SelectedDate == null)
        {
            SelectedDate = DateTime.Now.Date;
        }
    }

    private void DateTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var textBox = (TextBox)sender;

        // Текст, который будет после вставки нового символа
        if (!string.IsNullOrEmpty(textBox.SelectedText))
        {
            // Если есть выделенный текст — заменяем его новым вводом
            _ = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                                  .Insert(textBox.SelectionStart, e.Text);
        }
        else
        {
            // Обычное добавление символа
            _ = textBox.Text.Insert(textBox.CaretIndex, e.Text);
        }

        if (dateTextBox.Text.Length > 10)
        {
            e.Handled = true; // Блокируем ввод, если длина текста уже соответствует полному формату даты
        }
        else
        {
            e.Handled = !IsValidDateInput(e.Text);
        }
    }

    private void DateTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (textBox.Text.Length == 2)
            {
                textBox.Text += ".";
                textBox.CaretIndex = textBox.Text.Length;
            }
            if (textBox.Text.Length == 5)
            {
                textBox.Text += ".";
                textBox.CaretIndex = textBox.Text.Length;
            }

        }
    }

    private void OpenCalendar_Click(object sender, RoutedEventArgs e)
    {
        calendarPopup.IsOpen = true;
    }

    private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (calendar.SelectedDate.HasValue)
        {
            SelectedDate = calendar.SelectedDate.Value;
            calendarPopup.IsOpen = false;
        }
    }

    private static bool IsValidDateInput(string input) => DateInputRegex().IsMatch(input);

    private static bool IsValidDateFormat(string input) => DateFormatRegex().IsMatch(input);


    [GeneratedRegex("[0-9]", RegexOptions.Compiled)]
    private static partial Regex DateInputRegex();

    [GeneratedRegex(@"^(?:\d{2}\.\d{2}\.\d{2,4})?$", RegexOptions.Compiled)]
    private static partial Regex DateFormatRegex();
}
