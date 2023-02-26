using System.Threading;

namespace DataSharingAndSynchronization
{
    public class InterlockedBankAccount
    {
        private int _balance;

        public int Balance { get => _balance; set => _balance = value; }

        public void Deposit(int amount)
        {
            // += 
            //op1: temp <- ballance + amount
            //op2: ballance <- temp
            Interlocked.Add(ref _balance, amount);
        }

        public void WithDraw(int amount)
        {
            // -= 
            //op1: temp <- ballance - amount
            //op2: ballance <- temp
            Interlocked.Add(ref _balance, -amount);
        }
    }
}
