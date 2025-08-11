using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddInterestAccrualFunction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION accrue_interest(account_id UUID)
            RETURNS VOID AS
            $$
            DECLARE
                current_balance DECIMAL;
                interest_rate DECIMAL;
                interest_amount DECIMAL;
                account_currency TEXT;
            BEGIN
                SELECT ""Balance"", ""InterestRate"", ""Currency"" 
                INTO current_balance, interest_rate, account_currency
                FROM ""Accounts""
                WHERE ""Id"" = account_id
                FOR UPDATE;
                
                IF interest_rate > 0 AND current_balance > 0 THEN
                    interest_amount := current_balance * interest_rate;
                    
                    UPDATE ""Accounts""
                    SET ""Balance"" = ""Balance"" + interest_amount
                    WHERE ""Id"" = account_id;
                    
                    INSERT INTO ""Transactions"" 
                        (""Id"", ""AccountId"", ""Sum"", ""Currency"", ""Type"", ""Description"", ""DateTime"")
                    VALUES
                        (gen_random_uuid(), 
                         account_id, 
                         interest_amount, 
                         account_currency, 
                         'Interest'::transactiontype, 
                         'Interest Accrual', 
                         NOW());
                END IF;
            END;
            $$ LANGUAGE plpgsql;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS accrue_interest(account_id UUID)");
    }
}