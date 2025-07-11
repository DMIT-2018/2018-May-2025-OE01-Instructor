﻿using HogWildSystem.DAL;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class WorkingVersionService
    {
        private readonly HogWildContext _context;

        internal WorkingVersionService(HogWildContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public WorkingVersionView? GetWorkingVersion()
        {
            return _context.WorkingVersions
                    .Select(x => new WorkingVersionView
                    {
                        VersionId = x.VersionId,
                        Version = $"{x.Major}.{x.Minor}.{x.Build} Rev {x.Revision}",
                        AsOfDate = x.AsOfDate,
                        Comments = x.Comments
                    }).FirstOrDefault();
        }
    }
}
