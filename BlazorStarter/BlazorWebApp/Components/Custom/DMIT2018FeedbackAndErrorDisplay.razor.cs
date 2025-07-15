using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Custom
{
    public partial class DMIT2018FeedbackAndErrorDisplay
    {
        //The parameters can be passed from the parent (page) to the component.
        #region Parameter
        [Parameter]
        public List<string> ErrorDetails { get; set; } = [];
        [Parameter]
        public string ErrorMessage { get; set; } = string.Empty;
        [Parameter]
        public string Feedback { get; set; } = string.Empty;
        private bool hasError => !string.IsNullOrWhiteSpace(ErrorMessage) || ErrorDetails.Any();
        private bool hasFeedback => !string.IsNullOrWhiteSpace(Feedback);
        #endregion
    }
}
