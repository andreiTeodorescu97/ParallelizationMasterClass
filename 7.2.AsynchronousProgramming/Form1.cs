using System.Net;

namespace _7._2.AsynchronousProgramming
{
    public partial class Form1 : Form
    {
        public async Task<int> CalculateValueAsync()
        {
            await Task.Delay(5000);    
            return 123;
        }

        public Task<int> StartCalculateValueTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                return 123;
            });
        }
        public int CalculateValue()
        {
            Thread.Sleep(5000);
            return 123;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void btnCalculate_ClickAsync(object sender, EventArgs e)
        {
            //Withouit Async
            //int n = CalculateValue();
            //lblResult.Text = n.ToString();

            //Using Tasks
            //var calculation = StartCalculateValueTask();
            //calculation.ContinueWith(t => { lblResult.Text = t.Result.ToString(); }, TaskScheduler.FromCurrentSynchronizationContext());

            //Using Async and Await
            int value = await CalculateValueAsync();
            lblResult.Text = value.ToString();

            await Task.Delay(5000);

            using (var wc = new WebClient())
            {
                string data = await wc.DownloadStringTaskAsync("https://www.google.com/robots.txt");
                lblResult.Text = data.Split('\n')[0];
            }
        }
    }
}