using System;
using System.Collections.Generic;
using System.Text;

namespace DataSharingAndSynchronization
{
    internal class CriticalSectionBankAccount
    {
        public object padLock = new object();
        public int Balance { get; set; }

        public void Deposit(int amount)
        {
            // += 
            //op1: temp <- ballance + amount
            //op2: ballance <- temp
            lock (padLock)
            {
               Balance += amount;
            }
        }

        public void WithDraw(int amount)
        {
            // -= 
            //op1: temp <- ballance - amount
            //op2: ballance <- temp
            lock (padLock)
            {
                Balance -= amount;
            }
        }
    }

}
