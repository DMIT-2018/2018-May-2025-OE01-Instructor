using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class InvoiceEdit
    {
        #region Fields
        private InvoiceView invoice = new();
        private List<PartView> parts = [];
        private int? categoryID;
        private PartView? selectedPart;
        private int quantity;

        //Errors and Feedback
        private List<string> errorDetails = [];
        private string feedbackMessage = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Any();
        private bool hasFeedback => !string.IsNullOrWhiteSpace(feedbackMessage);
        #endregion

        #region Properties
        [Inject]
        protected InvoiceService InvoiceService { get; set; } = default!;
        [Inject]
        protected PartService PartService { get; set; } = default!;
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected ISnackbar Snackbar { get; set; } = default!;

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
            categoryID = newCategoryID.HasValue ? newCategoryID.Value : null;
            selectedPart = null;
        }

        private void AddPart()
        {
            if(selectedPart != null)
            {
                InvoiceLineView newLine = new()
                {
                    PartID = selectedPart.PartID,
                    Description = selectedPart.Description,
                    Price = selectedPart.Price,
                    Quantity = quantity,
                    RemoveFromViewFlag = selectedPart.RemoveFromViewFlag,
                    Taxable = selectedPart.Taxable,
                };
                invoice.InvoiceLines.Add(newLine);
                //reset the values
                categoryID = null;
                selectedPart = null;
                quantity = 0;
                UpdateTotals();
            }
            
        }

        private void QuantityEdited(InvoiceLineView changedLine, int quantity)
        {
            changedLine.Quantity = quantity;
            UpdateTotals();
        }
        private void PriceEdited(InvoiceLineView changedLine, decimal price)
        {
            changedLine.Price = price;
            UpdateTotals();
        }

        private async Task DeleteInvoiceLine(InvoiceLineView invoiceLine)
        {
            bool? results = await DialogService.ShowMessageBox("Confirm Delete", $"Are you sure that you wish to remove {invoiceLine.Description} from the invoice?", yesText: "Delete", cancelText: "Cancel");
            if(results == true)
            {
                invoice.InvoiceLines.Remove(invoiceLine);
                UpdateTotals();
            }
        }

        private void SyncPrice(InvoiceLineView invoiceLine)
        {
            //Find the original Price of the Part
            decimal originalPrice = parts.Where(x => x.PartID == invoiceLine.PartID).Select(x => x.Price).FirstOrDefault();
            invoiceLine.Price = originalPrice;
            UpdateTotals();
            //References vs Non Reference Example
            //var otherReference = invoiceLine;
            //otherReference.Price = 5.00m;
            //var notAReference = new InvoiceLineView()
            //{
            //    Price = invoiceLine.Price,
            //    Description = invoiceLine.Description,
            //    PartID = invoiceLine.PartID,
            //    InvoiceID = invoiceLine.InvoiceID,
            //    InvoiceLineID = invoiceLine.InvoiceID,
            //    Quantity = invoiceLine.Quantity,
            //    RemoveFromViewFlag = invoiceLine.RemoveFromViewFlag,
            //    Taxable = invoiceLine.Taxable
            //};
            //notAReference.Price = 10.00m;
        }

        private async Task Close()
        {
            bool? result = await DialogService.ShowMessageBox("Confirm Close/Cancel", "Do you want to close the invoice editor? All unsaved changes will be lost.",yesText:"Yes",noText:"No");

            if(result == true)
            {
                NavigationManager.NavigateTo($"/SamplePages/CustomerEdit/{CustomerID}");
            }
        }

        private async Task SaveInvoice()
        {
            // clear previous error details and messages
            errorDetails.Clear();
            errorMessage = string.Empty;
            feedbackMessage = String.Empty;

            //Used in feedback message
            bool isNewInvoice = false;

            try
            {
                var result = InvoiceService.AddEditInvoice(invoice);
                if(result.IsSuccess)
                {
                    //check if the invoiceID was previously 0, if it was the invoice was new
                    isNewInvoice = invoice.InvoiceID == 0;
                    //update the invoice from the results
                    invoice = result.Value ?? new();
                    feedbackMessage = isNewInvoice
                        ? $"New Invoice No {invoice.InvoiceID} was created!"
                        : $"Invoice No {invoice.InvoiceID} was updated!";
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    errorDetails = BlazorHelperClass.GetErrorMessages(result.Errors.ToList());
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        private async Task DeleteInvoice()
        {
            bool? result = await DialogService.ShowMessageBox("Confirm Deletion", $"Are you sure you want to delete Invoice No {invoice.InvoiceID}?",yesText:"Yes",noText:"No");

            if(result == true)
            {
                try
                {
                    var results = InvoiceService.DeleteInvoice(invoice.InvoiceID);
                    if(results.IsSuccess)
                    {
                        Snackbar.Add($"Invoice {invoice.InvoiceID} was successful deleted.", severity: Severity.Success, config => { config.ShowCloseIcon = false; config.VisibleStateDuration = 7000; });
                        NavigationManager.NavigateTo($"/SamplePages/CustomerEdit/{CustomerID}");
                    }
                    else
                    {
                        errorDetails = BlazorHelperClass.GetErrorMessages(results.Errors.ToList());
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
        }

        private void UpdateTotals()
        {
            invoice.SubTotal = invoice.InvoiceLines
                                .Where(x => x.RemoveFromViewFlag == false)
                                .Sum(x => x.Quantity * x.Price);
            invoice.Tax = invoice.InvoiceLines
                            .Where(x => x.RemoveFromViewFlag == false)
                            .Sum(x => x.Taxable ? x.Quantity * x.Price * 0.05m : 0);
        }
        #endregion
    }
}
