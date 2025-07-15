using HogWildSystem.BLL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
        private List<InvoiceView> invoices = [];
        //form fields
        private MudForm customerForm = new();
        private bool isFormValid;
        private bool hasDataChanged;
        private string closeButtonText => hasDataChanged ? "Cancel" : "Close";
        //Define the mask for the Phone number
        private IMask customerPhone = new BlockMask(delimiters: "-", new Block('0', 3, 3), new Block('0', 3, 3), new Block('0', 4, 4));
        //Define a mask for the Postal Code
        //Not working, will investigate
        //private IMask customerPostalCode = new RegexMask(@"^\d[A-Z]\d [A-Z]\d[A-Z]$");
        //Define a mask for the Email address
        private IMask customerEmail = RegexMask.Email();
        #endregion

        #region Properties
        [Parameter]
        public int CustomerID { get; set; }
        [Inject]
        protected CustomerService CustomerService { get; set; } = default!;
        [Inject]
        protected LookupService LookupService { get; set; } = default!;
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected InvoiceService InvoiceService { get; set; } = default!;
        #endregion

        #region Method
        protected override void OnInitialized()
        {
            //clear previous error details and messages
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = string.Empty;
            try
            {
                var result = CustomerService.GetCustomer_ByID(CustomerID);
                if (result.IsSuccess)
                {
                    customer = result.Value ?? new();
                    var invoiceResults = InvoiceService.GetInvoices_ByCustomerID(CustomerID);
                    if (invoiceResults.IsSuccess)
                        invoices = invoiceResults.Value ?? [];
                    else
                        errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
                }    
                        
                else
                    errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
            }
            catch (Exception ex)
            {
                // capture any exception message for display
                errorMessage = ex.Message;
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

        public void AddEditCustomer()
        {
            // clear previous error details and messages
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = String.Empty;

            // wrap the service call in a try/catch to handle unexpected exceptions
            try
            {
                var result = CustomerService.AddEditCustomer(customer);
                if (result.IsSuccess)
                {
                    customer = result.Value ?? new();
                    feedbackMessage = "Customer was successfully saved!";

                    //reset the trackers
                    hasDataChanged = false;
                    isFormValid = false;
                    customerForm.ResetTouched();
                } 
                else
                    errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
            }
            catch (Exception ex)
            {
                // capture any exception message for display
                errorMessage = ex.Message;
            }
        }

        public async Task Cancel()
        {
            if(hasDataChanged)
            {
                bool? result = await DialogService.ShowMessageBox("Confirm Cancel",
                                        "Are you sure you want to close the customer editor? All unsaved changes will be lost.", yesText: "Yes", noText: "No");
                //results will be true if the user selects the YES option, will be false if the user selects the NO option.
                //results could be null if the user dismisses the dialogue
                    //  e.g. clicked the close button 'x'
                if(result == false || result == null)
                {
                    return;
                }
            }
            NavigationManager.NavigateTo("/SamplePages/Customers");
        }

        public async Task EditInvoice(int invoiceID)
        {
            if(hasDataChanged)
            {
                bool? result = await DialogService.ShowMessageBox("Confirm Navigation",
                                        "Are you sure you want to edit the invoice? All unsaved customer changes will be lost.", yesText: "Yes", noText: "No");
                //results will be true if the user selects the YES option, will be false if the user selects the NO option.
                //results could be null if the user dismisses the dialogue
                    //  e.g. clicked the close button 'x'
                if(result == false || result == null)
                {
                    return;
                }
            }
            //Note: We are hard coding the Employee ID to 1 since we have no authentication yet
            NavigationManager.NavigateTo($"/SamplePages/InvoiceEdit/{invoiceID}/{CustomerID}/1");
        }
        public async Task NewInvoice()
        {
            if(hasDataChanged)
            {
                bool? result = await DialogService.ShowMessageBox("Confirm Navigation",
                                        "Would you like to save any customer changes before creating a new invoice?", yesText: "Yes", noText: "No");
                //results will be true if the user selects the YES option, will be false if the user selects the NO option.
                //results could be null if the user dismisses the dialogue
                    //  e.g. clicked the close button 'x'
                if(result == false || result == null)
                {
                    //NavigationManager
                }
                else
                {
                    AddEditCustomer();
                    //NavigationManager
                }
            }
            NavigationManager.NavigateTo($"/SamplePages/InvoiceEdit/0/{CustomerID}/1");
        }
        #endregion
    }
}
