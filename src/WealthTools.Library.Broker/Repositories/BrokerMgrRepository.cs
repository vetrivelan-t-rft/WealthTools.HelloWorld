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

        public bool IsBackOfficeInstitution()
        {
            int configId = Convert.ToInt32(_dbWrapper.ExecuteScalar(cmd =>
            {
                cmd.CommandText = SqlConstants.GET_INSTITUTION_CONFIG;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "InstitutionId", _context.Identity.InstitutionId);
            }, _context.Identity.InstitutionId));
            return configId == Constants.IDS_BACKOFFICE_CONFIG_ID;
        }

        private void GetACViewSetIds( ref int origAcViewId, ref int mappedAcViewId)
        {            
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlConstants.GET_AC_VIEW_ID;
                DatabaseWrapperHelper.AddInIntParameter(cmd, "group_id", _context.Identity.GroupId);
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                if (row["most_gran_yn"].ToString().ToUpper() == "Y")
                    origAcViewId = int.Parse(row["ac_view_id"].ToString());
                else
                    mappedAcViewId = int.Parse(row["ac_view_id"].ToString());
            }
            
            if (mappedAcViewId == 0)
                mappedAcViewId = origAcViewId;
        }

        public List<AssetClass> GetAssetClassification(bool broadAssetClass)
        {
            string query = @"SELECT x.asset_class_id,a.name,a.descr,a.red,a.green,
                                                  a.blue,a.abbreviation,NVL(a.perf_descr, a.descr) AS perf_descr,
                                                  parent_ac AS broad_asset_class_id,a.idxid,WIXM.Name as Index_Name,
                                                  wcxac.asset_class_universe_id, NULL AS asset_class_type, nvl(wcxac.seq_num, 0) as seq_num ,acs.sec_id 
                                           FROM
                                              (SELECT ac.asset_class_id,
                                                NVL(
                                                (SELECT asset_class_id_2
                                                   FROM web_asset_class_relationship
                                                  WHERE asset_class_id_2 IN
                                                  (SELECT asset_class_id FROM web_ac_view_asset_class WHERE ac_view_id = :a_mappedACViewId
                                                  ) START
                                                WITH asset_class_id_1= ac.asset_class_id CONNECT BY asset_class_id_1= prior asset_class_id_2
                                                ),
                                                (SELECT asset_class_id
                                                   FROM web_ac_view_asset_class
                                                  WHERE ac_view_id = :a_mappedACViewId
                                                AND asset_class_id = ac.asset_class_id
                                                )) parent_ac
                                                 FROM web_ac_view_asset_class ac
                                                WHERE ac_view_id = :a_origACViewId
                                              ) x              ,
                                              web_asset_class a,
                                              web_asset_class b,
                                              web_ix_master WIXM,
                                              web_classification_x_ac wcxac,
                                              ac_security acs
                                          WHERE x.asset_class_id=a.asset_class_id
                                            AND x.parent_ac         = b.asset_class_id (+)
                                            and wcxac.asset_class_id = x.asset_class_id
                                            and acs.asset_class_id =x.asset_class_id
                                            and WIXM.IDXID(+)=a.IDXID order by a.name asc";
            string broadACquery = @"select wac.asset_class_id, wac.NAME, wac.descr, wac.red, wac.green, wac.blue,
                                                         wac.abbreviation, NVL(wac.perf_descr, wac.descr) AS perf_descr, wac.broad_asset_class_id,
                                                         wac.idxid, WIXM.Name as Index_Name, acs.sec_id,
                                                         wcxac.asset_class_universe_id, wcxac.asset_class_type, nvl(wcxac.seq_num, 0) as seq_num 
                                                  from  web_asset_class wac,web_ac_view_asset_class acx,web_ix_master wixm,web_classification_x_ac wcxac ,
                                              ac_security acs
                                                  where wac.asset_class_id = acx.asset_class_id     
                                                        and WIXM.IDXID(+)=WAC.IDXID 
                                                        and acx.ac_view_id=:a_mappedACViewId   
                                                        and wcxac.asset_class_id (+)= wac.asset_class_id  
                                                        and acs.asset_class_id =x.asset_class_id
                                                        order by wac.asset_class_id";

            //if (_context.Identity.InstitutionId < 0)
            //institutionId = 6018;
            List<AssetClass> acClassList = new List<AssetClass>();
            int origACViewId = 0;
            int mappedACViewID = 0;

            GetACViewSetIds( ref origACViewId, ref mappedACViewID);
            IEnumerable<IDataRecord> records = _dbWrapper.QueryDataRecord(cmd =>
            {
                if (broadAssetClass == true)
                {
                    cmd.CommandText = broadACquery;
                    DatabaseWrapperHelper.AddInIntParameter(cmd, "a_mappedACViewId", mappedACViewID.ToString());
                }
                else
                {
                    cmd.CommandText = query;
                    DatabaseWrapperHelper.AddInIntParameter(cmd, "a_origACViewId", origACViewId.ToString());
                    DatabaseWrapperHelper.AddInIntParameter(cmd, "a_mappedACViewId", mappedACViewID.ToString());
                }
            }, _context.Identity.InstitutionId);
            foreach (var row in records)
            {
                AssetClass assetClass = new AssetClass();
                assetClass.Acid = Convert.ToInt64(row["asset_class_id"]);
                assetClass.Red = Convert.ToInt64(row["red"]);
                assetClass.Green = Convert.ToInt64(row["green"]);
                assetClass.Blue = Convert.ToInt64(row["blue"]);
                assetClass.Idxid = Convert.ToInt64(row["idxid"]);
                assetClass.BroadACId = Convert.ToInt64(row["broad_asset_class_id"]);
                assetClass.SortOrder = Convert.ToInt64(row["seq_num"]);
                assetClass.Name = row["NAME"].ToString();
                assetClass.Descr = row["descr"].ToString();
                assetClass.Abbreviation = row["abbreviation"].ToString();
                assetClass.PerfDescr = row["perf_descr"].ToString();
                assetClass.IndexName = row["Index_Name"].ToString();
                assetClass.AssetType =row["asset_class_type"].ToString();
                assetClass.SecId = row["sec_id"].ToString();
                acClassList.Add(assetClass);
            }

            return acClassList;
        }




    }
}
