
using Microsoft.JSInterop;

using System.Threading.Tasks;

namespace Wasm.Helper
{
    public static class JSRuntimeExtension
    {
        public static async ValueTask ToastrSuccess(this IJSRuntime JSRuntime, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowToastr", "success", message);
        }
        public static async ValueTask ToastrError(this IJSRuntime JSRuntime, string message)
        {
            await JSRuntime.InvokeVoidAsync("ShowToastr", "error", message);
        }
        public static async ValueTask ShowOrHideConfirmationModal(this IJSRuntime JSRuntime, string showOrHide)
        {
            await JSRuntime.InvokeVoidAsync("ShowToastr", showOrHide, showOrHide);
            //await JSRuntime.InvokeVoidAsync("ShowHideConfirmationModal", showOrHide);
        }
    }
}
