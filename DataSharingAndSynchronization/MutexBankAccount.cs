namespace DataSharingAndSynchronization
{
    public class MutexBankAccount
    {
        private int _balance;

        public int Balance { get => _balance; set => _balance = value; }

        public void Deposit(int amount)
        {
            // += 
            //op1: temp <- ballance + amount
            //op2: ballance <- temp
            _balance += amount;
        }

        public void WithDraw(int amount)
        {
            // -= 
            //op1: temp <- ballance - amount
            //op2: ballance <- temp
            _balance -= amount;
        }

        public void Transfer(MutexBankAccount where, int amount)
        {
            _balance -= amount;
            where.Balance += amount;
        }
    }
}
