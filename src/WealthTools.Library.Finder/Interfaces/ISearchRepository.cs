using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Search.Models;

namespace WealthTools.Library.Search.Interfaces
{
    public interface ISearchRepository
    {
        SearchResult Search(AdvanceSearchParams searchParameters);

        SearchResult SearchSecurities(string searchString);



    }
}
