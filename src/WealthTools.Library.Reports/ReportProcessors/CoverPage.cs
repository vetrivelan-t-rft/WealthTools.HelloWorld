using System;
using System.Collections.Generic;
using System.Text;
using WealthTools.Common.ReportEngine;
using WealthTools.Library.Reports.Models;
using WealthTools.Library.BrokerManager.Models;
using WealthTools.Library.BrokerManager;
using WealthTools.Library.BrokerManager.Repositories;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using Microsoft.Extensions.Configuration;
using WealthTools.Library.BrokerManager.Interfaces;

namespace WealthTools.Library.Reports
{
    public class CoverPage : ReportProcessor
    {
        IContext _context;
        IConfiguration _configuration;
        IBrokerMgrRepository _brokerMgrRepository;
        public CoverPage(IBrokerMgrRepository brokerMgrRepository, IContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            //_dbWrapper = dbWrapper;
            _brokerMgrRepository = brokerMgrRepository;
            _context = context;
        }
        public override string GetXslt()
        {
            return "inst_default\\ReportEngine\\DataGathering\\CoverPageDG.xsl";
        }

        public override void LoadData(ref BOContainer container, ref ReportRequest request)
        {
            string coverDate = DateTime.Now.ToString("MMMM d, yyyy");
            string sharedPath = _configuration.GetSection("sharedPath").Value;//@"C:\WM_Advisor_Code\Src\Web\shared\inst_default";


            Broker pBroker = _brokerMgrRepository.GetBrokerInfo();
            PortAnalytics_Cover_Page_Report coverpage = new PortAnalytics_Cover_Page_Report();
            coverpage.ReportDate = coverDate;
            coverpage.PreparedBy = pBroker.Name;
            coverpage.PreparedByTitle =  pBroker.Title;
            coverpage.Copyright = "CopyRight text to be added here";//copyRight.Text;
            coverpage.img = String.Format("{0}\\{1}", sharedPath, "inst_default\\cover\\cover.gif"); //CoverPageLogo;
            coverpage.FooterImg = String.Format("{0}\\{1}", sharedPath, "inst_default\\cover\\cover_logo_footer.svg"); ;////Footer_Logo;

            coverpage.PrintType = "L";
            // coverpage.Preferences.COVERPAGE_FIRM_NAME_OVERRIDE = "";
            coverpage.Preferences = new Preferences();
            coverpage.Preferences.Display_Footer_Logo = "true";
            coverpage.Preferences.show_svg_logo_cvrpage = "true";
            coverpage.Preferences.international_brokers_yn = "N";//internationalBrokersYN; // Need to Check
            coverpage.Preferences.report_header_bar_color = "";//GetRequestValue(request.GetValue(RequestParameterNames.ReportHeaderBarColor));
            coverpage.DisplayWatermark = true;// displayWatermark;
            coverpage.WatermarkImg = String.Format("{0}\\{1}", sharedPath, "inst_default\\images\\draft.png"); ;// Thomson.Financial.Infrastructure.Util.Utility.GetWatermarkImage();

            coverpage.ReportName = "Proposal Name";//(!string.IsNullOrEmpty(title) && title.Trim() != "") ? title : investmentPlanInfo.ProposalName;

            coverpage.CoverHeader = new CoverHeader();
            coverpage.CoverHeader.IDS_REPORT_COVER_PAGE = Constants.IDS_RPTCOVERPAGE;
            coverpage.CoverHeader.IDS_PREPAREDFOR = Constants.IDS_PREPAREDFOR;
            coverpage.CoverHeader.IDS_PREPAREDBY = Constants.IDS_PREPAREDBY;
            coverpage.CoverHeader.IDS_PROFILE_PHONE = Constants.IDS_COVERPAGEPHONE;
            coverpage.CoverHeader.IDS_PREPAREDDATE = Constants.IDS_PREPAREDDATE;
            coverpage.PreparedForData = new PreparedForData();
            coverpage.PreparedForData.Name = "";
            coverpage.PreparedForData.addr1 = "";
            coverpage.PreparedForData.addr2 = "";
            coverpage.PreparedForData.city = "";
            coverpage.PreparedForData.zipcode = "";
            coverpage.ContactInfo = new ContactInfo();
            foreach (ContactDetails contactInfo in pBroker.contactInfoList)
            {
                if (contactInfo.Contact_type.ToLower().Equals("firm"))
                {
                    coverpage.ContactInfo.AdvisorInfo.phone = contactInfo.Phone;
                    coverpage.PreparedByEmail =  contactInfo.Email;

                }
                else if (contactInfo.Contact_type.ToLower().Equals("clearing_firm"))
                {
                    coverpage.ContactInfo.ClearingFirmInfo.phone = contactInfo.Phone;
                }

            }
            foreach (Address addressInfo in pBroker.addressInfoList)
            {
                if (addressInfo.Address_Type == AddressType.clearing_firm)
                {
                    coverpage.ContactInfo.ClearingFirmInfo.firm_name = pBroker.Clearing_firm_name;
                    coverpage.ContactInfo.ClearingFirmInfo.addr1 = addressInfo.AddressLine1;
                    coverpage.ContactInfo.ClearingFirmInfo.addr2 =addressInfo.AddressLine2 ;
                    coverpage.ContactInfo.ClearingFirmInfo.city = addressInfo.City;
                    coverpage.ContactInfo.ClearingFirmInfo.addr3 = (addressInfo.State == "XX" ? "" : addressInfo.State) + " " + addressInfo.ZipCode;
                    coverpage.ContactInfo.ClearingFirmInfo.country = ((addressInfo.Country == "XX") ? "" : addressInfo.Country) ;

                    
                }
                else if (addressInfo.Address_Type == AddressType.firm)
                {
                    coverpage.ContactInfo.AdvisorInfo.firm_name = pBroker.Firm_name;
                    coverpage.ContactInfo.AdvisorInfo.addr1 = addressInfo.AddressLine1;
                    coverpage.ContactInfo.AdvisorInfo.addr2 = addressInfo.AddressLine2;
                    coverpage.ContactInfo.AdvisorInfo.city = addressInfo.City;
                    coverpage.ContactInfo.AdvisorInfo.addr3 = (addressInfo.State == "XX" ? "" : addressInfo.State) + " " + addressInfo.ZipCode;
                    coverpage.ContactInfo.AdvisorInfo.country = (addressInfo.Country == "XX") ? "" : addressInfo.Country;
                }
            }

            coverpage.cover_disclaimer.trigger.id = 2;
            coverpage.cover_disclaimer.trigger.placementId = 0;
            coverpage.cover_disclaimer.trigger.priority = 4030;
            coverpage.cover_disclaimer.trigger.text = @"{TP:111,ID:2,PR:4030,DB:63,TX:28751} <REM> Brokerage, investment and financial advisory services are made available through Ameriprise Financial Services, Inc. Member FINRA and SIPC. Some products and services may not be available in all jurisdictions or to all clients. Please review the Ameriprise® Managed Accounts Client Disclosure Brochure or, if you have elected to pay a consolidated advisory fee, the Ameriprise Managed Accounts and Financial Planning Service Combined Disclosure Brochure for a full description of services offered, including fees and expenses. <LF /><LF /> This report is provided for illustration purposes only, in one-on-one presentations and is not complete unless all pages, as noted in the table of contents are included with this document.  Please read the information in ""Disclosure Information"" found at the beginning of this report.</REM>";
            coverpage.cover_disclaimer.trigger.text = coverpage.cover_disclaimer.trigger.text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;quot;", "&quot;");
            coverpage.cover_disclaimer.trigger.textId = 28751;
            coverpage.cover_disclaimer.trigger.typeId = 111;

            coverpage.bank_disclosure.trigger.id = 2;
            coverpage.bank_disclosure.trigger.placementId = 0;
            coverpage.bank_disclosure.trigger.priority =15;
            coverpage.bank_disclosure.trigger.text = @"{TP:71,ID:2,PR:15,DB:63,TX:504} <REM><BX><TB> The securities referred to herein are not insured by the NCUA.  They are not deposits or obligations of, nor are they guaranteed by, the depository institution.  These securities are subject to investment risks, including the possible loss of principal invested. </TB></BX></REM>";
            coverpage.bank_disclosure.trigger.text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;quot;", "&quot;");
            coverpage.bank_disclosure.trigger.textId = 504;
            coverpage.bank_disclosure.trigger.typeId = 71;

            #region disclosure commented code
            /*
             //Get Disclaimer Disclosure
            TriggerManager trigMgr = new TriggerManager(this.ObjCtx);
            TriggersMemento trigMomento = trigMgr.GetTriggers((int)institutionId, (int)DBCode.DBC_ALL, (int)TriggerType.TT_COVER_PAGE, "2", 0);

            if (trigMomento != null)
            {
                foreach (TriggerDef itTrigger in trigMomento.triggers)
                {
                    if (itTrigger.id == 2)
                    {
                        coverpage.cover_disclaimer.trigger.id = itTrigger.id;
                        coverpage.cover_disclaimer.trigger.placementId = itTrigger.placementId;
                        coverpage.cover_disclaimer.trigger.priority = itTrigger.priority;
                        coverpage.cover_disclaimer.trigger.text = itTrigger.text;
                        coverpage.cover_disclaimer.trigger.textId = itTrigger.textId;
                        coverpage.cover_disclaimer.trigger.typeId = itTrigger.typeId;
                        break;
                    }
                }
            }

            if (pBroker.Affiliation == "b" || pBroker.Affiliation == "B" || pBroker.Affiliation == "c" || pBroker.Affiliation == "C")
            {
                DisclosureManager disclosure = new DisclosureManager(new BackofficeSettingsSvc(this.ObjCtx), request);
                string trigger_id = string.Empty;
                disclosure.discDataHelper.AddView(VIEW_TYPE.VIEW_AVP);

                disclosure.AddPostProcessor(new DisclosureManager.PostProcessDelegate(new CoverPageDisclosureBR().RemoveZeroPlacementTriggers));
                disclosure.AddBR(new DisclosureManager.ApplyBRDelegate(new CoverPageDisclosureBR().ApplyBR));

                BusinessObjectList<SectionTriggerDef> triggerlist = disclosure.GetTriggerList();
                if (pBroker.Affiliation == "b" || pBroker.Affiliation == "B")
                    trigger_id = "1";
                else
                    trigger_id = "2";
                foreach (SectionTriggerDef trigger in triggerlist.BusinessObjects)
                {
                    if (trigger.id.ToString() == trigger_id && (trigger.typeId == 19 || trigger.typeId == 71))
                    {
                        coverpage.bank_disclosure.trigger.id = trigger.id;
                        coverpage.bank_disclosure.trigger.placementId = trigger.placementId;
                        coverpage.bank_disclosure.trigger.priority = trigger.priority;
                        coverpage.bank_disclosure.trigger.text = trigger.text;
                        coverpage.bank_disclosure.trigger.textId = trigger.textId;
                        coverpage.bank_disclosure.trigger.typeId = trigger.typeId;
                    }
                }
            }
             */
            #endregion

            BOKey key = new BOKey();
            key.SetPropertyValue("ReportName", "PACoverPage");
            container.Add(key, coverpage);

        }

        public override void PostProcess(ref BOContainer container)
        {
            
        }
    }
}
