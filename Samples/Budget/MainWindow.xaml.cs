using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Autumn.Annotations;
using Autumn.WPF;

using Budget.Pages.Model;
using Budget.Pages.View;

namespace Budget;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    
    private sealed class NavigationElement {
        private readonly string title;
        private readonly Action action;
        public NavigationElement(string title, Action click) {
            this.title = title;
            this.action = click;
        }
        public void Navigate() => action.Invoke();
        public override string ToString() => title;
    }

    public MainWindow() { // This is a sensitive class, stack-overflow may occur if WPF DI stuff is interacted with -> defer to a [PostInit] method.
        InitializeComponent();
        this.NavigationListBox.ItemsSource = new List<NavigationElement>() {
            new NavigationElement("Overview", () => {
                var view = AutumnWpfApplication.GetView<DashboardPage, Dashboard>();
                Dispatcher.Invoke(view.Page!.UpdateAccounts);
                this.ContentFrame.Content = view;
            }),
            new NavigationElement("Create Account", () => {
                this.ContentFrame.Content = null;
            }),
            new NavigationElement("Create Transaction", () => {
                this.ContentFrame.Content = AutumnWpfApplication.GetView<CreateTransactionPage>();
            }),
            new NavigationElement("Create Income Source", () => {
                this.ContentFrame.Content = null;
            }),
        };
    }

    [PostInit]
    private void OnAutumnReady() {
        this.NavigationListBox.SelectedIndex = 0;
    }

    private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.AddedItems.Count != 1) {
            return;
        }
        if (e.AddedItems[0] is not NavigationElement elem)
            return;
        elem.Navigate();
    }

}
