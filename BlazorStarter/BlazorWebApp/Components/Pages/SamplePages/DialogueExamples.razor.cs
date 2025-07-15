
using BlazorWebApp.Components.Custom;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class DialogueExamples
    {
        private string buttonText = string.Empty;
        private Color? buttonColour = Color.Primary;
        private string feedbackText = string.Empty;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        private async Task CustomDialogue()
        {
            feedbackText = string.Empty;
            if(string.IsNullOrWhiteSpace(buttonText) || !buttonColour.HasValue) 
            {
                feedbackText = "Please enter button text and select a colour to show the dialogue.";
                return;
            }

            //For custom dialogues with parameters, you need to make a DialogParameters instance
            // that contains the parameter information
            var parameters = new DialogParameters<DMIT2018ExampleDialogue>
            {
                { x => x.ButtonText, buttonText },
                { x => x.ButtonColor, buttonColour.Value }
            };

            //For dialogues we can also specify specific options
            DialogOptions options = new DialogOptions()
            {
                CloseButton = false,
                Position = DialogPosition.Center,
                BackdropClick = false,
                CloseOnEscapeKey = false                
            };

            //Create the dialogue with the parameters and the options
            //must give it a title as well
            var dialogue = await DialogService.ShowAsync<DMIT2018ExampleDialogue>("Custom Dialogue", parameters, options);

            //await the results
            var results = await dialogue.Result;

            //check the results
            if(results != null && !results.Canceled)
            {
                feedbackText = results.Data?.ToString() ?? string.Empty;
            }
            else if (results != null && results.Canceled)
            {
                feedbackText = "The dialogue was canceled";
            }
        }
    }
}
