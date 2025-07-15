using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorWebApp.Components.Custom
{
    public partial class DMIT2018ExampleDialogue
    {
        private string feedbackText = string.Empty;

        #region Parameters
        //We need to include a cascading parameter in order to 
        //reference the parameter to pass results back or cancel the dialogue
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter]
        public string ButtonText { get; set; } = string.Empty;
        [Parameter]
        public Color ButtonColor { get; set; } = Color.Primary;
        #endregion

        #region Methods
        private void Cancel() => MudDialog.Cancel();

        private void Submit() => MudDialog.Close(DialogResult.Ok(feedbackText));
        #endregion
    }
}
