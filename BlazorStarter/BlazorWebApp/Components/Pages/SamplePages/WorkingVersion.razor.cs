using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class WorkingVersion
    {
        #region Fields
        private WorkingVersionView? workingVersionView;
        private List<string> errorMessages = [];
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
            }
            catch (Exception ex)
            {
                errorMessages.Add(BlazorHelperClass.GetInnerException(ex).Message);
            }
        }
        #endregion
    }
}
