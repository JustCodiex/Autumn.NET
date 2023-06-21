using System.Windows;
using System.Windows.Controls;

using Autumn.Annotations;
using Autumn.WPF.Annotations;

using Budget.Pages.Model;

namespace Budget.Pages.View;

/// <summary>
/// Interaction logic for CreateTransaction.xaml
/// </summary>
[Model(typeof(CreateTransactionPage))]
public partial class CreateTransaction : UserControl {

    [Inject]
    public MainWindow? Main { get; set; }

    public CreateTransactionPage Model { get; }

    public CreateTransaction([DataModel] CreateTransactionPage model) {
        this.Model = model;
        InitializeComponent();
    }

    private async void AddTransactionButton_Click(object sender, RoutedEventArgs e) {
        var result = await this.Model.CreateTransaction();
        if (!result.Success) {
            // TODO: Handle
            return;
        }
        if (Main is null)
            return;
        Main.NavigationListBox.SelectedIndex = 0;
    }

}
