using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Accounts
{
    class SqlConstants
    {

        public const string LOGGED_INSTITUTION_ID_SQL = @"SELECT VALUE from WEB_INST_SETTING where INSTITUTION_ID= :INSTITUTION_ID and INST_SETTING_TYPE_ID = 1";

        public const string READHOUSEHOLDACCOUNTS_SQL = @"SELECT wa.account_id, wa.account_num ACCOUNT_NUM, wa.NAME ACCOUNT_NAME, 'N' INTERNAL_YN, TO_CHAR(wa.PROGRAM_ID) PROGRAM_ID, 'N' TEST_YN, '' as attr2, 0 as Display_Type_ID
                            FROM web_household_x_account whxa, web_account wa
                            WHERE
                                whxa.account_id = wa.account_id
                                AND (wa.mark_deleted_yn = 'N' or wa.mark_deleted_yn is null)
                                AND whxa.household_id = :household_id
                                AND wa.plan_id is null
                            UNION ALL
                            SELECT boa.account_id, boa.external_id ACCOUNT_NUM, boa.short_name ACCOUNT_NAME, 'Y' INERNAL_YN, boa.PROGRAM_ID, DECODE(boa.TEST_YN,null,'N', boa.TEST_YN) as TEST_YN, attr2, 0 as Display_Type_ID
                            FROM
                                web_household_x_account whxa,
                                bo_account boa
                            WHERE
                                whxa.account_id = boa.account_id
                                and whxa.household_id = :household_id
                                AND (boa.mark_deleted_yn = 'N' OR boa.mark_deleted_yn IS NULL) AND (:bIncludeClosedActs = 'Y' or (boa.status_code is null or boa.status_code = 'AC'))
                                AND (:boInstitute = 'N' OR boa.account_id in (select wtxa.account_id from web_team_x_account wtxa, web_broker_x_team wbxt
                                where wbxt.broker_id = :broker_id AND wbxt.team_id = wtxa.team_id))
                                AND NOT EXISTS (SELECT 1 FROM bo_account_domain_attribute bada WHERE boa.account_id=bada.account_id and bada.attribute_definition_id = 503 and value in ('0033','0034'))
                            UNION ALL
                            SELECT boa.account_id, boa.external_id ACCOUNT_NUM, boa.short_name ACCOUNT_NAME, 'Y' INERNAL_YN, boa.PROGRAM_ID, DECODE(boa.TEST_YN,null,'N', boa.TEST_YN) as TEST_YN, attr2,whxad.DISPLAY_TYPE_ID as Display_Type_ID
                            FROM
                                WEB_HOUSEHOLD_X_ACCT_DISPLAY whxad,
                                bo_account boa
                            WHERE
                                whxad.account_id = boa.account_id
                                and whxad.household_id = :household_id
                                AND (boa.mark_deleted_yn = 'N' OR boa.mark_deleted_yn IS NULL) AND (:bIncludeClosedActs = 'Y' or (boa.status_code is null or boa.status_code = 'AC'))
                                AND (:boInstitute = 'N' OR boa.account_id in (select wtxa.account_id from web_team_x_account wtxa, web_broker_x_team wbxt
                                where wbxt.broker_id = :broker_id AND wbxt.team_id = wtxa.team_id))
                                AND NOT EXISTS (SELECT 1 FROM bo_account_domain_attribute bada WHERE boa.account_id=bada.account_id and bada.attribute_definition_id = 503 and value in ('0033','0034'))";

        public const string READHOUSEHOLDACCOUNTS_SKIPBANK_SQL = @"SELECT wa.account_id, wa.account_num ACCOUNT_NUM, wa.NAME ACCOUNT_NAME, 'N' INTERNAL_YN, TO_CHAR(wa.PROGRAM_ID) PROGRAM_ID, 'N' TEST_YN,'' as attr2, 0 as Display_Type_ID
        FROM
            web_household_x_account whxa,
            web_account wa
        WHERE
            whxa.account_id = wa.account_id
            AND (wa.mark_deleted_yn = 'N' or wa.mark_deleted_yn is null)
            AND whxa.household_id = :household_id
            AND wa.plan_id is null
            AND NVL(wa.portfolio_account_type,'X') &lt;&gt; 'HOGAN'
        UNION ALL
        SELECT boa.account_id, boa.external_id ACCOUNT_NUM, boa.short_name ACCOUNT_NAME, 'Y' INERNAL_YN, boa.PROGRAM_ID, DECODE(boa.TEST_YN,null,'N', boa.TEST_YN) as TEST_YN, attr2, 0 as Display_Type_ID
        FROM
            web_household_x_account whxa,
            bo_account boa
        WHERE
            whxa.account_id = boa.account_id
            and whxa.household_id = :household_id
            AND (boa.mark_deleted_yn = 'N' OR boa.mark_deleted_yn IS NULL) AND (:bIncludeClosedActs = 'Y' or (boa.status_code is null or boa.status_code = 'AC'))
            AND (:boInstitute = 'N' OR boa.account_id in (select wtxa.account_id from web_team_x_account wtxa, web_broker_x_team wbxt
            where wbxt.broker_id = :broker_id AND wbxt.team_id = wtxa.team_id))
            AND NOT EXISTS (SELECT 1 FROM bo_account_domain_attribute bada WHERE boa.account_id=bada.account_id and bada.attribute_definition_id = 503 and value in ('0033','0034'))
        UNION ALL
        SELECT boa.account_id, boa.external_id ACCOUNT_NUM, boa.short_name ACCOUNT_NAME, 'Y' INERNAL_YN, boa.PROGRAM_ID, DECODE(boa.TEST_YN,null,'N', boa.TEST_YN) as TEST_YN, attr2,whxad.DISPLAY_TYPE_ID 
        FROM
            WEB_HOUSEHOLD_X_ACCT_DISPLAY whxad,
            bo_account boa
        WHERE
            whxad.account_id = boa.account_id
            and whxad.household_id = :household_id
            AND (boa.mark_deleted_yn = 'N' OR boa.mark_deleted_yn IS NULL) AND (:bIncludeClosedActs = 'Y' or (boa.status_code is null or boa.status_code = 'AC'))
            AND (:boInstitute = 'N' OR boa.account_id in (select wtxa.account_id from web_team_x_account wtxa, web_broker_x_team wbxt
            where wbxt.broker_id = :broker_id AND wbxt.team_id = wtxa.team_id))
            AND NOT EXISTS (SELECT 1 FROM bo_account_domain_attribute bada WHERE boa.account_id=bada.account_id and bada.attribute_definition_id = 503 and value in ('0033','0034'))";

        public const string READ_INTERNAL_AND_EXTERNAL_ACCOUNT_DETAILS_SQl = @"WITH wss
            AS 
            (SELECT account_id, inwixa.investor_id ,
                             CONCAT (CONCAT (inwi.first_name, ' '),
                                     inwi.last_name
                                    ) owner_name,
                                    inwixa.ownership_type_id
                        FROM web_investor_x_account inwixa, web_investor inwi
                       WHERE 
                         inwixa.investor_id = inwi.investor_id
                         AND inwixa.account_id in (SELECT /*+cardinality(x,100)*/ x.* FROM TABLE(GETINLIST.GETVARCHARLIST(:account_ids))x))
            SELECT   wa.account_id, wa.NAME account_name, NULL irs_name, wpo.owner_name, wa.account_num,
                    NULL AS attr2,  wa.account_type, wa.program_id as program_id, wa.cash_balance AS cash_component, nvl(wa.currency_code,'USD') as currency_code,
                    c.rate as rate,
                    0 as long_market_value, 0 as short_market_value, 0 market_value,
                     wa.create_date, wa.internal_managed_yn, 'Y' AS manual_flag,
                     wa.tax_status_id, wat.account_type_descr NATURE_OF_ACCOUNT, wa.filing_status_id, wa.account_type_id,
                     wpo.primary_owner_id, wpo.primary_owner_id AS acct_owner_id,
                     wso.secondary_owner_id, holding_yn AS account_detail,
                     TO_CHAR (wa.last_modified_date, 'MM/DD/YYYY') AS last_modified_date,
                     wa.portfolio_account_type as portfolio_acct_type,wa.datasource_id
                FROM 
                     web_account wa, 
                     web_account_type wat,
                     (select account_id, investor_id AS primary_owner_id,owner_name
                      from wss
                      where ownership_type_id=1) wpo,
                     (SELECT account_id, investor_id AS secondary_owner_id
                        from wss
                      where ownership_type_id=2) wso,
                       currency_exchange_rate c
               WHERE wa.account_id = wso.account_id(+)
                 AND wa.account_id = wpo.account_id
                 AND wa.account_id IN (SELECT * FROM TABLE(GETINLIST.GETVARCHARLIST(:account_ids)))
                 AND wat.account_type_id (+) = wa.account_type_id
                 AND nvl(wa.currency_code,'USD') = c.from_currency_code (+) 
                 AND nvl(c.to_currency_code, 'USD') = 'USD'
                AND (wa.mark_deleted_yn = 'N' OR wa.mark_deleted_yn IS NULL)     
            UNION ALL
            SELECT   boa.account_id, boa.short_name account_name, boa.irs_name irs_name, wpo.owner_name,
                     boa.external_id account_num, boa.attr2,  NULL AS account_type, boa.program_id as program_id, 
                      bobal.cash_component,nvl(bobal.currency_code,'USD') as currency_code,c.rate as rate, bobal.long_market_value, bobal.short_market_value,
                     bobal.market_value,
                     boa.account_open_date create_date,
                     boa.internal_yn internal_managed_yn,
                      boa.manually_entered_yn manual_flag, NVL( boa.TAX_STATUS_ID ,case when boa.acct_type_id in (10,20,30) then 2  when boa.acct_type_id in (40,50,60) then 0 ELSE 1 END) tax_status_id, web_account_type.account_type_descr NATURE_OF_ACCOUNT,
                     boaxao.ownership_type_id filing_status_id, boa.acct_type_id account_type_id,
                     wpo.primary_owner_id, wpo.primary_owner_id AS acct_owner_id,
                     wso.secondary_owner_id, 'Y' AS account_detail,
                     TO_CHAR (boa.last_modified_date, 'MM/DD/YYYY') AS last_modified_date,
                     NULL as portfolio_acct_type,boa.datasource_id
                FROM 
                     (select account_id, investor_id AS primary_owner_id,owner_name
                      from wss
                      where ownership_type_id=1) wpo,
                     (SELECT account_id, investor_id AS secondary_owner_id
                        from wss
                      where ownership_type_id=2) wso,
                     bo_account boa,
                     (select account_id,    SUM(cash_component) cash_component, currency_code , SUM(NVL(cash_component,0))
                     + SUM(NVL(long_market_value,0))
                     + SUM(NVL(short_market_value,0)) market_value, SUM(NVL(long_market_value,0)) as long_market_value, SUM(NVL(short_market_value,0)) as short_market_value
                     from bo_account_balance
                     where account_id in (SELECT * FROM TABLE(GETINLIST.GETVARCHARLIST(:account_ids)))
                     group by account_id, currency_code) bobal, 
                     web_account_type, bo_acct_x_acct_owner boaxao,
                     currency_exchange_rate c
               WHERE boa.account_id = wso.account_id(+)
                 AND boa.account_id = wpo.account_id
                 AND (boa.mark_deleted_yn = 'N' OR boa.mark_deleted_yn IS NULL)  AND (:bIncludeClosedActs = 'Y'  
                 or (boa.status_code is null or boa.status_code = 'AC')) 
                 AND boa.account_id = bobal.account_id(+)
                 AND boa.acct_type_id = web_account_type.ACCOUNT_TYPE_ID(+)
                 AND boa.account_id = boaxao.account_id
                 AND nvl(bobal.currency_code,'USD') = c.from_currency_code (+) 
                 AND nvl(c.to_currency_code, 'USD') = 'USD'
            ORDER BY account_name";

        public const string READ_INTERNAL_ANDE_XTERNAL_POSITIONS_SQl = @"SELECT   p.sec_id, NULL AS position_id, p.account_id AS account_id,
         decode(short_indicator, 'S', 2, 1) position_term,
         NULL purchase_date,
         NULL purchase_price,
         SUM (p.quantity) AS quantity,
         SUM (p.market_value) AS market_value, 
         SUM (p.ESTIMATED_ANNUAL_INCOME) AS ESTIMATED_ANNUAL_INCOME, 
        0 as commission,
        'Y' AS market_value_preset,
        NULL AS mkt_val_entry_mode
    FROM bo_position p
    WHERE p.account_id IN (:ACCOUNT_IDS)
  GROUP BY p.account_id, p.sec_id, decode(short_indicator, 'S', 2, 1)
UNION ALL
  --External accounts
  SELECT p.sec_id, p.position_id, TO_CHAR(p.account_id) AS account_id,
         p.position_type_id position_term,
         p.purchase_date,
         p.purchase_price, 
         p.quantity
         ,0 AS market_value,
         NULL as ESTIMATED_ANNUAL_INCOME,
         tr.commission
         ,'N' AS market_value_preset
         ,p.market_value_entry_mode_id AS mkt_val_entry_mode
    FROM web_position p LEFT OUTER JOIN (select wt.position_id, SUM(wt.commission) commission
from web_transaction wt, web_position p
WHERE  p.position_id = wt.position_id
AND p.account_id IN (:ACCOUNT_IDS)
group by wt.position_id)tr ON p.position_id = tr.position_id
   WHERE p.account_id IN (:ACCOUNT_IDS)
     and p.quantity > 0
order by sec_id, position_id";

    public const string READ_SECURITYDETAILS_SQL = @"SELECT s.sec_id,
    s.cusip, s.sedol security_sedol, s.security_name name,  s.security_type AS sec_type,  s.sub_type AS sub_type,
    s.symbol AS ticker,  s.last_close_price AS current_price,  s.last_close_price_date as_of_date,  
    s.PRICE_FACTOR AS price_factor, s.current_factor,s.price_adj_factor,  s.status,
    NVL(s.currency_code,'USD') AS currency_code,  NVL(c.rate,1.0) AS rate,  s.cog_id,
    DECODE(NVL(FACTSHEET_FILE_NAME,'X'), FACTSHEET_FILE_NAME, 'true', decode(NVL(file_name,'Y'), file_name, 'true','false')) as FileExists
    FROM security s, WEB_SEC_FILE_RFRNC b, currency_exchange_rate c WHERE s.SEC_ID IN (:SEC_IDS)
    AND s.SEC_ID = b.SEC_ID(+)
    AND NVL(s.currency_code,'USD') = c.from_currency_code (+) 
    AND c.to_currency_code(+) ='USD'";

        public const string GETPROPOSAL_SQL = @"SELECT DISTINCT invplan.plan_id,
  invplan.name,
  invplan.profile_id,
  invprofile.name AS
profile_name,
  invplan.question_profile_id,
  invplan.modelbased_yn,
  invplan.holdinglevel,
  invplan.ac_view_id,
  invplan.cash_amount,
  invplan.holding_portfolio_id AS
initial_portfolio_id,
  invplan.target_portfolio_id,
  invplan.model_id,
  invplan.impl_plan_id,
  invplan.response_id,
  wp.status,
  invplan.plan_type_id,
  invplan.fee_schedule_id,
    (SELECT TRIM(CONCAT(nvl2(wi.first_name,    CONCAT(wi.first_name,    ' '),    ''),    wi.last_name))
   FROM web_investment_plan wip,
     web_solution_x_party wsxp,
     web_household wh,
     web_household_member whm,
     web_investor wi
   WHERE wip.solution_id = wsxp.solution_id
   AND wsxp.party_id = wh.household_id
   AND wsxp.party_type_id = 1
   AND wh.household_id = whm.household_id
   AND whm.investor_id = wi.investor_id
   AND whm.relationship_type_id = 1
   AND wip.plan_id = :a_nplanid)
AS
hh_name,
    (SELECT standard_rate
   FROM web_fee_schedule
   WHERE fee_schedule_id IN
    (SELECT component_fee_schedule_id
     FROM web_fee_schedule_composition
     WHERE fee_schedule_id =(invplan.fee_schedule_id))
  )
AS
user_fee,
  invplan.plan_process_id,
    (SELECT
   CASE
   WHEN wpfacct.holding_component_id IS NOT NULL THEN
    'TRUE'
   ELSE
    'FALSE'
   END
   FROM web_investment_plan invplan
   INNER JOIN web_holding_port_composition wpfacct ON invplan.holding_portfolio_id = wpfacct.holding_portfolio_id
   AND wpfacct.holding_component_type_id = 1
   AND invplan.plan_id = :a_nplanid
   AND rownum = 1)
AS
account_exists,
  invplan.program_id proposal_program_id,wwp.name PROGRAM_NAME,
  wwp.parent_program_id program_id
FROM web_process wp,
  web_investment_plan invplan LEFT
OUTER JOIN web_investor_profile_detail wipd ON invplan.profile_id = wipd.profile_dtl_id LEFT
OUTER JOIN web_investor_profile invprofile ON wipd.profile_id = invprofile.profile_id LEFT
OUTER JOIN web_wm_program wwp ON invplan.program_id = wwp.program_id
WHERE invplan.plan_id = :a_nplanid
 AND invplan.plan_process_id = wp.process_id";
        public const int PROPOPOSAL_PATH_ENTITY_TYPE_ID = 2617;

        public const string GET_WEB_PROCESS_PARAM = @"SELECT ENTITY_ID FROM WEB_PROCESS_PARAM WHERE PROCESS_ID = :PROCESS_ID AND ENTITY_TYPE_ID = :ENTITY_TYPE_ID";

        public const string GET_INCLUDED_CONTACTS_SQL = @"select distinct(wsp.party_id) from web_solution_x_party wsp, web_investment_plan wip,web_household_member wm where wip.plan_id = :a_Plan_Id and 
                                            wip.solution_id = wsp.solution_id and  party_type_id = 2 and wsp.party_id=wm.investor_id";

        public const string GET_POSITION_FILTER_SQL = @"select wpa.external_id as account_id, wpa.holding_portfolio_id as portfolio_id, wpf.sec_id, wpf.LOCKED_YN, wpf.exclude_yn, 100 as PCT 
from web_position_filter wpf, 
(select WHPC.holding_portfolio_id,  whp.external_id from  WEB_HOLDING_PORT_COMPOSITION WHPC,  web_holding_portfolio whp
where  WHPC.holding_portfolio_id = :a_PORTFOLIO_ID
and  WHPC.holding_component_type_id=1
and  WHPC.holding_component_id= whp.portfolio_id) WPA
where wpf.account_id(+) = wpa.external_id and wpf.portfolio_id(+)  = wpa.holding_portfolio_id";

        public const string GET_ASSET_CLASSIFICATION = @"select * from (
        SELECT wbc.asset_class_id, wbc.sec_id, wbc.pct percentage, wac.name, wac.idxid, wac.red, wac.green, wac.blue, '2' AS DS_ID
        FROM bo_security_classification wbc, web_asset_class wac
        WHERE wbc.sec_id IN(:SEC_IDS) AND wbc.classification_id = :a_classificationid AND wbc.asset_class_id = wac.asset_class_id
        UNION
        SELECT wsc.asset_class_id, wsc.sec_id, wsc.percentage, wac.name, wac.idxid, wac.red, wac.green, wac.blue, '1' AS DS_ID
        FROM web_security_classification wsc, web_asset_class wac
        WHERE wsc.sec_id IN(:SEC_IDS) AND wsc.classification_id = :a_classificationid AND  wsc.asset_class_id = wac.asset_class_id
        UNION
        SELECT wuc.asset_class_id, wuc.sec_id, wuc.percentage, wac.name, wac.idxid, wac.red, wac.green, wac.blue, '1' AS DS_ID
         FROM  web_udt_classification wuc, web_asset_class wac 
        WHERE wuc.sec_id IN (:SEC_IDS) AND wuc.classification_id = :a_classificationid AND wuc.asset_class_id = wac.asset_class_id
        UNION
        SELECT ac.asset_class_id, ac.sec_id, 100  as  percentage, wac.name, wac.idxid,wac.red, wac.green, wac.blue, '1' AS DS_ID
		FROM AC_SECURITY ac, web_asset_class wac
		WHERE ac.sec_id IN (:SEC_IDS) AND ac.asset_class_id = wac.asset_class_id)
        order by sec_id";

        public const string GET_PORTFOLIO_ID = @"SELECT HOLDING_PORTFOLIO_ID id FROM WEB_INVESTMENT_PLAN WHERE PLAN_ID = :PlanId";

        public const string GET_TARGET_PORTFOLIO_ID = @"select target_portfolio_id id from web_investment_plan where plan_id = :PlanId";

        public const string GET_EXISTING_ACCTS = @"select external_id as account_id from WEB_HOLDING_PORT_COMPOSITION WHPC, web_holding_portfolio WHP 
        where WHPC.HOLDING_PORTFOLIO_ID = :PORTFOLIOID
        and holding_component_type_id=1
        and portfolio_id=holding_component_id
        and (external_id IN (:acct_id))";

        public const string GET_NEXT_PORTFOLIO_ID = @"SELECT Get_Seq_Pattern( :Inst_ID, :Table_Name, :Record_Type) as SeqValue  FROM dual";

        public const string INSERT_INVESTMENT_PORTFOLIO = @"INSERT INTO WEB_INVESTMENT_PORTFOLIO 
        (PORTFOLIO_ID, PORTFOLIO_TYPE_ID, CREATE_DATE, CREATE_BY_USER_ID) VALUES 
        (:PORTFOLIO_ID, :PORTFOLIO_TYPE_ID, TRUNC(SYSDATE), :CREATE_BY_USER_ID)";

        public const string INSERT_HOLDING_PORTFOLIO = @"INSERT INTO WEB_HOLDING_PORTFOLIO 
        (PORTFOLIO_ID, PORTFOLIO_TYPE_ID, EXTERNAL_ID, DATASOURCE_ID, CREATE_DATE, CREATE_BY_USER_ID) VALUES 
        (:PORTFOLIO_ID, :PORTFOLIO_TYPE_ID, :EXTERNAL_ID, :DATASOURCE_ID, TRUNC(SYSDATE), :CREATE_BY_USER_ID)";

        public const string INSERT_HOLDING_PORTFOLIO_COMPOSITION = @"INSERT INTO WEB_HOLDING_PORT_COMPOSITION 
        (HOLDING_PORTFOLIO_ID, HOLDING_COMPONENT_ID, HOLDING_COMPONENT_TYPE_ID,  PRODUCT_PORTFOLIO_ID, CREATE_DATE, CREATE_BY_USER_ID) VALUES 
        (:HOLDING_PORTFOLIO_ID, :HOLDING_COMPONENT_ID, :HOLDING_COMPONENT_TYPE_ID, :PRODUCT_PORTFOLIO_ID, TRUNC(SYSDATE), :CREATE_BY_USER_ID)";

        public const string DELETE_HOLDING_PORTFOLIO_COMPOSITION_ACCOUNTS = @"DELETE from WEB_HOLDING_PORT_COMPOSITION where 
        holding_component_type_id =1 and HOLDING_PORTFOLIO_ID= :HOLDING_PORTFOLIO_ID and  
        holding_component_id in( select portfolio_id from web_holding_portfolio where external_id in (:EXTERNAL_ID))";

        public const string DELETE_PORTFOLIO_COMPOSITION = @"DELETE from web_portfolio_composition  WHERE PORTFOLIO_COMPONENT_ID IN(
        SELECT PORTFOLIO_ID FROM web_portfolio_product WHERE EXTERNAL_ID in (:EXTERNAL_ID) AND portfolio_id IN(
        SELECT PORTFOLIO_COMPONENT_ID from web_portfolio_composition where portfolio_id= :PORTFOLIO_ID))
        AND PORTFOLIO_ID= :PORTFOLIO_ID";

        public const string DELETE_FILTER_SECURITY= @"DELETE WEB_POSITION_FILTER WHERE 
        ACCOUNT_ID = :ACCOUNT_ID AND PORTFOLIO_ID = :PORTFOLIO_ID AND SEC_ID =:SEC_ID";

        public const string INSERT_FILTERED_SECURITY= @"INSERT INTO WEB_POSITION_FILTER 
        (PORTFOLIO_ID, ACCOUNT_ID, SEC_ID, LOCKED_YN, EXCLUDE_YN) VALUES
        (:PORTFOLIO_ID, :ACCOUNT_ID, :SEC_ID, :LOCKED_YN, :EXCLUDE_YN)";

        //Get Account types
        public const string GET_ACCOUNT_TYPES = @"SELECT WAT.ACCOUNT_TYPE_ID, WAT.TAX_STATUS_ID,WAT.ACCOUNT_TYPE_DESCR, WTS.NAME AS ACCOUNT_TAX_TYPE_DESC 
                                                FROM WEB_ACCOUNT_TYPE WAT, WEB_TAX_STATUS  WTS 
                                                WHERE WAT.TAX_STATUS_ID = WTS.TAX_STATUS_ID";
        //Create new account
        public const string GET_SEQUENCE_ACCOUNT_ID = @"SELECT WEB_ACCOUNT_SEQ_ACCOUNT_ID.NEXTVAL FROM DUAL";
        public const string CREATE_ACCOUNT_INFO = @"INSERT INTO WEB_ACCOUNT  (ACCOUNT_ID, NAME, TAX_STATUS_ID, ACCOUNT_TYPE_ID, FILING_STATUS_ID, CREATE_DATE, LAST_MODIFIED_DATE, MARK_DELETED_YN,  HOLDING_YN, INTERNAL_MANAGED_YN, DATASOURCE_ID, EXTERNAL_ID, ACCOUNT_NUM)  
                                            VALUES  (:ACCOUNT_ID, :ACCOUNT_NAME, :TAX_TREATMENT, :ACCOUNT_TYPE_ID, :FILING_STATUS, SYSDATE, SYSDATE, 'N', :ACCOUNT_DETAIL, :INTERNAL_MANAGED_YN, 1, :ACCOUNT_ID, :ACCOUNT_NUMBER)";
        public const string CREATE_ACCOUNT_INVESTOR = @"INSERT INTO WEB_INVESTOR_X_ACCOUNT ( INVESTOR_ID, ACCOUNT_ID, OWNERSHIP_TYPE_ID, DATASOURCE_ID) VALUES (:INVESTOR_ID, :ACCOUNT_ID, :OWNERSHIP_TYPE, :DATASOURCE_ID)";
        public const string CREATE_ACCOUNT_HOUSEHOLD = @"INSERT INTO WEB_HOUSEHOLD_X_ACCOUNT( HOUSEHOLD_ID, ACCOUNT_ID, DATASOURCE_ID) VALUES (:HOUSEHOLD_ID, :ACCOUNT_ID, :DATASOURCE_ID)";
        public const string DELETE_ACCOUNT = "PKG_PG_TRANS.Delete_Accounts";

        public const string update_Account_CashBalance = @"UPDATE WEB_ACCOUNT WA SET LAST_MODIFIED_DATE = SYSDATE, CASH_BALANCE = :CASH_COMPONENT WHERE WA.ACCOUNT_ID = :ACCOUNT_ID";
        public const string deleteAllTransactions = @"DELETE WEB_TRANSACTION WHERE POSITION_ID  IN (SELECT POSITION_ID FROM WEB_POSITION  WHERE ACCOUNT_ID = :ACCOUNT_ID)";
        public const string deleteAllPositions = @"DELETE WEB_POSITION  WHERE ACCOUNT_ID = :ACCOUNT_ID";
        public const string getNewPositionID = @"SELECT WEB_POSITION_SEQ_POSITION_ID.NEXTVAL FROM DUAL";
        public const string createNewAccountPosition = @"INSERT INTO WEB_POSITION ( POSITION_ID, ACCOUNT_ID, POSITION_TYPE_ID, QUANTITY, PURCHASE_PRICE, PURCHASE_DATE, MARKET_VALUE_ENTRY_MODE_ID, SEC_ID ) 
VALUES ( :POSITION_ID,  :ACCOUNT_ID, :POSITION_TERM, :QUANTITY, :PURCHASE_PRICE, TO_DATE(:PURCHASE_DATE,'MM/DD/YYYY'), :MKT_VAL_ENTRY_MODE, :SEC_ID)";
        public const string getNewTransactionID = @"SELECT WEB_TRANSACTION_SEQ_TRANSACTID.NEXTVAL FROM DUAL";
        public const string createNewAccountTransaction = @"INSERT INTO WEB_TRANSACTION ( POSITION_ID, TRANSACTION_ID, TRANSACTION_TYPE_ID, TRANSACTION_DATE, PRICE, QUANTITY, COMMISSION) 
VALUES ( :POSITION_ID,  :TRANSACTION_ID, :TRANSACTION_TYPE_ID,   TO_DATE(:PURCHASE_DATE,'MM/DD/YYYY'),  :PURCHASE_PRICE,  :QUANTITY,  :COMMISSION)";

        public const string UPDATE_ACCOUNT_INFO = @"UPDATE WEB_ACCOUNT WA SET NAME = :ACCOUNT_NAME, TAX_STATUS_ID = :TAX_TREATMENT, ACCOUNT_TYPE_ID = :ACCOUNT_TYPE_ID, 
                                                FILING_STATUS_ID = :OWNERSHIP_TYPE, LAST_MODIFIED_DATE = SYSDATE,
                                                ACCOUNT_NUM = :ACCOUNT_NUMBER WHERE WA.ACCOUNT_ID = :ACCOUNT_ID";
        public const string DELETE_INVESTOR = @"DELETE FROM WEB_INVESTOR_X_ACCOUNT WHERE ACCOUNT_ID = :ACCOUNT_ID";





    }
}
