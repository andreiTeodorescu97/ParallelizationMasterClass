using System.Threading;

namespace DataSharingAndSynchronization
{
    public class SpinLockBankAccount
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
    }
}
