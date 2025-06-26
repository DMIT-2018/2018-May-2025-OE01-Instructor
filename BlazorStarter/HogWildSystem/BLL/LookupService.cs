using BYSResults;
using HogWildSystem.DAL;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class LookupService
    {
        private readonly HogWildContext _context;

        internal LookupService(HogWildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Result<List<LookupView>> GetLookupValues(string categoryName)
        {
            var result = new Result<List<LookupView>>();
            //rule: categoryName must not be null or whitespace
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                result.AddError(new Error("Missing Information", "Please provide a category name."));
                return result;
            }
            //rule: the Lookup category must exist
            if (!_context.Categories.Any(x => x.CategoryName.ToLower() == categoryName.ToLower()))
            {
                result.AddError(new Error("Invalid Category", $"{categoryName} is not a valid lookup category."));
                return result;
            }
            var values = _context.Lookups
                    .Where(x => x.Category.CategoryName.ToLower() == categoryName.ToLower()
                            && !x.RemoveFromViewFlag)
                    .Select(x => new LookupView
                    {
                        LookupID = x.LookupID,
                        Name = x.Name,
                        RemoveFromViewFlag = x.RemoveFromViewFlag
                    })
                    .OrderBy(x => x.Name)
                    .ToList();
            //If returning a list then check if the count is <= 0
            //If returning a single record, check if nothing was returned by looking for null
            if (values.Count <= 0)
            {
                result.AddError(new Error("No Lookup Values", $"No lookup values found for category {categoryName}."));
                return result;
            }
            //return the results with the value(s) from the database LINQ query 
            return result.WithValue(values);
        }
    }
}
