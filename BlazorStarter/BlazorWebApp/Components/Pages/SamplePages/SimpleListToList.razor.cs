using HogWildSystem.BLL;
using HogWildSystem.ViewModels;
using Microsoft.AspNetCore.Components;

namespace BlazorWebApp.Components.Pages.SamplePages
{
    public partial class SimpleListToList
    {
        #region Fields
        private List<PartView> inventory { get; set; } = [];
        public List<InvoiceLineView> shoppingCart { get; set; } = [];
        #endregion

        #region Properties
        [Inject]
        protected PartService PartService { get; set; } = default!;
        #endregion

        #region Methods
        protected override void OnInitialized()
        {
            var result = PartService.GetParts();
            //Assuming that we have a success
            if(result.IsSuccess)
            {
                inventory = result.Value ?? [];
            }
        }

        private void AddPartToCart(PartView part)
        {
            if (part != null)
            {
                shoppingCart.Add(new InvoiceLineView
                {
                    PartID = part.PartID,
                    Description = part.Description,
                    Price = part.Price,
                    Quantity = 1,
                    Taxable = part.Taxable
                });
            }
        }
        private void RemovePartFromCart(InvoiceLineView line)
        {
            shoppingCart.Remove(line);
        }
        #endregion
    }
}
