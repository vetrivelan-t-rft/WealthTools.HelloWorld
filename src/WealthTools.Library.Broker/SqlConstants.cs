using System;

namespace WealthTools.Library.BrokerManager
{
  
    public class SqlConstants
    {
        public const string READBROKERINFO_SQL = @"SELECT WBR.BROKER_ID, WBR.BROKER_NAME, WBR.BROKER_TITLE, WUS.USERNAME, WBR.FIRM_NAME, WBR.CLRG_FIRM_NAME, 
			    WBR.FIRM_PHONE, WBR.EMAIL_ADDRESS as FIRM_EMAIL, NULL as FIRM_FAX, 
                NVL ((SELECT external_id FROM web_team WHERE mark_deleted_yn = 'N' AND team_hierarchy_type_id = 1 CONNECT BY PRIOR parent_team_id = team_id START WITH team_id IN (SELECT primary_team_id FROM web_broker WHERE broker_id = :broker_id)), '-1' ) AS SUB_NO,
			   WBR.CLRG_FIRM_PHONE, NULL as CLRG_FIRM_EMAIL, NULL as CLRG_FIRM_FAX, 
			    WBR.FIRM_ADDRESS1 as FIRM_ADDR1, WBR.FIRM_ADDRESS2 as FIRM_ADDR2, 
                WBR.FIRM_CITY, WBR.FIRM_STATE, WBR.FIRM_ZIPCODE as FIRM_POSTAL_CODE, WBR.FIRM_COUNTRY, 
				  NVL((SELECT NAME FROM WEB_COUNTRY_NAME WHERE COUNTRY_CODE = NVL(WBR.FIRM_COUNTRY, 'US') AND LANGUAGE_ID = 0), '') AS FIRM_COUNTRY_NAME, 
			    WBR.CLRG_FIRM_ADDRESS1 as CLRG_FIRM_ADDR1, WBR.CLRG_FIRM_ADDRESS2 as CLRG_FIRM_ADDR2, 
			    WBR.CLRG_FIRM_CITY, WBR.CLRG_FIRM_STATE, WBR.CLRG_FIRM_ZIPCODE as CLRG_FIRM_POSTAL_CODE, WBR.CLRG_FIRM_COUNTRY, 
				  NVL((SELECT NAME FROM WEB_COUNTRY_NAME WHERE COUNTRY_CODE = NVL(WBR.CLRG_FIRM_COUNTRY, 'US') AND LANGUAGE_ID = 0), '') AS CLRG_COUNTRY_NAME, 
               WBR.INSTITUTION_ID, WBR.AFFILIATION, WBR.STATE as PRIMARY_STATE, WBR.DESIGNATION_SUFFIX 
			FROM WEB_BROKER WBR, WEB_USER WUS  
			WHERE 
			 WUS.USER_ID= WBR.BROKER_ID AND 
			BROKER_ID=:broker_id";

        public const string GET_INSTITUTION_CONFIG = @"select VALUE from  web_inst_setting 
                                                       where INSTITUTION_ID =:InstitutionId and INST_SETTING_TYPE_ID = 1";

        public const string GET_AC_VIEW_ID = @"SELECT wav.ac_view_id,wav.most_gran_yn FROM web_group_inv_profile wgip,web_inv_profile_config wipc,web_ac_view wav
                                        WHERE wgip.inv_profile_config_id = wipc.inv_profile_config_id
                                          AND wipc.ac_view_set_id = wav.ac_view_set_id
                                          AND wgip.group_id =:group_id";

    }
}
