using System.Windows.Controls;
using System.Windows.Input;

using Autumn.Annotations;
using Autumn.Context;
using Autumn.WPF;
using Autumn.WPF.Annotations;

using Budget.Pages.Model;

namespace Budget.Pages.View;

/// <summary>
/// Interaction logic for Dashboard.xaml
/// </summary>
[Model(typeof(DashboardPage))]
public partial class Dashboard : UserControl {

    [DataModel]
    public DashboardPage? Page { get; set; }

    [Inject]
    public MainWindow? MainWindow { get; set; }

    [Inject]
    AutumnAppContext? AppContext { get; set; }

    public Dashboard() {
        InitializeComponent();
    }

    private void AccountListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        if (AccountListView.SelectedItems.Count == 0) {
            return;
        }

        if (AppContext is null) {
            return;
        }

        if (MainWindow is null) {
            return;
        }

        var selected = AccountListView.SelectedItems[0];
        if (selected is null) {
            return;
        }

        var accountPage = AppContext.GetInstanceOf<AccountDetailsPage>(selected);
        var accountView = AutumnWpfApplication.GetModelView(accountPage);

        MainWindow.ContentFrame.Content = accountView;

    }

}
