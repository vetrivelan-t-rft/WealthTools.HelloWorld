using System;
using System.Collections.Generic;
using System.Text;

namespace WealthTools.Library.Proposals
{
    class SqlConstants
    {
        public const string GET_PROPOSALS_BY_HH = @"SELECT wip.plan_id,wip.name, wip.plan_type_id,wwp.name as program_name, wip.HOLDINGLEVEL, wip.model_minor_version,trunc(wip.last_modified_date) as last_modified_date
        FROM web_investment_plan wip,web_wm_solution wws,web_solution_x_party wsxp, web_process wp, web_wm_program wwp
        WHERE wip.solution_id = wws.solution_id and wws.solution_id = wsxp.solution_id
        and   wip.program_id = wwp.program_id (+) 
        and   wip.plan_type_id in (:PLAN_TYPE_IDS)
        and   wsxp.party_id = :HOUSEHOLD_ID 
        and   wsxp.party_type_id = :PARTY_TYPE_ID				
        and   (wip.mark_deleted_yn IS NULL OR wip.mark_deleted_yn = 'N') 
        and wip.plan_process_id = wp.process_id and wp.status <>'-1'
        ORDER BY wip.last_modified_date DESC";

        public const string GET_INV_REPORTS_BY_HH = @"select archived_report_id,
        case when
          (sum(entitled_account_count) > 0 and sum(entitled_account_count) = count(*)) or (sum(entitled_account_count) > 0 and sum(entitled_account_count) = sum(internal_account_count))  then 'Y' 
          when sum(internal_account_count) = 0 then 'Y'
          else 'N'
        end ENTITLED_YN,create_date, plan_id, name, filename, location,streetscapeyn
        from            
        (SELECT war.archived_report_id, wara.account_id,
        (NVL((SELECT   distinct 1
        FROM   WEB_BROKER_X_TEAM X, WEB_TEAM_X_ACCOUNT Y
        WHERE       X.TEAM_ID = Y.TEAM_ID
        AND BROKER_ID = :BROKER_ID
        AND account_id IN (wara.account_id)),0))
        entitled_account_count, 
        (select count(*) from bo_account where account_id in (wara.account_id)) internal_account_count,
        last_modified_date as create_date, plan_id, name, filename, location, streetscapeyn 
        FROM web_archived_report war, WEB_ARCHIVED_REPORT_ACCOUNT wara
        WHERE war.plan_id in (:PLAN_IDS) AND war.report_type_id in (:REPORT_TYPE_IDS) AND war.usage_code = 'P'
        AND war.plan_type_id in (:PLAN_TYPE_IDS) AND war.mark_deleted_yn = 'N'
        AND wara.archived_report_id (+)= war.archived_report_id )
        group by archived_report_id,create_date, plan_id, name, filename, location, streetscapeyn
        order by create_date";


        public const string DELETE_WEB_INVESTMENT_PLAN = @"UPDATE WEB_INVESTMENT_PLAN IP SET IP.MARK_DELETED_YN = 'Y',
        IP.LAST_MODIFIED_DATE = SYSDATE
        WHERE IP.PLAN_ID = :PLAN_ID";


        //Create Proposal
        public const string GET_DEFAULT_ASSETCLASS_VIEW_ID= @"select min(v.ac_view_id) from web_ac_view v, web_inv_profile_config i 
            where i.AC_VIEW_SET_ID = v.AC_VIEW_SET_ID and i.INV_PROFILE_CONFIG_ID= :GROUP_ID";
        public const string GET_SEQUENCE_SOLUTION = @"SELECT WEB_WM_SOLUTION_SEQ_SOL_ID.NEXTVAL FROM DUAL";
        public const string CREATE_SOLUTION = @"INSERT INTO WEB_WM_SOLUTION(SOLUTION_ID,NAME,DESCR, CREATE_DATE, CREATED_BY) values (:SOLUTION_ID,:NAME,'',SYSDATE,:CREATED_BY)";
        public const string CREATE_SOLUTION_PARTY = @"INSERT INTO WEB_SOLUTION_X_PARTY(SOLUTION_ID,PARTY_ID,PARTY_TYPE_ID) VALUES (:SOLUTION_ID,:PARTY_ID,:PARTY_TYPE_ID)";
        public const string GET_SEQUENCE_INVESTMENT_PLAN= @"SELECT WEB_INV_PLAN_SEQ_PLAN_ID.NEXTVAL FROM DUAL";
        public const string GET_SEQUENCE_PORTFOLIO= @"SELECT Get_Seq_Pattern( :Pi_Inst_ID, :Pi_Table_Name, :Pi_Record_Type) as SeqValue  FROM dual";
        public const string GET_SEQUENCE_PROCESS = @"SELECT WEB_PROCESS_SEQ_PROCESS_ID.NEXTVAL FROM DUAL";
        public const string GET_SEQUENCE_PROCESS_LOG = @"SELECT PROC_ACTY_LOG_SEQ_LOG_ENTRY_ID.NEXTVAL FROM DUAL";
        public const string GET_DEFAULT_OPTIONS_IDS = @"Select distinct wipfo.inv_profile_factor_option_id
            from web_group wg inner join web_group_inv_profile a on wg.group_id = a.group_id 
            inner join web_inv_profile_conf_x_factor b on a.inv_profile_config_id= b.inv_profile_config_id
            inner join web_inv_profile_factor wipf  on wipf.inv_profile_factor_id = b.inv_profile_factor_id
            inner join web_inv_profile_factor_option wipfo on wipf.inv_profile_factor_id = wipfo.inv_profile_factor_id
            where wipfo.default_YN = 'Y' and wg.institution_id = :a_nGroupId and a.default_YN = 'Y'";
        public const string GET_DEFAULT_PROFILE_MODEL_ID= @"Select DISTINCT a.profile_dtl_id, c.model_id from ( 
            select profile.profile_dtl_id from
            (Select profile_dtl_id ,count(profile_dtl_id) as count1 from web_investor_profile_detail where inv_profile_factor_option_id 
            IN (:OPTION_IDS) group by profile_dtl_id ) profile
            where profile.count1 = :OPTION_COUNT ) a
            INNER JOIN web_investor_profile_detail b ON b.profile_dtl_id = a.profile_dtl_id
            LEFT OUTER JOIN ( select * from web_profile_x_model WHERE default_MODEL_YN  = 'Y' AND institution_id = :GROUP_ID ) c
            ON b.profile_id = c.profile_id LEFT OUTER JOIN web_portfolio_product wpp ON c.model_id = wpp.portfolio_id AND UPPER (wpp.status) = 'ACTIVE'";
        public const string CREATE_INVESTMENT_PORTFOLIO= @"INSERT INTO WEB_INVESTMENT_PORTFOLIO 
               (PORTFOLIO_ID, PORTFOLIO_TYPE_ID, CREATE_DATE, CREATE_BY_USER_ID) 
               VALUES (:PORTFOLIO_ID, :PORTFOLIO_TYPE_ID, TRUNC(SYSDATE), :CREATE_BY_USER_ID) ";
        public const string CREATE_HOLDING_PORTFOLIO= @"INSERT INTO WEB_HOLDING_PORTFOLIO 
               (PORTFOLIO_ID, PORTFOLIO_TYPE_ID, EXTERNAL_ID, DATASOURCE_ID, CREATE_DATE, 
                CREATE_BY_USER_ID) 
               VALUES 
               (:PORTFOLIO_ID,:PORTFOLIO_TYPE_ID, :EXTERNAL_ID, :DATASOURCE_ID,TRUNC(SYSDATE),  
                :CREATE_BY_USER_ID) ";
        public const string CREATE_WEB_PROCESS= @"INSERT INTO web_process(process_id, process_definition_id, start_date_time, complete_date_time, status, create_by_user_id,
create_date, update_by_user_id, update_date, parent_process_id, process_type_id)
VALUES (:PROCESS_ID, :PROCESS_DEFINITION_ID, sysdate, sysdate, '1', :BROKER_ID, sysdate, :BROKER_ID,
sysdate, null, null)";
        public const string CREATE_WEB_PROCESS_PARAM = @"INSERT INTO web_process_param(process_id, SEQ_NUM, ENTITY_TYPE_ID, ENTITY_ID)
       VALUES (:PROCESS_ID, :SEQ_NUM, :ENTITY_TYPE_ID, :ENTITY_ID)";
        public const string CREATE_WEB_PROCESS_ACTIVITY = @"INSERT INTO web_process_activity(process_id, activity_id, name, start_date_time, complete_date_time, status, 
create_by_user_id, create_date, update_by_user_id, update_date, process_seq_num)
VALUES (:PROCESS_ID, :ACTIVITY_ID, '', sysdate, sysdate, '-1', :BROKER_ID, sysdate, :BROKER_ID, sysdate, 0)";
        public const string CREATE_WEB_PROCESS_ACTIVITY_LOG = @"INSERT INTO web_process_activity_log (process_id, activity_id, log_entry_id, entity_type_id, entity_id, message,
create_by_user_id, create_date, update_by_user_id, update_date, process_seq_num)
VALUES (:PROCESS_ID, :ACTIVITY_ID, :LOG_ENTRY_ID, :ENTITY_TYPE_ID, ':BROKER_ID', '', :BROKER_ID, sysdate, :BROKER_ID,
sysdate, 0)";
        public const string CREATE_NEW_PROPOSAL= @"INSERT INTO WEB_INVESTMENT_PLAN(PLAN_ID,SOLUTION_ID,NAME,DESCR,PROFILE_ID,MODEL_ID,HOLDING_PORTFOLIO_ID,PLAN_PROCESS_ID,MARK_DELETED_YN,
                CREATE_DATE,CREATED_BY,LAST_MODIFIED_DATE, HOLDINGLEVEL, MODELBASED_YN, AC_VIEW_ID, PLAN_TYPE_ID,CASH_AMOUNT,PROGRAM_ID,QUESTION_PROFILE_ID) 
                VALUES( :PLAN_ID, :SOLUTION_ID,:NAME,:DESCR,:PROFILE_ID,:MODEL_ID,
                :HOLDING_PORTFOLIO_ID, :PLAN_PROCESS_ID,:MARK_DELETED_YN,SYSDATE,:CREATED_BY,
                SYSDATE, :HOLDINGLEVEL, :MODELBASED_YN,:AC_VIEW_ID, :PLAN_TYPE_ID, :CASH_AMOUNT, :PROGRAM_ID,:PROFILE_ID)";

        public const string INSERT_RECENT_PROPOSAL = @"INSERT INTO WEB_BROKER_RECENT_PLANS (BROKER_ID, PLAN_ID, ACCESS_DATE, PLAN_TYPE_ID) VALUES (:BROKER_ID, :PLAN_ID, SYSDATE, :PLAN_TYPE_ID)";
        public const string GET_INVESTOR_ID = @"select * from web_investor where INVESTOR_ID in (
                                                select INVESTOR_ID from  WEB_HOUSEHOLD_MEMBER where household_id = :HOUSEHOLD_ID 
                                                AND RELATIONSHIP_TYPE_ID = 1)";
    }
}
