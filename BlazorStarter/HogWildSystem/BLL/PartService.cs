using BYSResults;
using HogWildSystem.DAL;
using HogWildSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class PartService
    {
        private readonly HogWildContext _context;

        internal PartService(HogWildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Result<List<PartView>> GetParts()
        {
            var result = new Result<List<PartView>>();

            var parts = _context.Parts
                            .Where(x => !x.RemoveFromViewFlag)
                            .Select(p => new PartView
                            {
                                PartID = p.PartID,
                                PartCategoryID = p.PartCategoryID,
                                CategoryName = p.PartCategory.Name,
                                Description = p.Description,
                                Cost = p.Cost,
                                Price = p.Price,
                                ROL = p.ROL,
                                QOH = p.QOH,
                                Taxable = p.Taxable,
                                RemoveFromViewFlag = p.RemoveFromViewFlag
                            })
                            .OrderBy(p => p.CategoryName)
                            .ThenBy(p => p.Description)
                            .ToList();

            if (parts == null || parts.Count == 0)
            {
                result.AddError(new Error("No Records Found", "No parts were found"));
                return result;
            }

            return result.WithValue(parts);
        }
    }
}
