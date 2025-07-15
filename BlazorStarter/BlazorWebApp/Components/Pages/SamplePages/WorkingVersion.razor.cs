using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class WorkingVersion
    {
        #region Fields
        private WorkingVersionView? workingVersionView;
        private List<string> errorDetails = [];
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        #endregion

        #region Properties
        [Inject]
        protected WorkingVersionService WorkingVersionService { get; set; } = default!;
        #endregion

        #region Method
        private void GetWorkVersion()
        {
            try
            {
                workingVersionView = WorkingVersionService.GetWorkingVersion();
                feedbackMessage = "Working Version retrieved";
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        #endregion
    }
}
