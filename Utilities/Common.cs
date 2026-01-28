using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using MediatR;
using SHLAPI.Models.LogFile;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using SHLAPI.Features;
using SHLAPI.Models;
using System.Globalization;

namespace SHLAPI
{

    public enum AttachementType
    {
        Request = 1,
        Project,
        Story,
        Task,
        Product
    }

    public enum NavigationTypes
    {
        First = 1,
        Last = 2,
        Next = 3,
        Prev = 4,
        GetIt = 5
    }

    public enum OperationTypes
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        Query = 4
    }

    public enum Pages
    {
        Product = 1,
        ProductQuery = 2,
        User = 3,
        UsersQuery = 4,
        PermissionsCategory = 5,
        PermissionsCategoriesQuery = 6,
        Permission = 7,
        PermissionsQuery = 8,

        RolesPermissionsQuery = 9,
        UserRolesPermissionsQuery = 10,
        PermissionsReport = 11,
        Project = 12,
        ProjectQuery = 13,
        Story = 14,
        StoriesQuery = 15,
        Task = 16,
        Attachment = 17,
        Lookup = 18,
        LookupsQuery = 19,
        TaskActivity = 20,
        CodeReviewCondition = 21,
        TaskCodeReviewResult = 22,
        Sprint = 23,
        Team = 24,
        TestcaseTemplate = 25,
        Priority = 26,
        RequestChange = 27,
        DailyTaskReport = 29,
        Version = 53,
    }

    public enum StoryType
    {
        Story = 1,
        ProductionChange = 2
    }

    public enum TaskOperation
    {
        //analysis
        analysis_create = 1,
        analysis_start_analysis,
        analysis_finish_analysis,
        analysis_start_approving,
        analysis_finish_approving_approved,
        analysis_finish_approving_rejected,
        //draft
        draft_create,
        draft_recreate,
        draft_start_drafting,
        draft_finish_drafting_approved,
        draft_finish_drafting_rejected,
        //qastory
        qastory_create,
        qastory_recreate,
        qastory_finish_merge_testing,
        qastory_create_testcase,
        qastory_start_breaking_down,
        qastory_finish_breaking_down,
        qastory_start_bugfix_pool_meeting,
        qastory_finish_bugfix_pool_meeting,
        qastory_finish_the_story,
        //development_breaking_down
        development_breaking_down_create,
        development_breaking_down_recreate,
        development_breaking_down_start,
        development_breaking_down_finish,
        //development
        development_create,
        development_start_reading,
        development_finish_reading_approved,
        development_finish_reading_rejected,
        development_start_review_meeting,
        development_finish_review_meeting_approved,
        development_finish_review_meeting_rejected,
        development_add_in_sprint,
        development_start_development,
        development_finish_development,
        development_start_code_review,
        development_finish_code_review_approved,
        development_finish_code_review_rejected,
        //merging_testing
        merging_testing_create,
        merging_testing_recreate,
        merging_testing_start_merge_testing,
        merging_testing_finish_merge_testing,
        merging_testing_start_code_review_merge_testing,
        merging_testing_finish_code_review_merge_testing_approved,
        merging_testing_finish_code_review_merge_testing_rejected,
        //qa
        qa_create,
        qa_start_reading,
        qa_finish_reading_approved,
        qa_finish_reading_rejected,
        qa_start_review_meeting,
        qa_finish_review_meeting_approved,
        qa_finish_review_meeting_rejected,
        qa_add_in_sprint,
        qa_start,
        qa_finish,
        //bugfix
        bugfix_create,
        bugfix_start_team_meeting,
        bugfix_finish_bugfix_team_meeting,
        bugfix_add_in_sprint,
        bugfix_start_development,
        bugfix_finish_development,
        bugfix_start_code_review,
        bugfix_finish_code_review_approved,
        bugfix_finish_code_review_rejected,
        //ProductionChange
        production_change_create,//not used
        production_change_recreate,//not used
        production_change_start_initial_approval,//not used
        production_change_finish_initial_approval_approved,//not used
        production_change_finish_initial_approval_rejected,//not used
        production_change_add_in_sprint_development,
        production_change_start_development,
        production_change_finish_development,
        production_change_start_code_review,
        production_change_finish_code_review_approved,
        production_change_finish_code_review_rejected,
        production_change_add_in_sprint_qa,
        production_change_start_qa,
        production_change_finish_qa_approved,
        production_change_finish_qa_rejected,
        change_owner,
        non_story_start,
        non_story_finish,
        non_story_close,
        non_story_create,
        non_story_reopen,
        update_task,
        change_estimated_time,
        development_remove_from_sprint,
        bugfix_remove_from_sprint,
        qa_remove_from_sprint,
        production_change_remove_from_sprint_qa,
        production_change_remove_from_sprint_development,
        activate_deactivate_task,
        development_finish_reassign_developer,
        qastory_reopen_for_breaking_down,//update status to qastory_inprogress_breaking_down
        development_breaking_down_reopen,//update status to developmentbreakingdown_inprogress
        qa_reopen_for_create_bugfix,//update status to developmentbreakingdown_inprogress
        qa_finish_reassign_qa,
        analysis_reopen,
        close_bugfix_by_teamleader,
        qa_reject_bugfix
    }

    public enum TestcaseProgressivityStatus
    {
        NotExecuted = 1,
        Passed,
        Failed,
        Suspended
    }

    public enum TaskStatus
    {
        //analysis
        analysis_waiting = 1,
        analysis_inprogress,
        analysis_finished,
        analysis_waiting_approving,
        analysis_inprogress_approving,
        //draft
        draft_waiting,//6
        draft_inprogress,
        draft_wainting_after_reject_anaylsis,
        draft_finish,
        //qastory
        qastory_waiting_finish_developement,//10
        qastory_waiting_breaking_down,
        qastory_inprogress_breaking_down,//assign testcases to qa
        qastory_waiting_bugfix_pool,
        qastory_inprogress_bugfix_pool_meeting,
        qastory_waiting_after_bugfix_pool_meeting,
        qastory_finished_qastory,
        //development_breaking_down
        developmentbreakingdown_waiting,//17
        developmentbreakingdown_inprogress,
        developmentbreakingdown_finished,
        //development
        development_waiting_reading,//20
        development_inprogess_reading,
        development_wainting_team_leader_meeting,
        development_inprogress_review_meeting,
        development_waiting_decision,
        development_waiting_sprint_meeting,
        development_waiting,
        development_inprogress,
        development_waiting_code_review,
        development_inprogress_code_review,
        development_finished,
        //merging_testing
        mergetesting_waiting,//31
        mergetesting_inprogress,
        mergetesting_waiting_code_review,

        mergetesting_inprogress_code_review,
        mergetesting_finished,
        //qa
        qa_waiting_reading,//36
        qa_inprogress_reading,
        qa_waiting_tl_meeting,
        qa_inprogress_review_meeting,
        qa_waiting_decision,
        qa_waiting_sprint_meeting,
        qa_waiting,
        qa_inprogress,
        qa_waiting_development,
        qa_finished,
        //bugfix
        bugfix_waiting_bugfix_in_the_pool,//46
        ____bugfix_waiting_bugfix_fetl_Bbetl_qatl_meeting,//not used
        ____bugfix_inprogress_bugfix_fetl_Bbetl_qatl_meeting,//not used
        bugfix_waiting_bugfix_sprint_meeting,
        bugfix_waiting_bugfix_development,
        bugfix_inprogress_bugfix_development,
        bugfix_waiting_code_review_bugfix_development,
        bugfix_inprogress_code_review_bugfix_development,
        bugfix_waiting_in_the_solved_bugfix_pool,
        //ProductionChange
        ____production_change_waiting_initial_approval,//55//not used
        ____production_change_inprogress_initial_approval,//not used
        ____production_change_finished_refused,//not used
        ____production_change_waiting_team_meeting,//not used
        ____production_change_inprogress_team_meeting,//not used
        ____production_change_waiting_sprint_meeting_development,//not used
        ____production_change_waiting_development,//not used
        ____production_change_inprogress_development,//not used
        ____production_change_waiting_code_review_development,//not used
        ____production_change_inprogress_code_review_development,//not used
        ____production_change_waiting_sprint_meeting_qa,//not used
        ____production_change_waiting_qa,//not used
        ____production_change_inprogress_qa,//not used
        ____production_change_finished,//not used
        //Non story
        non_story_waiting,//69
        non_story_inprogress,
        non_story_finished,
        non_story_closed,
        non_story_waiting_sprint_meeting,
        analysis_waiting_sprint_meeting,
        bugfix_aborted_by_qa,
        bugfix_postponed,
        bugfix_waiting_postpone_decision,
        deleted
        // qa_story_finished_with_postponed_bugfixes,
    }

    public enum RequestApprovingByPMStatus
    {
        Waiting = 1,
        Approved,
        Rejected
    }

    public enum ProjectStatus
    {
        Waiting = 1,
        Done,
        Freezed
    }

    public enum VersionStatus
    {
        Waiting = 1,
        Finished,
        Released
    }

    public enum VersionTarget
    {
        Release = 1,
        Major,
        Minor,
        Fix
    }

    public enum TaskType
    {
        non_story = 1,
        analysis,
        draft,
        qa_story,
        development_breaking_down,
        development,
        merging_testing,
        qa,
        bugfix,
        production_change
    }

    public enum TestcaseTypes
    {
        Functional = 1,
        NonFunctional=2
    }

    public enum Role
    {
        analyst = 1,
        business_owner,
        qa_teamleader,
        qa,
        fe_development_teamleader,
        be_development_teamleader,
        fe_developer,
        be_developer,
        project_manager,
        tech_support_qa
    }

    public enum BugfixQAStatus
    {
        notexecuted = 1,
        passed,
        failed,
        suspended
    }

    public enum PullRequestStatus
    {
        waiting = 1,
        deleted = 2,
        finished_rejected = 3,
        finished_approved = 4
    }

    public enum ProjectDevelopmentType
    {
        FullStack = 1,
        FrontEndBackEnd
    }

    public enum CodeReviewConditionType
    {
        Both = 1,
        FrontEnd,
        BackEnd
    }

    public enum ApproveStatus
    {
        Undefine = 0,
        Approve = 1,
        rejected = 2
    }

    public class LookupTables
    {
        public static string tasks_operations_tbl = "tasks_operations_tbl";
    }
    public enum ErrorReason
    {
        other,
        pwdError,
        ApproveAllTaskCodeReviewResult,
        SprintIsNotActive,
        InvalidTaskStatus,
        MustFillEstimatedTime,
        UnAutherizedOperation,
        SprintHasTasks,
        PleaseFillTeamLeaderId,
        BreakingDownMustBeFinished,
        QAStoryTaskStatusNotValidForOperation,
        PeriodsIsConflict,
        TaskIsNotInASprint,
        SomeOwnersIsMissed,
        RelatedTaskIdIsNull,
        CantCreateBugfixForSolvedTestcase,
        CantSolveOpenTestcase,
        BugfixTasksRelatedFound,
        FinishDevelopmentBreakingdownWithoutTasks,
        RequestIsLocked,
        RequestStatusIsInvalid,
        ProjectMismatchWithProduct,
        TeammembersIsRequired,
        NotAuthorized,
        CantActivateMoreThanOneSprint,
        NoSprintAvailable,
        FatalError001,
        TaskAlreadyIsInASprint,
        InvalidProjectStatus,
        ProjectHasNotFinishedStories,
        VersionHasNotFinishedProject,
        ProductHasAlreadyOpenVersion,
        InvalidProjectVersion,
        NotAllBugfixIsApprovedByQA,
        InvalidParamters,
        OnlyTheTaskTeamleaderCanCloseIt,
        UserHasNoSault,
        UserIsDisabled,
        InvalidPassword,
        TwinsTaskIsLocked,
        QAMustHasTestcases,
        InvalidTaskOperation,
        CantCloseStoryThatHasOpenedTasks,
        MissingCodeReviewConditions,
        TaskOwnersCanSaveIt,
        FailedDueMinSprintDuration,
        AnalystCantDeleteDocuments,
        StoryIsDeleted,
        StoryIsDeactivated,
        TaskHasNoSprint,
        NoActiveSprintFounded,
        DataOwnerCanUpdateIt,
        VersionNotExist,
        NoPermissionToMoveTaskToNextSprint,
        NotExecutedBugfixDetected,
        PleaseSelectDateRange,
        TaskNotFound,
        CantSaveAnalysisWithoutAttachment,
        ApproveIsForTheApprovalUser,
        CantDeleteApprovedComment,
        CommentIsAlreadyApproved,
        CommentIsDeleted,
        CantDeleteOthersComment,
        NotApprovedCommentsExist,
        AnalystIsNotFilled,
        NoTestcasesReordered,
    }
    public class Common
    {

        internal static string AutomationBugTestcaseDescription = "automation bug testcase";
        
        internal static async Task CheckPermission(IDbConnection db, IDbTransaction trans, int userId, int permissionId)
        {
           
        }

        internal static async Task<bool> HasPermission(IDbConnection db, IDbTransaction trans, int userId, int permissionId)
        {
          
            return true;
        }

        internal static void FillDefault(FeatureBase feature, IHttpContextAccessor context)
        {
            feature.user_id = GetUserId(context);
            feature.lang_id = GetLanguageId(context);
        }

        internal static int[] GetStartOperationsToNotifyPM_TL()
        {
            return new int[]{
                (int)TaskOperation.analysis_start_analysis,
                (int)TaskOperation.analysis_start_approving,
                (int)TaskOperation.draft_start_drafting,
                (int)TaskOperation.qastory_start_breaking_down,
                (int)TaskOperation.development_breaking_down_start,
                (int)TaskOperation.development_start_reading,
                (int)TaskOperation.development_start_review_meeting,
                (int)TaskOperation.development_start_code_review,
                (int)TaskOperation.merging_testing_start_merge_testing,
                (int)TaskOperation.merging_testing_start_code_review_merge_testing,
                (int)TaskOperation.qa_start_reading,
                (int)TaskOperation.qa_start_review_meeting,
                (int)TaskOperation.qa_start,
                (int)TaskOperation.bugfix_start_development,
                (int)TaskOperation.bugfix_start_code_review
            };
        }

        //get friday count in dates range
        internal static int GetFridayCount(DateTime start, DateTime end)
        {
            int count = 0;
            for (DateTime i = start; i <= end; i = i.AddDays(1))
            {
                if (i.DayOfWeek == DayOfWeek.Friday)
                {
                    count++;
                }
            }
            return count;
        }

        internal static int[] GetFinishOperationsToNotifyPM_TL()
        {
            return new int[]{
                (int)TaskOperation.analysis_finish_analysis,
                (int)TaskOperation.qastory_finish_breaking_down,
                (int)TaskOperation.development_breaking_down_finish,
                (int)TaskOperation.development_finish_reading_approved,
                (int)TaskOperation.development_finish_review_meeting_approved,
                (int)TaskOperation.development_finish_code_review_approved,
                (int)TaskOperation.qa_finish_review_meeting_approved,
                (int)TaskOperation.qa_finish,
                (int)TaskOperation.bugfix_finish_development,
                (int)TaskOperation.bugfix_finish_code_review_approved
            };
        }

        public class ModelResult
        {
            public bool result;
            public int lastId;
            public int statusCode;
            public string name;
            public string no;
            public string deletedIds;
            public string errorMsg;
            public ErrorReason errorReason;
        }
        public class AccountResult
        {
            public bool result;
            public string accountNo;
        }

        internal static int GetOperation(int id)
        {
            if (id > 0) return (int)OperationTypes.Update;
            return (int)OperationTypes.Insert;
        }

        static public async Task<int> GetLastId(IDbConnection db, IDbTransaction trans)
        {
            int logFileId = 0;
            List<int> logFileIdList = (await db.QueryAsync<int>("SELECT LAST_INSERT_ID() as mmm", null, trans)).AsList();
            if (logFileIdList != null && logFileIdList.Count > 0)
            {
                logFileId = logFileIdList[0];
            }
            return logFileId;
        }

        public class OrderResult
        {
            public int maxRes;
        }
        static public async Task<int> GetMaxOrderNo(IDbConnection db, IDbTransaction trans, string tableName, int companyId,
         string columnOrderByName = "", int columnOrderByValue = 0, bool withCompany = true)
        {
            int orderNo = 0;
            string selectMaxStat = "select max(order_no) as maxRes from " + tableName + " where ";
            if (withCompany)
                selectMaxStat += " company_id=" + companyId;
            else selectMaxStat += " 1=1 ";
            if (columnOrderByName != "")
                selectMaxStat += " and " + columnOrderByName + "=" + columnOrderByValue;

            var orderNoList = (await db.QueryAsync<OrderResult>(selectMaxStat, null, trans)).AsList();
            if (orderNoList != null && orderNoList.AsList().Count > 0)
            {
                orderNo = orderNoList.AsList()[0].maxRes;
            }
            return orderNo;
        }

        //days count in dates range
        internal static int GetDaysCount(DateTime start, DateTime end)
        {
            int count = 0;
            for (DateTime i = start; i <= end; i = i.AddDays(1))
            {
                if (i.DayOfWeek != DayOfWeek.Friday)
                    count++;
            }
            return count;
        }
        //the code abovr replaced with this code
        /*internal static int DateDiffAsDays(DateTime startDate, DateTime endDate)
        {
            DateTime tmpDt=startDate;
            var daysCount = endDate-startDate;
            int daysCountWithoutFriday=0;
            for(int days=0;days<(int)daysCount.TotalDays;days++)
            {
                if(tmpDt.DayOfWeek!=DayOfWeek.Friday)
                    daysCountWithoutFriday++;
                tmpDt=tmpDt.AddDays(1);                
            }
            return daysCountWithoutFriday;
        }*/

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public static async Task<bool> SaveTrialLog(IDbConnection db, IDbTransaction trans, int operationId,
                                                    object obj, int pageId, int userId, int recordId, string note)
        {
            if (note != null)
            {
                note = note.Substring(0, note.Length > 199 ? 199 : note.Length);
            }

            AuditTrailM auditTrialObj = new AuditTrailM();
            auditTrialObj.date_ = DateTime.Now;
            auditTrialObj.note = note;
            auditTrialObj.object_ = Common.Zip(JsonConvert.SerializeObject(obj));
            auditTrialObj.operation_id = operationId;
            auditTrialObj.page_id = pageId;
            auditTrialObj.record_id = recordId;
            auditTrialObj.time_ = DateTime.Now;
            auditTrialObj.user_id = userId;
            await AuditTrailM.Save(db, trans, auditTrialObj);
            return true;
        }
        public static async Task<bool> SaveLog(IDbConnection db, IDbTransaction trans, int operationId,
                                   int companyId, object obj, int pageId, int userId, int recordId, string note)
        {
            LogTrialM auditTrialObj = new LogTrialM();
            auditTrialObj.date_ = DateTime.Now;
            auditTrialObj.company_id = companyId;
            auditTrialObj.note = note;
            auditTrialObj.object_ = null; //Common.Zip(JsonConvert.SerializeObject(obj));
            auditTrialObj.operation_id = operationId;
            auditTrialObj.page_id = pageId;
            auditTrialObj.record_id = recordId;
            auditTrialObj.time_ = DateTime.Now;
            auditTrialObj.user_id = userId;
            await LogTrialM.Save(db, trans, auditTrialObj);
            return true;
        }


        public static string Search_Specific(string str, string fieldName)
        {
            if (str == null)
            {
                str = " 1=2 ";
                return str;
            }
            str = str.Trim();
            string temp = "";
            List<string> Strings = SetArabicVariants(str);

            if (Strings.Count != 1)
            {
                for (int i = 0; i < Strings.Count; i++)
                {
                    if (Strings[i].Trim() != "")
                    {
                        if (i == 0)
                            temp += " ( " + fieldName + " LIKE N'%" + Strings[i] + "%'"; // //MLHIDE
                        else if (i == Strings.Count - 1)
                            temp += " OR " + fieldName + " LIKE N'%" + Strings[i] + "%')"; // //MLHIDE
                        else
                            temp += " OR " + fieldName + " LIKE N'%" + Strings[i] + "%'"; // //MLHIDE
                    }
                    else

                        temp += ") AND( 1=2 ";                    // //MLHIDE
                }
            }
            else
            {
                if (Strings[0].Trim() != "")
                    temp += fieldName + " LIKE N'%" + Strings[0] + "%'"; // //MLHIDE
                else
                    temp = " 1=1 ";                               // //MLHIDE
            }
            return temp;
        }
        public static bool SaveAttachmentsToFolder(string fileName, string innerPath, string fileId, byte[] attachment)
        {
            try
            {
                string extentions = "";
                if (fileName.Trim().Contains("."))
                {
                    extentions = fileName.Split(".")[1];
                }
                string attachPath = SHLAPI.Utilities.StringUtil.SystemAttachmentsFolderPath + "\\" + innerPath + "\\" + fileId + "." + extentions;
                File.WriteAllBytes(attachPath, attachment);
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public static bool DeleteAttachmentsToFolder(string fileName, string innerPath, string fileId)
        {
            try
            {
                string extentions = "";
                if (fileName.Trim().Contains("."))
                {
                    extentions = fileName.Split(".")[1];
                }
                string attachPath = SHLAPI.Utilities.StringUtil.SystemAttachmentsFolderPath + "\\" + innerPath + "\\" + fileId + "." + extentions;
                File.Delete(attachPath);
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public static string GetStringArabicVariants(string str, string fieldName)
        {
            if (str == null)
            {
                str = " 1=2 ";
                return str;
            }
            str = str.Trim();
            string temp = "";
            List<string> Strings = SetArabicVariantsWithOutSpace(str);
            if (Strings.Count != 1)
            {
                for (int i = 0; i < Strings.Count; i++)
                {
                    if (Strings[i].Trim() != "")
                    {
                        if (i == 0)
                            temp += " ( " + fieldName + " = N'" + Strings[i] + "'"; // //MLHIDE
                        else if (i == Strings.Count - 1)
                            temp += " OR " + fieldName + " = N'" + Strings[i] + "')"; // //MLHIDE
                        else
                            temp += " OR " + fieldName + " = N'" + Strings[i] + "'"; // //MLHIDE
                    }
                    else
                        temp += ") AND( 1=2 ";                    // //MLHIDE
                }
            }
            else
            {
                if (Strings[0].Trim() != "")
                    temp += fieldName + " = N'" + Strings[0] + "'"; // //MLHIDE
                else
                    temp = " 1=1 ";                               // //MLHIDE
            }
            return temp;
        }
        public static List<string> SetArabicVariants(string str)
        {
            string[] ArrOfAccount_Name = str.Trim().Split(' ');
            List<string> Strings = new List<string>();
            for (int i = 0; i < ArrOfAccount_Name.Length; i++)
            {
                Strings.Add(ArrOfAccount_Name[i]);
                if (ArrOfAccount_Name[i].Contains("ا"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('ا', 'أ'));
                }
                if (ArrOfAccount_Name[i].Contains("أ"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('أ', 'ا'));
                }
                if (ArrOfAccount_Name[i].Contains("ة") ||
                 ArrOfAccount_Name[i].Contains("ه"))           // //MLHIDE
                {
                    Strings.Add(ArrOfAccount_Name[i].Replace('ة', 'ه'));
                    Strings.Add(ArrOfAccount_Name[i].Replace('ه', 'ة'));
                }
                if (i + 1 != ArrOfAccount_Name.Length)
                    Strings.Add("");
            }
            return Strings;
        }
        public static List<string> SetArabicVariantsWithOutSpace(string str)
        {
            List<string> Strings = new List<string>();
            Strings.Add(str);
            if (str.Contains("ا"))           // //MLHIDE
            {
                Strings.Add(str.Replace('ا', 'أ'));
                Strings.Add(str.Replace('ا', 'إ'));
                Strings.Add(str.Replace('ا', 'ى'));
            }
            if (str.Contains("ة"))           // //MLHIDE
            {
                Strings.Add(str.Replace('ة', 'ه'));
            }
            return Strings;
        }
        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        static public int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;
            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;
            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;
            if (targetWordCount == 0)
                return sourceWordCount;
            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];
            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;
            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }
        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        static public double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;
            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        public static int GetLanguageId(IHttpContextAccessor _context)
        {
            int langId = 0;
            string ci = _context.HttpContext.Request.Headers["lang_id"];
            int.TryParse(ci, out langId);
            return langId;
        }
        public static int GetPageId(IHttpContextAccessor _context)
        {
            int pageId = 0;
            string ci = _context.HttpContext.Request.Headers["page_id"];
            int.TryParse(ci, out pageId);
            return pageId;
        }
        public static int GetUserId(IHttpContextAccessor _context)
        {
            int userId = 0;
            string ci = _context.HttpContext.Request.Headers["user_id"];
            int.TryParse(ci, out userId);
            return userId;
        }

        internal static DateTime GetMaxDate()
        {
            return new DateTime(2100, 12, 31);
        }

        internal static string GetVersionTypeName(int target)
        {
            string[] names = { "", "Release", "Major", "Minor", "Fix" };
            return names[target];
        }

        internal static string GetVersionStatusName(int target)
        {
            string[] names = { "", "Waiting", "Finished", "Released" };
            return names[target];
        }

        internal static string GetProjectProgressStatusName(int target)
        {
            string[] names = { "", "Waiting", "Finished", "Freezed" };
            return names[target];
        }

        internal static int GetRandom(int v1, int v2)
        {
            Random rand = new Random();
            return rand.Next(v1, v2);
        }

        internal static DateTime ParseDateTime(string date_str)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(date_str, "yyyy-MM-dd HH:mm", null);
        }

        internal static bool IsSameDate(DateTime date1, DateTime date2)
        {
            if (date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day)
                return true;
            return false;
        }

        internal static string GetTimeAsString(int final_estimated_time)
        {
            int days = final_estimated_time / (60 * 24);
            int hours = (final_estimated_time - (days * 60 * 24)) / 60;
            int mintes = (final_estimated_time - (days * 60 * 24) - (hours * 60));
            return string.Format("{0}:{1}:{2}", days, hours, mintes);
        }
    }


}