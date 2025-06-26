using HogWildSystem.BLL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class CustomerEdit
    {
        #region Fields
        private CustomerEditView customer = new();
        // feedback message to display to the user.
	    private string feedbackMessage = string.Empty;
	    // collected error details.
	    private List<string> errorDetails = new();
	    // general error message.
	    private string errorMessage = string.Empty;
        private List<LookupView> provinces = [];
        private List<LookupView> countries = [];
        private List<LookupView> customerStatuses = [];
        #endregion

        #region Properties
        [Parameter]
        public int CustomerID { get; set; }
        [Inject]
        protected CustomerService CustomerService { get; set; } = default!;
        [Inject]
        protected LookupService LookupService { get; set; } = default!;
        #endregion

        #region Method
        protected override void OnInitialized()
        {
            //clear previous error details and messages
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;

            if(CustomerID > 0)
            {
                try
		        {
			        var result = CustomerService.GetCustomer_ByID(CustomerID);
			        if (result.IsSuccess)
				        customer = result.Value ?? new();
			        else
				        errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
		        }
		        catch (Exception ex)
		        {
			        // capture any exception message for display
			        errorMessage = ex.Message;
		        }
            }
            else
            {
                customer = new();
            }

            //Get Lookup Data
            try
		    {
			    var provinceResult = LookupService.GetLookupValues("Province");
			    if (provinceResult.IsSuccess)
				    provinces = provinceResult.Value ?? [];
			    else
				    errorDetails = BlazorHelperClass.GetErrorMessages(provinceResult.Errors.ToList());
                var countryResults = LookupService.GetLookupValues("Country");
			    if (countryResults.IsSuccess)
				    countries = countryResults.Value ?? [];
			    else
				    errorDetails = BlazorHelperClass.GetErrorMessages(countryResults.Errors.ToList());
                var statusResult = LookupService.GetLookupValues("Customer Status");
			    if (statusResult.IsSuccess)
				    customerStatuses = statusResult.Value ?? [];
			    else
				    errorDetails = BlazorHelperClass.GetErrorMessages(statusResult.Errors.ToList());
		    }
		    catch (Exception ex)
		    {
			    // capture any exception message for display
			    errorMessage = ex.Message;
		    }
        }
        #endregion
    }
}
