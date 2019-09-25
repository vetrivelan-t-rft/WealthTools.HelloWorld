using System.Collections.Generic;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.Proposals.Models;

namespace WealthTools.Library.Proposals.Interfaces
{
    public interface IProposalsRepository
    {
        List<ProposalsModel> GetRecentProposals();
        List<ProposalByHH> GetProposalsByHH(string householdID);

        bool Delete_Web_Investment_Plan(int PlanID);

        long CreateNewProposal(string houseHoldID, string proposalName);
        List<ProposalsModel> SearchProposals(ProposalSearchParameters searchParameters);
        List<ProfileModelInfo> GetProfileModelData(List<string> planIds);
    }
}
