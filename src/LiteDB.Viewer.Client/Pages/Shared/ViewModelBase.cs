using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace LiteDB.Viewer.Client.Pages.Shared;

public partial class ViewModelBase : LayoutComponentBase
{
    private bool _isUpdated = true;

    //[Inject] private ILocalStorageService _localStorage { get; set; } = null!;
    [Inject] private IJSRuntime? _jsRuntime { get; set; } = null!;
    [Inject] private ClipboardService _clipboard { get; set; } = null!;
    [Inject] private IDialogService _dialogService { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ISnackbar _snackbar { get; set; } = null!;

    //protected async Task<T?> LoadSettingAsync<T>(string name, T? defaultValue = default)
    //{
    //    var exists = await _localStorage!.ContainKeyAsync(name);

    //    return !exists ? defaultValue
    //        : await _localStorage!.GetItemAsync<T>(name);
    //}

    //protected async Task SaveSettingAsync<T>(string name, T value)
    //{
    //    await _localStorage!.SetItemAsync(name, value);
    //}

    protected override bool ShouldRender()
    {
        if (_isUpdated)
        {
            _isUpdated = false;
            return true;
        }

        return false;
    }

    protected async Task CopyTextToClipboardAsync(string text)
    {
        var isSupported = await InvokeAsync(async () => await _clipboard!.IsSupportedAsync());

        if (!isSupported)
        {
            return;
        }

        await InvokeAsync(async () => await _clipboard!.WriteTextAsync(text));
        await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Copy item", "Item copied to clipboard"));
    }

    protected void UpdateProperty<T>(ref T property, T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        _isUpdated = true;
    }

    protected void UpdateProperty<T>(ref T property, T newValue, Action<T> action)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        _isUpdated = true;

        AttemptAction(() => action(newValue), "Unable to process property changed");
    }

    protected void UpdateProperty<T>(ref T property, T newValue, Func<T, Task> action)
    {
        if (EqualityComparer<T>.Default.Equals(property, newValue))
        {
            return;
        }

        property = newValue;
        _isUpdated = true;

        _ = AttemptActionAsync(async () => await action(newValue), "Unable to process property changed");
    }

    protected async Task<T?> InvokeAsync<T>(Func<Task<T>> action)
    {
        T? result = default;

        await InvokeAsync(async () => result = await action());
        return result;
    }

    protected Func<Task> CreateEventCallbackAsyncCommand(Func<Task> action, string message)
    {
        return async () =>
        {
            await AttemptActionAsync(async () => await action(), message);
            await RefreshAsync();
        };
    }

    protected Func<T, Task> CreateEventCallbackAsyncCommand<T>(Func<T, Task> action, string message)
    {
        return async (args) =>
        {
            await AttemptActionAsync(async () => await action(args), message);
            await RefreshAsync();
        };
    }

    protected Func<T1, T2, Task> CreateEventCallbackAsyncCommand<T1, T2>(Func<T1, T2, Task> action, string message)
    {
        return async (t1, t2) =>
        {
            await AttemptActionAsync(async () => await action(t1, t2), message);
            await RefreshAsync();
        };
    }

    protected Action<T> CreateEventCallbackCommand<T>(Action<T> action, string message)
    {
        return (args) =>
        {
            AttemptAction(() => action(args), message);
            RefreshAsync();
        };
    }

    protected Action CreateEventCallbackCommand(Action action, string message)
    {
        return () =>
        {
            AttemptAction(() => action(), message);
            InvokeAsync(() => StateHasChanged());
        };
    }

    protected void AttemptAction(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (_jsRuntime != null)
            {
                _ = _jsRuntime.InvokeAsync<object>("alert", $"{message}: {ex.Message}");
            }
        }

        RefreshAsync();
    }

    protected async Task AttemptActionAsync(Func<Task> actionAsync, string message)
    {
        try
        {
            await actionAsync();
        }
        catch (Exception ex)
        {
            if (_jsRuntime != null)
            {
                await _jsRuntime.InvokeAsync<object>("alert", $"{message}: {ex.Message}");
            }
        }

        await RefreshAsync();
    }

    protected async Task ShowAlertAsync(string message, bool isHtml = false)
    {
        await Task.Yield();
        var dialogOptions = new DialogOptions { FullWidth = true };

        if (isHtml)
        {
            var options = new MessageBoxOptions
            {
                MarkupMessage = new MarkupString(message),
                Title = "Alert"
            };

            await _dialogService!.ShowMessageBox(options, dialogOptions);
            return;
        }

        await _dialogService!.ShowMessageBox("Alert", message, options: dialogOptions);
    }

    protected async Task ShowDialogAsync<T>()
        where T : DialogViewModelBase
    {
        await Task.Yield();
        await _dialogService!.ShowAsync<T>();
    }

    protected async Task ShowDialogAsync<T>(string title, DialogParameters parameters)
        where T : DialogViewModelBase
    {
        await Task.Yield();
        await _dialogService!.ShowAsync<T>(title, parameters);
    }

    protected async Task<(bool success, TResult? value)> ShowDialogAsync<TDialog, TResult>()
        where TDialog : IComponent
    {
        await Task.Yield();
        var result = await _dialogService!.ShowAsync<TDialog>();

        if (result is null)
        {
            return (false, default);
        }

        var dialogResult = await result.Result;

        if (dialogResult is null || dialogResult.Data is null)
        {
            return (false, default);
        }

        return (true, (TResult)dialogResult.Data);
    }

    protected async Task<(bool success, TResult? value)> ShowDialogAsync<TDialog, TResult>(string title, DialogParameters parameters = null!)
        where TDialog : IComponent
    {
        await Task.Yield();

        parameters ??= new DialogParameters();
        var result = await _dialogService!.ShowAsync<TDialog>(title, parameters);

        if (result is null)
        {
            return (false, default);
        }

        var dialogResult = await result.Result;

        if (dialogResult is null || dialogResult.Data is null)
        {
            return (false, default);
        }

        return (true, (TResult)dialogResult.Data);
    }

    protected async Task ShowSnackbarMessageAsync(string message)
    {
        await Task.Yield();
        _snackbar.Add(message);
    }

    protected async Task ShowSnackbarMessageAsync(string message, Action<SnackbarOptions> configure)
    {
        await Task.Yield();
        _snackbar.Add(message, configure: configure);
    }

    protected Task RefreshAsync()
    {
        _isUpdated = true;
        return InvokeAsync(StateHasChanged);
    }

    protected async Task NavigateToAsync(string url, bool forceLoad = false, bool replace = false)
    {
        await Task.Yield();
        _navigationManager!.NavigateTo(url, forceLoad, replace);
    }

    protected Task<bool?> ConfirmActionAsync(string title, string message)
    {
        var options = new DialogOptions
        {
            FullWidth = true
        };

        return _dialogService!.ShowMessageBox(title, message, cancelText: "CANCEL", options: options);
    }

    protected Task<bool?> ConfirmActionAsync(string title, MarkupString message)
    {
        var options = new DialogOptions
        {
            FullWidth = true
        };

        return _dialogService!.ShowMessageBox(title, message, cancelText: "CANCEL", options: options);
    }
}