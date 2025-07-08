using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class InvoiceEdit
    {
        #region Fields
        private InvoiceView invoice = new();
        private List<PartView> parts = [];
        private int? categoryID;
        private PartView? selectedPart;

        //Errors and Feedback
        private List<string> errorDetails = [];
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        #endregion

        #region Properties
        [Inject]
        protected InvoiceService InvoiceService { get; set; } = default!;
        [Inject]
        protected PartService PartService { get; set; } = default!;

        //Page Parameters
        [Parameter]
        public int InvoiceID { get; set; }
        [Parameter]
        public int CustomerID { get; set; }
        [Parameter]
        public int EmployeeID { get; set; }
        #endregion

        #region Methods
        protected override void OnInitialized()
        {
            // clear previous error details and messages
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = String.Empty;

            try
            {
                var invoiceResult = InvoiceService.GetInvoice(InvoiceID, CustomerID, EmployeeID);
                if (invoiceResult.IsSuccess)
                    invoice = invoiceResult.Value ?? new();
                else
                    errorDetails = BlazorHelperClass.GetErrorMessages(invoiceResult.Errors.ToList());

                var partsResult = PartService.GetParts();
                if (partsResult.IsSuccess)
                    parts = partsResult.Value ?? [];
                else
                    errorDetails = BlazorHelperClass.GetErrorMessages(partsResult.Errors.ToList());
            }
            catch (Exception ex)
            {
                // capture any exception message for display
                errorMessage = ex.Message;
            }
        }

        private void CategoryChanged(int? newCategoryID)
        {
            if(newCategoryID.HasValue)
            {
                categoryID = newCategoryID.Value;
                selectedPart = null;
            }
        }
        #endregion
    }
}
