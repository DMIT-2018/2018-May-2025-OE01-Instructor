using HogWildSystem.BLL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class Customers
    {
        #region Fields
		// feedback message to display to the user.
		private string feedbackMessage = string.Empty;
		// collected error details.
		private List<string> errorDetails = new();
		// general error message.
		private string errorMessage = string.Empty;
		private List<CustomerSearchView> customerList = [];
		private string lastName = string.Empty;
		private string phone = string.Empty;
        #endregion

        #region Properties
        [Inject]
        protected CustomerService CustomerService { get; set; } = default!;
		[Inject]
		protected NavigationManager NavigationManager { get; set; } = default!;
        #endregion

        public void GetCustomers()
		{
			//clear previous error details and messages
			//Always start with this, or you end up with
				//repeat error messages or feedback
			errorDetails.Clear();
			errorMessage = string.Empty;
			feedbackMessage = string.Empty;
		
			//Wrap the call to the service in a try/catch to handle any unexcepted exceptions
			try 
			{
				var result = CustomerService.GetCustomers(lastName, phone);
					if (result.IsSuccess)
						customerList = result.Value ?? [];
					else
						errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
			}
			catch (Exception ex)
			{
				//capture any unexpected exceptions to our error message
				errorMessage = ex.Message;
			}
		}

		private void EditCustomer(int customerID)
		{
			NavigationManager.NavigateTo($"/SamplePages/CustomerEdit/{customerID}");
		}

		private void NewCustomer()
		{
			NavigationManager.NavigateTo("/SamplePages/CustomerEdit/0");
		}
    }
}
