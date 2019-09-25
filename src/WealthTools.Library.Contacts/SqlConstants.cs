using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Contacts
{
    
    class SqlConstants
    {
        public const string GET_ALL_CONTACTSBY_DEMOGRAPHICS = "PKG_CRM.GETALLCONTACTINFO";
        public const string GET_ALL_CONTACTSBY_ACCOUNTNO = "PKG_CRM.GETALLCONTACTINFO_BY_ACCTNO";
        public const string GET_ALL_CONTACTSBY_REPCODE = "PKG_CRM.GETALLCONTACTINFO_BY_REPCODE";        

        public const string GET_PERSONS_FOR_HH = @"SELECT /*+ index ( wio WEB_INVESTOR_OCCUPATION_PK_ID ) */ wi.investor_id as person_id, wh.household_id, wi.investor_num as person_num, UPPER (wi.first_name) AS first_name,UPPER (wi.mid_initial) AS mid_initial,UPPER (wi.last_name) AS last_name, wi.prefix, UPPER (wi.suffix) AS suffix, wi.marital_status, wi.ssn_tin,wht.NAME AS investor_type, wi.last_access_date, wi.create_date,
       wi.same_as_mail_yn, wrt.relationship_type_id AS relationship_type, wi.birthdate, wi.gender,
       wi.home_phone, wi.fax AS home_fax, wi.email_addr AS home_email,
       wi.phone_mobile AS mobile_phone,  wi.relationship, decode(whm.default_account_id, null, '-1', whm.default_account_id) as default_account_id,  NULL AS mobile_fax,
       NULL AS mobile_email, UPPER (wi.home_addr1) AS home_addr1,
       UPPER (wi.home_addr2) AS home_addr2, UPPER (wi.home_city) AS home_city,
       wi.home_state, wi.home_postal_code,
       UPPER (wi.home_country) AS home_country,
       UPPER (wi.mail_addr1) AS mail_addr1,
       UPPER (wi.mail_addr2) AS mail_addr2, UPPER (wi.mail_city) AS mail_city,
       wi.mail_state, wi.mail_postal_code,
       UPPER (wi.mail_country) AS mail_country,
       UPPER (wio.employer) AS employer, wio.phone_work AS business_phone,
       wio.phone_fax AS work_fax, wio.email_addr AS business_email,
       UPPER (wio.work_addr1) AS business_addr1,
       UPPER (wio.work_addr2) AS business_addr2,
       UPPER (wio.work_city) AS business_city, wio.work_state as business_state,
       NVL(wio.occupation_id, -1) as occupation_id,
       wio.work_postal_code as business_postal_code, UPPER (wio.work_country) AS business_country,
       wio.annual_income, UPPER (wio.job_title) AS job_title,
       wio.employment_status,
       whm.default_account_id,
       wi.datasource_id,
       nvl(wa.external_id, bo.account_number) default_account_num
  FROM web_investor wi LEFT OUTER JOIN web_investor_occupation wio ON (wi.investor_id = wio.investor_id AND (wi.mark_deleted_yn IS NULL OR wi.mark_deleted_yn = 'N'))
       INNER JOIN web_household_member whm ON (wi.investor_id = whm.investor_id)  
       LEFT OUTER JOIN web_account wa ON (whm.default_account_id = wa.account_id)
       LEFT OUTER JOIN bo_account bo ON (whm.default_account_id = bo.account_id)       
       INNER JOIN web_relationship_type wrt ON (whm.relationship_type_id = wrt.relationship_type_id)
       INNER JOIN web_household wh ON (wh.household_id = whm.household_id)
       INNER JOIN web_household_type wht ON (wht.household_type_id = wh.household_type_id)
        WHERE wrt.relationship_type_id IN (1,2,3,4)   AND wh.household_id = :household_id   ORDER by wrt.relationship_type_id, person_id";


        public const string GET_NEXT_SEQUENCE_HOUSEHOLDID = @"SELECT WEB_HOUSEHOLD_SEQ_HOUSEHOLD_ID.NEXTVAL FROM DUAL";
        public const string GET_NEXT_SEQUENCE_INVESTORID = "SELECT WEB_USER_SEQ_USER_ID.NEXTVAL FROM DUAL";

        public const string CREATE_HOUSEHOLD = "INSERT INTO WEB_HOUSEHOLD (HOUSEHOLD_ID, NAME, CREATE_DATE, HOUSEHOLD_TYPE_ID) VALUES(:householdId, :householdName, SYSDATE, :householdTypeId)";

        public const string CREATE_WEB_USER = @"INSERT INTO WEB_USER(USER_ID, INSTITUTION_ID, USERNAME, PASSWORD, PASSWORD_HINT, CREATE_DATE, STATUS, USER_TYPE_ID, INSTITUTION_ID_LOGIN)
        VALUES( :INVESTOR_ID,:INSTITUTION_ID,:USERNAME,:PASSWORD,:PASSWORD_HINT,
        :CREATE_DATE,:STATUS,:USER_TYPE_ID,:INSTITUTION_ID_LOGIN)";

        public const string CREATE_WEB_INVESTOR = @"INSERT INTO WEB_INVESTOR (INVESTOR_ID, PREFIX, SUFFIX, FIRST_NAME, MID_INITIAL, LAST_NAME, BIRTHDATE, SSN_TIN, MARK_DELETED_YN, GENDER,
                              INVESTOR_NUM, CREATE_DATE, LAST_ACCESS_DATE, MARITAL_STATUS, SAME_AS_MAIL_YN, EXTERNAL_ID,
                              HOME_PHONE, FAX , EMAIL_ADDR,  
                              PHONE_MOBILE, RELATIONSHIP, 
                              HOME_ADDR1, HOME_ADDR2, HOME_CITY, HOME_STATE, HOME_POSTAL_CODE, HOME_COUNTRY, 
                              MAIL_ADDR1, MAIL_ADDR2, MAIL_CITY, MAIL_STATE, MAIL_POSTAL_CODE, MAIL_COUNTRY)
                              VALUES ( :INVESTOR_ID, :PREFIX, :SUFFIX, :FIRST_NAME, :MID_INITIAL, :LAST_NAME, TO_DATE(:BIRTHDATE,'MM/DD/YYYY'), :SSN_TIN,:MARK_DELETED_YN, :GENDER,
                              :INVESTOR_NUM, :CREATE_DATE, :LAST_ACCESS_DATE, :MARITAL_STATUS, :SAME_AS_MAIL_YN,:INVESTOR_ID, 
                              :HOME_PHONE, :FAX, :EMAIL_ADDR,
                              :PHONE_MOBILE, :RELATIONSHIP,  
		                      :HOME_ADDR1, :HOME_ADDR2, :HOME_CITY,:HOME_STATE, :HOME_POSTAL_CODE, :HOME_COUNTRY, 
		                      :MAIL_ADDR1,:MAIL_ADDR2, :MAIL_CITY, :MAIL_STATE, :MAIL_POSTAL_CODE, :MAIL_COUNTRY)";
        public const string CREATE_HOUSEHOLD_MEMBER = "INSERT INTO WEB_HOUSEHOLD_MEMBER (HOUSEHOLD_ID, INVESTOR_ID, RELATIONSHIP_TYPE_ID,DEFAULT_ACCOUNT_ID) VALUES(:HOUSEHOLD_ID, :INVESTOR_ID, :RELATIONSHIP_TYPE_ID, :DEFAULT_ACCOUNT_ID)";

        public const string CREATE_WEB_HOUSEHOLD_QRY_FILTER = @"insert into web_household_qry_filter(household_id, filter_token, user_entered_yn)(select wth.household_Id, wt.team_id, decode(wth.team_assign_type_id, 3, 'Y', 'N') from web_team_x_household wth, web_team wt where wth.team_id = wt.team_id and wt.team_hierarchy_type_id = 3 and wth.household_id = :householdId)";
        public const string DELETE_WEB_HOUSEHOLD_QRY_FILTER = @"DELETE FROM WEB_HOUSEHOLD_QRY_FILTER WHERE HOUSEHOLD_ID = :householdId";

        public const string GET_INSTITUTION_CONFIG = @"select VALUE from  web_inst_setting 
                                                       where INSTITUTION_ID =:InstitutionId and INST_SETTING_TYPE_ID = 1";

        public const string GET_HOUSEHOLD_TEAMS = @"SELECT distinct
                              wtxb.household_id,
                              wt.team_id,
                              wt.name,
                              wt.team_hierarchy_type_id,
                              wt.search_key as rep_code,
                              wtxb.team_assign_type_id,
                              wtxb.team_role_id as team_role_id
                            FROM
                              web_team wt,
                              web_team_x_household wtxb
                            WHERE
                              wtxb.team_id = wt.team_id AND
                              wt.TEAM_HIERARCHY_TYPE_ID = :hierarchyType AND
                              wt.mark_deleted_yn='N' AND
                              wtxb.household_id= :household_id order by UPPER(wt.name)";

        public const string GET_DEFAULT_TEAMS = @"SELECT
	                        teamshare.broker_id,
	                        teams.team_id,
	                        teams.name,
	                        teams.search_key AS rep_code,
	                        teamshare.team_role_id,
	                        NULL AS team_assigntype_id,
	                        teams.team_hierarchy_type_id
                        FROM
	                        web_broker_x_teamshare teamshare
	                        JOIN web_team teams ON teamshare.team_id = teams.team_id
                        WHERE teamshare.broker_id = :brokerId
	                        AND teams.mark_deleted_yn = 'N'
	                        AND teams.team_hierarchy_type_id IN (3, 5, 6)
	                        AND EXISTS (
		                        SELECT
			                        1
		                        FROM
			                        web_broker_x_team bxt
		                        WHERE
			                        bxt.broker_id = teamshare.broker_id
			                        AND bxt.team_id IN (
				                        SELECT
					                        team_id
				                        FROM
					                        web_team
				                        WHERE
					                        mark_deleted_yn = 'N'
				                        CONNECT BY PRIOR parent_team_id = team_id
					                        START WITH team_id = teamshare.team_id
			                        )
	                        )
                        ORDER BY
	                        UPPER(teams.name)";

        public const string DELETE_TeamEntitlement = @"DELETE FROM WEB_TEAM_X_HOUSEHOLD WHERE HOUSEHOLD_ID = :householdId ";
        public const string CREATE_TEAM_ENTITLEMENT_ASSIGNTYPE_0 = @"INSERT INTO WEB_TEAM_X_HOUSEHOLD (TEAM_ID, HOUSEHOLD_ID) VALUES(:teamId,:householdId)";
        public const string CREATE_TEAM_ENTITLEMENT = @"INSERT INTO WEB_TEAM_X_HOUSEHOLD (TEAM_ID, HOUSEHOLD_ID, TEAM_ROLE_ID, TEAM_ASSIGN_TYPE_ID) VALUES(:teamId, :householdId, :teamRole, :teamAssignmentType)";
        public const string GET_HOUSEHOLD_TeamEntitlement = @"SELECT * FROM WEB_TEAM_X_HOUSEHOLD WHERE HOUSEHOLD_ID = :householdId";
        public const string GET_Parent_Teams = @"SELECT web_team.team_id as TEAM_ID,web_team.SEARCH_KEY as REP_CODE,web_team.NAME,web_team.TEAM_HIERARCHY_TYPE_ID, 2 as TEAM_ROLE_ID,  1 as TEAM_ASSIGN_TYPE_ID
                                from web_team where team_id in (
                                    SELECT DISTINCT web_team.parent_team_id   
                                           FROM web_team
                                          WHERE web_team.parent_team_id IS NOT NULL
                                     CONNECT BY web_team.team_id = PRIOR web_team.parent_team_id
                                     START WITH web_team.team_id IN (:TEAM_IDS )) AND web_team.mark_deleted_yn='N' ";

        public const string UPDATE_CONTACT = @"UPDATE WEB_INVESTOR 
                            SET FIRST_NAME = :FIRST_NAME, MID_INITIAL = :MID_INITIAL, LAST_NAME = :LAST_NAME, EMAIL_ADDR = :EMAIL_ADDR, HOME_ADDR1 = :HOME_ADDR1, HOME_ADDR2 = :HOME_ADDR2, HOME_CITY = :HOME_CITY, HOME_STATE = :HOME_STATE, HOME_POSTAL_CODE = :HOME_POSTAL_CODE
                            WHERE INVESTOR_ID = :INVESTOR_ID";

    }



}
