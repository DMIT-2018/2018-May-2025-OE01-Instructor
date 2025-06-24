using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class CustomerEdit
    {
        #region Properties
        [Parameter]
        public int CustomerID { get; set; }
        #endregion
    }
}
