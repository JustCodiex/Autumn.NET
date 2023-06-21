using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autumn.WPF.Annotations;

using Budget.Pages.Model;

namespace Budget.Pages.View;

/// <summary>
/// Interaction logic for AccountDetails.xaml
/// </summary>
[Model(typeof(AccountDetailsPage))]
public partial class AccountDetails : UserControl {

    [DataModel]
    public AccountDetailsPage? Page { get; set; }

    public AccountDetails() {
        InitializeComponent();
    }

}
