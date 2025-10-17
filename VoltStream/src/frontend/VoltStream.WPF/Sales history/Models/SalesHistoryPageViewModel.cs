﻿namespace VoltStream.WPF.Sales_history.Models;
using ApiServices.Extensions;
using ApiServices.Interfaces;
using ApiServices.Models;
using ApiServices.Models.Responses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using VoltStream.WPF.Commons;

public partial class SalesHistoryPageViewModel : ViewModelBase
{
    private readonly IServiceProvider services;

    public SalesHistoryPageViewModel(IServiceProvider services)
    {
        this.services = services;
        LoadInitialDataAsync();
    }

    [ObservableProperty] private CustomerResponse? selectedCustomer;
    [ObservableProperty] private ObservableCollection<CustomerResponse> customers = [];

    [ObservableProperty] private CategoryResponse? selectedCategory;
    [ObservableProperty] private ObservableCollection<CategoryResponse> categories = [];

    [ObservableProperty] private ProductResponse? selectedProduct;
    [ObservableProperty] private ObservableCollection<ProductResponse> allProducts = []; // Barcha mahsulotlar
    [ObservableProperty] private ObservableCollection<ProductResponse> products = []; // ComboBox uchun filtrlangan productlar

    [ObservableProperty] private ObservableCollection<ProductItemViewModel> saleItems = [];
    [ObservableProperty] private ObservableCollection<ProductItemViewModel> filteredSaleItems = [];

    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private DateTime? beginDate;
    [ObservableProperty] private DateTime? endDate;




    // --- Boshlang‘ich ma’lumotlarni yuklash
    private async Task LoadInitialDataAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
        await LoadCustomersAsync();
        await LoadSalesHistoryAsync();
        ApplyFilter();
    }

    public async Task LoadCustomersAsync()
    {
        try
        {
            var response = await services.GetRequiredService<ICustomersApi>().GetAllAsync().Handle();
            var mapper = services.GetRequiredService<IMapper>();
            if (response.IsSuccess)
                Customers = mapper.Map<ObservableCollection<CustomerResponse>>(response.Data!);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Mahsulotlar yuklanmadi: {ex.Message}");
        }
    }


    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCategory = null;
        SelectedProduct = null;
        SelectedCustomer = null;

        Products = new ObservableCollection<ProductResponse>(AllProducts);
        FilteredSaleItems = new ObservableCollection<ProductItemViewModel>(SaleItems);
        FinalAmount = FilteredSaleItems.Sum(x => x.TotalAmount);
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        try
        {
            if (FilteredSaleItems == null || !FilteredSaleItems.Any())
            {
                MessageBox.Show("Eksport qilish uchun ma'lumot topilmadi.",
                                "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel fayllari (*.xlsx)|*.xlsx",
                FileName = "Savdo tarixi.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Savdo tarixi");

                // 🧾 1-qator: Sana yoki sana oralig‘i
                string headerText = "Sotilgan mahsulotlar ro'yxati";


                worksheet.Cell(1, 1).Value = headerText;
                worksheet.Range("A1:J1").Merge();
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Height = 25;

                // 🟦 Ustun nomlari (2-qator)
                worksheet.Cell(2, 1).Value = "Sana";
                worksheet.Cell(2, 2).Value = "Xaridor";
                worksheet.Cell(2, 3).Value = "Mahsulot turi";
                worksheet.Cell(2, 4).Value = "Nomi";
                worksheet.Cell(2, 5).Value = "Rulon uzunligi";
                worksheet.Cell(2, 6).Value = "Rulon soni";
                worksheet.Cell(2, 7).Value = "Jami";
                worksheet.Cell(2, 8).Value = "O‘lchov";
                worksheet.Cell(2, 9).Value = "Narxi";
                worksheet.Cell(2, 10).Value = "Umumiy summa";

                var headerRange = worksheet.Range("A2:J2");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                int row = 3;
                foreach (var item in FilteredSaleItems)
                {
                    worksheet.Cell(row, 1).Value = item.OperationDate?.ToString("dd.MM.yyyy");
                    worksheet.Cell(row, 2).Value = item.Customer ?? "";
                    worksheet.Cell(row, 3).Value = item.Category ?? "";
                    worksheet.Cell(row, 4).Value = item.Name ?? "";
                    worksheet.Cell(row, 5).Value = (int?)item.RollLength ?? 0;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(row, 6).Value = (int?)item.Quantity ?? 0;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(row, 7).Value = (int?)item.TotalCount ?? 0;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(row, 8).Value = item.Unit ?? "";
                    worksheet.Cell(row, 9).Value = (decimal?)item.Price ?? 0;
                    worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 10).Value = (decimal?)item.TotalAmount ?? 0;
                    worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

                    row++;
                }

                // 🟨 Jami qatori
                worksheet.Cell(row, 1).Value = "Jami";
                worksheet.Range(row, 1, row, 9).Merge();
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 10).Value = (decimal?)FinalAmount ?? 0;
                worksheet.Cell(row, 10).Style.Font.Bold = true;
                worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(dialog.FileName);
            }

            MessageBox.Show("Ma'lumotlar muvaffaqiyatli Excel faylga eksport qilindi ✅",
                            "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Preview()
    {
        if (FilteredSaleItems == null || !FilteredSaleItems.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForPrint();
        var previewWindow = new Window
        {
            Title = "Print Preview",
            Width = 900,
            Height = 800,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new DocumentViewer
            {
                Document = fixedDoc,
                Margin = new Thickness(10, 5, 5, 5)
            }
        };
        previewWindow.ShowDialog();
    }

    [RelayCommand]
    private void Print()
    {
        if (FilteredSaleItems == null || !FilteredSaleItems.Any())
        {
            MessageBox.Show("Chop etish uchun ma’lumot topilmadi.",
                            "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForPrint();
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
            dlg.PrintDocument(fixedDoc.DocumentPaginator, "Savdo tarixi");
    }


    // Create FixedDocument (A4), pages with grid table, footer and total.
    private FixedDocument CreateFixedDocumentForPrint()
    {
        double pageWidth = 793.7;
        double pageHeight = 1122.5;
        double margin = 25;

        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(pageWidth, pageHeight);

        // Sahifaga nechta satr sig‘ishini hisoblash uchun
        int maxRowsPerPage = 45; // tajriba asosida sozlasa bo‘ladi
        int currentRow = 0;
        int pageNumber = 0;

        var items = FilteredSaleItems.ToList();
        int totalPages = (int)Math.Ceiling(items.Count / (double)maxRowsPerPage);
        int processedItems = 0;

        while (processedItems < items.Count)
        {
            pageNumber++;

            var page = new FixedPage { Width = pageWidth, Height = pageHeight, Background = Brushes.White };
            var grid = new Grid { Margin = new Thickness(margin, 5, margin, 5) }; // tepa va pastga joy qoldirish
            FixedPage.SetLeft(grid, margin);
            FixedPage.SetTop(grid, margin + 40);

            var headers = new[]
            {
            "Sana","Xaridor","Mahsulot turi","Nomi","Rulon uzunligi","Rulon soni","Jami","O‘lchov","Narxi","Umumiy summa"
        };

            for (int i = 0; i < headers.Length; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            int row = 0;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i < headers.Length; i++)
            {
                var border = new System.Windows.Controls.Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.LightGray,
                    Padding = new Thickness(4)
                };
                var text = new TextBlock
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                border.Child = text;
                Grid.SetRow(border, row);
                Grid.SetColumn(border, i);
                grid.Children.Add(border);
            }

            // Ma'lumotlar qo'shish
            var pageItems = items.Skip(processedItems).Take(maxRowsPerPage).ToList();

            foreach (var item in pageItems)
            {
                row++;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string[] values =
                [
                    item.OperationDate?.ToString("dd.MM.yyyy") ?? "",
                item.Customer ?? "",
                item.Category ?? "",
                item.Name ?? "",
                item.RollLength?.ToString("N0") ?? "",
                item.Quantity?.ToString("N0") ?? "",
                item.TotalCount?.ToString("N0") ?? "",
                item.Unit ?? "",
                item.Price?.ToString("N2") ?? "",
                item.TotalAmount?.ToString("N2") ?? ""
                ];

                for (int i = 0; i < values.Length; i++)
                {
                    var border = new System.Windows.Controls.Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Padding = new Thickness(4)
                    };
                    var text = new TextBlock
                    {
                        Text = values[i],
                        FontSize = 11,
                        TextAlignment = (i >= 4 ? System.Windows.TextAlignment.Right : System.Windows.TextAlignment.Left)
                    };
                    border.Child = text;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, i);
                    grid.Children.Add(border);
                }
            }

            processedItems += pageItems.Count;

            // Agar bu oxirgi sahifa bo‘lsa jami chiqsin
            if (processedItems >= items.Count)
            {
                row++;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var totalLabel = new TextBlock
                {
                    Text = "Jami:",
                    FontWeight = FontWeights.Bold,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                Grid.SetRow(totalLabel, row);
                Grid.SetColumn(totalLabel, headers.Length - 10);
                grid.Children.Add(totalLabel);

                var totalValue = new TextBlock
                {
                    Text = (FinalAmount ?? 0).ToString("N2"),
                    FontWeight = FontWeights.Bold,
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                Grid.SetRow(totalValue, row);
                Grid.SetColumn(totalValue, headers.Length - 1);
                grid.Children.Add(totalValue);
            }

            // --- Sarlavha qo‘shish ---
            var title = new TextBlock
            {
                Text = "Sotilgan mahsulotlar ro‘yxati",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Right,
                Margin = new Thickness(0, 10, 0, 5)
            };
            FixedPage.SetTop(title, 10);
            FixedPage.SetLeft(title, (pageWidth - 300) / 2);
            page.Children.Add(title);

            // --- Sahifa raqami qo‘shish ---
            var pageNumberText = new TextBlock
            {
                Text = $"{pageNumber}-bet / {totalPages}",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 30, 10)
            };
            FixedPage.SetBottom(pageNumberText, 10);
            FixedPage.SetRight(pageNumberText, 30);
            page.Children.Add(pageNumberText);

            page.Children.Add(grid);

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            fixedDoc.Pages.Add(pageContent);
        }

        return fixedDoc;
    }
    // --- Ombordagi mahsulotlar
    public async Task LoadSalesHistoryAsync()
    {
        try
        {
            // 🗓️ Bugungi sana (zaxira sifatida)
            string today = DateTime.Now.ToString("dd.MM.yyyy");

            // 🧭 Sana oralig‘ini aniqlaymiz
            string? begin = BeginDate?.ToString("dd.MM.yyyy");
            string? end = EndDate?.ToString("dd.MM.yyyy");

            FilteringRequest request = new()
            {
                Filters = new()
                {
                    ["Items"] = ["include:Product.Category"],
                    ["Customer"] = ["include"]
                }
            };

            //// 🔍 Agar foydalanuvchi sana tanlamagan bo‘lsa — bugungi sana
            //if (BeginDate == null && EndDate == null)
            //{
            //    request.Filters["Date"] = [today];
            //}
            //else if (BeginDate != null && EndDate == null)
            //{
            //    // faqat boshlanish sanasi bor — o‘sha kundan boshlab
            //    request.Filters["Date"] = [$"<={begin}"];
            //}
            //else if (BeginDate == null && EndDate != null)
            //{
            //    // faqat tugash sanasi bor — o‘sha kungacha
            //    request.Filters["Date"] = [$">={end}"];
            //}
            //else
            //{
            //    // Ikkalasi ham bor — oralig‘i bo‘yicha
            //    request.Filters["Date"] = [$"{$">={begin}"}, {$"<={end}"}"];
            //}

            var srvc = services.GetRequiredService<ISaleApi>();
            var response = await srvc.Filtering(request).Handle();

            if (response.IsSuccess)
            {
                SaleItems.Clear();

                foreach (var sale in response.Data!)
                {
                    foreach (var item in sale.Items)
                    {
                        SaleItems.Add(new ProductItemViewModel
                        {
                            OperationDate = sale.Date,
                            Category = item.Product.Category.Name,
                            Name = item.Product.Name,
                            RollLength = item.LengthPerRoll,
                            Quantity = item.RollCount,
                            Price = item.UnitPrice,
                            TotalCount = (int)item.TotalLength,
                            Customer = sale.Customer?.Name
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
        }
    }

    // --- Kategoriya bo‘yicha filtr
    partial void OnSelectedCategoryChanged(CategoryResponse? value)
    {
        if (value != null)
        {
            // ComboBox uchun mahsulotlarni faqat tanlangan kategoriyadan olish
            var filteredProducts = AllProducts
                .Where(p => p.CategoryId == value.Id)
                .ToList();

            Products.Clear();
            foreach (var product in filteredProducts)
                Products.Add(product);

            // Agar tanlangan product boshqa kategoriyaga tegishli bo‘lsa, tozalaymiz
            if (SelectedProduct != null && SelectedProduct.CategoryId != value.Id)
                SelectedProduct = null;
        }
        else
        {
            // Agar kategoriya olib tashlansa — barcha mahsulotlarni qaytaramiz
            Products.Clear();
            foreach (var product in AllProducts)
                Products.Add(product);
        }

        ApplyFilter();
    }

    // --- ProductResponse tanlanganda DataGrid filtrlansin
    partial void OnSelectedProductChanged(ProductResponse? value)
    {
        ApplyFilter();
    }

    // customerResponse tanlanganda DataGrid filtrlash
    partial void OnSelectedCustomerChanged(CustomerResponse? value)
    {
        ApplyFilter();
    }

    // --- Kategoriya yuklash
    public async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await services.GetRequiredService<ICategoriesApi>().GetAllAsync().Handle();
            var mapper = services.GetRequiredService<IMapper>();
            if (response.IsSuccess)
                Categories = mapper.Map<ObservableCollection<CategoryResponse>>(response.Data!);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kategoriya yuklanmadi: {ex.Message}");
        }
    }

    // --- Mahsulotlarni yuklash
    public async Task LoadProductsAsync()
    {
        try
        {
            var response = await services.GetRequiredService<IProductsApi>().GetAllAsync().Handle();
            var mapper = services.GetRequiredService<IMapper>();
            if (response.IsSuccess)
            {
                AllProducts = mapper.Map<ObservableCollection<ProductResponse>>(response.Data!);

                // Default holatda barcha productlar ComboBox’da ko‘rinadi
                Products.Clear();
                foreach (var product in AllProducts)
                    Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Mahsulotlar yuklanmadi: {ex.Message}");
        }
    }

    // --- Filtrlash funksiyasi (DataGrid uchun)
    private void ApplyFilter()
    {
        IEnumerable<ProductItemViewModel> filtered = SaleItems;

        if (SelectedCategory != null)
            filtered = filtered.Where(x => x.Category == SelectedCategory.Name);

        if (SelectedProduct != null)
            filtered = filtered.Where(x => x.Name == SelectedProduct.Name);

        if (SelectedCustomer != null)
            filtered = filtered.Where(x => x.Customer == SelectedCustomer.Name);

        FilteredSaleItems = new ObservableCollection<ProductItemViewModel>(filtered);
        FinalAmount = FilteredSaleItems.Sum(x => x.TotalAmount);
    }

    // --- Har bir product item o‘zgarishida summa qayta hisoblanadi
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductItemViewModel.TotalAmount))
            RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        FinalAmount = SaleItems.Sum(x => x.TotalAmount);
    }
}
