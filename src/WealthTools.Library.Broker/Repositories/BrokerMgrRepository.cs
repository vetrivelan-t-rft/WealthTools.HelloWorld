using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WealthTools.Common.DatabaseWrapper;
using WealthTools.Common.DatabaseWrapper.Interfaces;
using WealthTools.Common.Models.Interfaces;
using WealthTools.Library.BrokerManager.Interfaces;
using WealthTools.Library.BrokerManager.Models;

namespace WealthTools.Library.BrokerManager.Repositories
{
    public class BrokerMgrRepository : IBrokerMgrRepository
    {
        IDatabaseWrapper _dbWrapper;
        IContext _context;

        public BrokerMgrRepository(IDatabaseWrapper dbWrapper, IContext context)
        {
            _dbWrapper = dbWrapper;
            _context = context;
        }

       
        public Broker GetBrokerInfo()
        {
            Broker broker = new Broker();
            try { 
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.READBROKERINFO_SQL;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "broker_id", _context.Identity.BrokerId);               
            }, _context.Identity.InstitutionId);

            foreach (var row in records)
            {
                broker.BrokerId = _context.Identity.BrokerId;
                broker.Suffix = row["DESIGNATION_SUFFIX"].ToString();
                broker.Clearing_firm_name = row["CLRG_COUNTRY_NAME"].ToString();
                broker.Firm_country_name = row["FIRM_COUNTRY_NAME"].ToString();
                broker.Sub_Firm_No = row["SUB_NO"].ToString();

                broker.Primary_state = row["PRIMARY_STATE"].ToString();
                broker.Affiliation = row["AFFILIATION"].ToString();
                broker.Clearing_firm_name = row["CLRG_FIRM_NAME"].ToString();
                broker.Firm_name = row["FIRM_NAME"].ToString();
                broker.User_name = row["USERNAME"].ToString();
                broker.Title = row["BROKER_TITLE"].ToString();
               // broker.Sub_Firm_Num = row["SUB_FIRM_NUM"].ToString();
                broker.InstitutionId = row["INSTITUTION_ID"].ToString();
                //broker.SUB_ID = row["SUB_ID"].ToString();
                broker.Name = row["BROKER_NAME"].ToString();
                broker.USERNAME = row["USERNAME"].ToString();
                broker.BrokerId = row["BROKER_ID"].ToString();
                broker.Email = row["FIRM_EMAIL"].ToString();
                broker.FirmPhone = row["FIRM_PHONE"].ToString();

                broker.contactInfoList = new System.Collections.ArrayList();
                ContactDetails firmContact = new ContactDetails();
                firmContact.Contact_type = "firm";
                firmContact.Phone = row["FIRM_PHONE"].ToString();
                firmContact.Fax = row["FIRM_FAX"].ToString();
                firmContact.Email = row["FIRM_EMAIL"].ToString();
                broker.contactInfoList.Add(firmContact);
                ContactDetails clrgFirmContact = new ContactDetails();
                clrgFirmContact.Contact_type = "clearing_firm";
                clrgFirmContact.Phone = row["CLRG_FIRM_PHONE"].ToString();
                clrgFirmContact.Fax = row["CLRG_FIRM_FAX"].ToString();
                clrgFirmContact.Email = row["CLRG_FIRM_EMAIL"].ToString();
                broker.contactInfoList.Add(clrgFirmContact);

                broker.addressInfoList = new System.Collections.ArrayList();
                Address firmAddress = new Address();
                firmAddress.Address_Type = AddressType.firm;                
                firmAddress.AddressLine1 = row["FIRM_ADDR1"].ToString();
                firmAddress.AddressLine2 = row["FIRM_ADDR2"].ToString();
                firmAddress.City = row["FIRM_CITY"].ToString();
                firmAddress.State = row["FIRM_STATE"].ToString();
                firmAddress.Country = row["FIRM_COUNTRY"].ToString();
                firmAddress.ZipCode = row["FIRM_POSTAL_CODE"].ToString();
                broker.addressInfoList.Add(firmAddress);

                Address clrgFirmAddress = new Address();
                clrgFirmAddress.Address_Type = AddressType.clearing_firm;
                clrgFirmAddress.AddressLine1 = row["CLRG_FIRM_ADDR1"].ToString();
                clrgFirmAddress.AddressLine2 = row["CLRG_FIRM_ADDR2"].ToString();
                clrgFirmAddress.City = row["CLRG_FIRM_CITY"].ToString();
                clrgFirmAddress.State = row["CLRG_FIRM_STATE"].ToString();
                clrgFirmAddress.Country = row["FIRM_COUNTRY"].ToString();
                clrgFirmAddress.ZipCode = row["CLRG_FIRM_COUNTRY"].ToString();
                broker.addressInfoList.Add(clrgFirmAddress);
            }
            }
            catch (Exception ex)
            {
                throw ex;

            }
            return broker;

        }


      
    }
}
