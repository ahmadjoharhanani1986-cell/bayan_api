using System.Data;
using Dapper;
namespace SHLAPI.Models.PhysicalAccount
{
    public class PhysicalAccount_M
    {

        public static async Task<IEnumerable<dynamic>> GetData(IDbConnection db, IDbTransaction trans, int curr_id,int id)
        {
            try
            {
                string sql = @"
                                SELECT 
                                    bPA.id AS bankPhysicalAccountsId,
                                    bPA.no AS bankPhysicalAccountsNo,
                                    bPA.name AS bankPhysicalAccountsName,
                                    bPA.bank_branch_id AS branchId,
                                    bPA.check_pay AS checkBoxId,
                                    bPA.bank_account_no_main AS bankAccountNoMain,
                                    Bank_branches.no AS branchNo,
                                    Bank_branches.name AS branchName,
                                    Banks.no AS bankNo,
                                    Banks.name AS bankName,
                                    Banks.id AS bankId ,bPA.curr_id 
                                FROM BankPhysicalAccounts bPA
                                INNER JOIN Bank_branches ON Bank_branches.id = bPA.bank_branch_id
                                INNER JOIN Banks ON Banks.id = Bank_branches.parent_bank_id
                                WHERE  bPA.curr_id =@curr_id
                                ";

                if (id > 0)
                {
                        sql += " and bPA.id=@id";             
                }

                sql += " ORDER BY bPA.id ASC";

                var res = await db.QueryAsync<dynamic>(
                    sql,
                    new { curr_id,id },
                    transaction: trans,
                    commandType: CommandType.Text
                );

                return res;
            }
            catch (Exception EX)
            {
                throw;
            }
        }


    }
}