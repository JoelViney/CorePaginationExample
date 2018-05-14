﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePaginationExample
{
    public enum WidgetOrderBy
    {
        Name,
        DateCreated
    }

    public class WidgetRepository
    {
        private DatabaseContext _context;

        public WidgetRepository(DatabaseContext context)
        {
            this._context = context;
        }

        public async Task SaveAsync(IEnumerable<Widget> list)
        {
            // This is a bit basic but it's all we need it to do.
            var newList = list.Where(x => x.IsNew());
            await this._context.AddRangeAsync(list);

            await this._context.SaveChangesAsync();
        }

        public async Task<Paginator<Widget>> SearchAsync(int page, int resultsPerPage, string criteria = null, bool activeOnly = false, WidgetOrderBy orderBy = WidgetOrderBy.Name)
        {
            if (resultsPerPage < 1)
                resultsPerPage = 1;

            var query = (from x in this._context.Widgets
                         where
                         (
                            (String.IsNullOrEmpty(criteria) || x.Name.ToLower().Contains(criteria.ToLower()))
                             && (activeOnly == false || x.Active == true)
                         )
                         select x);

            var count = await query.CountAsync();

            var paginator = new Paginator<Widget>(page, resultsPerPage, count);

            IOrderedQueryable<Widget> orderedQuery;
            switch (orderBy)
            {
                case WidgetOrderBy.DateCreated: orderedQuery = query.OrderBy(o => o.DateCreated); break;
                default: orderedQuery = query.OrderBy(o => o.Name); break;
            }

            paginator.Items = await orderedQuery.Skip(paginator.Skip).Take(resultsPerPage).ToListAsync();

            return paginator;
        }
 
    }
}
