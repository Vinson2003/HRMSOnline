using HRMS.Service.Interface;
using Microsoft.JSInterop;

namespace HRMS.Service.Service
{
    public class DialogService(IJSRuntime js) : IDialogService
    {
        public async Task<bool> ConfirmAsync(string message)
        {
            try
            {
                return await js.InvokeAsync<bool>("confirm", message);
            }
            catch (JSDisconnectedException)
            {
                // The browser connection is gone: treat it as "not confirmed"
                return false;
            }
            catch (JSException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public async Task AlertAsync(string? message)
        {
            try
            {
                await js.InvokeVoidAsync("alert", message);
            }
            catch (JSDisconnectedException)
            {
                // Nothing to show if the browser connection is gone
            }
            catch (JSException)
            {
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
