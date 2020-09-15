using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ThreadingExample
{
    public partial class Form1 : Form
    {
        long i;
        Random r = new Random();
        ManualResetEvent e = new ManualResetEvent(false);

        long threads = 10;
        long done = 0;

        public Form1()
        {
            InitializeComponent();

            // This forces the GUI to refresh if you want a ghetto way to multithread (using while loop and just refresh each loop)
            Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs ea)
        {
            button1.Enabled = false;
            done = 0;

            // Method 1: Threadpool
            // Just runs everything you put in the threadpool as a separate thread
            // Make sure never to let multiple threads write the same variable at once!
            // Use  new object[] { arg1, arg2... } in second parameter of QueueUserWorkItem to pass arguments to the function

            for (int i = 0; i < threads; i++)
                ThreadPool.QueueUserWorkItem(new WaitCallback(hi));

            // Wait for all threads to finish in another thread so the GUI doesn't freeze
            ThreadPool.QueueUserWorkItem(new WaitCallback(end));
        }

        private void hi(object o = null)
        {
            //Interlocked class takes care of locking the variable before editing and reading
            long j = Interlocked.Increment(ref i); // Also returns its value so you don't have to worry about others writing to it

            // Pause randomly to make the threads out of order
            int rand = r.Next(1, 2000);
            Thread.Sleep(rand); // Normally freezes whole program if only one thread

            sharedFunction("Hi! Thread " + j);

            if (Interlocked.Increment(ref done) == threads)
                e.Set();
        }

        private void end(object o = null)
        {
            // Use a lock/event object to tell when all threads finished
            // Basically halts this main thread until e is true

            e.WaitOne(10000); // or until 10000 ms have passed
            // also returns true if timed out or not

            sharedFunction("Done!");
        }

        // If you need to have multiple threads access something shared (GUI, main class functions...)
        private delegate void DelegateThis(string arg1);
        private void sharedFunction(string arg1)
        {
            if (InvokeRequired) // Automatically checks if this function is being used by another thread
                Invoke(new DelegateThis(sharedFunction), new object[] { arg1 });
            else
            {
                richTextBox1.Text += arg1 + "\n";
            }
        }

    }
}
