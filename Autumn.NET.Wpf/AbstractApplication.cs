using System;
using System.Windows;
using System.Windows.Navigation;

namespace Autumn.WPF;

public abstract class AbstractApplication : ControlledApplication {

    protected sealed override void OnActivated(EventArgs e) {
        base.OnActivated(e);
    }

    protected sealed override void OnDeactivated(EventArgs e) {
        base.OnDeactivated(e);
    }

    protected sealed override void OnExit(ExitEventArgs e) {
        base.OnExit(e);
    }

    protected sealed override void OnFragmentNavigation(FragmentNavigationEventArgs e) {
        base.OnFragmentNavigation(e);
    }

    protected sealed override void OnLoadCompleted(NavigationEventArgs e) {
        base.OnLoadCompleted(e);
    }

    protected sealed override void OnNavigated(NavigationEventArgs e) {
        base.OnNavigated(e);
    }

    protected sealed override void OnNavigating(NavigatingCancelEventArgs e) {
        base.OnNavigating(e);
    }

    protected sealed override void OnNavigationFailed(NavigationFailedEventArgs e) {
        base.OnNavigationFailed(e);
    }

    protected sealed override void OnNavigationProgress(NavigationProgressEventArgs e) {
        base.OnNavigationProgress(e);
    }

    protected sealed override void OnNavigationStopped(NavigationEventArgs e) {
        base.OnNavigationStopped(e);
    }

    protected sealed override void OnSessionEnding(SessionEndingCancelEventArgs e) {
        base.OnSessionEnding(e);
    }

    protected sealed override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
    }

}
